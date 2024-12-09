using Apps.Jira;
using Apps.Jira.Dtos;
using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.MemoQ.DataSourceHandlers
{
    public class SprintDataHandler(InvocationContext invocationContext, [ActionParameter] BoardIdentifier input)
    : JiraInvocable(invocationContext), IAsyncDataSourceHandler
    {
        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            const int maxResultsPerPage = 50;
            var startAt = 0;
            var isLast = false;

            var allSprints = new List<SprintMoveIssueDto>();
            while (!isLast)
            {
                var request = new JiraRequest($"/rest/agile/1.0/board/{input.BoardId}/sprint", Method.Get);
                var sprints = await Client.ExecuteWithHandling<SprintsPaginationDto>(request);

                if (sprints?.Values != null)
                {
                    allSprints.AddRange(sprints.Values);
                }

                startAt += sprints?.MaxResults ?? maxResultsPerPage;
                isLast = sprints?.IsLast ?? true;
            }

            return allSprints
                .Where(sprint => context.SearchString == null
                                 || sprint.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(sprint => sprint.Id.ToString(), sprint => sprint.Name);
        }
    }
}