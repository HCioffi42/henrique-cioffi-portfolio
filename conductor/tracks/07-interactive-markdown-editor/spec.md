# Specification: Interactive Markdown Editor ✅

## Status: Completed ✅

## Feature Overview
The objective is to provide a rich, interactive editing experience for blog administrators. This includes a toolbar for common Markdown formatting and a live preview mode to verify the content's appearance before saving.

## Functional Requirements

### 1. Markdown Toolbar
- **UI**: A sticky bar positioned at the top of the article content editor.
- **Tools**:
    - **Bold**: Wraps selection with `**`.
    - **Italic**: Wraps selection with `_`.
    - **Heading 2**: Prepends `## ` to the selection.
    - **Heading 3**: Prepends `### ` to the selection.
    - **Link**: Wraps selection with `[selection](url)`.
    - **Bullet List**: Prepends `- ` to the selection.
    - **Image Upload**: Triggers file selection and inserts `![name](url)`.
- **Selection Logic**: If text is selected, the formatting should wrap or prepend to the selected text. If no text is selected, the markers should be inserted at the cursor, and the cursor should be placed between them (where applicable).

### 2. Live Preview
- **Toggle**: A "Write / Preview" switch located at the top right of the editor section.
- **Rendering**: 
    - **Write Mode**: Shows the Markdown Toolbar and the `textarea`.
    - **Preview Mode**: Hides the editor and renders the content using the shared `MarkdownRenderer`.
- **Consistency**: The preview must use the same styles (typography, image handling, code blocks) as the public article view.

### 3. Shared Components
- **`MarkdownRenderer`**: A centralized component for rendering Markdown using `react-markdown`. It must handle:
    - GitHub Flavored Markdown (via `remark-gfm`).
    - Syntax highlighting for code blocks (via `react-syntax-highlighter`).
    - Relative image URL mapping to the backend storage.
- **`MarkdownToolbar`**: A reusable toolbar component that accepts a `textareaRef` and content update callbacks.

## Design Standards
- **Icons**: Use `lucide-react` for all editor actions.
- **Colors**: Subtle greys and indigo accents for a professional look.
- **Responsiveness**: The editor and preview must adapt to different screen sizes, maintaining usability on tablets.
- **Feedback**: Provide loading indicators for image uploads and smooth transitions between modes.

## Technical Integrity
- Maintain **English** nomenclature for all code, components, and comments.
- Adhere to **Clean Architecture** by separating UI components from the rendering logic.
- Ensure **accessibility** (ARIA labels for icons/buttons).
