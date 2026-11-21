# Technical Audit Report: MeuSitePessoal

**Audit date:** 2026-11-19
**Scope:** Full-stack review (.NET 8 Clean Architecture API + React/Vite/TypeScript frontend) ahead of Phase 5 / Track 18 (Playwright E2E, A11y, k6).
**Method:** Static analysis of source, migrations, CI/CD config, and test suite. No application code was modified as part of this audit.

---

## Executive Summary

**Overall architectural health: 6.5/10. Not yet ready to build Track 18 on top of as-is.**

The codebase demonstrates genuine engineering discipline in several areas — strict TypeScript with zero `any`/`@ts-ignore` escapes, a clean MediatR/CQRS Application layer, parameterized full-text search, consistent RBAC on the highest-risk endpoints, and a real-PostgreSQL integration test strategy. These are production-grade foundations.

However, the audit surfaced **one confirmed live production bug** (newsletter emails have shipped with blank subjects/content since Track 17), **a dead database index** (full-text search silently regressed to a sequential scan), and **a set of security findings recorded separately** (see §3.3). None of these are edge cases; they sit directly in the platform's critical path (publishing, searching, and authenticating).

The i18n "Flat Schema" migration (Track 17) is the single largest source of debt uncovered: the legacy-column sync strategy was deliberately disabled mid-migration (confirmed via developer comments in the code itself), but the cleanup (dropping legacy columns/indexes, auditing every consumer) was never finished. This left a "half-migrated" state that is actively causing incorrect behavior.

**Recommendation:** Do not start Track 18 (E2E suite) until the High severity items in Section 3 are resolved — building Playwright coverage on top of a broken newsletter pipeline and an unindexed search endpoint means the E2E suite will either bake in wrong expectations or immediately start failing once real content volume hits production. The Medium/Low items can be interleaved with E2E work. See Section 5 for a proposed sequencing ("Backtrack Tracks").

| Pillar | Score | Note |
|---|---|---|
| Architecture & Code Quality | 7/10 | Clean layering mostly intact; repository pattern half-applied; one live i18n regression |
| Performance & Scalability | 5/10 | Dead FTS index, missing indexes on hottest table, N+1-equivalent full-table scan in related-articles |
| Security & Resilience | 5/10 | Strong RBAC and XSS posture; findings in this pillar are recorded separately (§3.3) |
| Test Suite Health | 7/10 | Real-Postgres integration tests are a strength; two features have zero coverage; DB rebuilt per test is slow |
| Frontend Code Quality | 7/10 | Clean TS discipline; bundle/re-render issues are classic but fixable; some dead/duplicated code |

---

## 2. Strengths

These are built to a standard worth preserving as Track 18 proceeds — do not refactor these away in the name of "cleanup."

- **CQRS/MediatR discipline.** The vast majority of Application logic flows through well-isolated command/query handlers with FluentValidation pipelines, making most business logic unit-testable without a web server (exceptions noted in §3.2).
- **RBAC on the highest-risk surface is correct and tested.** Article create/update/delete require `Admin` (`ArticlesController.cs:89,118,131`), comment ownership is enforced in handlers (`UpdateCommentCommandHandler.cs:37-43`), and both are backed by real integration tests asserting 403s (`ArticleAuthorizationTests.cs`, `CommentOwnershipTests.cs`).
- **No SQL injection surface.** Zero `FromSqlRaw`/`FromSqlInterpolated` usage anywhere; the only hand-built query (full-text search) goes through parameterized `EF.Functions` calls (`ArticleSearchService.cs:29-34`).
- **Safe-by-default rendering.** No `rehype-raw`, no `dangerouslySetInnerHTML` anywhere in the frontend; comments render as plain React text nodes (auto-escaped), and Markdown is rendered without raw-HTML passthrough (`MarkdownRenderer.tsx`).
- **Real PostgreSQL in integration tests**, not SQLite/in-memory substitutes — Postgres-specific behavior (tsvector, arrays) is exercised against the real engine, both locally and in CI (`BaseIntegrationTest.cs`, `ci.yml`).
- **Clean TypeScript discipline.** `strict: true`, zero `any`, zero `@ts-ignore`/`@ts-expect-error` across the entire `src/` tree — a genuinely uncommon result at this project size.
- **Correct cache-invalidation wiring.** Despite the i18n churn, the version-stamp cache invalidation pattern is correctly triggered on all three Article mutation paths (Create/Update/Delete) — the caching *mechanism* itself is sound even where its *expiration policy* is inconsistent (§3.1).
- **Clean async/await hygiene.** Zero instances of `.Result`, `.Wait()`, `async void`, or sync-over-async blocking anywhere in Api/Application/Infrastructure/Domain.
- **Parallelized article-detail fetch.** `ArticleDetails.tsx` correctly uses `Promise.all` for the article + related-articles fetch — no waterfall there.
- **Account-enumeration-safe password reset** and **email-confirmation-required sign-in** are both correctly implemented (`AuthController.cs:134-138`, `Program.cs:54`).

