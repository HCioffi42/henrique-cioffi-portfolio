# Spec: Track 17.5 — i18n Flat Schema Cleanup & Bilingual Newsletter

**Status**: Awaiting approval
**Date**: 2026-11-20
**Related**: `docs/roadmap.md` Track 17.5 · `docs/TECHNICAL_AUDIT_REPORT.md` findings A1, A2, A3, A4, P1
**Implementation plan**: `docs/plans/track-17.5-implementation-plan.md`

This document describes the **target state**: the contracts the system must satisfy once Track 17.5 lands. It is written to be implementable without reference to the defects that motivated it. Current-state defects live in the audit report; remediation sequencing lives in the roadmap.

---

## 1. Intent

The Flat Schema i18n model (Track 17) is the permanent content model: an article carries English and Portuguese fields side by side, both mandatory. Track 17.5 finishes that migration — it removes the superseded single-language columns, makes every consumer read the localized fields, and restores index-backed full-text search against them.

**Why now:** this is the last structural work standing between the current codebase and Phase 5. Track 18.2's E2E suite will assert on newsletter delivery and search behavior; both must express intended behavior before they are encoded as automated expectations.

### Goals

- An article's content is represented by exactly one schema: the localized (`*En`/`*Pt`) columns.
- A newsletter notification presents both language versions of an article in a single email.
- Full-text search is served by a GIN index with a dictionary matched to the reader's language.
- No read path in the system can produce empty user-facing text for a well-formed article.

### Non-goals

- Per-subscriber language targeting. `Subscriber.PreferredLanguage` exists but is never populated; switching to localized-per-recipient emails requires capturing language at signup and is tracked separately (roadmap 19.8).
- Adding a third language. The schema stays flat and two-language; generalizing to an `ArticleTranslation` table is explicitly out of scope.
- The missing `CreatedAt` / `Category` / `Tags` indexes (audit P2). Related to search performance, but separate work.
- Any change to article authoring UI, comments, auth, or caching.

### Anti-goals

Outcomes that would make this track a failure even if the code compiles and tests pass:

- A subscriber receives an email with a blank, duplicated, or half-rendered title or summary.
- Dropping the legacy columns destroys article text that exists nowhere else.
- The new indexes are created but the query planner does not use them — a cosmetic fix that leaves the sequential scan in place.

---

## 2. Approaches considered

### Newsletter language strategy

| Approach | Trade-off |
|---|---|
| **Single bilingual email** (chosen) | Every subscriber receives correct content immediately with no new data and no backfill. Email is longer, and readers see a language they may not read. |
| Per-subscriber localized email | The right long-term model, but `PreferredLanguage` is unpopulated for 100% of existing rows, so shipping it alone delivers English to everyone — functionally identical to the English-only option until the signup flow is changed. Deferred to roadmap 19.8. |
| English-only | Smallest change; abandons Portuguese readers on a deliberately bilingual blog. Rejected. |

### Legacy column disposition

| Approach | Trade-off |
|---|---|
| **Drop the columns** (chosen) | Removes the possibility of a consumer reading a stale field, and removes the dead index. Irreversible; requires a verified backup and a pre-drop data audit. |
| Retain and re-establish a standing sync | Keeps a rollback path, but reintroduces the dual-write burden the Track 17 author explicitly removed, and leaves two sources of truth for the same text. Rejected. |

### Full-text index shape

| Approach | Trade-off |
|---|---|
| **Two indexes, one per language dictionary** (chosen) | Each index matches a single concrete query shape, so the planner can use it. Correct stemming per language. Costs two indexes, and a search returns matches only from the reader's active language. |
| One combined index over concatenated vectors | Mirrors the previous structure and searches both languages at once, but a combined expression is harder to match exactly and applies one dictionary's stemming to both languages. Rejected. |

---

## 3. Contract: Article schema

### 3.1 Final `Articles` table

| Column | Type | Null | Notes |
|---|---|---|---|
| `Id` | `uuid` | NOT NULL | Primary key |
| `TitleEn` | `varchar(200)` | NOT NULL | |
| `SummaryEn` | `varchar(500)` | NOT NULL | |
| `ContentEn` | `text` | NOT NULL | |
| `TitlePt` | `varchar(200)` | NOT NULL | |
| `SummaryPt` | `varchar(500)` | NOT NULL | |
| `ContentPt` | `text` | NOT NULL | |
| `Tags` | `text[]` | NOT NULL | |
| `Category` | `text` | NOT NULL | Enum persisted as string |
| `CreatedAt` | `timestamptz` | NOT NULL | |

