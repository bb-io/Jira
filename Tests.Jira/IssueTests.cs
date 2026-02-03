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
            IssueTypeId = "10005",
            Description = "Test description",
            Labels = new List<string> { "form", "api-created" }
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
            IssueKey = "LOC-15297"
        };

        var response = await action.GetIssueByKey(project);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task GetIssueComment_ReturnsSuccess()
    {
        var action = new IssueCommentActions(InvocationContext);

        var project = new IssueCommentIdentifier
        {
            IssueKey = "AC-33",
            CommentId = "10042"
        };

        var response = await action.GetIssueComment(project);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task GetIssueComments_ReturnsSuccess()
    {
        var action = new IssueCommentActions(InvocationContext);

        var project = new GetIssueCommentsRequest
        {
            IssueKey = "AC-33"
        };

        var response = await action.GetIssueComments(project);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task FindComment_ReturnsSuccess()
    {
        var action = new IssueCommentActions(InvocationContext);

        var project = new FindCommentRequest
        {
            IssueKey = "AC-33",
            CommentContains = "test",
            Latest = true
        };

        var response = await action.FindComment(project);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task AddIssueComment_ReturnsSuccess()
    {
        var action = new IssueCommentActions(InvocationContext);

        var issue = new IssueIdentifier
        {
            IssueKey = "AC-33"
        };
        var project = new AddIssueCommentRequest
        {
            Text = "This is a test comment from API"
        };

        var response = await action.AddIssueComment(issue, project);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    //DeleteIssueComment

    [TestMethod]
    public async Task DeleteIssueComment_ReturnsSuccess()
    {
        var action = new IssueCommentActions(InvocationContext);

        var project = new IssueCommentIdentifier
        {
            IssueKey = "AC-33",
            CommentId = "10042"
        };

        await action.DeleteIssueComment( project);
        Assert.IsTrue(true);
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
