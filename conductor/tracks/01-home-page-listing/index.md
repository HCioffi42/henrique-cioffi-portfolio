# Track: Implement GET /api/Artigos/summaries

This track implemented an optimized query to list article summaries for the home page, using MediatR/CQRS and direct DbContext projection.

## Changes

### Application
- Added project reference to `MeuSitePessoal.Infrastructure`.
- Created `ArtigoSummaryDto`.
- Created `GetArtigosQuery` returning `List<ArtigoSummaryDto>`.
- Created `GetArtigosHandler` using `BlogDbContext` for optimized direct projection (no `Conteudo` fetching, `AsNoTracking`).
- Moved `ITokenService` to `MeuSitePessoal.Domain` to resolve a circular dependency between `Application` and `Infrastructure`.

### Infrastructure
- Removed project reference to `MeuSitePessoal.Application` to break the dependency cycle.
- Updated `TokenService` to use the new `ITokenService` location in `Domain`.

### API
- Updated `ArtigosController` with a new `GET /api/Artigos/summaries` endpoint.
- Updated `AuthController` and `Program.cs` to reflect the new `ITokenService` location.

## Results
- The home page can now fetch a lightweight list of articles.
- Architectural integrity maintained by resolving the circular dependency.
- All internal naming and comments follow the `HC` branding.
