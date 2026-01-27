using FluentMigrator.Runner;
using OrderManagementService.Domain.Abstractions;
using OrderManagementService.Application;
using OrderManagementService.Application.Abstractions;
using OrderManagementService.Infrastructure.DataAccess.Options;
using OrderManagementService.Infrastructure.DataAccess.Repositories;
using OrderManagementService.Infrastructure.DataAccess.Serializers;
using OrderManagementService.Infrastructure.DataAccess.Transactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace OrderManagementService.Infrastructure.DataAccess.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
        services.AddSingleton(sp =>
        {
            DatabaseOptions options = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            return string.IsNullOrWhiteSpace(options.ConnectionString)
                ? throw new InvalidOperationException("Database connection string is not configured")
                : new NpgsqlDataSourceBuilder(options.ConnectionString).Build();
        });

        return services;
    }

    public static IServiceCollection AddMigrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb.AddPostgres()
                .WithGlobalConnectionString(configuration.GetValue<string>("Database:ConnectionString"))
                .ScanIn(typeof(ServiceCollectionExtensions).Assembly).For.Migrations());

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton(_ => HistoryJsonOptionsFactory.Create());
        services.AddSingleton<OrderHistoryPayloadSerializer>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();
        services.AddScoped<ITransactionFactory, TransactionFactory>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddMigrations(configuration);
        services.AddRepositories();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderProcessingService, OrderService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