---

## 3. Weaknesses & Technical Debt

Findings are grouped by pillar, ranked High → Low within each group. File:line references were verified by the auditing agents against the current source.

### 3.1 Performance & Scalability

| # | Finding | Location | Severity |
|---|---|---|---|
| P1 | **Full-text search GIN index no longer matches the query it was built to accelerate.** The Track 13 migration indexes `to_tsvector(..., "Title", "Summary")` (legacy columns), but `ArticleSearchService.cs:31-34` queries `TitleEn/SummaryEn/TitlePt/SummaryPt` with different concatenation. Postgres cannot use a mismatched expression index — search has silently regressed to a sequential scan computing `to_tsvector` per row, for every search request, since Track 17 shipped. | `AddFullTextSearchIndex.cs:16-19` vs `ArticleSearchService.cs:31-34` | **High** |
| P2 | **`Articles` table has no indexes beyond the primary key** (apart from the now-dead index above). Every listing query sorts by `CreatedAt` (`GetArticlesQueryHandler.cs:67`, `ArticleRepository.cs:34`, `ArticleSearchService.cs:47`), filters by `Category` (`GetArticlesQueryHandler.cs:61`), and filters by `Tags` array containment (`GetArticlesQueryHandler.cs:55`) — none of these columns are indexed. | `BlogDbContextModelSnapshot.cs:94-156` | **High** |
| P3 | **Related-articles query loads the entire Articles table into memory**, including full `Content*` bodies, then computes tag overlap client-side before taking the top N. This is O(n) memory/network per request and ships full article bodies over the wire just to compare tags. | `GetRelatedArticlesQueryHandler.cs:53-67` | **High** |
| P4 | **Frontend ships a full, unsplit Prism syntax-highlighter bundle** (~1-1.2MB minified) to every visitor, including anonymous readers who never view a code block, because `react-syntax-highlighter`'s full build is imported rather than the light/async variant. | `MarkdownRenderer.tsx:4-5` | **High** |
| P5 | **Zero route-level code splitting on the frontend.** `App.tsx` statically imports every page, so the admin-only Markdown editor and its dependency tree (react-markdown, remark-gfm, the full syntax highlighter from P4) load into the same bundle a first-time anonymous reader downloads. | `App.tsx:4-19` | **High** |
| P6 | **Recursive `CommentItem` has no memoization**, and the parent `CommentSection` replaces the entire comment array on every create/edit/delete. Editing one reply in a 50-comment thread re-renders the whole tree. | `CommentItem.tsx`, `CommentSection.tsx:24-40` | **High** |
| P7 | **Uploaded images in Markdown content have no `loading="lazy"`, no explicit width/height (CLS risk), and no server-side resizing** — full-resolution originals are served to every viewport. | `MarkdownRenderer.tsx:101-116`, `imageService.ts:12-23` | **High** |
| P8 | **Admin "get all articles" dashboard query pulls full article bodies** (`Title`, `Content`, `Summary`, plus all four translated columns) for a listing view, unlike the public listing/search paths which correctly project only summary fields. | `GetAllArticlesHandler.cs:33-53`, `ArticleRepository.cs:25-40` | **Medium** |
| P9 | **Debounce hook built but never wired in** — `useDebounce.ts` exists and is fully implemented but has zero call sites; the search bar only fires on Enter/icon-click, not on keystroke, despite the roadmap describing Track 13.2 as "search bar with debounce logic." | `useDebounce.ts`, `SearchBar.tsx:26-34` | **Medium** |
| P10 | **Comments fetch only after the article page's data resolves and mounts the child component** (a two-stage waterfall: article+related in parallel, then comments sequentially), rather than firing in parallel keyed off the route param. | `CommentSection.tsx:38-40`, `ArticleDetails.tsx:108,196` | **Medium** |
| P11 | **No Redis/`IDistributedCache`** despite the roadmap describing a "MemoryCache/Redis" caching layer — only `AddMemoryCache()` is registered. Safe today (single API instance) but any horizontal scale-out will serve stale data from non-mutating instances for up to 10 minutes after an edit. | `Program.cs:137` | **Medium (latent)** |
| P12 | **Inconsistent cache expiration policy** across three near-identical caching call sites for the same entity — one uses sliding expiration, two use absolute — indicating copy-paste without a shared abstraction. | `GetArticlesQueryHandler.cs:85-86` vs `GetArticlesSearchQueryHandler.cs:49`, `GetRelatedArticlesQueryHandler.cs:80` | **Low-Medium** |
| P13 | Cache entries are orphaned (not actively evicted) on every invalidation since keys embed a version GUID; bounded by a 10-minute expiration but causes minor unbounded growth between edits. No per-key stampede guard on cache miss. | `GetArticlesQueryHandler.cs:93-97` and siblings | **Low** |
| P14 | Several read-only repository methods don't use `.AsNoTracking()` (`SubscriberRepository.cs:25-39`, `ArticleRepository.GetByIdAsync` used on read paths). Minor overhead from unnecessary change tracking. | `SubscriberRepository.cs:25-39`, `ArticleRepository.cs:45-48` | **Low** |
| P15 | No Vite `manualChunks`/vendor splitting — any single app change invalidates one large shared bundle's browser cache. | `vite.config.ts:1-14` | **Low** |

