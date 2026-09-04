using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Jira.DataSourceHandlers;

public class CommentSortDataSource : IStaticDataSourceHandler
{
    public const string NewestFirst = "-created";
    public const string OldestFirst = "created";

    public Dictionary<string, string> GetData()
    {
        return new Dictionary<string, string>
        {
            { NewestFirst, "Newest to oldest" },
            { OldestFirst, "Oldest to newest" }
        };
    }
}
