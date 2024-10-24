using Apps.Confluence.Invocables;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Confluence.Actions;

[ActionList]
public class DebugActions(InvocationContext invocationContext) : AppInvocable(invocationContext)
{
    [Action("[DEBUG] Get authentication credential providers", Description = "Get authentication credentials")]
    public List<AuthenticationCredentialsProvider> GetCredentialProviders()
    {
        return Creds.ToList();
    }
}