namespace OrderManagementService.Infrastructure.Kafka.Options;

public sealed class KafkaTopicsOptions
{
    public string OrderCreation { get; init; } = string.Empty;

    public string OrderProcessing { get; init; } = string.Empty;
}