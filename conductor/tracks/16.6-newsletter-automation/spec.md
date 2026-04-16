# Specification - Track 16.6: Post-Registration & Newsletter Automation

## 1. Problem Statement
The current system lacks a clear feedback loop after user registration and does not proactively notify subscribers of new content. Additionally, a professional newsletter system requires a secure way for users to opt-out.

## 2. Requirements

### 2.1 Post-Registration UI
- A dedicated landing page informing the user to check their inbox.
- Automatic redirection from the registration form upon success.

### 2.2 Newsletter Automation
- Automatic trigger when a new `Article` is published.
- Notification sent to all `IsVerified == true` and `IsActive == true` subscribers.
- Emails must be rendered using Razor templates for a professional look.

### 2.3 Unsubscribe Mechanism
- Secure, token-based unsubscribe links in every newsletter footer.
- Soft-deactivation of subscribers (`IsActive = false`) without deleting history.
- Friendly confirmation page on the frontend.

## 3. Technical Architecture

### 3.1 Domain Events
- Use `ArticlePublishedEvent` (INotification) to decouple article creation from email dispatching.

### 3.2 Persistence
- `ISubscriberRepository` for managing newsletter recipients.
- `UnsubscribeToken` added to the `Subscriber` entity for security.

### 3.3 Integration
- Register user as a subscriber during `RegisterCommandHandler`.
- Verify subscriber status during `ConfirmEmailCommandHandler`.

## 5. Verification Strategy

### 5.1 Unit Testing
- **UnsubscribeHandler**: Verified successful deactivation, token validation, and not-found scenarios.
- **ArticlePublishedEventHandler**: Verified batch email dispatching to active/verified subscribers and proper logging.
- **Identity Command Handlers**: Updated `Register` and `ConfirmEmail` tests to verify automatic newsletter enrollment and verification.
- **Article Command Handlers**: Verified that publishing an article correctly triggers the domain event.

### 5.2 Integration Testing
- **Newsletter API**: End-to-end verification of the `Unsubscribe` flow, ensuring the user is redirected to the frontend and the database status is updated.
- **Auth Flow**: Verified that the full registration-to-confirmation cycle correctly manages both Identity and Newsletter state.
