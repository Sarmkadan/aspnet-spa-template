#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using AspNetSpaTemplate.Configuration;
using AspNetSpaTemplate.Integration;
using AspNetSpaTemplate.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace AspNetSpaTemplate.Controllers;

/// <summary>
/// API controller for receiving webhooks from external services.
/// Validates signatures, queues processing, and returns immediate response.
/// Should respond quickly (under 5 seconds) to avoid timeout.
/// </summary>
[ApiController]
[Route("api/webhooks")]
public sealed class WebhooksController : ControllerBase
{
    private readonly WebhookHandler _webhookHandler;
    private readonly ILogger<WebhooksController> _logger;
    private readonly AspnetSpaTemplateOptions _options;

    public WebhooksController(
        WebhookHandler webhookHandler,
        ILogger<WebhooksController> logger,
        AspnetSpaTemplateOptions options)
    {
        _webhookHandler = webhookHandler;
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// Receives and processes webhook from payment provider.
    /// Validates HMAC signature and queues for processing.
    /// </summary>
    [HttpPost("payment")]
    [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> HandlePaymentWebhook([FromBody] WebhookRequest request)
    {
        if (request is null || string.IsNullOrEmpty(request.Payload))
        {
            _logger.LogWarning("Invalid webhook request: empty payload");
            return BadRequest(new WebhookResponse
            {
                Acknowledged = false,
                ErrorCode = "INVALID_PAYLOAD",
                Message = "Payload cannot be empty"
            });
        }

        // Verify X-Signature header before processing
        if (!VerifySignature("payment-provider", request.Payload))
        {
            return Unauthorized(new WebhookResponse
            {
                Acknowledged = false,
                ErrorCode = "SIGNATURE_INVALID",
                Message = "Invalid webhook signature"
            });
        }

        // Process webhook asynchronously
        var success = await _webhookHandler.HandleWebhookAsync("payment-provider", request.Payload, request.Signature);

        if (!success)
        {
            return Unauthorized(new WebhookResponse
            {
                Acknowledged = false,
                ErrorCode = "SIGNATURE_INVALID",
                Message = "Webhook signature verification failed"
            });
        }

        _logger.LogInformation("Payment webhook processed successfully");
        return Ok(new WebhookResponse
        {
            Acknowledged = true,
            Message = "Webhook received and queued for processing"
        });
    }

    /// <summary>
    /// Verifies webhook signature using HMAC-SHA256.
    /// Validates the X-Signature header against the raw request body.
    /// </summary>
    private bool VerifySignature(string provider, string payload)
    {
        // Get X-Signature header from request
        if (!Request.Headers.TryGetValue("X-Signature", out var signatureHeader))
        {
            _logger.LogWarning("X-Signature header is missing for provider {Provider}", provider);
            return false;
        }

        string signature = signatureHeader.ToString();

        if (string.IsNullOrWhiteSpace(signature))
        {
            _logger.LogWarning("X-Signature header is empty for provider {Provider}", provider);
            return false;
        }

        string secret = provider switch
        {
            "payment-provider" => _options.Webhooks.PaymentProviderSecret,
            "email-service" => _options.Webhooks.EmailServiceSecret,
            "shipping-provider" => _options.Webhooks.ShippingProviderSecret,
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(secret))
        {
            _logger.LogWarning("No webhook secret configured for provider {Provider}", provider);
            return false;
        }

        // Compute expected signature using HMAC-SHA256
        var expectedSignature = EncryptionHelper.ComputeHmacSha256(payload, secret);

        // Compare signatures (constant-time comparison to prevent timing attacks)
        var signatureValid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(signature));

        if (!signatureValid)
        {
            _logger.LogWarning("Invalid X-Signature header for provider {Provider}", provider);
        }

        return signatureValid;
    }

    /// <summary>
    /// Receives webhook from email service (delivery status, bounces, complaints).
    /// </summary>
    [HttpPost("email")]
    [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> HandleEmailWebhook([FromBody] WebhookRequest request)
    {
        // Verify X-Signature header before processing
        if (!VerifySignature("email-service", request.Payload))
        {
            return Unauthorized(new WebhookResponse
            {
                Acknowledged = false,
                ErrorCode = "SIGNATURE_INVALID",
                Message = "Invalid webhook signature"
            });
        }

        var success = await _webhookHandler.HandleWebhookAsync("email-service", request.Payload, request.Signature);

        if (!success)
        {
            return Unauthorized(new WebhookResponse
            {
                Acknowledged = false,
                ErrorCode = "SIGNATURE_INVALID",
                Message = "Webhook signature verification failed"
            });
        }

        return Ok(new WebhookResponse
        {
            Acknowledged = true,
            Message = "Webhook received and queued for processing"
        });
    }

    /// <summary>
    /// Receives webhook from shipping provider (shipment tracking updates).
    /// </summary>
    [HttpPost("shipping")]
    [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> HandleShippingWebhook([FromBody] WebhookRequest request)
    {
        // Verify X-Signature header before processing
        if (!VerifySignature("shipping-provider", request.Payload))
        {
            return Unauthorized(new WebhookResponse
            {
                Acknowledged = false,
                ErrorCode = "SIGNATURE_INVALID",
                Message = "Invalid webhook signature"
            });
        }

        var success = await _webhookHandler.HandleWebhookAsync("shipping-provider", request.Payload, request.Signature);

        if (!success)
        {
            return Unauthorized(new WebhookResponse
            {
                Acknowledged = false,
                ErrorCode = "SIGNATURE_INVALID",
                Message = "Webhook signature verification failed"
            });
        }

        return Ok(new WebhookResponse
        {
            Acknowledged = true,
            Message = "Webhook received and queued for processing"
        });
    }

    /// <summary>
    /// Generic webhook endpoint for custom integrations.
    /// Routes to appropriate handler based on provider.
    /// </summary>
    [HttpPost("{provider}")]
    [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> HandleGenericWebhook(string provider, [FromBody] WebhookRequest request)
    {
        if (string.IsNullOrEmpty(provider))
        {
            return BadRequest(new WebhookResponse
            {
                Acknowledged = false,
                ErrorCode = "INVALID_PROVIDER",
                Message = "Provider name is required"
            });
        }

        if (request is null || string.IsNullOrEmpty(request.Payload))
        {
            return BadRequest(new WebhookResponse
            {
                Acknowledged = false,
                ErrorCode = "INVALID_PAYLOAD",
                Message = "Payload cannot be empty"
            });
        }

        // Verify X-Signature header before processing
        if (!VerifySignature(provider, request.Payload))
        {
            return Unauthorized(new WebhookResponse
            {
                Acknowledged = false,
                ErrorCode = "SIGNATURE_INVALID",
                Message = "Invalid webhook signature"
            });
        }

        var success = await _webhookHandler.HandleWebhookAsync(provider, request.Payload, request.Signature);

        if (!success)
        {
            return Unauthorized(new WebhookResponse
            {
                Acknowledged = false,
                ErrorCode = "PROCESSING_FAILED",
                Message = "Failed to process webhook"
            });
        }

        return Ok(new WebhookResponse
        {
            Acknowledged = true,
            Message = "Webhook received and queued for processing"
        });
    }
}