namespace Apps.Jira.Dtos
{
    public class TransitionDto
    {
        public string Name { get; set; }

        public string Id { get; set; }
    }

    public class TransitionsResponseWrapper
    {
        public IEnumerable<TransitionDto> Transitions { get; set; }
    }
}
