# Track 10: SEO & Metadata - Technical Specification

## 1. Overview
The goal of this track is to implement a robust SEO and Metadata system for the "MeuSitePessoal" frontend. This includes dynamic title management, meta descriptions, keywords, and social media integration (Open Graph and Twitter Cards).

## 2. Technical Stack
- **Library**: `react-helmet-async` for managing the document head in a React-friendly way, avoiding memory leaks and ensuring SSR compatibility (if needed in the future).
- **Frontend**: React (TypeScript).

## 3. Architecture
The system will follow a centralized approach using a reusable `SEO` component.

### 3.1 `SEO` Component Props
The component will accept the following props:
```typescript
interface SEOProps {
    title?: string;
    description?: string;
    keywords?: string;
    image?: string;
    url?: string;
    type?: 'website' | 'article';
    articleData?: {
        publishedTime?: string;
        modifiedTime?: string;
        author?: string;
        section?: string;
        tags?: string[];
    };
}
```

### 3.2 Global Provider
The `App.tsx` will be wrapped in `HelmetProvider` to provide context for all `Helmet` instances.

### 3.3 Default Metadata
A set of default values will be used when specific props are missing:
- **Default Title**: "MySite - Technology, Design, and Engineering"
- **Default Description**: "Exploring the intersection of technology, design, and software engineering. Portfolio and blog by [Author Name]."
- **Default Image**: A static brand image located in `public/assets/og-image.png`.
- **Default URL**: The current window location.

## 4. Implementation Details

### 4.1 10.1 - Dynamic Meta Tags
- **Title**: If a title is provided, it will be formatted as `{title} | MySite`. If not, the default title is used.
- **Canonical Link**: Dynamically generated based on the current URL.
- **Keywords**: Comma-separated list derived from article tags or site defaults.

### 4.2 10.2 - Open Graph & Social Media
- **OG Tags**: `og:title`, `og:description`, `og:type`, `og:url`, `og:image`.
- **Twitter Tags**: `twitter:card` (default: 'summary_large_image'), `twitter:title`, `twitter:description`, `twitter:image`.
- **Article Specifics**: For `ArticleDetails`, `og:type` will be set to `article`, and additional tags like `article:published_time` and `article:tag` will be included.

## 5. Integration Points

### 5.1 `ArticleList.tsx`
The `SEO` component will be added to the main listing.
- **Dynamic Title**: "Articles tagged #{tag} | MySite" or "Category: {category} | MySite".
- **Description**: Based on the active filter or default.

### 5.2 `ArticleDetails.tsx`
The `SEO` component will be added after the article data is fetched.
- **Title**: Article title.
- **Description**: A summary or the first few sentences of the article.
- **Image**: The article's feature image URL (if implemented in the future) or default.
- **Reactive Updates**: Metadata will update as soon as the `article` state changes.

## 6. Constraints & Performance
- Ensure `react-helmet-async` is used to prevent issues with concurrent rendering.
- Metadata must be descriptive and follow SEO best practices (e.g., titles < 60 chars, descriptions < 160 chars).
- Avoid unnecessary re-renders of the `SEO` component by using `React.memo` if needed, although `Helmet` handles updates efficiently.

## 7. Verification Plan
- **Manual Verification**: Inspect the `<head>` element using Browser DevTools during navigation between pages and after applying filters.
- **Validation Tools**: Use "Social Share Preview" extensions or tools like [OpenGraph.xyz](https://www.opengraph.xyz) to verify social tags.
- **Automated Tests**: (Optional but recommended) Add a unit test for the `SEO` component to ensure it renders the correct tags in the head.
