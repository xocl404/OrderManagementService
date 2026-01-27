using OrderManagementService.Domain.Models.Products;
using System.Data.Common;

namespace OrderManagementService.Domain.Abstractions;

public interface IProductRepository
{
    Task<long> AddAsync(string name, decimal price, CancellationToken cancellationToken, DbTransaction? transaction = null);

    Task<PagedResult<Product>> QueryAsync(ProductFilter filter, CancellationToken cancellationToken);
}