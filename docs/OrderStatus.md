# OrderStatus

Enumeration for order statuses.

## Values

### `public OrderStatus Pending = 0`
Order has been placed but not yet confirmed for processing.

### `public OrderStatus Confirmed = 1`
Order has been confirmed and is ready for processing.

### `public OrderStatus Processing = 2`
Order is being prepared for shipment.

### `public OrderStatus Shipped = 3`
Order has been shipped to the customer.

### `public OrderStatus Delivered = 4`
Order has been delivered to the customer.

### `public OrderStatus Cancelled = 5`
Order has been cancelled before delivery.

### `public OrderStatus Refunded = 6`
Order has been refunded after delivery or cancellation.