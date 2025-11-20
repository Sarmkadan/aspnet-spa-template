// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;

namespace AspNetSpaTemplate.Models;

/// <summary>
/// Represents a customer order.
/// </summary>
public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; } = 0;
    public decimal Discount { get; set; } = 0;
    public decimal Total { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? Notes { get; set; }
    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<OrderItem>? Items { get; set; }

    public int GetTotalItems() => Items?.Sum(i => i.Quantity) ?? 0;

    public bool CanBeCancelled() => Status.CanBeCancelled();

    public bool CanBeProcessed() => Status.CanBeProcessed();

    public void Cancel(string? reason = null)
    {
        if (!CanBeCancelled())
            throw new InvalidOperationException($"Cannot cancel order with status {Status.ToDisplayName()}");

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
    }

    public void MarkAsProcessing()
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Order cannot be processed from current status");

        Status = OrderStatus.Processing;
    }

    public void MarkAsShipped(string trackingNumber)
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Only processing orders can be shipped");

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be marked as delivered");

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative");

        Discount = discountAmount;
        RecalculateTotal();
    }

    public void RecalculateTotal()
    {
        Total = SubTotal + TaxAmount + ShippingCost - Discount;
        if (Total < 0)
            Total = 0;
    }

    public bool IsRecent(int days = 30) => DateTime.UtcNow.Subtract(OrderedAt).TotalDays <= days;
}
