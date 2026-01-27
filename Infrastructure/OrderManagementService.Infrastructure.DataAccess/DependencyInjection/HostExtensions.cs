using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OrderManagementService.Infrastructure.DataAccess.DependencyInjection;

public static class HostExtensions
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using IServiceScope scope = host.Services.CreateScope();
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();

        return host;
    }
}