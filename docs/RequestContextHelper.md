# RequestContextHelper

`RequestContextHelper` is a static utility class that extracts and normalizes common HTTP request metadata from the current `HttpContext`. It provides strongly typed access to correlation IDs, user identity claims, client network information, content negotiation hints, and a factory method for building a lightweight `RequestContext` value object. All members rely on `IHttpContextAccessor` being registered in the dependency injection container.

## API

### GetCorrelationId
```csharp
public static string GetCorrelationId
```
Returns the correlation ID for the current request. The value is read from the `X-Correlation-Id` header if present; otherwise a new GUID is generated. This property never returns `null`.

### GetUserId
```csharp
public static int GetUserId
```
Extracts the authenticated user's ID from the `NameIdentifier` claim of the current `ClaimsPrincipal`. Returns `0` if the user is not authenticated or the claim is missing. Does not throw.

### GetApiToken
```csharp
public static string? GetApiToken
```
Reads the `Authorization` header and strips the `Bearer ` prefix if present. Returns `null` when the header is absent or empty.

### GetClientIpAddress
```csharp
public static string GetClientIpAddress
```
Determines the client's remote IP address by checking the `X-Forwarded-For` header first, then falling back to `HttpContext.Connection.RemoteIpAddress`. Returns `"0.0.0.0"` when neither source is available.

### GetUserAgent
```csharp
public static string GetUserAgent
```
Returns the value of the `User-Agent` request header. Returns `string.Empty` if the header is not present.

### GetReferer
```csharp
public static string? GetReferer
```
Returns the value of the `Referer` request header, or `null` if the header is absent.

### CreateContext
```csharp
public static RequestContext CreateContext
```
Constructs and returns a new `RequestContext` instance populated with the current request's correlation ID, user ID, client IP, user agent, HTTP method, request path, and a UTC timestamp. Throws `InvalidOperationException` if `IHttpContextAccessor` or its `HttpContext` is null.

### IsAjaxRequest
```csharp
public static bool IsAjaxRequest
```
Returns `true` when the `X-Requested-With` header equals `"XMLHttpRequest"` (case-insensitive). Returns `false` otherwise.

### IsMobileRequest
```csharp
public static bool IsMobileRequest
```
Returns `true` when the `User-Agent` header contains any of the substrings `"Mobi"`, `"Android"`, or `"iPhone"` (case-sensitive). Returns `false` otherwise.

### GetContentType
```csharp
public static string GetContentType
```
Returns the `Content-Type` header value. Returns `string.Empty` when the header is absent.

### WantsJson
```csharp
public static bool WantsJson
```
Returns `true` when the `Accept` header contains `"application/json"` (case-insensitive). Returns `false` otherwise.

### GetQueryParameter
```csharp
public static string? GetQueryParameter(string key)
```
Returns the value of the query string parameter identified by `key`, or `null` if the parameter is not present.

| Parameter | Type     | Description                    |
|-----------|----------|--------------------------------|
| key       | `string` | The query parameter name.      |

Throws `ArgumentNullException` when `key` is null.

### GetHeaderValue
```csharp
public static string? GetHeaderValue(string key)
```
Returns the value of the request header identified by `key`, or `null` if the header is absent. Throws `ArgumentNullException` when `key` is null.

### RequestContext Properties

The `RequestContext` type returned by `CreateContext` exposes the following read-only properties:

| Property      | Type       | Description                                |
|---------------|------------|--------------------------------------------|
| CorrelationId | `string`   | The correlation ID for the request.        |
| UserId        | `int`      | The authenticated user ID.                 |
| ClientIp      | `string`   | The client IP address.                     |
| UserAgent     | `string`   | The User-Agent header value.               |
| Method        | `string`   | The HTTP method (GET, POST, etc.).         |
| Path          | `string`   | The request path.                          |
| Timestamp     | `DateTime` | The UTC time when the context was created. |

## Usage

### Example 1: Logging enriched request information

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestContext = RequestContextHelper.CreateContext;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = requestContext.CorrelationId,
            ["ClientIp"] = requestContext.ClientIp,
            ["UserId"] = requestContext.UserId
        }))
        {
            _logger.LogInformation(
                "{Method} {Path} started at {Timestamp}",
                requestContext.Method,
                requestContext.Path,
                requestContext.Timestamp);

            await _next(context);
        }
    }
}
```

### Example 2: Conditional response formatting based on request characteristics

```csharp
[ApiController]
[Route("api/data")]
public class DataController : ControllerBase
{
    [HttpGet]
    public IActionResult GetData()
    {
        var correlationId = RequestContextHelper.GetCorrelationId;
        var clientIp = RequestContextHelper.GetClientIpAddress;

        if (RequestContextHelper.IsMobileRequest)
        {
            return Ok(new
            {
                correlationId,
                clientIp,
                message = "Mobile-optimized response",
                data = GetCompactData()
            });
        }

        if (RequestContextHelper.WantsJson)
        {
            return Ok(new
            {
                correlationId,
                clientIp,
                data = GetFullData()
            });
        }

        return Content(GetHtmlRepresentation(), "text/html");
    }
}
```

## Notes

- All static members internally resolve `IHttpContextAccessor` from the ambient service provider. If the accessor has not been registered or `HttpContext` is null (e.g., when called outside an HTTP request scope), `CreateContext` throws `InvalidOperationException`. Other members may return default values or throw `NullReferenceException` under the same conditions.
- `GetClientIpAddress` trusts the `X-Forwarded-For` header. In environments where this header can be spoofed, additional validation or middleware-level sanitization should be applied.
- `IsMobileRequest` uses simple substring matching and may produce false positives or negatives for uncommon user-agent strings. It is suitable for coarse device detection, not for analytics-grade accuracy.
- `GetUserId` returns `0` for unauthenticated users. Callers should treat `0` as the anonymous sentinel value rather than a valid user ID.
- The `RequestContext` type is a plain data object. Its `Timestamp` is captured at the moment `CreateContext` is called, not at the start of the HTTP request.
- This class is not thread-safe in the sense that it reads from the current `HttpContext`, which is inherently scoped to a single request. Concurrent calls from different requests operate on separate `HttpContext` instances and do not interfere with each other.
