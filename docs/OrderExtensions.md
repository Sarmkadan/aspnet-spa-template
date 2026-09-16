# OrderExtensions

OrderExtensions provides extension methods for calculating values and querying the line items associated with an `Order`. The methods return safe defaults when the order or its `Items` collection is `null`.

## API

### TotalAmount
```csharp
public static decimal TotalAmount(this Order order)
```
Calculates the order total from its line items. For each item, the method multiplies `UnitPrice` by `Quantity`, adds `TaxAmount`, and subtracts `Discount`, then sums the results.
- **Parameters**
  - `order`: The order whose line items are totaled.
- **Return value**
  The sum of `(UnitPrice * Quantity) + TaxAmount - Discount` for all items. Returns `0m` when `order` or `order.Items` is `null`, and when the collection is empty.
- **Exceptions**
  None under normal use.

### ItemCount
```csharp
public static int ItemCount(this Order order)
```
Calculates the total quantity of products across all line items in the order.
- **Parameters**
  - `order`: The order whose item quantities are counted.
- **Return value**
  The sum of `Quantity` for all items. Returns `0` when `order` or `order.Items` is `null`, and when the collection is empty.
- **Exceptions**
  None under normal use.

### ContainsProduct
```csharp
public static bool ContainsProduct(this Order order, int productId)
```
Determines whether any line item in the order has the specified product identifier.
- **Parameters**
  - `order`: The order to search.
  - `productId`: The product identifier to match against each item's `ProductId`.
- **Return value**
  `true` when at least one item has a matching `ProductId`; otherwise `false`. Returns `false` when `order` or `order.Items` is `null`.
- **Exceptions**
  None under normal use.

## Usage

### Example 1: Calculating the total amount and item count
```csharp
var order = new Order
{
    Items = new List<OrderItem>
    {
        new OrderItem
        {
            ProductId = 101,
            Quantity = 2,
            UnitPrice = 25m,
            TaxAmount = 5m,
            Discount = 10m
        },
        new OrderItem
        {
            ProductId = 202,
            Quantity = 1,
            UnitPrice = 40m,
            TaxAmount = 4m
        }
    }
};

decimal total = order.TotalAmount(); // 89m
int itemCount = order.ItemCount();    // 3
```

### Example 2: Checking for a product
```csharp
bool containsProduct = order.ContainsProduct(202); // true
bool containsOtherProduct = order.ContainsProduct(303); // false
```

### Example 3: Handling an order without loaded items
```csharp
var order = new Order { Items = null };

decimal total = order.TotalAmount();              // 0m
int itemCount = order.ItemCount();                 // 0
bool containsProduct = order.ContainsProduct(101); // false
```

## Notes
- `TotalAmount` calculates a value from the line items and does not read or update the `Order.Total` property.
- Discounts are subtracted per line item. The method does not clamp negative item totals or the final result to zero.
- `ItemCount` sums item quantities rather than counting entries in the `Items` collection.
- `ContainsProduct` compares integer product identifiers for exact equality and stops when it finds a match.
- The methods do not modify the order or its line items.
