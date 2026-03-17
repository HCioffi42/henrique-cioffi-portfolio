# Plan: Implement Article Listing and Home Page Integration

This track implements the frontend listing of articles using the optimized summary endpoint, including a reusable `ArticleCard` component and Home page integration.

## Backend Refactoring (Completed)
- Removed "HC" prefixes from the summary-related files and classes (`ArtigoSummaryDto`, `GetArtigosQuery`, `GetArtigosHandler`).
- Maintained the optimized query that excludes the `Conteudo` column and uses `AsNoTracking`.

## Frontend Implementation Steps

1. **Type Definition**: Create `ArtigoSummary.ts` in `meupessoal-web/src/models/` matching the backend DTO.
2. **Service Update**: Add `getArtigoSummaries` to `meupessoal-web/src/services/artigoService.ts`.
3. **Component Creation**: Create `ArticleCard.tsx` in `meupessoal-web/src/components/`.
   - Display: Title, summary snippet, formatted date, and tag badges.
   - Use Tailwind CSS for a clean, minimalist, and responsive design.
4. **Home Page Integration**: Update `meupessoal-web/src/pages/ArtigoList.tsx`.
   - Fetch data using `getArtigoSummaries`.
   - Map over the data and render `ArticleCard` components.
   - Handle loading and empty states.
5. **Verification**: Run the frontend and verify the layout and data fetching.

## Branding & Standards
- Use English for all code, types, and comments.
- Descriptive third-person comments.
- Professional naming conventions (no "HC" prefix).
- Fully responsive design using Tailwind CSS.
