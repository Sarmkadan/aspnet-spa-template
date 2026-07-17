# WebhookHandler

A utility class for receiving and processing webhook payloads from external providers. It encapsulates the logic for validating, acknowledging, and handling webhook messages with support for signature verification and error tracking.

## API

### `public WebhookHandler`

Initializes a new instance of the `WebhookHandler` class. This constructor sets up the handler with default values for all properties.

### `public async Task<bool> HandleWebhookAsync`

Processes the incoming webhook payload asynchronously. Returns `true` if the webhook was handled successfully; otherwise, `false`.

- **Parameters**: None
- **Return value**: A `Task<bool>` representing the asynchronous operation. The result indicates success (`true`) or failure (`false`).
- **Exceptions**: Throws `ArgumentNullException` if `Payload` is null or empty.

### `public void RegisterWebhook`

Registers the webhook with the specified provider. This method is typically called during application startup to configure webhook endpoints.

- **Parameters**: None
- **Return value**: None
- **Exceptions**: None

### `public string Provider`

Gets or sets the name of the webhook provider (e.g., "GitHub", "Stripe"). This value identifies the source of the incoming payload.

- **Type**: `string`
- **Default**: `null`

### `public string Payload`

Gets or sets the raw JSON payload received from the webhook provider. This is the unprocessed data sent by the external service.

- **Type**: `string`
- **Default**: `null`

### `public string Signature`

Gets or sets the signature provided by the webhook provider for verification. This is typically used to validate the authenticity of the payload.

- **Type**: `string`
- **Default**: `null`

### `public bool Acknowledged`

Gets or sets a value indicating whether the webhook has been acknowledged. Acknowledgment typically means the payload has been processed or queued for processing.

- **Type**: `bool`
- **Default**: `false`

### `public string? Message`

Gets or sets an optional message associated with the webhook processing (e.g., a success message or error description).

- **Type**: `string`
- **Default**: `null`

### `public string? ErrorCode`

Gets or sets an optional error code if the webhook processing failed. This can be used to identify specific failure scenarios.

- **Type**: `string`
- **Default**: `null`

## Usage

### Example 1: Basic Webhook Handling
```csharp
var handler = new WebhookHandler();
handler.Provider = "GitHub";
handler.Payload = "{\"action\":\"opened\",\"issue\":{\"number\":1}}";
handler.Signature = "sha256=...";

bool success = await handler.HandleWebhookAsync();
if (success)
{
    Console.WriteLine("Webhook processed successfully.");
}
else
{
    Console.WriteLine($"Error: {handler.ErrorCode} - {handler.Message}");
}
```

### Example 2: Registering a Webhook Provider
```csharp
var handler = new WebhookHandler();
handler.RegisterWebhook();
handler.Provider = "Stripe";

// Simulate receiving a webhook
handler.Payload = "{\"id\":\"evt_123\",\"type\":\"payment_intent.succeeded\"}";
handler.Signature = "t=1234567890,v1=...";

await handler.HandleWebhookAsync();
if (handler.Acknowledged)
{
    Console.WriteLine($"Webhook from {handler.Provider} acknowledged.");
}
```

## Notes

- **Thread Safety**: This class is not thread-safe. Concurrent access to properties like `Payload`, `Signature`, or methods like `HandleWebhookAsync` may lead to race conditions. External synchronization is required if used in a multi-threaded context.
- **Null Checks**: The `Payload` property must be set before calling `HandleWebhookAsync`; otherwise, an `ArgumentNullException` is thrown.
- **Signature Verification**: While the class exposes a `Signature` property, it does not perform automatic verification. Implementers must validate the signature against the provider's expected algorithm and secret.
- **Error Handling**: If `HandleWebhookAsync` fails, inspect `ErrorCode` and `Message` for diagnostic information. The handler does not throw exceptions for business-logic failures (e.g., invalid payload structure), instead setting these properties to indicate the issue.
