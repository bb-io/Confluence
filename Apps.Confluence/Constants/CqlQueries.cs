namespace Apps.Confluence.Constants;

public static class CqlQueries
{
    public static readonly string EditableContentCql = $"type IN ({string.Join(",", ContentTypes.EditableContentTypes)})";
}