namespace OrderManagementService.Presentation.HttpGateway.Models;

public sealed record OrderHistoryItemDto(
    long Id,
    long OrderId,
    DateTimeOffset CreatedAt,
    OrderHistoryItemKindDto Kind,
    OrderHistoryPayloadDto Payload);