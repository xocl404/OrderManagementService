namespace OrderManagementService.Domain;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, string? NextPageToken);