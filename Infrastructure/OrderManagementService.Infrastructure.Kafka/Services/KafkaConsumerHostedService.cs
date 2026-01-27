using Confluent.Kafka;
using OrderManagementService.Infrastructure.Kafka.Contracts;
using OrderManagementService.Infrastructure.Kafka.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrderManagementService.Infrastructure.Kafka.Services;

public sealed class KafkaConsumerHostedService<TKey, TValue> : BackgroundService
{
    private readonly KafkaConsumerOptions _options;
    private readonly IKafkaMessageHandler<TKey, TValue> _handler;
    private readonly ILogger<KafkaConsumerHostedService<TKey, TValue>> _logger;
    private readonly IDeserializer<TKey> _keyDeserializer;
    private readonly IDeserializer<TValue> _valueDeserializer;

    public KafkaConsumerHostedService(
        IOptions<KafkaConsumerOptions> options,
        IKafkaMessageHandler<TKey, TValue> handler,
        ILogger<KafkaConsumerHostedService<TKey, TValue>> logger,
        IDeserializer<TKey> keyDeserializer,
        IDeserializer<TValue> valueDeserializer)
    {
        _options = options.Value;
        _handler = handler;
        _logger = logger;
        _keyDeserializer = keyDeserializer;
        _valueDeserializer = valueDeserializer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using IConsumer<TKey, TValue> consumer = new ConsumerBuilder<TKey, TValue>(config)
            .SetKeyDeserializer(_keyDeserializer)
            .SetValueDeserializer(_valueDeserializer)
            .Build();

        consumer.Subscribe(_options.Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var batch = new List<ConsumeResult<TKey, TValue>>(_options.BatchSize);
                for (int i = 0; i < _options.BatchSize; i++)
                {
                    ConsumeResult<TKey, TValue>? result =
                        consumer.Consume(TimeSpan.FromMilliseconds(_options.PollIntervalMs));
                    if (result is null) break;
                    batch.Add(result);
                }

                if (batch.Count > 0)
                {
                    await _handler.HandleAsync(batch, stoppingToken);
                    consumer.Commit(batch[^1]);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka consume failed for topic {Topic}", _options.Topic);
            }
        }

        consumer.Close();
    }
}