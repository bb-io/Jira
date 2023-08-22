using Apps.Jira.Dtos;

namespace Apps.Jira.Models.Responses;

public class AttachmentsResponse
{
    public IEnumerable<AttachmentDto> Attachments { get; set; }
}