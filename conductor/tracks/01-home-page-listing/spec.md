# Specification: Article Summaries Listing (Home Page)

## Feature Overview
The goal of this track is to implement a high-performance listing of article summaries on the Home page, optimizing both backend database queries and frontend rendering.

## Backend Specification

### Endpoint: `GET /api/Artigos/summaries`
- **Request Type**: `GetArtigosQuery` (via MediatR).
- **Response Type**: `List<ArtigoSummaryDto>`.
- **Logic**:
  - Direct projection from `BlogDbContext.Artigos` to `ArtigoSummaryDto`.
  - Excludes the `Content` (content) column to minimize data transfer.
  - Ordered by `CreatedAt` descending.
  - Uses `.AsNoTracking()` for read-only optimization.

### Data Structures (C#)
- `ArtigoSummaryDto`:
  - `Guid Id`
  - `string Title`
  - `string Summary`
  - `DateTime CreatedAt`
  - `List<string> Tags`

## Frontend Specification

### Model: `ArtigoSummary`
Matches the backend `ArtigoSummaryDto` structure for TypeScript type safety.

### Component: `ArticleCard`
A reusable card component that displays:
- **Title**: Large and bold.
- **Summary**: A concise preview of the article.
- **Creation Date**: Formatted for readability (e.g., "Jan 1, 2024").
- **Tags**: A list of badges for categorized content.

### Page: `ArtigoList` (Home)
The main listing page that:
- Fetches data from `GET /api/Artigos/summaries`.
- Renders a responsive grid or list of `ArticleCard` components.
- Handles loading and "No articles found" states.
- Uses Tailwind CSS for styling.

## Architectural Integrity
- Clean separation between models, services, and components.
- Adheres to MediatR/CQRS patterns on the backend.
- Professional naming without "HC" prefixes.
- English code and documentation.
