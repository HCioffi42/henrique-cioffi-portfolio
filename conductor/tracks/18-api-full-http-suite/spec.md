# Specification: Track 18 - Full API HTTP Suite

This track focuses on expanding the `UsersApi.http` file to include all available endpoints in the `MeuSitePessoal.Api` project. This will allow for comprehensive manual testing directly from Rider or other IDEs that support `.http` (REST Client) files.

## Objectives
- Document all Authentication endpoints (Registration, Email Confirmation, Password Recovery, 2FA, OAuth2).
- Document all Comment system endpoints (Get by Article, Create, Update, Delete).
- Document all Newsletter system endpoints (Subscribe, Confirm, Unsubscribe).
- Document all Media management endpoints (Upload, Delete).
- Ensure all endpoints have proper headers (Authorization, Content-Type) and example request bodies.

## API Contracts to be added

### 1. Authentication
- `POST /api/auth/register`: Create a new user account.
- `GET /api/auth/confirm-email`: Confirm account via token.
- `POST /api/auth/forgot-password`: Request reset link.
- `POST /api/auth/reset-password`: Reset password with token.
- `POST /api/auth/verify-2fa`: Verify TOTP code.
- `GET /api/auth/external-login`: Initiate Google/GitHub login.
- `PUT /api/auth/language`: Update preferred language.

### 2. Comments
- `GET /api/articles/{id}/comments`: Retrieve nested comment tree.
- `POST /api/comments`: Create comment/reply.
- `PUT /api/comments/{id}`: Update comment content.
- `DELETE /api/comments/{id}`: Remove comment.

### 3. Newsletter
- `POST /api/newsletter/subscribe`: Initial subscription request.
- `GET /api/newsletter/confirm`: Confirm subscription.
- `GET /api/newsletter/unsubscribe`: Remove email from list.

### 4. Media
- `DELETE /api/images`: Delete an uploaded file by URL.

## Technical Details
- **File Location**: `MeuSitePessoal.Api/UsersApi.http`
- **Variables**: Reuse existing `@host`, `@contentType`, `@auth_token`, and `@articleId`. Add new variables like `@commentId` if necessary.
- **Security**: Use the `Authorization: Bearer {{auth_token}}` header for all protected endpoints.
