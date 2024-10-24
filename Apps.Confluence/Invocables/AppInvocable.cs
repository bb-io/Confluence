using Apps.Confluence.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Confluence.Invocables;

public class AppInvocable : BaseInvocable
{
    protected AuthenticationCredentialsProvider[] Creds =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected ApiClient Client { get; }

    protected AppInvocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Client = new ApiClient(Creds);
    }
}