namespace OrderManagementService.Presentation.HttpGateway.Models;

public sealed record OrderDto(
    long Id,
    OrderStateDto State,
    DateTimeOffset CreatedAt,
    string CreatedBy);