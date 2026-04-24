namespace OrderService.Domain;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Paid,
    Shipped,
    Cancelled,
    Failed,
}
