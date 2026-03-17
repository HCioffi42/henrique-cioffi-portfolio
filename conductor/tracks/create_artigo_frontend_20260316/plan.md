# Plan: Implement Create Article Frontend

## Objective
Provide an interface for administrators to create new articles directly from the frontend.

## Key Files & Context
- `meupessoal-web/src/services/artigoService.ts`: Service for API communication.
- `meupessoal-web/src/pages/CreateArtigo.tsx`: New page for the creation form.
- `meupessoal-web/src/App.tsx`: Routing configuration.
- `meupessoal-web/src/components/Layout.tsx`: Header navigation.

## Implementation Steps

### Phase 1: Service Layer
- [x] Add `createArtigo` function to `src/services/artigoService.ts` using Axios `post`.
- [x] Ensure proper typing and JSDoc documentation.

### Phase 2: Creation Page
- [x] Create `CreateArtigo.tsx` component.
- [x] Implement form state with `useState`.
- [x] Style the form using Tailwind CSS (modern, responsive look).
- [x] Implement field validation (Title, Summary, Content required).
- [x] Handle comma-separated tags and convert them to an array.

### Phase 3: Routing and Navigation
- [x] Register `/admin/new-post` in `src/App.tsx`.
- [x] Update "New Post" button in `src/components/Layout.tsx` to use `Link`.

### Phase 4: Verification
- [x] Run `npm run lint` to ensure code style.
- [x] Run `npm run build` to verify TypeScript and build integrity.
