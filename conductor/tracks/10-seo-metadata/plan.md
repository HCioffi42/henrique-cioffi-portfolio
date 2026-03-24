# Track 10: SEO & Metadata - Implementation Plan

## 1. Environment Setup
- [ ] Install `react-helmet-async` in `meupessoal-web`.
- [ ] Verify the installation in `package.json`.

## 2. Infrastructure
- [ ] **Global Provider**: Wrap the application in `HelmetProvider` within `App.tsx`.
- [ ] **Default Assets**: Ensure a default OG image exists (e.g., `public/favicon.svg` or a new `public/og-brand.png`). *Note: I will use favicon.svg as a fallback if no specific brand image is provided.*

## 3. Core Component Implementation
- [ ] **SEO Component**: Create `meupessoal-web/src/components/SEO.tsx`.
    - Implement `Helmet` logic for dynamic `<title>`, `<meta name="description">`, `<meta name="keywords">`, and `<link rel="canonical">`.
    - Implement Open Graph tags (`og:title`, `og:description`, `og:type`, `og:url`, `og:image`).
    - Implement Twitter Card tags (`twitter:card`, `twitter:title`, `twitter:description`, `twitter:image`).
    - Handle `article` specific metadata (published time, tags).

## 4. Integration
- [ ] **Article Listing (`ArticleList.tsx`)**:
    - Remove direct `document.title` manipulation.
    - Integrate the `SEO` component.
    - Pass dynamic titles based on active filters (Tags/Categories).
- [ ] **Article Details (`ArticleDetails.tsx`)**:
    - Integrate the `SEO` component.
    - Ensure it updates Reactively when the `article` state is populated.
    - Generate description from the article content (first 160 characters).
    - Map article tags to SEO keywords.

## 5. Validation
- [ ] Build the project to ensure no type errors.
- [ ] Manually verify the `<head>` in the browser for:
    - Homepage.
    - Filtered list (by tag).
    - Article details page.
- [ ] Check console for `react-helmet-async` warnings.

## 6. Finalization
- [ ] Create `index.md` in `conductor/tracks/10-seo-metadata/`.
- [ ] Update `conductor/tracks.md` to mark the track as completed.
