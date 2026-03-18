# Implementation Plan: Authentication & Protected Routes

**Track ID**: `04-auth-and-protected-routes`

This plan outlines the steps to implement a complete authentication system.

## Phase 1: Backend Infrastructure (Identity & JWT)

### 1.1 Update Database Context
- **Task**: Modify `BlogDbContext.cs`.
- **Action**: Inherit from `IdentityDbContext` and configure Identity tables.
- **Verification**: Run `dotnet ef migrations add AddIdentity` and `dotnet ef database update`.

### 1.2 Configure Identity in Program.cs
- **Task**: Register Identity services.
- **Action**: Add `builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<BlogDbContext>().AddDefaultTokenProviders();` and configure options.
- **Verification**: Build the project successfully.

### 1.3 Update Token Service
- **Task**: Update `ITokenService` and `TokenService`.
- **Action**: Modify `GenerateToken` to accept `IdentityUser` and include necessary claims.
- **Verification**: Unit test the token generation with mock users.

### 1.4 Update Auth Controller
- **Task**: Implement `Login` using `UserManager` and `SignInManager`.
- **Action**: Replace hardcoded logic with Identity service calls.
- **Verification**: Test the `/api/auth/login` endpoint with a registered user.

### 1.5 Secure Artigos Controller
- **Task**: Add `[Authorize]` attribute.
- **Action**: Apply to `Create`, `Update`, and `Delete` actions.
- **Verification**: Attempt unauthorized write requests and confirm `401` response.

## Phase 2: Frontend Implementation (React & Context)

### 2.1 Create Auth Context
- **Task**: Implement `src/context/AuthContext.tsx`.
- **Action**: Define `AuthContext`, `AuthProvider`, and `useAuth` hook. Implement token storage in `localStorage`.
- **Verification**: Verify that the context state is accessible across the app.

### 2.2 Implement Protected Route
- **Task**: Create `src/components/ProtectedRoute.tsx`.
- **Action**: Check `isAuthenticated` and redirect to `/login` if false.
- **Verification**: Manually test redirection by navigating to a protected URL.

### 2.3 Create Login Page
- **Task**: Implement `src/pages/Login.tsx`.
- **Action**: Create the UI form and integrate it with the `login` function from `AuthContext`.
- **Verification**: Perform a successful login and verify redirection to the dashboard/home.

### 2.4 Update Routing
- **Task**: Modify `App.tsx`.
- **Action**: Wrap the relevant routes with `ProtectedRoute`.
- **Verification**: Ensure that only authenticated users can access admin features.

## Phase 3: Final Validation & Cleanup

### 3.1 Integration Testing
- **Action**: Run the full stack and verify the end-to-end authentication flow.
- **Checklist**:
    - [x] Login works with valid credentials.
    - [x] Login fails with invalid credentials.
    - [x] Protected routes redirect unauthenticated users.
    - [x] Token is stored and sent in API requests (if applicable for write operations).

### 3.2 Documentation & Polish
- **Action**: Ensure all new methods have XML/JSDoc documentation. Refine the UI styles.
