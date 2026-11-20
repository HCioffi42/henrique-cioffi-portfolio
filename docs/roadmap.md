# Project Roadmap: MeuSitePessoal

**Current Milestone**: Phase 5: SDET Excellence (Quality Engineering)
**Global Progress**: [██████████████████░░] 90% — Phase 4 reopened pending audit remediation

> **2026-11-19 — Roadmap rewritten.** A full technical audit (`docs/TECHNICAL_AUDIT_REPORT.md`) found that Track 17 shipped with an unresolved regression (newsletter emails render blank) and that the platform currently has no rate limiting or login lockout. Phase 4 is marked reopened rather than completed until Track 17.5 and Track 18.0 close those gaps. See the audit report for full findings, file:line citations, and severity ratings — this roadmap only tracks the remediation work itself.
>
> The `conductor/` agent-outsourcing workflow and `docs/tracks/` per-track spec folder have been retired as of this rewrite (see "Retired Tooling" at the bottom). Going forward, a separate automation-framework repository will own E2E, API, and contract test specs against this repo's surface — this roadmap stays scoped to what ships inside *this* repo.

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
    * Validation of .NET Backend (178 integration/unit tests passed).
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

## Phase 4: Scalability & Engagement - REOPENED (pending remediation)

### [✓] Track 13: Advanced Search & Discovery
- [x] **13.1**: Implement Full-Text Search in PostgreSQL using `tsvector`.
- [x] **13.2**: Frontend search bar with debounce logic to optimize API calls.
- [x] **13.3**: "Related Articles" recommendation system based on shared tags.
- [!] **Known regression (audit P1/P9)**: the full-text search GIN index no longer matches the live query (dead index, sequential scan on every search); the debounce hook exists but is never wired into the search input. Fixed in Track 17.5 / 19.

### [✓] Track 14: Observability & Resilience
- [x] **14.1**: ASP.NET Core Health Checks implementation (Endpoint: `/health`).
- [x] **14.2**: Caching layer for article summaries using In-Memory or Redis.
- [x] **14.3**: Structured logging enrichment with Serilog for production tracing.
- [!] **Known gap (audit S3)**: "Resilience" here covers logging/health checks only — there is no rate limiting or login lockout anywhere in the API yet. Closed in Track 18.0.

### [✓] Track 15: Portfolio Polish & Social
- [x] **15.1 - Sticky Footer & Padding**: Refactor Layout.tsx to fix the footer on the page or to make the container have a "always in viewport.
- [x] **15.2 - Sticky Markdown Toolbar**: Adjsut the article writing components. The tool bar should have a Stiky top behaviour, keeping it in screen view while the user scroll when writing a text.
- [x] **15.3 - Dark Mode (Tailwind)**: Dark Mode support using Tailwind CSS strategy and `localStorage` persistence.
- [x] **15.4 - Header Standardization**: Fix dimensions for Search and Theme buttons to ensure visual symmetry and consistent "square" aspect ratios.
- [x] **15.5 - Accessibility Toggle**: Replace the simple icon-only button with a proper Switch/Toggle component including descriptive labels for better accessibility.
- [x] **15.6 - Mobile Navigation**: Implement a "Hamburger" (Sandwich) menu for smaller viewports to declutter the header.

### [✓] Track 16: Engagement & Community
- [x] **16.1 - Newsletter System**: Email subscription management featuring idempotency validation to prevent duplicate entries.
- [x] **16.2 - Comment Section**: Implementation of a nested comment system (threaded discussions) located below article content.
- [x] **16.3 - Identity Expansion**: Identity framework adjustments to support dedicated reader profiles and integration with external authentication providers (OIDC/OAuth2).
- [x] **16.4 - Access Control & RBAC**: Enforcement of role-based authorization using JWT claims to restrict CRUD operations to Admins while limiting Readers to social interactions (comments and replies).
- [x] **16.5 - Email Infrastructure & Identity Verification**:
	- [x] **16.5.1 - Mail Engine Strategy**: Integration with SMTP/SaaS providers (SendGrid/AWS SES) and implementation of the MailKit provider.
	- [x] **16.5.2 - Account Lifecycle Verification**: Email confirmation flows for new registrations and secure password recovery systems.
	- [x] **16.5.3 - Newsletter Double Opt-in**: Automated subscription verification and legal-compliant unsubscribe mechanisms.
	- [x] **16.5.4 - Email Templates & Branding**: Development of responsive HTML templates with the blog's visual identity.
	- [x] **16.5.5 - Confirmation UI**: Frontend implementation of verification pages for account and newsletter flows using React and Tailwind CSS.
