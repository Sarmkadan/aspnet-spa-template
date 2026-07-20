#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetSpaTemplate.Data.Repositories;

/// <summary>
/// Repository for product entity operations.
/// </summary>
public class ProductRepository : RepositoryBase<Product>
{
    public ProductRepository(AppDbContext context) : base(context) { }

    /// <summary>Parameterless constructor for test-time proxying.</summary>
    protected ProductRepository() { }

    public virtual async Task<IEnumerable<Product>> GetByCategoryAsync(ProductCategory category)
    {
        return await DbSet
            .Where(p => p.Category == category && p.IsAvailable)
            .OrderByDescending(p => p.Rating)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int limit = 10)
    {
        return await DbSet
            .Where(p => p.IsFeatured && p.IsAvailable)
            .OrderByDescending(p => p.Rating)
            .Take(limit)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<Product>> GetTopRatedAsync(int limit = 10)
    {
        return await DbSet
            .Where(p => p.IsAvailable)
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(limit)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<Product>> GetInStockAsync()
    {
        return await DbSet
            .Where(p => p.StockQuantity > 0 && p.IsAvailable)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10)
    {
        return await DbSet
            .Where(p => p.StockQuantity <= threshold && p.StockQuantity > 0)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<Product>> SearchAsync(
        string searchTerm,
        ProductCategory? category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        var query = DbSet
            .Where(p => p.IsAvailable)
            .AsQueryable();

        var term = searchTerm.ToLower();

        // Apply text search
        query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));

        // Apply category filter if specified
        if (category.HasValue)
        {
            query = query.Where(p => p.Category == category.Value);
        }

        // Apply price range filters if specified
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        return await query
            .OrderByDescending(p => p.Rating)
            .ToListAsync();
    }

    public virtual async Task<Product?> GetBySkuAsync(string sku)
    {
        return await DbSet.FirstOrDefaultAsync(p => p.Sku == sku);
    }

    public virtual async Task<IEnumerable<Product>> GetPagedByCategoryAsync(ProductCategory category, int pageNumber, int pageSize)
    {
        return await DbSet
            .Where(p => p.Category == category && p.IsAvailable)
            .OrderByDescending(p => p.Rating)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public virtual async Task<decimal> GetAveragePriceAsync(ProductCategory category)
    {
        return await DbSet
            .Where(p => p.Category == category && p.IsAvailable)
            .AverageAsync(p => p.Price);
    }

    public virtual async Task<int> GetAvailableProductCountAsync()
    {
        return await DbSet.CountAsync(p => p.IsAvailable && p.StockQuantity > 0);
    }
}