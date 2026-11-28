# Project Roadmap: MeuSitePessoal

**Current Milestone**: Phase 5: Pre-Automation Readiness
**Global Progress**: [███████████████░░░░░] 77% — 20 of 26 tracks closed. Phases 1 through 4 are complete. Phase 5 has Track 18.1 closed and Tracks 20 to 23 open. Phase 6 (external quality) starts once Track 23 lands; Track 19 runs alongside, non-blocking.

> **2026-11-19 — Roadmap rewritten.** A full technical audit (`docs/TECHNICAL_AUDIT_REPORT.md`) found that Track 17 shipped with an unresolved regression (newsletter emails render blank) and that the platform currently has no rate limiting or login lockout. Phase 4 was marked reopened rather than completed until those gaps closed. See the audit report for full findings, file:line citations, and severity ratings — this roadmap only tracks the remediation work itself.
>
> **2026-11-20 — Track 17.5 completed.** The newsletter regression and the dead search index are fixed, and the legacy columns are gone from both code and database. Track 18.0 (security hardening) is now the only remaining blocker for Track 18.2.
>
> **2026-11-20 — Track 18.0 completed.** The signing key is sourced from the environment with fail-fast startup validation, ingress is narrowed to the web tier, the client address is resolved from the forwarded chain, per-endpoint request throttling is active, Identity lockout is enabled with an administrative exemption, and media management is restricted to administrators. Security findings are now tracked privately rather than enumerated in this repository. Tracks 18.1 and 18.2 are unblocked.
>
> **2026-11-20 — Track 18.0 verified against production.** The out-of-band acceptance checks were carried out on the deployed system. The administrative lockout exemption was found not to apply there, because the convergence step sat inside the block guarded by the `AdminSetup` credentials that a deployed environment does not supply; `v2026.11.20.03` moved it outside that guard and keyed it on the `Admin` role. The forwarded-address range and conditional header trust both passed.
>
> **2026-11-21 — Track 18.1 completed.** The integration suite builds its schema once per run and clears data with Respawn, cutting runtime from 1m49s to 32s. Its connection string comes from configuration with no credential in source and no fallback, so CI's environment variable is load-bearing rather than decorative. The SMTP tests run against a server hosted in the test process instead of dialing a third party, which also revived a delivery test skipped since it was written. Newsletter double opt-in confirmation has handler tests, and two-factor authentication was removed rather than tested, since it shipped permanently disabled. Audit findings T1 through T5 are closed and the suite reports 273 passing with nothing skipped.
>
> **2026-11-28 — Track 18.0 ingress narrowing reached production.** The 2026-11-20 notes above say ingress is narrowed to the web tier. That held in the repository but not on the server until 2026-11-28: ports 5000 (API) and 5432 (PostgreSQL) stayed published on the host, and port 5000 answered from the internet without passing through Cloudflare. The server checkout had not moved since April and carried a hand-edited `docker-compose.yml`, so `git pull` failed on every deploy. The deploy script had no `set -e` and reported the exit code of its last command, `docker compose up -d`, which succeeded. Images still updated through `docker compose pull`, so application code shipped on every release while compose changes never did. The 2026-11-20 verification (18.0.5) covered the forwarded-address range and header trust, not the published ports, so it could not catch the gap. On 2026-11-28 the server checkout was reset to `dev`, the `docker-compose.override.yml` was removed and the containers were recreated; only port 80 is published now, confirmed from outside. The deploy now runs with `set -e` and moves the checkout with `git fetch` and `git reset --hard` instead of `git pull`, so a failed checkout update fails the deployment. The notes above stay as written: they record what was believed on their date.
>
> **2026-11-28 — Roadmap 2.0.** Phases now group tracks by domain instead of by the order the work happened. Track IDs stay fixed and nobody reuses them: new tracks take the next free number, and a track that changes phase keeps its ID. Track 18.0 moved to Phase 4 on that rule, so the plan, spec, audit and commits that cite it still resolve. Phase 5 now holds what must land before the external suite starts: the CI gate (Track 20), SemVer releases (Track 21), new categories (Track 22) and the reading layout (Track 23). Track 23 ships last so the external suite starts against a frozen DOM. Track 18.2 moved to a new Phase 6 and runs entirely in the separate automation repository, against ephemeral CI environments only.
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

