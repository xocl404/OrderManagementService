namespace OrderManagementService.Presentation.HttpGateway.Models.Requests;

public sealed record ApproveOrderRequestDto(
    bool IsApproved,
    string ApprovedBy,
    string? FailureReason);