# Track 09: Global Systems (Logging & Toasts) - Implementation Plan

## Phase 1: Backend Logging & Error Handling (9.1)

### Step 1.1: Review & Refine Logging Infrastructure
- **Files**: `MeuSitePessoal.Infrastructure.Logging/DependencyInjection.cs`
- **Actions**:
    - Verify that `AddSerilog` is correctly registered.
    - Ensure `ReadFrom.Configuration` is being used to allow future overrides from `appsettings.json`.
    - Ensure `Enrich.FromLogContext()` is present.
    - Check for `Serilog.Sinks.Async` and `Serilog.Sinks.File` NuGet packages.

### Step 1.2: Standardize Global Exception Handler
- **Files**: `MeuSitePessoal.Api/Middleware/GlobalExceptionHandler.cs`
- **Actions**:
    - Ensure all unhandled exceptions are caught and logged with full stack traces.
    - Use structured logging template: `_logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message)`.
    - Map `ValidationException` (from FluentValidation) to `ValidationProblemDetails`.
    - Map `KeyNotFoundException` to 404.
    - Map `UnauthorizedAccessException` to 401.
    - Return `ProblemDetails` object as JSON (RFC 7807).

### Step 1.3: Verification
- Run the API and trigger a 404, a 400 (validation), and a 500 (internal error) to verify the response format and logs.
- Check the `logs/` directory for rolling file creation.

## Phase 2: Global Notifications (9.2)

### Step 2.1: Frontend Dependencies & Configuration
- **Files**: `meupessoal-web/package.json`, `meupessoal-web/src/App.tsx`
- **Actions**:
    - Install `react-hot-toast`.
    - Add `<Toaster />` to `App.tsx`.
    - Position it at `top-right`.

### Step 2.2: Implement Notification Service
- **Files**: `meupessoal-web/src/services/notificationService.ts`
- **Actions**:
    - Create a wrapper service for `toast` to keep the UI components clean and provide a single point of configuration.
    - Methods: `success`, `error`, `loading`, `promise`, `dismiss`.

### Step 2.3: Refactor Components
- **Login Component**: `meupessoal-web/src/pages/Login.tsx`
    - Remove `error` state.
    - Use `notificationService.error()` for login failures.
    - Use `notificationService.success()` for login success.
- **Create Article Component**: `meupessoal-web/src/pages/CreateArticle.tsx`
    - Remove `error` state.
    - Use `notificationService.error()` for validation and save failures.
    - Use `notificationService.success()` for successful publication.
    - Use `notificationService.promise()` for image uploads (optional, but a good UX).
- **Edit Article Component**: `meupessoal-web/src/pages/EditArticle.tsx`
    - Remove `error` state.
    - Use `notificationService.error()` and `notificationService.success()`.
- **Dashboard Component**: `meupessoal-web/src/pages/Dashboard.tsx`
    - Refactor delete action to use a toast for success/error.

### Step 2.4: Verification
- Verify that toasts appear correctly for each scenario.
- Ensure that removing the local error state doesn't leave blank spaces or layout shifts in the components.
