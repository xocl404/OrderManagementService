namespace OrderManagementService.Presentation.HttpGateway.Models.Requests;

public sealed record CreateProductRequestDto(string Name, decimal Price);