using Apps.Jira.Actions;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Tests.Jira.Base;

namespace Tests.Jira;

[TestClass]
public class IssueTests :TestBase
{
    [TestMethod]
    public async Task CreateIssue_ReturnsSuccess()
    {
        var action = new IssueActions(InvocationContext,FileManager);

        var project = new ProjectIdentifier
        {
            ProjectKey = "AC"
        };
        var request = new CreateIssueRequest
        {
            Summary = "Test issue local2",
            IssueTypeId = "10006",
            Description = "Test description",
            ParentIssueKey = "AC-1"
        };
        var response = await action.CreateIssue(project,request);

        Console.WriteLine(response.Key);

        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task UpdateIssue_ReturnsSuccess()
    {
        // Arrange
        var action = new IssueActions(InvocationContext, FileManager);
        var project = new ProjectIdentifier { ProjectKey = "TL" };
        var issue = new IssueIdentifier { IssueKey = "TL-11" };
        var request = new UpdateIssueRequest
        {
            StatusId = "3",
        };

        // Act
        await action.UpdateIssue(project, issue, request);
    }

    [TestMethod]
    public async Task GetIssue_ReturnsSuccess()
    {
        var action = new IssueActions(InvocationContext, FileManager);

        var project = new IssueIdentifier
        {
            IssueKey = "AC-5"
        };

        var response = await action.GetIssueByKey(project);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task ListRecentlyCreatedIssues_ReturnsSuccess()
    {
        var action = new IssueActions(InvocationContext, FileManager);

        var project = new ProjectIdentifier { ProjectKey = "AC" };
        var listRequest = new ListRecentlyCreatedIssuesRequest
        {
            //Hours = 500,
            //Labels = ["form", "non-existent-label"],
            //Versions = ["v1.0", "v1.1"]
            //ParentIssue = "AC-8"
        };

        var response = await action.ListRecentlyCreatedIssues(project, listRequest, null);
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);

        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task AddIssueComment_ReturnsSuccess()
    {
        var action = new IssueCommentActions(InvocationContext);

        var project = new ProjectIdentifier
        {
            ProjectKey = "AC"
        };

        var issue = new IssueIdentifier
        {
            IssueKey = "AC-1"
        };
        var request = new AddIssueCommentRequest
        {
            Text= "Test comment Test",
        };
        await action.AddIssueComment(issue, request);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task CloneIssue_ReturnsSuccess()
    {
        var action = new IssueActions(InvocationContext, FileManager);
        var issue = new IssueIdentifier { IssueKey = "AC-18" };
        var cloneIssue = new CloneIssueRequest
        {
           ReporterName = "712020:75495005-bcf9-4f19-8ea8-d038a4dba86b",
        };

        var response = await action.CloneIssue(issue, cloneIssue);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);

        Assert.IsNotNull(response);
    }
}
