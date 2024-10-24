using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.AspNetCore.WebUtilities;

namespace Apps.Confluence.Auth.OAuth2;

public class OAuth2AuthorizeService(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IOAuth2AuthorizeService
{
    public string GetAuthorizationUrl(Dictionary<string, string> values)
    {
        const string oauthUrl = "https://auth.atlassian.com/authorize";
        var parameters = new Dictionary<string, string>
        {
            { "audience", "api.atlassian.com" },
            { "client_id", ApplicationConstants.ClientId },
            { "scope", $"{ApplicationConstants.Scope} offline_access" },
            { "redirect_uri", InvocationContext.UriInfo.AuthorizationCodeRedirectUri.ToString() },
            { "state", values["state"] },
            { "response_type", "code" }
        };
        
        return QueryHelpers.AddQueryString(oauthUrl, parameters!);
    }
}