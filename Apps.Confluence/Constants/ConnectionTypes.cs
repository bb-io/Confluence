namespace Apps.Confluence.Constants;

public static class ConnectionTypes
{
    public const string OAuth2 = "OAuth2";
    public const string OAuth2CustomApp = "OAuth2 (custom app)";
    public const string ServiceAccountApiToken = "Service account API token";

    public static readonly string[] SupportedConnectionTypes = [OAuth2, OAuth2CustomApp, ServiceAccountApiToken];
}