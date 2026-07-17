# CorrelationIdMiddleware

The `CorrelationIdMiddleware` class provides a centralized mechanism for tracking and propagating a correlation identifier across HTTP requests and downstream service calls. It automatically captures the client IP address, user agent, and timestamp for each incoming request, and exposes both instance-level and static APIs to read and set the correlation ID. This middleware is typically registered in the ASP.NET Core pipeline to enable end-to-end request tracing and logging correlation.

## API

### `public CorrelationIdMiddleware`

Initializes a new instance of the `CorrelationIdMiddleware` class. The constructor is invoked by the ASP.NET Core dependency injection system and typically receives a `RequestDelegate` representing the next middleware in the pipeline, along with optional `CorrelationIdOptions` for configuration (e.g., header name, include in response).

### `public async Task InvokeAsync`

Processes the current HTTP request by extracting or generating a correlation ID, recording the client IP, user agent, and timestamp, and storing these values for the duration of the request. The method accepts an `HttpContext` parameter and returns a `Task`. After setting the correlation context, it invokes the next middleware delegate. This method does not throw exceptions under normal operation; any exceptions from downstream middleware propagate as usual.

### `public static string GetCorrelationId()`

Returns the correlation ID for the current asynchronous context. The correlation ID is stored in an `AsyncLocal` and is therefore scoped to the current logical operation.  
**Returns:** A `string` containing the correlation ID, or `null` if no correlation ID has been set.

### `public static void SetCorrelationId(string correlationId)`

Sets the correlation ID for the current asynchronous context. This method is typically called by the middleware during request processing, but can also be used manually to propagate a correlation ID across async boundaries.  
**Parameters:**  
- `correlationId`: The correlation identifier to assign.  
**Throws:** `ArgumentNullException` if `correlationId` is `null`.

### `public string CorrelationId { get; }`

Gets the correlation ID that was assigned to the current request during middleware execution. This property is populated only after `InvokeAsync` has run for the request.  
**Value:** A `string` representing the correlation ID, or `null` if not yet set.

### `public string ClientIp { get; }`

Gets the client IP address of the current request, as determined from the `HttpContext.Connection.RemoteIpAddress`.  
**Value:** A `string` containing the IP address, or `null` if unavailable.

### `public string UserAgent { get; }`

Gets the `User-Agent` header value from the current request.  
**Value:** A `string` containing the user agent, or `null` if the header is missing.

### `public DateTime Timestamp { get; }`

Gets the UTC timestamp when the middleware began processing the current request.  
**Value:** A `DateTime` in UTC.

### `public static CorrelationContext FromHttpContext(HttpContext httpContext)`

Retrieves the `CorrelationContext` object that was attached to the specified `HttpContext` by the middleware. The returned object contains the `CorrelationId`, `ClientIp`, `UserAgent`, and `Timestamp` for that request.  
**Parameters:**  
- `httpContext`: The `HttpContext` from which to extract the correlation context.  
**Returns:** A `CorrelationContext` instance, or `null` if the middleware has not processed the request.  
**Throws:** `ArgumentNullException` if `httpContext` is `null`.

## Usage

The following examples demonstrate typical registration and consumption of the middleware.

### Example 1: Registering the middleware and accessing properties in a controller

```csharp
// In Program.cs or Startup.cs
app.UseMiddleware<CorrelationIdMiddleware>();

// In a controller action
[ApiController]
[Route("api/[controller]")]
public class SampleController : ControllerBase
{
    private readonly CorrelationIdMiddleware _middleware;

    public SampleController(CorrelationIdMiddleware middleware)
    {
        _middleware = middleware;
    }

    [HttpGet]
    public IActionResult Get()
    {
        // Access per-request correlation data
        var correlationId = _middleware.CorrelationId;
        var clientIp = _middleware.ClientIp;
        var userAgent = _middleware.UserAgent;
        var timestamp = _middleware.Timestamp;

        return Ok(new
        {
            CorrelationId = correlationId,
            ClientIp = clientIp,
            UserAgent = userAgent,
            Timestamp = timestamp
        });
    }
}
```

### Example 2: Using static methods and `FromHttpContext` in a background service

```csharp
public class LoggingService
{
    public void LogRequest(HttpContext context)
    {
        // Retrieve correlation context from the HttpContext
        var ctx = CorrelationIdMiddleware.FromHttpContext(context);
        if (ctx != null)
        {
            Console.WriteLine($"[{ctx.Timestamp:O}] {ctx.CorrelationId} - {ctx.ClientIp} - {ctx.UserAgent}");
        }

        // Propagate correlation ID to an async operation
        var correlationId = CorrelationIdMiddleware.GetCorrelationId();
        _ = Task.Run(async () =>
        {
            CorrelationIdMiddleware.SetCorrelationId(correlationId);
            // ... perform background work with the same correlation ID
        });
    }
}
```

## Notes

- **Thread safety:** The static `GetCorrelationId` and `SetCorrelationId` methods use `AsyncLocal<string>` and are safe to call from concurrent asynchronous operations. However, they are scoped to the current logical execution context; care must be taken when spawning new tasks or threads to ensure the correlation ID is propagated explicitly.
- **Instance properties (`CorrelationId`, `ClientIp`, `UserAgent`, `Timestamp`):** These are set during `InvokeAsync` and are valid only for the duration of the request. Accessing them outside of a request context (e.g., after the response has been sent) may return stale or `null` values. The middleware is typically registered as a singleton, so the same instance is reused across requests; the properties are overwritten per request.
- **`FromHttpContext` return value:** Returns `null` if the middleware has not yet processed the request or if the `HttpContext` does not contain a correlation context item. Always check for `null` before accessing the returned object.
- **Missing headers:** If the client IP or user agent cannot be determined (e.g., behind a proxy without proper forwarding headers), the corresponding properties will be `null`. Configure forwarding headers (e.g., `X-Forwarded-For`) as needed.
- **Exception behavior:** The middleware does not throw exceptions for missing or invalid correlation IDs. If `SetCorrelationId` is called with `null`, an `ArgumentNullException` is thrown.
