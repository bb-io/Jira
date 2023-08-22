using Newtonsoft.Json;

namespace Apps.Jira.Dtos
{
    public class IssueDto
    {
        public string Summary { get; set; }
        
        [JsonProperty("issuetype")]
        public IssueType IssueType { get; set; }
        
        public ProjectDto Project { get; set; }
        
        public Priority Priority { get; set; }
        
        public Status Status { get; set; }
        
        public UserDto? Assignee { get; set; }

        public Description? Description { get; set; }
        
        public IEnumerable<AttachmentDto>? Attachment { get; set; }
    }

    public class Description
    {
        public string Type { get; set; }
        public List<ContentObj> Content { get; set; }
    }

    public class ContentObj
    {
        public string Type { get; set; }
        public List<ContentData> Content { get; set; }
    }

    public class ContentData
    {
        public string Type { get; set; }
        public string Text { get; set; }
    }

    public class IssueType
    {
        public string Name { get; set; }
    }

    public class Priority
    {
        public string Name { get; set; }
    }

    public class Status
    {
        public string Name { get; set; }
    }

    public class IssueWrapper
    {
        public string Key { get; set; }
        public IssueDto Fields { get; set; }
    }
}