- [x] **16.6 - Post-Registration & Newsletter Automation**:
    - [x] **16.6.1 - "Check Email" UI**: Seamless feedback after registration.
    - [x] **16.6.2 - Content Dispatcher**: Automated newsletter emails when new articles are published via Domain Events.
    - [x] **16.6.3 - Secure Unsubscribe**: Legal-compliant, token-based deactivation mechanism.
- [!] **Known gaps (audit T1)**: two-factor verification and newsletter double opt-in confirmation (16.5.3) have zero test coverage at any level. Closed in Track 18.1.

### [✗→!] Track 17: Internationalization — shipped with an unresolved regression
- [x] **17.1 - i18next Setup**: Configure the translation framework with lazy loading and browser detection.
- [x] **17.2 - Language Toggle**: Add a responsive EN/PT selector to the header with persistence.
- [x] **17.3 - Multi-language Schema**: Adjust the PostgreSQL schema to handle translated content (titles, summaries, and tags).
- [x] **17.4 - Dashboard & UI localization**: Side-by-side editing and static string translation across the platform.
- [!] **Regression introduced**: the "Flat Schema" migration cut legacy columns (`Title`/`Content`/`Summary`) over to translated columns (`TitleEn/TitlePt`, etc.) but the sync from legacy → translated was a one-time backfill, not a standing mechanism — and was explicitly disabled for new/updated records. Two live consumers (newsletter email dispatch, email template) still read the now-permanently-empty legacy fields. **Every newsletter email sent since this track shipped has a blank subject and blank body headline/summary.** Tracked as Track 17.5 below.

### Track 17.5: i18n Flat Schema Cleanup & Newsletter Fix — **NEW, blocks Track 18**
- [ ] **17.5.1**: Fix `ArticlePublishedEventHandler` and the `NewPostNotification` email template to read `TitleEn/TitlePt`/`SummaryEn/SummaryPt` instead of the empty legacy fields.
- [ ] **17.5.2**: Decide the legacy-column endgame — drop `Title`/`Content`/`Summary` (and their now-dead full-text index) in a migration, or document why they're retained and implement the standing sync the current code claims not to need.
- [ ] **17.5.3**: Rebuild the full-text search GIN index to match the live query against the translated columns (currently a dead index causing a sequential scan on every search — audit P1).
- [ ] **17.5.4**: Add a regression test asserting newsletter emails render non-blank subject/body for a freshly published article, and a test covering legacy-only (pre-migration) data through every read path.

---

## Phase 5: SDET Excellence (Quality Engineering) - IN PROGRESS

### Track 18.0: Critical Security Hardening — **NEW, before any shared-environment E2E run**
- [ ] **18.0.1**: Move the JWT signing key out of `appsettings.json` into an environment variable / secret manager, and wire it into the production `docker-compose.yml` override (currently silently falls back to the committed secret in production — audit S1).
- [ ] **18.0.2**: Rotate the database credential found in git history, regardless of current usage (audit S2).
- [ ] **18.0.3**: Re-enable ASP.NET Identity lockout on failed logins (`lockoutOnFailure: true`) and add rate limiting to `/login`, `/register`, `/forgot-password`, comment creation, and newsletter subscription (audit S3).
- [ ] **18.0.4**: Restrict `ImagesController` to Admin-only, or add ownership checks before delete (audit S5 — currently any authenticated Reader can delete any image on the server by URL).

