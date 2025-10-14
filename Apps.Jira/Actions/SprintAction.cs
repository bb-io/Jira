using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.Actions;

[ActionList]
public class SprintActions(InvocationContext invocationContext) : JiraInvocable(invocationContext)
{
    [Action("Get relevant sprint for date", Description = "Get Sprint corresponding to the specified date for a selected board.")]
    public async Task<SprintsResponse> GetRelevantSprintForDate(
        [ActionParameter] GetSprintByDateRequest requestModel)
    {
        var client = new JiraClient(InvocationContext.AuthenticationCredentialsProviders, "agile");

        var jiraRequest = new JiraRequest($"/board/{requestModel.BoardId}/sprint", Method.Get);

        var allSprints = await client.Paginate<Sprint>(jiraRequest);

        var relevant = allSprints
            .Where(s => s.StartDate <= requestModel.Date && s.EndDate >= requestModel.Date)
            .ToList();

        if (!relevant.Any())
            return new SprintsResponse
            {
                Message = $"No sprints found for {requestModel.Date:yyyy-MM-dd}.",
                Sprints = new List<SprintDto>()
            };

        return new SprintsResponse
        {
            Message = $"Found {relevant.Count} sprint(s) for {requestModel.Date:yyyy-MM-dd}.",
            Sprints = relevant.Select(s => new SprintDto(s)).ToList()
        };
    }
}
