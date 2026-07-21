# OrderStatsResponse

Represents aggregated statistical data about orders within a specified context, such as a date range or filter criteria. This type is used to return summarized order metrics, including counts by status, revenue totals, and order volumes, enabling reporting and analytics for order management.

## API

### `StatusCounts`
- **Purpose**: Provides a breakdown of order counts grouped by their current status.
- **Type**: `Dictionary<string, int>`
- **Behavior**: Keys represent order statuses (e.g., "Completed", "Pending"), and values represent the number of orders in each status. The dictionary is populated based on the query criteria used to generate the response.

### `TotalRevenue`
- **Purpose**: Represents the cumulative revenue from all orders included in the response.
- **Type**: `decimal`
- **Behavior**: Sums the revenue of all orders, regardless of status. Does not account for refunds or cancellations unless explicitly included in the query logic.

### `DateRangeRevenue`
- **Purpose**: Represents the cumulative revenue from orders within a specified date range, if applicable.
- **Type**: `decimal?`
- **Behavior**: Null if no date range was specified in the query. Otherwise, sums the revenue of orders falling within the provided date bounds.

### `TotalOrders`
- **Purpose**: Indicates the total number of orders included in the response.
- **Type**: `int`
- **Behavior**: Counts all orders matching the query criteria, regardless of status.

### `CancelledOrders`
- **Purpose**: Indicates the number of orders marked as cancelled.
- **Type**: `int`
- **Behavior**: Counts orders with a "Cancelled" status. May overlap with `StatusCounts` if the status is explicitly tracked there.

### `RefundedOrders`
- **Purpose**: Indicates the number of orders that have been refunded.
- **Type**: `int`
- **Behavior**: Counts orders with a "Refunded" status or equivalent. May overlap with `StatusCounts` if the status is explicitly tracked there.

## Usage

### Example 1: Retrieving Order Statistics for a Date Range
