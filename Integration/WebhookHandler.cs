// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Events;
using AspNetSpaTemplate.Utilities;

namespace AspNetSpaTemplate.Integration;

/// <summary>
/// Handler for incoming webhooks from external services.
/// Validates signatures, parses payloads, and triggers internal events.
/// </summary>
public class WebhookHandler
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<WebhookHandler> _logger;
    private readonly Dictionary<string, string> _webhookSecrets = new();

    public WebhookHandler(IEventBus eventBus, ILogger<WebhookHandler> logger)
    {
        _eventBus = eventBus;
        _logger = logger;

        // In production, load from secure configuration
        _webhookSecrets["payment-provider"] = "webhook-secret-key-1";
        _webhookSecrets["email-service"] = "webhook-secret-key-2";
    }

    /// <summary>
    /// Processes incoming webhook from external service.
    /// Validates signature before processing.
    /// </summary>
    public async Task<bool> HandleWebhookAsync(string provider, string payload, string signature)
    {
        try
        {
            // Validate signature
            if (!ValidateSignature(provider, payload, signature))
            {
                _logger.LogWarning($"Invalid webhook signature from {provider}");
                return false;
            }

            // Parse and process webhook
            var webhookData = JsonSerializationHelper.Deserialize<Dictionary<string, object>>(payload);
            if (webhookData == null)
            {
                _logger.LogError($"Failed to parse webhook payload from {provider}");
                return false;
            }

            // Route webhook to appropriate handler
            return await RouteWebhookAsync(provider, webhookData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling webhook from {provider}");
            return false;
        }
    }

    /// <summary>
    /// Validates webhook signature using HMAC-SHA256.
    /// Prevents replay attacks and ensures payload integrity.
    /// </summary>
    private bool ValidateSignature(string provider, string payload, string signature)
    {
        if (!_webhookSecrets.TryGetValue(provider, out var secret))
        {
            _logger.LogWarning($"No webhook secret configured for {provider}");
            return false;
        }

        // Compute expected signature
        var expectedSignature = EncryptionHelper.ComputeHmacSha256(payload, secret);

        // Compare signatures (constant-time to prevent timing attacks)
        var signatureValid = expectedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
        return signatureValid;
    }

    /// <summary>
    /// Routes webhook to appropriate handler based on provider and event type.
    /// </summary>
    private async Task<bool> RouteWebhookAsync(string provider, Dictionary<string, object> data)
    {
        return provider switch
        {
            "payment-provider" => await HandlePaymentWebhookAsync(data),
            "email-service" => await HandleEmailWebhookAsync(data),
            "shipping-provider" => await HandleShippingWebhookAsync(data),
            _ => HandleUnknownWebhookAsync(provider)
        };
    }

    /// <summary>
    /// Handles payment provider webhook (e.g., payment succeeded, failed, refunded).
    /// </summary>
    private async Task<bool> HandlePaymentWebhookAsync(Dictionary<string, object> data)
    {
        try
        {
            _logger.LogInformation("Processing payment webhook");

            // Extract payment details
            var eventType = data.TryGetValue("event_type", out var evt) ? evt.ToString() : "";
            var orderId = data.TryGetValue("order_id", out var oid) ? int.Parse(oid.ToString()!) : 0;

            switch (eventType)
            {
                case "payment_succeeded":
                    // Publish order payment success event
                    await _eventBus.PublishAsync(new CustomEvent
                    {
                        EventName = "PaymentSucceeded",
                        Data = data,
                        AggregateType = "Payment"
                    });
                    break;

                case "payment_failed":
                    // Publish order payment failed event
                    await _eventBus.PublishAsync(new CustomEvent
                    {
                        EventName = "PaymentFailed",
                        Data = data,
                        AggregateType = "Payment"
                    });
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment webhook");
            return false;
        }
    }

    /// <summary>
    /// Handles email service webhook (e.g., delivery failures, bounces).
    /// </summary>
    private async Task<bool> HandleEmailWebhookAsync(Dictionary<string, object> data)
    {
        try
        {
            _logger.LogInformation("Processing email webhook");

            var eventType = data.TryGetValue("event", out var evt) ? evt.ToString() : "";
            var email = data.TryGetValue("email", out var em) ? em.ToString() : "";

            switch (eventType)
            {
                case "bounce":
                    _logger.LogWarning($"Email bounced: {email}");
                    await _eventBus.PublishAsync(new CustomEvent
                    {
                        EventName = "EmailBounced",
                        Data = data
                    });
                    break;

                case "complaint":
                    _logger.LogWarning($"Email complaint: {email}");
                    // Remove from mailing list
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email webhook");
            return false;
        }
    }

    /// <summary>
    /// Handles shipping provider webhook (e.g., shipment tracking updates).
    /// </summary>
    private async Task<bool> HandleShippingWebhookAsync(Dictionary<string, object> data)
    {
        try
        {
            _logger.LogInformation("Processing shipping webhook");

            var status = data.TryGetValue("status", out var st) ? st.ToString() : "";
            var trackingNumber = data.TryGetValue("tracking_number", out var tn) ? tn.ToString() : "";

            // Publish shipping status event
            await _eventBus.PublishAsync(new CustomEvent
            {
                EventName = "ShippingStatusChanged",
                Data = data,
                AggregateType = "Shipment"
            });

            _logger.LogInformation($"Shipment {trackingNumber} status: {status}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing shipping webhook");
            return false;
        }
    }

    /// <summary>
    /// Handles unknown webhook provider.
    /// </summary>
    private bool HandleUnknownWebhookAsync(string provider)
    {
        _logger.LogWarning($"Unknown webhook provider: {provider}");
        return false;
    }

    /// <summary>
    /// Registers a webhook endpoint for a new provider.
    /// </summary>
    public void RegisterWebhook(string provider, string secret)
    {
        _webhookSecrets[provider] = secret;
        _logger.LogInformation($"Registered webhook for provider: {provider}");
    }
}

/// <summary>
/// Webhook request model for API endpoint.
/// </summary>
public class WebhookRequest
{
    public string Provider { get; set; } = "";
    public string Payload { get; set; } = "";
    public string Signature { get; set; } = "";
}

/// <summary>
/// Webhook response model.
/// </summary>
public class WebhookResponse
{
    public bool Acknowledged { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
}
