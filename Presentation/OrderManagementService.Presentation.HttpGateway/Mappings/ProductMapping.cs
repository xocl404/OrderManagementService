using OrderManagementService.Presentation.HttpGateway.Models;
using OrderManagementService.Presentation.HttpGateway.Models.Requests;
using Products;

namespace OrderManagementService.Presentation.HttpGateway.Mappings;

public static class ProductMapping
{
    public static ProductFilter ToProto(this ProductQueryRequestDto dto)
    {
        var filter = new ProductFilter
        {
            PageSize = dto.PageSize,
            PageToken = dto.PageToken ?? string.Empty,
            MinPrice = dto.MinPrice.HasValue ? (double)dto.MinPrice.Value : 0,
            MaxPrice = dto.MaxPrice.HasValue ? (double)dto.MaxPrice.Value : 0,
            NameSubstring = dto.NameSubstring ?? string.Empty,
        };

        if (dto.Ids is { Length: > 0 })
        {
            filter.Ids.AddRange(dto.Ids);
        }

        return filter;
    }

    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto(product.Id, product.Name, (decimal)product.Price);
    }
}