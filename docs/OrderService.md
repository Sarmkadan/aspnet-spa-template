# OrderService

The `OrderService` class provides business logic for managing orders within the application. It handles operations such as retrieving orders, creating new orders, updating order statuses, applying discounts, and calculating revenue metrics. This service interacts with the underlying data store to perform these operations asynchronously.

## API

### `OrderService`
The constructor initializes the service. Dependency injection is typically used to provide required dependencies (e.g., database context, logging).

---

### `Task<OrderResponse?> GetOrderByIdAsync(int orderId)`
Retrieves a single order by its unique identifier.

**Parameters:**
- `orderId` (`int`): The ID of the order to retrieve.

**Returns:**
- `Task<OrderResponse?>`: The order details if found; otherwise, `null`.

**Throws:**
- `ArgumentException`: If `orderId` is less than or equal to zero.

---

### `Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)`
Creates a new order based on the provided request data.

**Parameters:**
- `request` (`CreateOrderRequest`): Contains details required to create an order (e.g., user ID, items, shipping information).

**Returns:**
- `Task<OrderResponse>`: The created order with its generated ID and status.

**Throws:**
- `ArgumentNullException`: If `request` is `null`.
- `ValidationException`: If the request data is invalid (e.g., empty items list, negative prices).

---

### `Task<OrderResponse> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)`
Updates the status of an existing order.

**Parameters:**
- `orderId` (`int`): The ID of the order to update.
- `newStatus` (`OrderStatus`): The new status to assign to the order.

**Returns:**
- `Task<OrderResponse>`: The updated order details.

**Throws:**
- `ArgumentException`: If `orderId` is invalid or `newStatus` is not a valid transition.
- `KeyNotFoundException`: If no order with the specified `orderId` exists.

---

### `Task<OrderResponse> ApplyDiscountAsync(int orderId, decimal discountAmount)`
Applies a discount to an existing order, adjusting the total price accordingly.

**Parameters:**
- `orderId` (`int`): The ID of the order to update.
- `discountAmount` (`decimal`): The discount value to apply.

**Returns:**
- `Task<OrderResponse>`: The updated order with the new total price.

**Throws:**
- `ArgumentException`: If `orderId` is invalid or `discountAmount` is negative or exceeds the order total.
- `KeyNotFoundException`: If no order with the specified `orderId` exists.
- `InvalidOperationException`: If the order status does not allow discounts (e.g., already shipped).

---

### `Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(int userId)`
Retrieves all orders associated with a specific user.

**Parameters:**
- `userId` (`int`): The ID of the user whose orders are to be retrieved.

**Returns:**
- `Task<IEnumerable<OrderResponse>>`: A collection of orders for the user. Returns an empty collection if no orders exist.

**Throws:**
- `ArgumentException`: If `userId` is less than or equal to zero.

---

### `Task<IEnumerable<OrderResponse>> GetPendingOrdersAsync()`
Retrieves all orders with a status of "Pending" (or equivalent).

**Returns:**
- `Task<IEnumerable<OrderResponse>>`: A collection of pending orders. Returns an empty collection if none exist.

**Throws:**
- None.

---

### `Task<decimal> GetTotalRevenueAsync()`
Calculates the total revenue generated from all completed orders.

**Returns:**
- `Task<decimal>`: The sum of all order totals for completed orders.

**Throws:**
- None.

---

### `Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)`
Calculates the total revenue generated from completed orders within a specified date range.

**Parameters:**
- `startDate` (`DateTime`): The start of the date range (inclusive).
- `endDate` (`DateTime`): The end of the date range (inclusive).

**Returns:**
- `Task<decimal>`: The sum of all order totals for completed orders within the date range.

**Throws:**
- `ArgumentException`: If `startDate` is after `endDate`.

## Usage

### Example 1: Creating and Retrieving an Order
