# ExternalApiException

`ExternalApiException` represents an error that occurs while communicating with an external HTTP API.  
It captures details such as the request endpoint, HTTP method, and response status code, allowing
consumers to diagnose integration failures more effectively.

## API

| Member | Description |
|--------|-------------|
| `public string? Endpoint { get; set; }` | The absolute or relative URL of the external API endpoint that triggered the exception. May be `null` when the endpoint is unknown. |
| `public int? StatusCode { get; set; }` | The HTTP status code returned by the external service, if any. A `null` value indicates that no response was received (e.g., network failure). |
| `public string? Method { get; set; }` | The HTTP method (`GET`, `POST`, etc.) used for the request that caused the exception. May be `null` when the method is not applicable. |
| `public ExternalApiException(string message) : base(message)` | Initializes a new instance with a custom error message. The base `Exception` message is set to `message`. |
| `public ExternalApiException()` | Parameter‑less constructor required for serialization and for scenarios where the message is set later. |
| `public ExternalApiException(string message, Exception innerException)` | Initializes a new instance with a custom error message and an inner exception that provides the original cause of the failure. |
| `public ExternalApiException(SerializationInfo info, StreamingContext context)` | Protected constructor used during deserialization of the exception. It restores the serialized state of the object. |
| `public ExternalApiException WithContext(string? endpoint = null, int? statusCode = null, string? method = null)` | Returns the same exception instance after optionally assigning the supplied context values to `Endpoint`, `StatusCode`, and `Method`. Enables fluent enrichment of the exception before it is thrown or re‑thrown. Returns `this` for chaining. |

## Usage

### Example 1 – Enriching an exception before re‑throwing

