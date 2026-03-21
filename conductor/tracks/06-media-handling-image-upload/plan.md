# Plan: Implement Media Handling & Image Upload

This track implements the storage abstraction, local storage service, and frontend image upload functionality for Markdown articles.

## Backend Implementation Steps

1. **Storage Abstraction**:
   - Create `IStorageService` in `MeuSitePessoal.Application/Interfaces/`.
   - Method: `Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)`.

2. **Local Storage Service**:
   - Implement `LocalStorageService` in `MeuSitePessoal.Infrastructure/Services/`.
   - Configure it to save to `wwwroot/uploads`.
   - Ensure the directory is created if it doesn't exist.
   - Use GUIDs for filename sanitization and to prevent collisions.

3. **Dependency Injection**:
   - Register `IStorageService` and `LocalStorageService` in `MeuSitePessoal.Infrastructure/DependencyInjection.cs` (or wherever registrations are handled).

4. **Image Controller**:
   - Create `ImageController` in `MeuSitePessoal.Api/Controllers/`.
   - Implement `Upload` endpoint:
     - `[Authorize]`.
     - Validate file presence, type (images only), and size.
     - Save via `IStorageService`.
     - Return the relative URL.

5. **Static File Serving**:
   - Ensure `app.UseStaticFiles()` is correctly configured in `Program.cs` to serve the `uploads` directory.

## Frontend Implementation Steps

1. **Image Service**:
   - Create `meupessoal-web/src/services/imageService.ts`.
   - Method: `uploadImage(file: File)`.
   - Handle the `POST` request with `multipart/form-data`.

2. **Create/Edit Article Component Enhancement**:
   - Update `meupessoal-web/src/pages/CreateArticle.tsx` and `meupessoal-web/src/pages/EditArticle.tsx`.
   - Add an "Upload Image" button (using Lucide icons if available).
   - Implement file input handling and upload progress/status.
   - Automatically insert the Markdown image syntax into the current textarea position.

3. **Verification**:
   - Verify image upload from the frontend.
   - Confirm file persistence in `wwwroot/uploads`.
   - Test Markdown rendering with the uploaded image URL.

## Branding & Standards
- Use English for all code, types, and comments.
- Descriptive third-person comments.
- Professional naming conventions.
- Adhere to Clean Architecture and SOLID principles.
