using Grpc.Core;
using OrderManagementService.Domain;
using OrderManagementService.Domain.Abstractions;
using OrderManagementService.Presentation.Grpc.Mappings;
using Products;
using Product = OrderManagementService.Domain.Models.Products.Product;
using ProductFilter = OrderManagementService.Domain.Models.Products.ProductFilter;

namespace OrderManagementService.Presentation.Grpc.Services;

public sealed class ProductService : ProductsService.ProductsServiceBase
{
    private readonly IProductService _productService;

    public ProductService(IProductService productService)
    {
        _productService = productService;
    }

    public override async Task<CreateProductResponse> CreateProduct(
        CreateProductRequest request,
        ServerCallContext context)
    {
        long id = await _productService.CreateAsync(request.Name, (decimal)request.Price, context.CancellationToken);
        return new CreateProductResponse { ProductId = id };
    }

    public override async Task<QueryProductsResponse> QueryProducts(
        QueryProductsRequest request,
        ServerCallContext context)
    {
        ProductFilter filter = request.Filter?.ToDomain() ?? new ProductFilter(PageSize: 50);
        PagedResult<Product> result = await _productService.QueryAsync(filter, context.CancellationToken);

        return new QueryProductsResponse { Result = result.ToProto() };
    }
}