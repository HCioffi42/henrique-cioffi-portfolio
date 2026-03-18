# Specification: Authentication & Protected Routes

**Track ID**: `04-auth-and-protected-routes`
**Status**: IN_PROGRESS

## 1. Goal
Implement a complete authentication and authorization system using ASP.NET Core Identity for the backend and React Context API with Protected Routes for the frontend. This system will secure administrative operations (POST, PUT, DELETE) and provide a user interface for login.

## 2. Backend Requirements (ASP.NET Core)

### 2.1 Identity Configuration
- Update `BlogDbContext` to inherit from `Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext`.
- Register Identity services in `Program.cs` with the following configuration:
    - User: `IdentityUser`.
    - Role: `IdentityRole`.
    - Password policies: Minimalist (8 chars, 1 uppercase, 1 non-alphanumeric).
    - Database: Use existing PostgreSQL (`BlogDbContext`).

### 2.2 Token Service
- Update `ITokenService` in the `Domain` or `Application` layer.
- Update `TokenService` in the `Infrastructure` layer to generate JWT tokens based on `IdentityUser`.
- Include claims: `Name`, `Email`, `Role`, and `Jti` (JWT ID).

### 2.3 Auth Controller
- Update `AuthController` in the `Api` project.
- Implement a `POST /api/auth/login` endpoint that:
    - Validates user credentials using `UserManager` and `SignInManager`.
    - Returns a JWT token if successful.
    - Returns `401 Unauthorized` for invalid credentials.

### 2.4 Authorization Policies
- Ensure `ArtigosController` uses the `[Authorize]` attribute on all write operations (POST, PUT, DELETE).
- Read operations (GET) must remain accessible to anonymous users.

## 3. Frontend Requirements (React + TypeScript)

### 3.1 Auth Context & Storage Utility
- Create `src/context/AuthContext.tsx`.
- Implement `src/util/storage.ts` to abstract `localStorage` access (Clean Code).
- Manage `user`, `token`, and `isInitialized` states.
- Persist the JWT token in `localStorage`.

### 3.2 Protected Route & Interceptors
- Create `src/components/ProtectedRoute.tsx`.
- Implement **Axios Interceptors** in `src/services/api.ts`:
    - **Request**: Inject Bearer Token automatically.
    - **Response**: Handle `401 Unauthorized` by clearing session and redirecting (Reactive Security).
- Configure environment variables using `.env` for API base URL.- Wrap routes that require authentication.
- Redirect unauthenticated users to the `/login` page using `Navigate` from `react-router-dom`.

### 3.3 Login Page
- Create `src/pages/Login.tsx`.
- Form with fields: `Username`, `Password`, and a `Submit` button.
- Style using Tailwind CSS (minimalist design, consistent with the rest of the site).
- Integrate with `AuthContext` to update user state upon successful login.

## 4. Engineering Standards
- **Clean Code & SOLID**: Ensure single responsibility for services and controllers.
- **Documentation**: 
    - XML Documentation for C# classes and methods in English (e.g., `<summary>Authenticates the user and returns a JWT.</summary>`).
    - JSDoc for TypeScript components and hooks in English (e.g., `/** @returns {JSX.Element} The login form component. */`).
- **Typing**: Strict TypeScript interfaces/types for all API responses and context state.
- **Security**: Never expose sensitive information in logs or API responses (except the token).

## 5. Verification Strategy
- **Backend**:
    - Functional test: Verify that unauthorized POST/PUT/DELETE requests return `401 Unauthorized`.
    - Functional test: Verify that a valid login returns a valid JWT.
- **Frontend**:
    - UI test: Attempting to access `/admin` or write routes without being logged in should redirect to `/login`.
    - UI test: Successful login should redirect to the home or admin dashboard.
