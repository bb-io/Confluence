using System.Net;
using Apps.Confluence.Api;
using Apps.Confluence.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using RestSharp;

namespace Tests.Confluence;

[TestClass]
public class ApiClientTests
{
    private readonly TestApiClient _client = new(
    [
        new AuthenticationCredentialsProvider(
            AuthenticationCredentialsRequestLocation.None,
            CredNames.ConfluenceId,
            "test-confluence-id")
    ]);

    [TestMethod]
    public void ConfigureErrorException_ParsesErrorsResponse()
    {
        var response = CreateResponse(
            HttpStatusCode.BadRequest,
            """{"errors":[{"status":400,"code":"INVALID_REQUEST","title":"Invalid request","detail":"Invalid CQL"}]}""");

        var exception = _client.GetErrorException(response);

        Assert.IsInstanceOfType<PluginApplicationException>(exception);
        StringAssert.Contains(exception.Message, "HTTP 400 BadRequest");
        StringAssert.Contains(exception.Message, "INVALID_REQUEST");
        StringAssert.Contains(exception.Message, "Invalid CQL");
    }

    [TestMethod]
    public void ConfigureErrorException_ParsesLegacyErrorResponse()
    {
        var response = CreateResponse(
            HttpStatusCode.Unauthorized,
            """{"statusCode":"401","message":"Authentication required"}""");

        var exception = _client.GetErrorException(response);

        Assert.IsInstanceOfType<PluginApplicationException>(exception);
        StringAssert.Contains(exception.Message, "HTTP 401 Unauthorized");
        StringAssert.Contains(exception.Message, "Authentication required");
    }

    [TestMethod]
    public void ConfigureErrorException_HandlesHtmlResponse()
    {
        var response = CreateResponse(
            HttpStatusCode.ServiceUnavailable,
            "<html><body>Service unavailable</body></html>");

        var exception = _client.GetErrorException(response);

        Assert.IsInstanceOfType<PluginApplicationException>(exception);
        StringAssert.Contains(exception.Message, "HTTP 503 ServiceUnavailable");
        StringAssert.Contains(exception.Message, "non-JSON or unsupported error response");
        StringAssert.Contains(exception.Message, "Response body: <html><body>Service unavailable</body></html>");
    }

    [TestMethod]
    public void ConfigureErrorException_TruncatesLongUnsupportedResponse()
    {
        var response = CreateResponse(
            HttpStatusCode.ServiceUnavailable,
            $"<html>{new string('a', 600)}BODY_END</html>");

        var exception = _client.GetErrorException(response);

        StringAssert.Contains(exception.Message, "Response body: <html>");
        StringAssert.EndsWith(exception.Message, "...");
        Assert.IsFalse(exception.Message.Contains("BODY_END"));
    }

    [TestMethod]
    public void ConfigureErrorException_HandlesEmptyResponse()
    {
        var response = CreateResponse(HttpStatusCode.BadGateway, null);

        var exception = _client.GetErrorException(response);

        Assert.IsInstanceOfType<PluginApplicationException>(exception);
        StringAssert.Contains(exception.Message, "HTTP 502 BadGateway");
        StringAssert.Contains(exception.Message, "empty error response");
    }

    [TestMethod]
    public async Task TransientRetryPolicy_RetriesTransientResponse()
    {
        var policy = ConfluencePollyPolicies.GetTransientRetryPolicy(retryCount: 1);
        var attempts = 0;

        var response = await policy.ExecuteAsync(() =>
        {
            attempts++;
            return Task.FromResult(CreateResponse(
                attempts == 1 ? HttpStatusCode.InternalServerError : HttpStatusCode.OK,
                null));
        });

        Assert.AreEqual(2, attempts);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task TransientRetryPolicy_DoesNotRetryNonTransientResponse()
    {
        var policy = ConfluencePollyPolicies.GetTransientRetryPolicy(retryCount: 1);
        var attempts = 0;

        var response = await policy.ExecuteAsync(() =>
        {
            attempts++;
            return Task.FromResult(CreateResponse(HttpStatusCode.BadRequest, null));
        });

        Assert.AreEqual(1, attempts);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static RestResponse CreateResponse(HttpStatusCode statusCode, string? content)
    {
        return new RestResponse
        {
            StatusCode = statusCode,
            Content = content
        };
    }

    private sealed class TestApiClient(IEnumerable<AuthenticationCredentialsProvider> credentials)
        : ApiClient(credentials)
    {
        public Exception GetErrorException(RestResponse response)
        {
            return ConfigureErrorException(response);
        }
    }
}
