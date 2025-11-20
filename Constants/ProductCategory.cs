// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Constants;

/// <summary>
/// Enumeration for product categories.
/// </summary>
public enum ProductCategory
{
    Electronics = 0,
    Clothing = 1,
    Home = 2,
    Sports = 3,
    Books = 4,
    Food = 5,
    Beauty = 6,
    Toys = 7
}

public static class ProductCategoryExtensions
{
    public static string ToDisplayName(this ProductCategory category) => category switch
    {
        ProductCategory.Electronics => "Electronics",
        ProductCategory.Clothing => "Clothing",
        ProductCategory.Home => "Home & Garden",
        ProductCategory.Sports => "Sports & Outdoors",
        ProductCategory.Books => "Books",
        ProductCategory.Food => "Food & Beverages",
        ProductCategory.Beauty => "Beauty & Personal Care",
        ProductCategory.Toys => "Toys & Games",
        _ => "Other"
    };

    public static decimal GetTaxRate(this ProductCategory category) => category switch
    {
        ProductCategory.Food => 0.05m,
        ProductCategory.Books => 0.0m,
        ProductCategory.Clothing => 0.1m,
        _ => 0.15m
    };
}
