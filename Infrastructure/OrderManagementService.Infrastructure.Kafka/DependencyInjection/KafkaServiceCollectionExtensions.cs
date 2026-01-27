using Confluent.Kafka;
using OrderManagementService.Infrastructure.Kafka.Contracts;
using OrderManagementService.Infrastructure.Kafka.Options;
using OrderManagementService.Infrastructure.Kafka.Producers;
using OrderManagementService.Infrastructure.Kafka.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrderManagementService.Infrastructure.Kafka.DependencyInjection;

public static class KafkaServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaProducer<TKey, TValue>(this IServiceCollection services, IConfiguration configuration, ISerializer<TKey>? keySerializer = null, ISerializer<TValue>? valueSerializer = null)
    {
        services.Configure<KafkaProducerOptions>(configuration.GetSection("Kafka"));
        services.Configure<KafkaTopicsOptions>(configuration.GetSection("Kafka:Topics"));

        services.AddSingleton(sp =>
        {
            KafkaProducerOptions opts = sp.GetRequiredService<IOptions<KafkaProducerOptions>>().Value;
            var config = new ProducerConfig
            {
                BootstrapServers = opts.BootstrapServers,
            };

            return new ProducerBuilder<TKey, TValue>(config)
                .SetKeySerializer(keySerializer ?? throw new InvalidOperationException("Key serializer is not provided"))
                .SetValueSerializer(valueSerializer ?? throw new InvalidOperationException("Value serializer is not provided"))
                .Build();
        });

        services.AddSingleton<IKafkaProducer<TKey, TValue>, KafkaProducer<TKey, TValue>>();
        return services;
    }

    public static IServiceCollection AddKafkaConsumer<TKey, TValue, THandler>(
        this IServiceCollection services,
        IConfiguration configuration,
        IDeserializer<TKey>? keyDeserializer = null,
        IDeserializer<TValue>? valueDeserializer = null)
        where THandler : class, IKafkaMessageHandler<TKey, TValue>
    {
        services.Configure<KafkaConsumerOptions>(configuration.GetSection("Kafka:Consumer"));

        services.AddSingleton(keyDeserializer ?? throw new InvalidOperationException("Key deserializer is not provided"));
        services.AddSingleton(valueDeserializer ?? throw new InvalidOperationException("Value deserializer is not provided"));

        services.AddSingleton<IKafkaMessageHandler<TKey, TValue>, THandler>();
        services.AddHostedService<KafkaConsumerHostedService<TKey, TValue>>();

        return services;
    }
}
