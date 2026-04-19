# Track 17: Internationalization (i18n)

## Status
- [x] Specification: `spec.md`
- [x] Implementation Plan: `plan.md`
- [x] Implementation: Completed

## Objectives
Implement a robust, scalable, and user-friendly internationalization system for the React portfolio, supporting English and Portuguese with lazy loading.

## Execution Summary
Implemented a full internationalization pipeline using `i18next`.
- **Infrastructure**: Integrated `i18next-http-backend` for asynchronous resource fetching and `i18next-browser-languagedetector` for automatic user preference handling.
- **Lazy Loading**: Configured the application to load translation JSONs on demand, reducing the initial bundle size.
- **UI Integration**: Added a Language Toggle component in the Header with immediate state synchronization.
- **Content**: Localized core navigation, user greetings, and footer components.

## Technical Details
- **Default Language**: English (`en`).
- **Persistence**: Choices are stored in `localStorage` under the key `i18nextLng`.
- **Loading State**: Handled by `React.Suspense` in `main.tsx`.

## Technical Debts / Observations
- Future tracks should expand localization to the remaining pages (Dashboard, Article Details, etc.).
- Article content (from the database) currently remains in its original language; a translation strategy for dynamic content may be needed later.
