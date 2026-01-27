using OrderManagementService.Domain.Models.Orders;

namespace OrderManagementService.Domain.Exceptions;

public class InvalidOrderStateException : DomainException
{
    public long OrderId { get; }

    public OrderState State { get; }

    public string Operation { get; }

    public InvalidOrderStateException(long orderId, OrderState state, string operation)
        : base($"Order {orderId} is in state {state} and can't perform operation {operation}")
    {
        OrderId = orderId;
        State = state;
        Operation = operation;
    }
}