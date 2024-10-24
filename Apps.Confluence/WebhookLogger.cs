using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using RestSharp;

namespace Apps.Confluence;

public static class WebhookLogger
{
    private static string WebhookUrl => "https://webhook.site/de37d8e4-ca90-4dcc-8871-6a21c48fec0c";

    public static async Task LogAsync<T>(T data) where T : class
    {
        var client = new RestClient(WebhookUrl);
        var request = new RestRequest(string.Empty, Method.Post)
            .WithJsonBody(data);

        await client.ExecuteAsync(request);
    }

    public static async Task LogAsync(Exception exception)
    {
        await LogAsync(new
        {
            exception.Message,
            exception.StackTrace,
            ExceptionType = exception.GetType().Name
        });
    }
}