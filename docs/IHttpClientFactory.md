# IHttpClientFactory

`IHttpClientFactory` is a service abstraction for creating and managing named or typed `HttpClient` instances with centralized configuration, automatic handler lifetime management, and built-in resiliency patterns. It prevents socket exhaustion by recycling underlying connection handlers and provides convenience extension methods for common JSON-based HTTP operations and safe request execution with structured error handling.

## API

### DefaultHttpClientFactory
The concrete, default implementation of `IHttpClientFactory` provided by the framework. It manages a cache of `HttpMessageHandler` instances keyed by client name, ensuring handlers are periodically recycled to avoid DNS staleness and socket exhaustion. This type is typically registered in the dependency injection container and resolved via the interface.

### HttpClient GetClient(string name)
Creates or retrieves a named `HttpClient` instance configured according to the settings registered under the specified logical name. The returned client uses a managed handler that is automatically recycled.

- **Parameters**: `name` — the logical name of the client as registered during service configuration (e.g., in `AddHttpClient` calls).
- **Returns**: A configured `HttpClient` instance ready for use.
- **Throws**: `ArgumentNullException` if `name` is null; `InvalidOperationException` if no client has been registered with the given name.

### HttpClient GetClient(Type type)
Creates or retrieves a typed `HttpClient` instance associated with the specified type. Typed clients are registered via `AddHttpClient<TClient>` and allow injecting a pre-configured `HttpClient` directly into consuming classes.

- **Parameters**: `type` — the `Type` that was used to register the typed client.
- **Returns**: A configured `HttpClient` instance bound to the typed client’s settings.
- **Throws**: `ArgumentNullException` if `type` is null; `InvalidOperationException` if no typed client has been registered for the given type.

### void Dispose()
Disposes the factory and triggers disposal of all managed `HttpMessageHandler` instances. After disposal, any attempt to call `GetClient` will result in undefined behavior. This method is typically called by the dependency injection container when the service scope ends.

### static async Task<T?> GetAsJsonAsync<T>(HttpClient client, string requestUri, CancellationToken cancellationToken = default)
Sends a GET request to the specified URI and deserializes a successful JSON response body into an instance of `T`. Returns `default(T)` if the response status code indicates failure or if deserialization fails.

- **Parameters**:
  - `client` — the `HttpClient` instance to use.
  - `requestUri` — the target URI.
  - `cancellationToken` — optional cancellation token.
- **Returns**: The deserialized object of type `T`, or `default(T)` on failure.
- **Throws**: `ArgumentNullException` if `client` or `requestUri` is null; `OperationCanceledException` if the token is canceled. Exceptions from the underlying HTTP call (e.g., `HttpRequestException`) propagate to the caller.

### static async Task<T?> PostAsJsonAsync<T>(HttpClient client, string requestUri, object value, CancellationToken cancellationToken = default)
Serializes the provided value as JSON, sends it in a POST request body to the specified URI, and deserializes a successful JSON response body into an instance of `T`. Returns `default(T)` if the response status code indicates failure or if deserialization fails.

- **Parameters**:
  - `client` — the `HttpClient` instance to use.
  - `requestUri` — the target URI.
  - `value` — the object to serialize as the JSON request body.
  - `cancellationToken` — optional cancellation token.
- **Returns**: The deserialized object of type `T`, or `default(T)` on failure.
- **Throws**: `ArgumentNullException` if `client`, `requestUri`, or `value` is null; `OperationCanceledException` if the token is canceled. Exceptions from the underlying HTTP call propagate to the caller.

### static async Task<(bool IsSuccess, int StatusCode, string Content)> SafeGetAsync(HttpClient client, string requestUri, CancellationToken cancellationToken = default)
Sends a GET request and returns a structured result tuple containing success status, the HTTP status code, and the raw response body as a string. This method never throws on HTTP-level failures; all outcomes are captured in the return tuple.

- **Parameters**:
  - `client` — the `HttpClient` instance to use.
  - `requestUri` — the target URI.
  - `cancellationToken` — optional cancellation token.
- **Returns**: A tuple where `IsSuccess` is `true` only for successful status codes (2xx), `StatusCode` contains the integer HTTP status code, and `Content` holds the response body string (empty if no body or on failure).
- **Throws**: `ArgumentNullException` if `client` or `requestUri` is null; `OperationCanceledException` if the token is canceled. True transport-level exceptions (e.g., `HttpRequestException` for DNS failure) still propagate.

## Usage

### Example 1: Named client with JSON GET and safe fallback

```csharp
// Registration in Startup or Program.cs
services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Consumption in a service
public class ProductService
{
    private readonly IHttpClientFactory _factory;

    public ProductService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<Product?> GetProductAsync(int productId)
    {
        var client = _factory.GetClient("ApiClient");

        // Primary attempt with typed deserialization
        var product = await IHttpClientFactory.GetAsJsonAsync<Product>(
            client, $"products/{productId}");

        if (product is not null)
            return product;

        // Fallback: safe call to inspect raw response
        var (isSuccess, statusCode, content) =
            await IHttpClientFactory.SafeGetAsync(client, $"products/{productId}");

        // Log the failure details and return null
        Console.WriteLine($"Product fetch failed: {statusCode}, Body: {content}");
        return null;
    }
}
```

### Example 2: Typed client with JSON POST

```csharp
// Typed client definition
public class OrderApiClient
{
    public HttpClient Client { get; }

    public OrderApiClient(HttpClient client)
    {
        Client = client;
    }
}

// Registration
services.AddHttpClient<OrderApiClient>(client =>
{
    client.BaseAddress = new Uri("https://orders.example.com/");
});

// Consumption
public class OrderService
{
    private readonly OrderApiClient _apiClient;

    public OrderService(OrderApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<OrderConfirmation?> PlaceOrderAsync(OrderRequest request)
    {
        var confirmation = await IHttpClientFactory.PostAsJsonAsync<OrderConfirmation>(
            _apiClient.Client, "api/orders", request);

        return confirmation;
    }
}
```

## Notes

- **Handler recycling**: The factory manages `HttpMessageHandler` lifetimes internally. Do not dispose `HttpClient` instances obtained from the factory; the factory handles cleanup when `Dispose()` is called on the factory itself, typically at scope end.
- **Thread safety**: `GetClient` is thread-safe. The returned `HttpClient` instances are themselves thread-safe for concurrent requests. The static extension methods are stateless and safe for concurrent invocation.
- **Default values on failure**: `GetAsJsonAsync<T>` and `PostAsJsonAsync<T>` return `default(T)` for non-success status codes or deserialization errors. For reference types, this is `null`; for value types, this is the zero-initialized value. Callers must null-check results to distinguish success from failure.
- **SafeGetAsync exception surface**: While `SafeGetAsync` captures HTTP-level failures in its tuple, true transport failures (network unreachable, DNS resolution failure, TLS negotiation errors) still throw `HttpRequestException`. Callers should wrap the call in a try-catch if complete resilience is required.
- **Cancellation**: All async methods accept a `CancellationToken`. If canceled, they throw `OperationCanceledException`. The underlying HTTP request is aborted, but the client and handler remain usable for subsequent requests.
- **Named vs typed resolution**: `GetClient(string)` and `GetClient(Type)` resolve from separate registrations. A typed client registration automatically creates a named registration using the type’s full name as the key, but the two resolution paths are independent—do not expect `GetClient("MyType")` to return a typed client unless explicitly registered by that name.
