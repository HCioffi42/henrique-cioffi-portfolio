# Implementation Plan: Server-Side Pagination (Summaries)

## 1. Generic Pagination Wrapper
Create the `PagedResult<T>` class to serve as a standardized return type for paginated queries.

### Tasks
- [x] **Step 2.2:** Create `MeuSitePessoal.Application/Common/Models/PagedResult.cs`.
  - Properties: `Items`, `TotalCount`, `PageNumber`, `PageSize`, `TotalPages`.
  - Constructor to calculate `TotalPages`.

## 2. Query and Handler Implementation
Modify the existing `GetArtigosQuery` and `GetArtigosHandler` to implement the pagination logic.

### Tasks
- [x] **Step 2.1 (Query):** Update `MeuSitePessoal.Application/Artigos/Queries/GetArtigos/GetArtigosQuery.cs`.
  - Add `PageNumber` and `PageSize` properties with default values.
  - Update the request interface to return `PagedResult<ArtigoSummaryDto>`.
- [x] **Step 2.1 (Handler):** Update `MeuSitePessoal.Application/Artigos/Queries/GetArtigos/GetArtigosHandler.cs`.
  - Modify `Handle` method to:
    1.  Count total articles (`_context.Artigos.CountAsync()`).
    2.  Apply `.Skip()` and `.Take()` logic to the database query.
    3.  Return the results wrapped in `PagedResult`.

## 3. Controller Integration
Expose the pagination parameters in the `ArtigosController`.

### Tasks
- [x] Update `MeuSitePessoal.Api/Controllers/ArtigosController.cs`.
  - Modify `GetSummaries()` to accept `[FromQuery] int pageNumber = 1` and `[FromQuery] int pageSize = 10`.
  - Pass these parameters into the `GetArtigosQuery` constructor.

## 4. Testing and Validation
Verify the changes using integration tests and manual API testing.

### Tasks
- [x] **Integration Test:** Update or create an integration test to verify the pagination response.
  - Ensure `TotalCount` is correct.
  - Ensure `Items.Count` matches `PageSize`.
  - Ensure the response structure matches `PagedResult`.
- [ ] **Manual Test:** Use Swagger or a tool like Postman to verify the `GET /api/artigos/summaries` endpoint.
