# Track: Complete Artigo CRUD

**Track ID**: `complete_artigo_crud_20260314`
**Goal**: Implement `UpdateArtigo` and `DeleteArtigo` features using MediatR and FluentValidation, ensuring full CRUD capabilities for the Artigo resource.

## Implementation Plan

### 1. Infrastructure Layer: Global Exception Handling
*   **Action**: Update `GlobalExceptionHandler.cs` to handle `KeyNotFoundException`.
*   **Requirement**: Map `KeyNotFoundException` to `404 Not Found`. Log as `Information` (not `Error`).

### 2. Application Layer: Update Artigo
*   **Command**: `UpdateArtigoCommand.cs` in `Application/Artigos/Commands/UpdateArtigo`.
    *   Properties: `Guid Id`, `string Titulo`, `string Conteudo`, `string Resumo`, `List<string> Tags`.
*   **Validator**: `UpdateArtigoCommandValidator.cs`.
    *   Rules: `Id` (Required), `Titulo` (Required, max 100), `Conteudo` (Required), `Resumo` (Required, max 500).
*   **Handler**: `UpdateArtigoCommandHandler.cs`.
    *   Logic:
        1. Fetch existing article via `_repository.ObterPorIdAsync(command.Id)`.
        2. Throw `KeyNotFoundException` if not found.
        3. Manual mapping (No AutoMapper). **Mandatory**: Do not modify entity `Id`.
        4. Call `_repository.AtualizarAsync(entity)`.

### 3. Application Layer: Delete Artigo
*   **Command**: `DeleteArtigoCommand.cs` in `Application/Artigos/Commands/DeleteArtigo`.
*   **Handler**: `DeleteArtigoCommandHandler.cs`.
    *   Logic:
        1. Fetch existing article via `_repository.ObterPorIdAsync(command.Id)`.
        2. Throw `KeyNotFoundException` if not found.
        3. Call `_repository.ExcluirAsync(command.Id)`.

### 4. API Layer: ArtigosController
*   **Action**: Refactor `PUT` and `DELETE` endpoints to use MediatR.
*   **Cleanup**: Remove `IArtigoRepository` dependency from the controller.

## Verification Strategy
*   Integration tests for successful update/delete.
*   Validation tests for update constraints.
*   Error handling tests for `404` and `400` responses.
