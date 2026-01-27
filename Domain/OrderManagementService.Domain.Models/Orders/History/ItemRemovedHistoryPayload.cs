namespace OrderManagementService.Domain.Models.Orders.History;

public sealed record ItemRemovedHistoryPayload(long ProductId, int Quantity) : OrderHistoryPayload;
