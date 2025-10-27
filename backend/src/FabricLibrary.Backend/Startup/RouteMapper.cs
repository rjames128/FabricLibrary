using Google.Apis.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FabricLibrary.Backend.Data;
using FabricLibrary.Backend.Models;
using FabricLibrary.Backend.Services;
using FabricLibrary.Backend.Contracts;

namespace FabricLibrary.Backend.Startup;

public static class RouteMapper
{
    public static void MapRoutes(WebApplication app)
    {
        // Minimal API: Google token sign-in
        app.MapPost("/api/auth/google", async (GoogleTokenRequest req, AppDbContext db, IJwtService jwt, IConfiguration cfg) =>
        {
            if (req is null || string.IsNullOrEmpty(req.IdToken))
                return Results.BadRequest(new { error = "id_token is required" });

            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { cfg["GOOGLE_CLIENT_ID"] }
            };

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken, settings);
            }
            catch (Exception)
            {
                return Results.Unauthorized();
            }

            var googleSub = payload.Subject;
            var email = payload.Email;
            var name = payload.Name;

            // Upsert user
            var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleSub == googleSub);
            if (user == null)
            {
                user = new User { GoogleSub = googleSub, Email = email ?? string.Empty, DisplayName = name };
                db.Users.Add(user);
            }
            else
            {
                user.LastSeenAt = DateTime.UtcNow;
            }
            await db.SaveChangesAsync();

            var token = jwt.CreateToken(user.Id, user.Email);
            return Results.Ok(new FabricLibrary.Backend.Contracts.AuthResponse(token));
        });

        // Protected test endpoint
        app.MapGet("/api/me", async (ClaimsPrincipal userPrincipal, AppDbContext db) =>
        {
            var idClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? userPrincipal.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(idClaim)) return Results.Unauthorized();

            if (!Guid.TryParse(idClaim, out var userId)) return Results.Unauthorized();

            var user = await db.Users.FindAsync(userId);
            if (user == null) return Results.NotFound();

            return Results.Ok(new FabricLibrary.Backend.Contracts.MeResponse(user.Id, user.Email, user.DisplayName));
        }).RequireAuthorization();
    }
}
