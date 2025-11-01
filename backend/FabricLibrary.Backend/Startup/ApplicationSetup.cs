using FabricLibrary.Backend.Data;
using Microsoft.AspNetCore.Builder;

namespace FabricLibrary.Backend.Startup;

/// <summary>
/// Application setup extension methods for configuring services.
/// </summary>
public static class ApplicationSetup
{
    /// <summary>
    /// Adds application-specific services (DbContext, etc.).
    /// Returns the builder to allow chaining in Program.cs.
    /// </summary>
    public static WebApplicationBuilder AddApplicationSetup(this WebApplicationBuilder builder)
    {
        // Register the application's EF Core DbContext using Aspire's helper which
        // wires the connection string from the AppHost orchestration when present.
        builder.AddNpgsqlDbContext<AppDbContext>("fabriclibrary");

        // Add other application services here in the future (e.g. IJwtService, repositories)

        // Register a hosted service that applies EF Core migrations on startup when
        // running in development or when the APPLY_MIGRATIONS env var is set to "true".
        builder.Services.AddHostedService<MigrationHostedService>();

        return builder;
    }
}
