

using Apps.Jira.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Jira.Models.Requests
{
    public class GetSprintByDateRequest
    {
        [Display("Board ID")]
        [DataSource(typeof(BoardDataSourceHandler))]
        public string BoardId { get; set; }

        [Display("Relevant date")]
        public DateTime Date { get; set; }

    }
}
