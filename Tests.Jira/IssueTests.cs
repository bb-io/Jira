using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.Jira.Actions;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Tests.Jira.Base;

namespace Tests.Jira
{
    [TestClass]
    public class IssueTests :TestBase
    {
        [TestMethod]
        public async Task CreateIssue_ReturnsSucces()
        {
            var action = new IssueActions(InvocationContext,FileManager);

            var project = new ProjectIdentifier
            {
                ProjectKey = "ELTF"
            };
            var request = new CreateIssueRequest
            {
                Summary = "Test issue local",
                IssueTypeId = "10002",
                Description = "Test description",
                DueDate = new DateTime(2025,3,6),
                OriginalEstimate = "3600",
                Reporter = "712020:75495005-bcf9-4f19-8ea8-d038a4dba86b"
            };
            var response = await action.CreateIssue(project,request);

            Console.WriteLine(response.Key);

            Assert.IsNotNull(response);
        }


        [TestMethod]
        public async Task UpdateIssue_ReturnsSucces()
        {
            var action = new IssueActions(InvocationContext, FileManager);

            var project = new ProjectIdentifier
            {
                ProjectKey = "ELTF"
            };

            var issue = new IssueIdentifier
            {
                IssueKey = "ELTF-1"
            };
            var request = new UpdateIssueRequest
            {
                Summary = "Test issue",
                IssueTypeId = "10002",
                Description = "Test description",
               // DueDate = DateTime.Now.AddDays(10),
                OriginalEstimate = "3600",
                Reporter = "712020:75495005-bcf9-4f19-8ea8-d038a4dba86b"
            };
            await action.UpdateIssue(project, issue, request);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task GetIssue_ReturnsSucces()
        {
            var action = new IssueActions(InvocationContext, FileManager);

            var project = new IssueIdentifier
            {
                IssueKey = "AC-1"
            };
          
            var response = await action.GetIssueByKey(project);

            Console.WriteLine(response.DueDate);
            Console.WriteLine(response.Reporter.DisplayName);
            Assert.IsNotNull(response);
        }


        [TestMethod]
        public async Task AddIssueComment_ReturnsSucces()
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
                Text= "Test comment",
            };
            await action.AddIssueComment(issue, request);
            Assert.IsTrue(true);
        }
    }
}
