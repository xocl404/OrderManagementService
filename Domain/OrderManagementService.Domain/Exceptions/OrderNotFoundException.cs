namespace OrderManagementService.Domain.Exceptions;

public class OrderNotFoundException : DomainException
{
    public long OrderId { get; }

    public OrderNotFoundException(long orderId) : base($"Order {orderId} not found")
    {
        OrderId = orderId;
    }
}