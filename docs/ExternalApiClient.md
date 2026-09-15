# ExternalApiClient

`ExternalApiClient` is a typed wrapper around an injected `HttpClient` for making JSON `GET` and `POST` requests to external APIs. It provides logging, retries for selected transient failures, JSON serialization through `JsonSerializationHelper`, and consistent error wrapping with `ExternalApiException`.

The class is registered as a scoped service by the application. Configure the injected `HttpClient` (for example, its base address, headers, and timeout) when registering it.

## API

### `public ExternalApiClient(HttpClient httpClient, ILogger<ExternalApiClient> logger)`

Creates a client using the supplied `HttpClient` and logger. The implementation uses a fixed maximum of three attempts and a linearly increasing delay of one second per retry number.

- `httpClient`: The client used to send requests. Relative endpoints are resolved using its `BaseAddress`.
- `logger`: Receives informational, warning, and error messages for request attempts and failures.

### `public Task<T> GetAsync<T>(string endpoint) where T : class`

Sends a `GET` request and deserializes a successful JSON response to `T`.

- `endpoint`: A relative or absolute request URI accepted by `HttpClient`.
- Returns: The deserialized response. A JSON `null` result is treated as an error.
- Retry behavior: HTTP `5xx` and `408 Request Timeout` responses are retried, up to three total attempts. An `HttpRequestException` whose inner exception is a `TimeoutException` is also retried. Delays before the second and third attempts are one and two seconds, respectively.
- Errors: Failures are wrapped in `ExternalApiException`. Non-success responses eventually pass through `EnsureSuccessStatusCode`; client errors are not retried. Deserialization errors and null responses are also wrapped.

### `public Task<T> PostAsync<T>(string endpoint, object request) where T : class`

Serializes `request` as JSON, sends it as UTF-8 `application/json`, and deserializes a successful JSON response to `T`.

- `endpoint`: A relative or absolute request URI accepted by `HttpClient`.
- `request`: The value serialized as the request body.
- Returns: The deserialized response. A JSON `null` result is treated as an error.
- Retry behavior: HTTP `5xx` responses are retried, up to three total attempts. An `HttpRequestException` whose inner exception is a `TimeoutException` is also retried. Unlike `GET`, an HTTP `408` response is not explicitly retried by the status-code branch. Delays before the second and third attempts are one and two seconds, respectively.
- Errors: Failures are wrapped in `ExternalApiException`. Non-success responses eventually pass through `EnsureSuccessStatusCode`; client errors are not retried. Serialization, deserialization, and null-response errors are also wrapped.

## ExternalApiConfig

`ExternalApiConfig` is a separate public configuration model. `ExternalApiClient` does not consume this type directly; it can be used by application registration code to configure an `HttpClient` or related integration services.

### `public string BaseUrl { get; set; } = ""`

The external service's base URL.

### `public string ApiKey { get; set; } = ""`

The API key used by the external service.

### `public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30)`

The configured request timeout. The default is 30 seconds.

### `public int MaxRetries { get; set; } = 3`

The configured maximum retry count. The default is 3. The current `ExternalApiClient` implementation uses its own fixed value of 3 rather than reading this property.

### `public bool LogRequests { get; set; } = true`

Indicates whether requests should be logged. The default is `true`. The current `ExternalApiClient` always writes its request-attempt log messages and does not read this property directly.

### `public bool LogResponses { get; set; } = false`

Indicates whether responses should be logged. The default is `false`. The current `ExternalApiClient` does not read this property or log response bodies.

## Usage

```csharp
var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.example.com"),
    Timeout = TimeSpan.FromSeconds(30)
};

var client = new ExternalApiClient(httpClient, logger);

var user = await client.GetAsync<User>("/users/42");

var request = new CreateOrderRequest { ProductId = 9, Quantity = 1 };
var order = await client.PostAsync<Order>("/orders", request);
```

## Notes

- `ExternalApiClient` does not implement `IDisposable`; ownership and lifetime of the injected `HttpClient` remain with the caller or dependency injection container.
- The client can be called concurrently when its injected `HttpClient` and logger are safe for concurrent use.
- Request attempts are logged at information level. Retryable GET server errors and timeout retries are logged at warning level, and terminal request failures are logged at error level.
- Error context may contain endpoint details and, for some POST failures, the request object. Avoid placing secrets in endpoints or request objects if exception context is recorded or exposed.
