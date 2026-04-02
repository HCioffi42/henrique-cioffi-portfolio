# Track 15: Portfolio Polish & UI/UX - Implementation Plan

## 1. Research & Analysis
- [ ] Analyze existing `Layout.tsx` structure and its interaction with `App.tsx`.
- [ ] Verify Tailwind v4 dark mode configuration requirements (defaults to `media` but supports manual overrides via classes).

## 2. Task 15.1: Sticky Footer & Layout Refactoring
- [ ] **Modify `meupessoal-web/src/components/Layout.tsx`**:
    - Update the wrapper `div` to use `min-h-screen` (or `min-h-dvh`) and `flex flex-col`.
    - Ensure the `<main>` element has `flex-grow`.
    - Adjust footer classes to remove unnecessary absolute positioning if any.
    - Standardize horizontal padding for the content container (`max-w-5xl mx-auto px-6`).

## 3. Task 15.3: Dark Mode Implementation
- [ ] **Create `meupessoal-web/src/hooks/useDarkMode.ts`**:
    - Implement theme detection logic.
    - Implement `localStorage` persistence.
    - Implement the effect to toggle the `.dark` class on the `document.documentElement`.
- [ ] **Create `meupessoal-web/src/components/ThemeToggle.tsx`**:
    - Implement a button that toggles between light and dark modes.
    - Use `Sun` and `Moon` icons from `lucide-react`.
    - Add hover effects and accessible labels.
- [ ] **Integrate into `meupessoal-web/src/components/Layout.tsx`**:
    - Add the `ThemeToggle` component to the header navigation.
    - Apply `dark:` variants to the header, footer, and background.
- [ ] **Update `meupessoal-web/src/index.css`**:
    - Add global transitions for `background-color` and `border-color`.
    - Define base dark mode styles for common elements (body, text colors).
- [ ] **Update existing components**:
    - Audit `ArticleCard.tsx` and `MarkdownRenderer.tsx` for hardcoded light-mode colors and apply `dark:` variants.

## 4. Validation & Testing
- [ ] Verify sticky footer behavior on pages with low content (e.g., Login, empty search).
- [ ] Test dark mode persistence across page reloads.
- [ ] Verify system preference detection (changing OS theme should update the site if no manual override exists).
- [ ] Ensure mobile responsiveness remains intact after layout changes.
- [ ] Run `npm run lint` and `npm run build` to ensure no regressions.
