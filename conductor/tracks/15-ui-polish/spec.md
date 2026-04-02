# Track 15: Portfolio Polish & UI/UX - Specification

## 1. Overview
This track focuses on the visual and structural refinement of the `meupessoal-web` application. The primary objectives are to implement a robust sticky footer layout and a comprehensive Dark Mode system using Tailwind CSS v4.

## 2. Layout & Sticky Footer (Task 15.1)

### 2.1 UI/UX Requirements
- The footer remains at the bottom of the viewport even when page content is minimal.
- The main wrapper maintains a minimum height of `100vh` (or `min-h-dvh` for mobile browsers).
- Consistent container padding across different screen sizes.

### 2.2 Implementation Strategy
- **Container**: Use a Flexbox column layout (`flex flex-col`) on the main wrapper.
- **Main Content**: Apply `flex-grow` to the `<main>` element to push the footer to the bottom.
- **Responsiveness**: Ensure the layout adjusts smoothly between mobile, tablet, and desktop views.

## 3. Dark Mode Support (Task 15.3)

### 3.1 Requirements
- **Persistence**: Store the user's theme preference in `localStorage`.
- **System Detection**: Detect and respect the operating system's theme preference (`prefers-color-scheme`) as a default.
- **Toggle**: A dedicated UI component for manual theme switching.
- **Strategy**: Tailwind CSS v4 `class` strategy (applying the `.dark` class to the `html` element).

### 3.2 Technical Components

#### 3.2.1 `useDarkMode` Hook
- **Path**: `meupessoal-web/src/hooks/useDarkMode.ts`
- **Functionality**:
    - Manages the theme state ('light' | 'dark').
    - Synchronizes with `localStorage`.
    - Updates the `document.documentElement` class list.
    - Handles system preference changes via `matchMedia`.

#### 3.2.2 `ThemeToggle` Component
- **Path**: `meupessoal-web/src/components/ThemeToggle.tsx`
- **Features**:
    - Uses `lucide-react` icons (`Sun`, `Moon`).
    - Smooth transition animations.
    - Clear hover and active states.

#### 3.2.3 Global Styles Refinement
- **Path**: `meupessoal-web/src/index.css`
- **Implementation**:
    - Define dark mode color variants using the `dark:` prefix.
    - Ensure background and text colors transition smoothly between modes.

