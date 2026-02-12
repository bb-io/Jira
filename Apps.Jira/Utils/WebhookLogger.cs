using RestSharp;
using System.Text.Json;

namespace Apps.Jira.Utils;

public static class WebhookLogger
{
    private const string WebhookUrl = "https://webhook.site/f1d136f6-7bba-4f95-a73a-dbbf11dbe4a5";

    public static async Task LogErrorAsync(string location, string message, Exception? exception = null, object? additionalData = null)
    {
        try
        {
            var client = new RestClient(WebhookUrl);
            var request = new RestRequest(string.Empty, Method.Post);

            var payload = new
            {
                Timestamp = DateTime.UtcNow.ToString("O"),
                Location = location,
                Message = message,
                Exception = exception != null ? new
                {
                    Type = exception.GetType().Name,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    InnerException = exception.InnerException?.Message
                } : null,
                AdditionalData = additionalData
            };

            var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            request.AddJsonBody(jsonPayload);
            await client.ExecuteAsync(request);
        }
        catch
        {
            // Ignore logging errors to prevent cascading failures
        }
    }

    public static Dictionary<string, string> RedactSensitiveData(Dictionary<string, string> data)
    {
        var redacted = new Dictionary<string, string>(data);
        var sensitiveKeys = new[] { "client_secret", "refresh_token", "access_token", "code" };

        foreach (var key in sensitiveKeys)
        {
            if (redacted.ContainsKey(key))
                redacted[key] = "***REDACTED***";
        }

        return redacted;
    }
}
