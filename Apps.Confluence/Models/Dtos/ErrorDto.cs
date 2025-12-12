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

public class ErrorResponse
{
    public List<ApiError> Errors { get; set; } = new();
}

public class ApiError
{
    public int Status { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Detail { get; set; }

    public override string ToString()
    {
        var detail = string.IsNullOrEmpty(Detail) ? "" : $" Details: {Detail}";
        return $"Status: {Status}, Code: {Code}, Message: {Title}.{detail}";
    }
}