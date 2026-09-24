using Apps.Confluence.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Confluence.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>
    {
        new()
        {
            Name = ConnectionTypes.OAuth2,
            DisplayName = "OAuth2",
            AuthenticationType = ConnectionAuthenticationType.OAuth2,
            ConnectionUsage = ConnectionUsage.Actions,
            ConnectionProperties = new List<ConnectionProperty>()
        },
        new()
        {
            Name = ConnectionTypes.OAuth2CustomApp,
            DisplayName = "OAuth2 (custom app)",
            AuthenticationType = ConnectionAuthenticationType.OAuth2,
            ConnectionProperties = 
            [
                new(CredNames.ClientId) { DisplayName = "Client ID" },
                new(CredNames.ClientSecret) { DisplayName = "Client secret", Sensitive = true },
                new(CredNames.CustomScopes) { DisplayName = "Scopes" }
            ]
        },
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        var token = values.First(v => v.Key == CredNames.AccessToken);
        yield return new AuthenticationCredentialsProvider(CredNames.AccessToken, token.Value);
            
        var connectionType = values[nameof(ConnectionPropertyGroup)] switch
        {
            var ct when ConnectionTypes.SupportedConnectionTypes.Contains(ct) => ct,
            _ => throw new Exception($"Unknown connection type: {values[nameof(ConnectionPropertyGroup)]}")
        };
        yield return new AuthenticationCredentialsProvider(CredNames.ConnectionType, connectionType);
            
        var customKeys = new[]
        {
            CredNames.ConfluenceId,
            CredNames.ClientId, 
            CredNames.ClientSecret, 
            CredNames.CustomScopes 
        };

        foreach (var key in customKeys)
        {
            if (values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                yield return new AuthenticationCredentialsProvider(key, value);
        }
    }
}