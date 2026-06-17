#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Linq.Expressions;

namespace AspNetSpaTemplate.Data.Repositories;

/// <summary>
/// Generic repository interface defining CRUD and query operations.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>The entity, or null if not found.</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Finds entities based on a predicate.
    /// </summary>
    /// <param name="predicate">The filter condition.</param>
    /// <returns>A collection of entities matching the predicate.</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Gets the first entity matching a predicate.
    /// </summary>
    /// <param name="predicate">The filter condition.</param>
    /// <returns>The first matching entity, or null if none found.</returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Counts the number of entities matching a predicate.
    /// </summary>
    /// <param name="predicate">The optional filter condition.</param>
    /// <returns>The count of matching entities.</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>
    /// Checks if any entity exists matching a predicate.
    /// </summary>
    /// <param name="predicate">The filter condition.</param>
    /// <returns>True if at least one entity matches, false otherwise.</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Gets a paginated collection of entities.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="predicate">The optional filter condition.</param>
    /// <returns>A paginated collection of entities.</returns>
    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null);

    /// <summary>
    /// Adds an entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    void Add(T entity);

    /// <summary>
    /// Adds a range of entities.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    void AddRange(IEnumerable<T> entities);

    /// <summary>
    /// Updates an entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(T entity);

    /// <summary>
    /// Updates a range of entities.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Removes an entity.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void Remove(T entity);

    /// <summary>
    /// Removes a range of entities.
    /// </summary>
    /// <param name="entities">The entities to remove.</param>
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>
    /// Saves changes to the data store.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();
}
