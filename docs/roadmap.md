# Project Roadmap: MeuSitePessoal

**Current Milestone**: Phase 4: Scalability & Engagement
**Global Progress**: [█████████████████░░░] 88%

---

## Phase 1: Foundation (Core Features) - COMPLETED

### [✓] Track 01: Initial Setup & Architecture
- [x] **1.1**: Clean Architecture setup (.NET + React).
- [x] **1.2**: Basic MediatR & Entity Framework Core integration.

### [✓] Track 02: Artigos CRUD (Backend)
- [x] **2.1**: Implementation of Create and Get (List/Detail) handlers.
- [x] **2.2**: SQLite/PostgreSQL configuration for testing and dev.

### [✓] Track 03: Frontend Basics (React + Tailwind)
- [x] **3.1**: Responsive grid layout for home page.
- [x] **3.2**: Markdown rendering support.

### [✓] Track 04: Auth & Protected Routes
- [x] **4.1**: ASP.NET Core Identity & JWT Token Service.
- [x] **4.2**: React Context API (`AuthContext`) & `storage.ts`.
- [x] **4.3**: `ProtectedRoute` and Axios Interceptors (401 handling).

---

## Phase 2: Content Management & Admin UX - COMPLETED

### [✓] Track 05: Article CRUD (Edit & Delete)
- [x] **5.1**: Implement `PUT /api/artigos/{id}` and `DELETE /api/artigos/{id}`.
- [x] **5.2**: Create the `EditArtigo` page, reusing the form component.
- [x] **5.3**: Admin Dashboard table with "Edit" and "Delete" actions.
- [x] **5.4**: Delete confirmation modal.

### [✓] Track 06: Media Handling & Image Upload
- [x] **6.1**: `ImageController` for `IFormFile` handling.
- [x] **6.2**: Local storage logic or AWS S3 integration.
- [x] **6.3**: Upload button returning URL to the Markdown editor.

### [✓] Track 07: Interactive Markdown Editor
- [x] **7.1**: Toolbar for Bold, Italic, Titles, and Lists.
- [x] **7.2**: Text selection manipulation logic (wrap selection with tags).
- [x] **7.3**: Live Preview toggle.

### [✓] Track 08: Tag & Category Management
- [x] **8.1**: Advanced MediatR filtering logic for tags.
- [x] **8.2**: Dynamic routing `/tags/:tag` for filtered discovery.

---

## Phase 3: Infrastructure, Polish & Deployment - COMPLETED

### [✓] Track 09: Global Systems (Logging & Toasts)
- [x] **9.1**: Integration with `Infrastructure.Logging`.
- [x] **9.2**: Global Notification System (e.g., `react-hot-toast`).

### [✓] Track 10: SEO & Metadata
- [x] **10.1**: `react-helmet-async` for dynamic Meta Tags.
- [x] **10.2**: Open Graph (OG) tags for social media.

### [✓] Track 11: CI/CD & Production
- [x] **11.1**: GitHub Actions for Build & Test
    * Validation of .NET Backend (96 integration/unit tests passed).
    * Validation of React Frontend (Lint and Build checks).
    * PostgreSQL service container integration for isolated testing.
- [x] **11.2**: Deployment to VPS (Docker)
    * Automated delivery to production environment via GitHub Actions and SSH.
    * Configuration of Docker containers (API, Web and Database) with Nginx as a reverse proxy.
    * Implementation of dynamic versioning (v2026.03.30.03) in the UI footer.

### [✓] Track 12: Custom Domain & Brand Identity
- [x] **12.1**: Registry of the Domain `hcioffi.dev`.
- [x] **12.2**: DNS Configuration on Cloudflare.
- [x] **12.3**: Nginx configurations to recognize the Domain and HTTPS.

---

## Phase 4: Scalability & Engagement - IN PROGRESS

### [✓] Track 13: Advanced Search & Discovery
- [x] **13.1**: Implement Full-Text Search in PostgreSQL using `tsvector`.
- [x] **13.2**: Frontend search bar with debounce logic to optimize API calls.
- [x] **13.3**: "Related Articles" recommendation system based on shared tags.

### [✓] Track 14: Observability & Resilience
- [x] **14.1**: ASP.NET Core Health Checks implementation (Endpoint: `/health`).
- [x] **14.2**: Caching layer for article summaries using In-Memory or Redis.
- [x] **14.3**: Structured logging enrichment with Serilog for production tracing.

### [ ] Track 15: Portfolio Polish & Social
- [ ] **15.1 - Sticky Footer & Padding**: Refactor Layout.tsx to fix the footer on the page or to make the container have a "always in viewport.
- [ ] **15.2 - Sticky Markdown Toolbar**: Adjsut the article writing components. The tool bar should have a Stiky top behaviour, keeping it in screen view while the user scroll when writing a text.
- [ ] **15.3 - Dark Mode (Tailwind)**: Dark Mode support using Tailwind CSS strategy and `localStorage` persistence.

### [ ] Track 16: Engagement & Community
- [ ] **16.1 - Newsletter System**: Email subscription management featuring idempotency validation to prevent duplicate entries.
- [ ] **16.2 - Comment Section**: Implementation of a nested comment system (threaded discussions) located below article content.
- [ ] **16.3 - Identity Expansion**: Identity framework adjustments to support dedicated reader profiles and integration with external authentication providers (OIDC/OAuth2).
---

## Phase 5: SDET Excellence (Quality Engineering)

### [ ] Track 17: Automated Quality Suite
- [ ] **17.1**: End-to-End (E2E) testing suite with **Playwright** for critical flows.
- [ ] **17.2**: Automated accessibility (A11y) audits in the CI pipeline.
- [ ] **17.3**: Performance benchmarking using **k6** for API endpoints.

---

## Agent Strategy: Outsourcing to Conductor

| Complexity | Task Type | Strategy |
| :--- | :--- | :--- |
| **Low** | DTOs, CRUD UI, Basic Components, SEO Tags | **Full Outsourcing**: Let the agent generate everything. |
| **Medium** | Auth Logic, Media Controllers, CI/CD YAML, HealthChecks | **Supervised**: Generate first, but perform a manual security audit. |
| **High** | Editor Logic, Logging Integration, E2E Test Scenarios | **Manual/Collaborative**: Write core logic manually; use agent for boilerplate. |