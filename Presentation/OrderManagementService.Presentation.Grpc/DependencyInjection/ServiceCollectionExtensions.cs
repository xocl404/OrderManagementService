using OrderManagementService.Presentation.Grpc.Interceptors;
using OrderManagementService.Presentation.Grpc.Options;

namespace OrderManagementService.Presentation.Grpc.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            IConfiguration config = sp.GetRequiredService<IConfiguration>();
            return new GrpcOptions
            {
                Port = config.GetValue("Grpc:Port", 5000),
            };
        });
        services.AddSingleton<ErrorInterceptor>();
        return services;
    }
}