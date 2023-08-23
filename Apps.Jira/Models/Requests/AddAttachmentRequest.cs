namespace Apps.Jira.Models.Requests;

public class AddAttachmentRequest
{
    public string Filename { get; set; }
    public byte[] File { get; set; }
}