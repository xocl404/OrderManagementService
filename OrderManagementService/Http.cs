using Grpc.Net.Client;
using OrderManagementService.Presentation.HttpGateway.Middleware;
using OrderManagementService.Presentation.HttpGateway.Models;
using OrderManagementService.Presentation.HttpGateway.Options;
using Microsoft.Extensions.Options;
using Orders;
using Orders.ProcessingService.Contracts;
using Products;

namespace OrderManagementService;

public static class Http
{
    public static void Main(string[] args)
    {
        WebApplication app = BuildApp(args);
        app.Run();
    }

    private static WebApplication BuildApp(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls("http://localhost:5001");
        ConfigureServices(builder);
        WebApplication app = builder.Build();
        ConfigurePipeline(app);

        return app;
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddLogging(lb => lb.AddConsole());

        builder.Services.AddOptions<GrpcClientOptions>().Configure<IConfiguration>((opts, config) =>
        {
            opts.Host = config.GetValue("Grpc:Host", "localhost");
            opts.Port = config.GetValue("Grpc:Port", 5000);
            opts.Scheme = config.GetValue("Grpc:Scheme", "http");
        });

        builder.Services.AddOptions<OrderProcessingGrpcClientOptions>().Configure<IConfiguration>((opts, config) =>
        {
            opts.Host = config.GetValue("ProcessingGrpc:Host", "localhost");
            opts.Port = config.GetValue("ProcessingGrpc:Port", 8084);
            opts.Scheme = config.GetValue("ProcessingGrpc:Scheme", "http");
        });

        builder.Services.AddSingleton(sp =>
        {
            GrpcClientOptions options = sp.GetRequiredService<IOptions<GrpcClientOptions>>().Value;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            string address = $"{options.Scheme}://{options.Host}:{options.Port}";
            var channel = GrpcChannel.ForAddress(address);
            return new OrdersService.OrdersServiceClient(channel);
        });

        builder.Services.AddSingleton(sp =>
        {
            GrpcClientOptions options = sp.GetRequiredService<IOptions<GrpcClientOptions>>().Value;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            string address = $"{options.Scheme}://{options.Host}:{options.Port}";
            var channel = GrpcChannel.ForAddress(address);
            return new ProductsService.ProductsServiceClient(channel);
        });

        builder.Services.AddSingleton(sp =>
        {
            OrderProcessingGrpcClientOptions options = sp.GetRequiredService<IOptions<OrderProcessingGrpcClientOptions>>().Value;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            string address = $"{options.Scheme}://{options.Host}:{options.Port}";
            var channel = GrpcChannel.ForAddress(address);
            return new OrderService.OrderServiceClient(channel);
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(o =>
        {
            o.UseOneOfForPolymorphism();
            o.SelectSubTypesUsing(baseType =>
                baseType == typeof(OrderHistoryPayloadDto)
                    ? new[]
                    {
                        typeof(CreatedOrderHistoryPayloadDto),
                        typeof(ItemAddedOrderHistoryPayloadDto),
                        typeof(ItemRemovedOrderHistoryPayloadDto),
                        typeof(StateChangedOrderHistoryPayloadDto),
                    }
                    : Enumerable.Empty<Type>());
        });
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseGrpcErrorHandling();
        app.MapControllers();
    }
}
