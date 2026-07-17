# ExternalApiClient

`ExternalApiClient` is a configurable HTTP client wrapper designed to simplify calling external REST APIs. It provides typed `GET` and `POST` methods with automatic JSON serialization, configurable retry logic, and optional request/response logging. The class is intended to be registered and managed via `IHttpClientFactory`.

## API

### `public ExternalApiClient`

The parameterized constructor. It accepts configuration values required to set up the underlying `HttpClient` and the client’s behavior. The exact signature is not listed here, but it initializes the `BaseUrl`, `ApiKey`, `Timeout`, `MaxRetries`, `LogRequests`, and `LogResponses` properties.

### `public async Task<T> GetAsync<T>(string requestUri)`

Sends a `GET` request to the specified relative or absolute URI and deserializes the response body as JSON.

- **Parameters:**
  - `requestUri` (`string`): The request URI, relative to `BaseUrl` or absolute.
- **Returns:** `Task<T>` – The deserialized response body.
- **Throws:**
  - `HttpRequestException` on network failures or non-success status codes.
  - `TaskCanceledException` when the request times out.
  - `JsonException` when the response body cannot be deserialized to `T`.

### `public async Task<T> PostAsync<T>(string path, object body)`

Sends a `POST` request with a JSON-serialized body and deserializes the response body as JSON.

- **Parameters:**
  - `path` (`string`): The request path, relative to `BaseUrl` or absolute.
  - `body` (`object`): The payload to serialize as JSON in the request body.
- **Returns:** `Task<T>` — the deserialized response body.
- **Throws:**
  - `HttpRequestException` on network failures or non-success status codes.
  - `TaskCanceledException` when the request times out.
  - `JsonException` when serialization of the request body or deserialization of the response fails.

### `public string BaseUrl`

The base URL used for all requests when a relative path is provided. Trailing slashes are normalized internally.

### `public string ApiKey`

The API key sent with every request, typically as a header (e.g., `X-Api-Key`). If `null` or empty, the header is omitted.

### `public TimeSpan Timeout`

The per-request timeout applied to the underlying `HttpClient`. Defaults to a value set during construction.

### `public int MaxRetries`

The maximum number of automatic retries for transient failures (e.g., `5xx` responses, network errors). A value of `0` disables retries.

### `public bool LogRequests`

When `true`, the full request URI, method, and headers are written to the configured logging infrastructure before the request is sent. Defaults to `false`.

### `public bool LogResponses`

When `true`, the response status code, headers, and body are logged after a response is received. Defaults to `false`.

## Usage

### Example 1: Basic GET request

```csharp
var client = new ExternalApiClient(httpClientFactory, options =>
{
    options.BaseUrl = "https://api.example.com";
    options.ApiKey = "sk-12345";
    options.Timeout = TimeSpan.FromSeconds(10);
    options.MaxRetries = 2;
    options.LogResponses = true;
});

var user = await client.GetAsync<User>("/users/42");
Console.WriteLine(user.Email);
```

### Example 2: POST with retries and logging

```csharp
var client = new ExternalApiClient(httpClientFactory, options =>
{
    options.BaseUrl = "https://api.example.com";
    options.ApiKey = "sk-67890";
    options.MaxRetries = 3;
    options.LogRequests = true;
    options.LogResponses = true;
});

var payload = new CreateOrderRequest { ProductId = 9, Quantity = 1 };
var order = await client.PostAsync<Order>("/orders", payload);
Console.WriteLine($"Order {order.Id} created.");
```

## Notes

- **Thread safety:** The `GetAsync<T>` and `PostAsync<T>` methods are safe to call concurrently from multiple threads. The underlying `HttpClient` is managed by `IHttpClientFactory` and is designed for concurrent use.
- **Retry behavior:** Retries are performed only for transient failures (typically `5xx` status codes, `408 Request Timeout`, and `TaskCanceledException` due to timeout). Non-transient errors (e.g., `400`, `401`, `403`) are not retried. The `MaxRetries` count includes the initial attempt; setting it to `1` means no retries.
- **Timeout:** The `Timeout` property sets the overall timeout for each HTTP call, including all retry attempts. If a single attempt exceeds the timeout, it is canceled and may be retried if retries remain.
- **Logging:** When `LogRequests` or `LogResponses` is enabled, sensitive data (including the `ApiKey` header value) may appear in logs. Ensure log redaction is applied in production environments.
- **Disposal:** The class does not implement `IDisposable`. The underlying `HttpClient` lifecycle is managed by the dependency injection container through `IHttpClientFactory`.
