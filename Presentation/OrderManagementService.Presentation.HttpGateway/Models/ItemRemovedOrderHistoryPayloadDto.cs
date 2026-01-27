namespace OrderManagementService.Presentation.HttpGateway.Models;

public sealed record ItemRemovedOrderHistoryPayloadDto(long ProductId, int Quantity) : OrderHistoryPayloadDto;