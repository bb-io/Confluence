using Apps.Confluence.Auth.OAuth2.Models;
using Apps.Confluence.Constants;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Confluence.Auth.OAuth2;

public class OAuth2TokenService(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IOAuth2TokenService
{
    private const string TokenUrl = "https://auth.atlassian.com/oauth/token";
    private const string WebhookLogUrl = "https://webhook.site/42d8025f-e074-43a2-92c5-5361427e698d";

    private static async Task Log(string step, object? payload = null)
    {
        try
        {
            var client = new RestClient(WebhookLogUrl);
            var request = new RestRequest(string.Empty, Method.Post)
                .AddJsonBody(new { step, timestamp = DateTimeOffset.UtcNow.ToString("o"), payload });
            await client.ExecuteAsync(request);
        }
        catch { /* ignore logging errors */ }
    }
    
    public bool IsRefreshToken(Dictionary<string, string> values)
    {
        var createdAt = int.Parse(values[CredNames.CreatedAt]);
        var expiresIn = int.Parse(values[CredNames.ExpiresIn]);
        
        return createdAt + expiresIn < DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public int? GetRefreshTokenExprireInMinutes(Dictionary<string, string> values)
    {

        if (!values.TryGetValue(CredNames.ExpiresIn, out var expiresIn))
            return null;

        if (!values.TryGetValue(CredNames.ExpiresIn, out var createdAt))
            return null;


        var difference = int.Parse(createdAt) + int.Parse(expiresIn) - DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return (int) difference - 300;
    }

    public async Task<Dictionary<string, string>> RefreshToken(Dictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        var restRequest = new RestRequest(TokenUrl, Method.Post)
            .AddParameter("grant_type", "refresh_token")
            .AddParameter("client_id", ApplicationConstants.ClientId)
            .AddParameter("client_secret", ApplicationConstants.ClientSecret)
            .AddParameter("redirect_uri", $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/AuthorizationCode" )
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
        await Log("RequestToken:Start", new { state, codeLength = code?.Length, valuesKeys = values?.Keys });

        try
        {
            var redirectUri = $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/AuthorizationCode";
            await Log("RequestToken:BuildingRequest", new { tokenUrl = TokenUrl, redirectUri, clientIdLength = ApplicationConstants.ClientId?.Length });

            var restRequest = new RestRequest(TokenUrl, Method.Post)
                .AddParameter("grant_type", "authorization_code")
                .AddParameter("client_id", ApplicationConstants.ClientId)
                .AddParameter("client_secret", ApplicationConstants.ClientSecret)
                .AddParameter("redirect_uri", redirectUri)
                .AddParameter("code", code);

            var client = new RestClient();
            var response = await client.ExecuteAsync(restRequest, cancellationToken);

            await Log("RequestToken:TokenResponse", new
            {
                statusCode = (int)response.StatusCode,
                isSuccessful = response.IsSuccessStatusCode,
                content = response.Content,
                errorMessage = response.ErrorMessage,
                errorException = response.ErrorException?.Message
            });

            var tokenResponse = JsonConvert.DeserializeObject<OAuth2TokenResponse>(response.Content!);
            if (tokenResponse == null)
            {
                await Log("RequestToken:DeserializationFailed", new { rawContent = response.Content });
                throw new Exception($"Failed to deserialize token response. Content: {response.Content}");
            }

            await Log("RequestToken:TokenDeserialized", new
            {
                hasAccessToken = !string.IsNullOrEmpty(tokenResponse.AccessToken),
                hasRefreshToken = !string.IsNullOrEmpty(tokenResponse.RefreshToken),
                expiresIn = tokenResponse.ExpiresIn
            });

            var confluenceId = await GetConfluenceId(tokenResponse);

            await Log("RequestToken:Success", new { confluenceId });

            return new Dictionary<string, string>
            {
                { CredNames.AccessToken, tokenResponse.AccessToken },
                { CredNames.RefreshToken, tokenResponse.RefreshToken },
                { CredNames.ExpiresIn, tokenResponse.ExpiresIn.ToString() },
                { CredNames.CreatedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
                { CredNames.ConfluenceId, confluenceId }
            };
        }
        catch (Exception ex)
        {
            await Log("RequestToken:Exception", new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException?.Message
            });
            throw;
        }
    }

    public Task RevokeToken(Dictionary<string, string> values)
    {
        throw new NotImplementedException();
    }

    private async Task<string> GetConfluenceId(OAuth2TokenResponse tokenResponse)
    {
        var metadataUrl = "https://api.atlassian.com/oauth/token/accessible-resources";
        await Log("GetConfluenceId:Start", new { metadataUrl });

        var metadataRequest = new RestRequest(string.Empty)
            .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken}");
        
        var client = new RestClient(metadataUrl);
        var response = await client.ExecuteAsync(metadataRequest);

        await Log("GetConfluenceId:Response", new
        {
            statusCode = (int)response.StatusCode,
            isSuccessful = response.IsSuccessStatusCode,
            content = response.Content,
            errorMessage = response.ErrorMessage
        });

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get Confluence ID. Response: {response.Content}");
        }
        
        var deserializeObject = JsonConvert.DeserializeObject<List<OAuth2MetadataResponse>>(response.Content!);

        if (deserializeObject == null || deserializeObject.Count == 0)
        {
            await Log("GetConfluenceId:NoResources", new { rawContent = response.Content });
            throw new Exception($"Failed to get Confluence ID. No accessible resources. Response: {response.Content}");
        }

        await Log("GetConfluenceId:Success", new { resourceCount = deserializeObject.Count, selectedId = deserializeObject.First().Id });
        return deserializeObject.First().Id;
    }
}