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
```