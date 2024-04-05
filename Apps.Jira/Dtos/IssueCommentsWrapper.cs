using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos;

public class IssueCommentsWrapper
{
    [Display("Start at")]
    public int StartAt { get; set; }
    
    [Display("Max results")]
    public int MaxResults { get; set; }
    
    public int Total { get; set; }
    
    public IssueCommentDto[] Comments { get; set; }
}

public class IssueCommentDto
{
    public string Self { get; set; }
    
    public string Id { get; set; }
    
    public AuthorDto Author { get; set; }
    
    public CommentBodyDto Body { get; set; }
    
    [Display("Update author")]
    public AuthorDto UpdateAuthor { get; set; }
    
    public string Created { get; set; }
    
    public string Updated { get; set; }
    
    [Display("Jsd public")]
    public bool JsdPublic { get; set; }
}

public class CommentBodyDto
{
    public CommentContentDto[] Content { get; set; }
    
    public string Type { get; set; }
    
    public int Version { get; set; }
}

public class CommentContentDto
{
    public CommentContentDataDto[] Content { get; set; }
    
    public string Type { get; set; }
}

public class CommentContentDataDto
{
    public string Text { get; set; }
    
    public string Type { get; set; }
}