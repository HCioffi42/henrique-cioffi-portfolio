# Specification: Implement CreateArtigo CQRS Command

## Overview
Implement a `CreateArtigoCommand` and its corresponding `CreateArtigoHandler` to handle the creation of new blog articles in the `MeuSitePessoal` project. This will follow the CQRS pattern using the MediatR library.

## Functional Requirements
- **Create Command:** A `CreateArtigoCommand` in the `Application` layer.
- **Handler Implementation:** A `CreateArtigoHandler` to process the command and persist a new `Artigo` to the database using the `IArtigoRepository`.
- **Fields:** The command must include:
  - `Title` (string)
  - `Content` (string)
  - `Summary` (string)
  - `Tags` (List of strings)
- **Response:** The handler should return the ID of the created article.

## Non-Functional Requirements
- **Clean Architecture:** Ensure the command and handler are placed in the `MeuSitePessoal.Application` layer.
- **Maintainability:** Follow the CQRS pattern for separation of reads and writes.
- **Testing:** Include unit tests for the command and handler in `MeuSitePessoal.Tests`.

## Acceptance Criteria
- [ ] `CreateArtigoCommand` is defined in the `Application` layer.
- [ ] `CreateArtigoHandler` correctly saves a new `Artigo` through the repository.
- [ ] Unit tests verify that the handler processes the command correctly.
- [ ] Integration with MediatR is verified.

## Out of Scope
- Frontend implementation for the creation form.
- Advanced validation (e.g., FluentValidation) unless explicitly requested later.
- Authentication/Authorization for creating articles (at this stage).
