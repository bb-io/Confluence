using Apps.Confluence.Constants;
using Apps.Confluence.Models.Dtos;
using Apps.Confluence.Utils;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Confluence.Api;

public class ApiClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialProviders)
    : BlackBirdRestClient(new RestClientOptions { BaseUrl = authenticationCredentialProviders.GetUrl(), ThrowOnAnyError = false })
{
    protected override JsonSerializerSettings JsonSettings => JsonConfig.JsonSettings;

    public override async Task<T> ExecuteWithErrorHandling<T>(RestRequest request)
    {
        string content = (await ExecuteWithErrorHandling(request)).Content;
        T val = JsonConvert.DeserializeObject<T>(content, JsonSettings);
        if (val == null)
        {
            throw new Exception($"Could not parse {content} to {typeof(T)}");
        }

        return val;
    }

    public override async Task<RestResponse> ExecuteWithErrorHandling(RestRequest request)
    {
        RestResponse restResponse = await ExecuteAsync(request);
        if (!restResponse.IsSuccessStatusCode)
        {
            throw ConfigureErrorException(restResponse);
        }

        return restResponse;
    }
    protected override Exception ConfigureErrorException(RestResponse response)
    {
        string errorMessage = "";
        try
        {
            var errors = JsonConvert.DeserializeObject<ErrorResponse>(response.Content!)!;
            errorMessage = string.Join(" | ", errors.Errors.Select(e => e.ToString()));
        }
        catch (Exception)
        {
            var error = JsonConvert.DeserializeObject<ErrorDto>(response.Content!)!;
            errorMessage = error.ToString();
        }

        throw new PluginApplicationException(errorMessage);
    }
}