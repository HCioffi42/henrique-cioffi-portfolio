# Project Roadmap: MeuSitePessoal

**Current Milestone**: Phase 5: SDET Excellence (Quality Engineering)
**Global Progress**: [███████████████████░] 96% — Phase 4 remediation complete; Track 18.0 (security hardening) closed. Tracks 18.1 and 18.2 are the open work in Phase 5.

> **2026-11-19 — Roadmap rewritten.** A full technical audit (`docs/TECHNICAL_AUDIT_REPORT.md`) found that Track 17 shipped with an unresolved regression (newsletter emails render blank) and that the platform currently has no rate limiting or login lockout. Phase 4 was marked reopened rather than completed until those gaps closed. See the audit report for full findings, file:line citations, and severity ratings — this roadmap only tracks the remediation work itself.
>
> **2026-11-20 — Track 17.5 completed.** The newsletter regression and the dead search index are fixed, and the legacy columns are gone from both code and database. Track 18.0 (security hardening) is now the only remaining blocker for Track 18.2.
>
> **2026-11-20 — Track 18.0 completed.** The signing key is sourced from the environment with fail-fast startup validation, ingress is narrowed to the web tier, the client address is resolved from the forwarded chain, per-endpoint request throttling is active, Identity lockout is enabled with an administrative exemption, and media management is restricted to administrators. Security findings are now tracked privately rather than enumerated in this repository. Tracks 18.1 and 18.2 are unblocked.
>
> **2026-11-20 — Track 18.0 verified against production.** The out-of-band acceptance checks were carried out on the deployed system. The administrative lockout exemption was found not to apply there, because the convergence step sat inside the block guarded by the `AdminSetup` credentials that a deployed environment does not supply; `v2026.11.20.03` moved it outside that guard and keyed it on the `Admin` role. The forwarded-address range and conditional header trust both passed.
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

## Phase 4: Scalability & Engagement - COMPLETED (Track 17.5 remediation closed 2026-11-20)

### [✓] Track 13: Advanced Search & Discovery
- [x] **13.1**: Implement Full-Text Search in PostgreSQL using `tsvector`.
- [x] **13.2**: Frontend search bar with debounce logic to optimize API calls.
- [x] **13.3**: "Related Articles" recommendation system based on shared tags.
- [x] **Audit P1 — resolved in Track 17.5**: the dead GIN index was replaced by one index per language dictionary, verified in use via `EXPLAIN (ANALYZE, BUFFERS)`.
- [!] **Known gap (audit P9)**: the debounce hook exists but is never wired into the search input, so there is still no type-ahead search. Open; tracked in Track 19.

### [✓] Track 14: Observability & Resilience
- [x] **14.1**: ASP.NET Core Health Checks implementation (Endpoint: `/health`).
- [x] **14.2**: Caching layer for article summaries using In-Memory or Redis.
- [x] **14.3**: Structured logging enrichment with Serilog for production tracing.
- [x] **Resilience follow-up (audit S3)**: request throttling and sign-in lockout were added in Track 18.0, extending this track's logging and health-check coverage.

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
- [!] **Known gaps (audit T1)**: newsletter double opt-in confirmation (16.5.3) has zero test coverage at any level. Closed in Track 18.1. The other half of T1, two-factor verification, was closed by removing the feature rather than by testing it (see 18.1.5).

### [✓] Track 17: Internationalization
- [x] **17.1 - i18next Setup**: Configure the translation framework with lazy loading and browser detection.
- [x] **17.2 - Language Toggle**: Add a responsive EN/PT selector to the header with persistence.
- [x] **17.3 - Multi-language Schema**: Adjust the PostgreSQL schema to handle translated content (titles, summaries, and tags).
- [x] **17.4 - Dashboard & UI localization**: Side-by-side editing and static string translation across the platform.
- [x] **Regression closed by Track 17.5** (2026-11-20): the "Flat Schema" migration left `Title`/`Content`/`Summary` permanently empty while the newsletter dispatcher and email template still read them, so every notification shipped with a blank subject and body. The legacy columns are now gone and all consumers read the localized columns.

