# Plan: Interactive Markdown Editor

This track implements a professional Markdown editing experience for administrators, including a sticky toolbar and a live preview mode.

**Status**: In Progress
**Track ID**: 07-interactive-markdown-editor

## Implementation Steps

1. [x] **Componentization & Refactoring**:
   - Extract `MarkdownRenderer` from `ArticleDetails.tsx` to `meupessoal-web/src/components/MarkdownRenderer.tsx`.
   - Ensure it uses the centralized `BACKEND_URL` for image mapping.
   - Update `ArticleDetails.tsx` to use the shared component.

2. [x] **Markdown Toolbar Development**:
   - Create `meupessoal-web/src/components/MarkdownToolbar.tsx`.
   - Install `lucide-react` for icons.
   - Implement tools for: Bold, Italic, H2, H3, Links, and Bullet Lists.
   - Integrate the existing image upload logic into the toolbar.

3. [x] **Advanced Text Selection Logic**:
   - Implement `insertMarkdown(prefix, suffix)` helper with `requestAnimationFrame`.
   - [x] **Problem 1 Fix**: Smart whitespace trimming to handle accidental spaces in double-click selections.
   - [x] **Problem 2 Fix**: Multi-line processing logic to apply formatting (Bold, Lists, etc.) to each line individually.
   - Ensure cursor persistence and focus after insertion.

4. [/] **Live Preview Integration**:
   - [x] Add `isPreviewMode` state to `CreateArticle.tsx` and `EditArticle.tsx`.
   - [ ] **Problem 3.1 Fix**: Move the "Write / Preview" toggle into the toolbar for better ergonomics (In Progress).
   - Toggle between the `textarea` + `Toolbar` and the `MarkdownRenderer`.

5. [/] **Styling & Aesthetics**:
   - Use Tailwind CSS for a modern, minimalist editor design.
   - Ensure the toolbar is sticky to the top of the editing area.
   - [ ] **Problem 3.2 Fix**: Implement `preventScroll: true` on focus calls to stop the page from jumping to the top (In Progress).

## Verification
- [x] Verify toolbar buttons function correctly with multi-line selection.
- [x] Confirm smart trimming prevents broken Markdown syntax on double-clicks.
- [ ] Confirm image preview works correctly in "Preview" mode.
- [x] Test the "Live Preview" toggle with complex Markdown content.
- [x] Ensure `EditArticle.tsx` correctly loads and renders existing article data.
