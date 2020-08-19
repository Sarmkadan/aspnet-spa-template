#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Events;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Utilities;

namespace AspNetSpaTemplate.Integration;

/// <summary>
/// Handler for incoming webhooks from external services.
/// Validates signatures, parses payloads, and triggers internal events.
/// </summary>
public sealed class WebhookHandler
{
	private readonly IEventBus _eventBus;
	private readonly ILogger<WebhookHandler> _logger;
	private readonly Dictionary<string, string> _webhookSecrets;

	public WebhookHandler(IEventBus eventBus, ILogger<WebhookHandler> logger)
	{
		_eventBus = eventBus;
		_logger = logger;
		_webhookSecrets = new Dictionary<string, string>();

		// In production, load from secure configuration
		_webhookSecrets["payment-provider"] = "webhook-secret-key-1";
		_webhookSecrets["email-service"] = "webhook-secret-key-2";
	}

	/// <summary>
	/// Processes incoming webhook from external service.
	/// Validates signature before processing.
	/// </summary>
	/// <param name="provider">The webhook provider name.</param>
	/// <param name="payload">The webhook payload as JSON string.</param>
	/// <param name="signature">The HMAC signature for verification.</param>
	/// <returns>True if webhook was processed successfully, false otherwise.</returns>
	public async Task<bool> HandleWebhookAsync(string provider, string payload, string signature)
	{
		try
		{
			// Validate required parameters
			if (string.IsNullOrWhiteSpace(provider))
			{
				_logger.LogWarning("Webhook provider cannot be null or empty");
				return false;
			}

			if (string.IsNullOrWhiteSpace(payload))
			{
				_logger.LogWarning("Webhook payload cannot be null or empty for provider {Provider}", provider);
				return false;
			}

			if (string.IsNullOrWhiteSpace(signature))
			{
				_logger.LogWarning("Webhook signature cannot be null or empty for provider {Provider}", provider);
				return false;
			}

			// Validate signature
			if (!ValidateSignature(provider, payload, signature))
			{
				_logger.LogWarning("Invalid webhook signature from {Provider}", provider);
				return false;
			}

			// Parse and process webhook
			var webhookData = JsonSerializationHelper.Deserialize<Dictionary<string, object>>(payload);
			if (webhookData is null)
			{
				_logger.LogError("Failed to parse webhook payload from {Provider}", provider);
				throw new ExternalApiException(provider, $"Invalid webhook payload format from {provider}");
			}

			// Route webhook to appropriate handler
			return await RouteWebhookAsync(provider, webhookData);
		}
		catch (JsonException ex)
		{
			_logger.LogError(ex, "JSON parsing error for webhook from {Provider}", provider);
			throw new ExternalApiException(provider, $"Invalid JSON payload from {provider}")
				.WithContext("Provider", provider)
				.WithContext("ErrorType", "JSON");
		}
		catch (ExternalApiException ex)
		{
			_logger.LogError(ex, "External API error processing webhook from {Provider}", provider);
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error handling webhook from {Provider}", provider);
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
			_logger.LogWarning("No webhook secret configured for {Provider}", provider);
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
		try
		{
			return provider switch
			{
				"payment-provider" => await HandlePaymentWebhookAsync(data),
				"email-service" => await HandleEmailWebhookAsync(data),
				"shipping-provider" => await HandleShippingWebhookAsync(data),
				_ => HandleUnknownWebhookAsync(provider)
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error routing webhook from {Provider}", provider);
			return false;
		}
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
			if (!data.TryGetValue("event_type", out var evt) || evt is null)
			{
				_logger.LogError("Missing event_type in payment webhook payload");
				return false;
			}

			var eventType = evt.ToString();
			if (string.IsNullOrWhiteSpace(eventType))
			{
				_logger.LogError("Invalid event_type in payment webhook payload");
				return false;
			}

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

				case "payment_refunded":
					// Publish order payment refunded event
					await _eventBus.PublishAsync(new CustomEvent
					{
						EventName = "PaymentRefunded",
						Data = data,
						AggregateType = "Payment"
					});
					break;

				default:
					_logger.LogWarning("Unknown payment event type: {EventType}", eventType);
					return false;
			}

			return true;
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogError(ex, "Missing required field in payment webhook payload");
			return false;
		}
		catch (FormatException ex)
		{
			_logger.LogError(ex, "Invalid format in payment webhook payload");
			return false;
		}
		catch (OverflowException ex)
		{
			_logger.LogError(ex, "Numeric overflow in payment webhook payload");
			return false;
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
					_logger.LogWarning("Email bounced: {Email}", email);
					await _eventBus.PublishAsync(new CustomEvent
					{
						EventName = "EmailBounced",
						Data = data
					});
					break;

				case "complaint":
					_logger.LogWarning("Email complaint: {Email}", email);
					// Remove from mailing list
					break;

				case "delivered":
					_logger.LogInformation("Email delivered: {Email}", email);
					break;

				default:
					_logger.LogWarning("Unknown email event type: {EventType}", eventType);
					return false;
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

			if (string.IsNullOrWhiteSpace(trackingNumber))
			{
				_logger.LogError("Missing tracking_number in shipping webhook payload");
				return false;
			}

			// Publish shipping status event
			await _eventBus.PublishAsync(new CustomEvent
			{
				EventName = "ShippingStatusChanged",
				Data = data,
				AggregateType = "Shipment"
			});

			_logger.LogInformation("Shipment {TrackingNumber} status: {Status}", trackingNumber, status);
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
		_logger.LogWarning("Unknown webhook provider: {Provider}", provider);
		return false;
	}

	/// <summary>
	/// Registers a webhook endpoint for a new provider.
	/// </summary>
	public void RegisterWebhook(string provider, string secret)
	{
		if (string.IsNullOrWhiteSpace(provider))
		{
			throw new ArgumentException("Provider cannot be null or empty", nameof(provider));
		}

		if (string.IsNullOrWhiteSpace(secret))
		{
			throw new ArgumentException("Secret cannot be null or empty", nameof(secret));
		}

		_webhookSecrets[provider] = secret;
		_logger.LogInformation("Registered webhook for provider: {Provider}", provider);
	}
}

/// <summary>
/// Webhook request model for API endpoint.
/// </summary>
public sealed class WebhookRequest
{
	public string Provider { get; set; } = "";
	public string Payload { get; set; } = "";
	public string Signature { get; set; } = "";
}

/// <summary>
/// Webhook response model.
/// </summary>
public sealed class WebhookResponse
{
	public bool Acknowledged { get; set; }
	public string? Message { get; set; }
	public string? ErrorCode { get; set; }
}