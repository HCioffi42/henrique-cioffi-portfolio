# Implementation Plan: Multi-Tag Filtering

## Phase 1: Backend Implementation

### 1. Update CQRS Contracts and Logic
- [x] **Step 1.1:** Update `MeuSitePessoal.Application/Artigos/Queries/GetArtigos/GetArtigosQuery.cs`.
    - Add `List<string>? Tags = null` property.
- [x] **Step 1.2:** Update `MeuSitePessoal.Application/Artigos/Queries/GetArtigos/GetArtigosHandler.cs`.
    - Implement `foreach` loop to chain `.Where` clauses for intersection filtering.
    - Capture loop variable to prevent EF Core expression tree closure bugs.

### 2. Controller and Testing
- [x] **Step 2.1:** Update `ArtigosController.cs` to accept `[FromQuery] List<string>? tags`.
- [x] **Step 2.2:** Create `ArtigoSummaryTagFilterTests.cs` in the Integration Tests project.
    - Implement `GetSummaries_WithSingleTagFilter_ShouldReturnMatchingArticles`.
    - Implement `GetSummaries_WithMultipleTagsFilter_ShouldReturnOnlyIntersection`.
    - Validate against the PostgreSQL test container.

## Phase 2: Frontend Implementation

### 3. Service and Contract Updates
- [x] **Step 3.1:** Update `meupessoal-web/src/services/artigoService.ts`.
    - Modify `getArtigoSummaries` to accept a string array.
    - Implement `URLSearchParams` to format the query string for the backend.

### 4. UI Components and URL State
- [x] **Step 4.1:** Update `meupessoal-web/src/pages/ArtigoList.tsx`.
    - Use `searchParams.getAll('tags')` to read the state.
    - Render individual removable filter chips for active tags.
    - Handle empty states gracefully.
- [x] **Step 4.2:** Update `meupessoal-web/src/components/ArticleCard.tsx`.
    - Make tags clickable.
    - Read current `searchParams` and append the new tag before navigating.
- [x] **Step 4.3:** Update `meupessoal-web/src/pages/ArtigoDetalhes.tsx`.
    - Ensure footer tags navigate back to `/?page=1&tags=...`.