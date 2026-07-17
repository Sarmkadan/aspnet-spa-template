# CreateProductRequest

Represents the data contract for creating a new product in the system. This type encapsulates all required and optional fields necessary to define a product entity, including its name, description, pricing, inventory quantity, categorization, and optional metadata such as an image URL or SKU. It is typically used as the body of an HTTP POST request to a product creation endpoint.

## API

### `public string Name`

Gets or sets the display name of the product. This value is required and must be a non-null, non-empty string. It serves as the primary human-readable identifier for the product in listings, search results, and detail views.

- **Parameters**: None (property setter accepts a `string` value).
- **Returns**: The current name assigned to the request (property getter returns `string`).
- **Throws**: No exceptions are thrown by the property itself. Validation failures (e.g., null or empty assignment) are typically deferred to model binding or explicit validation logic in the consuming layer.

### `public string Description`

Gets or sets the descriptive text for the product. This value is required and must be a non-null, non-empty string. It provides detailed information about the product’s features, specifications, or usage.

- **Parameters**: None (property setter accepts a `string` value).
- **Returns**: The current description assigned to the request (property getter returns `string`).
- **Throws**: No exceptions are thrown by the property itself. Validation is handled externally.

### `public decimal Price`

Gets or sets the unit price of the product. The value is expressed in the system’s base currency and must be a non-negative decimal. Precision and scale are determined by the underlying storage and validation rules.

- **Parameters**: None (property setter accepts a `decimal` value).
- **Returns**: The current price assigned to the request (property getter returns `decimal`).
- **Throws**: No exceptions are thrown by the property itself. Assigning a negative value does not cause an immediate exception but will typically result in a validation error downstream.

### `public int StockQuantity`

Gets or sets the initial quantity of the product available in inventory. Must be a non-negative integer. A value of zero indicates that the product is created with no stock on hand.

- **Parameters**: None (property setter accepts an `int` value).
- **Returns**: The current stock quantity assigned to the request (property getter returns `int`).
- **Throws**: No exceptions are thrown by the property itself. Negative values are rejected during validation.

### `public ProductCategory Category`

Gets or sets the category to which the product belongs. This is a required enumeration value of type `ProductCategory` that classifies the product for organization, filtering, and reporting purposes.

- **Parameters**: None (property setter accepts a `ProductCategory` value).
- **Returns**: The current category assigned to the request (property getter returns `ProductCategory`).
- **Throws**: No exceptions are thrown by the property itself. Assigning an undefined or out-of-range enum value may cause validation errors or unexpected behavior in downstream mapping.

### `public string? ImageUrl`

Gets or sets an optional URL pointing to an image representing the product. This value may be `null` if no image is provided. When supplied, it should be a valid, well-formed URI string.

- **Parameters**: None (property setter accepts a `string` or `null` value).
- **Returns**: The current image URL assigned to the request, or `null` if none is set (property getter returns `string?`).
- **Throws**: No exceptions are thrown by the property itself. URI format validation, if any, is performed by the consuming service.

### `public string? Sku`

Gets or sets an optional stock-keeping unit identifier for the product. This value may be `null` if no SKU is assigned. When provided, it is typically expected to be unique across the product catalog and conform to a defined format.

- **Parameters**: None (property setter accepts a `string` or `null` value).
- **Returns**: The current SKU assigned to the request, or `null` if none is set (property getter returns `string?`).
- **Throws**: No exceptions are thrown by the property itself. Uniqueness and format constraints are enforced by the persistence or validation layer.

## Usage

### Example 1: Creating a Basic Product with Required Fields

```csharp
var request = new CreateProductRequest
{
    Name = "Wireless Ergonomic Mouse",
    Description = "A comfortable, wireless mouse with adjustable DPI settings and a contoured grip.",
    Price = 49.99m,
    StockQuantity = 150,
    Category = ProductCategory.Electronics
};

// Typically sent to an API endpoint:
// var response = await productService.CreateAsync(request);
```

### Example 2: Creating a Product with All Optional Fields

```csharp
var request = new CreateProductRequest
{
    Name = "Organic Green Tea",
    Description = "Loose-leaf organic green tea sourced from high-altitude farms. Rich in antioxidants.",
    Price = 12.50m,
    StockQuantity = 300,
    Category = ProductCategory.FoodAndBeverage,
    ImageUrl = "https://assets.example.com/images/green-tea.jpg",
    Sku = "TEA-GRN-ORG-250G"
};

// The SKU and ImageUrl are optional; omitting them is valid.
// var response = await productService.CreateAsync(request);
```

## Notes

- **Validation**: None of the properties enforce validation at the point of assignment. All validation (required fields, range checks, enum validity, URI format, SKU uniqueness) is expected to be performed by a separate validation layer, such as data annotations, FluentValidation, or explicit checks in the service handling the request.
- **Default Values**: Numeric properties (`Price`, `StockQuantity`) default to `0` if not explicitly set. Reference-type properties (`Name`, `Description`, `ImageUrl`, `Sku`) default to `null`. `Category` defaults to the underlying zero-value of the `ProductCategory` enum, which may or may not represent a valid category depending on the enum definition.
- **Thread Safety**: This type is a plain data transfer object with public get/set properties. It provides no internal synchronization. Instances are not thread-safe when mutated concurrently across multiple threads. In typical usage, instances are constructed, populated, and passed to a service on a single thread, making concurrent mutation unlikely.
- **Immutability**: The type is fully mutable by design to facilitate model binding and straightforward object initialization. If immutability is desired, consumers should treat the instance as read-only after construction or map it to an immutable domain object.
- **Serialization**: As a simple POCO, this type serializes cleanly to JSON or XML using default serializers. Properties with `null` values (`ImageUrl`, `Sku`) are typically omitted or serialized as `null` depending on serializer configuration.
