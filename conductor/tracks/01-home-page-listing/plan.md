# Plan: Implement GET /api/Artigos/summaries

This track implements a optimized query to list article summaries for the home page, using MediatR/CQRS and direct DbContext projection.

## Implementation Steps

1. **Infrastructure Reference**: Add a project reference from `MeuSitePessoal.Application` to `MeuSitePessoal.Infrastructure`.
   - *Note: To avoid circular dependency, I'll check if I need to remove the reference from `Infrastructure` to `Application` and what it breaks.*
2. **DTO Creation**: Create `ArtigoSummaryDto` in `MeuSitePessoal.Application/Artigos/Queries/GetArtigos/ArtigoSummaryDto.cs`.
   - Fields: `Id`, `Titulo`, `Resumo`, `DataCriacao`, `Tags`.
3. **Query Creation**: Create `GetArtigosQuery` in `MeuSitePessoal.Application/Artigos/Queries/GetArtigos/GetArtigosQuery.cs`.
   - Returns: `List<ArtigoSummaryDto>`.
4. **Handler Implementation**: Create `GetArtigosHandler` in `MeuSitePessoal.Application/Artigos/Queries/GetArtigos/GetArtigosHandler.cs`.
   - Use `BlogDbContext` from `MeuSitePessoal.Infrastructure.Data`.
   - Project directly from `_context.Artigos` using `.Select()` and `.AsNoTracking()`.
   - Order by `DataCriacao` descending.
   - Do NOT fetch `Conteudo`.
5. **Controller Update**: Add `GetSummaries` action to `ArtigosController.cs`.
   - Route: `api/Artigos/summaries`.
   - Returns `Ok(List<ArtigoSummaryDto>)`.
6. **Verification**: Run build and verify if everything is correct.

## Branding & Standards
- Use "HC" for all internal naming and third-person descriptive comments.
- Adhere strictly to MediatR/CQRS patterns.
- Ensure no drift into other files.
