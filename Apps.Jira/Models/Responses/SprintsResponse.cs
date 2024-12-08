

using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Responses
{
    public class SprintsResponse
    {
        public string Message { get; set; }
        public List<SprintDto> Sprints { get; set; }
    }

    public class SprintDto
    {
        [Display("Sprint ID")]
        public int Id { get; set; }

        [Display("Name")]
        public string Name { get; set; }

        [Display("State")]
        public string State { get; set; }

        [Display("Start date")]
        public DateTime StartDate { get; set; }

        [Display("End date")]
        public DateTime EndDate { get; set; }

        public SprintDto(Sprint sprint)
        {
            Id = sprint.Id;
            Name = sprint.Name;
            State = sprint.State;
            StartDate = sprint.StartDate;
            EndDate = sprint.EndDate;
        }
    }
    public class SprintsWrapper
    {
        public int MaxResults { get; set; }
        public int StartAt { get; set; }
        public int Total { get; set; }
        public bool IsLast { get; set; }
        public List<Sprint> Values { get; set; }
    }

    public class Sprint
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
