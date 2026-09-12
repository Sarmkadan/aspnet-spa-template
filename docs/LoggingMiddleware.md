# LoggingMiddleware

Middleware component that logs basic information about incoming HTTP requests and outgoing HTTP responses, including the request method, path, authenticated user name, response status code, and elapsed processing time.

## API

### `public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)`

Constructor that initializes the logging middleware with the next request delegate and a logger.

- **Parameters**:
  - `next` – The `RequestDelegate` representing the next middleware in the pipeline.
  - `logger` – The logger used to record request, response, warning, and exception messages.
- **Exceptions**:
  - Throws `ArgumentNullException` if `next` or `logger` is `null`.

---

### `public async Task InvokeAsync(HttpContext context)`

Invokes the middleware pipeline while recording request and response details for the current HTTP request.

- **Parameters**:
  - `context` – The `HttpContext` for the current HTTP request.
- **Return value**: A `Task` representing the asynchronous operation.
- **Behavior**:
  - Starts a stopwatch and logs the request method, path, and authenticated user name. If no user name is available, the user is logged as `Anonymous`.
  - Replaces the response body stream with a temporary in-memory stream before invoking the next middleware.
  - After the next middleware completes, stops the stopwatch and logs the request method, path, response status code, and elapsed time in milliseconds.
  - Logs an additional warning when the response status code is `400` or greater.
  - Copies the buffered response to the original response body stream so it can be sent to the client.
  - If downstream processing throws an exception, stops the stopwatch, logs the exception and elapsed time, and rethrows the original exception.
  - Restores the original response body stream in all cases.

## Usage

### Basic Setup in ASP.NET Core Pipeline

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<LoggingMiddleware>();

app.MapControllers();
app.Run();
```

## Notes

- **Pipeline order**: Place the middleware before the components whose execution should be measured and logged.
- **Response buffering**: Responses are held in memory until downstream middleware completes, which can increase memory usage for large responses and prevent streaming responses from being sent incrementally.
- **Error handling**: Exceptions are logged and rethrown so later exception-handling behavior remains unchanged.
- **Response status warnings**: Both client errors and server errors produce a warning in addition to the standard response information log.
