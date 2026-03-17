# Technical Spec: Multi-Tag Filtering (Intersection)

## 1. Goal
Implement a tag-based filtering mechanism that allows users to filter the article summaries by one or multiple tags. The filtering must use an intersection logic (AND), meaning articles must contain *all* selected tags to be returned.

## 2. Backend Implementation (C# / MediatR / EF Core)

### 2.1. Query Expansion: `GetArtigosQuery`
- Modify the existing query to accept an optional collection of tags.
- Property: `List<string>? Tags` (defaults to null).

### 2.2. Handler Logic: `GetArtigosHandler`
- Iterate over the provided `Tags` list.
- Dynamically chain `.Where(a => a.Tags.Contains(tag))` clauses to the Entity Framework `IQueryable`.
- This chained approach ensures the database engine translates the query into an `AND` intersection filter.
- Recalculate `TotalCount` based on the filtered query to maintain accurate pagination.

### 2.3. Controller Interface
- Update `GET /api/artigos/summaries` to accept `[FromQuery] List<string>? tags`.

## 3. Frontend Implementation (React / TypeScript)

### 3.1. API Service Synchronization
- Update `getArtigoSummaries` to accept `tags?: string[]`.
- Utilize the native browser `URLSearchParams` API to append multiple `tags` keys (e.g., `?tags=dotnet&tags=cleancode`) to avoid Axios bracket serialization issues.

### 3.2. URL State Management (`ArtigoList.tsx`)
- Extract tags using `searchParams.getAll('tags')`.
- Ensure pagination `handlePageChange` preserves the active tags in the URL.
- Implement visual feedback: render an array of dismissible "chips" showing the active tags.
- Provide a clear fallback UI when a specific tag combination yields zero results.

### 3.3. Navigation Integration
- **ArticleCard.tsx:** Update tag buttons to append the clicked tag to the *existing* URL parameters, enabling filter composition.
- **ArtigoDetalhes.tsx:** Update the footer tag buttons to navigate back to the home route `/?page=1` while applying the clicked tag filter.

## 4. Testing Strategy
- Add `[Fact]` integration tests in `ArtigoSummaryTagFilterTests` to validate:
    - Single tag filtering accuracy and counts.
    - Multiple tags filtering (intersection/AND logic) exclusivity.~~~~