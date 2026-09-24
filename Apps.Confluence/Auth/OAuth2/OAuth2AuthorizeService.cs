using Apps.Confluence.Models.Utility;
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
        string bridgeOauthUrl = $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/oauth";
        const string atlassianAuthorizeUrl = "https://auth.atlassian.com/authorize";
        
        var creds = OAuth2Credentials.Create(values);
        var parameters = new Dictionary<string, string>
        {
            { "audience", "api.atlassian.com" },
            { "client_id", creds.ClientId },
            { "scope", creds.Scopes },
            { "redirect_uri", $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/AuthorizationCode" },
            { "state", values["state"] },
            { "response_type", "code" },
            { "authorization_url", atlassianAuthorizeUrl},
            { "actual_redirect_uri", InvocationContext.UriInfo.AuthorizationCodeRedirectUri.ToString() },
        };

        return QueryHelpers.AddQueryString(bridgeOauthUrl, parameters!);
    }
}