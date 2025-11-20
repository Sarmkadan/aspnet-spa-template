// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Models;

/// <summary>
/// Represents a line item in an order.
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Discount { get; set; } = 0;
    public decimal Total { get; set; }

    // Navigation properties
    public Order? Order { get; set; }
    public Product? Product { get; set; }

    public decimal GetSubtotal() => UnitPrice * Quantity;

    public decimal GetTotalWithTax() => (GetSubtotal() + TaxAmount) - Discount;

    public void RecalculateTotal()
    {
        Total = GetTotalWithTax();
        if (Total < 0)
            Total = 0;
    }

    public decimal GetDiscount() => Discount;

    public void ApplyDiscount(decimal discountAmount)
    {
        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative");

        if (discountAmount > GetSubtotal())
            throw new ArgumentException("Discount cannot exceed subtotal");

        Discount = discountAmount;
        RecalculateTotal();
    }

    public decimal GetAveragePricePerUnit() => GetSubtotal() / Quantity;

    public bool IsValid() => Quantity > 0 && UnitPrice > 0 && ProductId > 0 && OrderId > 0;
}
