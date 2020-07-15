// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Caching;
using AspNetSpaTemplate.Integration;

namespace AspNetSpaTemplate.Events;

/// <summary>
/// Event handlers for business logic triggered by domain events.
/// These are registered in the DI container and called by the event bus.
/// Keeps business logic decoupled from event publishing.
/// </summary>
public class DomainEventHandlers
{
    private readonly ICacheService _cacheService;
    private readonly NotificationService _notificationService;
    private readonly ILogger<DomainEventHandlers> _logger;

    public DomainEventHandlers(
        ICacheService cacheService,
        NotificationService notificationService,
        ILogger<DomainEventHandlers> logger)
    {
        _cacheService = cacheService;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Handles product created event.
    /// Invalidates product list cache.
    /// </summary>
    public async Task OnProductCreated(ProductCreatedEvent @event)
    {
        try
        {
            _logger.LogInformation($"Handling ProductCreatedEvent: {@event.ProductId} - {@event.ProductName}");

            // Invalidate product cache
            await _cacheService.RemoveByPatternAsync(CacheKeyBuilder.InvalidationPatterns.AllProducts);

            // Could also log to analytics, update search index, etc.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ProductCreatedEvent");
        }
    }

    /// <summary>
    /// Handles product updated event.
    /// Invalidates product cache.
    /// </summary>
    public async Task OnProductUpdated(ProductUpdatedEvent @event)
    {
        try
        {
            _logger.LogInformation($"Handling ProductUpdatedEvent: {@event.ProductId}");

            // Invalidate specific product cache
            await _cacheService.RemoveAsync(CacheKeyBuilder.ProductById(@event.ProductId));
            await _cacheService.RemoveByPatternAsync(CacheKeyBuilder.InvalidationPatterns.AllProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ProductUpdatedEvent");
        }
    }

    /// <summary>
    /// Handles product deleted event.
    /// Invalidates product cache.
    /// </summary>
    public async Task OnProductDeleted(ProductDeletedEvent @event)
    {
        try
        {
            _logger.LogInformation($"Handling ProductDeletedEvent: {@event.ProductId}");

            // Invalidate product cache
            await _cacheService.RemoveAsync(CacheKeyBuilder.ProductById(@event.ProductId));
            await _cacheService.RemoveByPatternAsync(CacheKeyBuilder.InvalidationPatterns.AllProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ProductDeletedEvent");
        }
    }

    /// <summary>
    /// Handles order placed event.
    /// Sends confirmation email and updates analytics.
    /// </summary>
    public async Task OnOrderPlaced(OrderPlacedEvent @event)
    {
        try
        {
            _logger.LogInformation($"Handling OrderPlacedEvent: {@event.OrderId} for user {@event.UserId}");

            // Queue order confirmation email
            // In real scenario, would fetch user email from database
            // await _notificationService.SendOrderConfirmationAsync(userEmail, @event.OrderId, @event.TotalAmount);

            // Could also: log to analytics, trigger fulfillment process, etc.
            _logger.LogInformation($"Order placed notification queued for order {@event.OrderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OrderPlacedEvent");
        }
    }

    /// <summary>
    /// Handles order completed event.
    /// Sends completion notification.
    /// </summary>
    public async Task OnOrderCompleted(OrderCompletedEvent @event)
    {
        try
        {
            _logger.LogInformation($"Handling OrderCompletedEvent: {@event.OrderId}");

            // Could send delivery notification, trigger shipment, etc.
            _logger.LogInformation($"Order {@event.OrderId} marked as completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OrderCompletedEvent");
        }
    }

    /// <summary>
    /// Handles order cancelled event.
    /// Sends cancellation notification and reverses inventory.
    /// </summary>
    public async Task OnOrderCancelled(OrderCancelledEvent @event)
    {
        try
        {
            _logger.LogInformation($"Handling OrderCancelledEvent: {@event.OrderId}, reason: {@event.Reason}");

            // Could trigger refund, restore inventory, send cancellation email, etc.
            _logger.LogInformation($"Order {@event.OrderId} cancelled: {@event.Reason}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OrderCancelledEvent");
        }
    }

    /// <summary>
    /// Handles user registered event.
    /// Sends welcome email and logs user activity.
    /// </summary>
    public async Task OnUserRegistered(UserRegisteredEvent @event)
    {
        try
        {
            _logger.LogInformation($"Handling UserRegisteredEvent: {@event.UserId} - {@event.Email}");

            // Queue welcome email
            // await _notificationService.SendEmailAsync(@event.Email, "Welcome!", "Thank you for registering...");

            // Could also: trigger onboarding flow, send verification email, log analytics, etc.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UserRegisteredEvent");
        }
    }

    /// <summary>
    /// Handles review submitted event.
    /// Updates product rating cache.
    /// </summary>
    public async Task OnReviewSubmitted(ReviewSubmittedEvent @event)
    {
        try
        {
            _logger.LogInformation($"Handling ReviewSubmittedEvent: {@event.ReviewId} for product {@event.ProductId}");

            // Invalidate product cache (rating changed)
            await _cacheService.RemoveAsync(CacheKeyBuilder.ProductById(@event.ProductId));
            await _cacheService.RemoveAsync(CacheKeyBuilder.ReviewsByProductId(@event.ProductId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ReviewSubmittedEvent");
        }
    }
}

/// <summary>
/// Extension method to register all event handlers in DI container.
/// </summary>
public static class EventHandlerExtensions
{
    public static void RegisterEventHandlers(this IServiceCollection services, IEventBus eventBus)
    {
        var handlerProvider = services.BuildServiceProvider();
        var handlers = handlerProvider.GetRequiredService<DomainEventHandlers>();

        // Subscribe handlers to their respective events
        eventBus.Subscribe<ProductCreatedEvent>(handlers.OnProductCreated);
        eventBus.Subscribe<ProductUpdatedEvent>(handlers.OnProductUpdated);
        eventBus.Subscribe<ProductDeletedEvent>(handlers.OnProductDeleted);
        eventBus.Subscribe<OrderPlacedEvent>(handlers.OnOrderPlaced);
        eventBus.Subscribe<OrderCompletedEvent>(handlers.OnOrderCompleted);
        eventBus.Subscribe<OrderCancelledEvent>(handlers.OnOrderCancelled);
        eventBus.Subscribe<UserRegisteredEvent>(handlers.OnUserRegistered);
        eventBus.Subscribe<ReviewSubmittedEvent>(handlers.OnReviewSubmitted);
    }
}
