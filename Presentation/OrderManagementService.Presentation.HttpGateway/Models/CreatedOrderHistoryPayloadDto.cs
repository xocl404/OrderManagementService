namespace OrderManagementService.Presentation.HttpGateway.Models;

public sealed record CreatedOrderHistoryPayloadDto(
    string CreatedBy,
    DateTimeOffset CreatedAt) : OrderHistoryPayloadDto;