**Removed:** `Title`, `Content`, `Summary`.

### 3.2 Entity and mapping

- `Article` exposes no `Title`/`Content`/`Summary` properties. The six localized properties are the only content surface.
- The `[Obsolete]` single-language constructor is removed. The six-argument localized constructor is the only public constructor, and its existing guard clauses (all six fields non-empty) are the enforced domain invariant.
- `BlogDbContext.OnModelCreating` contains no `Property(a => a.Title/.Content/.Summary)` configuration. Model and database agree with no drift.
- `MappingProfile` contains no `.ForMember(... Title/Content/Summary ... Ignore())` clauses, because the members no longer exist.
- `ArticleResponse` drops its three resolved-language members (`Title`, `Content`, `Summary`) only if no consumer requires them; otherwise they remain as **computed** values resolved from the active language with no legacy fallback. The builder confirms current consumers before choosing — see §7.

### 3.3 Read-path invariant

No read path performs a legacy fallback. For an article `a` and active language `lang`:

```
resolved_title   = lang starts with "pt" ? a.TitlePt   : a.TitleEn
resolved_summary = lang starts with "pt" ? a.SummaryPt : a.SummaryEn
resolved_content = lang starts with "pt" ? a.ContentPt : a.ContentEn
```

This is already the behavior of the public listing, search, and related-article paths. The administrative listing path must be brought in line with it.

---

## 4. Contract: Bilingual newsletter notification

### 4.1 Subject line

```
{TitleEn} | {TitlePt}
```

**Deduplication rule:** if `TitleEn` and `TitlePt` are ordinally equal, the subject is just `{TitleEn}`. This is not hypothetical — the Track 17 backfill copied the single legacy title into *both* localized columns, so every article predating that migration has identical titles and would otherwise render `Foo | Foo`.

**Builder's note for review:** this drops the previous `New Post: ` prefix and ` - hcioffi.dev` suffix. Two titles plus both affixes exceeds the ~60-character budget most mail clients display, and sender identity is already carried by the From name. Flagged here so it can be overridden at review.

### 4.2 Body structure

A single email containing, in order:

1. A masthead.
2. **English block** — `TitleEn`, `SummaryEn`, and a call-to-action reading "Read Full Article".
3. A visual separator.
4. **Portuguese block** — `TitlePt`, `SummaryPt`, and a call-to-action reading "Ler Artigo Completo".
5. Footer with the subscription explanation and the unsubscribe link.

Both calls-to-action point at the same absolute article URL (`{ClientSettings:BaseUrl}/article/{id}`). Per-language deep links (`?lang=`) are out of scope.

### 4.3 Single-language degradation rule

If exactly one language's title and summary are non-empty, the email renders only that language's block and uses only that language's title as the subject. If both are empty the notification is not sent, and a warning is logged naming the article id.

This rule is the enforced expression of the anti-goal "no subscriber receives a blank email." The domain constructor already makes both-empty unreachable for well-formed articles; this is defense in depth at the delivery boundary.

### 4.4 Template model

The template is rendered from a dedicated view model, not the `Article` domain entity:

```csharp
public sealed record NewPostNotificationViewModel(
    string? TitleEn, string? SummaryEn,
    string? TitlePt, string? SummaryPt,
    string ArticleUrl, string UnsubscribeUrl)
{
    public bool ShowEnglish { get; }     // both TitleEn and SummaryEn non-empty
    public bool ShowPortuguese { get; }  // both TitlePt and SummaryPt non-empty
    public string Subject { get; }       // §4.1, including the dedup rule
}
```

