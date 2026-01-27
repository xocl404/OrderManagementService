using OrderManagementService.Domain.Models.Products;

namespace OrderManagementService.Domain.Abstractions;

public interface IProductService
{
    Task<long> CreateAsync(string name, decimal price, CancellationToken cancellationToken);

    Task<PagedResult<Product>> QueryAsync(ProductFilter filter, CancellationToken cancellationToken);
}