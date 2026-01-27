namespace OrderManagementService.Domain.Models.Orders;

public enum OrderState
{
    Created,
    Processing,
    Completed,
    Cancelled,
}