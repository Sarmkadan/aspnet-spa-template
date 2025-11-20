// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;

namespace AspNetSpaTemplate.Models;

/// <summary>
/// Represents a product in the catalog.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public ProductCategory Category { get; set; }
    public string? ImageUrl { get; set; }
    public string? Sku { get; set; }
    public decimal Rating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public bool IsAvailable { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<OrderItem>? OrderItems { get; set; }
    public ICollection<Review>? Reviews { get; set; }

    public bool IsInStock() => StockQuantity > 0 && IsAvailable;

    public bool CanPurchase(int quantity) => IsInStock() && StockQuantity >= quantity;

    public void ReduceStock(int quantity)
    {
        if (!CanPurchase(quantity))
            throw new InvalidOperationException($"Cannot reduce stock by {quantity}. Current stock: {StockQuantity}");

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal GetTaxAmount() => Price * ProductCategory.GetTaxRate();

    public decimal GetPriceWithTax() => Price + GetTaxAmount();

    public void UpdateRating(decimal newRating, int newReviewCount)
    {
        if (newRating < 0 || newRating > 5)
            throw new ArgumentException("Rating must be between 0 and 5");

        Rating = newRating;
        ReviewCount = newReviewCount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string description, decimal price, ProductCategory category, string? imageUrl)
    {
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAvailability(bool available)
    {
        IsAvailable = available;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFeatured(bool featured)
    {
        IsFeatured = featured;
        UpdatedAt = DateTime.UtcNow;
    }
}
