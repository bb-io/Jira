using System.Net;
using Polly;
using Polly.Retry;
using RestSharp;

namespace Apps.Jira
{
    public static class JiraPollyPolicies
    {
        public static AsyncRetryPolicy<RestResponse> GetTooManyRequestsRetryPolicy(int retryCount = 5)
        {
            double minDelaySeconds = 5.0;
            double maxDelaySeconds = 45.0;
            var random = new Random();

            return Policy
                .HandleResult<RestResponse>(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync<RestResponse>(
                    retryCount,
                    sleepDurationProvider: (attempt, outcome, ctx) =>
                    {
                        double delaySeconds = 0;

                        var retryAfterHeader = outcome.Result.Headers
                            .FirstOrDefault(h => h.Name.Equals("Retry-After", StringComparison.OrdinalIgnoreCase))
                            ?.Value?.ToString();

                        if (!string.IsNullOrEmpty(retryAfterHeader) && double.TryParse(retryAfterHeader, out double headerSeconds))
                        {
                            delaySeconds = headerSeconds;
                        }
                        else
                        {
                            delaySeconds = random.NextDouble() * (maxDelaySeconds - minDelaySeconds) + minDelaySeconds;
                        }

                        return TimeSpan.FromSeconds(delaySeconds);
                    },
                    onRetryAsync: async (outcome, timeSpan, attempt, ctx) =>
                    {
                        await Task.CompletedTask;
                    });
        }
    }
}