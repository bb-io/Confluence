using Apps.Confluence.Api;
using Apps.Confluence.Invocables;
using Apps.Confluence.Models.Identifiers;
using Apps.Confluence.Models.Responses.Content;
using Apps.Confluence.Models.Responses.Pages;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;
using System.Text;

namespace Apps.Confluence.Actions;

[ActionList("Pages")]

public class PageActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : AppInvocable(invocationContext)
{
    [Action("Download page (storage format)",
    Description = "Downloads a Confluence page using the v2 API and storage format, exported as HTML file.")]
    public async Task<GetContentAsHtmlResponse> DownloadPageStorageAsync([ActionParameter] ContentIdentifier request)
    {
        var endpoint = $"/api/v2/pages/{request.ContentId}";

        var apiRequest = new ApiRequest(endpoint, Method.Get, Creds)
            .AddParameter("body-format", "storage", ParameterType.QueryString);

        var response = await Client.ExecuteWithErrorHandling<PageResponse>(apiRequest);

        if (response == null)
            throw new PluginApplicationException($"Page with ID {request.ContentId} not found.");

        if (string.IsNullOrWhiteSpace(response.Body))
            throw new PluginApplicationException($"Page {request.ContentId} does not contain body content.");

        var html = response.Body;

        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(html));
        fileStream.Position = 0;

        var fileRef = await fileManagementClient.UploadAsync(
            fileStream,
            "text/html",
            $"page-{request.ContentId}-storage.html"
        );

        return new()
        {
            File = fileRef
        };
    }

}
