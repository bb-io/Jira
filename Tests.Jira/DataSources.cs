using Apps.Jira.Actions;
using Apps.Jira.DataSourceHandlers;
using Apps.Jira.DataSourceHandlers.CustomFields;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
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
        public async Task CustomNumericFieldHandlerReturnsValues()
        {
            var handler = new CustomNumericFieldDataSourceHandler(InvocationContext);

            var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);

            foreach (var item in response)
            {
                Console.WriteLine($"{item.Value}: {item.Key}");
            }

            Assert.IsNotNull(response);

        }

        [TestMethod]
        public async Task Get_NumericFieldHandlerReturnsValues()
        {
            var handler = new IssueCustomFieldsActions(InvocationContext);

            var input1 = new IssueIdentifier 
            { 
                IssueKey = "TES-4"
            };

            var input2 = new CustomNumericFieldIdentifier
            {
                CustomNumberFieldId = "customfield_10054"
            };
            var response = await handler.GetCustomNumericFieldValue(input1,input2);
            
                Console.WriteLine($"{response.Value}");

            Assert.IsNotNull(response);

        }

        [TestMethod]
        public async Task Set_NumericFieldHandlerReturnsValues()
        {
            var handler = new IssueCustomFieldsActions(InvocationContext);

            var input1 = new IssueIdentifier
            {
                IssueKey = "TES-4"
            };

            var input2 = new CustomNumericFieldIdentifier
            {
                CustomNumberFieldId = "customfield_10054"
            };
            var input3 = 10230.0;
            await handler.SetCustomNumericFieldValue(input1, input2,input3);

            var response = await handler.GetCustomNumericFieldValue(input1, input2);
            Console.WriteLine($"{response.Value}");
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


        [TestMethod]
        public async Task GetIssuesHandlerReturnsValues()
        {
            var handler = new IssueDataSourceHandler(InvocationContext);

            var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);

            foreach (var item in response)
            {
                Console.WriteLine($"{item.Value}: {item.Key}");
            }

            Assert.IsNotNull(response);

        }

        [TestMethod]
        public async Task GetIssuesWrongReturnsValues()
        {
            var handler = new IssueActions(InvocationContext,FileManager);

            var input = new IssueIdentifier { IssueKey="TES-7"};
            await Assert.ThrowsExceptionAsync<PluginApplicationException>(async () =>
            {
                await handler.GetIssueByKey(input);
            });
        }

    }
}
