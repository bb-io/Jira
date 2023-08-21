using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos
{
    public class TransitionDto
    {
        [Display("Transition ID")]
        public string Id { get; set; }
        
        [Display("Transition name")]
        public string Name { get; set; }
    }
}
