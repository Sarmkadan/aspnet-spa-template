// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Exceptions;

namespace AspNetSpaTemplate.Utilities;

/// <summary>
/// Maps exceptions to appropriate HTTP status codes and error codes.
/// Centralizes error handling logic for consistency.
/// Prevents leaking internal error details to clients.
/// </summary>
public static class ErrorMappingHelper
{
    /// <summary>
    /// Maps exception to HTTP status code.
    /// </summary>
    public static int MapToStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            BusinessException => StatusCodes.Status422UnprocessableEntity,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            TimeoutException => StatusCodes.Status504GatewayTimeout,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    /// <summary>
    /// Maps exception to error code for client handling.
    /// </summary>
    public static string MapToErrorCode(Exception exception)
    {
        return exception switch
        {
            ValidationException ex => "VALIDATION_ERROR",
            NotFoundException => "NOT_FOUND",
            BusinessException => "BUSINESS_ERROR",
            UnauthorizedAccessException => "UNAUTHORIZED",
            InvalidOperationException => "INVALID_OPERATION",
            ArgumentException => "INVALID_ARGUMENT",
            TimeoutException => "REQUEST_TIMEOUT",
            _ => "INTERNAL_SERVER_ERROR"
        };
    }

    /// <summary>
    /// Maps exception to user-friendly error message.
    /// Hides internal implementation details.
    /// </summary>
    public static string MapToUserMessage(Exception exception)
    {
        return exception switch
        {
            ValidationException ex => ex.Message,
            NotFoundException => "The requested resource was not found",
            BusinessException ex => ex.Message,
            UnauthorizedAccessException => "You are not authorized to access this resource",
            InvalidOperationException => "The requested operation cannot be completed at this time",
            ArgumentException => "One or more required parameters are invalid",
            TimeoutException => "The request took too long to process. Please try again.",
            _ => "An error occurred while processing your request"
        };
    }

    /// <summary>
    /// Gets retry information for failed requests.
    /// Some errors are retryable (503, 408), others are not (4xx).
    /// </summary>
    public static (bool Retryable, int? RetryAfterSeconds) GetRetryInfo(Exception exception, int attempt = 1)
    {
        var statusCode = MapToStatusCode(exception);

        // Determine if retryable
        var retryable = statusCode switch
        {
            StatusCodes.Status408RequestTimeout => true,
            StatusCodes.Status429TooManyRequests => true,
            StatusCodes.Status503ServiceUnavailable => true,
            StatusCodes.Status504GatewayTimeout => true,
            _ when statusCode >= 500 => true,
            _ => false
        };

        // Calculate exponential backoff for retries
        int? retryAfter = null;
        if (retryable && attempt < 3)
        {
            retryAfter = (int)Math.Pow(2, attempt); // 2, 4, 8 seconds
        }

        return (retryable, retryAfter);
    }

    /// <summary>
    /// Determines if error should be logged at ERROR level vs WARNING/INFO.
    /// </summary>
    public static LogLevel GetLogLevel(Exception exception)
    {
        return exception switch
        {
            ValidationException => LogLevel.Warning,
            NotFoundException => LogLevel.Warning,
            BusinessException => LogLevel.Warning,
            TimeoutException => LogLevel.Warning,
            _ => LogLevel.Error
        };
    }

    /// <summary>
    /// Extracts detailed error information for logging.
    /// Includes inner exceptions and stack trace.
    /// </summary>
    public static ErrorDetails ExtractErrorDetails(Exception exception)
    {
        var details = new ErrorDetails
        {
            ExceptionType = exception.GetType().Name,
            Message = exception.Message,
            StackTrace = exception.StackTrace ?? "",
            Timestamp = DateTime.UtcNow
        };

        // Include inner exception details
        if (exception.InnerException != null)
        {
            details.InnerException = exception.InnerException.GetType().Name;
            details.InnerMessage = exception.InnerException.Message;
        }

        return details;
    }

    /// <summary>
    /// Checks if exception is transient (safe to retry).
    /// </summary>
    public static bool IsTransientError(Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            _ => false
        };
    }

    /// <summary>
    /// Checks if exception is critical (should trigger alerts).
    /// </summary>
    public static bool IsCriticalError(Exception exception)
    {
        return exception switch
        {
            OutOfMemoryException => true,
            StackOverflowException => true,
            Exception ex when ex.HResult == unchecked((int)0x80131509) => true, // AppDomainUnloadedException
            _ => false
        };
    }
}

/// <summary>
/// Error details for structured error logging.
/// </summary>
public class ErrorDetails
{
    public string ExceptionType { get; set; } = "";
    public string Message { get; set; } = "";
    public string StackTrace { get; set; } = "";
    public string? InnerException { get; set; }
    public string? InnerMessage { get; set; }
    public DateTime Timestamp { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Type: {ExceptionType}");
        sb.AppendLine($"Message: {Message}");
        if (!string.IsNullOrEmpty(InnerException))
        {
            sb.AppendLine($"Inner: {InnerException} - {InnerMessage}");
        }
        return sb.ToString();
    }
}
