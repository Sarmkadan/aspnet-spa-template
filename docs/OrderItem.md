# OrderItem

Represents a single line item within an `Order`, tracking the product, quantity, pricing details, and tax/discount calculations. This entity is central to order processing, inventory management, and financial reporting, serving as the bridge between `Order` and `Product` aggregates. It encapsulates both the static data (e.g., `ProductId`, `Quantity`) and dynamic calculations (e.g., `Total`, `TaxAmount`) required for accurate order fulfillment and invoicing.

## API

### Properties

#### `public int Id`
Unique identifier for the order item. Assigned by the persistence layer upon creation. Read-only after initialization.

#### `public int OrderId`
Foreign key referencing the associated `Order`. Non-nullable; must correspond to an existing `Order.Id`.

#### `public int ProductId`
Foreign key referencing the associated `Product`. Non-nullable; must correspond to an existing `Product.Id`.

#### `public int Quantity`
Number of units of the product included in this order item. Must be ≥ 1. Throws `ArgumentOutOfRangeException` if set to a non-positive value.

#### `public decimal UnitPrice`
Base price per unit of the product at the time of order creation. Read-only after initialization; reflects the product's `ListPrice` unless overridden by promotions or manual adjustments.

#### `public decimal TaxAmount`
Calculated tax amount for this order item, derived from `UnitPrice`, `Quantity`, and applicable tax rates. Updated via `RecalculateTotal()`.

#### `public decimal Discount`
Discount applied to this order item, expressed as an absolute value (not percentage). Defaults to 0.00. Must be ≤ `GetSubtotal()`. Throws `ArgumentOutOfRangeException` if set to a negative value.

#### `public decimal Total`
Final amount for this order item, including tax and discounts. Updated via `RecalculateTotal()`. Read-only; use `RecalculateTotal()` to refresh after modifications to `Quantity`, `Discount`, or tax rates.

#### `public Order? Order`
Navigation property to the parent `Order`. Lazy-loaded; may be `null` if not explicitly loaded.

#### `public Product? Product`
Navigation property to the associated `Product`. Lazy-loaded; may be `null` if not explicitly loaded.

### Methods

#### `public decimal GetSubtotal()`
Calculates the subtotal before tax and discounts.
**Returns**: `UnitPrice * Quantity`.
**Throws**: None.

#### `public decimal GetTotalWithTax()`
Calculates the total including tax but excluding discounts.
**Returns**: `GetSubtotal() + TaxAmount`.
**Throws**: None.

#### `public void RecalculateTotal()`
Updates `TaxAmount` and `Total` based on current `UnitPrice`, `Quantity`, `Discount`, and applicable tax rates. Invokes tax calculation logic (e.g., `TaxService.CalculateTax()`) if tax rates are dynamic.
**Parameters**: None.
**Returns**: Void.
**Throws**: `InvalidOperationException` if `Product` is `null` and tax rates cannot be determined.

#### `public decimal GetDiscount()`
Retrieves the current discount value.
**Returns**: `Discount`.
**Throws**: None.

#### `public void ApplyDiscount(decimal discountAmount)`
Applies a discount to the order item. Adjusts `Discount` and triggers `RecalculateTotal()`.
**Parameters**:
- `discountAmount`: Absolute discount value. Must be ≤ `GetSubtotal()`.
**Returns**: Void.
**Throws**:
- `ArgumentOutOfRangeException` if `discountAmount` is negative or exceeds `GetSubtotal()`.

#### `public decimal GetAveragePricePerUnit()`
Calculates the average price per unit after discounts.
**Returns**: `(GetSubtotal() - Discount) / Quantity`.
**Throws**: `DivideByZeroException` if `Quantity` is 0 (should never occur due to guard clauses).

#### `public bool IsValid()`
Validates the order item's state.
**Returns**: `true` if:
- `Quantity` ≥ 1,
- `Discount` ≤ `GetSubtotal()`,
- `ProductId` and `OrderId` reference existing entities (if navigation properties are loaded).
**Throws**: None.

## Usage

### Example 1: Creating and Calculating an Order Item