### [✓] Track 17.5: i18n Flat Schema Cleanup & Newsletter Fix — COMPLETED 2026-11-20
Spec: `docs/specs/track-17.5-i18n-and-newsletter.md` · Plan: `docs/plans/track-17.5-implementation-plan.md`
- [x] **17.5.1**: `ArticlePublishedEventHandler` and the `NewPostNotification` template render both languages from `TitleEn/TitlePt` and `SummaryEn/SummaryPt`, behind a dedicated `NewPostNotificationViewModel` that owns the subject and block-visibility rules.
- [x] **17.5.2**: Legacy `Title`/`Content`/`Summary` dropped from the entity, EF mapping, AutoMapper profile, and database (`RemoveLegacyArticleColumns`), together with the obsolete single-language constructor. Preceded by a verified production backup and a `rows_at_risk = 0` data audit.
- [x] **17.5.3**: Replaced the dead index with `idx_articles_search_en` and `idx_articles_search_pt`, one per dictionary; `EXPLAIN (ANALYZE, BUFFERS)` over 20001 rows confirms a Bitmap Index Scan on each (audit P1 closed).
- [x] **17.5.4**: Regression coverage added — newsletter subject/body content assertions, real-template rendering, the identical-titles shape every pre-migration row carries, and `Accept-Language` region/case variants.
- **Behavior change to note**: search is now scoped to the reader's active language, so a Portuguese reader no longer matches English-only text. This follows from per-language dictionaries and is intentional.

---

## Phase 5: SDET Excellence (Quality Engineering) - IN PROGRESS

### Track 18.0: Critical Security Hardening — **NEW, before any shared-environment E2E run**
- [x] **18.0.1**: JWT signing key sourced from the environment, with fail-fast startup
  validation and the production value supplied through `docker-compose.yml` (audit S1).
- [x] **18.0.2**: Database credential rotated (audit S2). Completed operationally against
  production on 2026-11-20, not by a code change in this track.
- [x] **18.0.3**: Proxy-aware client address resolution, per-endpoint request rate limiting,
  and Identity lockout with an administrative exemption (audit S3).
- [x] **18.0.4**: Media upload and deletion restricted to administrators (audit S5).
- [x] **18.0.5 — Deployed verification**: the three acceptance checks a green suite cannot confirm were carried out against production on 2026-11-20. The forwarded-address range and conditional header trust both passed; the administrative lockout exemption did not apply in the deployed environment and was closed by `v2026.11.20.03`. Results and evidence are recorded in `docs/plans/track-18.0-implementation-plan.md` under Verification Summary.

### Track 18.1: Test Suite Foundations — **NEW, parallel with E2E build-out**
- [ ] **18.1.1**: Replace the per-test full DB drop/migrate/reseed cycle with a respawn/transaction-rollback strategy to cut integration suite runtime (audit T2).
- [x] **18.1.2**: Fix the CI/test connection-string mismatch so CI's env var is actually load-bearing, and remove the hardcoded test password from source (audit T3/T4). The suite resolves `ConnectionStrings:DefaultConnection` from user secrets then environment variables, with no fallback — an unconfigured run fails immediately with the `dotnet user-secrets set` command to run. CI's service container and its connection string now name the same database, and that string carries no credential.
- [ ] **18.1.3**: Stop the live outbound SMTP integration test from depending on third-party network access in CI (audit T5).
- [ ] **18.1.4**: Add handler-level tests for newsletter double opt-in confirmation — currently zero coverage at any level (audit T1).
- [x] **18.1.5**: Remove Two-Factor Authentication. The flow was never enabled (`FeatureToggles:Enable2FA` shipped `false`), and a portfolio blog does not justify the reader friction or the maintenance surface of a TOTP enrolment, recovery-code and reset path. The `verify-2fa` endpoint, its command and handler, the `RequiresTwoFactor` field on the login response, the feature-toggle service that existed only to gate it, and the frontend verification view are all gone. The `auth-verify` rate-limit policy is retained for `reset-password`.

