using OrderManagementService.Domain.Models.Orders.History;

namespace OrderManagementService.Domain.Abstractions;

public interface IOrderService
{
    Task<long> CreateAsync(string createdBy, CancellationToken cancellationToken);

    Task<bool> AddItemAsync(long orderId, long productId, int quantity, CancellationToken cancellationToken);

    Task<bool> RemoveItemAsync(long orderItemId, CancellationToken cancellationToken);

    Task<bool> StartProcessingAsync(long orderId, CancellationToken cancellationToken);

    Task<bool> CompleteAsync(long orderId, CancellationToken cancellationToken);

    Task<bool> CancelAsync(long orderId, CancellationToken cancellationToken);

    Task<PagedResult<OrderHistoryItem>> GetHistoryAsync(long orderId, OrderHistoryFilter filter, CancellationToken cancellationToken);
}