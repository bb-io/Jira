using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos
{
    public class TransitionDto
    {
        public string Name { get; set; }

        [Display("Transition ID")]
        public string Id { get; set; }
    }

    public class TransitionsResponseWrapper
    {
        public IEnumerable<TransitionDto> Transitions { get; set; }
    }
}
