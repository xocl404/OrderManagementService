namespace OrderManagementService.Presentation.HttpGateway.Options;

public sealed class OrderProcessingGrpcClientOptions
{
    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 8084;

    public string Scheme { get; set; } = "http";
}