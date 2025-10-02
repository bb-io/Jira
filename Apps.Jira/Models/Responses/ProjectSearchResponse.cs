namespace Apps.Jira.Models.Responses
{
    public class ProjectSearchResponse
    {
        public List<ProjectItem> Values { get; set; } = new();
    }

    public class ProjectItem
    {
        public string Key { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
}
