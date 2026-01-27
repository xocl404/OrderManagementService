namespace OrderManagementService.Presentation.HttpGateway.Options;

public sealed class GrpcClientOptions
{
    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 5000;

    public string Scheme { get; set; } = "http";
}