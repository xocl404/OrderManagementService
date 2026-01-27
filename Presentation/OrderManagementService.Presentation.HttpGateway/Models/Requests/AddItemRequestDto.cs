namespace OrderManagementService.Presentation.HttpGateway.Models.Requests;

public sealed record AddItemRequestDto(long ProductId, int Quantity);