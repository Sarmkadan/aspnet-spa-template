# ProductExtensions

ProductExtensions provides extension methods for checking product availability, building a human-readable product name, and calculating a discounted price without modifying the `Product` instance.

## API

### IsInStock
```csharp
public static bool IsInStock(this Product product)
```
Determines whether the product has stock available and is marked as available.
- **Parameters**
  - `product`: The product to check.
- **Return value**
  `true` when `StockQuantity` is greater than zero and `IsAvailable` is `true`; otherwise `false`.
- **Exceptions**
  - `ArgumentNullException` if `product` is `null`.

### DisplayName
```csharp
public static string DisplayName(this Product product)
```
Builds a display name from the product name, optional SKU, and category display name. The SKU is enclosed in parentheses and omitted when it is `null`, empty, or whitespace. The category is included only when its value is defined by `ProductCategory`.
- **Parameters**
  - `product`: The product for which to build a display name.
- **Return value**
  A space-separated string containing the product name, optional SKU, and category display name.
- **Exceptions**
  - `ArgumentNullException` if `product` is `null`.

### ApplyDiscount
```csharp
public static decimal ApplyDiscount(this Product product, decimal percent)
```
Calculates the product price after applying a percentage discount. The calculation does not update `Price` or any other product property.
- **Parameters**
  - `product`: The product whose price is used in the calculation.
  - `percent`: The discount percentage, from `0m` through `100m`, inclusive.
- **Return value**
  The value of `Price * (1m - (percent / 100m))`.
- **Exceptions**
  - `ArgumentNullException` if `product` is `null`.
  - `ArgumentOutOfRangeException` if `percent` is less than `0m` or greater than `100m`.

## Usage

### Example 1: Checking stock and creating a display name
```csharp
var product = new Product
{
    Name = "Laptop",
    Sku = "LAP-100",
    Category = ProductCategory.Electronics,
    StockQuantity = 5,
    IsAvailable = true
};

bool inStock = ProductExtensions.IsInStock(product); // true
string name = product.DisplayName();                 // "Laptop (LAP-100) Electronics"
```

### Example 2: Omitting a missing SKU and formatting a category
```csharp
var product = new Product
{
    Name = "Planter",
    Sku = null,
    Category = ProductCategory.Home
};

string name = product.DisplayName(); // "Planter Home & Garden"
```

### Example 3: Calculating discounts without changing the product
```csharp
var product = new Product { Price = 80m };

decimal salePrice = product.ApplyDiscount(25m); // 60m
decimal fullPrice = product.ApplyDiscount(0m);  // 80m
decimal freePrice = product.ApplyDiscount(100m); // 0m

Console.WriteLine(product.Price); // 80m
```

## Notes
- All methods validate that `product` is not `null`.
- `IsInStock` requires both a positive stock quantity and an available product. A product with zero or negative stock is not in stock even when `IsAvailable` is `true`.
- `Product` also defines an instance method named `IsInStock` with the same result. Normal `product.IsInStock()` syntax resolves to that instance method; call `ProductExtensions.IsInStock(product)` when explicitly invoking the extension method.
- `DisplayName` uses the category's `ToDisplayName` mapping, so categories such as `Home` are rendered as `Home & Garden`.
- For an undefined `ProductCategory` value, `DisplayName` omits the category rather than using the mapping's fallback value.
- `ApplyDiscount` accepts the boundary values `0m` and `100m` and performs decimal arithmetic without rounding.
- None of the extension methods modify the product.
