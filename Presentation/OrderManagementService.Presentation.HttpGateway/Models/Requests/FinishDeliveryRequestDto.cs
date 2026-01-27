namespace OrderManagementService.Presentation.HttpGateway.Models.Requests;

public sealed record FinishDeliveryRequestDto(bool IsSuccessful, string? FailureReason);