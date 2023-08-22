namespace Apps.Jira.Models.Responses;

public class DownloadAttachmentResponse
{
    public string Filename { get; set; }
    public byte[] File { get; set; }
}