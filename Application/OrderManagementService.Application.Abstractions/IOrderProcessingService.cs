namespace OrderManagementService.Application.Abstractions;

public interface IOrderProcessingService
{
    Task CancelFromProcessorAsync(long orderId, CancellationToken cancellationToken);

    Task CompleteFromProcessorAsync(long orderId, CancellationToken cancellationToken);

    Task AddHistoryEventAsync(long orderId, string fromState, string toState, CancellationToken cancellationToken);
}