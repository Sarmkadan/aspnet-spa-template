# NotificationWorker

`NotificationWorker` is an `IBackgroundTask` that drains queued notifications from `NotificationService`. Each execution first removes active push subscriptions whose `LastActiveAt` is missing or older than the configured `PwaOptions.InactiveSubscriptionPurgeDays`, unless that option is zero or negative. It then dequeues at most 100 notifications and dispatches email, SMS, and push messages. The current delivery methods simulate their respective transports with short delays; they do not call an external delivery provider.

Individual notification failures are logged and do not stop the rest of the batch. Consecutive failures add a cancellation-aware delay of one second per failure, capped at five seconds. Invalid notification data is identified as a poison message for logging, but dequeued failures are not requeued or moved to a dead-letter store. After dispatch, the message's `SentAt` value is set. `GetStatus()` reports the last completed execution time, sent and failed totals, stale-subscription cleanup count, and consecutive failures.

## Scheduling and lifecycle

The task name is `NotificationWorker`, and `ExecutionInterval` is 30 seconds. The worker does not run its own loop: a registered `BackgroundTaskScheduler` invokes it. A newly registered task is eligible on the scheduler's first one-second polling cycle. Once an execution finishes or throws, its next run is scheduled for 30 seconds after that completion, and an atomic running flag prevents overlapping executions of the same task. The scheduler can also trigger the task immediately by name; that trigger is ignored if the task is already running.

The host's stopping token is passed into `ExecuteAsync`. The worker checks it before each notification and passes it to cleanup, delivery delays, and failure backoff. Cancellation is logged and rethrown so the scheduler can observe it. Other execution-level errors are also logged and rethrown; cleanup errors are logged inside the cleanup operation and do not prevent notification processing.

## Usage

With the application's notification and background-task services configured, queue a notification and execute the registered worker with a cancellation token:

```csharp
var notifications = app.Services.GetRequiredService<NotificationService>();
var worker = app.Services.GetRequiredService<NotificationWorker>();

await notifications.SendEmailAsync(
    "customer@example.com",
    "Order received",
    "<p>Thanks for your order.</p>");

await worker.ExecuteAsync(CancellationToken.None);
```

In normal hosted use, the scheduler calls `ExecuteAsync` according to the worker's 30-second interval rather than application code calling it directly.
