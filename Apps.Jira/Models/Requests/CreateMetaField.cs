namespace Apps.Jira.Models.Requests
{
    public class CreateMetaField
    {
        public string? Name { get; set; }
        public string? Key { get; set; }
        public string? SchemaType { get; set; }
        public string? SchemaItems { get; set; }
        public string? Custom { get; set; }
        public bool Required { get; set; }
    }
}
