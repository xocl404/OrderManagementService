using OrderManagementService.Domain.Models.Orders.History;
using System.Data.Common;

namespace OrderManagementService.Domain.Abstractions;

public interface IOrderHistoryRepository
{
    Task<long> AddAsync(
        long orderId,
        OrderHistoryItemKind kind,
        OrderHistoryPayload payload,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken,
        DbTransaction? transaction = null);

    Task<PagedResult<OrderHistoryItem>> QueryAsync(OrderHistoryFilter filter, CancellationToken cancellationToken);
}