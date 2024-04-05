using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Identifiers;

public class UserIdentifier
{
    [Display("Account ID"), DataSource(typeof(UserDataSourceHandler))]
    public string AccountId { get; set; }
}