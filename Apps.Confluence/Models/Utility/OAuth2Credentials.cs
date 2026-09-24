using Apps.Confluence.Constants;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Confluence.Models.Utility;

public class OAuth2Credentials
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Scopes { get; set; }

    private OAuth2Credentials(string clientId, string clientSecret, string scopes)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
        Scopes = scopes;
    }
    
    public static OAuth2Credentials Create(Dictionary<string, string> values)
    {
        var connectionType = values[nameof(ConnectionPropertyGroup)] switch
        {
            var ct when ConnectionTypes.SupportedConnectionTypes.Contains(ct) => ct,
            _ => throw new Exception(
                $"Unknown connection type in OAuth2Credentials class: {values[nameof(ConnectionPropertyGroup)]}")
        };

        string clientId = string.Empty; 
        string clientSecret = string.Empty;
        string scopes = string.Empty;
        
        switch (connectionType)
        {
            case ConnectionTypes.OAuth2:
                clientId = ApplicationConstants.ClientId;
                clientSecret = ApplicationConstants.ClientSecret;
                scopes = ApplicationConstants.Scope;
                break;
            case ConnectionTypes.OAuth2CustomApp:
                clientId = values[CredNames.ClientId];
                clientSecret = values[CredNames.ClientSecret];
                scopes = values[CredNames.CustomScopes];
                break;
        }

        return new(clientId, clientSecret, scopes);
    }
}