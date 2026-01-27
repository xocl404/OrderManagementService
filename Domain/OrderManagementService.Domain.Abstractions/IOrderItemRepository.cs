using OrderManagementService.Domain.Models.Orders;
using System.Data.Common;

namespace OrderManagementService.Domain.Abstractions;

public interface IOrderItemRepository
{
    Task<long> AddAsync(long orderId, long productId, int quantity, CancellationToken cancellationToken, DbTransaction? transaction = null);

    Task<bool> SoftDeleteAsync(long orderItemId, CancellationToken cancellationToken, DbTransaction? transaction = null);

    Task<PagedResult<OrderItem>> QueryAsync(OrderItemFilter filter, CancellationToken cancellationToken);

    Task<OrderItem?> GetAsync(long orderItemId, CancellationToken cancellationToken, DbTransaction? transaction = null);
}