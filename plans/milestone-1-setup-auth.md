# Milestone 1 — Project setup & Auth (Google)

This document is a developer handoff for Milestone 1. It lists tasks, acceptance criteria, environment setup, implementation notes, and short code examples to help a C# developer (backend) get started quickly.

## Objective
Deliver a working project skeleton and a secure Google sign-in flow so an authenticated SPA can call protected backend APIs.

## Deliverables
- Repo skeleton with `backend/` and `ui/` folders and README instructions.
 - .NET Aspire orchestration for dev (Postgres + backend), and `.env.example`.
- ASP.NET Core Web API scaffolded in `backend/` with `Users` model and EF Core migrations applied in dev.
- POST /api/auth/google endpoint that verifies Google ID tokens and upserts a `users` record.
- App-level authentication (JWT or secure cookie) enabling a protected test endpoint (GET /api/me) that returns the current user.
- Basic GitHub Actions CI workflow that builds the backend and runs unit tests.

---

## High-level tasks (for the C# developer)
Below is the Milestone 1 task list with checkboxes to track progress. Mark items as you complete them.

- [ ] 1) Create project skeleton
   - [ ] Create folder `backend/` and initialize an ASP.NET Core Web API (.NET 8 or latest LTS).
   - [ ] Add a README in `backend/` with build/run instructions.
   - [ ] Add `.editorconfig` and keep code style consistent with repo instructions (semicolons, etc.).

- [ ] 2) Add dev environment (.NET Aspire orchestration)
   - [ ] Add or use the existing .NET Aspire orchestration project under `/orchestration` to bring up Postgres (and optional admin DB UI).
   - [ ] Provide `.env.example` with `ConnectionStrings__Default` / `DATABASE_URL` or individual DB variables and document how the Aspire orchestration exposes the DB to the backend.

- [ ] 3) Data access and migrations
   - [ ] Add EF Core + Npgsql packages and configure DbContext.
   - [ ] Implement `User` entity and initial migration.
   - [ ] Provide commands/scripts for applying migrations in dev (and document them in the README).

- [ ] 4) Implement Google ID token verification endpoint
   - [ ] Add endpoint `POST /api/auth/google` accepting `{ id_token }`.
   - [ ] Validate the ID token server-side (see minimal API example below).
   - [ ] Create or update user record using `google_sub` (sub claim) as primary identity mapping.
   - [ ] Issue a secure app auth token (see Token strategy section).

- [ ] 5) Authentication wiring
   - [ ] Configure authentication middleware in ASP.NET Core (JWT Bearer or Cookie auth).
   - [ ] Add a protected test route `GET /api/me` that returns the current user.

- [ ] 6) CI skeleton
   - [ ] Add `.github/workflows/ci.yml` which restores, builds, and runs tests for the backend.

- [ ] 7) Developer documentation
   - [ ] Add a short `backend/README.md` with setup steps, env vars, how to create Google OAuth credentials, and test instructions.

---

## Detailed steps, commands & examples

### 1) Project scaffold
Commands (run in `backend/`):

```powershell
dotnet new webapi -n FabricLibrary.Backend --no-https
cd FabricLibrary.Backend
dotnet sln add FabricLibrary.Backend.csproj
```

Add solution folders if desired. Commit initial scaffold.


### 2) .NET Aspire (dev)
Use the `.NET Aspire` orchestration project in `/orchestration` to start required dev services (Postgres, etc.).

Notes for the orchestration project:
- Ensure the orchestration exposes Postgres on a stable host/port for local development (for example, localhost:5432 or a predictable container host).
- Document how to start and stop the orchestration in `/orchestration/README.md` (examples: run the Aspire project from the IDE or `dotnet run` in the orchestration project folder).
- Add `.env.example` with clear placeholders for DB connection variables the backend will use (e.g. `ConnectionStrings__Default` or `DATABASE_URL`).


### 3) EF Core & Users model
Add NuGet packages:

```powershell
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
```

