using Apps.Confluence.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using RestSharp;

namespace Apps.Confluence.Api;

public class ApiRequest(string resource, Method method, IEnumerable<AuthenticationCredentialsProvider> creds) 
    : BlackBirdRestRequest(resource, method, creds)
{
    protected override void AddAuth(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        var apiKey = creds.Get(CredNames.AccessToken).Value;
        this.AddHeader("Authorization", $"Bearer {apiKey}");
    }
}