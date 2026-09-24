using System.Net;
using Apps.Confluence.Api;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using RestSharp;

namespace Apps.Confluence.Connections;

public class ConnectionValidator : IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        try
        {
            var creds = authenticationCredentialsProviders.ToArray();
            var client = new ApiClient(creds);
            var request = new ApiRequest("/rest/api/user/current", Method.Get, creds);

            var response = await client.ExecuteAsync(request, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            {
                return new ConnectionValidationResponse
                {
                    IsValid = false,
                    Message = "Invalid credentials"
                };
            }

            return new ConnectionValidationResponse
            {
                IsValid = true,
                Message = "Success"
            };
        }
        catch (Exception ex)
        {
            return new ConnectionValidationResponse
            {
                IsValid = false,
                Message = $"Connection validation failed: {ex.Message}"
            };
        }
    }
}