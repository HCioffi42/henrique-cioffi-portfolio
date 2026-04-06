# Identity Expansion & Modular Auth Implementation Plan (Task 16.3)

## Step 1: Backend Infrastructure & Configuration
1. **Roles Setup:** Modify `MeuSitePessoal.Infrastructure/Configuration/DbInitializer.cs` to explicitly seed the `"Reader"` role alongside the Admin role.
2. **Feature Toggle Service:** 
    * Add `"FeatureToggles": { "Enable2FA": false }` to `appsettings.json`.
    * Create `IFeatureToggleService` interface.
    * Implement `FeatureToggleService` that reads `IConfiguration` to verify the state of `Enable2FA`.
3. **OAuth Configuration:** Update `Program.cs` to add placeholder mechanisms for `.AddGoogle()` and `.AddGitHub()`, utilizing secrets from development app settings.

## Step 2: Backend Application Layer (MediatR)
1. **Register User:**
    * Create `RegisterCommand` and `RegisterCommandHandler`.
    * The handler will use `UserManager` to create the user and immediately `AddToRoleAsync(user, "Reader")`.
2. **Refactor Login Logic:**
    * Update `LoginCommand` to return a `LoginResponse` DTO (`Token`, `RequiresTwoFactor`).
    * Inject `IFeatureToggleService` into `LoginCommandHandler`. 
    * Execute password verification. If successful:
        * Wrap 2FA verification in: `if (_featureToggle.Is2FAEnabled()) { return new LoginResponse { RequiresTwoFactor = true }; }`.
        * Else, return the generated JWT token instantly.
3. **2FA Verification:**
    * Create `VerifyTwoFactorCommand` and `VerifyTwoFactorCommandHandler`.
    * This uses `UserManager.VerifyTwoFactorTokenAsync(...)`. If successful, generates and returns the JWT.
    
## Step 3: Backend External Authentication (API Controllers)
1. **AuthController Extensions:**
    * Expose standard `POST /api/auth/register`.
    * Add `GET /api/auth/external-login` which builds `AuthenticationProperties` with a redirect URI and calls `Challenge(properties, provider)`.
    * Add `GET /api/auth/external-callback` to capture the External Login Info, auto-provision an `IdentityUser` under the `Reader` role if they're new, and issue a JWT token.

## Step 4: Frontend Development (React + Tailwind v4)
1. **Update AuthContext:** Adapt the login flow inside `AuthContext.tsx` to handle intermediate `requiresTwoFactor` states without crashing or prematurely flagging the user as logged in.
2. **Create Register Page:** `meupessoal-web/src/pages/Register.tsx`.
    * Follow the minimalist aesthetic. Re-use components (buttons/inputs) where applicable.
3. **Refactor Login Page:**
    * Adapt `Login.tsx` to accept a new TOTP view.
    * Add "Login with Google" and "Login with GitHub" stylized buttons below the local login form. Clicking redirects the document directly to the backend `external-login` route.
4. **OAuth Callback Interception:** Create a small `OAuthCallback.tsx` page to securely capture the returned JWT and log the user in locally via `AuthContext`.

## Step 5: Verification & Testing
1. **Registration:** Verify that creating a user successfully writes row data and assigns `Reader` role.
2. **Toggle Evaluation (Local Login):** 
    * Test with `Enable2FA = false` (Immediate login).
    * Test with `Enable2FA = true` (Proceeds to TOTP required flag). *Note: generating the user's initial secret is out of scope for the login flow, but an Admin setup will be necessary to functionally test a valid OTP.*
3. **External Flows:** Try OIDC flows locally (requires provisioning GitHub/Google dev apps or mocking the endpoints).
