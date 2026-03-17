# Track: Implement Multi-Tag Filtering (Intersection)

## Overview
Implementation of a robust, URL-driven multi-tag filtering system for the blog's article list. The architecture pivots from a single-tag route to a query-string based array (`?tags=x&tags=y`), applying an intersection (AND) logic in the backend to ensure high precision in technical content discovery.

## Status
- [x] Specification approved
- [x] Backend Query and Handler updated for `List<string>`
- [x] Integration Tests for single and multiple tags created
- [x] Frontend `artigoService` updated to handle `URLSearchParams`
- [x] Frontend `ArtigoList` refactored to support active filter chips
- [x] Clickable tags implemented in Cards and Article Details

## Related Tracks
- [Track: Server-Side Pagination (Summaries)](../02-server-side-pagination/) (The filtering logic inherits and preserves the URL pagination state).