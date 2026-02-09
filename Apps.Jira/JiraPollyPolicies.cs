using System.Net;
using Polly;
using Polly.Retry;
using RestSharp;

namespace Apps.Jira;

public static class JiraPollyPolicies
{
    public static ResiliencePipeline<RestResponse> GetTooManyRequestsRetryPolicy(int retryCount = 6)
    {
        const double minDelaySeconds = 5.0;
        const double maxDelaySeconds = 45.0;

        var retryOptions = new RetryStrategyOptions<RestResponse>
        {
            MaxRetryAttempts = retryCount,

            ShouldHandle = new PredicateBuilder<RestResponse>()
                .HandleResult(r =>
                    r.StatusCode == HttpStatusCode.TooManyRequests ||
                    r.StatusCode == HttpStatusCode.InternalServerError ||
                    r.StatusCode == HttpStatusCode.ServiceUnavailable)
                .Handle<HttpRequestException>(ex =>
                    ex.StatusCode == HttpStatusCode.TooManyRequests ||
                    ex.StatusCode == HttpStatusCode.InternalServerError ||
                    ex.StatusCode == HttpStatusCode.ServiceUnavailable),

            DelayGenerator = args =>
            {
                var response = args.Outcome.Result;

                var retryAfter = response?.Headers?
                    .FirstOrDefault(h => h.Name?.Equals("Retry-After", StringComparison.OrdinalIgnoreCase) == true)
                    ?.Value?.ToString();

                if (!string.IsNullOrWhiteSpace(retryAfter) &&
                    double.TryParse(retryAfter, out var headerSeconds))
                {
                    return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(headerSeconds));
                }

                var delaySeconds =
                    Random.Shared.NextDouble() * (maxDelaySeconds - minDelaySeconds) + minDelaySeconds;

                return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(delaySeconds));
            }
        };

        return new ResiliencePipelineBuilder<RestResponse>()
            .AddRetry(retryOptions)
            .Build();
    }
}