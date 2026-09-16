# DomainEventHandlers

The `DomainEventHandlers` class contains the application's strongly typed handlers for product, order, user, and review domain events. It connects event-bus notifications to application side effects such as cache invalidation and logging while keeping event publishers independent of those concerns.

`DomainEventHandlers` is a sealed service. Its constructor requires an `ICacheService`, a `NotificationService`, and an `ILogger<DomainEventHandlers>`; each dependency is validated and an `ArgumentNullException` is thrown when any dependency is `null`.

## API

The class exposes the following public members.

| Member | Description |
|--------|-------------|
| `DomainEventHandlers(ICacheService cacheService, NotificationService notificationService, ILogger<DomainEventHandlers> logger)` | Creates the handler service with its cache, notification, and logging dependencies. |
| `Task OnProductCreated(ProductCreatedEvent @event)` | Logs the new product and invalidates the cached product lists. |
| `Task OnProductUpdated(ProductUpdatedEvent @event)` | Invalidates the individual product cache entry and the cached product lists. |
| `Task OnProductDeleted(ProductDeletedEvent @event)` | Invalidates the individual product cache entry and the cached product lists. |
| `Task OnOrderPlaced(OrderPlacedEvent @event)` | Logs that the order was placed and that its notification was queued. |
| `Task OnOrderCompleted(OrderCompletedEvent @event)` | Logs that the order was completed. |
| `Task OnOrderCancelled(OrderCancelledEvent @event)` | Logs the cancellation and its reason. |
| `Task OnUserRegistered(UserRegisteredEvent @event)` | Logs the registered user's identifier and email address. |
| `Task OnReviewSubmitted(ReviewSubmittedEvent @event)` | Invalidates the affected product and product-review cache entries. |

Every handler throws `ArgumentNullException` when its event argument is `null`. Exceptions raised while processing a non-null event are caught and logged, so they are not propagated to the event bus.

The same source file also defines the `EventHandlerExtensions` static class:

| Member | Description |
|--------|-------------|
| `void RegisterEventHandlers(this IServiceCollection services, IEventBus eventBus)` | Resolves `DomainEventHandlers` from a service provider and subscribes all eight handler methods to their corresponding event types. |

`RegisterEventHandlers` does not explicitly validate its arguments. A null argument or a missing `DomainEventHandlers` registration therefore fails through the service-resolution or subscription operations it performs.

## Usage

Register `DomainEventHandlers` and its dependencies with the service collection, then connect the handlers to the event bus:

```csharp
services.AddSingleton<DomainEventHandlers>();

var eventBus = serviceProvider.GetRequiredService<IEventBus>();
services.RegisterEventHandlers(eventBus);
```

Once registered, publishing a matching event invokes its handler:

```csharp
await eventBus.PublishAsync(new ProductCreatedEvent
{
    ProductId = 42,
    ProductName = "Premium Headphones",
    Price = 199.99m
});
```

## Notes

- Product and review handlers perform cache invalidation; the current order and user handlers only log their work and contain placeholders for future notification or workflow integration.
- The injected `NotificationService` is retained for those notification scenarios but is not currently called by the public handlers.
- Handler delegates are subscribed to the event bus by event type. The event bus invokes subscribers sequentially.
- Operational failures are logged and suppressed by each handler. Callers can rely on a completed task after a handled failure, but should inspect logs for errors.
