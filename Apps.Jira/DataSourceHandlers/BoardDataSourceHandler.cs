using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var endpoint = "/rest/agile/1.0/board";

            var request = new JiraRequest(endpoint, Method.Get);
            var response = await Client.ExecuteWithHandling<BoardsResponse>(request);

            return response.Values.ToDictionary(board => board.Id.ToString(), board => board.Name);
        }
    }
}
