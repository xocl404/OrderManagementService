namespace OrderManagementService.Domain.Models.Orders;

public sealed record OrderItemFilter(
    IReadOnlyCollection<long>? OrderIds = null,
    IReadOnlyCollection<long>? ProductIds = null,
    bool? IsDeleted = null,
    int PageSize = 50,
    string? PageToken = null);