## Phase 4: Scalability, Engagement & Security - COMPLETED (Track 17.5 remediation closed 2026-11-20, Track 18.0 hardening closed 2026-11-20)

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
- [x] **Known gaps (audit T1)**: closed in Track 18.1. Newsletter double opt-in confirmation (16.5.3) is covered by handler tests (18.1.4); the other half of T1, two-factor verification, was closed by removing the feature rather than by testing it (18.1.5).
- [x] **Cross-reference, two-factor authentication**: 16.3 introduced a TOTP flow behind `FeatureToggles:Enable2FA`, which shipped `false` and never reached a reader. Track 18.1.5 removed the flow, its endpoint and its toggle.

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

### [✓] Track 18.0: Critical Security Hardening — COMPLETED 2026-11-20
Spec: `docs/specs/track-18.0-security-hardening.md` · Plan: `docs/plans/track-18.0-implementation-plan.md`

> **Moved from Phase 5 in Roadmap 2.0, ID unchanged.** The track hardened the application itself, which is Phase 4's domain. It sat in Phase 5 because its spec made it a precondition for Track 18.2: a browser suite that exercises authentication repeatedly against a shared environment needs throttling and lockout in place to produce results instead of noise.

- [x] **18.0.1**: JWT signing key sourced from the environment, with fail-fast startup
  validation and the production value supplied through `docker-compose.yml` (audit S1).
- [x] **18.0.2**: Database credential rotated (audit S2). Completed operationally against
  production on 2026-11-20, not by a code change in this track.
- [x] **18.0.3**: Proxy-aware client address resolution, per-endpoint request rate limiting,
  and Identity lockout with an administrative exemption (audit S3).
- [x] **18.0.4**: Media upload and deletion restricted to administrators (audit S5).
- [x] **18.0.5 — Deployed verification**: the three acceptance checks a green suite cannot confirm were carried out against production on 2026-11-20. The forwarded-address range and conditional header trust both passed; the administrative lockout exemption did not apply in the deployed environment and was closed by `v2026.11.20.03`. Results and evidence are recorded in `docs/plans/track-18.0-implementation-plan.md` under Verification Summary.
  **Correction (2026-11-28)**: the ingress narrowing (18.0 Task 2) took effect in production only on 2026-11-28. See the dated note at the top of this roadmap.

---

## Phase 5: Pre-Automation Readiness - IN PROGRESS

Everything the external suite (Phase 6) depends on: a test gate it can trust, releases it can pin to, a stable content model, and a DOM that stops moving. Tracks run in the order listed. Track 23 goes last because it fixes the markup and selectors the external suite will target.

### [✓] Track 18.1: Test Suite Foundations — completed 2026-11-21
- [x] **18.1.1**: Replace the per-test full DB drop/migrate/reseed cycle with a respawn/transaction-rollback strategy to cut integration suite runtime (audit T2). The schema is now built once per run and each test clears data with Respawn instead of dropping the database and replaying every migration. Suite runtime fell from 1m49s to 32s. Reseeding stays per-test, so the production `DbInitializer` path is still exercised. The suite remains sequential: the tests share one database, so `DisableTestParallelization` is still load-bearing and 18.1.1 does not close that on its own.
- [x] **18.1.2**: Fix the CI/test connection-string mismatch so CI's env var is actually load-bearing, and remove the hardcoded test password from source (audit T3/T4). The suite resolves `ConnectionStrings:DefaultConnection` from user secrets then environment variables, with no fallback — an unconfigured run fails immediately with the `dotnet user-secrets set` command to run. CI's service container and its connection string now name the same database, and that string carries no credential.
- [x] **18.1.3**: Stop the live outbound SMTP integration test from depending on third-party network access in CI (audit T5). An SMTP server now runs inside the test process on a loopback port, so connect/authenticate/send is still exercised for real with no network, DNS or provider involved. The delivery test that had been skipped since it was written now runs: the suite reports 268 passing and nothing skipped.
- [x] **18.1.4**: Add handler-level tests for newsletter double opt-in confirmation — previously zero coverage at any level (audit T1). Five tests cover activation, unknown address, rejected token, repeat confirmation, and address-case normalization. Each was checked against a deliberately broken handler to confirm it fails when the behavior it names is removed.
- [x] **18.1.5**: Remove Two-Factor Authentication. The flow was never enabled (`FeatureToggles:Enable2FA` shipped `false`), and a portfolio blog does not justify the reader friction or the maintenance surface of a TOTP enrolment, recovery-code and reset path. The `verify-2fa` endpoint, its command and handler, the `RequiresTwoFactor` field on the login response, the feature-toggle service that existed only to gate it, and the frontend verification view are all gone. The `auth-verify` rate-limit policy is retained for `reset-password`. This also kept the removed endpoint out of the first contract the external suite consumes (18.2.4).

