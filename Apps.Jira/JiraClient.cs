using Apps.Jira.Dtos;
using Apps.Jira.Extensions;
using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RestSharp;
using System.Net;

namespace Apps.Jira;

public class JiraClient : RestClient
{
    private static readonly ResiliencePipeline<RestResponse> CloudIdPipeline =
        JiraPollyPolicies.GetTooManyRequestsRetryPolicy();

    private readonly ResiliencePipeline<RestResponse> _retryPolicy;

    public JiraClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, string routeType = "api")
        : base(new RestClientOptions
        { ThrowOnAnyError = false, BaseUrl = GetUri(authenticationCredentialsProviders, routeType) })
    {
        this.AddDefaultHeader("Authorization",
            authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value);

        _retryPolicy = JiraPollyPolicies.GetTooManyRequestsRetryPolicy();
    }

    public async Task<T> ExecuteWithHandling<T>(RestRequest request)
    {
        var response = await ExecuteWithHandling(request);

        return response.Content.Deserialize<T>();
    }

    public async Task<RestResponse> ExecuteWithHandling(RestRequest request)
    {
        var response = await ExecuteSafeAsync(ct => base.ExecuteAsync(request, ct));

        if (response.IsSuccessful)
            return response;

        throw ConfigureErrorException(response);
    }

    private static Uri GetUri(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, string routeType)
    {
        var cloudId = GetJiraCloudId(authenticationCredentialsProviders);

        string basePath = routeType switch
        {
            "agile" => $"/rest/agile/1.0",
            "api" => $"/rest/api/3",
            _ => throw new ArgumentException("Invalid route type")
        };

        var uri = $"https://api.atlassian.com/ex/jira/{cloudId}{basePath}";
        return new Uri(uri);
    }

    private static string GetJiraCloudId(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        const string atlassianResourcesUrl = "https://api.atlassian.com/oauth/token/accessible-resources";
        string jiraUrl = authenticationCredentialsProviders.First(p => p.KeyName == "JiraUrl").Value;
        string authorizationHeader = authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value;
        var restClient = new RestClient(new RestClientOptions
        { ThrowOnAnyError = false, BaseUrl = new Uri(atlassianResourcesUrl) });
        var request = new RestRequest("");
        request.AddHeader("Authorization", authorizationHeader);

        var response = ExecuteSafe(CloudIdPipeline,ct => restClient.Execute(request));

        if (!response.IsSuccessful)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new PluginApplicationException("Request rate limit exceeded - Too Many Requests. Please try again later or add a retry policy in your bird.");
            }

            throw new PluginApplicationException($"Atlassian accessible-resources failed: {(int)response.StatusCode} {response.StatusDescription}. Body: {response.Content}");
        }

        var resources = response.Content.Deserialize<List<AtlassianCloudResourceDto>>();

        var cloudId = resources.FirstOrDefault(r => jiraUrl.Contains(r.Url, StringComparison.OrdinalIgnoreCase))?.Id;

        return cloudId ?? throw new PluginMisconfigurationException("The Jira URL is incorrect. No matching Atlassian Cloud resource found.");
    }

    private Exception ConfigureErrorException(RestResponse response)
    {
        if (response == null)
            return new PluginApplicationException("Jira did not return a response.");

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return new PluginApplicationException(
                "Jira is limiting requests due to high activity. Please try again later."
            );
        }

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            return new PluginApplicationException(
                $"Jira returned error {(int)response.StatusCode} {response.StatusDescription} with empty body."
            );
        }

        ErrorDto? error = null;
        try
        {
            error = response.Content.Deserialize<ErrorDto>();
        }
        catch (JsonException)
        {
            return new PluginApplicationException(
                $"Jira returned error {(int)response.StatusCode} with body: {response.Content}"
            );
        }

        if (error != null)
        {
            if (!string.IsNullOrEmpty(error.Message))
                return new PluginApplicationException(error.Message);

            if (error.ErrorMessages?.Any() == true)
            {
                var combined = string.Join(" ", error.ErrorMessages);
                return new PluginApplicationException(combined);
            }

            if (error.Errors != null)
            {
                var firstError = error.Errors
                    .Properties()
                    .Select(p => p.Value.ToString())
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(firstError))
                    return new PluginApplicationException(firstError);
            }
        }

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            return new PluginApplicationException(
                $"Jira returned Internal Server Error. Body: {response.Content}"
            );
        }

        return new PluginApplicationException("Internal system error");
    }

    public async Task<List<TItem>> Paginate<TItem>(JiraRequest originalRequest)
    {
        var allItems = new List<TItem>();
        var endpoint = originalRequest.Resource;
        var method = originalRequest.Method;
        int startAt = 0;

        bool isLast = false;

        do
        {
            var pageRequest = new JiraRequest(endpoint, method);

            foreach (var p in originalRequest.Parameters
                                         .Where(x => x.Type == ParameterType.QueryString))
            {
                pageRequest.AddQueryParameter(p.Name, p.Value?.ToString());
            }

            pageRequest.AddQueryParameter("startAt", startAt.ToString());
            pageRequest.AddQueryParameter("maxResults", "50");

            var page = await ExecuteWithHandling<PaginationResponse<TItem>>(pageRequest);

            if (page?.Values != null)
                allItems.AddRange(page.Values);

            isLast = page?.IsLast ?? true;
            startAt = (page?.StartAt ?? 0) + (page?.Values?.Count ?? 0);

        } while (!isLast);

        return allItems;
    }

    private async Task<RestResponse> ExecuteSafeAsync(Func<CancellationToken, Task<RestResponse>> action)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(ct => new ValueTask<RestResponse>(action(ct)),CancellationToken.None);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new PluginApplicationException("Request rate limit exceeded - Too Many Requests. Please try again later or add a retry policy in your bird.",ex);
        }
        catch (Exception ex) when (ex is not PluginApplicationException)
        {
            throw new PluginApplicationException($"Request failed: {ex.Message}", ex);
        }
    }

    private static RestResponse ExecuteSafe(ResiliencePipeline<RestResponse> pipeline,Func<CancellationToken, RestResponse> action)
    {
        try
        {
            return pipeline.Execute(action, CancellationToken.None);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new PluginApplicationException("Request rate limit exceeded - Too Many Requests). Please try again later or add a retry policy in your bird.",ex);
        }
        catch (Exception ex) when (ex is not PluginApplicationException)
        {
            throw new PluginApplicationException($"Request failed: {ex.Message}", ex);
        }
    }
}