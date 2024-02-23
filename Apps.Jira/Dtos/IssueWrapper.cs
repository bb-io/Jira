using Newtonsoft.Json;

namespace Apps.Jira.Dtos;

public class IssuesWrapper
{
    public IEnumerable<IssueWrapper> Issues { get; set; }
}

public class IssueWrapper
{
    public string Key { get; set; }
    public IssueFields Fields { get; set; }
}

public class IssueFields
{
    public string Summary { get; set; }
        
    [JsonProperty("issuetype")]
    public IssueTypeSimpleDto IssueType { get; set; }
        
    public ProjectDto Project { get; set; }
        
    public PriorityDto Priority { get; set; }
        
    public StatusDto Status { get; set; }
        
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