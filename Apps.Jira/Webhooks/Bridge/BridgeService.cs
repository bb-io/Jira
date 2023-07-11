using Apps.Jira.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;

namespace Apps.Jira.Webhooks.Bridge
{
    public class BridgeService
    {
        public BridgeService(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) 
        {

        }
        public void Subscribe(string _event, string jiraInstance, string url)
        {
            var client = new RestClient(ApplicationConstants.BridgeServiceUrl);
            var request = new RestRequest($"/{jiraInstance}/{_event}", Method.Post);
            request.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
            request.AddBody(url);
            client.Execute(request);
        }

        public void Unsubscribe(string _event, string jiraInstance, string url)
        {
            var client = new RestClient(ApplicationConstants.BridgeServiceUrl);
            var requestGet = new RestRequest($"/{jiraInstance}/{_event}", Method.Get);
            requestGet.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
            var webhooks = client.Get<List<BridgeGetResponse>>(requestGet);
            var webhook = webhooks.FirstOrDefault(w => w.Value == url);

            var requestDelete = new RestRequest($"/{jiraInstance}/{_event}/{webhook.Id}", Method.Delete);
            requestDelete.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
            client.Delete(requestDelete);
        }
    }
}
