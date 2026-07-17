# OrderRepository

Provides data access methods for `Order` entities, including retrieval by various criteria, aggregate calculations, and standard CRUD operations. Inherits from a base repository class and uses `AppDbContext` for database operations.

## API

### `OrderRepository(AppDbContext context) : base(context)`

Constructor that initializes the repository with the provided database context.

**Parameters**
- `context`: The `AppDbContext` instance used for database operations.

---

### `public virtual async Task<Order?> GetByOrderNumberAsync(string orderNumber)`

Retrieves an `Order` by its unique order number.

**Parameters**
- `orderNumber`: The order number to search for.

**Returns**
- An `Order` instance if found; otherwise `null`.

**Exceptions**
- Throws `ArgumentException` if `orderNumber` is null or whitespace.

---

### `public virtual async Task<IEnumerable<Order>> GetByUserIdAsync(string userId)`

Retrieves all orders associated with a specific user.

**Parameters**
- `userId`: The user identifier to filter orders.

**Returns**
- An enumerable collection of `Order` instances.

**Exceptions**
- Throws `ArgumentException` if `userId` is null or whitespace.

---

### `public virtual async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)`

Retrieves all orders matching a specific status.

**Parameters**
- `status`: The `OrderStatus` to filter orders.

**Returns**
- An enumerable collection of `Order` instances.

---

### `public virtual async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)`

Retrieves all orders for a specific user, including historical and pending orders.

**Parameters**
- `userId`: The user identifier to filter orders.

**Returns**
- An enumerable collection of `Order` instances.

**Exceptions**
- Throws `ArgumentException` if `userId` is null or whitespace.

---
### `public virtual async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count)`

Retrieves the most recent orders up to the specified count, ordered by creation date descending.

**Parameters**
- `count`: The maximum number of orders to return.

**Returns**
- An enumerable collection of `Order` instances.

**Exceptions**
- Throws `ArgumentException` if `count` is less than or equal to zero.

---
### `public virtual async Task<IEnumerable<Order>> GetPendingOrdersAsync()`

Retrieves all orders with a status of `Pending`.

**Returns**
- An enumerable collection of `Order` instances with `OrderStatus.Pending`.

---
### `public virtual async Task<decimal> GetTotalRevenueAsync()`

Calculates the total revenue from all completed orders.

**Returns**
- The sum of order totals for all completed orders.

---
### `public virtual async Task<decimal> GetTotalRevenueAsync(OrderStatus status)`

Calculates the total revenue from orders matching the specified status.

**Parameters**
- `status`: The `OrderStatus` to filter orders.

**Returns**
- The sum of order totals for orders with the specified status.

---
### `public virtual async Task<int> GetOrderCountAsync()`

Retrieves the total number of orders in the system.

**Returns**
- The count of all `Order` records.

---
### `public virtual async Task<decimal> GetAverageOrderValueAsync()`

Calculates the average order value across all completed orders.

**Returns**
- The average order total as a `decimal`.

---
### `public override async Task<Order?> GetByIdAsync(int id)`

Retrieves an `Order` by its primary key identifier.

**Parameters**
- `id`: The primary key of the order.

**Returns**
- An `Order` instance if found; otherwise `null`.

**Exceptions**
- Throws `ArgumentException` if `id` is less than or equal to zero.

## Usage

### Retrieving a user's recent orders
