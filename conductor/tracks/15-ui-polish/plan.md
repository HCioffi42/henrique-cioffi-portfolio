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

## 6. Task 15.4: Header Standardization
- [x] **Modify `meupessoal-web/src/components/SearchBar.tsx`**:
    - Update the search button container to use consistent dimensions (`w-10 h-10`).
    - Use Flexbox to ensure the search icon is perfectly centered.
- [x] **Audit `meupessoal-web/src/components/Layout.tsx`**:
    - Standardize dimensions for all header action buttons.
    - Implement consistent spacing and vertical alignment for navigation items.

## 7. Task 15.5: Accessibility Theme Toggle
- [x] **Refactor `meupessoal-web/src/components/ThemeToggle.tsx`**:
    - Implement a `Switch` pattern using a `button` with `role="switch"` and `aria-checked`.
    - Add Sun and Moon icons as visual state indicators.
    - Include descriptive `aria-labels` and visual (or screen-reader only) labels for accessibility.
    - Add smooth sliding transitions for the toggle switch thumb.
    - Maintain integration with the existing `useTheme` context.

## 8. Task 15.6: Mobile Navigation (Hamburger Menu)
- [x] **Create `meupessoal-web/src/components/MobileMenu.tsx`**:
    - Implement a slide-over or dropdown menu for mobile devices.
    - Use a backdrop overlay to focus attention and handle "click-outside" closures.
    - Ensure all desktop navigation links (Blog, Projects, Admin/Login) are present.
- [x] **Update `meupessoal-web/src/components/Layout.tsx`**:
    - Integrate the hamburger menu trigger button (visible below `lg` breakpoint).
    - Use Tailwind's responsive classes (`hidden lg:flex`) to toggle between desktop and mobile navigation layouts.
    - Implement logic to close the menu upon navigation.

## 9. Validation & Testing
- [x] Verify header symmetry and dimension consistency across browsers.
- [x] Test the Theme Toggle accessibility with screen readers.
- [x] Validate mobile navigation functionality (Hamburger menu opening/closing and link clicks).
- [x] Ensure the Search and Theme Toggle remain accessible on mobile devices.
- [x] Test Dark Mode transitions for smoothness.
- [x] Run `npm run lint` and `npm run build` to ensure no regressions.
