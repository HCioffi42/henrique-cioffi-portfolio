# Specification: Media Handling & Image Upload

## Feature Overview
The goal of this track is to implement image upload functionality to support Markdown articles. The system will allow administrators to upload images from the article creation/edition pages and automatically insert the Markdown syntax into the editor.

## Backend Specification

### Storage Abstraction: `IStorageService`
- **Location**: `MeuSitePessoal.Application/Interfaces/IStorageService.cs`
- **Methods**:
  - `Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)`: Saves a file and returns its relative or absolute URL.

### Implementation: `LocalStorageService`
- **Location**: `MeuSitePessoal.Infrastructure/Services/LocalStorageService.cs`
- **Logic**:
  - Saves files to `wwwroot/uploads`.
  - Generates unique filenames (e.g., `GUID_originalName` or `timestamp_originalName`) to prevent collisions.
  - Ensures the directory exists before saving.
  - Returns the relative path (e.g., `/uploads/filename.jpg`).

### Endpoint: `POST /api/Images/upload`
- **Controller**: `ImageController`
- **Authentication**: `[Authorize]`
- **Request**: `IFormFile file`
- **Validation**:
  - File is not null and has length > 0.
  - File type is an image (e.g., `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`).
  - File size is within a reasonable limit (e.g., 5MB).
- **Response**: `200 OK` with the URL of the uploaded image.

## Frontend Specification

### Service: `imageService.ts`
- **Location**: `meupessoal-web/src/services/imageService.ts`
- **Methods**:
  - `uploadImage(file: File): Promise<string>`: Sends a `multipart/form-data` request to `/api/Images/upload`.

### UI Integration: `CreateArticle` & `EditArticle`
- **Location**: `meupessoal-web/src/pages/CreateArticle.tsx`, `meupessoal-web/src/pages/EditArticle.tsx`
- **Changes**:
  - Add an "Upload Image" button (with an icon) near the Markdown textarea.
  - Trigger a hidden file input when clicked.
  - Show a loading state during upload.
  - Upon success, append `![Image Description](url)` to the current position in the Markdown editor.

## Architectural Integrity
- Clean separation between storage abstraction and concrete implementation.
- Adheres to SOLID principles.
- Professional naming and English documentation/comments.
- Secure file handling (filename sanitization and type validation).
