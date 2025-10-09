using Apps.Confluence.Invocables;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Confluence.Actions;

[ActionList("Debug")]
public class DebugActions(InvocationContext invocationContext) : AppInvocable(invocationContext)
{
    [Action("Debug", Description = "Debug action.")]
    public List<AuthenticationCredentialsProvider> GetCredentialProviders()
    {
        return Creds.ToList();
    }
}