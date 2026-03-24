# Implementation Plan: Tag & Category Management

## Phase 1: Backend Implementation

### 1. Domain & Migration
- [ ] **Step 1.1:** Create `MeuSitePessoal.Domain/ArticleCategory.cs` with the enum.
- [ ] **Step 1.2:** Modify `MeuSitePessoal.Domain/Article.cs` to include the `Category` property.
- [ ] **Step 1.3:** Create an EF Core migration: `dotnet ef migrations add AddArticleCategory`.
- [ ] **Step 1.4:** Update `BlogDbContext.cs` to map the `Category` property.

### 2. CQRS and Logic
- [ ] **Step 2.1:** Update `GetArticlesQuery` and `ArticleSummaryDto` in `MeuSitePessoal.Application`.
- [ ] **Step 2.2:** Update `GetArticlesQueryHandler.cs`:
    - Chain `.Where(a => a.Category == request.Category)` if category is provided.
    - Ensure tags intersection uses the `Aggregate` pattern to chain `.Where(a => a.Tags.Contains(tag))` on the `IQueryable`.
- [ ] **Step 2.3:** Update `CreateArticleCommand` and `UpdateArticleCommand` Handlers to map the `Category` field.
- [ ] **Step 2.4:** Update `ArticlesController.cs` to support the new query parameters.

### 3. Verification
- [ ] **Step 3.1:** Add integration tests for Category and multi-tag filtering.
- [ ] **Step 3.2:** Verify SQL translation using EF Core logging to ensure no in-memory filtering.

## Phase 2: Frontend Implementation

### 4. Models and Shared Constants
- [ ] **Step 4.1:** Create `meupessoal-web/src/models/ArticleCategory.ts` (Shared Enum/Type).
- [ ] **Step 4.2:** Update `Article.ts` and `ArticleSummary.ts` interfaces.

### 5. Routing and UI
- [ ] **Step 5.1:** Update `App.tsx` to add `<Route path="/tags/:tag" element={<ArticleList />} />`.
- [ ] **Step 5.2:** Refactor `ArticleList.tsx`:
    - Use `useParams()` to detect `:tag`.
    - Implement `useEffect` to synchronize document title and active filters.
    - Add dynamic `<h2>` header based on active filters.
- [ ] **Step 5.3:** Update `articleService.ts` to include `category` in API requests.

### 6. Admin Forms
- [ ] **Step 6.1:** Modify `CreateArticle.tsx` and `EditArticle.tsx`.
    - Add a `<select>` dropdown for categories.
    - Update state management and submit logic.

## Finalization
- [ ] Update `conductor/tracks.md` and create `index.md`.
