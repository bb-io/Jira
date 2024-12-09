using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos
{
    public class SprintMoveIssueDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class SprintsPaginationDto
    {
        public int MaxResults { get; set; }
        public int StartAt { get; set; }
        public bool IsLast { get; set; }
        public List<SprintMoveIssueDto> Values { get; set; }
    }

    public class BoardIdentifier
    {
        [Display("Board ID")]
        public string BoardId { get; set; }
    }
}
