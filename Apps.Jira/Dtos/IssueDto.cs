namespace Apps.Jira.Dtos
{
    public class IssueDto
    {
        public string Statuscategorychangedate { get; set; }
        public Issuetype Issuetype { get; set; }
        public Project Project { get; set; }
        public int Workratio { get; set; }
        public Watches Watches { get; set; }
        public string LastViewed { get; set; }
        public string Created { get; set; }
        public Priority Priority { get; set; }
        public Assignee Assignee { get; set; }
        public string Updated { get; set; }
        public Status Status { get; set; }
        public Description Description { get; set; }
        public string Summary { get; set; }
        public Creator Creator { get; set; }
        public Reporter Reporter { get; set; }
        public Aggregateprogress Aggregateprogress { get; set; }
        public ProgressObj Progress { get; set; }
    }

    public class Aggregateprogress
    {
        public int Progress { get; set; }
        public int Total { get; set; }
    }

    public class Assignee
    {
        public string Self { get; set; }
        public string AccountId { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public bool Active { get; set; }
        public string TimeZone { get; set; }
        public string AccountType { get; set; }
    }

    public class Creator
    {
        public string Self { get; set; }
        public string AccountId { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public bool Active { get; set; }
        public string TimeZone { get; set; }
        public string AccountType { get; set; }
    }

    public class Description
    {
        public int Version { get; set; }
        public string Type { get; set; }
        public List<ContentObj> Content { get; set; }
    }

    public class ContentObj
    {
        public string Type { get; set; }
        public List<ContentData> Content { get; set; }
    }

    public class ContentData
    {
        public string Type { get; set; }
        public string Text { get; set; }
    }

    public class Issuetype
    {
        public string Self { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public bool Subtask { get; set; }
        public int AvatarId { get; set; }
        public string EntityId { get; set; }
        public int HierarchyLevel { get; set; }
    }

    public class NonEditableReason
    {
        public string Reason { get; set; }
        public string Message { get; set; }
    }

    public class Priority
    {
        public string Self { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public class ProgressObj
    {
        public int Progress { get; set; }
        public int Total { get; set; }
    }

    public class Project
    {
        public string Self { get; set; }
        public string Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string ProjectTypeKey { get; set; }
        public bool Simplified { get; set; }
    }

    public class Reporter
    {
        public string Self { get; set; }
        public string AccountId { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public bool Active { get; set; }
        public string TimeZone { get; set; }
        public string AccountType { get; set; }
    }

    public class Status
    {
        public string Self { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public StatusCategory StatusCategory { get; set; }
    }

    public class StatusCategory
    {
        public string Self { get; set; }
        public int Id { get; set; }
        public string Key { get; set; }
        public string ColorName { get; set; }
        public string Name { get; set; }
    }

    public class Watches
    {
        public string Self { get; set; }
        public int WatchCount { get; set; }
        public bool IsWatching { get; set; }
    }

}
