# Track 06: Media Handling & Image Upload

## Status: Completed ✅

## Overview
Implemented a secure and efficient image upload system for Markdown articles. Administrators can now upload images directly from the article creation/edition pages, and the corresponding Markdown syntax is automatically inserted into the editor.

## Key Changes

### Backend
- **Storage Abstraction**: Created `IStorageService` in the `Application` layer.
- **Local Storage Implementation**: Implemented `LocalStorageService` in the `Infrastructure` layer, saving files to `wwwroot/uploads`.
- **Filename Sanitization**: Used GUIDs to ensure unique filenames and prevent collisions.
- **Image Controller**: Created `ImagesController` with an `[Authorize]` upload endpoint, including file type and size validation.
- **Static Files**: Configured the API to serve static files from `wwwroot`.

### Frontend
- **Image Service**: Created `imageService.ts` to handle API communication for file uploads.
- **UI Enhancement**: 
    - Added an "Upload Image" button to `CreateArticle` and `EditArticle` pages.
    - Implemented cursor-position insertion for Markdown image syntax: `![original_name](url)`.
    - Added loading states and error handling for the upload process.

## Verification Results
- [x] Storage service correctly saves files to the filesystem.
- [x] Controller validates file types and sizes.
- [x] Frontend successfully uploads images and updates the Markdown editor.
- [x] Uploaded images are accessible via their public URLs.
