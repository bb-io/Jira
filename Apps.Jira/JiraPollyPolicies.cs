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
            double baseDelaySeconds = 5.0;
            double maxRetryDelaySeconds = 30.0;
            double jitterMin = 0.7;
            double jitterMax = 1.3;
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
                else if (outcome.Result.StatusCode == HttpStatusCode.TooManyRequests)
                {                  
                    delaySeconds = Math.Min(baseDelaySeconds * Math.Pow(2, attempt - 1), maxRetryDelaySeconds);
                }

                double jitterMultiplier = random.NextDouble() * (jitterMax - jitterMin) + jitterMin;
                double finalDelaySeconds = delaySeconds * (1 + jitterMultiplier);

                return TimeSpan.FromSeconds(finalDelaySeconds);
            },
            onRetryAsync: async (outcome, timeSpan, attempt, ctx) =>
            {
                await Task.CompletedTask;
            });
        }
    }
}