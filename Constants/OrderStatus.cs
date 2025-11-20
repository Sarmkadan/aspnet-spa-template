// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Constants;

/// <summary>
/// Enumeration for order statuses.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Refunded = 6
}

public static class OrderStatusExtensions
{
    public static string ToDisplayName(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Pending",
        OrderStatus.Confirmed => "Confirmed",
        OrderStatus.Processing => "Processing",
        OrderStatus.Shipped => "Shipped",
        OrderStatus.Delivered => "Delivered",
        OrderStatus.Cancelled => "Cancelled",
        OrderStatus.Refunded => "Refunded",
        _ => "Unknown"
    };

    public static bool CanBeProcessed(this OrderStatus status) =>
        status is OrderStatus.Pending or OrderStatus.Confirmed;

    public static bool CanBeCancelled(this OrderStatus status) =>
        status is not (OrderStatus.Delivered or OrderStatus.Refunded or OrderStatus.Cancelled);

    public static bool IsFinal(this OrderStatus status) =>
        status is OrderStatus.Delivered or OrderStatus.Refunded or OrderStatus.Cancelled;
}
