namespace OrderManagementService.Infrastructure.Kafka.Options;

public sealed class KafkaConsumerOptions
{
    public string BootstrapServers { get; init; } = string.Empty;

    public string GroupId { get; init; } = string.Empty;

    public string Topic { get; init; } = string.Empty;

    public int BatchSize { get; init; } = 10;

    public int PollIntervalMs { get; init; } = 500;
}