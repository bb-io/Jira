using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Jira.DataSourceHandlers.Enum
{
    public class RichTextMarksHandler : IStaticDataSourceHandler
    {
        protected Dictionary<string, string> EnumValues => new()
        {
            {"code", "Code"},
            {"link", "Link"},
            {"strike","Strike" },
            {"strong", "Strong" },
            {"underline", "Underline" }
        };
        public Dictionary<string, string> GetData()
        {
            return EnumValues;
        }
    }
}
