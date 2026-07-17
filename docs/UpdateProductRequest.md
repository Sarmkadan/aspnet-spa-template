# UpdateProductRequest

The `UpdateProductRequest` type is a data transfer object (DTO) used to capture the fields required to modify an existing product in the `aspnet-spa-template` application. It combines an identifier for the target product with the set of mutable properties that can be updated, and includes a validation method to enforce business rules before the request is processed.

## API

### Properties

| Member | Type | Description |
|--------|------|-------------|
| `Id` | `int` | The unique identifier of the product to update. Must correspond to an existing product in the data store. |
| `Name` | `string` | The new name of the product. Must not be null or empty after trimming. |
| `Description` | `string` | The new description of the product. May be empty but must not be null. |
| `Price` | `decimal` | The new price of the product. Must be greater than or equal to zero. |
| `StockQuantity` | `int` | The new stock quantity. Must be non-negative. |
| `Category` | `ProductCategory` | The new category of the product, expressed as a member of the `ProductCategory` enum. |
| `ImageUrl` | `string?` | An optional URL pointing to an image for the product. If provided, must be a valid absolute URI. |
| `IsAvailable` | `bool` | Indicates whether the product should be marked as available for purchase. |
| `Sku` | `string?` | An optional stock-keeping unit identifier. If provided, must be unique among all products. |
| `Rating` | `decimal` | The average customer rating for the product. Typically between 0 and 5. |
| `ReviewCount` | `int` | The number of customer reviews that contribute to the rating. Must be non-negative. |
| `IsFeatured` | `bool` | Indicates whether the product should be highlighted as a featured item. |

### Methods

#### `void Validate()`

Validates the current state of the request against business rules. This method should be called before the request is passed to the update handler.

**Parameters:** None.

**Returns:** Nothing.

**Exceptions:**
- `ArgumentException` – Thrown if `Name` is null or empty after trimming.
- `ArgumentOutOfRangeException` – Thrown if `Price` is negative, `StockQuantity` is negative, or `ReviewCount` is negative.
- `InvalidOperationException` – Thrown if `ImageUrl` is not null and is not a valid absolute URI, or if `Sku` is not null and conflicts with an existing product’s SKU (the uniqueness check is performed against the current data store at validation time).
- `ValidationException` – Thrown if any other validation rule (e.g., category is undefined) is violated.

## Usage

### Example 1: Basic product update

```csharp
var request = new UpdateProductRequest
{
    Id = 42,
    Name = "Wireless Mouse",
    Description = "Ergonomic wireless mouse with USB receiver.",
    Price = 29.99m,
    StockQuantity = 150,
    Category = ProductCategory.Electronics,
    ImageUrl = "https://example.com/images/mouse.png",
    IsAvailable = true,
    Sku = "WM-2024-001",
    Rating = 4.5m,
    ReviewCount = 32,
    IsFeatured = false
};

try
{
    request.Validate();
    // Pass request to the update service
    await productService.UpdateProductAsync(request);
}
catch (ValidationException ex)
{
    logger.LogError("Product update validation failed: {Message}", ex.Message);
}
```

### Example 2: Partial update with optional fields omitted

```csharp
var request = new UpdateProductRequest
{
    Id = 7,
    Name = "Classic T-Shirt",
    Description = "Cotton crew neck t-shirt.",
    Price = 19.99m,
    StockQuantity = 500,
    Category = ProductCategory.Clothing,
    ImageUrl = null,   // keep existing image
    IsAvailable = true,
    Sku = null,        // keep existing SKU
    Rating = 4.2m,
    ReviewCount = 128,
    IsFeatured = true
};

request.Validate();

// The update handler will only modify fields that are explicitly set;
// null or default values for optional fields (ImageUrl, Sku) are treated
// as "do not update".
await productService.UpdateProductAsync(request);
```

## Notes

- **Thread safety:** `UpdateProductRequest` is not thread-safe. Its properties are mutable and intended to be set on a single thread before calling `Validate()` and passing the instance to a service. Concurrent reads or writes to the same instance may produce inconsistent state.
- **Validation side effects:** The `Validate()` method may perform I/O (e.g., checking SKU uniqueness against a database). In such cases, the method should be called within a scope that can handle transient failures. Repeated calls to `Validate()` on the same instance may yield different results if the underlying data changes.
- **Null handling:** All string properties (`Name`, `Description`, `ImageUrl`, `Sku`) are nullable except `Name` and `Description`, which must be non-null after validation. Setting `ImageUrl` or `Sku` to `null` is valid and typically signals that the existing value should be retained.
- **Enum values:** The `Category` property must be a defined member of the `ProductCategory` enum. Passing an undefined value will cause `Validate()` to throw a `ValidationException`.
- **Inherited members:** The properties `Id`, `Sku`, `Rating`, `ReviewCount`, and `IsFeatured` are inherited from a base class. Their behavior is identical to the base class implementation; no additional constraints are imposed by `UpdateProductRequest`.
