# RepositoryBase

`RepositoryBase<T>` is an abstract base class that implements the repository pattern for entity types in the `aspnet-spa-template` project. It provides a set of virtual asynchronous and synchronous methods for common data access operations, such as retrieving, adding, updating, removing, and counting entities. The class is designed to be inherited by concrete repository implementations, which can override the virtual members to customize behavior (e.g., add eager loading, filtering, or logging). All asynchronous methods rely on the underlying `DbContext` and are intended for use with Entity Framework Core or a similar ORM.

## API

### `GetByIdAsync`
```csharp
public virtual async Task<T?> GetByIdAsync(object id)
```
Retrieves an entity by its primary key value.  
- **Parameters**  
  `id` – The primary key value of the entity to find.  
- **Returns**  
  The entity if found; otherwise `null`.  
- **Throws**  
  `ArgumentNullException` if `id` is `null`.  
  `InvalidOperationException` if the entity type does not have a primary key defined.

### `GetAllAsync`
```csharp
public virtual async Task<IEnumerable<T>> GetAllAsync()
```
Returns all entities of type `T` from the data store.  
- **Parameters**  
  None.  
- **Returns**  
  An `IEnumerable<T>` containing all entities.  
- **Throws**  
  No exceptions are thrown under normal operation, but underlying data store errors may propagate.

### `FindAsync`
```csharp
public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
```
Returns all entities that satisfy the specified predicate.  
- **Parameters**  
  `predicate` – A lambda expression representing the filter condition.  
- **Returns**  
  An `IEnumerable<T>` of matching entities.  
- **Throws**  
  `ArgumentNullException` if `predicate` is `null`.

### `FirstOrDefaultAsync`
```csharp
public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
```
Returns the first entity that matches the predicate, or `null` if no match is found.  
- **Parameters**  
  `predicate` – A lambda expression representing the filter condition.  
- **Returns**  
  The first matching entity, or `null`.  
- **Throws**  
  `ArgumentNullException` if `predicate` is `null`.

### `CountAsync`
```csharp
public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
```
Counts the number of entities that satisfy the specified predicate.  
- **Parameters**  
  `predicate` – A lambda expression representing the filter condition.  
- **Returns**  
  The count of matching entities.  
- **Throws**  
  `ArgumentNullException` if `predicate` is `null`.

### `ExistsAsync`
```csharp
public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
```
Determines whether any entity matches the specified predicate.  
- **Parameters**  
  `predicate` – A lambda expression representing the filter condition.  
- **Returns**  
  `true` if at least one matching entity exists; otherwise `false`.  
- **Throws**  
  `ArgumentNullException` if `predicate` is `null`.

### `GetPagedAsync`
```csharp
public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageIndex, int pageSize, Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
```
Returns a page of entities, optionally filtered and ordered.  
- **Parameters**  
  `pageIndex` – The zero-based page index.  
  `pageSize` – The number of entities per page.  
  `filter` – An optional predicate to filter results.  
  `orderBy` – An optional function to apply ordering to the query.  
- **Returns**  
  An `IEnumerable<T>` containing the requested page of entities.  
- **Throws**  
  `ArgumentOutOfRangeException` if `pageIndex` is negative or `pageSize` is less than 1.  
  `ArgumentNullException` if `filter` is provided but is `null` (though the parameter is optional, passing `null` explicitly is allowed).

### `Add`
```csharp
public virtual void Add(T entity)
```
Marks an entity as added for insertion on the next `SaveChangesAsync` call.  
- **Parameters**  
  `entity` – The entity to add.  
- **Returns**  
  None.  
- **Throws**  
  `ArgumentNullException` if `entity` is `null`.

### `AddRange`
```csharp
public virtual void AddRange(IEnumerable<T> entities)
```
Marks a collection of entities as added for insertion.  
- **Parameters**  
  `entities` – The entities to add.  
- **Returns**  
  None.  
- **Throws**  
  `ArgumentNullException` if `entities` is `null`.

