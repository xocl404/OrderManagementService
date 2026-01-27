namespace OrderManagementService.Domain.Models.Orders.History;

public sealed record CreatedOrderHistoryPayload(string CreatedBy, DateTimeOffset CreatedAt) : OrderHistoryPayload;