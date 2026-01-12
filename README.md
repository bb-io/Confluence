# Blackbird.io Confluence

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Create, collaborate, and organize all your work in one place. Confluence is a team workspace where knowledge and collaboration meet. Dynamic pages give your team a place to create, capture, and collaborate on any project or idea.

## Before setting up

You must ensure that you have a Confluence account and access to it. Our app uses OAuth 2.0 for authentication, so you don't need to provide any API keys. You will be redirected to the Confluence login page to authorize the app. Note, this app leverages the [Confluence Cloud REST API](https://developer.atlassian.com/cloud/confluence/rest/v1/intro/#about) to provide a streamlined experience for managing your content.

## Connecting

1. Navigate to Apps, and identify the **Confluence** app. You can use search to find it.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'My organization'.
4. Click _Authorize connection_.
5. You will be redirected to the Confluence login page. Enter your credentials and authorize the app.
6. Confirm that the connection has appeared and the status is _Connected_.

![Connection](image/README/connection.png)

## Actions

### Content

- **Search content** - Returns a list of content that matches the search criteria.
- **Get content** - Returns a single content object specified by the content ID.
- **Get content as HTML** - Returns a single content HTML specified by the content ID.
- **Update content from HTML** - Updates a content from HTML file.
- **Create content from HTML** - Creates a content from HTML file.
- **Create content** - Creates a new content with specified data.
- **Delete content** - Deletes a content specified by the content ID.

### Pages

- **Download page** - Downloads a Confluence page as HTML file.

### Page proeprties

- **Search page properties** - Lists page properties.
- **Get page property** - Gets a specific page property.
- **Set page property** - Creates or updates a page property.

## Events

- **On content created** - Polling event. Triggered after specified time interval and returns new content.
- **On content updated** - Polling event. Triggered after specified time interval and returns updated content.

## Error handling

Our app returns errors in a structured format. Below are the typical error responses you might encounter:

`Status code: {Status code}, Message: {Message}`

- Status code: The HTTP status code associated with the error.
- Message: A brief message describing the error which we get from the Confluence API.

If you encounter an error, please try to resolve it using the information provided in the error message. If you are unable to resolve the error, please contact us for further assistance.

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