### Track 20: CI Test Gate
`ci.yml` already runs the backend suite against a PostgreSQL service and lints and builds the frontend on every pull request to `dev`. Nothing makes either result binding: the `dev` ruleset requires only `check-branch-name`, and the release workflow builds images without running tests.
- [ ] **20.1**: Add `backend-validation` and `frontend-validation` to the `dev` ruleset's required status checks, next to `check-branch-name`.
- [ ] **20.2**: Decide the owner bypass and record the decision here. The ruleset grants the repository admin an `always` bypass. That bypass lets small infrastructure changes go straight to `dev`, and it also lets those pushes skip every required check. Either remove it, so the gate binds the owner and every change goes through a pull request, or keep it and accept that the gate binds external contributors only.
- [ ] **20.3**: Run the suite before every release. Expose `ci.yml` through `workflow_call` and make the release workflow's `build-and-push` job depend on it, so the pipeline never builds an image from a commit whose tests fail.
- [ ] **20.4**: Pin the CI PostgreSQL service to `postgres:16-alpine`, the image production runs, instead of `postgres:latest`. A new PostgreSQL major on Docker Hub can change CI behavior today without any commit in this repository.
- [ ] **20.5**: Move Node 20 to the current LTS in `ci.yml` and in the frontend `Dockerfile` build stage together, and update the GitHub Actions that warn about the Node 20 runtime deprecation.

### Track 21: Releases via SemVer Tags
- [ ] **21.1 - Versioning policy**: `vMAJOR.MINOR.PATCH`. MAJOR changes when the public API contract breaks (the contract that 18.2.4 publishes and the external suite consumes), MINOR when a feature ships, PATCH for fixes. Record the policy in this track once adopted.
- [ ] **21.2 - Tag trigger**: the release workflow triggers on pushed tags matching `v<major>.<minor>.<patch>` instead of `release/v*` branches. The server script fetches tags and resets the checkout to the tag (`git fetch --tags --force origin` then `git reset --hard "refs/tags/<tag>"`), keeping `set -e`. The current script resets to `origin/<ref_name>`, which only resolves for a branch.
- [ ] **21.3 - Image tags**: Docker images carry the SemVer version alongside `latest`.
- [ ] **21.4 - Footer release date**: the footer shows the release date instead of the version string, as `© 2026 Henrique Cioffi - Desenvolvido com .NET & React | 2026.11.21` (localized `footer.copy`, unchanged). The workflow computes the date in `America/Sao_Paulo` and passes it as a build argument; a UTC date would show the next day for any release cut after 21:00 local time.
- [ ] **21.5 - Retire release branches**: releases become annotated tags on `dev`, which drops the `release/v*` branch and its merge back into `dev`. The existing CalVer tags (`v2026.MM.DD.NN`) stay as history. Tools that sort tags by version will rank them above `v1.0.0`; GitHub Releases orders by date and is unaffected.

### Track 22: Category Expansion
- [ ] **22.1 - Backend**: append `Culture = 7`, `Philosophy = 8` and `Psychology = 9` to `ArticleCategory`. Existing values keep their numbers. EF persists the enum as a string (`HasConversion<string>()`), so the change needs no migration, and both validators accept the new values through `IsInEnum()`. Add a test that creates and reads back an article in each new category.
- [ ] **22.2 - Single category map (audit A10, category half)**: the frontend maps categories in three places today: `models/ArticleCategory.ts`, `util/categoryMapping.ts`, and an inline map in `ArticleCard.tsx`. Keep one source in `models/ArticleCategory.ts`, derive display labels from the `categories.*` translation keys, and delete the other two. The `ThemeProvider` half of the old 19.4 stays in Track 19.
- [ ] **22.3 - Translations**: add `culture`/`philosophy`/`psychology` under `categories` in both locale files: Culture, Philosophy, Psychology (EN) and Cultura, Filosofia, Psicologia (PT).

