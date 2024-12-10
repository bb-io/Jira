using Apps.Jira;
using Apps.Jira.Dtos;
using Apps.Jira.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.MemoQ.DataSourceHandlers
{
    public class SprintDataHandler : JiraInvocable, IAsyncDataSourceHandler
    {
        private readonly MoveIssuesToSprintRequest _input;

        public SprintDataHandler(InvocationContext invocationContext, [ActionParameter] MoveIssuesToSprintRequest input)
            : base(invocationContext)
        {
            _input = input;
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            const int maxResultsPerPage = 50;
            var startAt = 0;
            var isLast = false;

            var authenticationProviders = InvocationContext.AuthenticationCredentialsProviders;
            var client = new JiraClient(authenticationProviders, "agile");

            var allSprints = new List<SprintMoveIssueDto>();

            while (!isLast)
            {
                var request = new JiraRequest($"/board/{_input.BoardId}/sprint?startAt={startAt}", Method.Get);
                var sprints = await client.ExecuteWithHandling<SprintsPaginationDto>(request);

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
