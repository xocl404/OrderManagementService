namespace OrderManagementService.Presentation.HttpGateway.Models;

public sealed record StateChangedOrderHistoryPayloadDto(
    string From,
    string To) : OrderHistoryPayloadDto;