# Technical Spec: Tag & Category Management (Refined)

## 1. Goal
Extend the article management system to include a dedicated **Category** classification and implement a dynamic **Tag-specific discovery route** on the frontend. This track ensures that articles are organized by a primary, standardized category while maintaining the existing multi-tag intersection filtering with high SQL performance.

## 2. Backend Implementation (C# / EF Core / MediatR)

### 2.1. Domain & Data Consistency
- **Category Enum:** Define `ArticleCategory` enum in `MeuSitePessoal.Domain`.
    - Options: `Technology`, `Tutorial`, `Life`, `News`, `Opinion`, `Projects`.
- **Article.cs:** Add `ArticleCategory Category` property.
- **Migration:** Add the `Category` column to the `Articles` table.

### 2.2. SQL Performance & Handler Logic
- **GetArticlesQueryHandler:**
    - **Intersection Logic:** Ensure the `IQueryable` uses multiple chained `.Where(a => a.Tags.Contains(tag))` clauses. 
    - **SQL Translation:** Verify that Npgsql translates this to a efficient PostgreSQL array overlap or containment check (e.g., `@>`).
    - **No In-Memory Filtering:** Explicitly forbid `.AsEnumerable()` or `.ToList()` before all filters (Category and Tags) are applied.
- **Filter by Category:** Add `ArticleCategory? Category` to `GetArticlesQuery` and apply a single `.Where` if present.

### 2.3. Command & DTO Updates
- **Create/Update Commands:** Include `ArticleCategory Category`.
- **ArticleSummaryDto:** Include `ArticleCategory Category`.

## 3. Frontend Implementation (React / TypeScript)

### 3.1. Routing & State Management
- **App.tsx:** Implement `/tags/:tag` route.
- **ArticleList.tsx:** 
    - Capture `:tag` from `useParams`.
    - Update `document.title` based on the active filter (e.g., "Articles tagged #dotnet - MySite").
    - **Dynamic Header:** Display "Articles tagged with #tag" or "Category: Technology" when filters are active.

### 3.2. Form Components
- **Category Selection:** Replace text input with a `<select>` or `Dropdown` component in `CreateArticle` and `EditArticle`.
- **Predefined Options:** Use a shared constant array matching the Backend Enum.

## 4. Testing Strategy
- **Backend Integration:** Validate that multiple tags results in an `AND` intersection in the generated SQL (via logs or result verification).
- **Frontend E2E/Manual:** Verify the dynamic header and document title updates correctly when navigating via tags.
