# FabricLibrary Backend (Milestone 1 scaffold)

This folder contains a minimal ASP.NET Core backend scaffold for the FabricLibrary project.

What is included
- Minimal API style `Program.cs` with a Google sign-in endpoint (`POST /api/auth/google`) and a protected test endpoint (`GET /api/me`).
- `AppDbContext` + `User` model using EF Core.
- `IJwtService` and a simple `JwtService` that issues HMAC-signed JWTs.
- `FabricLibrary.Backend.csproj` with package references for EF Core, Npgsql, Google token validation and Swagger.

Run locally (developer machine)
1. Ensure .NET 8 SDK is installed.
2. Start the development orchestration in `/orchestration` (this repo uses .NET Aspire for dev orchestration) so Postgres is available.
3. Set environment variables or `appsettings.json` with a valid Postgres connection string (`ConnectionStrings:Default`) and `JWT_SECRET`.

Example (PowerShell):

```powershell
# set env vars for this session
$env:ConnectionStrings__Default = 'Host=localhost;Port=5432;Database=fabricdb;Username=fabric;Password=fabric'
$env:JWT_SECRET = 'dev-secret-change-me'
$env:GOOGLE_CLIENT_ID = '<your-google-client-id>'

cd backend\src\FabricLibrary.Backend
dotnet restore
dotnet build
dotnet run
```

Notes for the dev
- This scaffold is intentionally minimal so the team can review and iterate.
- Migrations are not committed here; run `dotnet ef migrations add InitialCreate` and `dotnet ef database update` once the DB is reachable.
- The Google token validation uses `Google.Apis.Auth` and validates audience against `GOOGLE_CLIENT_ID` from configuration.

Next steps
- Review the code and tell me what you want changed before I mark Task 1 complete.
- I can add EF migrations, tests, and CI next if you approve the scaffold.
