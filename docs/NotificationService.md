# NotificationService

The `NotificationService` component provides a unified API for sending asynchronous notifications via email, SMS, and push channels, as well as for querying the internal notification queue. It is intended to be instantiated per‑request or per‑scope and configured through its properties before invoking the appropriate send method.

## API

### Constructor

```csharp
public NotificationService()
```
Creates a new instance with default property values. All properties are initialized to their default states (`null` for reference types, `0` for numeric types, `DateTime.MinValue` for dates, and `false` for flags).

### Methods

#### SendEmailAsync
```csharp
public async Task SendEmailAsync()
```
Sends an email notification using the current property values (`Recipient`, `Subject`, `Body`, `Data`, etc.).  
- **Parameters:** None.  
- **Return value:** A `Task` that completes when the email has been handed off to the underlying email provider.  
- **Exceptions:**  
  - `ArgumentException` if `Recipient` is null, empty, or not a valid email format.  
  - `InvalidOperationException` if the email provider has not been configured.  
  - `OperationCanceledException` if the associated cancellation token is triggered (if any).

#### SendSmsAsync
```csharp
public async Task SendSmsAsync()
```
Sends an SMS notification using the current property values (`Recipient`, `Body`, `Data`, etc.).  
- **Parameters:** None.  
- **Return value:** A `Task` that completes when the SMS has been handed off to the SMS gateway.  
- **Exceptions:**  
  - `ArgumentException` if `Recipient` is null, empty, or not a valid phone number.  
  - `InvalidOperationException` if the SMS provider is not configured.  
  - `OperationCanceledException` on cancellation.

#### SendPushAsync
```csharp
public async Task SendPushAsync()
```
Sends a push notification using the current property values (`Recipient`, `Subject`, `Body`, `Data`, etc.).  
- **Parameters:** None.  
- **Return value:** A `Task` that completes when the push notification has been handed off to the push service.  
- **Exceptions:**  
  - `ArgumentException` if `Recipient` is null or empty.  
  - `InvalidOperationException` if the push service is not configured.  
  - `OperationCanceledException` on cancellation.

#### SendOrderConfirmationAsync
```csharp
public async Task SendOrderConfirmationAsync()
```
Sends a specialized order‑confirmation notification (typically email) using the current property values.  
- **Parameters:** None.  
- **Return value:** A `Task` that completes when the confirmation has been queued for delivery.  
- **Exceptions:**  
  - `ArgumentException` if required fields such as `Recipient` or `Body` are missing.  
  - `InvalidOperationException` if the service lacks an email sender.  
  - `OperationCanceledException` on cancellation.

#### SendPasswordResetAsync
```csharp
public async Task SendPasswordResetAsync()
```
Sends a password‑reset notification (typically email) using the current property values.  
- **Parameters:** None.  
- **Return value:** A `Task` that completes when the reset message has been queued.  
- **Exceptions:**  
  - `ArgumentException` if `Recipient` or `Body` is null/empty.  
  - `InvalidOperationException` if the email sender is unavailable.  
  - `OperationCanceledException` on cancellation.

#### GetPendingNotifications
```csharp
public IEnumerable<NotificationMessage> GetPendingNotifications()
```
Retrieves a snapshot of notifications that are currently waiting to be processed.  
- **Parameters:** None.  
- **Return value:** An `IEnumerable<NotificationMessage>` representing the queued items. The enumeration is a snapshot; modifications to the queue after the call do not affect the returned sequence.  
- **Exceptions:** None under normal operation.

#### GetQueueSize
```csharp
public int GetQueueSize()
```
Returns the number of notifications currently awaiting processing.  
- **Parameters:** None.  
- **Return value:** An `int` indicating the queue length.  
- **Exceptions:** None.

### Properties

#### Type
```csharp
public NotificationType Type { get; set; }
```
Gets or sets the type of notification to send (e.g., Email, Sms, Push). Used by the send methods to determine the delivery channel.

#### Recipient
```csharp
public string Recipient { get; set; }
```
Gets or sets the destination address or identifier for the notification (email address, phone number, or push token).

#### UserId
```csharp
public int? UserId { get; set; }
```
Gets or sets the optional identifier of the user associated with the notification. May be null if the notification is not user‑specific.

#### Subject
```csharp
public string Subject { get; set; }
```
Gets or sets the subject line for email‑style notifications; ignored for SMS and push unless the underlying service utilizes it.

#### Body
```csharp
public string Body { get; set; }
```
Gets or sets the main content of the notification. Required for all notification types.

#### Data
```csharp
public string? Data { get; set; }
```
Gets or sets an optional payload (often JSON) that can be interpreted by the receiving service or client.

#### QueuedAt
```csharp
public DateTime QueuedAt { get; set; }
```
Gets or sets the timestamp when the notification was placed into the queue. Typically set automatically by the service.

#### SentAt
```csharp
public DateTime? SentAt { get; set; }
```
Gets or sets the timestamp when the notification was successfully sent; null if not yet sent or if sending failed.

#### IsRetry
```csharp
public bool IsRetry { get; set; }
```
Gets or sets a flag indicating whether the current attempt is a retry of a previously failed notification.

## Usage

### Sending an email notification
```csharp
var notifier = new NotificationService
{
    Type       = NotificationType.Email,
    Recipient  = "user@example.com",
    Subject    = "Your order has been shipped",
    Body       = "Dear customer, your order #12345 is on its way.",
    QueuedAt   = DateTime.UtcNow
};

await notifier.SendEmailAsync();
```

### Processing pending notifications
```csharp
var notifier = new NotificationService();

foreach (var msg in notifier.GetPendingNotifications())
{
    // Example routing based on Type
    switch (msg.Type)
    {
        case NotificationType.Email:
            await notifier with { Recipient = msg.Recipient, Subject = msg.Subject, Body = msg.Body, Data = msg.Data }
                         .SendEmailAsync();
            break;
        case NotificationType.Sms:
            await notifier with { Recipient = msg.Recipient, Body = msg.Body, Data = msg.Data }
                         .SendSmsAsync();
            break;
        case NotificationType.Push:
            await notifier with { Recipient = msg.Recipient, Subject = msg.Subject, Body = msg.Body, Data = msg.Data }
                         .SendPushAsync();
            break;
    }

    // Mark as sent (pseudo‑code)
    msg.SentAt = DateTime.UtcNow;
}
```

## Notes

- The instance is **not thread‑safe**. Concurrent modification of properties such as `Recipient` or `Body` while a send method is executing may result in undefined behavior. For concurrent usage, create separate instances or synchronize access externally.  
- All send methods rely on the current property values; failing to set required properties before invocation will cause an `ArgumentException`.  
- `GetPendingNotifications` returns a snapshot; the underlying queue may change immediately after the call, so the returned enumeration should not be relied upon for precise queue length—use `GetQueueSize` for that purpose.  
- The `Data` property is optional and may be null; services that consume it should handle null gracefully.  
- Setting `IsRetry` to `true` does not automatically alter retry logic; it is merely a flag that callers may inspect to implement custom retry policies.  
- Date‑time values (`QueuedAt`, `SentAt`) should be supplied in UTC to avoid timezone‑related inconsistencies.  
- If a notification fails to send, the service does not automatically resend based on `IsRetry`; retry handling must be implemented by the caller.
