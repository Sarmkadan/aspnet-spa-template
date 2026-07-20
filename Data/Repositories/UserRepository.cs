#nullable enable
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

    /// <summary>Parameterless constructor for test-time proxying.</summary>
    protected UserRepository() { }

    public virtual async Task<User?> GetByEmailAsync(string email)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public virtual async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await DbSet.Where(u => u.IsActive && !u.IsDeleted()).ToListAsync();
    }

    public virtual async Task<IEnumerable<User>> GetVerifiedUsersAsync()
    {
        return await DbSet.Where(u => u.IsEmailVerified && u.IsActive && !u.IsDeleted()).ToListAsync();
    }

    public virtual async Task<IEnumerable<User>> GetRecentlyActiveAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await DbSet
            .Where(u => u.LastLoginAt != null && u.LastLoginAt >= cutoffDate && u.IsActive && !u.IsDeleted())
            .OrderByDescending(u => u.LastLoginAt)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<User>> GetUsersByCountryAsync(string country)
    {
        return await DbSet
            .Where(u => u.Country == country && u.IsActive && !u.IsDeleted())
            .ToListAsync();
    }

    public virtual async Task<int> GetUserCountAsync()
    {
        return await DbSet.CountAsync(u => !u.IsDeleted());
    }

    public virtual async Task<int> GetActiveUserCountAsync()
    {
        return await DbSet.CountAsync(u => u.IsActive && !u.IsDeleted());
    }

    public virtual async Task<bool> EmailExistsAsync(string email)
    {
        return await DbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted());
    }

    public virtual async Task<User?> GetByIdAsync(int id)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted());
    }

    public virtual async Task<IEnumerable<User>> GetAllAsync()
    {
        return await DbSet.Where(u => !u.IsDeleted()).ToListAsync();
    }

    public virtual async Task<User?> GetByIdIncludingDeletedAsync(int id)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Id == id);
    }

    public virtual void SoftDelete(User user)
    {
        user.SoftDelete();
        Update(user);
    }

    public virtual void Restore(User user)
    {
        user.Restore();
        Update(user);
    }
}
