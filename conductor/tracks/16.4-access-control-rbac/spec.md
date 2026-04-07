# Track 16.4: Access Control & RBAC - Specification

## 1. Overview
Implement a Role-Based Access Control (RBAC) system to distinguish between 'Admin' and 'Reader' roles. This ensures that only authorized users can manage content, while readers can interact with comments they own.

## 2. User Roles
- **Admin**: Full access to the system. Can create, update, and delete articles, categories, tags, and all comments.
- **Reader**: Can view articles and comments. Can create comments and update/delete ONLY their own comments.

## 3. Functional Requirements

### 3.1 Backend (ASP.NET Core)
- **JWT Claims**: The `TokenService` must include the `role` claim in the JWT.
- **Global Article Protection**: All write operations (Create, Update, Delete) for Articles must be restricted to the 'Admin' role using `[Authorize(Roles = "Admin")]`.
- **Comment Ownership**:
  - `Reader` can create comments.
  - `Reader` can only update or delete comments where they are the owner.
  - `Admin` can update or delete any comment.
- **Current User Context**: Implement a way to retrieve the current user's ID in the Application layer (e.g., `ICurrentUserService`).
- **Persistence**: Update the `Comment` entity to store the `UserId` of the author.
- **Secure Identity Seeding**:
  - Refactor `DbInitializer` to remove hardcoded credentials.
  - Use `IConfiguration` for initial Admin credentials (`AdminSetup:Email`, `AdminSetup:Password`).
  - Seed 'Admin' and 'Reader' roles if missing.
  - Promote the configured Admin email user to the 'Admin' role.
  - Use `ILogger` to report seeding progress.

### 3.2 Frontend (React)
- **Role Detection**: The `AuthContext` must decode the JWT to extract the user's role and store it in the state.
- **Conditional UI**:
  - Hide "New Post", "Edit", and "Delete" buttons for articles if the user is not an `Admin`.
  - Show "Edit" and "Delete" buttons for comments only if the user is the owner or an `Admin`.
- **Route Guarding**: Update `ProtectedRoute` to support role-based filtering, redirecting unauthorized users to a 403 (Forbidden) page or the home page.

## 4. Technical Requirements

### 4.1 Backend
- **Namespace**: `MeuSitePessoal.Api.Controllers`, `MeuSitePessoal.Application.Comments`
- **Security**: Use `System.Security.Claims` for role checks.
- **Database**: Add `UserId` column to `Comments` table.

### 4.2 Frontend
- **JWT Decoding**: Use `jwt-decode` library or a custom decoder to read claims.
- **Component**: Create a `PermissionGate` component for easy UI toggling.

## 5. Definition of Done
- Admins can perform all CRUD operations.
- Readers can only manage their own comments.
- Unauthorized API calls return `403 Forbidden`.
- UI accurately reflects user permissions.
- Unit and integration tests verify the RBAC logic.
