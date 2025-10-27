instructions
---
applyTo: "**/*.cs, **/*.csproj, **/*.sln"
---

## Purpose

This file documents C#/.NET coding conventions and engineering guidance for the FabricLibrary backend and any shared C# code. The goal is maintainable, safe, and idiomatic .NET code that integrates with project tooling and CI.

## Brief contract

- Inputs: C# source files (`**/*.cs`), project files (`*.csproj`), solution files.
- Outputs: Readable, tested, analyzers-passing code that builds cleanly on CI.
- Error modes: Compiler errors, analyzer warnings, runtime null refs, resource leaks.
- Success: PRs compile with no new warnings (configurable), analyzers pass, unit tests and CI checks green.

## High-level rules

- Target modern LTS .NET SDKs (for example .NET 8 LTS if available and compatible). Use the same SDK across the team.
- Enable nullable reference types across projects: `<Nullable>enable</Nullable>` in `.csproj`.
- Favor small, single-purpose classes and methods. Keep methods < ~50 lines where practical.
- Prefer explicit types for public APIs; `var` is fine for local variables when the type is obvious from the right-hand side.
- Use PascalCase for types and methods, camelCase for parameters and private fields (with underscore prefix `_field` if project prefers it).
- Interfaces start with `I` (e.g., `IFabricRepository`).
- Use `readonly` for fields that never change after construction.
- Prefer dependency injection (constructor injection) for services.
- Keep exception handling focused — don’t catch System.Exception unless rethrowing or wrapping with additional context.
- Prefer minimal APIs and avoid over-engineering; YAGNI applies.
- Add documentation comments to explain non-obvious logic, public APIs, and complex algorithms.
- Add comments to explain decisions that are not obvious from the code itself.

## Project & csproj guidance

- Use SDK-style projects and single-target where possible. Multi-target only if needed.
- Include these in common `PropertyGroup`:
  - `<LangVersion>latest</LangVersion>`
  - `<Nullable>enable</Nullable>`
  - `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` (consider only for CI or after warming up).
  - `<ImplicitUsings>enable</ImplicitUsings>` optionally for minimal program templates.
- Centralize versioning and shared properties using Directory.Build.props if multiple projects exist.

## Formatting, analyzers and tools

- Use EditorConfig at repo root to enforce formatting and rules. Example entries:
  - c#_style_var_elsewhere = true/false per team preference
  - dotnet_style_qualification_for_field = false
- Use Roslyn analyzers:
  - Microsoft.CodeAnalysis.NetAnalyzers (recommended)
  - StyleCop.Analyzers (optional; enable only rules you agree on)
  - SonarLint / other quality tools as needed
- Use `dotnet format` in CI or pre-commit to keep formatting consistent.
- Prefer fixing analyzer warnings rather than suppressing them; if suppression is necessary add a comment and create a follow-up issue.

## Naming, files & organization