### Track 23: Reading Layout & Summary Lead (DOM freeze)
Last functional change before Phase 6. Once it lands, the markup and test hooks below are the contract the external suite targets.
- [ ] **23.1 - Prose measure**: paragraphs, headings, lists and blockquotes keep the current `max-w-3xl` column (768px). At `text-lg` that holds about 70 characters per line, the upper bound for comfortable reading, so the text column does not grow.
- [ ] **23.2 - Wide blocks**: at the `lg` breakpoint (1024px) and above, code blocks, images and tables may extend to `max-w-5xl` (1024px), centered on the prose column, which matches the header and footer width. Below `lg` they fill the prose column, and code blocks scroll horizontally. No viewport from 360px up shows a page-level horizontal scrollbar.
- [ ] **23.3 - Summary lead**: the article page renders the summary in the reader's active language as a single paragraph between `ArticleHeader` and the body, styled apart from body text (larger, muted). The detail response already carries `summary`, so the API does not change. The lead appears only once; the body does not repeat it.
- [ ] **23.4 - Home cards**: the card summary clamps at 4 lines (up from 3), and cards in the same grid row share one height. The article lead now shows the full summary, so the card stays a preview.
- [ ] **23.5 - Stable test hooks**: add `data-testid` to the card root, card title, card summary, article title, summary lead and article body. From this point a rename of any of them counts as a breaking change for the external suite.
- [ ] **23.6 - Visual check**: capture the home page and one article at 360px, 768px and 1280px, in light and dark themes, and attach the captures to the pull request.

---

## Phase 6: External Quality (Separate Repository) - NOT STARTED

Starts once Track 23 lands. Everything in this phase except 18.2.4 lives and runs in the separate automation repository.

**Target environments**: ephemeral only. The external repository's CI starts the API, web and PostgreSQL containers from the published images, runs the suite against them, and discards them. No E2E, API, contract or load run ever targets production. Production runs on a free-tier Oracle Cloud VM with little spare capacity, and test traffic there would compete with real readers and trip the production rate limits.

### Track 18.2: Automated Quality Suite (original Track 18)
- [ ] **18.2.1**: End-to-End (E2E) testing suite with **Playwright** for critical flows, in the external repository.
- [ ] **18.2.2**: Automated accessibility (A11y) audits in the external repository's CI pipeline.
- [ ] **18.2.3**: Performance benchmarking using **k6** for API endpoints, in the external repository.
    - [ ] **18.2.3.1**: Author k6 load-test scripts (JS) covering the hottest read paths (article listing, search, article detail).
    - [ ] **18.2.3.2**: Define pass/fail **thresholds** per script (e.g. p95 latency, error rate) so a regression fails the run instead of just reporting a number.
    - [ ] **18.2.3.3**: Run on demand or on a schedule (not on every PR) using `grafana/k6-action`, against the ephemeral containers only.
    - [ ] **18.2.3.4**: Keep output lightweight for a project this size — CLI summary + CI artifact is enough; skip a hosted dashboard (Grafana Cloud k6, etc.) unless traffic/need grows.
- [ ] **18.2.4**: API/contract specifications for this repo's surface (endpoints, DTOs, auth/RBAC matrix), written for consumption by the separate automation-framework repository described below. The one 18.2 item authored in this repository.

> **Sequencing note**: 17.5 and 18.0 were hard blockers for 18.2 and both have landed. Roadmap 2.0 adds Phase 5 as the remaining precondition: 18.2 starts after Track 23 freezes the DOM.

> **Threshold note for 18.2**: a browser suite authenticates from a single client address and therefore shares one rate-limit partition. Production thresholds (`auth-signin`: 10 per 15 minutes) are exhausted within ten sign-ins. The environment the suite runs against must declare its own `RateLimiting` configuration; thresholds bind from configuration for exactly this reason.

---

## Track 19: Architecture Consolidation (non-blocking, runs alongside Phases 5 and 6)

