using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Jira.DataSourceHandlers;

public class FormattingDataSource : IStaticDataSourceHandler
{
    public Dictionary<string, string> GetData()
    {
        return new()
        {
            { "none", "None" },
            { "strong", "Strong" },
            { "em", "Italic" },
            { "code", "Code" },
            { "strike", "Strike" },
            { "underline", "Underline" }
        };
    }
}