# Specification: Implement Pagination for Artigo List

## Objective
Update the `GetTodosArtigosQuery` and its handler to support pagination, ensuring that large numbers of articles can be efficiently retrieved.

## Requirements
1. Create a generic `PagedList<T>` class in the Application layer to wrap paginated results and metadata (CurrentPage, TotalPages, PageSize, TotalCount).
2. Update `GetTodosArtigosQuery` and its Handler to accept `PageNumber` and `PageSize` parameters.
3. Use EF Core's `.Skip()` and `.Take()` to perform pagination at the database level.
4. Update `ArtigosController.ListarTodos` to accept these parameters using `[FromQuery]` with default values (e.g., page 1, size 10).
5. Ensure the response includes the pagination metadata.
6. Update existing integration tests to verify that pagination works (e.g., requesting 2 items returns exactly 2 items).
