using Confluent.Kafka;

namespace OrderManagementService.Infrastructure.Kafka.Contracts;

public interface IKafkaMessageHandler<TKey, TValue>
{
    Task HandleAsync(IReadOnlyList<ConsumeResult<TKey, TValue>> messages, CancellationToken cancellationToken);
}