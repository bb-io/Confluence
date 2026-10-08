using Apps.Confluence.Constants;
using Apps.Confluence.Models.Dtos;
using Apps.Confluence.Utils;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using Polly.Retry;
using RestSharp;

namespace Apps.Confluence.Api;

public class ApiClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialProviders)
    : BlackBirdRestClient(new RestClientOptions { BaseUrl = authenticationCredentialProviders.GetUrl(), ThrowOnAnyError = false })
{
    private const int MaxErrorContentExcerptLength = 500;

    private static readonly AsyncRetryPolicy<RestResponse> RetryPolicy =
        ConfluencePollyPolicies.GetTransientRetryPolicy();

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
        var restResponse = request.Method == Method.Get
            ? await RetryPolicy.ExecuteAsync(() => ExecuteAsync(request))
            : await ExecuteAsync(request);

        if (!restResponse.IsSuccessStatusCode)
        {
            throw ConfigureErrorException(restResponse);
        }

        return restResponse;
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var content = response.Content?.Trim();
        string? errorMessage = null;

        if (!string.IsNullOrEmpty(content) &&
            (content.StartsWith('{') || content.StartsWith('[')))
        {
            try
            {
                var errors = JsonConvert.DeserializeObject<ErrorResponse>(content, JsonSettings);
                if (errors?.Errors?.Count > 0)
                {
                    errorMessage = string.Join(" | ", errors.Errors.Select(error => error.ToString()));
                }
            }
            catch (JsonException)
            {
                // The response may use another Confluence error schema.
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                try
                {
                    var error = JsonConvert.DeserializeObject<ErrorDto>(content, JsonSettings);
                    if (!string.IsNullOrWhiteSpace(error?.Message))
                    {
                        errorMessage = error.ToString();
                    }
                }
                catch (JsonException)
                {
                    // Fall back to a generic message below.
                }
            }
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                errorMessage = !string.IsNullOrWhiteSpace(response.ErrorMessage)
                    ? response.ErrorMessage
                    : "Confluence returned an empty error response.";
            }
            else
            {
                var bodyMessage = "Confluence returned a non-JSON or unsupported error response. " +
                    $"Response body: {GetContentExcerpt(content)}";

                errorMessage = !string.IsNullOrWhiteSpace(response.ErrorMessage)
                    ? $"{response.ErrorMessage}. {bodyMessage}"
                    : bodyMessage;
            }
        }

        var status = response.StatusCode == 0
            ? "No HTTP status"
            : $"HTTP {(int)response.StatusCode} {response.StatusCode}";

        return new PluginApplicationException($"Confluence request failed. {status}. {errorMessage}");
    }

    private static string GetContentExcerpt(string content)
    {
        var excerptLength = Math.Min(content.Length, MaxErrorContentExcerptLength);
        var excerpt = string.Concat(content
            .Take(excerptLength)
            .Select(character => char.IsControl(character) ? ' ' : character));

        return content.Length > MaxErrorContentExcerptLength
            ? $"{excerpt}..."
            : excerpt;
    }
}
