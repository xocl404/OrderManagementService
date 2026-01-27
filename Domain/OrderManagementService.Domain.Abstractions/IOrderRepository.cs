using OrderManagementService.Domain.Models.Orders;
using System.Data.Common;

namespace OrderManagementService.Domain.Abstractions;

public interface IOrderRepository
{
    Task<long> AddAsync(OrderState state, DateTimeOffset createdAt, string createdBy, CancellationToken cancellationToken, DbTransaction? transaction = null);

    Task<bool> UpdateStateAsync(long orderId, OrderState newState, CancellationToken cancellationToken, DbTransaction? transaction = null);

    Task<PagedResult<Order>> QueryAsync(OrderFilter filter, CancellationToken cancellationToken);

    Task<Order?> GetAsync(long orderId, CancellationToken cancellationToken, DbTransaction? transaction = null);
}