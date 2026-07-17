# RateLimitingMiddleware

Middleware component that enforces rate limiting policies on incoming HTTP requests based on configurable constraints such as requests per minute, requests per hour, exempt paths, IP address, or API key.

## API

### `public RateLimitingMiddleware`

Constructor that initializes the rate limiting middleware with default configuration values.

- **Parameters**: None
- **Remarks**: Default values are `RequestsPerMinute = 60`, `RequestsPerHour = 1000`, `ExemptPaths = new List<string>()`, `EnableByIpAddress = true`, and `EnableByApiKey = false`.

---

### `public async Task InvokeAsync(HttpContext context, RequestDelegate next)`

Invokes the middleware pipeline to apply rate limiting logic to the current HTTP request.

- **Parameters**:
  - `context` – The `HttpContext` for the current HTTP request.
  - `next` – The `RequestDelegate` representing the next middleware in the pipeline.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**:
  - Throws `ArgumentNullException` if `context` or `next` is `null`.
  - Throws `InvalidOperationException` if rate limit tracking fails due to storage issues.

---

### `public int RequestsPerMinute`

Gets or sets the maximum number of requests allowed per minute per tracked identifier (IP address or API key).

- **Default value**: `60`
- **Remarks**: Must be a positive integer. Changing this value after middleware initialization will affect subsequent requests.

---

### `public int RequestsPerHour`

Gets or sets the maximum number of requests allowed per hour per tracked identifier (IP address or API key).

- **Default value**: `1000`
- **Remarks**: Must be a positive integer. Changing this value after middleware initialization will affect subsequent requests.

---
### `public List<string> ExemptPaths`

Gets or sets a list of path prefixes that are exempt from rate limiting checks.

- **Default value**: Empty list
- **Remarks**: Paths are compared using `StartsWith` against the request path. Modifications to this list after middleware initialization are thread-safe.

---
### `public bool EnableByIpAddress`

Gets or sets a value indicating whether rate limiting should be enforced based on client IP address.

- **Default value**: `true`
- **Remarks**: When `false`, IP-based tracking is disabled. This setting does not affect API key-based tracking if `EnableByApiKey` is `true`.

---
### `public bool EnableByApiKey`

Gets or sets a value indicating whether rate limiting should be enforced based on API key (assumed to be present in a header named `X-API-Key`).

- **Default value**: `false`
- **Remarks**: When `false`, API key-based tracking is disabled. Requires the presence of the `X-API-Key` header to function.

## Usage

### Basic Setup in ASP.NET Core Pipeline

```csharp
// In Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args);

// Configure rate limiting
builder.Services.AddRateLimiting(options =>
{
    options.RequestsPerMinute = 100;
    options.RequestsPerHour = 1200;
    options.ExemptPaths.Add("/health");
    options.EnableByApiKey = true;
});

var app = builder.Build();

app.UseRateLimiting(); // Registers the RateLimitingMiddleware

app.MapControllers();
app.Run();
```

### Custom Configuration with Dependency Injection

```csharp
// Define a configuration service
public class RateLimitConfig
{
    public int RequestsPerMinute { get; set; } = 60;
    public int RequestsPerHour { get; set; } = 1000;
    public List<string> ExemptPaths { get; } = new();
    public bool EnableByIpAddress { get; set; } = true;
    public bool EnableByApiKey { get; set; } = false;
}

// Register and configure in DI
builder.Services.Configure<RateLimitConfig>(options =>
{
    options.RequestsPerMinute = 120;
    options.ExemptPaths.AddRange(new[] { "/status", "/ping" });
});

// Apply middleware with injected config
app.UseRateLimiting();
```

## Notes

- **Thread Safety**: The `ExemptPaths` list is not thread-safe for concurrent modifications. If the list must be updated at runtime, synchronize access using a `lock` or use a thread-safe collection such as `ConcurrentBag<string>`.
- **Storage**: Rate counters are stored in-memory using `MemoryCache`. In distributed environments, consider replacing with a distributed cache (e.g., Redis) to ensure consistency across instances.
- **Header Sensitivity**: API key tracking relies on the `X-API-Key` header. Ensure HTTPS is enforced to prevent header leakage.
- **Exempt Path Matching**: Paths are matched using `StartsWith`, so `/api/v1/users` will match `/api/v1`. Use trailing slashes consistently to avoid unintended exemptions.
- **Rate Enforcement**: If both `EnableByIpAddress` and `EnableByApiKey` are `true`, the stricter limit (lower of the two) is applied.
