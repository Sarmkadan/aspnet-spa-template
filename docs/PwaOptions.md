# PwaOptions

The `PwaOptions` class serves as a configuration container for Progressive Web App (PWA) features within the ASP.NET SPA template, specifically managing push notification settings via VAPID keys and offline synchronization parameters. It encapsulates both security credentials required for web push protocols and operational thresholds that govern retry logic, batching limits, and subscription lifecycle management, allowing developers to tune the behavior of background sync and notification delivery without altering core service implementations.

## API

### Vapid
Gets or sets the `VapidOptions` instance containing the cryptographic keys and contact information required for Web Push protocol authentication. This property is used by the push service to sign requests and identify the application server.

### EnablePushNotifications
Gets or sets a boolean value indicating whether the push notification subsystem is active. When set to `false`, the application will not attempt to register service workers for push or send notifications, effectively disabling the feature regardless of other configured values.

### EnableOfflineSync
Gets or sets a boolean value that enables or disables the offline synchronization queue. If `true`, failed requests or data mutations performed while offline are queued for later transmission; if `false`, offline operations may fail immediately or be discarded depending on the client implementation.

### MaxNotificationsPerBatch
Gets or sets the maximum number of notifications the server will attempt to send in a single batch operation. This integer limits throughput to prevent overwhelming the push service or triggering rate-limiting errors during high-volume events.

### MaxSyncRetries
Gets or sets the maximum number of retry attempts for a failed synchronization task before it is marked as permanently failed. This integer value determines the resilience of the offline sync mechanism against transient network errors.

### SyncRetryBaseDelaySeconds
Gets or sets the base delay, in seconds, used for calculating the wait time between synchronization retry attempts. This value is typically used in an exponential backoff algorithm where the actual delay increases with each subsequent retry.

### SyncQueueMaxAgeHours
Gets or sets the maximum age, in hours, that a synchronized item can remain in the queue before it is considered stale and automatically purged. This prevents the system from attempting to sync outdated data that may no longer be relevant to the current application state.

### PushDeliveryTimeoutSeconds
Gets or sets the timeout duration, in seconds, for HTTP requests made to the Web Push service when delivering notifications. If a push server does not respond within this timeframe, the delivery attempt is aborted and may trigger a retry based on `MaxSyncRetries`.

### InactiveSubscriptionPurgeDays
Gets or sets the number of days a subscription can remain inactive (e.g., failing to acknowledge pushes) before it is automatically removed from the database. This maintenance setting helps keep the subscription store clean and reduces costs associated with sending to invalid endpoints.

### PublicKey
Gets or sets the string representation of the VAPID public key. This key is exposed to the client-side application to facilitate the subscription process with the browser's push manager.

### PrivateKey
Gets or sets the string representation of the VAPID private key. This secret value is used server-side to sign JWTs for authenticating push messages and must be kept secure; it is never transmitted to the client.

### Subject
Gets or sets the subject string (typically a `mailto:` URL) used in the VAPID JWT payload. This identifies the application owner to the push service and is required for compliance with the Web Push protocol specification.

## Usage

### Example 1: Basic Configuration in Startup
The following example demonstrates how to configure `PwaOptions` within the `Program.cs` or `Startup.cs` file to enable push notifications with specific VAPID credentials and standard retry logic.

```csharp
builder.Services.Configure<PwaOptions>(options =>
{
    options.EnablePushNotifications = true;
    options.EnableOfflineSync = true;
    
    // VAPID Configuration
    options.PublicKey = "BKx..."; 
    options.PrivateKey = "34r...";
    options.Subject = "mailto:admin@example.com";
    
    // Operational Limits
    options.MaxNotificationsPerBatch = 50;
    options.MaxSyncRetries = 3;
    options.SyncRetryBaseDelaySeconds = 5;
    options.PushDeliveryTimeoutSeconds = 30;
    options.InactiveSubscriptionPurgeDays = 14;
    options.SyncQueueMaxAgeHours = 48;
});
```

### Example 2: Conditional Feature Toggling
This example illustrates retrieving the options via dependency injection to conditionally execute logic based on whether offline sync is enabled, while dynamically adjusting batch sizes based on environment load.

```csharp
public class NotificationService
{
    private readonly PwaOptions _options;

    public NotificationService(IOptions<PwaOptions> options)
    {
        _options = options.Value;
    }

    public async Task DispatchAlertsAsync(List<Notification> alerts)
    {
        if (!_options.EnablePushNotifications)
        {
            return;
        }

        var batchSize = _options.MaxNotificationsPerBatch;
        if (Environment.GetEnvironmentVariable("ENV") == "Development")
        {
            // Reduce batch size in development for easier debugging
            batchSize = Math.Min(batchSize, 5);
        }

        foreach (var batch in alerts.Chunk(batchSize))
        {
            await SendBatchAsync(batch, _options.PushDeliveryTimeoutSeconds);
        }
    }
}
```

## Notes

*   **Thread Safety**: The `PwaOptions` class is typically registered as a singleton configuration object. While reading primitive properties (int, bool, string) is thread-safe in .NET, the `VapidOptions` object referenced by the `Vapid` property should be treated as immutable after application startup to avoid race conditions during cryptographic operations.
*   **Key Security**: The `PrivateKey` property contains sensitive cryptographic material. Ensure that this value is populated via secure configuration providers (such as Azure Key Vault or User Secrets) rather than hardcoded source control values.
*   **Retry Logic Overflow**: When configuring `SyncRetryBaseDelaySeconds` and `MaxSyncRetries`, be aware that if an exponential backoff strategy is implemented downstream, large values for both properties could result in total wait times spanning several hours before a job is finally abandoned.
*   **Data Consistency**: Setting `SyncQueueMaxAgeHours` to a value lower than the expected maximum offline duration for mobile users may result in legitimate data loss if the device remains disconnected longer than the configured threshold.
*   **Validation**: The class does not enforce validation attributes internally. It is the responsibility of the consuming service to verify that `PublicKey` and `PrivateKey` are valid Base64Url-encoded strings and that `Subject` follows the URI format before attempting to generate VAPID signatures.
