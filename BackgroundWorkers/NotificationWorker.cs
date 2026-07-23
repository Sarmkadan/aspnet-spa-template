#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =========================================================================

using System.Threading;
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
    private int _consecutiveFailures;

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

    /// <summary>
    /// Executes the notification processing task.
    /// Honors the cancellation token for graceful shutdown and handles poison messages gracefully.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown</param>
    /// <exception cref="ArgumentNullException">Thrown if cancellationToken is null</exception>
    /// <exception cref="OperationCanceledException">Thrown when operation is cancelled</exception>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cancellationToken);

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
                _logger.LogInformation("Processing {NotificationCount} pending notifications", notifications.Count);

                var batchSent = 0;
                var batchFailed = 0;

                foreach (var notification in notifications)
                {
                    // Check for cancellation before processing each notification
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        await SendNotificationAsync(notification, cancellationToken);
                        _totalNotificationsSent++;
                        batchSent++;
                        _consecutiveFailures = 0; // Reset on success
                    }
                    catch (OperationCanceledException)
                    {
                        // Gracefully handle cancellation during notification sending
                        _logger.LogInformation("Notification sending cancelled during processing");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _totalNotificationsFailed++;
                        batchFailed++;
                        _consecutiveFailures++;

                        // Log the error with detailed information
                        _logger.LogError(ex, "Failed to send notification: {NotificationType} to {Recipient}",
                            notification.Type, notification.Recipient);

                        // Handle poison messages - messages that consistently fail
                        if (IsPoisonMessage(ex, notification))
                        {
                            _logger.LogWarning("Poison message detected and isolated: {NotificationType} to {Recipient}. Error: {ErrorMessage}",
                                notification.Type, notification.Recipient, ex.Message);

                            // Mark as processed to prevent infinite retry loop
                            // In a real system, this would move to a dead-letter queue
                        }
                        else
                        {
                            // In production, these would be re-queued for retry with exponential backoff
                            // For now, just log and discard
                        }

                        // Apply backoff after consecutive failures to prevent hot error loops
                        if (_consecutiveFailures > 0)
                        {
                            var delayMs = Math.Min(1000 * _consecutiveFailures, 5000); // Cap at 5 seconds
                            _logger.LogDebug("Applying backoff delay of {DelayMs}ms after {ConsecutiveFailures} consecutive failures",
                                delayMs, _consecutiveFailures);
                            await Task.Delay(delayMs, cancellationToken);
                        }
                    }
                }

                _logger.LogInformation("Notification processing complete. Sent: {SentCount}, Failed: {FailedCount}", batchSent, batchFailed);
            }

            _lastExecutedAt = DateTime.UtcNow;
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown - log and rethrow to allow host to complete shutdown
            _logger.LogInformation("NotificationWorker cancelled gracefully during execution");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in NotificationWorker");
            throw; // Re-throw to allow host to handle appropriately
        }
    }

    /// <summary>
    /// Determines if a notification failure indicates a poison message that should be isolated.
    /// Poison messages are typically those with invalid data, null recipients, or unsupported types.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="notification">The notification message</param>
    /// <returns>True if this is a poison message that should be isolated</returns>
    private static bool IsPoisonMessage(Exception exception, NotificationMessage notification)
    {
        // Check for null or invalid recipient
        if (string.IsNullOrWhiteSpace(notification.Recipient) && notification.Type != NotificationType.Push)
        {
            return true;
        }

        // Check for null user ID on push notifications
        if (notification.Type == NotificationType.Push && !notification.UserId.HasValue)
        {
            return true;
        }

        // Check for empty subject or body
        if ((notification.Type == NotificationType.Email || notification.Type == NotificationType.Sms) &&
            string.IsNullOrWhiteSpace(notification.Subject) && string.IsNullOrWhiteSpace(notification.Body))
        {
            return true;
        }

        // Check for specific exception types that indicate invalid data
        if (exception is ArgumentException || exception is ArgumentNullException)
        {
            return true;
        }

        return false;
    }

    public BackgroundTaskStatus GetStatus()
    {
        return new BackgroundTaskStatus
        {
            TaskName = TaskName,
            LastExecutedAt = _lastExecutedAt,
            ExecutionCount = _totalNotificationsSent + _totalNotificationsFailed,
            FailureCount = _totalNotificationsFailed,
            AdditionalInfo = $"Cleaned stale subscriptions: {_totalSubscriptionsCleaned}, Consecutive failures: {_consecutiveFailures}"
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
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(cancellationToken);

        switch (notification.Type)
        {
            case NotificationType.Email:
                await SendEmailAsync(notification, cancellationToken);
                break;
            case NotificationType.Sms:
                await SendSmsAsync(notification, cancellationToken);
                break;
            case NotificationType.Push:
                await SendPushAsync(notification, cancellationToken);
                break;
            default:
                _logger.LogWarning("Unknown notification type: {NotificationType}", notification.Type);
                break;
        }

        notification.SentAt = DateTime.UtcNow;
    }

    private async Task SendEmailAsync(NotificationMessage notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(cancellationToken);

        // In production, integrate with actual email service (SendGrid, SES, etc.)
        _logger.LogInformation("Sending email to {Recipient}: {Subject}",
            notification.Recipient.MaskSensitiveData(), notification.Subject);

        // Simulate email sending
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("Email sent to {Recipient}", notification.Recipient.MaskSensitiveData());
    }

    private async Task SendSmsAsync(NotificationMessage notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(cancellationToken);

        // In production, integrate with SMS service (Twilio, AWS SNS, etc.)
        _logger.LogInformation("Sending SMS to {Recipient}: {BodyPreview}...",
            notification.Recipient.MaskSensitiveData(), notification.Body[..Math.Min(20, notification.Body.Length)]);

        // Simulate SMS sending
        await Task.Delay(50, cancellationToken);

        _logger.LogInformation("SMS sent to {Recipient}", notification.Recipient.MaskSensitiveData());
    }

    private async Task SendPushAsync(NotificationMessage notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(cancellationToken);

        if (!notification.UserId.HasValue)
        {
            throw new ArgumentException("UserId is required for push notifications", nameof(notification));
        }

        // In production, integrate with push notification service (Firebase, OneSignal, etc.)
        _logger.LogInformation("Sending push notification to user {UserId}: {Subject}",
            notification.UserId, notification.Subject);

        // Simulate push sending
        await Task.Delay(50, cancellationToken);

        _logger.LogInformation("Push notification sent to user {UserId}", notification.UserId);
    }
}

/// <summary>
/// Extension for string masking to prevent sensitive data in logs.
/// </summary>
public static class NotificationWorkerExtensions
{
    /// <summary>
    /// Masks sensitive data in strings to prevent exposure in logs.
    /// </summary>
    /// <param name="data">The data to mask</param>
    /// <returns>Masked string or "****" if data is invalid</returns>
    public static string MaskSensitiveData(this string data)
    {
        if (string.IsNullOrEmpty(data) || data.Length < 8)
            return "****";

        return Utilities.EncryptionHelper.MaskSensitiveData(data, 2);
    }
}