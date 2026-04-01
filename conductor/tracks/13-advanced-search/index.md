# Track 13: Advanced Search & Discovery - Implementation Summary

## Status: Completed ✅

## Overview
Implemented an advanced search and discovery system. This includes Full-Text Search (FTS) capabilities (via placeholder and fallback) in the backend and a modern, debounced search experience in the frontend. A "Related Stories" recommendation system based on tag intersection was also added to enhance user engagement.

## Changes

### Backend
- **MediatR Queries**:
    - GetArticlesSearchQuery: Handles keyword-based article discovery.
    - GetRelatedArticlesQuery: Calculates tag intersection to recommend similar content.
- **Controller**:
    - Added GET /api/Articles/search endpoint.
    - Added GET /api/Articles/{id}/related endpoint.
- **Logic**: Implemented a robust tag intersection algorithm in GetRelatedArticlesQueryHandler.

### Frontend
- **Hooks**:
    - useDebounce: Custom hook for optimizing API calls during typing.
- **Components**:
    - SearchBar: A reusable, styled search input with clear button and loading state.
- **Pages**:
    - ArticleList: Integrated the SearchBar and updated the fetching logic to prioritize keyword searches.
    - ArticleDetails: Added a "Related Stories" section at the bottom, fetching data dynamically based on the current article's tags.

### Quality Gate
- **Unit Tests**:
    - `GetArticlesSearchQueryHandlerTests`: 6 tests covering title/summary search, case-insensitivity, pagination, and empty terms.
    - `GetRelatedArticlesQueryHandlerTests`: 6 tests covering tag intersection, ranking, base article exclusion, and limits.
- **Integration Tests**:
    - ArticleSearchTests: Validates search accuracy, empty terms, and no-match scenarios.
    - RelatedArticlesTests: Validates recommendation logic and relevance ordering.
- **Frontend Validation**:
    - 
pm run lint: Success.
    - 
pm run build: Success.

## Technical Debt / Observations
- **FTS Optimization**: The backend currently uses a LINQ-based Contains fallback for search. A manual implementation of PostgreSQL 	svector and 	squery is planned for production optimization to handle larger datasets efficiently.
- **Intersection Logic**: For very large datasets, the in-memory intersection calculation in GetRelatedArticlesQueryHandler should be replaced with a database-level query (e.g., using PostgreSQL JSONB or array functions).
