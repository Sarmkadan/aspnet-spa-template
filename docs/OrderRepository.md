# OrderRepository

Provides data access methods for `Order` entities, including retrieval by order number, user, status, and date; aggregate calculations; status counts; and retrieval by ID. It inherits from `RepositoryBase<Order>` and uses `AppDbContext` for database operations.

Queries that return orders include their `Items` collection.

## API

### `public OrderRepository(AppDbContext context)`

Initializes the repository with the provided database context.

**Parameters**

- `context`: The `AppDbContext` instance used for database operations.

---

### `public virtual async Task<Order?> GetByOrderNumberAsync(string orderNumber)`

Retrieves the first order whose order number matches `orderNumber`, including its items.

**Parameters**

- `orderNumber`: The order number to search for.

**Returns**

- The matching `Order`, or `null` if no order is found.

**Exceptions**

- Throws `ArgumentException` if `orderNumber` is empty or consists only of whitespace.
- Throws `ArgumentNullException` if `orderNumber` is `null`.

---

### `public virtual async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)`

Retrieves all orders for a user, including their items, ordered by `OrderedAt` descending.

**Parameters**

- `userId`: The user ID to match.

**Returns**

- The matching orders, ordered from newest to oldest.

---

### `public virtual async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)`

Retrieves all orders with the specified status, including their items, ordered by `OrderedAt` descending.

**Parameters**

- `status`: The order status to match.

**Returns**

- The matching orders, ordered from newest to oldest.

---

### `public virtual async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId, int pageNumber, int pageSize)`

Retrieves one page of orders for a user, including their items and ordered by `OrderedAt` descending. The query skips `(pageNumber - 1) * pageSize` records and takes `pageSize` records.

**Parameters**

- `userId`: The user ID to match.
- `pageNumber`: The one-based page number used to calculate the number of records to skip.
- `pageSize`: The maximum number of records to return.

**Returns**

- The requested page of the user's orders, ordered from newest to oldest.

---

### `public virtual async Task<IEnumerable<Order>> GetRecentOrdersAsync(int days = 30)`

Retrieves orders placed on or after `DateTime.UtcNow.AddDays(-days)`, including their items and ordered by `OrderedAt` descending.

**Parameters**

- `days`: The number of days used to calculate the cutoff date. Defaults to `30`.

**Returns**

- Orders on or after the calculated cutoff date, ordered from newest to oldest.

---

### `public virtual async Task<IEnumerable<Order>> GetPendingOrdersAsync()`

Retrieves orders whose status is `OrderStatus.Pending` or `OrderStatus.Confirmed`, including their items and ordered by `OrderedAt` descending.

**Returns**

- Pending and confirmed orders, ordered from newest to oldest.

---

### `public virtual async Task<decimal> GetTotalRevenueAsync()`

Calculates the sum of `Total` for all orders except those with `OrderStatus.Cancelled` or `OrderStatus.Refunded`.

**Returns**

- The sum of qualifying order totals.

---

### `public virtual async Task<decimal> GetTotalRevenueAsync(int days)`

Calculates the sum of `Total` for orders placed on or after `DateTime.UtcNow.AddDays(-days)`, excluding orders with `OrderStatus.Cancelled` or `OrderStatus.Refunded`.

**Parameters**

- `days`: The number of days used to calculate the cutoff date.

**Returns**

- The sum of qualifying order totals on or after the calculated cutoff date.

---

### `public virtual async Task<int> GetOrderCountAsync(int userId)`

Counts orders associated with the specified user.

**Parameters**

- `userId`: The user ID to match.

**Returns**

- The number of matching orders.

---

### `public virtual async Task<decimal> GetAverageOrderValueAsync()`

Calculates the average `Total` of all orders except those with `OrderStatus.Cancelled` or `OrderStatus.Refunded`.

**Returns**

- The average total of qualifying orders.

---

### `public virtual async Task<Dictionary<string, int>> GetStatusCountsAsync()`

Groups non-cancelled and non-refunded orders by status and returns their counts keyed by each status's display name. Every `OrderStatus` display name is included in the result; statuses absent from the query receive a count of zero.

**Returns**

- A dictionary mapping every order status display name to its count. Cancelled and refunded statuses have zero counts because they are excluded from the grouped query.

---

### `public virtual async Task<int> GetTotalOrdersCountAsync()`

Counts all orders.

**Returns**

- The total number of orders.

---

### `public virtual async Task<int> GetCancelledOrdersCountAsync()`

Counts orders with `OrderStatus.Cancelled`.

**Returns**

- The number of cancelled orders.

---

### `public virtual async Task<int> GetRefundedOrdersCountAsync()`

Counts orders with `OrderStatus.Refunded`.

**Returns**

- The number of refunded orders.

---

### `public override async Task<Order?> GetByIdAsync(int id)`

Retrieves the first order whose `Id` matches `id`, including its items.

**Parameters**

- `id`: The order ID to match.

**Returns**

- The matching `Order`, or `null` if no order is found.
