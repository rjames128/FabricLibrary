using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using FabricLibrary.Backend.Data;
using FabricLibrary.Backend.Services;

namespace FabricLibrary.Backend.Startup;

public static class ApplicationSetup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext (connection string provided via env or appsettings)
        services.AddDbContext<AppDbContext>(opts =>
        {
            var conn = configuration.GetConnectionString("Default") ?? configuration["ConnectionStrings:Default"]; 
            if (string.IsNullOrEmpty(conn))
            {
                // Use a placeholder local connection for developer convenience. Replace in production.
                conn = "Host=localhost;Port=5432;Database=fabricdb;Username=fabric;Password=fabric";
            }
            opts.UseNpgsql(conn);
        });

        // Jwt service
        services.AddScoped<IJwtService, JwtService>();

        // Authentication - JWT Bearer (minimal config for MVP)
        var jwtSecret = configuration["JWT_SECRET"] ?? "dev-secret-change-me";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // for local dev
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateLifetime = true
            };
        });

        services.AddAuthorization();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
}