- [ ] **19.1**: Unify Article data access behind `IArticleRepository` consistently (currently split between the repository and direct `DbSet<Article>` access); add an equivalent repository for Comments, which currently has none (audit A5).
- [ ] **19.2**: Extract the duplicated "localized Markdown field" block in `CreateArticle.tsx`/`EditArticle.tsx` into a shared component + `useArticleForm` hook (audit A9).
- [ ] **19.3**: Move `AuthController.UpdateLanguage`/`ExternalLoginCallback` business logic into proper MediatR commands with validation (audit A7).
- [ ] **19.4**: Memoize `ThemeProvider`'s context value (audit A11). The category-label half (audit A10) moved to Track 22.2 in Roadmap 2.0.
- [ ] **19.5**: Route-level code splitting (`React.lazy`) to isolate the admin dashboard/editor bundle from the public reading experience, and switch to the light/async `react-syntax-highlighter` build (audit P4/P5).
- [ ] **19.6**: Memoize the recursive `CommentItem` tree and stop replacing the full comment array on every mutation (audit P6).
- [ ] **19.7**: Promote nullable-reference warnings to build errors (`TreatWarningsAsErrors`, or scoped to the nullable warning codes) across all `.csproj` files — `Nullable` is enabled everywhere already, but nothing currently fails the build when a nullable warning is introduced.
- [ ] **19.8**: Capture `PreferredLanguage` at newsletter signup (frontend sends the active i18next language; `SubscribeToNewsletterCommand` accepts and persists it) and switch the article notification from a single bilingual email to a per-subscriber localized one. Deferred from Track 17.5: the column exists but is populated for no subscriber, so localizing per recipient today would deliver English to everyone.
- [ ] **19.9**: Make Portuguese search accent-insensitive. Found while verifying Track 17.5: the `portuguese` dictionary stems `programação` to `program` but `programacao` to `programaca`, so a reader typing without accents — common on mobile and ASCII keyboards — gets zero results for accented articles. The current behavior is pinned by `Search_WithUnaccentedPortugueseTerm_DoesNotMatchAccentedText`. The fix is `CREATE EXTENSION unaccent` plus an `unaccent`-wrapped tsvector; note the GIN index DDL and `ArticleSearchService` must change together or the index stops being used, and `unaccent` must be schema-qualified to stay IMMUTABLE for indexing.
- [ ] **19.10**: Restrict origin ingress on 80/443 to the CDN's published address ranges, so the forwarded-address chain cannot be presented by a caller reaching the origin directly. Recorded during Track 18.0 as residual scope (spec §4.4).
- [ ] **19.11**: Add edge throttling (CDN rules or nginx `limit_req`) as a complement to the in-application limiter, rejecting excess traffic before it reaches the origin. Deferred from Track 18.0 so one enforcement point owned the behavior first.

---

## Relationship to the Separate Automation Framework

A dedicated repository will own **E2E (Playwright), API, contract, and load (k6) tests** exercising this application from the outside. This repo's roadmap stays scoped to what ships inside it:

- This repo is responsible for: unit tests, integration tests (MediatR handlers, controllers via `WebApplicationFactory`), and — per Track 18.2.4 — the API/contract specs that describe its own stable surface.
- The automation-framework repo is responsible for: consuming those specs, driving the app through real HTTP/browser interactions, running load benchmarks, and maintaining its own test-authoring conventions independent of this codebase's release cadence.
- The automation-framework repo runs only against ephemeral environments its own CI starts from the published images. It never targets production (see Phase 6).
- Specs for "what needs fixing" are **not** written as formal contracts — they live as roadmap tracks (above) and the audit report, and only become specs once the corresponding fix has actually landed, so the automation framework is never built against intentionally-temporary broken behavior.

---

## Retired Tooling

- **`docs/TECHNICAL_AUDIT_REPORT.md` — moved out of the repository 2026-11-28.** The audit and its 2026-11-28 re-assessment are kept privately, because a public itemized list of open weaknesses helps someone probing the deployed system more than it helps maintenance. Finding labels (`P#`, `A#`, `S#`, `T#`) cited in this roadmap, the specs and the plans stay valid as references to that private record.

- **`conductor/` folder (agent-outsourcing workflow) — removed 2026-11-19.** It had grown past the point of being useful and was creating more overhead than value; its only cross-reference was the "Agent Strategy" table that lived in this file, which is dropped in this rewrite.
- **`docs/tracks/` folder (per-track spec files) — removed 2026-11-19.** Superseded by this roadmap's inline track descriptions and `docs/TECHNICAL_AUDIT_REPORT.md` for anything requiring deeper detail. Future per-feature specs that need this level of depth belong in the separate automation-framework repo once a track is actually in flight, not pre-written here.
