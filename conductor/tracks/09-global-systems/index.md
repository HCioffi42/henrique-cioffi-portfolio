# Track 09: Global Systems (Logging & Toasts) - Summary

## Implementation Overview
Standardized backend logging and global exception handling, and implemented a unified notification system in the frontend.

## Key Changes

### Backend (9.1)
- **Infrastructure**: Refined Serilog configuration in `MeuSitePessoal.Infrastructure.Logging`. Sinks include Console and daily rolling File logs.
- **Global Exception Handler**: Updated `GlobalExceptionHandler.cs` to map `UnauthorizedAccessException` to 401. Ensured all unhandled exceptions are logged with structured templates and return RFC 7807 Problem Details.

### Frontend (9.2)
- **Dependencies**: Installed `react-hot-toast`.
- **Global Provider**: Added `<Toaster />` to `App.tsx` for application-wide notifications.
- **Notification Service**: Created `notificationService.ts` as a clean wrapper for toast calls (success, error, promise).
- **Refactored Components**:
  - `Login.tsx`: Removed local error state, added success and error toasts.
  - `CreateArticle.tsx`: Replaced state-based errors with toasts. Used `toast.promise` for creation and image uploads.
  - `EditArticle.tsx`: Replaced state-based errors with toasts. Used `toast.promise` for updates and image uploads.
  - `Dashboard.tsx`: Integrated toasts for delete actions and loading errors. Fixed inconsistent route link for "New Article".

## Verification Results
- Backend logs are correctly written to the `logs/` directory.
- Frontend notifications provide immediate and clear feedback for all asynchronous operations.
- API error responses follow the Problem Details standard.

## Technical Debt / Next Steps
- Consider adding a `RequestLoggingMiddleware` to log all incoming requests and their processing time (partially handled by `UseSerilogRequestLogging`).
- Expand `notificationService` if custom toast styling is needed in the future.