### 3.2 Architecture & Code Quality

| # | Finding | Location | Severity |
|---|---|---|---|
| A1 | **The i18n "legacy columns sync" is not an ongoing mechanism — it was a one-time migration backfill, then explicitly disabled.** Developer comments in the code itself confirm the decision: *"We no longer populate legacy fields for new records... to avoid 'Split Brain' issues"* and *"STOPPED mirroring to legacy columns for new/updated records as requested."* Every article created or edited since Track 17 shipped has permanently empty legacy `Title`/`Content`/`Summary` columns, with no trigger, computed column, or standing application logic replacing the one-time backfill. | `Article.cs:62-66,103-128`, `UpdateArticleCommandHandler.cs:40-42`, `MappingProfile.cs:16-23` | **High (named debt item)** |
| A2 | **Confirmed live bug caused directly by A1: every newsletter notification email sent since Track 17 has a blank subject line and blank headline/summary in the body.** The event handler and email template both read the now-permanently-empty legacy `Article.Title`/`Article.Summary` fields instead of the active `TitleEn/TitlePt`/`SummaryEn/SummaryPt` columns. | `ArticlePublishedEventHandler.cs:38,83`, `NewPostNotification.cshtml:19,21` | **High — production regression, not hypothetical** |
| A3 | **Inconsistent legacy-fallback strategy across read paths.** Public listing/search handlers correctly dropped the legacy fallback (comment: "REMOVED legacy fallback"), but the admin dashboard and article-detail/edit handlers still carry a now-dead fallback branch for data that no longer needs it — meaning a future engineer fixing a blank-title bug in one handler has no reason to check the other three, which is exactly how A2 went unnoticed. | `GetArticlesQueryHandler.cs:73-76`, `ArticleSearchService.cs:53-55` vs `GetAllArticlesHandler.cs:33-53` | **Medium** |
| A4 | **Legacy columns and their now-dead GIN index remain physically in the schema** with no migration path to drop them or replace the index with one matching the active translated columns (see P1). The "dual-write → cutover → drop" migration pattern was only half-executed. | `BlogDbContextModelSnapshot.cs:100-141` | **Medium-High** |
| A5 | **Repository pattern is only half-applied.** `IArticleRepository` coexists with direct `IBlogDbContext`/`DbSet<Article>` access from the Application layer (`GetArticlesQueryHandler`, `GetArticlesSearchQueryHandler` via `ArticleSearchService`, `GetRelatedArticlesQueryHandler`), and **Comments have no repository abstraction at all** — only raw `DbSet<Comment>` access. Two different mental models are required to reason about how the same entities are queried. | `IArticleRepository.cs` vs `IBlogDbContext.cs:10-19` | **Medium-High** |
| A6 | **Domain layer depends on ASP.NET Core Identity.** `ApplicationUser : IdentityUser` is a direct framework dependency inside the Domain project — the one confirmed Domain-purity violation found. | `ApplicationUser.cs:1,9` | **Medium** |
| A7 | **Business logic embedded directly in `AuthController`**, bypassing the MediatR pattern used by every other endpoint in the same file. `UpdateLanguage` has no allow-list validation on the language string; `ExternalLoginCallback` contains ~70 lines of user-provisioning/role-assignment/JWT-issuance logic with no Application-layer command and therefore no unit-test surface. | `AuthController.cs:53-70,194-265` | **Medium** |
| A8 | **Validation is duplicated and inconsistent between FluentValidation and Domain guard clauses.** FluentValidation caps title length at 100 chars; the DB column allows 200; the Domain constructor enforces no length at all (only non-empty). If FluentValidation is ever bypassed, nothing in the Domain prevents the inconsistency. | `CreateArticleCommandValidator.cs:10-12,22-24` vs `AddFlatSchemaI18n.cs:57-71`, `Article.cs:45-50` | **Low-Medium** |
| A9 | **`CreateArticle.tsx` (388 lines) and `EditArticle.tsx` (430 lines) are near-duplicate monoliths.** The ~140-line "localized Markdown field" block (toolbar + textarea + preview, per language) is copy-pasted twice within each file — effectively the same component duplicated 4 times across 2 files. Any editor/upload bugfix must be applied up to 4 times. | `CreateArticle.tsx:283-360`, `EditArticle.tsx` (equivalent) | **High** |
| A10 | Category label mapping (`Record<number,string>`) is independently hand-duplicated in at least two frontend files rather than using the existing shared `categoryMapping.ts` helper — a new backend category requires editing multiple literal maps. | `ArticleCard.tsx:26-36`, `ArticleDetails.tsx:57-67` | **Medium** |
| A11 | `ThemeProvider`'s context value object is not memoized (unlike `AuthProvider`, which correctly uses `useMemo`) — latent inconsistency that will cause full-tree re-renders once any sibling state is added to the provider. | `ThemeProvider.tsx:13` vs `AuthProvider.tsx:85-92` | **Medium** |
| A12 | Fat domain-event handler mixes subscriber retrieval, URL-building business rules, dispatch orchestration, and per-item error handling in one method — a candidate for extraction into a dedicated notifier service. | `ArticlePublishedEventHandler.cs` (105 lines) | **Low** |
| A13 | `Dashboard.tsx` redefines `ArticleSummary`/`PagedResult` locally instead of importing the shared model used elsewhere, risking type drift. | `Dashboard.tsx:10-23` | **Low-Medium** |

