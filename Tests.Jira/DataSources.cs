using Apps.Jira.Actions;
using Apps.Jira.DataSourceHandlers.CustomFields;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.Jira.Base;

namespace Tests.Jira
{
    [TestClass]
    public class DataSources : TestBase
    {
        [TestMethod]
        public async Task CustomStringFieldHandlerReturnsValues()
        {
            var handler = new CustomStringFieldDataSourceHandler(InvocationContext);

            var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);

            foreach (var item in response)
            {
                Console.WriteLine($"{item.Value}: {item.Key}");
            }

            Assert.IsNotNull(response);

        }
   

        [TestMethod]
        public async Task MoveIssue_IsNotNull()
        {
            var handler = new IssueActions(InvocationContext, FileManager);

            var input = new MoveIssuesToSprintRequest { BoardId="1", SprintId = "1", Issues= ["TES-6", "TES-4", "TES-2"] };

            for (int i = 0; i < 50; i++)
            {
                var response = await handler.MoveIssuesToSprint(input);
                Console.WriteLine($"{response.Success} {response.Message}");
                Assert.IsTrue(response.Success);
            }
        }

    }
}
