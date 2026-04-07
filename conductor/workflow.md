# Workflow

This project uses the Conductor extension for structured development.

## 1. Planning
- **Track Initialization**: For every new feature or significant task, a dedicated subfolder MUST be created within the `conductor/tracks/` directory (e.g., `01-home-page-listing`).
- **Technical Spec (`spec.md`)**: Draft a formal specification including:
  - **API Contracts**: Request/Response models and status codes.
  - **Data Models**: TypeScript interfaces or C# DTOs.
  - **Architecture**: Design patterns to be used (CQRS, MediatR, etc.).
- **Implementation Plan (`plan.md`)**: After the Spec is approved, draft a step-by-step plan.
  - List specific files to be created or modified.
  - Detail logic flows (e.g., "The Handler uses a direct projection to skip the Content column").
  - Identify potential risks (e.g., circular dependencies).
- **Metadata Generation**: A `metadata.json` file must also be generated in the track folder containing `track_id`, `name`, `status`, and `related_files` to ensure project traceability along side with the `index.md` for that track.
- **Approval**: The detailed plan and spec must be presented in the chat for user validation before proceeding to the Implementation phase.

## 2. Implementation
- **Task Execution**: Follow the "Plan -> Act -> Validate" cycle for each step in the `plan.md`.
- **Standards**:
  - **Language**: English for all code and documentation.
  - **Commentary**: Third-person descriptive comments (e.g., "The component renders a grid of articles").
  - **No Drift**: Do not modify files outside the current track's scope.
- **Constraint Enforcement**: Adhere strictly to the rules defined in `product-guidelines.md` regarding architecture (Clean Architecture, MediatR, SOLID, etc.).
- **Concerning Interfaces**: All newly created interfaces should be in this namespace: `MeuSitePessoal.Domain.Interfaces`.

## 3. Testing & Validation
- Ensure all changes are covered by relevant tests (Unit and/or Integration).
- Run tests before completing a track.
- Perform final architectural and style checks.

## 4. Finalization
- **Style Check**: Ensure the code matches the approved `spec.md`.
- **Implementation Summary**: Upon completion, a comprehensive `index.md` must be created in the track subfolder, summarizing the final state of the implementation and any technical debts or observations.
- **Tracks Registry**: The root `conductor/tracks.md` file must be automatically updated to reflect the progress and "Completed" status of the task, as well as the `./docs/roadmap.md` regarding the track that is completed.
- **Commit Readiness**: Prepare a semantic commit message based on the changes documented in the track but do not try to commit. This should be a suggestion only.
