namespace OrderManagementService.Domain.Models.Orders.History;

public sealed record OrderHistoryItem(
    long Id,
    long OrderId,
    DateTimeOffset CreatedAt,
    OrderHistoryItemKind Kind,
    OrderHistoryPayload Payload);