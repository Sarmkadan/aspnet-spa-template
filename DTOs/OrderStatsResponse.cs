#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using AspNetSpaTemplate.Constants;

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Data transfer object for order statistics.
/// </summary>
public sealed class OrderStatsResponse
{
    /// <summary>
    /// Statistics grouped by order status.
    /// </summary>
    public Dictionary<string, int> StatusCounts { get; set; } = new();

    /// <summary>
    /// Total revenue across all orders (excluding cancelled and refunded).
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Total revenue for the specified date range (if provided).
    /// </summary>
    public decimal? DateRangeRevenue { get; set; }

    /// <summary>
    /// Total number of orders.
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Total number of cancelled orders.
    /// </summary>
    public int CancelledOrders { get; set; }

    /// <summary>
    /// Total number of refunded orders.
    /// </summary>
    public int RefundedOrders { get; set; }
}