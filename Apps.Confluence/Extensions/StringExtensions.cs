namespace Apps.Confluence.Extensions;

public static class StringExtensions
{
    public static string ToCqlString(this string input)
    {
        string escaped = input.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return $"\"{escaped}\"";
    }
}