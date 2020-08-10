// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Service for order-related business logic.
/// </summary>
public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;

    public OrderService(OrderRepository orderRepository, ProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new NotFoundException("Order", id);

        return MapToResponse(order);
    }

    public async Task<OrderResponse> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
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

        decimal subtotal = 0;
        decimal totalTax = 0;

        foreach (var itemRequest in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemRequest.ProductId);
            if (product == null)
                throw new NotFoundException("Product", itemRequest.ProductId);

            if (!product.CanPurchase(itemRequest.Quantity))
                throw new BusinessException($"Insufficient stock for product {product.Name}", "INSUFFICIENT_STOCK");

            var itemSubtotal = product.Price * itemRequest.Quantity;
            var itemTax = itemSubtotal * ProductCategory.GetTaxRate();

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

        order.SubTotal = subtotal;
        order.TaxAmount = totalTax;
        order.Total = subtotal + totalTax + order.ShippingCost - order.Discount;

        _orderRepository.Add(order);
        await _orderRepository.SaveChangesAsync();

        return MapToResponse(order);
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(int id, UpdateOrderStatusRequest request)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new NotFoundException("Order", id);

        if (Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
        {
            switch (newStatus)
            {
                case OrderStatus.Processing:
                    order.MarkAsProcessing();
                    break;
                case OrderStatus.Shipped:
                    order.MarkAsShipped(request.TrackingNumber ?? "");
                    break;
                case OrderStatus.Delivered:
                    order.MarkAsDelivered();
                    break;
                case OrderStatus.Cancelled:
                    order.Cancel();
                    break;
                default:
                    throw new ValidationException("Status", "Invalid order status");
            }
        }
        else
        {
            throw new ValidationException("Status", "Invalid order status format");
        }

        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return MapToResponse(order);
    }

    public async Task<OrderResponse> ApplyDiscountAsync(int id, ApplyDiscountRequest request)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new NotFoundException("Order", id);

        if (order.IsFinal())
            throw new BusinessException("Cannot apply discount to a finalized order", "ORDER_FINALIZED");

        order.ApplyDiscount(request.DiscountAmount);
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return MapToResponse(order);
    }

    public async Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(int userId, int pageNumber = 1, int pageSize = 10)
    {
        var orders = await _orderRepository.GetUserOrdersAsync(userId, pageNumber, pageSize);
        return orders.Select(MapToResponse).ToList();
    }

    public async Task<IEnumerable<OrderResponse>> GetPendingOrdersAsync()
    {
        var orders = await _orderRepository.GetPendingOrdersAsync();
        return orders.Select(MapToResponse).ToList();
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _orderRepository.GetTotalRevenueAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync(int days)
    {
        return await _orderRepository.GetTotalRevenueAsync(days);
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
