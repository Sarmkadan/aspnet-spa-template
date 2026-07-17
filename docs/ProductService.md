# ProductService

The `ProductService` provides methods for managing product data in an e-commerce application, including CRUD operations, product discovery, and status management. It abstracts data access behind asynchronous APIs that return strongly-typed responses.

## API

### `ProductService`

Constructor for the service. Initializes dependencies required for product operations.

### `async Task<ProductResponse?> GetProductByIdAsync(int id)`

Retrieves a single product by its unique identifier.

- **Parameters**
  - `id` – The product identifier to search for.
- **Returns**
  - A `ProductResponse` object if found; otherwise `null`.
- **Exceptions**
  - Throws `ArgumentException` if `id` is less than or equal to zero.

### `async Task<ProductListResponse> GetAllProductsAsync()`

Retrieves all products available in the system.

- **Returns**
  - A `ProductListResponse` containing the full list of products.
- **Exceptions**
  - Throws `InvalidOperationException` if the underlying data store is unavailable.

### `async Task<ProductListResponse> GetProductsByCategoryAsync(string categorySlug)`

Retrieves all products belonging to a specific category.

- **Parameters**
  - `categorySlug` – The URL-friendly category identifier.
- **Returns**
  - A `ProductListResponse` containing matching products.
- **Exceptions**
  - Throws `ArgumentException` if `categorySlug` is `null` or whitespace.
  - Throws `InvalidOperationException` if the category does not exist or the data store is unavailable.

### `async Task<List<ProductResponse>> GetFeaturedProductsAsync()`

Retrieves a list of featured products marked in the system.

- **Returns**
  - A list of up to ten `ProductResponse` objects.
- **Exceptions**
  - Throws `InvalidOperationException` if the data store is unavailable.

### `async Task<List<ProductResponse>> GetTopRatedProductsAsync()`

Retrieves a list of products with the highest average ratings.

- **Returns**
  - A list of up to ten `ProductResponse` objects, ordered by rating.
- **Exceptions**
  - Throws `InvalidOperationException` if the data store is unavailable.

### `async Task<List<ProductResponse>> SearchProductsAsync(string query, int maxResults = 10)`

Searches products by name, description, or SKU using a free-text query.

- **Parameters**
  - `query` – The search term to match.
  - `maxResults` – Maximum number of results to return (default: 10).
- **Returns**
  - A list of matching `ProductResponse` objects.
- **Exceptions**
  - Throws `ArgumentException` if `query` is `null` or whitespace.
  - Throws `ArgumentOutOfRangeException` if `maxResults` is less than 1 or greater than 100.

### `async Task<ProductResponse> CreateProductAsync(ProductRequest request)`

Creates a new product in the system.

- **Parameters**
  - `request` – The product details to create.
- **Returns**
  - A `ProductResponse` representing the newly created product.
- **Exceptions**
  - Throws `ArgumentNullException` if `request` is `null`.
  - Throws `InvalidOperationException` if the product cannot be created or the data store is unavailable.

### `async Task<ProductResponse> UpdateProductAsync(int id, ProductRequest request)`

Updates an existing product with new values.

- **Parameters**
  - `id` – The identifier of the product to update.
  - `request` – The updated product details.
- **Returns**
  - A `ProductResponse` representing the updated product.
- **Exceptions**
  - Throws `ArgumentNullException` if `request` is `null`.
  - Throws `ArgumentException` if `id` is less than or equal to zero.
  - Throws `InvalidOperationException` if the product does not exist or the data store is unavailable.

### `async Task SetProductAvailabilityAsync(int id, bool isAvailable)`

Toggles the availability status of a product.

- **Parameters**
  - `id` – The product identifier.
  - `isAvailable` – The desired availability state.
- **Exceptions**
  - Throws `ArgumentException` if `id` is less than or equal to zero.
  - Throws `InvalidOperationException` if the product does not exist or the data store is unavailable.

### `async Task SetProductFeaturedAsync(int id, bool isFeatured)`

Marks a product as featured or removes that status.

- **Parameters**
  - `id` – The product identifier.
  - `isFeatured` – The desired featured state.
- **Exceptions**
  - Throws `ArgumentException` if `id` is less than or equal to zero.
  - Throws `InvalidOperationException` if the product does not exist or the data store is unavailable.

### `async Task DeleteProductAsync(int id)`

Removes a product from the system.

- **Parameters**
  - `id` – The product identifier to delete.
- **Exceptions**
  - Throws `ArgumentException` if `id` is less than or equal to zero.
  - Throws `InvalidOperationException` if the product does not exist or the data store is unavailable.

## Usage

```csharp
// Example 1: Fetch and display a product
var productService = new ProductService(productRepository, categoryService);
var product = await productService.GetProductByIdAsync(42);
if (product != null)
{
    Console.WriteLine($"Product: {product.Name} (${product.Price})");
}

// Example 2: Create a new product
var newProduct = new ProductRequest
{
    Name = "Wireless Headphones",
    Description = "Noise-cancelling wireless headphones with 30-hour battery.",
    Price = 199.99m,
    Sku = "AUD-WH-001",
    CategorySlug = "electronics"
};
var created = await productService.CreateProductAsync(newProduct);
Console.WriteLine($"Created product ID: {created.Id}");
```

## Notes

- All methods are asynchronous and should be awaited to avoid blocking calls.
- Methods that accept identifiers (`id`) validate that values are positive; negative or zero values throw `ArgumentException`.
- Search and listing methods limit result sizes to prevent excessive memory usage; adjust `maxResults` within documented bounds.
- The service assumes the underlying data store is thread-safe; concurrent calls are supported provided the repository or context used internally is thread-safe.
- If the data store becomes unavailable, `InvalidOperationException` is thrown; implement retry logic at the caller level if transient faults are expected.
