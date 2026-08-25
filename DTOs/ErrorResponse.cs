#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Standard error response format.
/// </summary>
public sealed class ErrorResponse
{
    public string Message { get; set; public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";
 = string.Empty;
    public string? ErrorCode { get; set; public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";

    public Dictionary<string, List<string>>? Errors { get; set; public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";

    public string? TraceId { get; set; public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";

    public int StatusCode { get; set; public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";

    public DateTime Timestamp { get; set; public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";
 = DateTime.UtcNow;

    public ErrorResponse() { public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";


    public ErrorResponse(string message, int statusCode = 400)
    {
        Message = message;
        StatusCode = statusCode;
    public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";


    public ErrorResponse(string message, string errorCode, int statusCode = 400)
    {
        Message = message;
        ErrorCode = errorCode;
        StatusCode = statusCode;
    public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";


    public ErrorResponse(string message, Dictionary<string, List<string>> errors, int statusCode = 400)
    {
        Message = message;
        Errors = errors;
        StatusCode = statusCode;
    public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";

public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";


/// <summary>
/// Standard success response format.
/// </summary>
public sealed class SuccessResponse<T>
{
    public T? Data { get; set; public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";

    public bool Success { get; set; public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";
 = true;
    public string Message { get; set; public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";
 = "Operation completed successfully";

    public SuccessResponse() { public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";


    public SuccessResponse(T data)
    {
        Data = data;
    public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";


    public SuccessResponse(T data, string message)
    {
        Data = data;
        Message = message;
    public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";

public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";


/// <summary>
/// Paginated response wrapper.
/// </summary>
public sealed class PaginatedResponse<T>
{
    public List<T> Items { get; set; public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";
 = new();
    public int TotalCount { get; set; public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";

    public int PageNumber { get; set; public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";

    public int PageSize { get; set; public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";


    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
public override string ToString() => $"ErrorResponse {{ Message = {Message}, ErrorCode = {ErrorCode}, Errors = {Errors}, TraceId = {TraceId}, StatusCode = {StatusCode}, Timestamp = {Timestamp} }}";

