namespace Apps.Confluence.Models.Dtos;

public class ErrorDto
{
    public string Message { get; set; } = string.Empty;
    
    public string StatusCode { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Status code: {StatusCode}, Message: {Message}";
    }
}