# Order

Represents a customer order in the e-commerce system, tracking financial totals, lifecycle timestamps, associated user and line items. This entity is persisted in the database and serves as the central record for order processing, fulfillment, and reporting.

## API

### `public int Id`
Unique identifier for the order. Assigned by the database upon creation. Never modified after insertion.

### `public int UserId`
Foreign key to the `User` who placed the order. Required; cannot be zero or negative.

### `public string OrderNumber`
Human-readable identifier generated at order creation (e.g., `"ORD-2024-000123"`). Immutable after assignment.

### `public OrderStatus Status`
Current state of the order (`Pending`, `Processing`, `Shipped`, `Delivered`, `Cancelled`, `Refunded`). Transitions follow a strict workflow; invalid transitions throw `InvalidOperationException`.

### `public decimal SubTotal`
Sum of all line item prices before taxes, shipping, or discounts. Always ≥ 0.

### `public decimal TaxAmount`
Calculated tax amount based on jurisdiction rules. Always ≥ 0.

### `public decimal ShippingCost`
Cost of shipping. Always ≥ 0.

### `public decimal Discount`
Total discount applied to the order (e.g., coupon or loyalty points). Always ≤ 0.

### `public decimal Total`
Final amount charged (`SubTotal + TaxAmount + ShippingCost + Discount`). Always ≥ 0.

### `public string? ShippingAddress`
Full shipping address in a single string. Null if digital-only order.

### `public string? BillingAddress`
Full billing address in a single string. Null if same as shipping address.

### `public string? Notes`
Optional internal notes (e.g., gift message, special instructions). Null if none provided.

### `public DateTime OrderedAt`
Timestamp when the order was placed. Immutable after creation.

### `public DateTime? ShippedAt`
Timestamp when the order was shipped. Null if not yet shipped.

### `public DateTime? DeliveredAt`
Timestamp when the order was delivered. Null if not yet delivered.

### `public DateTime? CancelledAt`
Timestamp when the order was cancelled. Null if not cancelled.

### `public User? User`
Navigation property to the associated `User`. Null if not loaded (lazy-loading disabled).

### `public ICollection<OrderItem>? Items`
Navigation property to the line items. Null if not loaded. Each item references a `Product` and tracks quantity and price.

### `public int GetTotalItems()`
Returns the sum of quantities across all line items. Throws `InvalidOperationException` if `Items` is null or empty.

### `public bool CanBeCancelled()`
Returns `true` if the order can still be cancelled (i.e., `Status` is `Pending` or `Processing`). Returns `false` otherwise.

## Usage

### Example 1: Creating and Persisting an Order
