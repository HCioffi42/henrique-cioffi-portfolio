# Specification: Implement Create Article Frontend

## Overview
Implement the "Create Article" feature in the `meupessoal-web` project to allow users to add new content to the blog.

## Functional Requirements
- **Service Function:** `createArtigo` must send a POST request to `/Artigos`.
- **Form Fields:**
  - Title (Input)
  - Summary (Input)
  - Content (Textarea)
  - Tags (Input for comma-separated values)
- **Validation:** Prevent empty submissions for required fields.
- **Navigation:**
  - "New Post" button in the header must lead to the creation page.
  - "Cancel" button in the form must return to the home page.
  - Successful submission must redirect to the home page.

## Non-Functional Requirements
- **Styling:** Use Tailwind CSS for a clean and modern appearance.
- **Typing:** Strict TypeScript typing (no `any`).
- **Documentation:** JSDoc comments for all new functions and components.
- **Responsive:** The form must work well on mobile and desktop.

## Acceptance Criteria
- [x] New article can be submitted through the form.
- [x] API request matches the backend's expected structure.
- [x] Code is clean, documented, and linted.
- [x] Build passes without errors.
