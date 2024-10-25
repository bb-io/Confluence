using Newtonsoft.Json;

namespace Apps.Confluence.Auth.OAuth2.Models;

public class OAuth2MetadataResponse
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("scopes")]
    public string[] Scopes { get; set; } = Array.Empty<string>();
    
    [JsonProperty("avatarUrl")]
    public string AvatarUrl { get; set; } = string.Empty;
}