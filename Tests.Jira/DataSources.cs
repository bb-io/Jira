using Apps.Jira.Actions;
using Apps.Jira.DataSourceHandlers;
using Apps.Jira.DataSourceHandlers.CustomFields;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Webhooks.Payload;
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
        public async Task Get_MultiselectFieldHandlerReturnsValues()
        {
            var handler = new IssueCustomFieldsActions(InvocationContext);

            var input1 = new IssueIdentifier
            {
                IssueKey = "TES-4"
            };

            var input2 = new CustomNumericFieldIdentifier
            {
                CustomNumberFieldId = "customfield_10055"
            };
            var response = await handler.GetCustomNumericFieldValue(input1, input2);

            Console.WriteLine($"{response.Value}");

            Assert.IsNotNull(response);

        }

        [TestMethod]
        public async Task Set_MultiselectFieldHandlerReturnsValues()
        {
            var handler = new IssueCustomFieldsActions(InvocationContext);

            var input1 = new IssueIdentifier
            {
                IssueKey = "TES-4"
            };

            var input2 = new CustomMultiselectFieldIdentifier
            {
                CustomMultiselectFieldId = "customfield_10055"
            };
            var input3 = new CustomMultiselectFieldInput { ValueProperty = ["Hello"] };
            await handler.SetCustomMultiselectFieldValue(input1, input2, input3);

            for (int i = 0; i<50;i++)
            {
                var response = await handler.GetCustomMultiselectFieldValue(input1, input2);
                Console.WriteLine($"{response.Count}");
                Assert.IsNotNull(response);
            }
        }

        [TestMethod]
        public async Task MultiselectFieldHandlerReturnsValues()
        {
            var handler = new CustomMultiselectFieldDataSourceHandler(InvocationContext);

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
            var response = await handler.GetCustomNumericFieldValue(input1, input2);

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
            await handler.SetCustomNumericFieldValue(input1, input2, input3);

            var response = await handler.GetCustomNumericFieldValue(input1, input2);
            Console.WriteLine($"{response.Value}");
            Assert.IsNotNull(response);

        }

        [TestMethod]
        public async Task MoveIssue_IsNotNull()
        {
            var handler = new IssueActions(InvocationContext, FileManager);

            var input = new MoveIssuesToSprintRequest { BoardId = "1", SprintId = "1", Issues = ["TES-6", "TES-4", "TES-2"] };

            var response = await handler.MoveIssuesToSprint(input);
            Console.WriteLine($"{response.Success} {response.Message}");
            Assert.IsTrue(response.Success);
        }


        [TestMethod]
        public async Task RetryAfter_IsNotFail()
        {
            var handler = new IssueActions(InvocationContext, FileManager);

            var input = new MoveIssuesToSprintRequest { BoardId = "1", SprintId = "1", Issues = ["TES-6", "TES-4", "TES-2"] };

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
            var handler = new IssueActions(InvocationContext, FileManager);

            var input = new IssueIdentifier { IssueKey = "TES-7" };
            await Assert.ThrowsExceptionAsync<PluginApplicationException>(async () =>
            {
                await handler.GetIssueByKey(input);
            });
        }


        [TestMethod]
        public async Task Set_TextFieldHandlerReturnsValues()
        {
            var handler = new IssueCustomFieldsActions(InvocationContext);

            var input1 = new IssueIdentifier
            {
                IssueKey = "LOC-34522"
            };

            var input2 = new CustomStringFieldIdentifier
            {
                CustomStringFieldId = "customfield_11258"
            };
            var input3 = "XTM project name: ZendeskFAQ_16546575023261_4";
            await handler.SetCustomStringFieldValue(input1, input2, input3);

            var response = await handler.GetCustomStringFieldValue(input1, input2);
            Console.WriteLine($"{response.Value}");
            Assert.IsNotNull(response);

        }

        [TestMethod]
        public async Task Get_StringFieldHandlerReturnsValues()
        {
            var handler = new IssueCustomFieldsActions(InvocationContext);

            var input1 = new IssueIdentifier
            {
                IssueKey = "LOC-34522"
            };

            var input2 = new CustomStringFieldIdentifier
            {
                CustomStringFieldId = "customfield_11258"
            };
            var response = await handler.GetCustomStringFieldValue(input1, input2);

            Console.WriteLine($"{response.Value}");

            Assert.IsNotNull(response);

        }


        [TestMethod]
        public async Task Get_Relevant_Sprint_ReturnsValues()
        {
            var handler = new SprintActions(InvocationContext, FileManager);

            var response = await handler.GetRelevantSprintForDate(
                new GetSprintByDateRequest { BoardId= "456", Date = new DateTime(2025, 5, 20)});

            foreach (var item in response.Sprints)
            {
                Console.WriteLine($"{item.Name}: {item.Id}");

                Assert.IsNotNull(response);
            }
        }

        //GetUserEmail

        [TestMethod]
        public async Task Get_User_email_ReturnsValues()
        {
            var handler = new UserActions(InvocationContext);

            var response = await handler.GetUserEmail(new UserIdentifier { AccountId= "712020:6965b2d5-4fb8-4142-b657-41ce3db735e9" });

            Console.WriteLine($"{response.Email}");
            Assert.IsNotNull(response);
        }
    }
}