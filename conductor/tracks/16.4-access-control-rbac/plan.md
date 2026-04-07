# Track 16.4: Access Control & RBAC - Implementation Plan

## 1. Research & Analysis
- [x] Review current `TokenService.cs` implementation.
- [x] Review existing `AuthContext` and `ProtectedRoute` in frontend.
- [x] Review MediatR Handlers for Article operations.
- [x] Review `DbInitializer.cs` and `Program.cs` for seeding logic.

## 2. Backend Implementation (C# / .NET)

### 2.1 Update Domain & Infrastructure
- [x] Add `UserId` property to `Comment` entity in `MeuSitePessoal.Domain/Entities/Comment.cs`.
- [x] Update `BlogDbContext` mapping for `Comment` to include `UserId`.
- [x] Create a migration to add `UserId` column to `Comments` table.

### 2.2 Secure Identity Seeding & Roles
- [x] Update `appsettings.json` with `AdminSetup` section (placeholder for environment variables).
- [x] Refactor `DbInitializer.cs`:
    - [x] Change `SeedAsync` to be non-static or accept `ILogger`.
    - [x] Inject/Pass `IConfiguration` and `ILogger<DbInitializer>`.
    - [x] Implement role check/creation for 'Admin' and 'Reader'.
    - [x] Implement secure admin creation/promotion using `AdminSetup:Email` and `AdminSetup:Password`.
    - [x] Add detailed logging for each seeding step.
- [x] Update `Program.cs` to pass the necessary dependencies to `DbInitializer`.

### 2.3 Current User Context
- [x] Implement `ICurrentUserService` in `MeuSitePessoal.Application/Common/Interfaces`.
- [x] Implement `CurrentUserService` in `MeuSitePessoal.Infrastructure/Services` using `IHttpContextAccessor`.
- [x] Register `ICurrentUserService` and `IHttpContextAccessor` in `DependencyInjection.cs`.

### 2.4 Command & Handler Updates
- [x] Update `CreateCommentCommand` to include `UserId` (automatically from `ICurrentUserService`).
- [x] Update `CreateCommentCommandHandler` to set `UserId`.
- [x] Implement `UpdateComment` and `DeleteComment` commands with ownership checks.
- [x] Use `ICurrentUserService` in Handlers to verify permissions.

### 2.5 API Controllers Protection
- [x] Update `ArticlesController` to use `[Authorize(Roles = "Admin")]` for POST, PUT, DELETE.
- [x] Update `CommentsController` to use `[Authorize]` for POST, PUT, DELETE.
- [x] Expose new Comment endpoints (PUT, DELETE).

### 2.6 Testing
- [x] Add unit tests for `LoginCommandHandler` to verify role inclusion in the token.
- [x] Add integration tests in `AuthIntegrationTests` for 403 scenarios.
- [x] Add integration tests for Comment ownership validation.
- [x] Add integration test to verify admin seeding from configuration.

## 3. Frontend Implementation (TypeScript / React)

### 3.1 Auth State Enhancement
- [x] Update `User` model to include `role`.
- [x] Install `jwt-decode` if not present, or implement a basic JWT decoder.
- [x] Update `AuthProvider` to decode the JWT on login and store the role.

### 3.2 Conditional Rendering
- [x] Create `PermissionGate` component in `meupessoal-web/src/components/PermissionGate.tsx`.
- [x] Wrap "New Post", "Edit", and "Delete" buttons in `PermissionGate`.
- [x] Implement ownership logic for comment actions.

### 3.3 Route Guarding
- [x] Update `ProtectedRoute` to accept an `requiredRole` prop.
- [x] Update `App.tsx` routes to use `requiredRole="Admin"` for administrative paths.

### 3.4 API Integration
- [x] Update `commentService.ts` to include `updateComment` and `deleteComment`.

## 4. Final Validation
- [x] Verify that a `Reader` user cannot call Admin endpoints using tools like Postman/curl.
- [x] Verify that the UI correctly hides buttons based on roles.
- [x] Run all tests (unit and integration).
