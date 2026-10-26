namespace Apps.Confluence.Constants;

public static class CqlQueries
{
    public static readonly string EditableContentCql = $"type IN ({string.Join(",", ContentTypes.EditableContentTypes)})";

    public const string ContentListExpand = "ancestors,space,history,history.lastUpdated";
}