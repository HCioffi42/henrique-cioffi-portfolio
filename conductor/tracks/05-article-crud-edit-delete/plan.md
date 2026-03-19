# Implementation Plan: Article CRUD (Edit & Delete)

**Track ID**: `05-article-crud-edit-delete`

This plan outlines the steps to implement `Update` and `Delete` operations for articles, including an administrative dashboard.

## Phase 1: Backend Infrastructure (AutoMapper & MediatR)

### 1.1 Setup AutoMapper
- **Task**: Install and configure AutoMapper.
- **Action**: 
    - Install `AutoMapper.Extensions.Microsoft.DependencyInjection` in `Application` and `Api` projects.
    - Create a mapping profile (e.g., `MappingProfile.cs`) in the `Application` layer.
    - Register AutoMapper in `Program.cs`.
- **Verification**: Ensure the project builds without errors.

### 1.2 Implement Update Article
- **Task**: Create `UpdateArtigoCommand` and its handler.
- **Action**:
    - Implement `UpdateArtigoCommand` with `Id`, `Titulo`, `Conteudo`, `Resumo`, and `Tags`.
    - Implement `UpdateArtigoCommandHandler` using `IMapper` to map the command to the existing entity.
- **Verification**: Unit test the handler to confirm entity fields are correctly updated.

### 1.3 Implement Delete Article
- **Task**: Create `DeleteArtigoCommand` and its handler.
- **Action**: Implement the command and handler to remove an article from the repository by ID.
- **Verification**: Unit test the handler to confirm the repository's delete method is called.

### 1.4 Update ArtigosController
- **Task**: Add `PUT` and `DELETE` endpoints.
- **Action**:
    - Implement `PUT /api/artigos/{id}` and `DELETE /api/artigos/{id}`.
    - Apply the `[Authorize]` attribute to both.
- **Verification**: Use Swagger or Postman to verify that unauthorized requests are blocked and authorized requests work as expected.

## Phase 2: Frontend Implementation (Dashboard & Forms)

### 2.1 Admin Dashboard with Pagination
- **Task**: Create `src/pages/Dashboard.tsx`.
- **Action**: 
    - Fetch articles from `GET /api/artigos` using pagination parameters (`pageNumber`, `pageSize`).
    - Implement a table with columns: Title, Created At, Tags, Actions.
    - Add pagination controls.
- **Verification**: Verify that the table correctly displays data and pagination works (fetches new data on page change).

### 2.2 Edit Article Page
- **Task**: Create `src/pages/EditArtigo.tsx`.
- **Action**:
    - Reuse the form component from article creation.
    - Fetch article details by ID and pre-fill the form fields.
    - Implement the update logic calling `PUT /api/artigos/{id}`.
- **Verification**: Successfully edit an existing article and verify changes in the dashboard/home page.

### 2.3 Delete Confirmation Modal
- **Task**: Create `src/components/DeleteModal.tsx`.
- **Action**:
    - Implement a confirmation modal with "Confirm" and "Cancel" buttons.
    - Trigger the deletion via `DELETE /api/artigos/{id}` upon confirmation.
- **Verification**: Confirm that an article is removed only after clicking "Confirm" in the modal.

## Phase 3: Final Validation & Polish

### 3.1 Integration & Security Check
- **Action**:
    - Verify that all write operations require a valid JWT token.
    - Ensure that the frontend handles 401 errors gracefully (e.g., redirecting to login).
- **Verification**: Manually test the full flow from login to dashboard, then edit and delete.

### 3.2 UI/UX Polish
- **Action**: Refine Tailwind styles for the dashboard and modal. Ensure responsiveness.
- **Verification**: Check the UI on different screen sizes.
