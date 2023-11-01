using Apps.Jira.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using RestSharp;

namespace Apps.Jira.Webhooks.Handlers
{
    public abstract class BaseWebhookHandler : BaseInvocable, IWebhookEventHandler
    {
        private readonly string[] _subscriptionEvents;

        protected BaseWebhookHandler(InvocationContext invocationContext, string[] subscriptionEvents) : base(invocationContext)
        {
            _subscriptionEvents = subscriptionEvents;
        }

        public async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
            Dictionary<string, string> values)
        {
            var jiraHost = new Uri(authenticationCredentialsProviders.First(p => p.KeyName == "JiraUrl").Value).Host;
            var payloadUrl = values["payloadUrl"];
            var bridgeClient = new RestClient($"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/webhooks/jira");
            
            foreach (var subscriptionEvent in _subscriptionEvents)
            {
                var bridgeSubscribeRequest = new RestRequest($"/{jiraHost}/{subscriptionEvent}", Method.Post);
                bridgeSubscribeRequest.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
                bridgeSubscribeRequest.AddBody(payloadUrl);
                await bridgeClient.ExecuteAsync(bridgeSubscribeRequest);
            }
        }

        public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
            Dictionary<string, string> values)
        {
            var jiraHost = new Uri(authenticationCredentialsProviders.First(p => p.KeyName == "JiraUrl").Value).Host;
            var payloadUrl = values["payloadUrl"];
            var bridgeClient = new RestClient($"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/webhooks/jira");
            
            foreach (var subscriptionEvent in _subscriptionEvents)
            {
                var getWebhooksRequest = new RestRequest($"/{jiraHost}/{subscriptionEvent}");
                getWebhooksRequest.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
                var webhooks = await bridgeClient.GetAsync<List<BridgeGetResponse>>(getWebhooksRequest);
                var webhook = webhooks.FirstOrDefault(w => w.Value == payloadUrl);
                
                var deleteWebhookRequest = new RestRequest($"/{jiraHost}/{subscriptionEvent}/{webhook.Id}", Method.Delete);
                deleteWebhookRequest.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
                await bridgeClient.ExecuteAsync(deleteWebhookRequest);
            }
        }
    }
}