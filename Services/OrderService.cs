#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using Microsoft.Extensions.Logging;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Service for order-related business logic, handling creation, status updates, and reporting.
/// </summary>
public sealed class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;
    private readonly ILogger<OrderService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="OrderService"/>
    /// </summary>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="productRepository">The product repository.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when orderRepository, productRepository, or logger is null.</exception>
    public OrderService(OrderRepository orderRepository, ProductRepository productRepository, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets an order by its ID.
    /// </summary>
    /// <param name="id">The order ID.</param>
    /// <returns>The order response, or throws NotFoundException.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when ID is less than or equal to 0.</exception>
    public async Task<OrderResponse?> GetOrderByIdAsync(int id)
    {
        _logger.LogDebug("Getting order by ID: {OrderId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Invalid order ID: {OrderId}", id);
            throw new ArgumentOutOfRangeException(nameof(id), "Order ID must be greater than 0");
        }

        var order = await _orderRepository.GetByIdAsync(id);
        if (order is null)
        {
            _logger.LogWarning("Order not found: {OrderId}", id);
            throw new NotFoundException("Order", id);
        }

        _logger.LogInformation("Retrieved order: {OrderId} - {OrderNumber} (Status: {Status})", order.Id, order.OrderNumber, order.Status.ToDisplayName());
        return MapToResponse(order);
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="request">The create order request.</param>
    /// <returns>The created order response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="ValidationException">Thrown when items list is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when product is not found.</exception>
    /// <exception cref="BusinessException">Thrown when there's insufficient stock.</exception>
    public async Task<OrderResponse> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId), "User ID must be greater than 0");

        if (request.Items is null || request.Items.Count == 0)
            throw new ValidationException("Items", "Order must contain at least one item");

        var order = new Order
        {
            UserId = userId,
            OrderNumber = GenerateOrderNumber(),
            Status = OrderStatus.Pending,
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress,
            Notes = request.Notes,
            OrderedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        try
        {
            var (subtotal, totalTax) = await ProcessOrderItemsAsync(order, request.Items);
            
            order.SubTotal = subtotal;
            order.TaxAmount = totalTax;
            order.Total = subtotal + totalTax + order.ShippingCost - order.Discount;

            _orderRepository.Add(order);
            await _orderRepository.SaveChangesAsync();

            return MapToResponse(order);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Rollback stock changes if order creation fails
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product is not null)
                {
                    product.IncreaseStock(item.Quantity);
                    _productRepository.Update(product);
                }
            }

            // Domain failures (insufficient stock, missing product, validation) must
            // surface as-is; only unexpected errors are wrapped as a creation failure.
            if (ex is AspNetSpaTemplateException)
                throw;

            throw new BusinessException("Failed to create order due to database error", "ORDER_CREATION_FAILED", 500).WithData(ex);
        }
    }

    private async Task<(decimal Subtotal, decimal TotalTax)> ProcessOrderItemsAsync(Order order, List<OrderItemRequest> itemRequests)
    {
        order.Items ??= new List<OrderItem>();

        decimal subtotal = 0;
        decimal totalTax = 0;

        foreach (var itemRequest in itemRequests)
        {
            var product = await _productRepository.GetByIdAsync(itemRequest.ProductId);
            if (product is null)
                throw new NotFoundException("Product", itemRequest.ProductId);

            if (!product.CanPurchase(itemRequest.Quantity))
                throw new BusinessException($"Insufficient stock for product {product.Name}", "INSUFFICIENT_STOCK");

            var itemSubtotal = product.Price * itemRequest.Quantity;
            var itemTax = itemSubtotal * product.Category.GetTaxRate();

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemRequest.Quantity,
                UnitPrice = product.Price,
                TaxAmount = itemTax,
                Total = itemSubtotal + itemTax
            };

            order.Items.Add(orderItem);
            subtotal += itemSubtotal;
            totalTax += itemTax;

            // Reduce product stock
            product.ReduceStock(itemRequest.Quantity);
            _productRepository.Update(product);
        }

        return (subtotal, totalTax);
    }

    /// <summary>
    /// Updates the status of an order.
    /// </summary>
    /// <param name="id">The order ID.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated order response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="NotFoundException">Thrown when order is not found.</exception>
    /// <exception cref="ValidationException">Thrown when status is invalid.</exception>
    public async Task<OrderResponse> UpdateOrderStatusAsync(int id, UpdateOrderStatusRequest request)
    {
        _logger.LogInformation("Updating order status: {OrderId} - Status={Status}", id, request?.Status ?? "Unknown");

        if (request is null)
        {
            _logger.LogWarning("UpdateOrderStatusAsync called with null request for order {OrderId}", id);
            throw new ArgumentNullException(nameof(request));
        }

        var order = await _orderRepository.GetByIdAsync(id);
        if (order is null)
        {
            _logger.LogWarning("Order not found for status update: {OrderId}", id);
            throw new NotFoundException("Order", id);
        }

        if (Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
        {
            switch (newStatus)
            {
                case OrderStatus.Processing:
                    order.MarkAsProcessing();
                    _logger.LogDebug("Order marked as Processing: {OrderId}", id);
                    break;
                case OrderStatus.Shipped:
                    order.MarkAsShipped(request.TrackingNumber ?? "");
                    _logger.LogDebug("Order marked as Shipped: {OrderId} - Tracking: {TrackingNumber}", id, request.TrackingNumber ?? "None");
                    break;
                case OrderStatus.Delivered:
                    order.MarkAsDelivered();
                    _logger.LogDebug("Order marked as Delivered: {OrderId}", id);
                    break;
                case OrderStatus.Cancelled:
                    order.Cancel();
                    _logger.LogDebug("Order cancelled: {OrderId}", id);
                    break;
                default:
                    _logger.LogWarning("Invalid order status: {Status}", request.Status);
                    throw new ValidationException("Status", "Invalid order status");
            }
        }
        else
        {
            _logger.LogWarning("Invalid order status format: {Status}", request.Status);
            throw new ValidationException("Status", "Invalid order status format");
        }

        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("Order status updated successfully: {OrderId} - New Status: {Status}", order.Id, order.Status.ToDisplayName());
        return MapToResponse(order);
    }

    /// <summary>
    /// Applies a discount to an order.
    /// </summary>
    /// <param name="id">The order ID.</param>
    /// <param name="request">The discount request.</param>
    /// <returns>The updated order response.</returns>
    /// <exception cref="NotFoundException">Thrown when order is not found.</exception>
    /// <exception cref="BusinessException">Thrown when order is finalized.</exception>
    public async Task<OrderResponse> ApplyDiscountAsync(int id, ApplyDiscountRequest request)
    {
        _logger.LogInformation("Applying discount to order: {OrderId} - Discount: {DiscountAmount}", id, request?.DiscountAmount ?? 0);

        if (request is null)
        {
            _logger.LogWarning("ApplyDiscountAsync called with null request for order {OrderId}", id);
            throw new ArgumentNullException(nameof(request));
        }

        var order = await _orderRepository.GetByIdAsync(id);
        if (order is null)
        {
            _logger.LogWarning("Order not found for discount: {OrderId}", id);
            throw new NotFoundException("Order", id);
        }

        if (order.Status.IsFinal())
        {
            _logger.LogWarning("Cannot apply discount to finalized order: {OrderId} - Status: {Status}", id, order.Status.ToDisplayName());
            throw new BusinessException("Cannot apply discount to a finalized order", "ORDER_FINALIZED");
        }

        order.ApplyDiscount(request.DiscountAmount);
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("Discount applied successfully: {OrderId} - New Total: {Total}", order.Id, order.Total);
        return MapToResponse(order);
    }

    /// <summary>
    /// Cancels an order.
    /// </summary>
    /// <param name="id">The order ID.</param>
    /// <param name="userId">The user ID attempting to cancel.</param>
    /// <returns>The cancelled order response.</returns>
    /// <exception cref="NotFoundException">Thrown when order is not found.</exception>
    /// <exception cref="BusinessException">Thrown when order cannot be cancelled.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when user does not own the order.</exception>
    public async Task<OrderResponse> CancelOrderAsync(int id, int userId)
    {
        _logger.LogInformation("Cancelling order: {OrderId} by user {UserId}", id, userId);

        var order = await _orderRepository.GetByIdAsync(id);
        if (order is null)
        {
            _logger.LogWarning("Order not found for cancellation: {OrderId}", id);
            throw new NotFoundException("Order", id);
        }

        if (order.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to cancel order {OrderId} that doesn't belong to them", userId, id);
            throw new UnauthorizedAccessException("You can only cancel your own orders");
        }

        if (!order.CanBeCancelled())
        {
            _logger.LogWarning("Cannot cancel order {OrderId} with status: {Status}", id, order.Status.ToDisplayName());
            throw new BusinessException("Order cannot be cancelled in its current state", "ORDER_NOT_CANCELLABLE");
        }

        order.Cancel();
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("Order cancelled successfully: {OrderId} - New Status: {Status}", order.Id, order.Status.ToDisplayName());
        return MapToResponse(order);
    }

    /// <summary>
    /// Gets orders for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A collection of orders.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when userId is less than or equal to 0.</exception>
    public async Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(int userId, int pageNumber = 1, int pageSize = 10)
    {
        _logger.LogDebug("Getting orders for user: {UserId} (page {PageNumber}, pageSize {PageSize})", userId, pageNumber, pageSize);

        if (userId <= 0)
        {
            _logger.LogWarning("Invalid user ID for order retrieval: {UserId}", userId);
            throw new ArgumentOutOfRangeException(nameof(userId), "User ID must be greater than 0");
        }

        var orders = await _orderRepository.GetUserOrdersAsync(userId, pageNumber, pageSize);
        var count = orders.Count();
        _logger.LogInformation("Retrieved {OrderCount} orders for user {UserId}", count, userId);

        return orders.Select(MapToResponse).ToList();
    }

    /// <summary>
    /// Gets all pending orders.
    /// </summary>
    /// <returns>A collection of pending orders.</returns>
    public async Task<IEnumerable<OrderResponse>> GetPendingOrdersAsync()
    {
        _logger.LogDebug("Getting all pending orders");
        var orders = await _orderRepository.GetPendingOrdersAsync();
        var count = orders.Count();
        _logger.LogInformation("Retrieved {PendingOrderCount} pending orders", count);
        return orders.Select(MapToResponse).ToList();
    }

    /// <summary>
    /// Gets total revenue.
    /// </summary>
    /// <returns>The total revenue.</returns>
    public async Task<decimal> GetTotalRevenueAsync()
    {
        _logger.LogDebug("Getting total revenue");
        var revenue = await _orderRepository.GetTotalRevenueAsync();
        _logger.LogInformation("Total revenue: {Revenue}", revenue);
        return revenue;
    }

    /// <summary>
    /// Gets total revenue for the last N days.
    /// </summary>
    /// <param name="days">Number of days.</param>
    /// <returns>The total revenue.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when days is less than 1.</exception>
    public async Task<decimal> GetTotalRevenueAsync(int days)
    {
        _logger.LogDebug("Getting revenue for last {Days} days", days);

        if (days < 1)
        {
            _logger.LogWarning("Invalid days parameter: {Days}", days);
            throw new ArgumentOutOfRangeException(nameof(days), "Days must be greater than 0");
        }

        var revenue = await _orderRepository.GetTotalRevenueAsync(days);
        _logger.LogInformation("Revenue for last {Days} days: {Revenue}", days, revenue);
        return revenue;
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }

    private OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToDisplayName(),
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingCost = order.ShippingCost,
            Discount = order.Discount,
            Total = order.Total,
            ShippingAddress = order.ShippingAddress,
            Notes = order.Notes,
            OrderedAt = order.OrderedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            Items = order.Items?.Select(i => new OrderItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TaxAmount = i.TaxAmount,
                Discount = i.Discount,
                Total = i.Total
            }).ToList() ?? new List<OrderItemResponse>()
        };
    }
}