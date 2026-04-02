# Track 15: Portfolio Polish & UI/UX - Implementation Plan

## 1. Research & Analysis
- [x] Analyze existing `Layout.tsx` structure and its interaction with `App.tsx`.
- [x] Verify Tailwind v4 dark mode configuration requirements (defaults to `media` but supports manual overrides via classes).

## 2. Task 15.1: Sticky Footer & Layout Refactoring
- [x] **Modify `meupessoal-web/src/components/Layout.tsx`**:
    - Update the wrapper `div` to use `min-h-screen` (or `min-h-dvh`) and `flex flex-col`.
    - Ensure the `<main>` element has `flex-grow`.
    - Adjust footer classes to remove unnecessary absolute positioning if any.
    - Standardize horizontal padding for the content container (`max-w-5xl mx-auto px-6`).

## 3. Task 15.2: Sticky Toolbar & Markdown Toggle Logic
- [x] **Enhance `meupessoal-web/src/components/MarkdownToolbar.tsx`**:
    - Refactor `insertMarkdown` to include toggle (wrap/unwrap) logic.
    - Implement multi-line detection for the toggle behavior.
    - Update CSS classes for sticky positioning (`sticky top-[72px]`).
    - Ensure `z-index` and background consistency (solid background for sticky).
- [x] **Verify in `CreateArticle.tsx` and `EditArticle.tsx`**:
    - Test the sticky behavior with long content.
    - Ensure the toolbar doesn't overlap with the main header or get covered by it.

## 4. Task 15.3: Dark Mode Implementation
- [x] **Create `meupessoal-web/src/hooks/useDarkMode.ts`**:
    - Implement theme detection logic.
    - Implement `localStorage` persistence.
    - Implement the effect to toggle the `.dark` class on the `document.documentElement`.
- [x] **Create `meupessoal-web/src/components/ThemeToggle.tsx`**:
    - Implement a button that toggles between light and dark modes.
    - Use `Sun` and `Moon` icons from `lucide-react`.
    - Add hover effects and accessible labels.
- [x] **Integrate into `meupessoal-web/src/components/Layout.tsx`**:
    - Add the `ThemeToggle` component to the header navigation.
    - Apply `dark:` variants to the header, footer, and background.
- [x] **Update `meupessoal-web/src/index.css`**:
    - Add global transitions for `background-color` and `border-color`.
    - Define base dark mode styles for common elements (body, text colors).
- [x] **Update existing components**:
    - Audit `ArticleCard.tsx` and `MarkdownRenderer.tsx` for hardcoded light-mode colors and apply `dark:` variants.
    - Refine `Login.tsx`, `Dashboard.tsx`, `ArticleList.tsx`, `ArticleDetails.tsx`, `CreateArticle.tsx`, and `EditArticle.tsx`.

## 5. Validation & Testing
- [x] Verify sticky footer behavior on pages with low content (e.g., Login, empty search).
- [x] Test dark mode persistence across page reloads.
- [x] Verify system preference detection (changing OS theme should update the site if no manual override exists).
- [x] Ensure mobile responsiveness remains intact after layout changes.
- [x] Run `npm run lint` and `npm run build` to ensure no regressions.
