namespace OrderManagementService.Domain.Models.Orders;

public sealed record OrderItem(long Id, long OrderId, long ProductId, int Quantity, bool IsDeleted);