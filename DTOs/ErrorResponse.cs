// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Standard error response format.
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public Dictionary<string, List<string>>? Errors { get; set; }
    public string? TraceId { get; set; }
    public int StatusCode { get; set; }

    public ErrorResponse() { }

    public ErrorResponse(string message, int statusCode = 400)
    {
        Message = message;
        StatusCode = statusCode;
    }

    public ErrorResponse(string message, string errorCode, int statusCode = 400)
    {
        Message = message;
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public ErrorResponse(string message, Dictionary<string, List<string>> errors, int statusCode = 400)
    {
        Message = message;
        Errors = errors;
        StatusCode = statusCode;
    }
}

/// <summary>
/// Standard success response format.
/// </summary>
public class SuccessResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "Operation completed successfully";

    public SuccessResponse() { }

    public SuccessResponse(T data)
    {
        Data = data;
    }

    public SuccessResponse(T data, string message)
    {
        Data = data;
        Message = message;
    }
}

/// <summary>
/// Paginated response wrapper.
/// </summary>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
