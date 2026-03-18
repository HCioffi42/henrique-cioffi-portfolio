# Project Roadmap: MeuSitePessoal

**Current Milestone**: Phase 2 - Content Management & Admin UX
**Global Progress**: [████░░░░░░] 40%

---

## Phase 1: Foundation (Core Features) - COMPLETED

### [x] Track 01: Initial Setup & Architecture
- Clean Architecture setup (.NET + React).
- Basic MediatR & Entity Framework Core integration.

### [x] Track 02: Artigos CRUD (Backend)
- Implementation of Create and Get (List/Detail) handlers.
- SQLite/PostgreSQL configuration for testing and dev.

### [x] Track 03: Frontend Basics (React + Tailwind)
- Responsive grid layout for home page.
- Markdown rendering support.

### [x] Track 04: Auth & Protected Routes
- **4.1**: ASP.NET Core Identity & JWT Token Service.
- **4.2**: React Context API (`AuthContext`) & `storage.ts`.
- **4.3**: `ProtectedRoute` and Axios Interceptors (401 handling).

---

## Phase 2: Content Management & Admin UX - IN PROGRESS

### [ ] Track 05: Article CRUD (Edit & Delete)
- **5.1**: Implement `PUT /api/artigos/{id}` and `DELETE /api/artigos/{id}`.
- **5.2**: Create the `EditArtigo` page, reusing the form component.
- **5.3**: Admin Dashboard table with "Edit" and "Delete" actions.
- **5.4**: Delete confirmation modal.

### [ ] Track 06: Media Handling & Image Upload
- **10.1**: `ImageController` for `IFormFile` handling.
- **10.2**: Local storage logic or AWS S3 integration.
- **10.3**: Upload button returning URL to the Markdown editor.

### [ ] Track 07: Interactive Markdown Editor
- **6.1**: Toolbar for Bold, Italic, Titles, and Lists.
- **6.2**: Text selection manipulation logic (wrap selection with tags).
- **6.3**: Live Preview toggle.

### [ ] Track 08: Tag & Category Management
- **3.1**: Advanced MediatR filtering logic for tags.
- **3.2**: Dynamic routing `/tags/:tag` for filtered discovery.

---

## Phase 3: Infrastructure, Polish & Deployment

### [ ] Track 09: Global Systems (Logging & Toasts)
- **7.1**: Integration with `Infrastructure.Logging`.
- **8.1**: Global Notification System (e.g., `react-hot-toast`).

### [ ] Track 10: SEO & Metadata
- **9.1**: `react-helmet-async` for dynamic Meta Tags.
- **9.2**: Open Graph (OG) tags for social media.

### [ ] Track 11: CI/CD & Production
- **11.1**: GitHub Actions for Build & Test.
- **11.2**: Deployment to VPS (Docker or Systemd).

---

## Agent Strategy: Outsourcing to Conductor

| Complexity | Task Type | Strategy |
| :--- | :--- | :--- |
| **Low** | DTOs, CRUD UI, Basic Components, SEO Tags | **Full Outsourcing**: Let the agent generate everything. |
| **Medium** | Auth Logic, Media Controllers, CI/CD YAML | **Supervised**: Generate first, but perform a manual security audit. |
| **High** | Editor Logic, Logging Integration, Test Refactoring | **Manual/Collaborative**: Write core logic manually; use agent for boilerplate. |
