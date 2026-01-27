using Confluent.Kafka;
using OrderManagementService.Infrastructure.Kafka.Contracts;

namespace OrderManagementService.Infrastructure.Kafka.Producers;

public sealed class KafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>, IDisposable
{
    private readonly IProducer<TKey, TValue> _producer;

    public KafkaProducer(IProducer<TKey, TValue> producer)
    {
        _producer = producer;
    }

    public async Task ProduceAsync(string topic, TKey key, TValue value, CancellationToken cancellationToken)
    {
        var message = new Message<TKey, TValue>
        {
            Key = key,
            Value = value,
        };

        await _producer.ProduceAsync(topic, message, cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _producer.Flush();
        _producer.Dispose();
    }
}