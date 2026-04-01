# Implementation Plan: Advanced Search & Discovery

## Phase 1: Backend Infrastructure (CQRS)
1. **[ ]** Create GetArticlesSearchQuery and GetArticlesSearchQueryHandler in MeuSitePessoal.Application.
   - Implement basic structure and MediatR integration.
   - Add placeholder for Npgsql Full-Text Search logic as requested.
2. **[ ]** Update ArticlesController in MeuSitePessoal.Api to include the search endpoint.
3. **[ ]** Implement "Related Articles" logic in a new Query or within the existing Article Detail query.

## Phase 2: Frontend Components & Hooks
1. **[ ]** Create useDebounce.ts in meupessoal-web/src/hooks.
2. **[ ]** Create SearchBar.tsx in meupessoal-web/src/components.
   - Use Tailwind CSS for styling.
   - Integrate useDebounce.
3. **[ ]** Integrate SearchBar into Home.tsx.
   - Handle API calls and update article listing based on search results.

## Phase 3: Validation & Testing
1. **[ ]** Add Integration Tests in MeuSitePessoal.Tests/Integration.
   - Focus on search accuracy and edge cases.
2. **[ ]** Manual verification of "Related Articles" on the Article Detail page.
3. **[ ]** Performance check: Ensure search queries are efficient.

## Risks & Mitigations
- **Risk**: FTS performance on large datasets.
- **Mitigation**: User will manually optimize PostgreSQL indexes.
