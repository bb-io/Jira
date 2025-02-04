using System.Net;
using Polly;
using Polly.Retry;
using RestSharp;

namespace Apps.Jira
{
    public static class JiraPollyPolicies
    {
        public static AsyncRetryPolicy<RestResponse> GetTooManyRequestsRetryPolicy(int retryCount = 10)
        {
            return Policy
        .HandleResult<RestResponse>(r => (int)r.StatusCode == 429)
        .WaitAndRetryAsync<RestResponse>(
            retryCount,
            sleepDurationProvider: (attempt, outcome, ctx) =>
            {
                var retryAfterHeader = outcome.Result.Headers
                    .FirstOrDefault(h => h.Name.Equals("Retry-After", StringComparison.OrdinalIgnoreCase))
                    ?.Value?.ToString();

                return double.TryParse(retryAfterHeader, out var seconds)
                    ? TimeSpan.FromSeconds(seconds)
                    : TimeSpan.FromSeconds(5);
            },
            onRetryAsync: async (outcome, timeSpan, attempt, ctx) =>
            {
                await Task.CompletedTask;
            }
        );
        }
    }
}
