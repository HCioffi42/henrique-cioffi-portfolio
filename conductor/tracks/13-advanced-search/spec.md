# Track Specification: Advanced Search & Discovery

## 1. Overview
Implement an advanced search and discovery system to allow users to find articles by keywords and see related content. This track covers the full-stack implementation from PostgreSQL Full-Text Search (FTS) to a debounced frontend search bar.

## 2. Backend Specification

### 2.1. API Contract
- **Endpoint**: GET /api/Articles/search
- **Query Parameters**:
  - SearchTerm (string): The text to search for.
  - PageNumber (int, default 1): For pagination.
  - PageSize (int, default 10): For pagination.
- **Response**: PaginatedList<ArticleSummaryDTO>
  - Includes Id, Title, Summary, Tags, and PublishedDate.

### 2.2. MediatR Query
- **Name**: GetArticlesSearchQuery
- **Location**: MeuSitePessoal.Application/Articles/Queries/GetArticlesSearch
- **Logic**: Use PostgreSQL 	svector for FTS.
  - *Note: Npgsql-specific logic placeholder for user implementation.*
  - The query should filter by Title and Summary.

### 2.3. DTO Updates
- Ensure ArticleSummaryDTO is fully populated to support the search results UI.

### 2.4. Related Articles Logic
- **Method**: Recommend articles based on shared tags.
- **Algorithm**:
  1. Get tags of the current article.
  2. Find other articles that share at least one tag.
  3. Sort by the number of intersecting tags (descending).
  4. Take the top 3-5 results.

## 3. Frontend Specification

### 3.1. SearchBar Component
- **Tech Stack**: React + Tailwind CSS.
- **Behavior**:
  - Input field with a search icon.
  - Triggers search on change with a 500ms debounce.
  - Displays "Searching..." state or "No results found".

### 3.2. Custom Hook: useDebounce
- **Approach**: native React useEffect + setTimeout.
- **Contract**: useDebounce<T>(value: T, delay: number): T

### 3.3. Integration
- Add the SearchBar to the Home page header or top section.
- Update Home page state to handle search results.

## 4. Quality Gate (SDET Focus)

### 4.1. Integration Tests
- **Target**: ArticlesController search endpoint.
- **Scenarios**:
  1. Valid search term returns matching articles.
  2. Empty search term returns all articles (or default behavior).
  3. Term with no matches returns empty paginated list.
  4. Special characters handling.
  5. Case-insensitivity check.

## 5. Architectural Alignment
- **Clean Architecture**: Strictly separate logic into MediatR handlers.
- **SOLID**: Ensure the search service or logic is decoupled from the controller.
- **Performance**: Use AsNoTracking() in EF Core for search queries.
