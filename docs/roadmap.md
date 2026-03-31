# Project Roadmap: MeuSitePessoal

**Current Milestone**: Phase 4: Scalability & Engagement
**Global Progress**: [██████████░░] 85%

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

## Phase 2: Content Management & Admin UX - COMPLETED

### [x] Track 05: Article CRUD (Edit & Delete)
- **5.1**: Implement `PUT /api/artigos/{id}` and `DELETE /api/artigos/{id}`.
- **5.2**: Create the `EditArtigo` page, reusing the form component.
- **5.3**: Admin Dashboard table with "Edit" and "Delete" actions.
- **5.4**: Delete confirmation modal.

### [x] Track 06: Media Handling & Image Upload
- **6.1**: `ImageController` for `IFormFile` handling.
- **6.2**: Local storage logic or AWS S3 integration.
- **6.3**: Upload button returning URL to the Markdown editor.

### [x] Track 07: Interactive Markdown Editor
- **7.1**: Toolbar for Bold, Italic, Titles, and Lists.
- **7.2**: Text selection manipulation logic (wrap selection with tags).
- **7.3**: Live Preview toggle.

### [x] Track 08: Tag & Category Management
- **8.1**: Advanced MediatR filtering logic for tags.
- **8.2**: Dynamic routing `/tags/:tag` for filtered discovery.

---

## Phase 3: Infrastructure, Polish & Deployment - COMPLETED

### [x] Track 09: Global Systems (Logging & Toasts)
- **9.1**: Integration with `Infrastructure.Logging`.
- **9.2**: Global Notification System (e.g., `react-hot-toast`).

### [x] Track 10: SEO & Metadata
- **10.1**: `react-helmet-async` for dynamic Meta Tags.
- **10.2**: Open Graph (OG) tags for social media.

### [x] Track 11: CI/CD & Production
- [x] **11.1**: GitHub Actions for Build & Test
    * Validation of .NET Backend (96 integration/unit tests passed).
    * Validation of React Frontend (Lint and Build checks).
    * PostgreSQL service container integration for isolated testing.
- [x] **11.2**: Deployment to VPS (Docker)
    * Automated delivery to production environment via GitHub Actions and SSH.
    * Configuration of Docker containers (API, Web and Database) with Nginx as a reverse proxy.
    * Implementation of dynamic versioning (v2026.03.30.03) in the UI footer.

### [x] Track 12: Custom Domain & Brand Identity
- [x] **12.1**: Registry of the Domain `hcioffi.dev`.
- [x] **12.2**: DNS Configuration on Cloudflare.
- [x] **12.3**: Nginx configurations to recognize the Domain and HTTPS.

---

## Phase 4: Scalability & Engagement - IN PROGRESS

### [ ] Track 13: Advanced Search & Discovery
- **13.1**: Implement Full-Text Search in PostgreSQL using `tsvector`.
- **13.2**: Frontend search bar with debounce logic to optimize API calls.
- **13.3**: "Related Articles" recommendation system based on shared tags.

### [ ] Track 14: Observability & Resilience
- **14.1**: ASP.NET Core Health Checks implementation (Endpoint: `/health`).
- **14.2**: Caching layer for article summaries using In-Memory or Redis.
- **14.3**: Structured logging enrichment with Serilog for production tracing.

### [ ] Track 15: Portfolio Polish & Social
- **15.1**: Dark Mode support using Tailwind CSS strategy.
- **15.2**: Newsletter subscription form with automated email integration.
- **15.3**: Contact form with server-side validation and notifications.

---

## Phase 5: SDET Excellence (Quality Engineering)

### [ ] Track 16: Automated Quality Suite
- **16.1**: End-to-End (E2E) testing suite with **Playwright** for critical flows.
- **16.2**: Automated accessibility (A11y) audits in the CI pipeline.
- **16.3**: Performance benchmarking using **k6** for API endpoints.

---

## Agent Strategy: Outsourcing to Conductor

| Complexity | Task Type | Strategy |
| :--- | :--- | :--- |
| **Low** | DTOs, CRUD UI, Basic Components, SEO Tags | **Full Outsourcing**: Let the agent generate everything. |
| **Medium** | Auth Logic, Media Controllers, CI/CD YAML, HealthChecks | **Supervised**: Generate first, but perform a manual security audit. |
| **High** | Editor Logic, Logging Integration, E2E Test Scenarios | **Manual/Collaborative**: Write core logic manually; use agent for boilerplate. |