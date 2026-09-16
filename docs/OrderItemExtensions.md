# OrderItemExtensions

OrderItemExtensions provides extension methods for calculating an order item's line total, checking whether a discount is applied, and creating a human-readable summary of the item.

## API

### LineTotal
```csharp
public static decimal LineTotal(this OrderItem item)
```
Calculates the line total as `(UnitPrice * Quantity) - Discount`, clamps negative results to zero, and rounds the result to two decimal places.
- **Parameters**
  - `item`: The order item whose line total is calculated.
- **Return value**
  The non-negative line total rounded to two decimal places.
- **Exceptions**
  - `ArgumentNullException` if `item` is `null`.

### IsDiscounted
```csharp
public static bool IsDiscounted(this OrderItem item)
```
Determines whether the order item has a positive discount.
- **Parameters**
  - `item`: The order item to inspect.
- **Return value**
  `true` when `Discount` is greater than zero; otherwise `false`.
- **Exceptions**
  - `ArgumentNullException` if `item` is `null`.

### ToDisplayString
```csharp
public static string ToDisplayString(this OrderItem item)
```
Creates a human-readable summary containing the quantity, currency-formatted unit price, optional discount, and calculated line total.
- **Parameters**
  - `item`: The order item to format.
- **Return value**
  A display string in the form `quantity × unit price = line total`. When a positive discount is present, the discount is included in parentheses before the line total.
- **Exceptions**
  - `ArgumentNullException` if `item` is `null`.

## Usage

### Example 1: Calculating a line total
```csharp
var item = new OrderItem
{
    Quantity = 3,
    UnitPrice = 12.345m,
    Discount = 0m
};

decimal total = item.LineTotal(); // 37.04
bool discounted = item.IsDiscounted(); // false
```

### Example 2: Applying a discount
```csharp
var item = new OrderItem
{
    Quantity = 2,
    UnitPrice = 20m,
    Discount = 2m
};

decimal total = item.LineTotal(); // 38.00
bool discounted = item.IsDiscounted(); // true
```

### Example 3: Creating a display string
```csharp
var item = new OrderItem
{
    Quantity = 2,
    UnitPrice = 20m,
    Discount = 2m
};

string display = item.ToDisplayString();
// In the en-US culture: "2 × $20.00 (−$2.00) = $38.00"
```

## Notes
- `LineTotal` does not include `TaxAmount` and does not read or update the `Total` property.
- A discount equal to or greater than the subtotal produces a line total of zero; negative totals are never returned.
- `LineTotal` uses `Math.Round(decimal, 2)`, whose default midpoint rounding mode is to the nearest even number.
- `IsDiscounted` returns `false` for zero and negative discount values.
- `ToDisplayString` uses the current culture's currency formatting, so currency symbols and number formatting can vary by environment.
- The methods do not modify the order item.
