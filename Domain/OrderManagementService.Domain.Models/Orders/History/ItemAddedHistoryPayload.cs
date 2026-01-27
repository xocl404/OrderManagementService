namespace OrderManagementService.Domain.Models.Orders.History;

public sealed record ItemAddedHistoryPayload(long ProductId, int Quantity) : OrderHistoryPayload;