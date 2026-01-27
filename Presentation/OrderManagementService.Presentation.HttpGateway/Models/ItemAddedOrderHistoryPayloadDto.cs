namespace OrderManagementService.Presentation.HttpGateway.Models;

public sealed record ItemAddedOrderHistoryPayloadDto(
    long ProductId,
    int Quantity) : OrderHistoryPayloadDto;