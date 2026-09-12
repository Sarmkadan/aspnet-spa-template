# WebhooksController
The `WebhooksController` class is responsible for receiving webhooks from external payment, email, shipping, and custom providers. It validates each request with an HMAC-SHA256 signature supplied in the `X-Signature` header, delegates accepted payloads to `WebhookHandler`, and immediately acknowledges successfully queued work.

## API
The `WebhooksController` class exposes the following public members:
* `public WebhooksController`: The constructor for the `WebhooksController` class. It requires a `WebhookHandler`, an `ILogger<WebhooksController>`, and the application's `AspnetSpaTemplateOptions`.
* `public async Task<IActionResult> HandlePaymentWebhook`: Handles `POST /api/webhooks/payment`. It validates the request payload and the payment provider signature, then queues the payload under the `payment-provider` provider name.
* `public async Task<IActionResult> HandleEmailWebhook`: Handles `POST /api/webhooks/email`. It validates the request payload and the email service signature, then queues the payload under the `email-service` provider name.
* `public async Task<IActionResult> HandleShippingWebhook`: Handles `POST /api/webhooks/shipping`. It validates the request payload and the shipping provider signature, then queues the payload under the `shipping-provider` provider name.
* `public async Task<IActionResult> HandleGenericWebhook`: Handles `POST /api/webhooks/{provider}` for custom integrations. It validates the provider, payload, and signature before forwarding the webhook to the configured handler.

## Usage
Here are two examples of calling the webhook endpoints:
```csharp
// Example 1: Sending a payment webhook
using var paymentRequest = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/payment");
paymentRequest.Headers.Add("X-Signature", computedPaymentSignature);
paymentRequest.Content = JsonContent.Create(new WebhookRequest
{
    Payload = paymentPayload,
    Signature = computedPaymentSignature
});

var paymentResponse = await httpClient.SendAsync(paymentRequest);
paymentResponse.EnsureSuccessStatusCode();

// Example 2: Sending a webhook for a custom provider
using var customRequest = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/custom-provider");
customRequest.Headers.Add("X-Signature", computedCustomSignature);
customRequest.Content = JsonContent.Create(new WebhookRequest
{
    Payload = customPayload,
    Signature = computedCustomSignature
});

var customResponse = await httpClient.SendAsync(customRequest);
customResponse.EnsureSuccessStatusCode();
```

## Notes
When using the `WebhooksController` class, consider the following validation and processing behavior:
* Every endpoint expects a `WebhookRequest` JSON body with a non-empty `Payload` value.
* The `X-Signature` header is required and must contain the HMAC-SHA256 signature of the payload, calculated with the secret configured for the provider.
* Payment, email, and shipping requests use the secrets configured for `payment-provider`, `email-service`, and `shipping-provider`, respectively.
* The generic endpoint only accepts providers for which the controller can resolve a configured secret; unknown or unconfigured providers fail signature verification.
* A `200 OK` response indicates that the webhook was received and queued for processing.
* Invalid providers or payloads return `400 Bad Request`. Missing, empty, or invalid signatures return `401 Unauthorized`; handler rejection also returns `401 Unauthorized`.
* Webhook processing is delegated to `WebhookHandler` so the endpoint can acknowledge the request promptly.
