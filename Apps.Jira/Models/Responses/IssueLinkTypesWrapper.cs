namespace Apps.Jira.Models.Responses
{
    public class IssueLinkTypesWrapper
    {
        public IssueLinkTypeDto[] IssueLinkTypes { get; set; } = Array.Empty<IssueLinkTypeDto>();
    }

    public class IssueLinkTypeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Inward { get; set; } = string.Empty;
        public string Outward { get; set; } = string.Empty;
        public string Self { get; set; } = string.Empty;
    }
}
