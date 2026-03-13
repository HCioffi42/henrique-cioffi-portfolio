# Implementation Plan: Implement CreateArtigo CQRS Command

## Phase 1: Preparation & Dependency Check
- [ ] Task: Ensure MediatR is installed in the `MeuSitePessoal.Application` and `MeuSitePessoal.Api` projects.
- [ ] Task: Review the `Artigo` domain entity and `IArtigoRepository` for compatibility with the new command.

## Phase 2: Application Layer Implementation
- [ ] Task: Create `CreateArtigoCommand` in the `MeuSitePessoal.Application` layer with the specified fields (Title, Content, Summary, Tags).
- [ ] Task: Create `CreateArtigoHandler` in the `MeuSitePessoal.Application` layer to handle the command.
    - [ ] Inject `IArtigoRepository` into the handler.
    - [ ] Implement the `Handle` method to create a new `Artigo` and save it.

## Phase 3: API Layer Integration
- [ ] Task: Register MediatR in the `Program.cs` file (if not already registered).
- [ ] Task: Update the `ArtigosController` to include a `POST` endpoint that sends the `CreateArtigoCommand` via MediatR.

## Phase 4: Testing & Validation
- [ ] Task: Write unit tests for the `CreateArtigoHandler` in `MeuSitePessoal.Tests`.
- [ ] Task: Verify that a new article is correctly created in the database through the API endpoint.
- [ ] Task: Ensure all tests pass and follow the Clean Architecture principles.
