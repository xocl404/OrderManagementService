namespace OrderManagementService.Domain.Models.Orders;

public sealed record Order(long Id, OrderState State, DateTimeOffset CreatedAt, string CreatedBy);