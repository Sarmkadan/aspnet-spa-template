# CreateOrderRequest

Represents the data contract for creating a new order in the system. This type encapsulates all required and optional fields necessary to define an order entity, including order items, shipping/billing addresses, and optional notes. It is typically used as the body of an HTTP POST request to an order creation endpoint.

## API

### `public List<OrderItemRequest> Items`

Gets or sets the collection of items to include in the order. This value is required and must contain at least one item. Each item specifies a product ID and quantity.

- **Parameters**: None (property setter accepts a `List<OrderItemRequest>` value).
- **Returns**: The current items assigned to the request (property getter returns `List<OrderItemRequest>`).
- **Throws**: No exceptions are thrown by the property itself. Validation failures (e.g., empty list or invalid items) are typically deferred to model binding or explicit validation logic in the consuming layer.

### `public string? ShippingAddress`

Gets or sets the shipping address for the order. This value may be `null` if no shipping address is provided. When supplied, it should contain the complete delivery address.

- **Parameters**: None (property setter accepts a `string` or `null` value).
- **Returns**: The current shipping address assigned to the request, or `null` if none is set (property getter returns `string?`).
- **Throws**: No exceptions are thrown by the property itself. Address format validation, if any, is performed by the consuming service.

### `public string? BillingAddress`

Gets or sets the billing address for the order. This value may be `null` if no billing address is provided. When supplied, it should contain the complete billing address.

- **Parameters**: None (property setter accepts a `string` or `null` value).
- **Returns**: The current billing address assigned to the request, or `null` if none is set (property getter returns `string?`).
- **Throws**: No exceptions are thrown by the property itself. Address format validation, if any, is performed by the consuming service.

### `public string? Notes`

Gets or sets optional notes or special instructions for the order. This value may be `null` if no notes are provided.

- **Parameters**: None (property setter accepts a `string` or `null` value).
- **Returns**: The current notes assigned to the request, or `null` if none is set (property getter returns `string?`).
- **Throws**: No exceptions are thrown by the property itself.

## OrderItemRequest

### `public int ProductId`

Gets or sets the product identifier for the order item. This value is required and must be a valid product ID that exists in the product catalog.

- **Parameters**: None (property setter accepts an `int` value).
- **Returns**: The current product ID assigned to the request item (property getter returns `int`).
- **Throws**: No exceptions are thrown by the property itself. Invalid product IDs are typically rejected during validation.

### `public int Quantity`

Gets or sets the quantity of the product to order. This value is required and must be a positive integer greater than zero.

- **Parameters**: None (property setter accepts an `int` value).
- **Returns**: The current quantity assigned to the request item (property getter returns `int`).
- **Throws**: No exceptions are thrown by the property itself. Non-positive values are rejected during validation.

## Usage

### Example 1: Creating a Basic Order with Required Fields

```csharp
var request = new CreateOrderRequest
{
    Items = new List<OrderItemRequest>
    {
        new OrderItemRequest { ProductId = 1, Quantity = 2 },
        new OrderItemRequest { ProductId = 2, Quantity = 1 }
    },
    ShippingAddress = "123 Main St, Anytown, USA 12345",
    BillingAddress = "123 Main St, Anytown, USA 12345"
};

// Typically sent to an API endpoint:
// var response = await orderService.CreateAsync(request);
```

### Example 2: Creating an Order with All Fields

```csharp
var request = new CreateOrderRequest
{
    Items = new List<OrderItemRequest>
    {
        new OrderItemRequest { ProductId = 10, Quantity = 3 },
        new OrderItemRequest { ProductId = 25, Quantity = 1 }
    },
    ShippingAddress = "456 Oak Avenue, Somewhere, USA 67890",
    BillingAddress = "456 Oak Avenue, Somewhere, USA 67890",
    Notes = "Please leave package at back door if no one is home."
};

// The Notes field is optional; omitting it is valid.
// var response = await orderService.CreateAsync(request);
```

## Notes

- **Validation**: None of the properties enforce validation at the point of assignment. All validation (required fields, range checks, item validity, address format) is expected to be performed by a separate validation layer, such as data annotations, FluentValidation, or explicit checks in the service handling the request.
- **Default Values**: The `Items` property initializes to an empty list but must contain at least one item for a valid order. Reference-type properties (`ShippingAddress`, `BillingAddress`, `Notes`) default to `null`.
- **Thread Safety**: This type is a plain data transfer object with public get/set properties. It provides no internal synchronization. Instances are not thread-safe when mutated concurrently across multiple threads. In typical usage, instances are constructed, populated, and passed to a service on a single thread, making concurrent mutation unlikely.
- **Immutability**: The type is fully mutable by design to facilitate model binding and straightforward object initialization. If immutability is desired, consumers should treat the instance as read-only after construction or map it to an immutable domain object.
- **Serialization**: As a simple POCO, this type serializes cleanly to JSON or XML using default serializers. Properties with `null` values (`ShippingAddress`, `BillingAddress`, `Notes`) are typically omitted or serialized as `null` depending on serializer configuration.