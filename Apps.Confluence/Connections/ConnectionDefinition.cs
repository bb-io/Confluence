using Apps.Confluence.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Exceptions;

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
        new()
        {
            Name = ConnectionTypes.ServiceAccountApiToken,
            DisplayName = "Service account API token",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties =
            [
                new(CredNames.ConfluenceId)
                {
                    DisplayName = "Cloud ID",
                    Description = "Get it from https://<your-site>.atlassian.net/_edge/tenant_info"
                },
                new(CredNames.ServiceAccountApiToken) { DisplayName = "API token", Sensitive = true }
            ]
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(Dictionary<string, string> values)
    {
        var connectionType = values[nameof(ConnectionPropertyGroup)];
        if (!ConnectionTypes.SupportedConnectionTypes.Contains(connectionType))
            throw new PluginMisconfigurationException($"Unknown connection type: {connectionType}");
            
        yield return new AuthenticationCredentialsProvider(CredNames.ConnectionType, connectionType);
            
        var customKeys = new[]
        {
            CredNames.AccessToken,
            CredNames.ConfluenceId,
            CredNames.ClientId, 
            CredNames.ClientSecret, 
            CredNames.CustomScopes,
            CredNames.ServiceAccountApiToken
        };

        foreach (var key in customKeys)
        {
            if (values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                yield return new AuthenticationCredentialsProvider(key, value);
        }
    }
}