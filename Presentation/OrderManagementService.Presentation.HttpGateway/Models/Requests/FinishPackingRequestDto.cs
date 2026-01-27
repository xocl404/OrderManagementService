namespace OrderManagementService.Presentation.HttpGateway.Models.Requests;

public sealed record FinishPackingRequestDto(bool IsSuccessful, string? FailureReason);