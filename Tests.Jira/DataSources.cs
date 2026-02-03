using Apps.Jira.Actions;
using Apps.Jira.DataSourceHandlers;
using Apps.Jira.DataSourceHandlers.CustomFields;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Webhooks.Inputs;
using Apps.Jira.Webhooks.Polling;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Polling;
using Tests.Jira.Base;

namespace Tests.Jira;

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

        for (int i = 0; i < 50; i++)
        {
            var response = await handler.GetCustomMultiselectFieldValue(input1, input2);
            Console.WriteLine($"{response.Count}");
            Assert.IsNotNull(response);
        }
    }

    [TestMethod]
    public async Task Set_MulticheckboxtFieldHandlerReturnsValues()
    {
        var handler = new IssueCustomFieldsActions(InvocationContext);

        var input1 = new IssueIdentifier
        {
            IssueKey = "AC-33"
        };

        var input2 = new CustomMulticheckboxesFieldIdentifier
        {
            CustomMulticheckboxesFieldId = "customfield_10182"
        };

        await handler.SetCustomMulticheckboxesFieldValue(input1, input2, new CustomMulticheckboxesFieldInput { Values= ["Multi 5", "Multi 1"] });
        Assert.IsTrue(true);
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
            IssueKey = "LOCP-47309"
        };

        var input2 = new CustomNumericFieldIdentifier
        {
            CustomNumberFieldId = "customfield_10022"
        };
        var input3 = 0.5;
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
    public async Task GetLabelsHandlerReturnsValues()
    {
        var handler = new IssueLabelDataHandler(InvocationContext);

        var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);

        foreach (var item in response)
        {
            Console.WriteLine($"{item.Value}: {item.Key}");
        }

        Assert.IsNotNull(response);

    }

    [TestMethod]
    public async Task GetIssuesTypeHandlerReturnsValues()
    {
        var handler = new IssueTypeDataSourceHandler(InvocationContext, new ProjectIdentifier { ProjectKey = "AC" });

        var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);

        foreach (var item in response)
        {
            Console.WriteLine($"{item.Value}: {item.Key}");
        }

        Assert.IsNotNull(response);

    }

    [TestMethod]
    public async Task CustomMulticheckboxFieldDataSourceHandler_IsSuccess()
    {
        var handler = new CustomMulticheckboxFieldDataSourceHandler(InvocationContext);

        var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);

        foreach (var item in response)
        {
            Console.WriteLine($"{item.Value}: {item.Key}");
        }

        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task CustomMulticheckboxesOptionsDataSourceHandler_IsSuccess()
    {
        var handler = new CustomMulticheckboxesOptionsDataSourceHandler(InvocationContext, new IssueIdentifier {  IssueKey="AC-33"},
            new CustomMulticheckboxesFieldIdentifier {  CustomMulticheckboxesFieldId= "customfield_10182" });

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
            IssueKey = "LOCP-47309"
        };

        var input2 = new CustomStringFieldIdentifier
        {
            CustomStringFieldId = "customfield_10022"
        };
        var response = await handler.GetCustomStringFieldValue(input1, input2);

        Console.WriteLine($"{response.Value}");

        Assert.IsNotNull(response);

    }


    [TestMethod]
    public async Task Get_Relevant_Sprint_ReturnsValues()
    {
        var handler = new SprintActions(InvocationContext);

        var response = await handler.GetRelevantSprintForDate(
            new GetSprintByDateRequest { BoardId = "456", Date = new DateTime(2025, 5, 20) });

        foreach (var item in response.Sprints)
        {
            Console.WriteLine($"{item.Name}: {item.Id}");

            Assert.IsNotNull(response);
        }
    }

    [TestMethod]
    public async Task Get_User_by_Email_ReturnsValues()
    {
        var handler = new UserActions(InvocationContext);

        var response = await handler.FindUserByEmail(new UserEmailRequest { Email = "ariabushenko@blackbird.io" });

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);

    }


    [TestMethod]
    public async Task Get_User_email_ReturnsValues()
    {
        var handler = new UserActions(InvocationContext);

        var response = await handler.GetUserEmail(new UserIdentifier { AccountId = "712020:6965b2d5-4fb8-4142-b657-41ce3db735e9" });

        Console.WriteLine($"{response.Email}");
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task Set_RichtextFieldHandlerReturnsValues()
    {
        var handler = new IssueCustomFieldsActions(InvocationContext);

        var input1 = new IssueIdentifier
        {
            IssueKey = "TES-4"
        };

        var input2 = new CustomStringFieldIdentifier
        {

        };

        var text = "https://cloud.memsource.com/web/project2/show/WdwZFaKaeUHD2JKyymEri2";
        await handler.SetCustomRichTextFieldValue(input1, input2, text);


        Assert.IsTrue(true);

    }

    [TestMethod]
    public async Task FixVersionDatahandler_ReturnsValues()
    {
        var handler = new FixVersionDataHandler(InvocationContext, new ProjectIdentifier { ProjectKey = "SKP" });
        var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);
        foreach (var item in response)
        {
            Console.WriteLine($"{item.Value}: {item.DisplayName}");
        }
        Assert.IsNotNull(response);
    }

    //IssueTypeDataSourceHandler


    [TestMethod]
    public async Task IssueTypeDataSourceHandler_ReturnsValues()
    {
        var handler = new IssueTypeDataSourceHandler(InvocationContext, new ProjectIdentifier { ProjectKey = "AC" });
        var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);
        foreach (var item in response)
        {
            Console.WriteLine($"{item.Value}: {item.Key}");
        }
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task IssueResolutionDataSourceHandler_ReturnsValues()
    {
        var handler = new IssueResolutionDataSourceHandler(InvocationContext);
        var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);
        foreach (var item in response)
        {
            Console.WriteLine($"{item.Value}: {item.Key}");
        }
        Assert.IsNotNull(response);
    }

    //SetResolution

    [TestMethod]
    public async Task SetResolution_ReturnsValues()
    {
        var action = new IssueCustomFieldsActions(InvocationContext);
        await action.SetResolution(new IssueIdentifier { IssueKey= "LOC-15297" }, new SetResolutionRequest { ResolutionId= "10003" });
        Assert.IsTrue(true);
    }


    [TestMethod]
    public async Task OnIssuesReachStatusPolling_ReturnsValues()
    {
        var polling = new IssuesPolling(InvocationContext);
        var projectId = new ProjectIdentifier { ProjectKey = "AC" };
        var issueReach = new IssuesReachStatusInput
        {
            IssueKeys = ["AC-1", "AC-3"],
            Statuses = ["10004", "10005"]
        };
        var request = new PollingEventRequest<PollingMemory>
        {
            //PollingTime = 
        };
        var response = await polling.OnIssuesReachStatusPolling(request,projectId, issueReach);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);

    }
}