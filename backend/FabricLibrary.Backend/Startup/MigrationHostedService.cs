using FabricLibrary.Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FabricLibrary.Backend.Startup;

/// <summary>
/// Hosted service that applies EF Core migrations on application startup.
/// Runs when the environment is Development or when the APPLY_MIGRATIONS env var is set to "true".
/// This is suitable for development orchestration scenarios; for production consider a safer migration strategy.
/// </summary>
public class MigrationHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<MigrationHostedService> _logger;
    private readonly IHostEnvironment _env;

    public MigrationHostedService(IServiceProvider services, ILogger<MigrationHostedService> logger, IHostEnvironment env)
    {
        _services = services;
        _logger = logger;
        _env = env;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var applyFlag = Environment.GetEnvironmentVariable("APPLY_MIGRATIONS");
        if (!_env.IsDevelopment() && string.Equals(applyFlag, "true", StringComparison.OrdinalIgnoreCase) == false)
        {
            _logger.LogInformation("Skipping EF Core migrations on startup (not Development and APPLY_MIGRATIONS != true).");
            return;
        }

        try
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _logger.LogInformation("Applying pending EF Core migrations...");
            await db.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while applying database migrations.");
            // Depending on policy, decide whether to rethrow. For dev we rethrow so the host fails fast.
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
