using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Jira.Models.Requests;

public class AddAttachmentRequest
{
    public FileReference Attachment { get; set; }
}