### 3.3 Security & Resilience

The 2026-11-19 audit recorded **12 findings** in this category, scored 5/10 in the pillar
table above.

The findings themselves are kept in a private record rather than enumerated here. This
repository is public, and an itemized list of weaknesses in a deployed system — with file
and line references and severity ratings — serves someone probing that system far better
than it serves anyone maintaining it.

The `S#` identifiers remain in use as opaque labels, so the cross-references from
`docs/roadmap.md`, `CLAUDE.md`, and §5 below still resolve. Track 18.0 closed S1, S2, S3,
and S5; the rest are scheduled through the private record.

This narrowing reduces casual discovery and nothing more. The content remains in this
repository's git history and in any existing clone or fork, and it is not a substitute for
closing the items still open.

### 3.4 Test Suite Health

| # | Finding | Location | Severity |
|---|---|---|---|
| T1 | **Two features have zero test coverage at any level (unit or integration): two-factor authentication verification, and newsletter double opt-in confirmation.** Both are real, user-facing flows described in the roadmap (Track 16.5.3, and `RequiresTwoFactor` is a live field in the login response) with no corresponding handler test anywhere. | no `*TwoFactor*` or `*ConfirmSubscription*` test files found | **High** |
| T2 | **The test database is dropped and fully re-migrated before every single test method** (`EnsureDeletedAsync` + `MigrateAsync` + reseed in `IAsyncLifetime.InitializeAsync`, invoked per xUnit test-class instance). This is extremely slow at 178 tests and is only safe today because the whole suite is forced to run sequentially (`DisableTestParallelization = true`) — the suite cannot be parallelized without a rewrite of the base fixture. | `BaseIntegrationTest.cs:88-161`, `AssemblyInfo.cs:4` | **Medium-High (scaling risk)** |
| T3 | **CI's connection-string environment variable is dead configuration.** `ci.yml` sets a connection string pointing at a different database name than what `BaseIntegrationTest.cs` actually hardcodes and connects to; tests only pass in CI because the Postgres container is configured with `trust` auth, silently accepting any password. Any future CI hardening of Postgres auth will break every integration test without an obvious cause. | `ci.yml:45` vs `BaseIntegrationTest.cs:25,120-123` | **Medium** |
| T4 | A real local-looking database password is hardcoded directly in test source. | `BaseIntegrationTest.cs:25` | **Medium** |
| T5 | **A live outbound SMTP integration test runs in CI** against the real `smtp.resend.com`, depending on network access, DNS, and third-party provider behavior remaining stable — a genuine flakiness/CI-reliability risk, and it was not skipped unlike its sibling happy-path test. | `SmtpIntegrationTests.cs:62-78` | **Medium** |
| T6 | **Mock-chasing in cache invalidation tests.** All three Article cache-invalidation unit tests assert only that `Mock<IMemoryCache>.Remove(...)` was called — none verify that a subsequent read actually returns fresh data, so the tests cannot catch a real invalidation regression. | `ArticleCacheInvalidationTests.cs:43,64,82` | **Medium** |
| T7 | **Query-handler unit tests use EF Core InMemory, which silently diverges from the production Postgres code path** for full-text search — `ArticleSearchService.cs` special-cases non-Npgsql providers with a naive `.Contains()` fallback, so these unit tests never exercise the `tsvector` logic at all (relevant given P1's dead-index bug went undetected by any test). | `GetArticlesSearchQueryHandler` unit tests (InMemory provider) | **Medium** |
| T8 | **Untested regression risk tied to A1/A3:** no test verifies that pre-migration rows (legacy-only data) still render correctly now that the legacy fallback was removed from search/listing handlers — meaning a regression exactly like A2 is structurally undetectable by the current suite. | cross-reference with A1/A3 | **Medium** |
| T9 | Several integration tests use weak assertions (status-code-only, non-null-only) where DB-state or claims-content assertions would catch real regressions — e.g. newsletter subscribe doesn't assert a `Subscriber` row was created; login success doesn't assert token claims/role content. | `NewsletterIntegrationTests.cs:23-24`, `AuthIntegrationTests.cs:58` | **Low-Medium** |
| T10 | Comment-thread tests only cover single-level replies; no coverage for reply-to-reply (depth ≥ 2), cross-article parent IDs, or replying to a deleted comment. | `CommentsIntegrationTests.cs:52-102` | **Low** |

---

## 4. Performance Optimization Roadmap

Ranked by ROI (impact vs. effort), API and Web separated.

### API

| Priority | Action | Addresses |
|---|---|---|
| **High** | Rebuild the full-text search GIN index to match the actual query expression against `TitleEn/TitlePt/SummaryEn/SummaryPt`, and drop the dead legacy-column index in the same migration. | P1, A4 |
| **High** | Add indexes: B-tree on `Articles.CreatedAt` (listing sort), B-tree on `Articles.Category` (filter), GIN on `Articles.Tags` (array containment filter). | P2 |
| **High** | Rewrite `GetRelatedArticlesQueryHandler` to filter/intersect tags in SQL (e.g. Postgres array overlap `&&` operator or a join against a normalized tags table) instead of loading every article row into memory. | P3 |
| **Medium** | Project only summary columns in the admin "get all articles" dashboard query, matching the optimization already applied to the public listing/search paths. | P8 |
| **Medium** | Standardize cache expiration (pick sliding or absolute, apply consistently) and consider a thin `ICacheService` wrapper to stop the copy-paste drift. | P12 |
| **Low** | Add `.AsNoTracking()` to the remaining read-only repository methods. | P14 |
| **Low** | If/when horizontally scaling the API, move from `IMemoryCache` to `IDistributedCache` (Redis) before adding a second instance — not urgent at current scale. | P11 |

### Web

| Priority | Action | Addresses |
|---|---|---|
| **High** | Switch `react-syntax-highlighter` to the light/async build with only the languages actually used in article content. | P4 |
| **High** | Add `React.lazy`/`Suspense` route splitting, at minimum isolating the admin dashboard + Markdown editor bundle from the public-facing reading experience. | P5 |
| **High** | Wrap `CommentItem` in `React.memo`, and change `CommentSection` to patch the affected node instead of replacing the whole comment array on every mutation. | P6 |
| **High** | Add `loading="lazy"` and explicit `width`/`height` (or an aspect-ratio wrapper) to the Markdown `img` renderer; consider server-side image resizing on upload. | P7 |
| **Medium** | Wire the existing `useDebounce` hook into `SearchBar`'s keystroke handler to actually deliver the debounced type-ahead search the roadmap describes. | P9 |
| **Medium** | Fetch comments in parallel with the article/related-articles call (keyed off the route `id`) instead of gating on the parent's resolved state. | P10 |
| **Low** | Add `manualChunks` vendor splitting in `vite.config.ts`. | P15 |

---

## 5. Recommended Backtrack Tracks

Proposed as discrete, sequenced tracks to execute **before or alongside** Track 18, in priority order. Each is scoped to be small enough to land without blocking E2E work indefinitely.

### Track 17.5: i18n Flat Schema Cleanup (blocks reliable E2E fixtures — do first)
- Fix the newsletter email bug (A2) by switching `ArticlePublishedEventHandler` and the email template to read `TitleEn/TitlePt`/`SummaryEn/SummaryPt`.
- Decide and execute the actual legacy-column endgame: either drop `Title`/`Content`/`Summary` entirely (and the dead index) in a migration, or formally document why they're kept and add the standing sync mechanism the current code claims not to need.
- Rebuild the full-text search GIN index to match the live query (P1/A4).
- Add a regression test asserting that newsletter emails render non-blank subject/body for a freshly published article, and a test covering pre-Flat-Schema legacy-only data rendering through every read path (T8).

**Why first:** Track 18's E2E suite will script "publish an article → verify newsletter is sent" and "search for an article" as near-certain golden-path scenarios. Building those scripts against the current broken behavior means either the E2E suite encodes the bug as expected behavior, or it immediately fails on day one for reasons unrelated to Playwright itself.

### Track 18.0: Critical Security Hardening (blocks any pre-prod E2E run against a shared environment)
- Move the JWT signing key to an environment-variable/secret-manager value and add it to `docker-compose.yml`'s production override list (S1); rotate the key once moved.
- Rotate the database password found in git history regardless of current usage (S2).
- Re-enable Identity lockout (`lockoutOnFailure: true`) and add ASP.NET Core rate limiting to at minimum `/login`, `/register`, `/forgot-password`, comment creation, and newsletter subscription (S3).
- Restrict `ImagesController` to `Admin` or add ownership checks on delete (S5).

**Why before/alongside Track 18:** running a Playwright suite against a shared staging environment that has a forgeable Admin JWT and no login throttling is itself a risk; this should land before CI starts exercising real auth flows at scale.

### Track 18.1 (parallel with Playwright build-out): Test Suite Foundations
- Replace the per-test `EnsureDeletedAsync`/`MigrateAsync` cycle with a respawn/transaction-rollback strategy (e.g. Respawn library or a single migrated database reset via checkpoint) to cut integration suite runtime (T2).
- Fix the CI/test connection-string mismatch so `ci.yml`'s env var is actually load-bearing (T3), and move the hardcoded test password out of source (T4).
- Mark or mock the live SMTP integration test so it doesn't depend on network access to a third party in CI (T5).
- Add handler-level tests for newsletter confirmation (T1) — exactly the kind of multi-step flow Playwright will also need fixtures for, so building the unit/integration layer first gives E2E a verified foundation to script against. T1's other half, two-factor verification, was closed by removing the feature instead (roadmap 18.1.5): it shipped permanently disabled, so testing it would have been coverage of a path no user could reach.

### Track 19 (can follow Track 18, not blocking): Architecture Consolidation
- Unify Article data access behind `IArticleRepository` consistently, and add an equivalent repository for Comments (A5).
- Extract the duplicated "localized Markdown field" block in `CreateArticle.tsx`/`EditArticle.tsx` into a shared component + `useArticleForm` hook (A9).
- Move `AuthController.UpdateLanguage`/`ExternalLoginCallback` logic into MediatR commands with proper validation (A7).
- Centralize the category-label mapping (A10) and memoize `ThemeProvider`'s context value (A11).

---

## 6. Closing Note on Readiness for Track 18

The team should not read this report as "the project is in bad shape" — the strengths in Section 2 are real and the failure modes found are typical of a solo/small-team project that executed 17 tracks of feature work faster than it could fully close out each one's edge cases (the i18n migration in particular). The specific risk for Track 18 is narrower than "is this codebase good": it's that an E2E suite is a *behavioral contract*, and two of the behaviors it would most naturally lock in (newsletter delivery, search relevance) are currently broken in ways no existing test catches. Fixing Track 17.5 first turns Track 18 into validating correct behavior, rather than accidentally certifying a bug as a feature.
