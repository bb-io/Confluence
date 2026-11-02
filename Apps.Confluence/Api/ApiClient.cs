using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
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

    private const string ForbiddenAccessHint =
        "Check the connected account's permissions, data security policies and connection scopes.";

    private static readonly Regex JavaExceptionNamePrefix = new(@"^.*Exception:\s*", RegexOptions.Compiled);

    private static readonly AsyncRetryPolicy<RestResponse> RetryPolicy =
        ConfluencePollyPolicies.GetTransientRetryPolicy();

    protected override JsonSerializerSettings JsonSettings => JsonConfig.JsonSettings;

    public async Task<List<T>> Paginate<T>(RestRequest request, int? maxPages = null)
    {
        var allResults = new List<T>();
        int pageSize = 100;
        int pages = maxPages ?? 1000;

        request.AddOrUpdateParameter("limit", pageSize, ParameterType.QueryString);
        
        for (var page = 0; page < pages; page++)
        {
            var response = await ExecuteWithErrorHandling<BasePaginationResponse<T>>(request);
            allResults.AddRange(response.Results);

            if (response.Results.Count == 0 || string.IsNullOrEmpty(response.Links?.Next))
                break;
            
            if (!ApplyNextLink(request, response.Links.Next))
                break;
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

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return CreateForbiddenException(content);

        string? errorMessage = null;

        if (IsJson(content))
        {
            var errors = DeserializeOrDefault<ErrorResponse>(content);
            if (errors?.Errors?.Count > 0)
            {
                errorMessage = string.Join(" | ", errors.Errors.Select(error => error.ToString()));
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                var error = DeserializeOrDefault<ErrorDto>(content);
                if (!string.IsNullOrWhiteSpace(error?.Message))
                {
                    errorMessage = error.ToString();
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

    private PluginMisconfigurationException CreateForbiddenException(string? content)
    {
        var reason = GetForbiddenReason(content);
        var reasonSentence = string.IsNullOrWhiteSpace(reason) ? string.Empty : $"{reason.TrimEnd('.', ' ')}. ";

        return new PluginMisconfigurationException(
            $"Confluence denied access (HTTP 403 Forbidden). {reasonSentence}{ForbiddenAccessHint}");
    }

    private string? GetForbiddenReason(string? content)
    {
        if (!IsJson(content))
            return null;

        var legacyMessage = DeserializeOrDefault<ErrorDto>(content)?.Message;
        if (!string.IsNullOrWhiteSpace(legacyMessage))
            return JavaExceptionNamePrefix.Replace(legacyMessage, string.Empty).Trim();

        var apiError = DeserializeOrDefault<ErrorResponse>(content)?.Errors?.FirstOrDefault();
        return apiError?.Detail ?? apiError?.Title;
    }

    private static bool IsJson([NotNullWhen(true)] string? content)
    {
        return !string.IsNullOrEmpty(content) && (content.StartsWith('{') || content.StartsWith('['));
    }

    private T? DeserializeOrDefault<T>(string content) where T : class
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(content, JsonSettings);
        }
        catch (JsonException)
        {
            return null;
        }
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

    private static bool ApplyNextLink(RestRequest request, string nextLink)
    {
        var queryStart = nextLink.IndexOf('?');
        if (queryStart < 0)
            return false;

        var query = HttpUtility.ParseQueryString(nextLink[queryStart..]);
        foreach (var key in query.AllKeys.OfType<string>())
            request.AddOrUpdateParameter(key, query[key], ParameterType.QueryString);

        return query.Count > 0;
    }
}
