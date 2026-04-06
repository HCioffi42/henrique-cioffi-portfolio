# Track 16.3 — Identity Expansion & Modular Auth

**Status:** Completed  
**Date:** 2026-04-06

## Summary

This track expands the authentication system from Admin-only to a multi-role, multi-provider architecture. The implementation follows Clean Architecture and MediatR patterns throughout.

## What Was Built

### Backend

| Component | Description |
|---|---|
| `IFeatureToggleService` | Domain interface decoupling config from Application layer |
| `FeatureToggleService` | Reads `FeatureToggles:Enable2FA` from `appsettings.json` |
| `DbInitializer` | Now seeds both `Admin` and `Reader` roles idempotently |
| `RegisterCommand` + Handler | Creates `IdentityUser` and assigns `Reader` role |
| `RegisterCommandValidator` | FluentValidation for email format and password length |
| `LoginCommand` + Handler | Validates credentials; forks on 2FA toggle: token or `RequiresTwoFactor` flag |
| `VerifyTwoFactorCommand` + Handler | Validates TOTP via `UserManager`; issues JWT on success |
| `AuthController` | Full rewrite: `/register`, `/login`, `/verify-2fa`, `/external-login`, `/external-callback` |
| `Program.cs` | Registers `IFeatureToggleService`, conditionally adds Google/GitHub OAuth if secrets present |

### Frontend

| Component | Description |
|---|---|
| `Auth.ts` | Extended with `RegisterRequest`, `VerifyTwoFactorRequest`, updated `LoginResponse` |
| `authService.ts` | Added `register`, `verifyTwoFactor`, `initiateExternalLogin` |
| `Login.tsx` | State-machine view: `credentials → twoFactor`; GitHub/Google OAuth buttons |
| `Register.tsx` | New public page with inline error display and Reader role notice |
| `OAuthCallback.tsx` | Reads `?token=&username=` from backend redirect; completes local session |
| `App.tsx` | Added `/register` and `/oauth/callback` routes |

## Architectural Highlight: 2FA Feature Toggle

```
LoginCommandHandler
    ├── CheckPasswordSignInAsync ✓
    └── _featureToggle.Is2FAEnabled()
            ├── false → GenerateToken() → return { token, username }
            └── true  → return { RequiresTwoFactor: true }
                            ↓
                      Frontend shows TOTP input
                            ↓
                      POST /verify-2fa → VerifyTwoFactorCommandHandler
                            ↓
                      VerifyTwoFactorTokenAsync ✓ → GenerateToken()
```

## NuGet Packages Added

- `Microsoft.AspNetCore.Authentication.Google` v8.0.*
- `AspNet.Security.OAuth.GitHub` v8.0.*

## Technical Debts / Observations

- **OAuth credentials** must be added to `appsettings.Development.json` locally (or Docker secrets in production). The system gracefully skips provider registration if credentials are empty.
- **2FA Setup Flow** (QR code generation for users enabling 2FA) is out of scope for this track. Admin-side setup is required before `Enable2FA: true` is usable in production.
- **TOTP Handling** — `VerifyTwoFactorTokenAsync` relies on the user having a registered authenticator key via `GetAuthenticatorKeyAsync`. A dedicated "Setup 2FA" admin endpoint was not built in this track.
