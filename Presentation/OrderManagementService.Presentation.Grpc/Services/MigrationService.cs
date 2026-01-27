using FluentMigrator.Runner;

namespace OrderManagementService.Presentation.Grpc.Services;

public sealed class MigrationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<MigrationService> _logger;

    private readonly IHostApplicationLifetime _lifetime;

    public MigrationService(IServiceScopeFactory scopeFactory, ILogger<MigrationService> logger, IHostApplicationLifetime lifetime)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _lifetime = lifetime;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Database migration failed");
            _lifetime.StopApplication();
        }

        return Task.CompletedTask;
    }
}