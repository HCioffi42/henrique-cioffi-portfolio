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
- [x] **Manual Test:** Use Swagger or a tool like Postman to verify the `GET /api/artigos/summaries` endpoint.

## Phase 2: Frontend Implementation

### 5. Data Model and Service
Align the frontend models and services with the new paginated API.

- [x] Create `meupessoal-web/src/models/PagedResult.ts`.
  - Define generic `PagedResult<T>` interface.
- [x] Update `meupessoal-web/src/services/artigoService.ts`.
  - Update `getArtigoSummaries` to accept `page: number` and `pageSize: number`.
  - Update return type to `Promise<PagedResult<ArtigoSummary>>`.

### 6. UI Components
Build the pagination controls.

- [x] Create `meupessoal-web/src/components/Pagination.tsx`.
  - Implement functional component with Props defined in spec.
  - Apply Tailwind CSS styles for buttons and current page indicator.
  - Implement disabled states for first/last page.

### 7. Page Integration (URL Sync)
Integrate pagination into the article list.

- [x] Update `meupessoal-web/src/pages/ArtigoList.tsx` (or the home page listing component).
  - Use `useSearchParams` to manage the `page` parameter.
  - Update `useEffect` to trigger a fetch when the `page` parameter changes.
  - Implement `handlePageChange` to update the URL via `setSearchParams`.
  - Pass pagination metadata and the callback to the `Pagination` component.

### 8. Verification and Polishing
- [x] Verify that navigating to `/?page=2` correctly fetches the second page.
- [x] Verify that clicking "Previous"/"Next" updates the URL and fetches data.
- [x] Ensure the loading state is shown correctly during page transitions.