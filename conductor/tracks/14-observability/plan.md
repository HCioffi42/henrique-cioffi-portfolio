# Implementation Plan - Track 14: Observability & Resilience

## Phase 1: Health Checks (Task 14.1)
- [x] Install `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` in `MeuSitePessoal.Api`.
- [x] Register HealthChecks in `Program.cs`.
  - Add `AddHealthChecks().AddDbContextCheck<BlogDbContext>("PostgreSQL")`.
- [x] Map HealthCheck endpoint in `Program.cs` to `/health`.
- [x] Verify endpoint response (200 OK / 503 Unhealthy) using `curl`.
- [x] Ensure the endpoint is anonymous.

## Phase 2: Caching (Task 14.2)
- [x] Install `Microsoft.Extensions.Caching.Memory` in `MeuSitePessoal.Application`.
- [x] Register `IMemoryCache` in `Program.cs` via `builder.Services.AddMemoryCache()`.
- [x] Update `GetArticlesQueryHandler.cs`:
  - Inject `IMemoryCache`.
  - Implement cache key generation logic based on `PageNumber`, `PageSize`, `Tags`, `Category`, and `CacheVersion`.
  - Check cache before database query.
  - Store results in cache with 10 minutes sliding expiration.
- [x] Update `CreateArticleHandler.cs`:
  - Inject `IMemoryCache`.
  - Invalidate the article listing cache using `Articles_CacheVersion`.
- [x] Update `UpdateArticleCommandHandler.cs` and `DeleteArticleCommandHandler.cs` to also invalidate the cache.
- [x] Test caching behavior.

## Phase 3: Tracing & Logging (Task 14.3)
- [x] Add OpenTelemetry NuGet packages to `MeuSitePessoal.Infrastructure.Logging`.
  - `OpenTelemetry.Extensions.Hosting`
  - `OpenTelemetry.Instrumentation.AspNetCore`
  - `OpenTelemetry.Instrumentation.EntityFrameworkCore`
  - `OpenTelemetry.Exporter.Console`
- [x] Update `DependencyInjection.cs` in `MeuSitePessoal.Infrastructure.Logging`:
  - Create `AddCustomTracing`.
  - Configure OpenTelemetry with `AddAspNetCoreInstrumentation()` and `AddEntityFrameworkCoreInstrumentation()`.
- [x] Call `AddCustomTracing()` in `Program.cs`.
- [x] Verify structured logs and traces.

## 4. Verification Strategy
- **Health Check**: `curl http://localhost:5000/health`.
- **Caching**: 
  - Fetch articles twice; the second request should not trigger a SQL query in logs.
  - Create/Update an article and verify the next fetch triggers a fresh SQL query.
- **Tracing**: Confirm OpenTelemetry logs appear in the application output.

## 5. Constraints
- **NO FRONTEND CHANGES**: Do not modify `meupessoal-web`.
- Follow Clean Architecture and MediatR patterns.
- Ensure all new methods/classes have XML documentation.
