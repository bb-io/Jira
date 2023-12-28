using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Jira.Models.Responses;

public class DownloadAttachmentResponse
{
    public FileReference Attachment { get; set; }
}