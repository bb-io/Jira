using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos;

public class AttachmentDto
{
    [Display("Attachment ID")]
    public string Id { get; set; }
    
    public string Filename { get; set; }
    
    [Display("Size in bytes")]
    public int Size { get; set; }
    
    [Display("Mime type")]
    public string MimeType { get; set; }
    
    [Display("Content download URL")]
    public string Content { get; set; }
}