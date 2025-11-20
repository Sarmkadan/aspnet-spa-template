// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetSpaTemplate.Data.Repositories;

/// <summary>
/// Repository for user entity operations.
/// </summary>
public class UserRepository : RepositoryBase<User>
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await DbSet.Where(u => u.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetVerifiedUsersAsync()
    {
        return await DbSet.Where(u => u.IsEmailVerified && u.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetRecentlyActiveAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await DbSet
            .Where(u => u.LastLoginAt != null && u.LastLoginAt >= cutoffDate)
            .OrderByDescending(u => u.LastLoginAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersByCountryAsync(string country)
    {
        return await DbSet
            .Where(u => u.Country == country && u.IsActive)
            .ToListAsync();
    }

    public async Task<int> GetUserCountAsync()
    {
        return await DbSet.CountAsync();
    }

    public async Task<int> GetActiveUserCountAsync()
    {
        return await DbSet.CountAsync(u => u.IsActive);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await DbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }
}
