using CloudScribe.Notes.API.Configuration;
using CloudScribe.Notes.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CloudScribe.Notes.API.HostedServices;

/// <summary>
/// Background service that applies pending EF Core migrations to the database.
/// This runs independently and can be configured to run on a separate schedule or instance.
/// </summary>
public class DatabaseMigrationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrationHostedService> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly MigrationJobSettings _settings;

    public DatabaseMigrationHostedService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseMigrationHostedService> logger,
        IHostApplicationLifetime applicationLifetime,
        IOptions<MigrationJobSettings> settings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _settings = settings.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Database Migration Job is disabled via configuration");
            return;
        }

        _logger.LogInformation("Database Migration Job started");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CloudScribeDbContext>();

            _logger.LogInformation("Checking for pending migrations...");

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingMigrationsList = pendingMigrations.ToList();

            if (pendingMigrationsList.Count != 0)
            {
                _logger.LogInformation("Found {Count} pending migration(s): {Migrations}",
                    pendingMigrationsList.Count,
                    string.Join(", ", pendingMigrationsList));

                _logger.LogInformation("Applying pending migrations...");
                await dbContext.Database.MigrateAsync(cancellationToken);

                _logger.LogInformation("Database migrations applied successfully");
            }
            else
            {
                _logger.LogInformation("No pending migrations found. Database is up to date");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while applying database migrations");

            if (_settings.StopApplicationOnFailure)
            {
                _logger.LogCritical("Stopping application due to migration failure (configured behavior)");
                _applicationLifetime.StopApplication();
            }

            throw;
        }

        _logger.LogInformation("Database Migration Job completed");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Database Migration Job is stopping");
        return Task.CompletedTask;
    }
}
