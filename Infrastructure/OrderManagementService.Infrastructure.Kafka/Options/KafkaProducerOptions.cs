namespace OrderManagementService.Infrastructure.Kafka.Options;

public sealed class KafkaProducerOptions
{
    public string BootstrapServers { get; init; } = string.Empty;
}