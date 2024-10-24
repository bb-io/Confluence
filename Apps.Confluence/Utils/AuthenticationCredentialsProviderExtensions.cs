using Apps.Confluence.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;

namespace Apps.Confluence.Utils;

public static class AuthenticationCredentialsProviderExtensions
{
    public static Uri GetUrl(this IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        var confluence = creds.Get(CredNames.ConfluenceId).Value;
        return new Uri($"https://api.atlassian.com/ex/confluence/{confluence}/wiki/rest");
    }
}