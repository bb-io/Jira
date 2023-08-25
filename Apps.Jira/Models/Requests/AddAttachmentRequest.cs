using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Jira.Models.Requests;

public class AddAttachmentRequest
{
    public File Attachment { get; set; }
}