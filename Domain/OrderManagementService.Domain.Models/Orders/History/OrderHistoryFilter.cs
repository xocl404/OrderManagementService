namespace OrderManagementService.Domain.Models.Orders.History;

public sealed record OrderHistoryFilter(
    IReadOnlyCollection<long>? OrderIds = null,
    OrderHistoryItemKind? Kind = null,
    int PageSize = 50,
    string? PageToken = null);