Create `User` entity (example C#):

```csharp
public class User
{
    public Guid Id { get; set; }
    public string GoogleSub { get; set; } = null!; // sub claim
    public string Email { get; set; } = null!;
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSeenAt { get; set; }
}
```

Configure DbContext and add migration:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Provide a script target in the README for running migrations in dev (or use `dotnet ef database update` via Docker entrypoint using `DOTNET_USE_POLLING_FILE_WATCHER` as needed).


### 4) Google ID token verification endpoint
Recommended libraries: `Google.Apis.Auth` (NuGet: Google.Apis.Auth)

Install package:

```powershell
dotnet add package Google.Apis.Auth
```

Example minimal API handler (simplified):

```csharp
using Google.Apis.Auth;

// In Program.cs (minimal API style)
app.MapPost("/api/auth/google", async (GoogleTokenRequest req, AppDbContext db, IJwtService jwt, IConfiguration cfg) =>
{
   // Validate payload and audience
   var settings = new GoogleJsonWebSignature.ValidationSettings()
   {
      Audience = new[] { cfg["GOOGLE_CLIENT_ID"] }
   };
   var payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken, settings);

   var googleSub = payload.Subject;
   var email = payload.Email;
   var name = payload.Name;

   // Upsert user
   var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleSub == googleSub);
   if (user == null)
   {
      user = new User { GoogleSub = googleSub, Email = email, DisplayName = name };
      db.Users.Add(user);
   }
   else
   {
      user.LastSeenAt = DateTime.UtcNow;
   }
   await db.SaveChangesAsync();

   // Issue app token
   var token = jwt.CreateToken(user.Id, user.Email);
   return Results.Ok(new { token });
});

public record GoogleTokenRequest(string IdToken);
```

Notes:
- This uses the minimal API style (Program.cs) instead of ApiController classes. It performs the same token validation and user upsert flow.
- Validate `payload.Audience` against `GOOGLE_CLIENT_ID` (as shown) to ensure the token was issued for your app.


### 5) Token strategy (recommendation)
Options:
- Server-issued JWT access token (short-lived) + refresh token stored server-side (or rotate refresh token). Use `JwtBearer` for authentication in ASP.NET Core.
- Or, use an HttpOnly secure cookie with an encrypted session token set by the backend. This simplifies CSRF protections for SPAs when using same-site cookies.

Recommendation for MVP: issue a short-lived JWT (15–60m), returned to the SPA and stored in memory (or localStorage if you accept XSS risk). Provide a `/api/auth/refresh` to exchange a refresh token (httpOnly cookie) for a new access token. If you want strictly secure storage, use httpOnly cookies for the refresh token and store the access token client-side.

Implement a small `IJwtService` that signs tokens with a symmetric key (HMAC) stored in env var `JWT_SECRET` for MVP. For production, consider asymmetric keys.


### 6) Protect endpoints and create `GET /api/me`
Register authentication middleware and add a sample controller:

```csharp
[Authorize]
[HttpGet("/api/me")]
public async Task<IActionResult> Me()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var user = await _db.Users.FindAsync(Guid.Parse(userId));
    return Ok(new { id = user.Id, email = user.Email, displayName = user.DisplayName });
}
```

Ensure when issuing JWT you include `sub` or `nameid` claim with user's Guid.


### 7) CI (GitHub Actions) minimal
Create `.github/workflows/ci.yml` that runs:
- `dotnet restore`
- `dotnet build --configuration Release`
- `dotnet test`

Use action `actions/setup-dotnet` to set .NET SDK.


## Environment variables and secrets (backend)
- `ConnectionStrings__Default` or `DATABASE_URL` (Postgres connection)
- `JWT_SECRET` (HMAC secret for signing JWTs)
- `GOOGLE_CLIENT_ID` (to validate audience)
- `SENDGRID_API_KEY` (if adding SendGrid; optional for Milestone 1)

Add `.env.example` with placeholders and document setting them in README.


## Local dev workflow (recommended)
1. Start Postgres:

```powershell
docker-compose up -d db
```

2. In `backend/` restore deps:

```powershell
dotnet restore
```

3. Apply migrations (either running `dotnet ef database update` locally against dev DB or use a startup migration step).

4. Run backend:

```powershell
dotnet run --project FabricLibrary.Backend
```

5. Use Postman / HTTP client to test `POST /api/auth/google` with a Google ID token from frontend or via browser-based Google sign-in flow.


## Acceptance criteria (Milestone 1)
- [ ] `POST /api/auth/google` validates Google ID token, upserts `users` table, and returns an app token.
- [ ] `GET /api/me` returns authenticated user details and is protected.
- [ ] Backend can run locally using Docker Compose Postgres and apply migrations.
- [ ] Basic CI pipeline builds the backend and runs unit tests.
- [ ] Developer README contains steps to create Google OAuth credentials and set `GOOGLE_CLIENT_ID`.


## Estimated time
Approx. 3–5 days for one experienced .NET developer (includes scaffolding, token flow, migrations, and docs). CI and polish may add another day.


## Implementation notes & best practices
- Validate Google token audience (`aud`) against your `GOOGLE_CLIENT_ID`.
- Keep `JWT_SECRET` out of source control and store it in CI secrets and environment manager.
- Rate-limit the auth endpoint to mitigate brute-force or token spam.
- Add monitoring/logging to capture auth errors and invalid token attempts.
- Make sure `GoogleJsonWebSignature.ValidateAsync` is used with caching of Google's certificate keys (the library handles this).


## Optional enhancements (defer to later milestones)
- Use refresh tokens in HTTP-only cookies and rotate them.
- Add OAuth server-side exchange to return an auth code flow for server-issued tokens.
- Implement ASP.NET Core Identity if you plan to add email/password later.
- Add a lightweight integration test that spins up test Postgres container and runs auth flow.

---

If you'd like, I can also:
- Add the `docker-compose.yml` and `.env.example` to the repo now.
- Create the initial `backend/` solution with `User` model and migration scaffolded and committed.

Which of these should I do next?