using Newtonsoft.Json;

namespace Apps.Confluence.Auth.OAuth2.Models;

public class OAuth2TokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = default!;
    
    [JsonProperty("token_type")]
    public string TokenType { get; set; } = default!;
    
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; } = default!;
    
    [JsonProperty("created_at")]
    public int CreatedAt { get; set; }
}