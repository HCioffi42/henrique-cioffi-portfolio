# Specification: Authentication & Protected Routes

**Track ID**: `04-auth-and-protected-routes`
**Status**: COMPLETED

## 1. Goal
Implement a complete authentication and authorization system using ASP.NET Core Identity for the backend and React Context API with Protected Routes for the frontend. This system secures administrative operations (POST, PUT, DELETE) and provides a user interface for login and session management.

## 2. Backend Requirements (ASP.NET Core)

### 2.1 Identity Configuration
- Update `BlogDbContext` to inherit from `Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext`.
- Register Identity services in `Program.cs` with the following configuration:
    - User: `IdentityUser`.
    - Role: `IdentityRole`.
    - Password policies: Minimalist (8 chars, 1 uppercase, 1 non-alphanumeric).
    - Database: PostgreSQL (`BlogDbContext`).

### 2.2 Token Service
- Implementation of `ITokenService` to generate JWT tokens based on `IdentityUser`.
- Include claims: `Sub` (NameIdentifier), `Email`, `Name`, and `Jti` (JWT ID).
- Support for configurable expiration via `appsettings.json` with a 60-minute fallback.

### 2.3 Auth Controller
- Endpoint `POST /api/auth/login` to validate credentials using `UserManager` and `SignInManager`.
- Returns a JWT token and user metadata upon successful authentication.
- Returns `401 Unauthorized` for invalid credentials.

### 2.4 Authorization Policies
- Secure `ArtigosController` using the `[Authorize]` attribute on all write operations (POST, PUT, DELETE).
- Read operations (GET) remain accessible to anonymous users.

## 3. Frontend Requirements (React + TypeScript)

### 3.1 Auth Context & Storage Utility
- Implementation of `src/context/AuthContext.tsx` to manage global authentication state.
- Use of `src/util/storage.ts` to abstract `localStorage` access, ensuring clean code and easier testing.
- Management of `user`, `token`, and `isInitialized` states to handle page refreshes without UI flickering.

### 3.2 Protected Route & API Interceptors
- Implementation of `src/components/ProtectedRoute.tsx` to guard administrative routes.
- **Axios Interceptors** in `src/services/api.ts`:
    - **Request**: Automatically inject the Bearer Token into the `Authorization` header.
    - **Response**: Reactive security handling to clear session and redirect on `401 Unauthorized` errors.

### 3.3 UI Components
- **Login Page**: Type-safe form with loading states and error handling.
- **Layout Integration**: Dynamic navigation bar that shows "Login" or "New Post/Logout" based on the authentication status.

## 4. Engineering Standards
- **Clean Code & SOLID**: Single responsibility for services and controllers.
- **Documentation**: 
    - XML Documentation for C# classes and methods in English.
    - JSDoc for TypeScript components and hooks in English.
- **Testing**: 
    - Integration tests with parallelization disabled to prevent database race conditions.
    - Unit tests for token generation and claim validation.

## 5. Verification Strategy
- **Automated**: Full test suite passing with 65+ tests covering identity and CRUD logic.
- **Manual**: 
    - Verification of the login/logout flow.
    - Confirmation that unauthorized attempts to write routes are redirected to the login page.