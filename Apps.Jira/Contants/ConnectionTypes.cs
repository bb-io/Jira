namespace Apps.Jira.Contants;

public static class ConnectionTypes
{
    public const string OAuth2 = "OAuth2";
    public const string OAuth2CustomApp = "OAuth2 (custom app)";
    public const string ServiceAccount = "Service account";

    public static readonly string[] SupportedConnectionTypes = [OAuth2, OAuth2CustomApp, ServiceAccount];
}
