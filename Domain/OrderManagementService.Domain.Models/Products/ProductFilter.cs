namespace OrderManagementService.Domain.Models.Products;

public sealed record ProductFilter(
    IReadOnlyCollection<long>? Ids = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? NameSubstring = null,
    int PageSize = 50,
    string? PageToken = null);