# Implementation Plan: Implement Pagination for Artigo List

## 1. Create PagedList Class
- Create `MeuSitePessoal.Application/Common/Models/PagedList.cs`.
- Include properties: `Items`, `CurrentPage`, `TotalPages`, `PageSize`, `TotalCount`, `HasPrevious`, `HasNext`.

## 2. Update Application Layer
- Update `GetTodosArtigosQuery` in `MeuSitePessoal.Application/Artigos/Queries/GetTodosArtigos/GetTodosArtigosQuery.cs` to include `PageNumber` and `PageSize`.
- Update `GetTodosArtigosQueryHandler` to use these parameters.
- Calculate `TotalCount` and apply `.Skip()` and `.Take()` using EF Core.

## 3. Update Presentation Layer
- Update `ArtigosController.cs` in `MeuSitePessoal.Api/Controllers/ArtigosController.cs`.
- Modify `ListarTodos` to accept `[FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10`.
- Return the `PagedList` or include metadata in headers.

## 4. Verification and Testing
- Update `MeuSitePessoal.Tests/Integration/Artigos/ArtigosIntegrationTests.cs`.
- Add test cases for different page sizes and page numbers.
- Verify total count and items returned.
