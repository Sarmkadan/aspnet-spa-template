#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;

namespace AspNetSpaTemplate.Models;

/// <summary>
/// Represents a status change history entry for an order.
/// </summary>
public sealed class StatusHistory
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public OrderStatus FromStatus { get; set; }

    public OrderStatus ToStatus { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public string? ChangedBy { get; set; }

    public string? Notes { get; set; }

    // Navigation property
    public Order? Order { get; set; }
}