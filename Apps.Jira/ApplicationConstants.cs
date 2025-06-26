namespace Apps.Jira;

public class ApplicationConstants
{
    public const string ClientId = "#{JIRA_CLIENT_ID}#";
    public const string ClientSecret = "#{JIRA_SECRET}#";
    public const string Scopes = "read:email-address:jira read:me read:sprint:jira-software read:jira-work write:jira-work read:jira-user offline_access read:board-scope:jira-software read:project:jira write:sprint:jira-software";
    public const string BlackbirdToken = "#{JIRA_BLACKBIRD_TOKEN}#";
}
