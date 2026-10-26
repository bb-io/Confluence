using Apps.Confluence.Api;
using Apps.Confluence.Models.Entities.Space;
using Blackbird.Applications.Sdk.Common.Authentication;
using RestSharp;

namespace Apps.Confluence.Extensions;

public static class ApiClientExtensions
{
    public static async Task<string> GetSpaceKey(
        this ApiClient client, 
        IEnumerable<AuthenticationCredentialsProvider> creds, 
        string spaceId)
    {
        var request = new ApiRequest($"/api/v2/spaces/{spaceId}", Method.Get, creds);
        var space = await client.ExecuteWithErrorHandling<SpaceEntity>(request);
        return space.Key;
    }
}