### Track 18.2: Automated Quality Suite (original Track 18)
- [ ] **18.2.1**: End-to-End (E2E) testing suite with **Playwright** for critical flows.
- [ ] **18.2.2**: Automated accessibility (A11y) audits in the CI pipeline.
- [ ] **18.2.3**: Performance benchmarking using **k6** for API endpoints.
    - [ ] **18.2.3.1**: Author k6 load-test scripts (JS) under `tests/load/`, covering the hottest read paths (article listing, search, article detail).
    - [ ] **18.2.3.2**: Define pass/fail **thresholds** per script (e.g. p95 latency, error rate) so a regression fails the run instead of just reporting a number.
    - [ ] **18.2.3.3**: Add a separate, on-demand/scheduled GitHub Actions workflow (not on every PR) using `grafana/k6-action`, run against a staging target or the CI-spun-up API+Postgres containers.
    - [ ] **18.2.3.4**: Keep output lightweight for a project this size — CLI summary + CI artifact is enough; skip a hosted dashboard (Grafana Cloud k6, etc.) unless traffic/need grows.
- [ ] **18.2.4**: API/contract specifications for this repo's surface (endpoints, DTOs, auth/RBAC matrix), written for consumption by the separate automation-framework repository described below.

> **Sequencing note**: 17.5 and 18.0 were hard blockers for 18.2 and both have landed, so 18.2 is clear to start. 18.1 can run in parallel with it, since it strengthens the fixtures Playwright will also need but does not block the Playwright setup itself.

> **Threshold note for 18.2**: a browser suite authenticates from a single client address and therefore shares one rate-limit partition. Production thresholds (`auth-signin`: 10 per 15 minutes) are exhausted within ten sign-ins. The environment the suite runs against must declare its own `RateLimiting` configuration; thresholds bind from configuration for exactly this reason.

---

## Track 19: Architecture Consolidation (post-Phase 5, non-blocking)

- [ ] **19.1**: Unify Article data access behind `IArticleRepository` consistently (currently split between the repository and direct `DbSet<Article>` access); add an equivalent repository for Comments, which currently has none (audit A5).
- [ ] **19.2**: Extract the duplicated "localized Markdown field" block in `CreateArticle.tsx`/`EditArticle.tsx` into a shared component + `useArticleForm` hook (audit A9).
- [ ] **19.3**: Move `AuthController.UpdateLanguage`/`ExternalLoginCallback` business logic into proper MediatR commands with validation (audit A7).
- [ ] **19.4**: Centralize the category-label mapping (audit A10) and memoize `ThemeProvider`'s context value (audit A11).
- [ ] **19.5**: Route-level code splitting (`React.lazy`) to isolate the admin dashboard/editor bundle from the public reading experience, and switch to the light/async `react-syntax-highlighter` build (audit P4/P5).
- [ ] **19.6**: Memoize the recursive `CommentItem` tree and stop replacing the full comment array on every mutation (audit P6).
- [ ] **19.7**: Promote nullable-reference warnings to build errors (`TreatWarningsAsErrors`, or scoped to the nullable warning codes) across all `.csproj` files — `Nullable` is enabled everywhere already, but nothing currently fails the build when a nullable warning is introduced.
- [ ] **19.8**: Capture `PreferredLanguage` at newsletter signup (frontend sends the active i18next language; `SubscribeToNewsletterCommand` accepts and persists it) and switch the article notification from a single bilingual email to a per-subscriber localized one. Deferred from Track 17.5: the column exists but is populated for no subscriber, so localizing per recipient today would deliver English to everyone.
- [ ] **19.9**: Make Portuguese search accent-insensitive. Found while verifying Track 17.5: the `portuguese` dictionary stems `programação` to `program` but `programacao` to `programaca`, so a reader typing without accents — common on mobile and ASCII keyboards — gets zero results for accented articles. The current behavior is pinned by `Search_WithUnaccentedPortugueseTerm_DoesNotMatchAccentedText`. The fix is `CREATE EXTENSION unaccent` plus an `unaccent`-wrapped tsvector; note the GIN index DDL and `ArticleSearchService` must change together or the index stops being used, and `unaccent` must be schema-qualified to stay IMMUTABLE for indexing.
- [ ] **19.10**: Restrict origin ingress on 80/443 to the CDN's published address ranges, so the forwarded-address chain cannot be presented by a caller reaching the origin directly. Recorded during Track 18.0 as residual scope (spec §4.4).
- [ ] **19.11**: Add edge throttling (CDN rules or nginx `limit_req`) as a complement to the in-application limiter, rejecting excess traffic before it reaches the origin. Deferred from Track 18.0 so one enforcement point owned the behavior first.

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
