using System.Globalization;
using System.Net;
using Polly;
using Polly.Retry;
using RestSharp;

namespace Apps.Confluence.Api;

public static class ConfluencePollyPolicies
{
    private const int DefaultRetryCount = 2;
    private const double BaseDelaySeconds = 0.5;
    private const double MaxRetryDelaySeconds = 10;
    private const double JitterMin = 0.7;
    private const double JitterMax = 1.3;

    public static AsyncRetryPolicy<RestResponse> GetTransientRetryPolicy(
        int retryCount = DefaultRetryCount)
    {
        return Policy
            .HandleResult<RestResponse>(IsTransientResponse)
            .WaitAndRetryAsync<RestResponse>(
                retryCount,
                sleepDurationProvider: (attempt, outcome, _) =>
                    GetRetryDelay(attempt, outcome.Result),
                onRetryAsync: (_, _, _, _) => Task.CompletedTask);
    }

    private static bool IsTransientResponse(RestResponse response)
    {
        return response.StatusCode is 0
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;
    }

    private static TimeSpan GetRetryDelay(int attempt, RestResponse? response)
    {
        var retryAfter = TryGetRetryAfter(response);
        if (retryAfter.HasValue)
        {
            return TimeSpan.FromSeconds(retryAfter.Value);
        }

        var delaySeconds = Math.Min(
            BaseDelaySeconds * Math.Pow(2, attempt - 1),
            MaxRetryDelaySeconds);

        var jitter = Random.Shared.NextDouble() * (JitterMax - JitterMin) + JitterMin;
        return TimeSpan.FromSeconds(Math.Min(delaySeconds * jitter, MaxRetryDelaySeconds));
    }

    private static double? TryGetRetryAfter(RestResponse? response)
    {
        var retryAfterHeader = response?.Headers?
            .FirstOrDefault(header =>
                string.Equals(header.Name, "Retry-After", StringComparison.OrdinalIgnoreCase))
            ?.Value?.ToString();

        if (string.IsNullOrWhiteSpace(retryAfterHeader))
        {
            return null;
        }

        if (double.TryParse(
                retryAfterHeader,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var seconds))
        {
            return Math.Max(0, seconds);
        }

        if (DateTimeOffset.TryParse(
                retryAfterHeader,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var retryAt))
        {
            return Math.Max(0, (retryAt - DateTimeOffset.UtcNow).TotalSeconds);
        }

        return null;
    }
}
