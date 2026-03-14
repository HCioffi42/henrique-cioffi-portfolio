# Track: Implement JWT Security for Write Operations

**Track ID**: `jwt_security_20260314`
**Goal**: Protect administrative operations (POST, PUT, DELETE) using JWT Bearer authentication.

## Implementation Plan

### 1. Setup
- Install `Microsoft.AspNetCore.Authentication.JwtBearer` in the API project.
- Add `JwtSettings` to `appsettings.json`.

### 2. Infrastructure: Token Service
- Create `ITokenService` in the Application layer.
- Implement `TokenService` in the Infrastructure layer to generate signed JWT tokens.

### 3. Configuration: Program.cs
- Configure JWT Authentication middleware.
- Configure Authorization policies.
- Update Swagger definition to support Bearer tokens (Security Definition & Requirement).

### 4. Controller Implementation
- Create `AuthController` with a `Login` endpoint.
- Protect `ArtigosController` using `[Authorize]` for write actions and `[AllowAnonymous]` for read actions.

### 5. Test Updates
- Update `ArtigoCrudTests.cs` to obtain a token and include it in the Authorization header for protected requests.

## Verification Strategy
- Manual test via Swagger UI.
- Automated tests in `ArtigoCrudTests.cs`.
