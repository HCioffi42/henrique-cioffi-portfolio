# Implementation Plan: Tag & Category Management

## Phase 1: Backend Implementation

### 1. Domain & Migration
- [x] **Step 1.1:** Create `MeuSitePessoal.Domain/ArticleCategory.cs` with the enum.
- [x] **Step 1.2:** Modify `MeuSitePessoal.Domain/Article.cs` to include the `Category` property.
- [x] **Step 1.3:** Create an EF Core migration: `dotnet ef migrations add AddArticleCategory`.
- [x] **Step 1.4:** Update `BlogDbContext.cs` to map the `Category` property.

### 2. CQRS and Logic
- [x] **Step 2.1:** Update `GetArticlesQuery` and `ArticleSummaryDto` in `MeuSitePessoal.Application`.
- [x] **Step 2.2:** Update `GetArticlesQueryHandler.cs`:
    - Chain `.Where(a => a.Category == request.Category)` if category is provided.
    - Ensure tags intersection uses the `Aggregate` pattern to chain `.Where(a => a.Tags.Contains(tag))` on the `IQueryable`.
- [x] **Step 2.3:** Update `CreateArticleCommand` and `UpdateArticleCommand` Handlers to map the `Category` field.
- [x] **Step 2.4:** Update `ArticlesController.cs` to support the new query parameters.

### 3. Verification
- [x] **Step 3.1:** Add integration tests for Category and multi-tag filtering.
- [x] **Step 3.2:** Verify SQL translation using EF Core logging to ensure no in-memory filtering.
- [x] **Step 3.3:** Add unit tests for Domain logic, Validators, and Handlers.

## Phase 2: Frontend Implementation

### 4. Models and Shared Constants
- [x] **Step 4.1:** Create `meupessoal-web/src/models/ArticleCategory.ts` (Shared Enum/Type).
- [x] **Step 4.2:** Update `Article.ts` and `ArticleSummary.ts` interfaces.

### 5. Routing and UI
- [x] **Step 5.1:** Update `App.tsx` to add `<Route path="/tags/:tag" element={<ArticleList />} />`.
- [x] **Step 5.2:** Refactor `ArticleList.tsx`:
    - Use `useParams()` to detect `:tag`.
    - Implement `useEffect` to synchronize document title and active filters.
    - Add dynamic `<h2>` header based on active filters.
- [x] **Step 5.3:** Update `articleService.ts` to include `category` in API requests.

### 6. Admin Forms
- [x] **Step 6.1:** Modify `CreateArticle.tsx` and `EditArticle.tsx`.
    - Add a `<select>` dropdown for categories.
    - Update state management and submit logic.

## Finalization
- [x] Update `conductor/tracks.md` and create `index.md`.
- [x] Create `metadata.json`.
