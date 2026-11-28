# Project Guidelines: MeuSitePessoal

Personal blog/CMS (hcioffi.dev). Backend: .NET 8, Clean Architecture, MediatR/CQRS,
EF Core, ASP.NET Core Identity + JWT, PostgreSQL. Frontend: React 19 + Vite +
TypeScript (strict) + Tailwind CSS, i18next (EN/PT).

## Where the real plan lives

- `docs/roadmap.md` — the Track system (numbered phases/tracks/sub-items). Check
  the current in-progress track before starting work; update checkboxes as items land.
- `docs/TECHNICAL_AUDIT_REPORT.md` — 2026-11-19 full audit (architecture, perf,
  security, test health). Findings are cited as `P#`/`A#`/`S#`/`T#` and cross-referenced
  from the roadmap. Read it before touching Articles, search, caching, auth, or the
  i18n "Flat Schema" columns — several open bugs live there.
- This repo's scope ends at unit/integration tests and (per Track 18.2.4) published
  API/contract specs. A separate automation-framework repo owns E2E/API/contract test
  execution against the deployed app — don't build E2E specs inside this repo.

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
- The frontend has no test framework yet; it will be introduced via the
  TypeScript/Playwright stack (roadmap Track 18.2.1), not vitest/jest — align any
  new frontend test tooling with that choice.

## Blog article writing conventions

- **Storage path.** Drafts live **outside this repository**, in the `articles/` folder that
  sits beside the project root (`../articles/YYYY-MM-DD/` relative to this file), dated by
  target publication day, as three clean Markdown files: `metadata.md`, `article-en.md`,
  `article-pt.md`. Never create an `articles/` folder inside the repository: it is no longer
  in `.gitignore`, so drafts written there would be picked up by `git add`. Drafts reach the
  site through the application, not through a commit.
- **No AI attribution in articles. This overrides the PR and Issue rule.** GitHub Issues
  and Pull Requests carry the standard attribution blockquote. Articles carry nothing:
  no header, blockquote, disclaimer, footnote, byline, or passing mention of AI tools or
  agents, in either language. Every article reads as Henrique Cioffi writing in the first
  person about his own engineering work.
- **Prose and schema discipline.** Invoke `stop-slop` when drafting or reviewing an
  article. Respect the PostgreSQL column limits the schema enforces: `TitleEn` and
  `TitlePt` at 200 characters, `SummaryEn` and `SummaryPt` at 500 characters. `Category`
  must be a valid `ArticleCategory` name (`MeuSitePessoal.Domain/Entities/ArticleCategory.cs`:
  Technology, Tutorial, Life, News, Opinion, Projects). Never publish an unpatched
  security finding from `docs/TECHNICAL_AUDIT_REPORT.md`; write about a vulnerability only
  once the fix has shipped.

## Secrets

- `docker-compose.yml` already injects secrets via `${VAR}` substitution for DB/email
  — follow that pattern for any new secret (e.g. the JWT signing key fix in roadmap
  18.0.1). Never hardcode a secret in `appsettings.json` or a compose file.
