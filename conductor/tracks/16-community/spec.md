# Anonymous Newsletter System Specification

## 1. Overview
The goal is to implement an anonymous newsletter subscription system capturing reader emails with clear feedback on subscription status. It needs to be independent of the Identity/User framework.

## 2. API Contracts
**Endpoint:** `POST /api/newsletter/subscribe`

**Request Body (`SubscribeRequestDto`):**
```json
{
  "email": "user@example.com"
}
```

**Responses:**
*   **200 OK:** Successfully subscribed, returns no content or a success boolean.
*   **400 Bad Request:** Validation failed (empty or invalid email format).
*   **409 Conflict:** Email already exists and is active. Returns a specific error message `{"message": "This email is already subscribed."}`.

## 3. Data Models
**C# Domain Entity (`Subscriber`):**
*   `Id` (Guid)
*   `Email` (string, unique constraint)
*   `SubscribedAt` (DateTime)
*   `IsActive` (bool)

**C# DTOs / Commands:**
*   `SubscribeToNewsletterCommand(string Email)` : `IRequest<Result>` (using a custom Result object or returning success/fail enum). For simplicity, returning a specific `NewsletterSubscriptionResult` or handling exceptions/custom responses in the controller.

**TypeScript Interface (`NewsletterSubscriptionRequest`):**
```typescript
export interface NewsletterSubscriptionRequest {
    email: string;
}
```

## 4. Architecture Design & Patterns
*   **Backend (MeuSitePessoal.Api):**
    *   **Clean Architecture:**
        *   **Domain:** `Subscriber` entity added to Domain.
        *   **Application:** CQRS via MediatR. `SubscribeToNewsletterCommand`, `SubscribeToNewsletterCommandHandler`.
        *   **Validation:** FluentValidation rules integrated in a `SubscribeToNewsletterCommandValidator` class to validate the e-mail explicitly.
        *   **Infrastructure:** EF Core. `BlogDbContext` will contain `DbSet<Subscriber>`. A new migration will be generated.
    *   **Idempotency & Behavior:** The handler will check if the email exists. If true and `IsActive`, returns conflict. If `!IsActive`, marks as `IsActive = true` and updates `SubscribedAt`.
*   **Frontend (meupessoal-web):**
    *   **Component (`NewsletterBox.tsx`):** A functional component using React Hooks for state management (`idle`, `loading`, `success`, `error`).
    *   **Integration:** Can be included at the App.tsx level so it renders on all main pages, such as the footer.
    *   **User Feedback:** Handle the 409 error explicitly and display the exact string to the user.
