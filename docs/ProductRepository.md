# ProductRepository

The `ProductRepository` class provides data‑access operations for the `Product` entity within the ASP.NET SPA template. It wraps an `AppDbContext` instance and exposes asynchronous methods for querying products by various criteria, such as category, featured status, rating, stock levels, and search terms, as well as aggregations like average price and available count.

## API

### `public ProductRepository(AppDbContext context) : base(context)`

- **Purpose**: Initializes a new instance of the repository with the supplied Entity Framework Core context.
- **Parameters**: 
  - `context` – The `AppDbContext` used to execute queries and track changes.
- **Return value**: (constructor) – No return value.
- **Exceptions**: Throws `System.ArgumentNullException` if `context` is `null`.

### `public virtual async Task<IEnumerable<Product>> GetByCategoryAsync`

- **Purpose**: Returns all products that belong to a specific category.
- **Parameters**: (as defined in the source) – typically a category identifier or name.
- **Return value**: A `Task` whose result is an `IEnumerable<Product>` containing the matching products; may be empty if no products match.
- **Exceptions**: May propagate exceptions from the underlying `DbContext` (e.g., `DbUpdateException`, `InvalidOperationException`) or throw `OperationCanceledException` if a cancellation token is triggered.

### `public virtual async Task<IEnumerable<Product>> GetFeaturedProductsAsync`

- **Purpose**: Retrieves products marked as featured.
- **Parameters**: None.
- **Return value**: A `Task` yielding an `IEnumerable<Product>` of featured products; empty sequence if none are featured.
- **Exceptions**: Same as above – any data‑access error bubbles up as a `DbException`‑derived exception.

### `public virtual async Task<IEnumerable<Product>> GetTopRatedAsync`

- **Purpose**: Returns products with the highest ratings.
- **Parameters**: None.
- **Return value**: A `Task` producing an `IEnumerable<Product>` of top‑rated products; empty if no ratings exist.
- **Exceptions**: Propagates any exceptions thrown by the context.

### `public virtual async Task<IEnumerable<Product>> GetInStockAsync`

- **Purpose**: Returns products that currently have a positive stock quantity.
- **Parameters**: None.
- **Return value**: A `Task` resolving to an `IEnumerable<Product>` of in‑stock items; may be empty.
- **Exceptions**: Same as other query methods.

### `public virtual async Task<IEnumerable<Product>> GetLowStockAsync`

- **Purpose**: Returns products whose stock level falls below a defined low‑stock threshold.
- **Parameters**: None.
- **Return value**: A `Task` yielding an `IEnumerable<Product>` of low‑stock products; empty if all products are adequately stocked.
- **Exceptions**: May throw exceptions from the data access layer.

### `public virtual async Task<IEnumerable<Product>> SearchAsync`

- **Purpose**: Performs a free‑text search across product fields (e.g., name, description) and returns matching products.
- **Parameters**: (as defined in the source) – typically a search string and optional filters.
- **Return value**: A `Task` producing an `IEnumerable<Product>` of matches; empty if no matches are found.
- **Exceptions**: Propagates any query‑execution exceptions; may throw `ArgumentException` if the search term is invalid.

### `public virtual async Task<Product?> GetBySkuAsync`

- **Purpose**: Retrieves a single product by its stock‑keeping unit (SKU).
- **Parameters**: (as defined in the source) – the SKU string to look up.
- **Return value**: A `Task` yielding the matching `Product` or `null` if no product with the given SKU exists.
- **Exceptions**: Throws exceptions from the context; may throw `ArgumentNullException` if the SKU argument is `null`.

### `public virtual async Task<IEnumerable<Product>> GetPagedByCategoryAsync`

- **Purpose**: Returns a paged list of products for a given category, supporting scenarios such as infinite scroll or traditional pagination.
- **Parameters**: (as defined in the source) – typically category identifier, page index, and page size.
- **Return value**: A `Task` yielding an `IEnumerable<Product>` containing the requested page; empty if the page is out of range.
- **Exceptions**: Same as other async query methods; may also throw `ArgumentOutOfRangeException` for invalid paging arguments.

### `public virtual async Task<decimal> GetAveragePriceAsync`

- **Purpose**: Calculates the average price of all products (or of a filtered set, depending on implementation).
- **Parameters**: None.
- **Return value**: A `Task` producing a `decimal` representing the average price; returns `0` if no products exist.
- **Exceptions**: Propagates any exceptions from the underlying query; may throw `InvalidOperationException` if the context cannot compute the average.

### `public virtual async Task<int> GetAvailableProductCountAsync`

- **Purpose**: Returns the number of products that are currently available for purchase (e.g., in stock and not discontinued).
- **Parameters**: None.
- **Return value**: A `Task` yielding an `int` count of available products.
- **Exceptions**: Same as other query methods; may throw exceptions from the data access layer.

## Usage

```csharp
using var context = new AppDbContext(options);
var repository = new ProductRepository(context);

// Get all featured products and display their names.
var featured = await repository.GetFeaturedProductsAsync();
foreach (var p in featured)
{
    Console.WriteLine(p.Name);
}
```

```csharp
using var context = new AppDbContext(options);
var repository = new ProductRepository(context);

// Retrieve the second page of products in the "Electronics" category, 10 items per page.
var page = await repository.GetPagedByCategoryAsync(
    categoryId: 4,   // Electronics
    pageIndex: 1,    // zero‑based; 1 = second page
    pageSize: 10);

Console.WritePage(page); // hypothetical helper to render the page
```

## Notes

- The repository does **not** maintain any internal state beyond the injected `AppDbContext`. Consequently, it is **not thread‑safe**; the same instance should not be used concurrently from multiple threads without external synchronization. The underlying `DbContext` also is not thread‑safe.
- Methods that return `IEnumerable<Product>` may yield an empty enumeration rather than `null` when no data matches the criteria.
- `GetBySkuAsync` is the only method that can return `null`; callers should check for `null` before accessing product properties.
- Exceptions thrown by these methods typically originate from Entity Framework Core (e.g., `DbUpdateException`, `InvalidOperationException`). Consumers should handle them according to the application’s error‑handling strategy.
- Because all members are `virtual`, derived repositories can override behavior to add caching, logging, or additional filtering without changing the public contract.
