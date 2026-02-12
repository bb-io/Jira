using System.Net;
using Apps.Jira.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.Connections;

public class ConnectionValidator(InvocationContext invocationContext) : BaseInvocable(invocationContext), IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
        CancellationToken cancellationToken)
    {
        var client = new JiraClient(authenticationCredentialsProviders);
        var request = new JiraRequest("/myself", Method.Get);
        
        try
        {
            var response = await client.ExecuteAsync(request, cancellationToken);
            
            var isValid = response.StatusCode != HttpStatusCode.Unauthorized;
            
            if (!isValid)
            {
                await WebhookLogger.LogErrorAsync(
                    "ConnectionValidator.ValidateConnection",
                    "Connection validation failed",
                    null,
                    new
                    {
                        StatusCode = response.StatusCode,
                        ErrorMessage = response.ErrorMessage,
                        ResponseContent = response.Content,
                        AvailableCredentials = authenticationCredentialsProviders.Select(p => p.KeyName).ToList()
                    });
            }
            
            return new ConnectionValidationResponse
            {
                IsValid = isValid,
                Message = isValid ? "Success" : $"Validation failed: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            await WebhookLogger.LogErrorAsync(
                "ConnectionValidator.ValidateConnection",
                "Exception during connection validation",
                ex,
                new
                {
                    AvailableCredentials = authenticationCredentialsProviders.Select(p => p.KeyName).ToList()
                });

            InvocationContext.Logger?.LogError($"[JiraConnectionValidator] Exception occurred while validating connection: {ex.Message}", []);
            
            return new ConnectionValidationResponse
            {
                IsValid = false,
                Message = "Ping failed"
            };
        }
    }
}