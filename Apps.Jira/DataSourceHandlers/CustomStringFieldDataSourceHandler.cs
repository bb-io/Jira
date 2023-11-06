﻿using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers;

public class CustomStringFieldDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    public CustomStringFieldDataSourceHandler(InvocationContext invocationContext): base(invocationContext) { }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new JiraClient(Creds);
        var request = new JiraRequest("/field", Method.Get, Creds);
        var fields = await client.ExecuteWithHandling<IEnumerable<FieldDto>>(request);
        var customStringFields = fields.Where(f =>
            f.Custom && f.Schema.Custom == "com.atlassian.jira.plugin.system.customfieldtypes:textfield");
        return customStringFields.ToDictionary(f => f.Id, f => f.Name);
    }
}