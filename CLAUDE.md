# Project Guidelines: MeuSitePessoal

Personal blog/CMS (hcioffi.dev). Backend: .NET 8, Clean Architecture, MediatR/CQRS,
EF Core, ASP.NET Core Identity + JWT, PostgreSQL. Frontend: React 19 + Vite +
TypeScript (strict) + Tailwind CSS, i18next (EN/PT).

## Where the real plan lives

- `docs/roadmap.md` — the Track system (numbered phases/tracks/sub-items). Check
  the current in-progress track before starting work; update checkboxes as items land.
  Track IDs are permanent: never renumber or reuse one. A new track takes the next free
  number, and a track that moves to another phase keeps its ID, because plans, specs,
  the audit and commit messages cite tracks by ID.
- `docs/IDEAS.md` — a local, git-ignored parking lot for ideas kept out of the roadmap,
  with the technical notes gathered when they were discussed. It may be absent in a fresh
  clone. Nothing in it is committed work: promote an idea to a roadmap track before
  implementing it.
- Technical audits live outside this public repository, in `../articles/docs/`:
  `TECHNICAL_AUDIT_REPORT.md` (2026-11-19 baseline) and
  `TECHNICAL_AUDIT_REPORT-2026-11-28.md` (re-assessment, private, lists open security
  findings). Findings are cited as `P#`/`A#`/`S#`/`T#` (and `N-*` for the re-assessment)
  from the roadmap, specs and plans. Read the re-assessment before touching Articles,
  search, caching, auth or the deploy path; several open bugs live there. Never copy its
  security findings into this repository. The folder may be absent outside the owner's
  machine.
- This repo's scope ends at unit/integration tests and (per Track 18.2.4) published
  API/contract specs. A separate automation-framework repo owns E2E, API, contract and
  k6 load test execution — don't build E2E or load specs inside this repo. That repo runs
  only against ephemeral environments its own CI starts from the published images, never
  against production: the site runs on a free-tier Oracle Cloud VM with little spare
  capacity.

## Architecture conventions — known inconsistencies, don't "fix" by guessing

- Repository pattern is **half-applied**: `IArticleRepository` exists, but several
  Application handlers query `IBlogDbContext`/`DbSet<Article>` directly, and Comments
  have no repository at all (audit A5, roadmap 19.1). Match whichever pattern the
  handler you're editing already uses; don't silently introduce a third approach.
- The i18n "Flat Schema" is now the only content model. Track 17.5 (2026-11-20) dropped
  the legacy `Title`/`Content`/`Summary` columns from the entity, the EF mapping and the
  database, so they no longer exist in any form — `TitleEn/TitlePt`, `SummaryEn/SummaryPt`
  and `ContentEn/ContentPt` are the whole surface, all six are mandatory, and no read path
  carries a fallback (audit A1/A2/A3/A4 closed).
- Full-text search uses one GIN index per language (`idx_articles_search_en`,
  `idx_articles_search_pt`). PostgreSQL only uses an expression index when the query
  expression matches it exactly, so if you change the predicate in `ArticleSearchService`
  you must change the index DDL to match — `ArticleSearchSqlShapeTests` pins the emitted
  SQL and will fail if they drift (audit P1 closed).
- `ApplicationUser : IdentityUser` lives in the Domain project — a known, accepted
  framework-purity violation (audit A6). It's tracked separately; don't fix it in passing.
- FluentValidation and Domain guard clauses currently enforce different limits on the
  same fields in places (audit A8) — when adding validation, check both layers agree.

## Testing conventions

- xUnit. Integration tests run against a **real PostgreSQL** instance
  (`BaseIntegrationTest.cs`), not SQLite/InMemory — preserve this; InMemory diverges
  from Postgres-specific behavior (tsvector, arrays) and has already hidden one
  regression (audit T7).
- The test database is named by `ConnectionStrings:DefaultConnection`, resolved in
  `TestDatabase.cs` from user secrets then environment variables, with no fallback and
  no credential in source. A fresh clone must run the `dotnet user-secrets set` command
  the failure message prints before any integration test passes. The suite **drops and
  re-creates** that database, so never point it at a development instance.
- The suite is forced sequential (`DisableTestParallelization`) because every test shares
  one database and clears it on entry (`TestDatabase.ResetAsync`) — don't parallelize
  without giving each worker its own database first. Roadmap 18.1.1 replaced the per-test
  drop/re-migrate with a Respawn reset, which cut the runtime but not this constraint.
- The frontend has no test framework, and this repo will not get one: Playwright
  (roadmap Track 18.2.1) lives in the external automation repo. Don't add Playwright,
  vitest or jest here. The frontend checks in this repo are `npm run lint` and
  `npm run build`.

## Blog article writing conventions

- **The rules live in `../articles/CLAUDE.md`.** Read that file before drafting or
  reviewing an article from this repository; it owns layout, field limits, categories,
  tags and the security-topic rule.
- **Storage path.** Drafts live **outside this repository**, in `../articles/YYYY-MM-DD/`.
  Never create an `articles/` folder inside the repository: it is not in `.gitignore`, so
  drafts written there would be picked up by `git add`.
- **No AI attribution in articles. This overrides the PR and Issue rule.** GitHub Issues
  and Pull Requests carry the standard attribution blockquote. Articles carry nothing.

## Secrets

- `docker-compose.yml` already injects secrets via `${VAR}` substitution for DB/email
  — follow that pattern for any new secret (e.g. the JWT signing key fix in roadmap
  18.0.1). Never hardcode a secret in `appsettings.json` or a compose file.
