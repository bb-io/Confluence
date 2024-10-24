using Apps.Confluence.Auth.OAuth2.Models;
using Apps.Confluence.Constants;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Confluence.Auth.OAuth2;

public class OAuth2TokenService(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IOAuth2TokenService
{
    private const string TokenUrl = "https://auth.atlassian.com/oauth/token";
    
    public bool IsRefreshToken(Dictionary<string, string> values)
    {
        var createdAt = int.Parse(values[CredNames.CreatedAt]);
        var expiresIn = int.Parse(values[CredNames.ExpiresIn]);
        
        return createdAt + expiresIn < DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public async Task<Dictionary<string, string>> RefreshToken(Dictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        var restRequest = new RestRequest(TokenUrl, Method.Post)
            .AddParameter("grant_type", "refresh_token")
            .AddParameter("client_id", ApplicationConstants.ClientId)
            .AddParameter("client_secret", ApplicationConstants.ClientSecret)
            .AddParameter("redirect_uri", InvocationContext.UriInfo.AuthorizationCodeRedirectUri.ToString())
            .AddParameter("refresh_token", values[CredNames.RefreshToken]);
        
        var client = new RestClient();
        var response = await client.ExecuteAsync(restRequest, cancellationToken);
        var tokenResponse = JsonConvert.DeserializeObject<OAuth2TokenResponse>(response.Content!)!;
        
        return new Dictionary<string, string>
        {
            { CredNames.AccessToken, tokenResponse.AccessToken },
            { CredNames.RefreshToken, tokenResponse.RefreshToken },
            { CredNames.ExpiresIn, tokenResponse.ExpiresIn.ToString() },
            { CredNames.CreatedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
        };
    }

    public async Task<Dictionary<string, string>> RequestToken(
        string state,
        string code,
        Dictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        var restRequest = new RestRequest(TokenUrl, Method.Post)
            .AddParameter("grant_type", "authorization_code")
            .AddParameter("client_id", ApplicationConstants.ClientId)
            .AddParameter("client_secret", ApplicationConstants.ClientSecret)
            .AddParameter("redirect_uri", InvocationContext.UriInfo.AuthorizationCodeRedirectUri.ToString())
            .AddParameter("code", code);

        var client = new RestClient();
        var response = await client.ExecuteAsync(restRequest, cancellationToken);
        var tokenResponse = JsonConvert.DeserializeObject<OAuth2TokenResponse>(response.Content!)!;
        
        var confluenceId = await GetConfluenceId(tokenResponse);
        
        await WebhookLogger.LogAsync(new
        {
            tokenResponse,
            confluenceId
        });
        
        return new Dictionary<string, string>
        {
            { CredNames.AccessToken, tokenResponse.AccessToken },
            { CredNames.RefreshToken, tokenResponse.RefreshToken },
            { CredNames.ExpiresIn, tokenResponse.ExpiresIn.ToString() },
            { CredNames.CreatedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            { CredNames.ConfluenceId, confluenceId }
        };
    }

    public Task RevokeToken(Dictionary<string, string> values)
    {
        throw new NotImplementedException();
    }

    private async Task<string> GetConfluenceId(OAuth2TokenResponse tokenResponse)
    {
        var metadataUrl = "https://api.atlassian.com/oauth/token/accessible-resources";
        var metadataRequest = new RestRequest(string.Empty)
            .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken}");
        
        var client = new RestClient(metadataUrl);
        var response = await client.ExecuteAsync(metadataRequest);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get Confluence ID. Response: {response.Content}");
        }
        
        var deserializeObject = JsonConvert.DeserializeObject<List<OAuth2MetadataResponse>>(response.Content!)
            ?? throw new Exception($"Failed to get Confluence ID. Response: {response.Content}");
        
        return deserializeObject.First().Id;
    }
}