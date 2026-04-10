# Track 16.5: Email Infrastructure Planning - Implementation Plan

This track focuses on establishing the core email infrastructure for the platform, enabling future features such as newsletter notifications and contact form responses.

## Phase 1: Core SMTP Infrastructure [COMPLETED]

The core abstraction and implementation for email delivery using MailKit have been established and verified via unit tests.

### 1. Research & Analysis
- [x] Research MailKit and MimeKit best practices for .NET 8.
- [x] Evaluate SMTP provider configuration (Mailtrap).
- [x] Review integration points.

### 2. Application Layer (Abstraction)
- [x] Define the `IEmailSender` interface.

### 3. Infrastructure Layer (Implementation)
- [x] Add NuGet packages (`MailKit`, `MimeKit`).
- [x] Create `EmailSettings` configuration model.
- [x] Implement `MailKitEmailService`.
- [x] Register services in `DependencyInjection.cs`.

### 4. Configuration
- [x] Add dummy `EmailSettings` to `appsettings.json`.
- [x] Add test settings for local development.

### 5. Testing & Validation
- [x] Create automated unit tests for `MailKitEmailService`.
- [x] Verify message construction and settings mapping.

---

## Phase 2: Account Lifecycle Verification [COMPLETED]

Integrated `IEmailSender` with ASP.NET Core Identity to enforce email confirmation for new accounts.

### 1. Identity Configuration
- [x] Update `Program.cs` to set `RequireConfirmedEmail = true`.
- [x] Configure `ClientSettings:BaseUrl` in `appsettings.json` and its development override.

### 2. Registration Flow
- [x] Update `RegisterCommandHandler` to:
    - [x] Inject `IEmailSender` and `IConfiguration`.
    - [x] Remove manual `EmailConfirmed = true`.
    - [x] Generate and encode confirmation token (`Base64Url`).
    - [x] Send welcome email with verification link.

### 3. Verification Logic
- [x] Create `ConfirmEmailCommand` and `ConfirmEmailCommandHandler`.
- [x] Integrate `ConfirmEmail` endpoint in `AuthController`.

### 4. Password Recovery Flow
- [x] Implement `ForgotPasswordCommand` and `ForgotPasswordCommandHandler`.
    - [x] Enforce `EmailConfirmed` check.
    - [x] Protect against account enumeration with generic responses.
- [x] Implement `ResetPasswordCommand` and `ResetPasswordCommandHandler`.
    - [x] Use `Base64Url` encoding/decoding for reset tokens.
- [x] Update `AuthController` with `forgot-password` and `reset-password` endpoints.


---

## 6. Track Registry & Finalization
- [x] Create automated integration tests for SMTP delivery.
- [ ] Create `conductor/tracks/16.5-email-infrastructure/index.md` upon completion.
- [ ] Update root `conductor/tracks.md` with the new track status.
- [x] Generate `walkthrough.md` for user review.


