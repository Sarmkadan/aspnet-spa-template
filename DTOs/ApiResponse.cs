// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Standard wrapper for all API responses.
/// Provides consistent structure for success and error responses.
/// Includes metadata for debugging and client-side handling.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Creates successful response with data.
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Success"
        };
    }

    /// <summary>
    /// Creates error response.
    /// </summary>
    public static ApiResponse<T> Error(string message, string errorCode, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            TraceId = traceId
        };
    }

    /// <summary>
    /// Adds metadata to response (helpful for analytics, debugging).
    /// </summary>
    public ApiResponse<T> WithMetadata(string key, object value)
    {
        Metadata ??= new Dictionary<string, object>();
        Metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Converts response to different type (data projection).
    /// </summary>
    public ApiResponse<TNew> Map<TNew>(Func<T?, TNew> mapper)
    {
        return new ApiResponse<TNew>
        {
            Success = Success,
            Data = Success ? mapper(Data) : default,
            Message = Message,
            ErrorCode = ErrorCode,
            TraceId = TraceId,
            Timestamp = Timestamp,
            Metadata = Metadata
        };
    }
}

/// <summary>
/// Standard wrapper for non-generic responses.
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse Ok(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message ?? "Success"
        };
    }

    public static ApiResponse Error(string message, string errorCode, string? traceId = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            TraceId = traceId
        };
    }
}

/// <summary>
/// Response for list/pagination endpoints.
/// </summary>
public class ApiListResponse<T>
{
    public bool Success { get; set; }
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiListResponse<T> Ok(List<T> items, int pageNumber, int pageSize, int totalCount, string? message = null)
    {
        return new ApiListResponse<T>
        {
            Success = true,
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Message = message ?? "Success"
        };
    }

    public static ApiListResponse<T> Empty(string? message = null)
    {
        return new ApiListResponse<T>
        {
            Success = true,
            Items = new List<T>(),
            PageNumber = 1,
            PageSize = 0,
            TotalCount = 0,
            Message = message ?? "No items found"
        };
    }
}

/// <summary>
/// Response for batch operations.
/// </summary>
public class ApiBatchResponse
{
    public bool Success { get; set; }
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BatchOperationError> Errors { get; set; } = new();
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public class BatchOperationError
    {
        public int ItemIndex { get; set; }
        public string Message { get; set; } = "";
        public string? ErrorCode { get; set; }
    }
}

/// <summary>
/// Response for long-running operations (async tasks).
/// </summary>
public class AsyncOperationResponse
{
    public string OperationId { get; set; } = "";
    public string Status { get; set; } = "Started"; // Started, InProgress, Completed, Failed
    public int? PercentComplete { get; set; }
    public string? ResultUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public TimeSpan? ElapsedTime => CompletedAt.HasValue ? CompletedAt - CreatedAt : null;
}
