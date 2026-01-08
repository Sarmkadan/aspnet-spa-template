// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Represents an item to add to an order.
/// </summary>
public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Data transfer object for creating a new order.
/// </summary>
public class CreateOrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Data transfer object for order item response.
/// </summary>
public class OrderItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
}

/// <summary>
/// Data transfer object for order response.
/// </summary>
public class OrderResponse
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
    public DateTime OrderedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}

/// <summary>
/// Data transfer object for applying discount to an order.
/// </summary>
public class ApplyDiscountRequest
{
    public decimal DiscountAmount { get; set; }
}

/// <summary>
/// Data transfer object for updating order status.
/// </summary>
public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
}
