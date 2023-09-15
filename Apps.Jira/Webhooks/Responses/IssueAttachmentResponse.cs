using Apps.Jira.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Webhooks.Responses;

public class IssueAttachmentResponse
{
    [Display("Issue key")]
    public string IssueKey { get; set; }
        
    [Display("Project key")]
    public string ProjectKey { get; set; }

    public AttachmentDto Attachment { get; set; }
}