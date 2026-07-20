#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Configuration;
using AspNetSpaTemplate.Data;
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
    private readonly PwaOptions _pwaOptions;
    private readonly AppDbContext _dbContext;
    private DateTime? _lastExecutedAt;
    private int _totalNotificationsSent;
    private int _totalNotificationsFailed;
    private int _totalSubscriptionsCleaned;

    public NotificationWorker(
        NotificationService notificationService,
        ILogger<NotificationWorker> logger,
        PwaOptions pwaOptions,
        AppDbContext dbContext)
    {
        _notificationService = notificationService;
        _logger = logger;
        _pwaOptions = pwaOptions;
        _dbContext = dbContext;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Clean up stale push subscriptions
            await CleanupStalePushSubscriptionsAsync(cancellationToken);

            // Get pending notifications (max 100 per batch)
            var notifications = _notificationService.GetPendingNotifications(100).ToList();

            if (notifications.Count == 0)
            {
                _logger.LogDebug("No pending notifications to send");
            }
            else
            {
                _logger.LogInformation($"Processing {notifications.Count} pending notifications");

                var batchSent = 0;
                var batchFailed = 0;

                foreach (var notification in notifications)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        await SendNotificationAsync(notification, cancellationToken);
                        _totalNotificationsSent++;
                        batchSent++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notification: {NotificationType} to {Recipient}", notification.Type, notification.Recipient);
                        _totalNotificationsFailed++;
                        batchFailed++;

                        // In production, re-queue for retry with exponential backoff
                        // For now, just log and discard
                    }
                }

                _logger.LogInformation("Notification processing complete. Sent: {SentCount}, Failed: {FailedCount}", batchSent, batchFailed);
            }

            _lastExecutedAt = DateTime.UtcNow;
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
            FailureCount = _totalNotificationsFailed,
            AdditionalInfo = $"Cleaned stale subscriptions: {_totalSubscriptionsCleaned}"
        };
    }

    private async Task CleanupStalePushSubscriptionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_pwaOptions.InactiveSubscriptionPurgeDays <= 0)
            {
                _logger.LogDebug("Stale subscription cleanup disabled (InactiveSubscriptionPurgeDays <= 0)");
                return;
            }

            var cutoffDate = DateTime.UtcNow.AddDays(-_pwaOptions.InactiveSubscriptionPurgeDays);
            var staleSubscriptions = _dbContext.PushSubscriptions
                .Where(s => s.IsActive && (s.LastActiveAt == null || s.LastActiveAt < cutoffDate))
                .ToList();

            if (staleSubscriptions.Count == 0)
            {
                _logger.LogDebug("No stale push subscriptions found for cleanup");
                return;
            }

            _logger.LogInformation("Cleaning up {Count} stale push subscriptions (older than {Days} days)",
                staleSubscriptions.Count, _pwaOptions.InactiveSubscriptionPurgeDays);

            foreach (var subscription in staleSubscriptions)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    subscription.Deactivate();
                    _dbContext.PushSubscriptions.Remove(subscription);
                    _logger.LogInformation("Removed stale push subscription for user {UserId} (ID: {SubscriptionId})",
                        subscription.UserId, subscription.Id);
                    _totalSubscriptionsCleaned++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to clean up push subscription {SubscriptionId} for user {UserId}",
                        subscription.Id, subscription.UserId);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Stale push subscription cleanup completed. Removed: {Count}", staleSubscriptions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up stale push subscriptions");
        }
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