- One public type per file named after the type (e.g., `FabricService.cs` contains `public class FabricService`).
- Use file-scoped namespaces (C# 10+) for brevity:
  - namespace FabricLibrary.Services;
- Organize code by feature or layer (e.g., `Services/`, `Controllers/`, `Data/`, `Models/`, `Dtos/`, `Tests/`).

## Async & threading

- Use async/await liberally for I/O-bound work. Avoid `async void` except for top-level event handlers.
- Avoid blocking on async (no `.Result` or `.Wait()`); prefer `await`.
- Use `ConfigureAwait(false)` in library code if the library may be used in contexts where capturing SynchronizationContext matters (less necessary in ASP.NET Core but still a conscious choice).
- Cancel long-running operations using `CancellationToken`.

## Dependency Injection & ASP.NET Core patterns

- Register services with appropriate lifetimes:
  - Singleton — stateless, thread-safe singletons.
  - Scoped — per-request services (DbContext should be scoped).
  - Transient — light, short-lived services.
- Prefer explicit registration over scanning when clarity matters.
- Avoid service locator pattern; prefer constructor injection.
- Keep controllers thin; push logic to services.

## EF Core guidance (if used)

- Keep `DbContext` scoped.
- Use async EF Core methods (`ToListAsync`, `FirstOrDefaultAsync`, etc.).
- Avoid returning IQueryable from public API surface; materialize in service layer.
- Use migrations for schema changes and keep migrations in source control.
- Be mindful of N+1 queries; use eager loading (`Include`) or explicit queries.

## Error handling & logging

- Log with structured logging (Microsoft.Extensions.Logging). Use meaningful event names and properties.
- Do not swallow exceptions silently. Add contextual details when rethrowing (use `throw;` to preserve stack or `throw new Exception("context", ex)` carefully).
- Map exceptions to user-friendly API responses at the boundary (controllers/middleware) with a consistent error contract.
- Use exception filters or middleware for centralized handling.

## Security & secrets

- Never store secrets in source control. Use user secrets for local development and environment secrets in CI/CD.
- Validate inputs and sanitize outputs where necessary.
- Keep packages up-to-date and monitor known vulnerabilities.

## Performance & memory

- Prefer `Span<T>`/`Memory<T>` patterns for hot paths and high-throughput code when appropriate.
- Avoid large object allocations; reuse buffers when possible.
- Use `IAsyncEnumerable<T>` for streaming large datasets.
- Consider caching only where it simplifies performance and not as primary correctness mechanism.

## Tests

- Unit tests: xUnit recommended (NUnit/MSTest acceptable).
- Use Moq/Mock frameworks for dependencies. Prefer hand-written fakes for complex behavior when clearer.
- Integration tests: use TestServer (for ASP.NET Core) or Docker-based tests for external dependencies.
- Keep tests fast and deterministic. Use `dotnet test` with proper test isolation.
- Include a baseline code coverage expectation in CI (not necessarily 100%).

## Documentation & XML docs

- Add XML docs for public APIs (enable `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in csproj for libraries).
- Keep README or docs for non-obvious architectural decisions.

## PR checklist

- [ ] Solution builds cleanly: `dotnet build` (no errors).
- [ ] Type check and analyzers: no new high-severity analyzer warnings.
- [ ] Unit tests and integration tests added/updated: `dotnet test` passes.
- [ ] No new TODOs left in code without tracked issue.
- [ ] XML docs added/updated for public surface changes.
- [ ] Secrets not added to code or config.
- [ ] Perf implications noted if changes alter hot paths.

## When to suppress warnings

- Rarely. If you must:
  - Use localized suppression with justification comments.
  - Prefer to file a follow-up issue and track technical debt.

## Common edge cases to watch

- Null reference exceptions — use nullable annotations and guard early.
- Async deadlocks and fire-and-forget tasks.
- Long-running synchronous work on thread pool threads.
- Disposing `IDisposable`/`IAsyncDisposable` incorrectly — ensure deterministic disposal.

## Small examples

- Enable nullable in csproj:
  - <PropertyGroup>
      <TargetFramework>net8.0</TargetFramework>
      <Nullable>enable</Nullable>
    </PropertyGroup>

- Async method example:
  - public async Task<FabricDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _dbContext.Fabrics.FindAsync(new object?[] { id }, ct);
        if (entity is null) throw new NotFoundException(id);
        return _mapper.Map<FabricDto>(entity);
    }

- DI registration example:
  - services.AddScoped<IFabricRepository, FabricRepository>();

- Record example (immutable DTO):
  - public record FabricDto(Guid Id, string Name, decimal Price);

## Recommended toolchain

- .NET SDK (matching team target, e.g., .NET 8).
- dotnet format
- Microsoft.CodeAnalysis.NetAnalyzers
- StyleCop.Analyzers (optional, team-enforced rule set)
- xUnit + Moq
- GitHub Actions / Azure Pipelines for CI running build, test, format, analyzers

## Appendix — CI quick checks (suggested)

- dotnet restore
- dotnet build --configuration Release
- dotnet test --no-build --configuration Release
- dotnet format --verify-no-changes (or run format on commits)
- run analyzers (via build or dedicated step)

---
