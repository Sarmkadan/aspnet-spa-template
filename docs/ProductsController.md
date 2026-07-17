# ProductsController

The `ProductsController` is an ASP.NET Core Web API controller that exposes endpoints for managing product data in the application. It provides operations to retrieve, create, update, and delete products, as well as to query products by category, featured status, rating, and search terms. The controller relies on injected services for data access and business logic, returning standard HTTP responses wrapped in `IActionResult`.

## API

### GetProduct
- **Purpose:** Returns the details of a single product identified by its ID.
- **Parameters:** `id` (int) – the unique identifier of the product to retrieve.
- **Return value:** `Task<IActionResult>` – yields `200 OK` with a product DTO when found, or `404 Not Found` if no product matches the ID.
- **When it throws:** May throw `ArgumentException` if `id` is less than or equal to zero; any underlying data‑access exceptions are propagated as `500 Internal Server Error`.

### GetProducts
- **Purpose:** Returns a collection of all products.
- **Parameters:** None.
- **Return value:** `Task<IActionResult>` – yields `200 OK` with an array of product DTOs; returns an empty array if no products exist.
- **When it throws:** Propagates exceptions from the data layer as `500 Internal Server Error`.

### GetProductsByCategory
- **Purpose:** Returns products that belong to a specified category.
- **Parameters:** `categoryId` (int) – the identifier of the category to filter by.
- **Return value:** `Task<IActionResult>` – yields `200 OK` with an array of matching product DTOs, or `404 Not Found` if the category has no products.
- **When it throws:** Throws `ArgumentException` for invalid `categoryId` (≤0); data‑access errors become `500 Internal Server Error`.

### GetFeaturedProducts
- **Purpose:** Returns products marked as featured.
- **Parameters:** None.
- **Return value:** `Task<IActionResult>` – yields `200 OK` with an array of featured product DTOs; empty array if none are featured.
- **When it throws:** Propagates data‑access exceptions as `500 Internal Server Error`.

### GetTopRatedProducts
- **Purpose:** Returns the top‑rated products, optionally limited by a count.
- **Parameters:** `count` (int, optional) – maximum number of products to return; if omitted, a default count is used.
- **Return value:** `Task<IActionResult>` – yields `200 OK` with an array of the highest‑rated product DTOs.
- **When it throws:** Throws `ArgumentException` if `count` is less than 1; data‑access errors become `500 Internal Server Error`.

### SearchProducts
- **Purpose:** Returns products whose name or description matches a search term.
- **Parameters:** 
  - `searchTerm` (string) – the text to search for.
  - `pageIndex` (int, optional) – zero‑based page number for pagination.
  - `pageSize` (int, optional) – number of items per page.
- **Return value:** `Task<IActionResult>` – yields `200 OK` with a paginated result set of matching product DTOs.
- **When it throws:** Throws `ArgumentNullException` if `searchTerm` is null; throws `ArgumentOutOfRangeException` for negative pagination values; data‑access errors become `500 Internal Server Error`.

### CreateProduct
- **Purpose:** Creates a new product.
- **Parameters:** `productDto` (ProductCreateDto) – DTO containing the data for the new product.
- **Return value:** `Task<IActionResult>` – yields `201 Created` with the newly created product DTO and location header, or `400 Bad Request` if the DTO fails validation.
- **When it throws:** Throws `ArgumentNullException` if `productDto` is null; validation failures result in `400 Bad Request`; unexpected errors propagate as `500 Internal Server Error`.

### UpdateProduct
- **Purpose:** Updates an existing product.
- **Parameters:** 
  - `id` (int) – identifier of the product to update.
  - `productDto` (ProductUpdateDto) – DTO containing the updated values.
- **Return value:** `Task<IActionResult>` – yields `204 No Content` on success, `400 Bad Request` for validation errors, or `404 Not Found` if the product does not exist.
- **When it throws:** Throws `ArgumentException` if `id` is invalid; throws `ArgumentNullException` if `productDto` is null; validation errors lead to `400 Bad Request`; data‑access errors become `500 Internal Server Error`.

### SetAvailability
- **Purpose:** Updates the availability status (in stock / out of stock) of a product.
- **Parameters:** 
  - `id` (int) – identifier of the product.
  - `isAvailable` (bool) – new availability flag.
- **Return value:** `Task<IActionResult>` – yields `204 No Content` on success, or `404 Not Found` if the product does not exist.
- **When it throws:** Throws `ArgumentException` for invalid `id`; propagates data‑access errors as `500 Internal Server Error`.

### SetFeatured
- **Purpose:** Sets or clears the featured flag for a product.
- **Parameters:** 
  - `id` (int) – identifier of the product.
  - `isFeatured` (bool) – new featured flag.
- **Return value:** `Task<IActionResult>` – yields `204 No Content` on success, or `404 Not Found` if the product does not exist.
- **When it throws:** Throws `ArgumentException` for invalid `id`; data‑access errors become `500 Internal Server Error`.

### DeleteProduct
- **Purpose:** Removes a product from the system.
- **Parameters:** `id` (int) – identifier of the product to delete.
- **Return value:** `Task<IActionResult>` – yields `204 No Content` on successful deletion, or `404 Not Found` if the product does not exist.
- **When it throws:** Throws `ArgumentException` for invalid `id`; propagates data‑access errors as `500 Internal Server Error`.

## Usage

```csharp
// Example 1: Using the controller directly in a unit test or middleware
var controller = new ProductsController(productService, mapper);
var result = await controller.GetProduct(42);
var okResult = result as OkObjectResult;
ProductDto product = okResult?.Value as ProductDto;
// product now contains the requested product or null if not found
```

```csharp
// Example 2: Calling the API from an HttpClient (typical client‑side usage)
using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
HttpResponseMessage response = await client.GetAsync("api/products/search?searchTerm=laptop&pageIndex=0&pageSize=10");
response.EnsureSuccessStatusCode();
string json = await response.Content.ReadAsStringAsync();
var searchResult = JsonSerializer.Deserialize<ProductSearchResult>(json);
// searchResult.Items contains the matching products
```

## Notes

- The controller is stateless; all dependencies are injected via the constructor, making it safe for concurrent request handling as long as those dependencies (services, repositories) are thread‑safe.
- Methods that accept identifiers (`id`, `categoryId`) validate that the value is greater than zero; invalid values result in a 400‑level response before any data access is attempted.
- Update and creation operations rely on model validation; if the supplied DTO fails validation, the controller returns a 400 Bad Request response containing validation errors.
- Delete, SetAvailability, and SetFeatured operations are idempotent: repeating the call with the same parameters yields the same outcome (204 No Content) after the first successful execution.
- In the event of a database concurrency conflict (e.g., two users updating the same product simultaneously), the underlying service may throw a `DbUpdateConcurrencyException`, which propagates as a 500 Internal Server Error; callers should consider implementing retry logic or handling the exception at a higher layer if needed.
