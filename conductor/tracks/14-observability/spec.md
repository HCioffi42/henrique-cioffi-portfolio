# Technical Specification - Track 14: Observability & Resilience

## 1. Goal
Enhance backend observability with Health Checks, Caching for improved performance, and Structured Tracing with OpenTelemetry.

## 2. Technical Scope
- **No Frontend Changes**: This track is strictly Backend-focused. No files in `meupessoal-web` will be modified.
- **Health Checks**: Implement `Microsoft.Extensions.Diagnostics.HealthChecks` to monitor system vitals.
- **Caching**: Implement `IMemoryCache` in the article listing query to reduce database load.
- **Tracing**: Integrate OpenTelemetry for basic HTTP and SQL tracing, enhancing the existing Serilog-based logging.

## 3. Architecture
### 3.1 Health Checks (Task 14.1)
- **Endpoint**: `/health` (exposed in `MeuSitePessoal.Api`).
- **Checks**:
  - **PostgreSQL**: Verify connection to the database using `DbContext`.
- **Response**: Returns `Healthy` (200), `Degraded` (200), or `Unhealthy` (503).

### 3.2 Caching (Task 14.2)
- **Implementation**: `IMemoryCache` (In-memory caching).
- **Target**: `GetArticlesQueryHandler` (Retrieval of paginated article summaries).
- **Strategy**:
  - **Key Generation**: Unique key derived from query parameters: `PageNumber`, `PageSize`, `Tags` (serialized), and `Category`.
  - **Expiration**: Sliding expiration of 10 minutes.
  - **Invalidation**: 
    - The cache must be invalidated whenever an article is **Created**, **Updated**, or **Deleted**.
    - For simplicity in this track, a global "Articles" cache version or prefix will be cleared to ensure consistency.

### 3.3 Tracing & Logging (Task 14.3)
- **Logging**: Maintain Serilog for structured logging.
- **Tracing**: OpenTelemetry integration.
  - **Instrumentation**: 
    - `OpenTelemetry.Instrumentation.AspNetCore`: For incoming HTTP requests.
    - `OpenTelemetry.Instrumentation.EntityFrameworkCore`: For database queries.
  - **Exporter**: Console exporter for development/Docker (OTLP exporter can be added if a collector is available).
- **Isolation**: Tracing configuration should be centralized in `MeuSitePessoal.Infrastructure.Logging` or a similar infrastructure layer.

## 4. Data Models & Contracts
### Health Check Response (Standard ASP.NET Core)
```json
{
  "status": "Healthy",
  "entries": {
    "PostgreSQL": {
      "status": "Healthy",
      "description": "Postgres connection is valid."
    }
  }
}
```

## 5. Dependencies
- `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`
- `OpenTelemetry.Extensions.Hosting`
- `OpenTelemetry.Instrumentation.AspNetCore`
- `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- `OpenTelemetry.Exporter.Console` (or OTLP)
