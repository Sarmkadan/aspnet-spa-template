# CachedProductService

`CachedProductService` is an `IProductService` decorator that adds caching to featured-product queries while delegating product operations to another `IProductService` implementation.

`GetFeaturedProductsAsync` caches each requested limit under a separate `featured_products:{limit}` key for 60 seconds. Creating, updating, deleting, changing availability, changing featured status, or bulk-updating product prices removes all `featured_products:*` entries. The remaining query methods pass directly to the decorated service. Constructor arguments are checked for `null`.

## Public methods

- `GetProductByIdAsync(int id)` gets a product by ID from the decorated service.
- `GetAllProductsAsync(int pageNumber = 1, int pageSize = 10)` gets a paginated product list from the decorated service.
- `GetProductsByCategoryAsync(ProductCategory category, int pageNumber = 1, int pageSize = 10)` gets a paginated category-specific list from the decorated service.
- `GetFeaturedProductsAsync(int limit = 10)` gets featured products through the cache, using a 60-second absolute expiration.
- `GetTopRatedProductsAsync(int limit = 10)` gets top-rated products from the decorated service.
- `SearchProductsAsync(string query, ProductCategory? category = null, decimal? minPrice = null, decimal? maxPrice = null)` searches through the decorated service.
- `CreateProductAsync(CreateProductRequest request)` creates a product, then invalidates the featured-products cache.
- `UpdateProductAsync(int id, UpdateProductRequest request)` updates a product, then invalidates the featured-products cache.
- `SetProductAvailabilityAsync(int id, bool isAvailable)` changes availability, then invalidates the featured-products cache.
- `SetProductFeaturedAsync(int id, bool isFeatured)` changes featured status, then invalidates the featured-products cache.
- `DeleteProductAsync(int id)` deletes a product, then invalidates the featured-products cache.
- `UpdatePricesAsync(UpdateProductPriceRequest request)` performs a bulk price update, then invalidates the featured-products cache.

## Usage

Register the concrete product service and expose the cached decorator as `IProductService`:

```csharp
services.AddScoped<ProductService>();
services.AddScoped<IProductService>(sp =>
    new CachedProductService(
        sp.GetRequiredService<ProductService>(),
        sp.GetRequiredService<ICacheService>(),
        sp.GetRequiredService<ILogger<CachedProductService>>()));
```

Consumers can then use the interface normally:

```csharp
public async Task<List<ProductResponse>> GetHomepageProductsAsync(
    IProductService products)
{
    return await products.GetFeaturedProductsAsync(limit: 6);
}
```
