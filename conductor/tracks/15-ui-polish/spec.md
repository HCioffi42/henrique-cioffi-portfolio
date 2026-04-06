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

## 4. Header Standardization (Task 15.4)

### 4.1 UI/UX Requirements
- Interactive header elements (Search, Theme Toggle, Auth buttons) must have consistent visual weight.
- Use fixed square dimensions (e.g., `w-10 h-10`) for icon-based buttons to ensure symmetry.
- Icons must be perfectly centered within their containers using Flexbox or Grid.
- Maintain consistent spacing (gap) between navigation elements.

### 4.2 Implementation Strategy
- **Container Sizing**: Apply standard Tailwind width/height classes to all header action buttons.
- **Alignment**: Use `flex items-center justify-center` for centralized icon placement.
- **Consistency**: Audit all header components to ensure they adhere to the new sizing standards.

## 5. Accessibility Theme Toggle (Task 15.5)

### 5.1 UI/UX Requirements
- Transition from a simple icon button to a robust `Switch` or `Toggle` pattern.
- Include visual indicators (Sun and Moon icons) for both states.
- Provide descriptive text labels (Light/Dark) that are accessible to Screen Readers.
- Implement smooth CSS transitions for the "thumb" or background of the toggle.

### 5.2 Technical Requirements
- **Semantic HTML**: Use a `<button>` with `role="switch"` and `aria-checked`.
- **Accessibility**: Use `aria-label` to clearly state the current and target theme states.
- **State Management**: Continue using the existing `ThemeContext` and `useDarkMode` hook.
- **Animation**: Utilize Tailwind CSS transitions or CSS keyframes for a polished "slide" effect.

## 6. Mobile Navigation (Task 15.6)

### 6.1 UI/UX Requirements
- Reveal a "Hamburger" menu icon for viewports below the `lg` breakpoint.
- Hide standard desktop navigation links on small screens.
- Reveal a mobile-friendly slide-over or dropdown menu containing all key links:
    - Blog
    - Admin/Login
    - New Post (if authenticated)
- Ensure the `Search` and `ThemeToggle` remain accessible within the mobile layout.
- Use a backdrop overlay to focus attention on the active menu.

### 6.2 Technical Requirements
- **State Management**: Use a local React state to manage the open/closed status of the menu.
- **Responsive Classes**: Employ Tailwind's `hidden` and `lg:flex` / `lg:block` classes for layout switching.
- **Animations**: Implement a clean entry/exit transition for the menu.
- **UX**: Ensure the menu is closed automatically when a navigation link is clicked.

