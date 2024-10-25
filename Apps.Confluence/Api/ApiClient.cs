using Apps.Confluence.Constants;
using Apps.Confluence.Models.Dtos;
using Apps.Confluence.Utils;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Confluence.Api;

public class ApiClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialProviders)
    : BlackBirdRestClient(new RestClientOptions { BaseUrl = authenticationCredentialProviders.GetUrl(), ThrowOnAnyError = false })
{
    protected override JsonSerializerSettings JsonSettings => JsonConfig.JsonSettings;
    
    protected override Exception ConfigureErrorException(RestResponse response)
    {
        try
        {
            var error = JsonConvert.DeserializeObject<ErrorDto>(response.Content!)!;
            return new Exception(error.ToString());
        }
        catch (Exception)
        {
            return new Exception($"Status code: {response.StatusCode}, Message: {response.Content}");
        }
    }
}