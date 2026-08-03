## StatusHistory

The StatusHistory type represents a change in the status of an order. It contains the following properties:

* Id: a unique identifier for the status history entry
* OrderId: the identifier of the order that the status change belongs to
* FromStatus and ToStatus: the previous and new status of the order
* ChangedAt: the date and time that the status change occurred
* ChangedBy: the user who made the status change (optional)
* Notes: any additional notes or comments about the status change (optional)
* Order: the order that the status change belongs to (optional)

Example usage:

```csharp
public class MyOrderService
{
    public void UpdateOrderStatus(int orderId, OrderStatus newStatus)
    {
        var order = _dbContext.Orders.Find(orderId);
        order.StatusHistory.Add(new StatusHistory
        {
            OrderId = orderId,
            FromStatus = order.Status,
            ToStatus = newStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = "MyOrderService"
        });
        _dbContext.SaveChanges();
    }
}

## UpdateProductPriceRequest

The `UpdateProductPriceRequest` DTO is used to initiate a bulk price update operation for multiple products. It encapsulates a list of `ProductPriceUpdate` objects, allowing clients to efficiently update several product prices in a single API request.

It contains the following properties:

* PriceUpdates: A list of `ProductPriceUpdate` objects containing the `ProductId` and `NewPrice` for each update.

Example usage:

```csharp
using AspNetSpaTemplate.DTOs;

var request = new UpdateProductPriceRequest
{
    PriceUpdates = new List<ProductPriceUpdate>
    {
        new ProductPriceUpdate { ProductId = 1, NewPrice = 19.99m },
        new ProductPriceUpdate { ProductId = 2, NewPrice = 29.99m }
    }
};
```