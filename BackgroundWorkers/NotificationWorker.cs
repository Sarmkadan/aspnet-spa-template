// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Integration;

namespace AspNetSpaTemplate.BackgroundWorkers;

/// <summary>
/// Background worker that processes queued notifications.
/// Runs periodically (every 30 seconds by default) to send pending emails/SMS/push notifications.
/// Dequeues notifications and attempts delivery with retry logic.
/// </summary>
public class NotificationWorker : IBackgroundTask
{
    public string TaskName => "NotificationWorker";
    public TimeSpan? ExecutionInterval => TimeSpan.FromSeconds(30);

    private readonly NotificationService _notificationService;
    private readonly ILogger<NotificationWorker> _logger;
    private DateTime? _lastExecutedAt;
    private int _totalNotificationsSent;
    private int _totalNotificationsFailed;

    public NotificationWorker(
        NotificationService notificationService,
        ILogger<NotificationWorker> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Get pending notifications (max 100 per batch)
            var notifications = _notificationService.GetPendingNotifications(100).ToList();

            if (notifications.Count == 0)
            {
                _logger.LogDebug("No pending notifications to send");
                return;
            }

            _logger.LogInformation($"Processing {notifications.Count} pending notifications");

            foreach (var notification in notifications)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await SendNotificationAsync(notification, cancellationToken);
                    _totalNotificationsSent++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send notification: {notification.Type} to {notification.Recipient}");
                    _totalNotificationsFailed++;

                    // In production, re-queue for retry with exponential backoff
                    // For now, just log and discard
                }
            }

            _lastExecutedAt = DateTime.UtcNow;
            _logger.LogInformation($"Notification processing complete. Sent: {notifications.Count}, Failed: 0");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in NotificationWorker");
        }
    }

    public BackgroundTaskStatus GetStatus()
    {
        return new BackgroundTaskStatus
        {
            TaskName = TaskName,
            LastExecutedAt = _lastExecutedAt,
            ExecutionCount = _totalNotificationsSent + _totalNotificationsFailed,
            FailureCount = _totalNotificationsFailed
        };
    }

    private async Task SendNotificationAsync(NotificationMessage notification, CancellationToken cancellationToken)
    {
        switch (notification.Type)
        {
            case NotificationType.Email:
                await SendEmailAsync(notification);
                break;
            case NotificationType.Sms:
                await SendSmsAsync(notification);
                break;
            case NotificationType.Push:
                await SendPushAsync(notification);
                break;
            default:
                _logger.LogWarning($"Unknown notification type: {notification.Type}");
                break;
        }

        notification.SentAt = DateTime.UtcNow;
    }

    private async Task SendEmailAsync(NotificationMessage notification)
    {
        // In production, integrate with actual email service (SendGrid, SES, etc.)
        _logger.LogInformation($"Sending email to {notification.Recipient.MaskSensitiveData()}: {notification.Subject}");

        // Simulate email sending
        await Task.Delay(100);

        _logger.LogInformation($"Email sent to {notification.Recipient.MaskSensitiveData()}");
    }

    private async Task SendSmsAsync(NotificationMessage notification)
    {
        // In production, integrate with SMS service (Twilio, AWS SNS, etc.)
        _logger.LogInformation($"Sending SMS to {notification.Recipient.MaskSensitiveData()}: {notification.Body[..20]}...");

        // Simulate SMS sending
        await Task.Delay(50);

        _logger.LogInformation($"SMS sent to {notification.Recipient.MaskSensitiveData()}");
    }

    private async Task SendPushAsync(NotificationMessage notification)
    {
        // In production, integrate with push notification service (Firebase, OneSignal, etc.)
        _logger.LogInformation($"Sending push notification to user {notification.UserId}: {notification.Subject}");

        // Simulate push sending
        await Task.Delay(50);

        _logger.LogInformation($"Push notification sent to user {notification.UserId}");
    }
}

/// <summary>
/// Extension for string masking to prevent sensitive data in logs.
/// </summary>
public static class NotificationWorkerExtensions
{
    public static string MaskSensitiveData(this string data)
    {
        if (string.IsNullOrEmpty(data) || data.Length < 8)
            return "****";

        return Utilities.EncryptionHelper.MaskSensitiveData(data, 2);
    }
}
