namespace OrderManagementService.Domain.Models.Orders;

public sealed record OrderFilter(
    IReadOnlyCollection<long>? Ids = null,
    OrderState? State = null,
    string? CreatedBy = null,
    int PageSize = 50,
    string? PageToken = null);