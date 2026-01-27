namespace OrderManagementService.Domain.Models.Orders.History;

public sealed record StateChangedHistoryPayload(string From, string To) : OrderHistoryPayload;