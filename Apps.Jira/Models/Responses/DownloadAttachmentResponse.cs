using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Jira.Models.Responses;

public class DownloadAttachmentResponse
{
    public File Attachment { get; set; }
}