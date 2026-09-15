# WebhookHandler

`WebhookHandler` receives JSON webhook payloads from supported external providers and publishes the corresponding internal events through `IEventBus`. Signature verification is not performed by this class; callers must validate the signature before invoking it.

The handler and its request and response models are in the `AspNetSpaTemplate.Integration` namespace.

## `WebhookHandler` API

### `public WebhookHandler(IEventBus eventBus, ILogger<WebhookHandler> logger)`

Creates a handler with the event bus used to publish internal events and the logger used to record processing results.

- `eventBus`: Event bus that receives events produced from webhook payloads.
- `logger`: Logger for webhook validation, routing, and processing messages.
- Throws `ArgumentNullException` if either dependency is `null`.

### `public Task<bool> HandleWebhookAsync(string provider, string payload, string signature)`

Processes a previously authenticated webhook and routes it according to its provider.

- `provider`: Provider identifier. Supported values are `payment-provider`, `email-service`, and `shipping-provider`.
- `payload`: Webhook body as a JSON object string.
- `signature`: The webhook signature. It is required, but is assumed to have already been validated by the caller.
- Returns `true` when the provider and event are supported and processing succeeds; otherwise, returns `false`.
- Throws `ArgumentNullException` when any argument is `null`.
- Throws `ArgumentException` when any argument is an empty string.
- Throws `ExternalApiException` when deserialization produces `null` (for example, for the JSON literal `null`).

Whitespace-only arguments pass the initial argument guards but are rejected during processing and return `false`. Malformed JSON is wrapped by `JsonSerializationHelper` and, like other unexpected processing or routing exceptions, is logged and normally results in `false`.

## Provider routing

### Payment provider

For `payment-provider`, the payload must contain a non-empty `event_type`. The supported event types publish these `CustomEvent` instances:

| `event_type` | Published `EventName` | `AggregateType` |
| --- | --- | --- |
| `payment_succeeded` | `PaymentSucceeded` | `Payment` |
| `payment_failed` | `PaymentFailed` | `Payment` |
| `payment_refunded` | `PaymentRefunded` | `Payment` |

The original deserialized payload is assigned to the event's `Data`. An unknown or missing event type, or an invalid `order_id` value when present, returns `false`.

### Email service

For `email-service`, the handler reads the `event` and `email` fields. A `bounce` publishes an `EmailBounced` `CustomEvent` containing the original payload. `complaint` and `delivered` are logged and return `true` without publishing an event. Unknown event types return `false`.

### Shipping provider

For `shipping-provider`, the payload must contain a non-empty `tracking_number`. Successful processing publishes a `ShippingStatusChanged` `CustomEvent` with `AggregateType` set to `Shipment` and the original payload assigned to `Data`. A missing tracking number returns `false`.

Unknown provider identifiers are logged and return `false`.

## Request and response models

The source file also defines the public DTOs used by a webhook API endpoint.

### `public sealed class WebhookRequest`

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Provider` | `string` | `""` | Provider identifier passed to the handler. |
| `Payload` | `string` | `""` | Raw JSON payload passed to the handler. |
| `Signature` | `string` | `""` | Signature supplied by the provider and validated by the caller. |

### `public sealed class WebhookResponse`

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Acknowledged` | `bool` | `false` | Whether the webhook was acknowledged. |
| `Message` | `string?` | `null` | Optional response message. |
| `ErrorCode` | `string?` | `null` | Optional error code. |

## Usage

```csharp
var handler = new WebhookHandler(eventBus, logger);

var handled = await handler.HandleWebhookAsync(
    "payment-provider",
    """{"event_type":"payment_succeeded","order_id":123}""",
    validatedSignature);

if (!handled)
{
    // Return an appropriate non-success response to the provider.
}
```

Each invocation uses only its arguments and injected services; `WebhookHandler` does not retain request state. Whether concurrent calls are safe therefore depends on the injected `IEventBus` and logger implementations.
