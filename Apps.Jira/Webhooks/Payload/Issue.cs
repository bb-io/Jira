using System.Text.Json.Serialization;
using Apps.Jira.Dtos;

namespace Apps.Jira.Webhooks.Payload
{
    public class Issue
    {
        public string Id { get; set; }
        public string Self { get; set; }
        public string Key { get; set; }
        public Fields Fields { get; set; }
    }

    public class Fields
    {
        [JsonPropertyName("issuetype")] 
        public IssueType IssueType { get; set; }
        
        public Project Project { get; set; }
        
        public Priority? Priority { get; set; }
        
        public Assignee? Assignee { get; set; }
        
        public Status Status { get; set; }
        
        public string Summary { get; set; }
        
        public IEnumerable<AttachmentDto> Attachment { get; set; }
        
        public string? Description { get; set; }
        
        [JsonPropertyName("labels")]
        public List<string> Labels { get; set; } = new();
    }
    
    public class IssueType
    {
        public string Self { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public bool Subtask { get; set; }
        public int AvatarId { get; set; }
        public string EntityId { get; set; }
        public int HierarchyLevel { get; set; }
    }
    
    public class Project
    {
        public string Self { get; set; }
        public string Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string ProjectTypeKey { get; set; }
        public bool Simplified { get; set; }
    }
    
    public class Priority
    {
        public string Self { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
    }
    
    public class Assignee
    {
        public string Self { get; set; }
        public string AccountId { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public bool Active { get; set; }
        public string TimeZone { get; set; }
        public string AccountType { get; set; }
    }

    public class Status
    {
        public string Self { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
    }
}

