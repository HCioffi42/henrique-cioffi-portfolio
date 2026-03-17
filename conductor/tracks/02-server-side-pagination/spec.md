# Technical Spec: Server-Side Pagination (Summaries)

## 1. Goal
Implement server-side pagination for the article summaries list used on the home page. This ensures efficient data retrieval as the number of blog posts grows.

## 2. Generic Pagination Wrapper
Design a generic `PagedResult<T>` class to encapsulate paginated data and its metadata.

### Data Model: `PagedResult<T>`
Location: `MeuSitePessoal.Application.Common.Models`

| Property | Type | Description |
| :--- | :--- | :--- |
| `Items` | `List<T>` | The collection of items for the current page. |
| `TotalCount` | `int` | The total number of records across all pages. |
| `PageNumber` | `int` | The current page number (1-indexed). |
| `PageSize` | `int` | The number of items per page. |
| `TotalPages` | `int` | The total number of pages calculated from `TotalCount` and `PageSize`. |

## 3. Query Expansion: `GetArtigosQuery`
Update `GetArtigosQuery` to support pagination parameters.

### Properties
- `PageNumber` (int, default: 1)
- `PageSize` (int, default: 10)

### Return Type
Change return type from `List<ArtigoSummaryDto>` to `PagedResult<ArtigoSummaryDto>`.

## 4. Handler Logic: `GetArtigosHandler`
Update the MediatR handler to perform the following operations:
1.  Count total records matching the query (for `TotalCount`).
2.  Apply `.Skip()` based on `(PageNumber - 1) * PageSize`.
3.  Apply `.Take()` based on `PageSize`.
4.  Project results to `ArtigoSummaryDto`.
5.  Return a new `PagedResult<ArtigoSummaryDto>`.

## 5. API Contract: `ArtigosController.GetSummaries`
Update the `GET /api/artigos/summaries` endpoint.

### Query Parameters
- `pageNumber` (int, optional, default: 1)
- `pageSize` (int, optional, default: 10)

### Response
- **Status Code:** 200 OK
- **Body:** `PagedResult<ArtigoSummaryDto>`
