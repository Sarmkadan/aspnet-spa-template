# AppConstants

The `AppConstants` class centralizes application-wide API values and limits used for validation, pagination, products, orders, and caching. Its constants provide consistent defaults and boundaries across the application.

## API

### ApiVersion
The current API version segment. Its value is `"v1"`.

### ApiBaseRoute
The base route for versioned API endpoints. It combines `/api/` with `ApiVersion`, producing `/api/v1`.

## Validation

The `Validation` nested static class defines general input-length constraints.

### MinNameLength
The minimum allowed name length, set to `2` characters.

### MaxNameLength
The maximum allowed name length, set to `100` characters.

### MaxEmailLength
The maximum allowed email address length, set to `255` characters.

### MinPasswordLength
The minimum allowed password length, set to `8` characters.

### MaxPasswordLength
The maximum allowed password length, set to `128` characters.

### MaxDescriptionLength
The maximum allowed description length, set to `5000` characters.

### MaxSearchQueryLength
The maximum allowed search query length, set to `100` characters.

### MaxSearchTermLength
The maximum allowed individual search term length, set to `50` characters.

## Pagination

The `Pagination` nested static class defines page-size defaults and boundaries.

### DefaultPageSize
The default number of items returned per page, set to `10`.

### MaxPageSize
The maximum number of items allowed per page, set to `100`.

### MinPageSize
The minimum number of items allowed per page, set to `1`.

## Product

The `Product` nested static class defines product pricing, stock, and update constraints.

### MinPrice
The minimum product price, set to `0.01`.

### MaxPrice
The maximum product price, set to `999999.99`.

### MinStock
The minimum product stock quantity, set to `0`.

### MaxStock
The maximum product stock quantity, set to `1000000`.

### MaxProductUpdateNameLength
The maximum product-name length accepted by product update operations, set to `200` characters.

### MaxProductUpdatePrice
The maximum price accepted by product update operations, set to `1000000`.

## Order

The `Order` nested static class defines order-item quantity and value constraints.

### MinItemQuantity
The minimum quantity allowed for an order item, set to `1`.

### MaxItemQuantity
The maximum quantity allowed for an order item, set to `10000`.

### MinOrderValue
The minimum allowed order value, set to `0.01`.

## Cache

The `Cache` nested static class defines standard cache durations in minutes.

### DefaultDurationMinutes
The default cache duration, set to `30` minutes.

### ShortDurationMinutes
The short cache duration, set to `5` minutes.

### LongDurationMinutes
The long cache duration, set to `120` minutes.
