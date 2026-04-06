# Identity Expansion & Modular Auth Specification (Task 16.3)

## 1. Overview
The objective is to expand the current authentication system, which currently only supports an Admin role, to allow public user registration under a "Reader" role. Furthermore, it introduces external OAuth2 authentication (Google/GitHub) and a modular 2FA (Two-Factor Authentication) workflow controlled by a feature toggle.

## 2. API Contracts
### 2.1 `POST /api/auth/register`
Creates a new `IdentityUser` and assigns them the `Reader` role.

**Request (`RegisterRequest`):**
```json
{
  "email": "user@example.com",
  "password": "StrongPassword123!"
}
```

**Response:**
*   **200 OK:** Registration successful.
*   **400 Bad Request:** Validation failed (password requirements, existing email).

### 2.2 `POST /api/auth/login` (Updated)
Updated to support the 2FA workflow if the feature is enabled via `appsettings.json`.

**Response (`LoginResponse`):**
```json
{
  "token": "jwt_token_string_here_or_null",
  "requiresTwoFactor": true
}
```

### 2.3 `POST /api/auth/verify-2fa`
Verifies the TOTP code if 2FA was required during login.

**Request (`VerifyTwoFactorRequest`):**
```json
{
  "email": "user@example.com",
  "code": "123456"
}
```

**Response:**
*   **200 OK:** Returns the JWT token.
*   **401 Unauthorized:** Invalid code.

### 2.4 External Authentication (OAuth2/OIDC)
*   **`GET /api/auth/external-login?provider={provider}`**: Initiates the OAuth challenge and redirects the user to the provider (Google or GitHub).
*   **`GET /api/auth/external-callback`**: Handles the provider's callback. If the user does not exist, an `IdentityUser` is created and assigned the `Reader` role. It returns a final Redirect to the frontend with the JWT authorization (e.g., via a short-lived secure cookie or URL fragment depending on frontend route strategy).

## 3. Data Models
### 3.1 C# CQRS Models
*   `RegisterCommand(string Email, string Password)`
*   `LoginCommand(string Email, string Password)` -> Returns `LoginResponse`
*   `VerifyTwoFactorCommand(string Email, string Code)`

### 3.2 TypeScript Interfaces
```typescript
export interface LoginResponse {
    token: string | null;
    requiresTwoFactor: boolean;
}

export interface VerifyTwoFactorRequest {
    email: string;
    code: string;
}
```

## 4. Architecture Design & Patterns

### 4.1 Backend (Clean Architecture)
*   **Identity Provisioning:** The system uses standard ASP.NET Core `IdentityUser`. The `DbInitializer` will insert a default `Reader` role into the database if it doesn't already exist.
*   **Modular 2FA Toggle:** We will implement an `IFeatureToggleService` in the Infrastructure Layer that reads the `Enable2FA` flag from `appsettings.json`. 
    *   The `LoginCommandHandler` will inject `IFeatureToggleService`. 
    *   If `_featureToggleService.Is2FAEnabled()` is *false*, the handler immediately builds and returns the JWT token after password validation.
    *   If *true*, it skips JWT generation and returns `RequiresTwoFactor = true`.
*   **OAuth Integration:** Configured via `AddAuthentication().AddGoogle().AddGitHub()` in `Program.cs`. We will ensure `signInManager.ExternalLoginSignInAsync` handles role assignment effectively upon a successful new external registration.

### 4.2 Frontend (meupessoal-web)
*   **Registration Form:** A clean, minimalistic `/register` page using Tailwind CSS v4, matching the existing visual identity defined in `product-guidelines.md`.
*   **2FA Conditional Rendering:** The Login component will store an intermediate state. If `loginResponse.requiresTwoFactor` is true, the UI swaps out the password form for a 6-digit TOTP input form using the same centered modular layout.
*   **OAuth Callbacks:** The frontend will have a designated `/oauth/callback` parsing route if tokens are delivered via URL, or rely on standard cross-origin token delivery.