### `Update`
```csharp
public virtual void Update(T entity)
```
Marks an existing entity as modified.  
- **Parameters**  
  `entity` – The entity to update.  
- **Returns**  
  None.  
- **Throws**  
  `ArgumentNullException` if `entity` is `null`.

### `UpdateRange`
```csharp
public virtual void UpdateRange(IEnumerable<T> entities)
```
Marks a collection of existing entities as modified.  
- **Parameters**  
  `entities` – The entities to update.  
- **Returns**  
  None.  
- **Throws**  
  `ArgumentNullException` if `entities` is `null`.

### `Remove`
```csharp
public virtual void Remove(T entity)
```
Marks an entity as deleted.  
- **Parameters**  
  `entity` – The entity to remove.  
- **Returns**  
  None.  
- **Throws**  
  `ArgumentNullException` if `entity` is `null`.

### `RemoveRange`
```csharp
public virtual void RemoveRange(IEnumerable<T> entities)
```
Marks a collection of entities as deleted.  
- **Parameters**  
  `entities` – The entities to remove.  
- **Returns**  
  None.  
- **Throws**  
  `ArgumentNullException` if `entities` is `null`.

### `SaveChangesAsync`
```csharp
public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
```
Persists all pending changes to the data store.  
- **Parameters**  
  `cancellationToken` – An optional token to cancel the operation.  
- **Returns**  
  The number of state entries written to the underlying database.  
- **Throws**  
  `DbUpdateException` if an error occurs during save.  
  `DbUpdateConcurrencyException` if a concurrency conflict is detected.  
  `OperationCanceledException` if the cancellation token is triggered.

## Usage

### Example 1: Basic CRUD operations for a `Product` entity

```csharp
public class ProductRepository : RepositoryBase<Product>
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }
}

// Usage in a service
public class ProductService
{
    private readonly ProductRepository _repo;

    public ProductService(ProductRepository repo) => _repo = repo;

    public async Task<Product?> GetProductAsync(int id) => await _repo.GetByIdAsync(id);

    public async Task AddProductAsync(Product product)
    {
        _repo.Add(product);
        await _repo.SaveChangesAsync();
    }

    public async Task<bool> ProductExistsAsync(string name) =>
        await _repo.ExistsAsync(p => p.Name == name);
}
```

### Example 2: Paged and filtered query with ordering

```csharp
public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int page, int pageSize)
{
    return await _orderRepo.GetPagedAsync(
        pageIndex: page,
        pageSize: pageSize,
        filter: o => o.OrderDate >= DateTime.UtcNow.AddDays(-7),
        orderBy: q => q.OrderByDescending(o => o.OrderDate)
    );
}
```

## Notes

- **Thread safety** – Instances of `RepositoryBase` are not thread-safe. Each repository should be scoped to a single `DbContext` instance, and concurrent access from multiple threads is not supported. Use dependency injection with a scoped lifetime for repositories.
- **Null arguments** – All methods that accept an entity or a predicate throw `ArgumentNullException` when a `null` argument is passed. Always validate inputs before calling these methods.
- **Asynchronous methods** – The `async` methods execute database queries asynchronously. They should be awaited to avoid blocking threads. The synchronous `Add`, `Update`, `Remove`, and their range counterparts do not perform I/O; they only change the entity state in the change tracker.
- **Paging edge cases** – `GetPagedAsync` returns an empty collection when the requested page exceeds the available data. A `pageIndex` of 0 returns the first page. If `pageSize` is larger than the total number of records, all matching records are returned in a single page.
- **Concurrency** – `SaveChangesAsync` may throw `DbUpdateConcurrencyException` when optimistic concurrency conflicts occur (e.g., when another user has modified the same entity). Handle this exception appropriately in calling code.
- **Inheritance** – Override any virtual method to add custom behavior such as including related entities (`.Include()`), applying global filters, or logging. Ensure that overrides call the base implementation if the default logic is still needed.
