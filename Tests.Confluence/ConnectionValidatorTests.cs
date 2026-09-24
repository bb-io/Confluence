using Apps.Confluence.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;
using Tests.Confluence.Base;

namespace Tests.Confluence;

[TestClass]
public class ConnectionValidatorTests : TestBase
{
    private readonly ConnectionValidator _validator = new();

    [TestMethod]
    public async Task ValidateConnection_WithValidCredentials_ReturnsValid()
    {
        var result = await _validator.ValidateConnection(Creds, CancellationToken.None);

        Console.WriteLine(result.Message);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task ValidateConnection_WithInvalidCredentials_ReturnsInvalid()
    {
        var invalidCreds = Creds.Select(x => new AuthenticationCredentialsProvider(x.KeyName, x.Value + "_incorrect"));
        var result = await _validator.ValidateConnection(invalidCreds, CancellationToken.None);

        Console.WriteLine(result.Message);
        Assert.IsFalse(result.IsValid);
    }
}