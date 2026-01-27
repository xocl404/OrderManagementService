namespace OrderManagementService.Infrastructure.Kafka.Contracts;

public interface IKafkaProducer<TKey, TValue>
{
    Task ProduceAsync(string topic, TKey key, TValue value, CancellationToken cancellationToken);
}