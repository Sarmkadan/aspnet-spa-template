# RequestResponseLoggingMiddleware

Middleware component that records structured HTTP request and response information, including the request method, path, response status code, elapsed time, and correlation ID. Logging behavior is controlled by `LoggingMiddlewareOptions`, and sensitive query-string values and headers are redacted.

## API

### `public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger, IOptions<LoggingMiddlewareOptions> options)`

Constructor that initializes the request and response logging middleware.

- **Parameters**:
  - `next` – The `RequestDelegate` representing the next middleware in the pipeline.
  - `logger` – The logger used to emit structured request, response, slow-request, and server-error messages.
  - `options` – The configured `LoggingMiddlewareOptions` values.
- **Exceptions**:
  - Throws `ArgumentNullException` if `next`, `logger`, or `options` is `null`.

---

### `public async Task InvokeAsync(HttpContext context)`

Invokes the middleware pipeline and logs the current HTTP request and response when logging is enabled and the request path is not excluded.

- **Parameters**:
  - `context` – The `HttpContext` for the current HTTP request.
- **Return value**: A `Task` representing the asynchronous operation.
- **Behavior**:
  - Immediately invokes the next middleware without logging when `Enabled` is `false` or the request path is excluded.
  - Logs the request before invoking the next middleware. `Minimal` verbosity logs only the method and path; other levels also include the query string with sensitive values redacted. Request headers are included for `Detailed` verbosity or when `LogRequestHeaders` is enabled, with sensitive headers redacted.
  - Temporarily buffers the response body while measuring the duration of the downstream pipeline.
  - In a `finally` block, logs the response status, duration, and correlation ID, warns when the duration exceeds `SlowRequestThresholdMs`, and logs an error for status codes of 500 or greater.
  - Copies the buffered response to the original response stream and restores that stream, including when downstream middleware throws.

---

### `public List<string> ExcludedPaths`

Gets or sets additional path fragments for which request and response logging is skipped.

- **Default value**: Empty list
- **Remarks**: The configured entries are combined with the default exclusions `/health`, `/metrics`, `/swagger`, and `/favicon`. Matching is case-insensitive and uses `Contains`, so an entry excludes any request path containing that text, not only an exact path or prefix. When a path is excluded, the middleware invokes the next delegate without buffering or logging the request or response.

## Usage

### Configuration in `appsettings.json`

```json
{
  "RequestLogging": {
    "Enabled": true,
    "VerbosityLevel": "Standard",
    "SlowRequestThresholdMs": 1000,
    "ExcludedPaths": [
      "/health",
      "/internal/status"
    ]
  }
}
```

## Notes

- **Sensitive Data**: Query-string parameters named `password`, `token`, `apikey`, `secret`, `creditcard`, `ssn`, or `authorization` are replaced with `[REDACTED]`. Request headers with those exact names are also redacted, using case-insensitive comparison.
- **Response Buffering**: Non-excluded responses are buffered in memory before being copied to the original response stream.
- **Path Matching**: Excluded paths use case-insensitive substring matching. Choose entries carefully to avoid excluding unrelated paths that contain the same text.
- **Logging Outcomes**: Requests slower than `SlowRequestThresholdMs` produce a warning, and responses with a status code of 500 or greater produce an error log.
