# Implementation Plan: Track 18 - Full API HTTP Suite

This plan details the steps to complete the `UsersApi.http` file.

## Steps

### 1. Variables and Metadata
- Add `@commentId`, `@subscriberEmail`, and `@token` placeholder variables.

### 2. Authentication Flow Expansion
- Add `POST /api/auth/register` with example body.
- Add `GET /api/auth/confirm-email` (placeholder token).
- Add `POST /api/auth/forgot-password` and `POST /api/auth/reset-password`.
- Add `POST /api/auth/verify-2fa`.
- Add `GET /api/auth/external-login`.
- Add `PUT /api/auth/language`.

### 3. Comment System Implementation
- Add `GET /api/articles/{{articleId}}/comments`.
- Add `POST /api/comments` (Auth required).
- Add `PUT /api/comments/{{commentId}}` (Auth required).
- Add `DELETE /api/comments/{{commentId}}` (Auth required).

### 4. Newsletter System Implementation
- Add `POST /api/newsletter/subscribe`.
- Add `GET /api/newsletter/confirm`.
- Add `GET /api/newsletter/unsubscribe`.

### 5. Media Cleanup
- Add `DELETE /api/images?url=...` (Auth required).

### 6. Validation
- Verify all endpoints match the route definitions in the controllers.
- Ensure `Authorization: Bearer {{auth_token}}` is correctly applied to protected routes.