### Track 18.1: Test Suite Foundations — **NEW, parallel with E2E build-out**
- [ ] **18.1.1**: Replace the per-test full DB drop/migrate/reseed cycle with a respawn/transaction-rollback strategy to cut integration suite runtime (audit T2).
- [ ] **18.1.2**: Fix the CI/test connection-string mismatch so CI's env var is actually load-bearing, and remove the hardcoded test password from source (audit T3/T4).
- [ ] **18.1.3**: Stop the live outbound SMTP integration test from depending on third-party network access in CI (audit T5).
- [ ] **18.1.4**: Add handler-level tests for two-factor verification and newsletter double opt-in confirmation — currently zero coverage at any level (audit T1).

### Track 18.2: Automated Quality Suite (original Track 18)
- [ ] **18.2.1**: End-to-End (E2E) testing suite with **Playwright** for critical flows.
- [ ] **18.2.2**: Automated accessibility (A11y) audits in the CI pipeline.
- [ ] **18.2.3**: Performance benchmarking using **k6** for API endpoints.
- [ ] **18.2.4**: API/contract specifications for this repo's surface (endpoints, DTOs, auth/RBAC matrix), written for consumption by the separate automation-framework repository described below.

> **Sequencing note**: 18.0 and 17.5 are hard blockers for 18.2 — an E2E suite built before they land would either encode the newsletter/search bugs as expected behavior, or run against a forgeable Admin JWT with no login throttling. 18.1 can run in parallel with the start of 18.2 since it strengthens the foundation Playwright will also need fixtures from, but doesn't block starting the Playwright setup itself.

---

## Track 19: Architecture Consolidation (post-Phase 5, non-blocking)

- [ ] **19.1**: Unify Article data access behind `IArticleRepository` consistently (currently split between the repository and direct `DbSet<Article>` access); add an equivalent repository for Comments, which currently has none (audit A5).
- [ ] **19.2**: Extract the duplicated "localized Markdown field" block in `CreateArticle.tsx`/`EditArticle.tsx` into a shared component + `useArticleForm` hook (audit A9).
- [ ] **19.3**: Move `AuthController.UpdateLanguage`/`ExternalLoginCallback` business logic into proper MediatR commands with validation (audit A7).
- [ ] **19.4**: Centralize the category-label mapping (audit A10) and memoize `ThemeProvider`'s context value (audit A11).
- [ ] **19.5**: Route-level code splitting (`React.lazy`) to isolate the admin dashboard/editor bundle from the public reading experience, and switch to the light/async `react-syntax-highlighter` build (audit P4/P5).
- [ ] **19.6**: Memoize the recursive `CommentItem` tree and stop replacing the full comment array on every mutation (audit P6).

---

## Relationship to the Separate Automation Framework

A dedicated repository will own **E2E (Playwright), API, and contract tests** exercising this application from the outside. This repo's roadmap stays scoped to what ships inside it:

- This repo is responsible for: unit tests, integration tests (MediatR handlers, controllers via `WebApplicationFactory`), and — per Track 18.2.4 — the API/contract specs that describe its own stable surface.
- The automation-framework repo is responsible for: consuming those specs, driving the deployed app through real HTTP/browser interactions, and maintaining its own test-authoring conventions independent of this codebase's release cadence.
- Specs for "what needs fixing" are **not** written as formal contracts — they live as roadmap tracks (above) and the audit report, and only become specs once the corresponding fix has actually landed, so the automation framework is never built against intentionally-temporary broken behavior.

---

## Retired Tooling

- **`conductor/` folder (agent-outsourcing workflow) — removed 2026-11-19.** It had grown past the point of being useful and was creating more overhead than value; its only cross-reference was the "Agent Strategy" table that lived in this file, which is dropped in this rewrite.
- **`docs/tracks/` folder (per-track spec files) — removed 2026-11-19.** Superseded by this roadmap's inline track descriptions and `docs/TECHNICAL_AUDIT_REPORT.md` for anything requiring deeper detail. Future per-feature specs that need this level of depth belong in the separate automation-framework repo once a track is actually in flight, not pre-written here.