**Rationale (builder's call):** it removes `@using MeuSitePessoal.Domain.Entities` from an Infrastructure template, makes §4.1 and §4.3 unit-testable without rendering Razor or mocking a mail sender, and guarantees the subject the handler sends and the title the body shows are derived from one source. It also follows the existing precedent of `NewsletterVerificationViewModel`.

### 4.5 Observability

The handler's log statements identify articles by `TitleEn`, never by a resolved or legacy title.

---

## 5. Contract: Full-text search

### 5.1 Index definitions

```sql
CREATE INDEX idx_articles_search_en ON "Articles"
    USING GIN (to_tsvector('english', "TitleEn" || ' ' || "SummaryEn"));

CREATE INDEX idx_articles_search_pt ON "Articles"
    USING GIN (to_tsvector('portuguese', "TitlePt" || ' ' || "SummaryPt"));
```

`idx_articles_search_gin` is dropped.

**`coalesce` is deliberately absent.** All four columns are `NOT NULL`, so it is unnecessary — and more importantly, PostgreSQL matches an expression index only when the query expression is *syntactically* equivalent. The expression above must match what EF Core emits character for character, or the index is created and never used. Verifying this against the real planner output, rather than assuming it, is a required acceptance step (§6).

### 5.2 Query shape

`ArticleSearchService` selects the dictionary from the active request language and queries that language's columns only:

| Active language | Vector | Query parser |
|---|---|---|
| starts with `pt` | `to_tsvector('portuguese', "TitlePt" || ' ' || "SummaryPt")` | `websearch_to_tsquery('portuguese', @term)` |
| otherwise | `to_tsvector('english', "TitleEn" || ' ' || "SummaryEn")` | `websearch_to_tsquery('english', @term)` |

Both the vector and the query use the same dictionary, so a search term is stemmed by the same rules as the text it is matched against.

**Deliberate behavior change:** search becomes scoped to the reader's active language. A reader browsing in Portuguese no longer matches English-only text. This follows from per-language dictionaries and is the intended contract: results are in the language the reader is reading. Recorded explicitly so it is reviewed as a product decision, not discovered as a side effect.

The non-PostgreSQL `Contains` fallback branch stays as-is for provider compatibility, and remains untested ground — integration tests run against real PostgreSQL and therefore exercise only the tsvector path.

---

## 6. Acceptance criteria

The track is complete when all of the following hold:

1. `Article` has no `Title`/`Content`/`Summary` members, and the solution compiles with no reference to them outside migration history.
2. A migration drops the three columns and `idx_articles_search_gin`, and creates `idx_articles_search_en` and `idx_articles_search_pt`.
3. `dotnet ef migrations has-pending-model-changes` reports no drift.
4. `EXPLAIN (ANALYZE, BUFFERS)` on a search executed through the application's own query path shows a **Bitmap Index Scan** on the language-appropriate index — not a Seq Scan. This is verified against the real emitted SQL, not a hand-written equivalent.
5. A test asserts the dispatched subject **contains** `TitleEn`, rather than matching any string.
6. A test renders the real `NewPostNotification` template through `RazorEmailTemplateService` and asserts the output contains `TitleEn`, `SummaryEn`, `TitlePt`, and `SummaryPt`.
7. Unit tests cover the subject deduplication rule (§4.1) and both single-language degradation branches (§4.3).
8. A test covers an article whose localized columns carry identical values in both languages — the shape every pre-Track-17 row has — through the newsletter path and the administrative listing path.
9. The full suite passes, and the frontend builds and lints clean.

---

## 7. Left to the builder

- Whether `ArticleResponse` retains computed `Title`/`Content`/`Summary` members or drops them. Determined by what the admin dashboard and edit page actually consume; both are reachable from `GetAllArticlesHandler` and `GetArticleByIdHandler`. If the frontend reads them, keep them as computed values with no fallback; if not, remove them and adjust the frontend types in the same change.
- Exact Razor markup, inline CSS, and separator styling for the two language blocks, consistent with the existing template's visual identity.
- Whether the pre-drop data audit (§migration safety, in the plan) is run as a one-off `psql` query or an EF-based check.
- Test file placement, following the existing `Unit/Application/...` and `Integration/...` conventions.

---

## 8. Constraints

- **Production data exists.** The column drop is irreversible and must be preceded by a verified backup and a data-audit query proving no row carries legacy text absent from the localized columns.
- Integration tests run against real PostgreSQL (`BaseIntegrationTest`). Index and tsvector behavior must be asserted there, never against the InMemory provider, which silently takes the `Contains` branch.
- The suite is currently sequential by necessity; this track must not introduce parallel-execution assumptions (roadmap 18.1.1 owns that change).
- Email templates are copied to the output directory (`CopyToOutputDirectory=Always`), so tests may render them from `AppContext.BaseDirectory`.
- No secret, connection string, or credential is introduced or relocated by this track.
