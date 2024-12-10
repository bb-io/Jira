using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers
{
    public class BoardDataSourceHandler : JiraInvocable, IAsyncDataSourceHandler
    {
        public BoardDataSourceHandler(InvocationContext invocationContext) : base(invocationContext) { }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
        {
            var authenticationProviders = InvocationContext.AuthenticationCredentialsProviders;

            var client = new JiraClient(authenticationProviders, "agile");

            var endpoint = "/board";
            var request = new JiraRequest(endpoint, Method.Get);

            var response = await client.ExecuteWithHandling<BoardsResponse>(request);

            return response.Values.ToDictionary(board => board.Id.ToString(), board => board.Name);
        }
    }
}