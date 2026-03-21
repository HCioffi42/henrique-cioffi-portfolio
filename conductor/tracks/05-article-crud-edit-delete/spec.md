# Specification: Article CRUD (Edit & Delete)

**Track ID**: `05-article-crud-edit-delete`
**Status**: COMPLETE

## 1. Goal
Implement complete CRUD operations for articles, specifically the `Update` and `Delete` features, both on the backend and frontend. This track includes creating an administrative dashboard with server-side pagination for article management and a deletion confirmation modal.

## 2. Backend Requirements (ASP.NET Core)

### 2.1 MediatR Commands & Handlers
- **UpdateArtigoCommand**: 
    - Include `Guid Id`, `string Title`, `string Content`, `string Summary`, and `List<string> Tags`.
    - Implement `UpdateArtigoCommandHandler`:
        - Fetch the article by ID.
        - Update fields using **AutoMapper** (`IMapper`).
        - Persist changes to the database.
- **DeleteArtigoCommand**:
    - Include `Guid Id`.
    - Implement `DeleteArtigoCommandHandler`:
        - Fetch the article by ID.
        - Delete from the repository.
        - Persist changes to the database.

### 2.2 ArtigosController
- Implement `PUT /api/artigos/{id}`:
    - Calls `UpdateArtigoCommand`.
    - Returns `204 No Content` on success or `404 Not Found` if the article doesn't exist.
    - **Security**: Must be protected with `[Authorize]`.
- Implement `DELETE /api/artigos/{id}`:
    - Calls `DeleteArtigoCommand`.
    - Returns `204 No Content` on success or `404 Not Found` if the article doesn't exist.
    - **Security**: Must be protected with `[Authorize]`.

### 2.3 AutoMapper Infrastructure
- Install `AutoMapper.Extensions.Microsoft.DependencyInjection`.
- Configure the mapping between `UpdateArtigoCommand` and `Artigo` entity.

## 3. Frontend Requirements (React + TypeScript)

### 3.1 Admin Dashboard
- Create `src/pages/Dashboard.tsx`.
- Display a table listing articles with **Server-Side Pagination**:
    - Send `pageNumber` and `pageSize` parameters in the request.
    - Display pagination controls (Next/Previous, current page).
- Columns:
    - Title
    - Date of Creation
    - Tags
    - Actions (Edit and Delete buttons).
- Style using Tailwind CSS.

### 3.2 Edit Article Page
- Create `src/pages/EditArtigo.tsx`.
- Reuse form logic from `CreateArtigo.tsx`.
- Fetch article data by ID on component mount and pre-fill the form.
- Submit changes to `PUT /api/artigos/{id}`.

### 3.3 Delete Confirmation Modal
- Create `src/components/DeleteModal.tsx`.
- A reusable modal to confirm article deletion.
- Options: "Confirm" and "Cancel".

## 4. Engineering Standards
- **Clean Code & SOLID**: Adhere to the established architecture.
- **AutoMapper**: Use `IMapper` dependency injection to update entities safely.
- **Documentation**: Use English for all code comments (third person).
- **Security**: Ensure that only authenticated users can perform `Update` and `Delete` operations.

## 5. Verification Strategy
- **Backend**:
    - Functional test: Verify that unauthorized `PUT`/`DELETE` requests return `401 Unauthorized`.
    - Functional test: Verify successful update and deletion of an article.
- **Frontend**:
    - UI test: Verify that the dashboard displays paginated articles.
    - UI test: Verify that the edit form correctly loads existing data.
    - UI test: Verify that the delete modal correctly triggers the deletion process.
