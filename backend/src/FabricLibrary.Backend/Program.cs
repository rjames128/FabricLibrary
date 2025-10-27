using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FabricLibrary.Backend.Data;
using FabricLibrary.Backend.Models;
using FabricLibrary.Backend.Services;
using FabricLibrary.Backend.Startup;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;
var services = builder.Services;

// Configure application services (DbContext, auth, etc.)
ApplicationSetup.ConfigureServices(services, configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Minimal API: Google token sign-in
// Map application routes
RouteMapper.MapRoutes(app);

app.Run();

public record GoogleTokenRequest(string IdToken);
