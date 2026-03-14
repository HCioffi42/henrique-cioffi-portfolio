# Track: Implement Artigo CRUD Integration Tests

**Track ID**: `implement_crud_integration_tests_20260314`
**Goal**: Create a comprehensive integration test suite for the `UpdateArtigo` and `DeleteArtigo` features.

## Implementation Plan

### 1. Test File Creation
*   **File**: `ArtigoCrudTests.cs` in `MeuSitePessoal.Tests/Integration/Artigos/`.
*   **Base Class**: Inherit from `BaseIntegrationTest`.

### 2. Helper Method
*   **Method**: `SeedArtigoAsync` (Private).
*   **Logic**: Use the `_client` to `POST` a new article and return the `Guid` ID. This ensures we test the full flow and maintain database independence via the existing `BaseIntegrationTest` infrastructure.

### 3. Test Scenarios
1.  **Update Success**:
    *   Seed an article.
    *   Send a `PUT` request with updated data.
    *   Verify `204 No Content`.
    *   Fetch the article and verify the updates.
2.  **Update 404**:
    *   Send a `PUT` request with a non-existent `Guid`.
    *   Verify `404 Not Found`.
    *   Verify the title is "Resource Not Found".
3.  **Update Validation Error**:
    *   Seed an article.
    *   Send a `PUT` request with an empty title.
    *   Verify `400 Bad Request` with "Validation Error" title.
4.  **Delete Success**:
    *   Seed an article.
    *   Send a `DELETE` request.
    *   Verify `204 No Content`.
    *   Try to fetch the article and verify `404 Not Found`.
5.  **Delete 404**:
    *   Send a `DELETE` request with a non-existent `Guid`.
    *   Verify `404 Not Found`.

## Verification Strategy
*   Run `dotnet test` and ensure all tests in `ArtigoCrudTests.cs` pass.
*   Ensure no regressions in existing tests.
