using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Identifiers;

public class AttachmentIdentifier
{
    [Display("Attachment ID")]
    public string AttachmentId { get; set; }
}