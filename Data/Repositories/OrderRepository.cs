// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetSpaTemplate.Data.Repositories;

/// <summary>
/// Repository for order entity operations.
/// </summary>
public class OrderRepository : RepositoryBase<Order>
{
    public OrderRepository(AppDbContext context) : base(context) { }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        return await DbSet
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        return await DbSet
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
    {
        return await DbSet
            .Include(o => o.Items)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.OrderedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId, int pageNumber, int pageSize)
    {
        return await DbSet
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await DbSet
            .Include(o => o.Items)
            .Where(o => o.OrderedAt >= cutoffDate)
            .OrderByDescending(o => o.OrderedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
    {
        return await DbSet
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed)
            .OrderByDescending(o => o.OrderedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await DbSet
            .Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded)
            .SumAsync(o => o.Total);
    }

    public async Task<decimal> GetTotalRevenueAsync(int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await DbSet
            .Where(o => o.OrderedAt >= cutoffDate &&
                       o.Status != OrderStatus.Cancelled &&
                       o.Status != OrderStatus.Refunded)
            .SumAsync(o => o.Total);
    }

    public async Task<int> GetOrderCountAsync(int userId)
    {
        return await DbSet.CountAsync(o => o.UserId == userId);
    }

    public async Task<decimal> GetAverageOrderValueAsync()
    {
        return await DbSet
            .Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded)
            .AverageAsync(o => o.Total);
    }

    public override async Task<Order?> GetByIdAsync(int id)
    {
        return await DbSet
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
