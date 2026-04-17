# Implementation Plan - Track 16.6: Post-Registration & Newsletter Automation

## Phase 1: Frontend Infrastructure
- [x] Create `CheckEmail.tsx` component.
- [x] Create `UnsubscribeSuccess.tsx` component.
- [x] Update `App.tsx` routes.
- [x] Update `Register.tsx` to handle navigation.

## Phase 2: Domain & Data
- [x] Add `UnsubscribeToken` to `Subscriber` entity.
- [x] Create `ArticlePublishedEvent` in Domain layer.
- [x] Define `ISubscriberRepository` interface.
- [x] Implement `SubscriberRepository` in Infrastructure layer.
- [x] Register repository in `DependencyInjection` / `Program.cs`.

## Phase 3: Backend Logic
- [x] Update `CreateArticleHandler` to publish `ArticlePublishedEvent`.
- [x] Implement `ArticlePublishedEventHandler`.
- [x] Create `NewPostNotification.cshtml` Razor template.
- [x] Implement `UnsubscribeCommand` and handler.
- [x] Add `Unsubscribe` action to `NewsletterController`.

## Phase 4: Lifecycle Integration
- [x] Update `RegisterCommandHandler` to add newsletter subscription.
- [x] Update `ConfirmEmailCommandHandler` to verify newsletter subscription.

## Phase 5: Verification
- [x] Build solution and fix compilation errors.
- [x] Add `UnsubscribeHandlerTests` for command logic.
- [x] Add `ArticlePublishedEventHandlerTests` for email delivery logic.
- [x] Update `RegisterCommandHandlerTests` to verify newsletter enrollment.
- [x] Update `ConfirmEmailCommandHandlerTests` to verify newsletter verification.
- [x] Add `Unsubscribe` integration test in `NewsletterIntegrationTests`.
- [x] Update `AuthIntegrationTests` for full-cycle newsletter verification.
- [x] Add EF Migration `AddUnsubscribeTokenToSubscriber` and verify in integration tests.
