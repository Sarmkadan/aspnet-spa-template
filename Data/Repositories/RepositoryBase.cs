// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace AspNetSpaTemplate.Data.Repositories;

/// <summary>
/// Base repository implementation providing common CRUD and query operations.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public abstract class RepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected RepositoryBase(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate == null
            ? await DbSet.CountAsync()
            : await DbSet.CountAsync(predicate);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.AnyAsync(predicate);
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null)
    {
        var query = predicate == null ? DbSet.AsQueryable() : DbSet.Where(predicate);
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public virtual void Add(T entity)
    {
        DbSet.Add(entity);
    }

    public virtual void AddRange(IEnumerable<T> entities)
    {
        DbSet.AddRange(entities);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        DbSet.UpdateRange(entities);
    }

    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
    }

    public virtual async Task<int> SaveChangesAsync()
    {
        return await Context.SaveChangesAsync();
    }
}
