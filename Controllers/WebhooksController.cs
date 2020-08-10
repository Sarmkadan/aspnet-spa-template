// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Integration;

namespace AspNetSpaTemplate.Controllers;

/// <summary>
/// API controller for receiving webhooks from external services.
/// Validates signatures, queues processing, and returns immediate response.
/// Should respond quickly (< 5 seconds) to avoid timeout.
/// </summary>
[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly WebhookHandler _webhookHandler;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(WebhookHandler webhookHandler, ILogger<WebhooksController> logger)
    {
        _webhookHandler = webhookHandler;
        _logger = logger;
    }

    /// <summary>
    /// Receives and processes webhook from payment provider.
    /// Validates HMAC signature and queues for processing.
    /// </summary>
    [HttpPost("payment")]
    [ProduceResponseType(typeof(WebhookResponse), StatusCodes.Status200OK)]
    [ProduceResponseType(typeof(WebhookResponse), StatusCodes.Status400BadRequest)]
    [ProduceResponseType(typeof(WebhookResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> HandlePaymentWebhook([FromBody] WebhookRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Payload))
        {
            _logger.LogWarning("Invalid webhook request: empty payload");
            return BadRequest(new WebhookResponse
            {
                Acknowledged = false,
                ErrorCode = "INVALID_PAYLOAD",
                Message = "Payload cannot be empty"
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
    /// Receives webhook from email service (delivery status, bounces, complaints).
    /// </summary>
    [HttpPost("email")]
    [ProduceResponseType(typeof(WebhookResponse), StatusCodes.Status200OK)]
    [ProduceResponseType(typeof(WebhookResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> HandleEmailWebhook([FromBody] WebhookRequest request)
    {
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
    [ProduceResponseType(typeof(WebhookResponse), StatusCodes.Status200OK)]
    [ProduceResponseType(typeof(WebhookResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> HandleShippingWebhook([FromBody] WebhookRequest request)
    {
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
    [ProduceResponseType(typeof(WebhookResponse), StatusCodes.Status200OK)]
    [ProduceResponseType(typeof(WebhookResponse), StatusCodes.Status400BadRequest)]
    [ProduceResponseType(typeof(WebhookResponse), StatusCodes.Status401Unauthorized)]
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

        if (request == null || string.IsNullOrEmpty(request.Payload))
        {
            return BadRequest(new WebhookResponse
            {
                Acknowledged = false,
                ErrorCode = "INVALID_PAYLOAD",
                Message = "Payload cannot be empty"
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
