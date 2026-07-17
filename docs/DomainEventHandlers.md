# DomainEventHandlers

Centralized event handler registry for domain events in the application. This class provides asynchronous handlers for core domain events such as product lifecycle changes, order processing, user registration, and review submissions. Handlers are registered once during application startup to decouple event producers from consumers.

## API

### `public DomainEventHandlers`

Static class containing all domain event handler methods. No instance members are exposed; all functionality is provided through static methods.

### `public async Task OnProductCreated(ProductCreatedEvent @event)`

Handles the `ProductCreatedEvent` domain event.
- **Parameters**: `@event` – The domain event containing product details.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `@event` is null.

### `public async Task OnProductUpdated(ProductUpdatedEvent @event)`

Handles the `ProductUpdatedEvent` domain event.
- **Parameters**: `@event` – The domain event containing updated product details.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `@event` is null.

### `public async Task OnProductDeleted(ProductDeletedEvent @event)`

Handles the `ProductDeletedEvent` domain event.
- **Parameters**: `@event` – The domain event containing the identifier of the deleted product.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `@event` is null.

### `public async Task OnOrderPlaced(OrderPlacedEvent @event)`

Handles the `OrderPlacedEvent` domain event.
- **Parameters**: `@event` – The domain event containing order details.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `@event` is null.

### `public async Task OnOrderCompleted(OrderCompletedEvent @event)`

Handles the `OrderCompletedEvent` domain event.
- **Parameters**: `@event` – The domain event containing order details.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `@event` is null.

### `public async Task OnOrderCancelled(OrderCancelledEvent @event)`

Handles the `OrderCancelledEvent` domain event.
- **Parameters**: `@event` – The domain event containing order details.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `@event` is null.

### `public async Task OnUserRegistered(UserRegisteredEvent @event)`

Handles the `UserRegisteredEvent` domain event.
- **Parameters**: `@event` – The domain event containing user registration details.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `@event` is null.

### `public async Task OnReviewSubmitted(ReviewSubmittedEvent @event)`

Handles the `ReviewSubmittedEvent` domain event.
- **Parameters**: `@event` – The domain event containing review details.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `@event` is null.

### `public static void RegisterEventHandlers(IServiceCollection services)`

Registers all domain event handlers with the dependency injection container.
- **Parameters**: `services` – The `IServiceCollection` instance to register handlers with.
- **Return value**: `void`.
- **Exceptions**: Throws `ArgumentNullException` if `services` is null.

## Usage

Register handlers during application startup:

```csharp
// In Program.cs or Startup.cs
services.RegisterEventHandlers(services);
```

Subscribe to domain events via the event bus:

```csharp
// Example: Publishing a ProductCreatedEvent
var productCreatedEvent = new ProductCreatedEvent(
    productId: Guid.NewGuid(),
    name: "Premium Headphones",
    price: 199.99m,
    stockQuantity: 50
);
await eventBus.PublishAsync(productCreatedEvent);
```

## Notes

Handlers are designed for fire-and-forget execution; failures should be logged and not propagated to the caller. Each handler is invoked sequentially by the event bus, so handlers should avoid long-running operations to prevent blocking subsequent events. Thread safety is ensured by the event bus implementation, which serializes event processing. Handlers should be idempotent where possible, as retries may occur after transient failures.
