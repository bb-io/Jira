using System.Net;
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
            
            return new ConnectionValidationResponse
            {
                IsValid = isValid,
                Message = isValid ? "Success" : $"Validation failed: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            InvocationContext.Logger?.LogError($"Connection validation failed: {ex.Message}", []);
            
            return new ConnectionValidationResponse
            {
                IsValid = false,
                Message = "Connection validation failed"
            };
        }
    }
}