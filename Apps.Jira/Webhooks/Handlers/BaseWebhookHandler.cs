using Apps.Jira.Webhooks.Payload;
using Apps.Jira.Webhooks.Responses;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Webhooks;
using RestSharp;

namespace Apps.Jira.Webhooks.Handlers
{
    public abstract class BaseWebhookHandler : IWebhookEventHandler<IWebhookInput>
    {
        private readonly string[] _subscriptionEvents;
        private readonly string _jqlFilter;
        protected readonly IWebhookInput? webhookInput;

        public BaseWebhookHandler(string[] subscriptionEvents)
        {
            _subscriptionEvents = subscriptionEvents;
            _jqlFilter = GetJqlFilter();
        }

        public BaseWebhookHandler(string[] subscriptionEvents, [WebhookParameter] IWebhookInput webhookInput)
            : this(subscriptionEvents)
        {
            this.webhookInput = webhookInput;
        }
        
        public async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
            Dictionary<string, string> values)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest("/webhook", Method.Post, authenticationCredentialsProviders);

            request.AddJsonBody(new 
            {
                url = values["payloadUrl"],
                webhooks = new []
                {
                    new
                    {
                        events = _subscriptionEvents,
                        jqlFilter = _jqlFilter
                    }
                }
            });
            await client.ExecuteAsync(request);
        }

        public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
            Dictionary<string, string> values)
        {
            var client = new JiraClient(authenticationCredentialsProviders);
            var request = new JiraRequest("/webhook", Method.Get, authenticationCredentialsProviders);
            var webhooks = client.Get<WebhooksResponse>(request).Values;
            var currentWebhookId = webhooks
                .FirstOrDefault(w => w.Events.SequenceEqual(_subscriptionEvents) && w.JqlFilter == _jqlFilter)?.Id;
            if (currentWebhookId == null)
                return;
            
            var deleteRequest = new JiraRequest("/webhook", Method.Delete, authenticationCredentialsProviders);
            var webhookId = int.Parse(currentWebhookId);
            deleteRequest.AddJsonBody(new
            {
                webhooksIds = new[] { webhookId }
            });
            await client.ExecuteAsync(request);
        }
        
        protected abstract string GetJqlFilter();
    }
}