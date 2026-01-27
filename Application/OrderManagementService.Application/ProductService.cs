using OrderManagementService.Domain;
using OrderManagementService.Domain.Abstractions;
using OrderManagementService.Domain.Models.Products;

namespace OrderManagementService.Application;

public class ProductService : IProductService
{
    private readonly IProductRepository _products;

    public ProductService(IProductRepository products)
    {
        _products = products;
    }

    public Task<long> CreateAsync(string name, decimal price, CancellationToken cancellationToken)
    {
        return _products.AddAsync(name, price, cancellationToken);
    }

    public Task<PagedResult<Product>> QueryAsync(ProductFilter filter, CancellationToken cancellationToken)
    {
        return _products.QueryAsync(filter, cancellationToken);
    }
}