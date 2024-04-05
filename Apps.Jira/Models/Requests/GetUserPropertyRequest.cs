using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests;

public class GetUserPropertyRequest
{
    [Display("Account ID"), DataSource(typeof(UserDataSourceHandler))]
    public string AccountId { get; set; }
    
    [Display("Property Key"), DataSource(typeof(UserPropertiesDataHandler))]
    public string PropertyKey { get; set; }
}