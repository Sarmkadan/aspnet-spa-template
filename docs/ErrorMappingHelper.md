# ErrorMappingHelper

`ErrorMappingHelper` is a utility class that provides standardized error mapping and classification for ASP.NET Core applications. It converts exceptions and error codes into HTTP status codes, log levels, user-friendly messages, and retry policies, enabling consistent error handling across the application.

## API

### `public static int MapToStatusCode(Exception exception)`
Maps a given exception to an appropriate HTTP status code.

- **Parameters**
  - `exception`: The exception to map.
- **Return value**
  - An HTTP status code (e.g., 400, 404, 500).
- **Throws**
  - `ArgumentNullException`: If `exception` is `null`.

### `public static string MapToErrorCode(Exception exception)`
Maps a given exception to a standardized error code string.

- **Parameters**
  - `exception`: The exception to map.
- **Return value**
  - A string representing the error code (e.g., `"INVALID_INPUT"`, `"RESOURCE_NOT_FOUND"`).
- **Throws**
  - `ArgumentNullException`: If `exception` is `null`.

### `public static string MapToUserMessage(Exception exception)`
Maps a given exception to a user-friendly error message.

- **Parameters**
  - `exception`: The exception to map.
- **Return value**
  - A string intended for display to end users.
- **Throws**
  - `ArgumentNullException`: If `exception` is `null`.

### `public static (bool Retryable, int? RetryAfterSeconds) GetRetryInfo(Exception exception)`
Determines whether an exception represents a retryable error and, if so, the recommended delay before retrying.

- **Parameters**
  - `exception`: The exception to evaluate.
- **Return value**
  - A tuple where:
    - `Retryable`: `true` if the error is transient and may succeed on retry; otherwise `false`.
    - `RetryAfterSeconds`: Optional delay in seconds before retrying (e.g., `30` for a 30-second backoff).
- **Throws**
  - `ArgumentNullException`: If `exception` is `null`.

### `public static LogLevel GetLogLevel(Exception exception)`
Maps an exception to a suitable logging level for diagnostic purposes.

- **Parameters**
  - `exception`: The exception to classify.
- **Return value**
  - A `LogLevel` (e.g., `Error`, `Warning`, `Information`).
- **Throws**
  - `ArgumentNullException`: If `exception` is `null`.

### `public static ErrorDetails ExtractErrorDetails(Exception exception)`
Extracts a structured set of error details from an exception.

- **Parameters**
  - `exception`: The exception to extract details from.
- **Return value**
  - An `ErrorDetails` object containing:
    - `ExceptionType`
    - `Message`
    - `StackTrace`
    - `InnerException`
    - `InnerMessage`
    - `Timestamp`
- **Throws**
  - `ArgumentNullException`: If `exception` is `null`.

### `public static bool IsTransientError(Exception exception)`
Determines whether an exception represents a transient error that may resolve on retry.

- **Parameters**
  - `exception`: The exception to evaluate.
- **Return value**
  - `true` if the error is transient; otherwise `false`.
- **Throws**
  - `ArgumentNullException`: If `exception` is `null`.

### `public static bool IsCriticalError(Exception exception)`
Determines whether an exception represents a critical error that should halt processing or trigger immediate alerts.

- **Parameters**
  - `exception`: The exception to evaluate.
- **Return value**
  - `true` if the error is critical; otherwise `false`.
- **Throws**
  - `ArgumentNullException`: If `exception` is `null`.

### `public string ExceptionType`
Gets the type name of the exception.

- **Type**: `string`
- **Access**: Read-only

### `public string Message`
Gets the exception message.

- **Type**: `string`
- **Access**: Read-only

### `public string StackTrace`
Gets the exception stack trace.

- **Type**: `string`
- **Access**: Read-only

### `public string? InnerException`
Gets the message of the inner exception, if present.

- **Type**: `string?`
- **Access**: Read-only

### `public string? InnerMessage`
Gets the message of the inner exception, if present.

- **Type**: `string?`
- **Access**: Read-only

### `public DateTime Timestamp`
Gets the timestamp when the error details were captured.

- **Type**: `DateTime`
- **Access**: Read-only

### `public override string ToString()`
Returns a formatted string representation of the error details.

- **Return value**
  - A string combining exception type, message, stack trace, and inner exception details.
- **Overrides**
  - `Object.ToString()`

## Usage

### Example 1: Mapping an exception to HTTP response components
```csharp
try
{
    // Simulate a transient error
    throw new TimeoutException("Database operation timed out.");
}
catch (Exception ex)
{
    var statusCode = ErrorMappingHelper.MapToStatusCode(ex);
    var errorCode = ErrorMappingHelper.MapToErrorCode(ex);
    var userMessage = ErrorMappingHelper.MapToUserMessage(ex);
    var (retryable, retryAfter) = ErrorMappingHelper.GetRetryInfo(ex);

    // Use in controller
    return StatusCode(statusCode, new
    {
        ErrorCode = errorCode,
        Message = userMessage,
        Retryable = retryable,
        RetryAfterSeconds = retryAfter
    });
}
```

### Example 2: Logging and retry policy decision
```csharp
var ex = new InvalidOperationException("Invalid input data.");
var logLevel = ErrorMappingHelper.GetLogLevel(ex);
var isTransient = ErrorMappingHelper.IsTransientError(ex);

_logger.Log(logLevel, ex, "Processing failed: {ErrorCode}", ErrorMappingHelper.MapToErrorCode(ex));

if (isTransient)
{
    // Schedule retry with exponential backoff
    _retryPolicy.ExecuteAsync(() => ProcessRequestAsync());
}
```

## Notes

- **Null handling**: All static methods validate input and throw `ArgumentNullException` if `exception` is `null`. Ensure proper exception handling before calling these methods.
- **Thread safety**: The class is stateless and thread-safe. All methods are static and do not maintain internal state.
- **Error classification**: Transient and critical classifications are based on exception type and message patterns. Custom exceptions should derive from known transient types (e.g., `TimeoutException`, `SqlException`) or implement internal logic to override default behavior.
- **Stack trace inclusion**: The `StackTrace` property is populated only when the exception is captured with full details (e.g., not truncated by middleware). Avoid logging stack traces in production unless in debug or diagnostic contexts.
- **Inner exception handling**: When extracting details via `ExtractErrorDetails`, inner exceptions are traversed up to one level deep. Nested inner exceptions beyond the first are not included in `InnerException` or `InnerMessage`.
