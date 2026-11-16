using Apps.Confluence.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using RestSharp;

namespace Apps.Confluence.Api;

public class ApiRequest(string resource, Method method, IEnumerable<AuthenticationCredentialsProvider> creds) 
    : BlackBirdRestRequest(resource, method, creds)
{
    protected override void AddAuth(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        string connectionType = creds.Get(CredNames.ConnectionType).Value;

        string token = connectionType switch
        {
            ConnectionTypes.OAuth2 or ConnectionTypes.OAuth2CustomApp => creds.Get(CredNames.AccessToken).Value,
            ConnectionTypes.ServiceAccountApiToken => creds.Get(CredNames.ServiceAccountApiToken).Value,
            _ => throw new PluginMisconfigurationException($"This connection type is not supported: {connectionType}")
        };

        this.AddHeader("Authorization", $"Bearer {token}");
    }
}