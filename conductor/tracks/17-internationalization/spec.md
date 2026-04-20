# Specification - Track 17: Internationalization (i18n)

## 1. Problem Statement
The portfolio needs to support multiple languages to reach a wider audience, specifically English (EN) and Portuguese (PT). The implementation must be efficient (lazy loading) and persist user preferences. Additionally, dynamic content (Articles, Categories) must support localization.

## 2. Requirements
- Support for English (default) and Portuguese.
- Lazy loading of translation files to minimize initial bundle size.
- Automatic language detection based on browser settings or previous user choice.
- Persistence of the chosen language in `localStorage`.
- Language toggle UI in the Header.
- Integration with existing Tailwind CSS styling.
- Localization of dynamic database content (titles, summaries, tags).

## 3. Technical Architecture

### 3.1 Dependencies
- `i18next`: Core internationalization framework.
- `react-i18next`: React integration for i18next.
- `i18next-browser-languagedetector`: Detects user language in the browser.
- `i18next-http-backend`: Loads translation files from a server/public folder.

### 3.2 Configuration
- Configuration file located at `src/i18n/config.ts`.
- Translation files served from `public/locales/{{lng}}/translation.json`.
- Main application wrapped in `React.Suspense` to handle asynchronous loading.
- Database: Extend entities to store localized content (e.g., using JSONB or dedicated translation tables).

### 3.3 Data Models (JSON Structure)
UI Translations follow a nested JSON structure:
- `nav`: Navigation links.
- `footer`: Footer content.
- `common`: Reusable strings (Loading, Error, etc.).

## 4. UI/UX Design
- A language toggle button in the Header featuring a `Languages` icon.
- Immediate UI updates upon language change without page reload.
- Responsive design ensuring the toggle is accessible on all devices.
