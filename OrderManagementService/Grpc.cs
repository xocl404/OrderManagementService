using OrderManagementService.Infrastructure.DataAccess.DependencyInjection;
using OrderManagementService.Infrastructure.Kafka.DependencyInjection;
using OrderManagementService.Infrastructure.Kafka.Handlers;
using OrderManagementService.Presentation.Grpc.DependencyInjection;
using OrderManagementService.Presentation.Grpc.Interceptors;
using OrderManagementService.Presentation.Grpc.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace OrderManagementService;

public static class Grpc
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = CreateBuilder(args);
        ConfigureServices(builder);
        WebApplication app = BuildApp(builder);
        await app.RunAsync();
    }

    private static WebApplicationBuilder CreateBuilder(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http2);
        });

        return builder;
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddLogging(lb => lb.AddConsole());

        builder.Services.AddHostedService<MigrationService>();
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddKafkaProducer<byte[], byte[]>(builder.Configuration);
        builder.Services.AddKafkaConsumer<byte[], byte[], OrderProcessingMessageHandler>(builder.Configuration);

        builder.Services.AddPresentation();
        builder.Services.AddGrpc(options => options.Interceptors.Add<ErrorInterceptor>());
    }

    private static WebApplication BuildApp(WebApplicationBuilder builder)
    {
        WebApplication app = builder.Build();
        app.MapGrpcService<OrderService>();
        app.MapGrpcService<ProductService>();
        return app;
    }
}