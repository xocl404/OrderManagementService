namespace OrderManagementService.Presentation.HttpGateway.Models;

public sealed record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    string? NextPageToken);