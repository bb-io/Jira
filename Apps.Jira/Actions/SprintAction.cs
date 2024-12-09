using System.Globalization;
using Apps.Jira.Dtos;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;

namespace Apps.Jira.Actions
{
    //[ActionList]
    //public class SprintActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    //: JiraInvocable(invocationContext)
    //{

    //    [Action("Get relevant sprint for date", Description = "Get Sprint corresponding to the specified date for a selected board.")]
    //    public async Task<SprintsResponse> GetRelevantSprintForDate(
    //        [ActionParameter] GetSprintByDateRequest requestModel)
    //    {

    //        var endpoint = $"/rest/agile/1.0/board/{requestModel.BoardId}/sprint";
    //        var request = new JiraRequest(endpoint, Method.Get);
    //        var response = await Client.ExecuteWithHandling<SprintsWrapper>(request);

    //        var relevantSprints = response.Values
    //            .Where(sprint =>
    //                sprint.StartDate <= requestModel.Date && sprint.EndDate >= requestModel.Date)
    //            .ToList();

    //        if (!relevantSprints.Any())
    //            return new SprintsResponse
    //            {
    //                Message = $"No sprints found for the date {requestModel.Date.ToShortDateString()}.",
    //                Sprints = new List<SprintDto>()
    //            };

    //        return new SprintsResponse
    //        {
    //            Message = $"Found {relevantSprints.Count} relevant sprint(s) for the date {requestModel.Date.ToShortDateString()}.",
    //            Sprints = relevantSprints.Select(s => new SprintDto(s)).ToList()
    //        };
    //    }


    //}
}
