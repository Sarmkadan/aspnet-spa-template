# OrderItemRequest

Represents a request or data-transfer object used to capture line-item details, order-level charges, and nested item collections within the application's ordering workflow. It combines product identification, pricing breakdowns, address information, and optional notes into a single structure that can be submitted by clients and processed by order-handling services.

## API

### `int ProductId`
The unique identifier of the product being ordered. Used to look up catalog information and validate availability during order processing.

### `int Quantity`
The number of units requested for the associated product. Must be a positive integer; zero or negative values may be rejected by validation logic downstream.

### `List<OrderItemRequest> Items`
A nested collection of `OrderItemRequest` instances, enabling hierarchical or bundled order structures where a parent item contains child line items. Each element follows the same shape described in this document.

### `string? ShippingAddress`
An optional shipping destination address. When `null`, the order may default to the customer's stored shipping address or require explicit input before fulfillment.

### `string? BillingAddress`
An optional billing address. When `null`, the system may fall back to the shipping address or the customer's default billing address, depending on business rules.

### `string? Notes`
Free-form text for special instructions, gift messages, or delivery notes. Nullable; no length constraint is enforced at the type level, but persistence or validation layers may impose limits.

### `int Id`
A generic identifier field. Depending on context, this may represent the order item's own record ID, a correlation ID, or a client-supplied reference. Interpretation is determined by the consuming service.

### `string ProductName`
The human-readable name of the product, typically resolved from the catalog and populated by the server or included by the client for display purposes.

### `decimal UnitPrice`
The price per single unit of the product before tax and discounts. Represented as a decimal for monetary precision.

### `decimal TaxAmount`
The tax applied to this line item. Calculated based on `UnitPrice`, `Quantity`, `Discount`, and applicable tax rates.

### `decimal Discount`
The discount amount applied to this line item. Subtracted from the gross item total before tax calculations, depending on pricing rules.

### `decimal Total`
The final total for this line item after applying quantity, discounts, and tax. Typically computed as `(UnitPrice * Quantity) - Discount + TaxAmount`.

### `string OrderNumber`
A system-generated or client-provided order reference number. Used to correlate the item with its parent order in multi-item submissions.

### `string Status`
The current processing status of the order or item (e.g., "Pending", "Shipped", "Cancelled"). Accepted values are determined by the order state machine.

### `decimal SubTotal`
The sum of line-item totals before tax and shipping are applied at the order level. May represent an aggregated value across all items when used in a parent context.

### `decimal ShippingCost`
The cost associated with shipping this item or the entire order. Interpretation depends on whether the instance represents a single item or an order summary.

## Usage

### Example 1: Creating a Single Line Item
```csharp
var item = new OrderItemRequest
{
    ProductId = 42,
    ProductName = "Wireless Keyboard",
    Quantity = 2,
    UnitPrice = 79.99m,
    Discount = 5.00m,
    TaxAmount = 11.62m,
    Total = 166.60m,
    ShippingAddress = "123 Main St, Springfield, IL",
    Notes = "Leave at the back door if no answer."
};

// Submit as part of an order payload
await orderService.SubmitOrderAsync(new List<OrderItemRequest> { item });
```

### Example 2: Bundled Order with Nested Items
```csharp
var bundle = new OrderItemRequest
{
    Id = 1001,
    OrderNumber = "ORD-2025-0042",
    Status = "Pending",
    SubTotal = 349.97m,
    TaxAmount = 27.99m,
    ShippingCost = 12.50m,
    BillingAddress = "456 Corporate Dr, Suite 200, Austin, TX",
    Items = new List<OrderItemRequest>
    {
        new OrderItemRequest
        {
            ProductId = 10,
            ProductName = "USB-C Hub",
            Quantity = 1,
            UnitPrice = 49.99m,
            Total = 49.99m
        },
        new OrderItemRequest
        {
            ProductId = 22,
            ProductName = "Monitor Stand",
            Quantity = 2,
            UnitPrice = 149.99m,
            Discount = 20.00m,
            TaxAmount = 22.39m,
            Total = 302.37m
        }
    }
};

await orderService.SubmitBundleAsync(bundle);
```

## Notes

- **Field duplication:** The type exposes `ProductId`, `Quantity`, `Id`, and `TaxAmount` more than once. Consumers must ensure they reference the correct occurrence for their context; serializers may produce ambiguous output if all are populated simultaneously. Prefer using a single, well-defined set per instance.
- **Nullable addresses:** `ShippingAddress` and `BillingAddress` are nullable. Validation layers may reject submissions where both are `null` if no customer default exists. Always check downstream requirements before omitting them.
- **Nested `Items` recursion:** The `Items` property allows arbitrary nesting depth. Implementations should guard against circular references or excessive depth that could cause stack overflows during serialization or recursive validation.
- **Monetary precision:** All `decimal` fields (`UnitPrice`, `TaxAmount`, `Discount`, `Total`, `SubTotal`, `ShippingCost`) should be rounded to the currency's minor unit (typically two decimal places) before submission to avoid floating-point drift or rounding disputes.
- **Thread safety:** This type is a plain data object with no synchronization mechanisms. Instances are not safe for concurrent mutation. If shared across threads, treat them as immutable snapshots or use locking/synchronization externally.
- **Status values:** The `Status` field has no enum constraint at this level. Passing unrecognized values may result in validation errors or silent persistence of invalid states depending on the backend implementation.
