using Apps.Confluence.Constants;
using Apps.Confluence.Models.Dtos;
using Apps.Confluence.Models.Responses.Utility;
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

    public async Task<List<T>> Paginate<T>(RestRequest request, int? maxPages = null)
    {
        var allResults = new List<T>();
        var start = 0;
        int pageSize = 100;
        int pages = maxPages ?? 1000;

        for (var page = 0; page < pages; page++)
        {
            request
                .AddOrUpdateParameter("start", start, ParameterType.QueryString)
                .AddOrUpdateParameter("limit", pageSize, ParameterType.QueryString);

            var response = await ExecuteWithErrorHandling<BasePaginationResponse<T>>(request);
            if (response.Results.Count == 0)
                break;

            allResults.AddRange(response.Results);

            if (string.IsNullOrEmpty(response.Links?.Next))
                break;

            var nextStart = ExtractStartFromNextLink(response.Links.Next);
            if (nextStart <= start)
                break;

            start = nextStart;
        }

        return allResults;
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
    
    private static int ExtractStartFromNextLink(string nextLink)
    {
        var uri = new Uri("https://dummy" + nextLink);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        return int.TryParse(query["start"], out var startValue)
            ? startValue
            : 0;
    }
}
