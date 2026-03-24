# Track 09: Global Systems (Logging & Toasts)

## Overview
This track focuses on enhancing the system's observability and user feedback mechanisms. It includes standardizing backend logging with Serilog and implementing a global notification system in the frontend using `react-hot-toast`.

## 1. Backend Logging (9.1)

### 1.1 Infrastructure
- **Project**: `MeuSitePessoal.Infrastructure.Logging`
- **Library**: Serilog
- **Sinks**:
  - **Console**: For real-time monitoring during development.
  - **File**: Daily rolling files in `logs/log-.txt`.
- **Enrichment**: `FromLogContext` to include correlation IDs and other context.

### 1.2 Global Exception Middleware
- **Location**: `MeuSitePessoal.Api/Middleware/GlobalExceptionHandler.cs`
- **Behavior**:
  - Catch all unhandled exceptions.
  - Log exceptions using structured logging (e.g., `_logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message)`).
  - Return standardized **Problem Details (RFC 7807)**.
  - Map specific exceptions to HTTP status codes:
    - `ValidationException` -> 400 Bad Request (with validation errors).
    - `KeyNotFoundException` -> 404 Not Found.
    - `UnauthorizedAccessException` -> 401 Unauthorized.
    - Default -> 500 Internal Server Error.

### 1.3 Structured Logging Standards
- Avoid string interpolation in log templates.
- **Good**: `_logger.LogInformation("Article {ArticleId} created by {User}", id, user)`
- **Bad**: `_logger.LogInformation($"Article {id} created by {user}")`

## 2. Global Notifications (9.2)

### 2.1 Frontend Library
- **Library**: `react-hot-toast`
- **Installation**: `npm install react-hot-toast`

### 2.2 Configuration
- Add `<Toaster />` component to the root of `App.tsx` (inside `AuthProvider`).
- Configure default options (position: 'top-right', duration: 4000ms).

### 2.3 Notification Service
- Create `meupessoal-web/src/services/notificationService.ts`.
- **API**:
  - `success(message: string)`
  - `error(message: string)`
  - `promise(promise: Promise<T>, messages: { loading: string, success: string, error: string })`

### 2.4 Component Refactoring
- Remove local `error` and `success` states used for notifications in:
  - `Login.tsx`
  - `CreateArticle.tsx`
  - `EditArticle.tsx`
  - `Dashboard.tsx` (if applicable for delete actions)
- Replace state-based messages with `notificationService` calls.

## 3. Success Criteria
- [ ] Backend logs are written to both console and daily files.
- [ ] API returns RFC 7807 Problem Details for all errors.
- [ ] Frontend displays toast notifications for success and error events.
- [ ] No "ghost" error messages remain in refactored components.
