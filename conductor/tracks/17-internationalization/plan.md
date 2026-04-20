# Implementation Plan - Track 17: Internationalization (i18n)

## Phase 1: Setup
- [x] Install dependencies: `i18next`, `react-i18next`, `i18next-browser-languagedetector`, `i18next-http-backend`.
- [x] Create directory structure for locales in `public/locales`.
- [x] Initialize English translation file (`en/translation.json`).
- [x] Initialize Portuguese translation file (`pt/translation.json`).

## Phase 2: Configuration
- [x] Create `src/i18n/config.ts` with backend and detection plugins.
- [x] Configure lazy loading via `loadPath`.
- [x] Import `i18n/config` in `src/main.tsx`.
- [x] Wrap `<App />` in `React.Suspense` for loading state.

## Phase 3: UI Integration
- [x] Refactor `Layout.tsx` to use `useTranslation`.
- [x] Implement `toggleLanguage` logic.
- [x] Add Language Toggle button with Lucide icon in the Header.
- [x] Localize navigation links and footer content.

## Phase 4: Verification
- [x] Verify language detection on first load.
- [x] Verify language persistence in `localStorage`.
- [x] Verify responsive behavior of the toggle button.

## Phase 5: Multi-language Schema
- [x] Update Domain Entities (Article, Category) to support localized fields.
- [x] Migrate database to support multi-language content columns.
- [x] Implement translation service for dynamic database content.
