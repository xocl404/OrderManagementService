using OrderManagementService.Domain;
using OrderManagementService.Domain.Models.Products;
using ProtoPagedResultProduct = Products.PagedResultProduct;
using ProtoProduct = Products.Product;
using ProtoProductFilter = Products.ProductFilter;

namespace OrderManagementService.Presentation.Grpc.Mappings;

public static class ProductMapping
{
    public static ProtoProduct ToProto(this Product product)
    {
        return new ProtoProduct
        {
            Id = product.Id,
            Name = product.Name,
            Price = (double)product.Price,
        };
    }

    public static ProductFilter ToDomain(this ProtoProductFilter filter)
    {
        return new ProductFilter(
            Ids: filter.Ids.Count > 0 ? filter.Ids.ToArray() : null,
            MinPrice: filter.MinPrice == 0 ? null : (decimal?)filter.MinPrice,
            MaxPrice: filter.MaxPrice == 0 ? null : (decimal?)filter.MaxPrice,
            NameSubstring: string.IsNullOrWhiteSpace(filter.NameSubstring) ? null : filter.NameSubstring,
            PageSize: filter.PageSize,
            PageToken: string.IsNullOrWhiteSpace(filter.PageToken) ? null : filter.PageToken);
    }

    public static ProtoPagedResultProduct ToProto(this PagedResult<Product> result)
    {
        var proto = new ProtoPagedResultProduct
        {
            NextPageToken = result.NextPageToken ?? string.Empty,
        };

        foreach (Product product in result.Items)
        {
            proto.Items.Add(product.ToProto());
        }

        return proto;
    }
}