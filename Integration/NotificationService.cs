// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Utilities;

namespace AspNetSpaTemplate.Integration;

/// <summary>
/// Service for sending notifications via multiple channels (email, SMS, push).
/// Abstracts notification implementation details from business logic.
/// Queues notifications for async processing to avoid blocking requests.
/// </summary>
public class NotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly Queue<NotificationMessage> _notificationQueue = new();
    private readonly object _queueLock = new();

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Sends email notification (queued for async processing).
    /// Returns immediately without waiting for actual delivery.
    /// </summary>
    public async Task SendEmailAsync(string recipient, string subject, string htmlBody)
    {
        ValidateEmail(recipient);

        var notification = new NotificationMessage
        {
            Type = NotificationType.Email,
            Recipient = recipient,
            Subject = subject,
            Body = htmlBody,
            QueuedAt = DateTime.UtcNow
        };

        QueueNotification(notification);
        _logger.LogInformation($"Email queued for {recipient}: {subject}");
    }

    /// <summary>
    /// Sends SMS notification (queued).
    /// </summary>
    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        ValidatePhoneNumber(phoneNumber);

        if (message.Length > 160)
            throw new ArgumentException("SMS message exceeds 160 characters");

        var notification = new NotificationMessage
        {
            Type = NotificationType.Sms,
            Recipient = phoneNumber,
            Body = message,
            QueuedAt = DateTime.UtcNow
        };

        QueueNotification(notification);
        _logger.LogInformation($"SMS queued for {phoneNumber}");
    }

    /// <summary>
    /// Sends push notification (to in-app notification system).
    /// </summary>
    public async Task SendPushAsync(int userId, string title, string message, string? deepLink = null)
    {
        var notification = new NotificationMessage
        {
            Type = NotificationType.Push,
            UserId = userId,
            Subject = title,
            Body = message,
            Data = deepLink,
            QueuedAt = DateTime.UtcNow
        };

        QueueNotification(notification);
        _logger.LogInformation($"Push notification queued for user {userId}: {title}");
    }

    /// <summary>
    /// Sends order confirmation email with template.
    /// </summary>
    public async Task SendOrderConfirmationAsync(string email, int orderId, decimal total)
    {
        var htmlBody = $@"
            <h1>Order Confirmation</h1>
            <p>Thank you for your order!</p>
            <p><strong>Order ID:</strong> {orderId}</p>
            <p><strong>Total:</strong> ${total:F2}</p>
            <p>We'll send you tracking information once your order ships.</p>
        ";

        await SendEmailAsync(email, $"Order Confirmation #{orderId}", htmlBody);
    }

    /// <summary>
    /// Sends password reset email with link.
    /// </summary>
    public async Task SendPasswordResetAsync(string email, string resetToken)
    {
        var resetLink = $"https://yourapp.com/reset-password?token={resetToken}";
        var htmlBody = $@"
            <h1>Password Reset</h1>
            <p>Click the link below to reset your password:</p>
            <p><a href='{resetLink}'>Reset Password</a></p>
            <p>This link expires in 1 hour.</p>
        ";

        await SendEmailAsync(email, "Password Reset Request", htmlBody);
    }

    /// <summary>
    /// Dequeues and processes pending notifications.
    /// Called by background worker to actually deliver notifications.
    /// </summary>
    public IEnumerable<NotificationMessage> GetPendingNotifications(int batchSize = 10)
    {
        lock (_queueLock)
        {
            var pending = new List<NotificationMessage>();
            for (int i = 0; i < batchSize && _notificationQueue.Count > 0; i++)
            {
                pending.Add(_notificationQueue.Dequeue());
            }
            return pending;
        }
    }

    /// <summary>
    /// Gets current queue size (for monitoring).
    /// </summary>
    public int GetQueueSize()
    {
        lock (_queueLock)
        {
            return _notificationQueue.Count;
        }
    }

    private void QueueNotification(NotificationMessage notification)
    {
        lock (_queueLock)
        {
            _notificationQueue.Enqueue(notification);
        }
    }

    private void ValidateEmail(string email)
    {
        if (!email.IsValidEmail())
            throw new ArgumentException($"Invalid email: {email}");
    }

    private void ValidatePhoneNumber(string phoneNumber)
    {
        ValidationHelper.ValidPhoneNumber(phoneNumber);
    }
}

/// <summary>
/// Notification message envelope for queuing.
/// </summary>
public class NotificationMessage
{
    public NotificationType Type { get; set; }
    public string Recipient { get; set; } = "";
    public int? UserId { get; set; }
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public string? Data { get; set; }
    public DateTime QueuedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsRetry { get; set; }
}

public enum NotificationType
{
    Email,
    Sms,
    Push,
    InApp
}
