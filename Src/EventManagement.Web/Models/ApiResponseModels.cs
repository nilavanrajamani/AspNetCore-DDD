namespace EventManagement.Web.Models;

/// <summary>
/// Represents the standardized response structure from DDD.Services.Api
/// </summary>
public class DddApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}

/// <summary>
/// Internal response wrapper for EventManagement.Web services
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}