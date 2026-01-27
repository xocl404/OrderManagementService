namespace OrderManagementService.Presentation.HttpGateway.Models.Requests;

public sealed record ProductQueryRequestDto(
    long[]? Ids,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? NameSubstring,
    int PageSize = 50,
    string? PageToken = null);