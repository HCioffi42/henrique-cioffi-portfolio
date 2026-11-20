# Track 17.5 — i18n Flat Schema Cleanup & Bilingual Newsletter: Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the newsletter render both language versions of an article, remove the superseded single-language columns from code and database, and restore index-backed full-text search per language.

**Architecture:** Three sequential movements. Tasks 1-3 fix the delivery path behind a dedicated view model, shipping the production bug fix independently. Task 4 is a safety gate. Tasks 5-6 remove the legacy columns and swap the search indexes — the model change and its migration are one atomic task because the database columns are `NOT NULL` with no default, so unmapping them in code without dropping them in the same deployment breaks every article insert. Task 7 records the outcome.

**Tech Stack:** .NET 8, EF Core + Npgsql, MediatR, RazorLight (email templates), xUnit + Moq + FluentAssertions, PostgreSQL full-text search.

**Spec:** `docs/specs/track-17.5-i18n-and-newsletter.md`

## Global Constraints

- Both localized columns are mandatory for well-formed articles; the domain constructor's six non-empty guard clauses are the enforced invariant.
- Integration tests run against real PostgreSQL via `BaseIntegrationTest`. Never assert tsvector or index behavior against the InMemory provider — it silently takes the `Contains` branch.
- Do not introduce parallel-execution assumptions in tests; the suite is sequential by necessity (roadmap 18.1.1 owns that).
- Subject line format: `{TitleEn} | {TitlePt}`, collapsing to a single title when the two are ordinally equal.
- Index expressions carry no `coalesce` — all four columns are `NOT NULL`, and the expression must match EF's emitted SQL exactly or the planner ignores the index.
- No secret, connection string, or credential is introduced or relocated by this track.
- Comments in English, third person, no AI attribution in commit messages.

## Review Focus

Five conditions the spec implies but does not pin. Each has a test assigned to the task owning the code.

1. **Language header case and region variants.** `Accept-Language: PT-BR` or `pt_BR` must resolve to Portuguese. The existing `.StartsWith("pt")` is ordinal case-sensitive, so `PT-BR` silently serves English. (Task 6)
2. **Accented Portuguese search terms.** `programação` and `programacao` must behave predictably against the `portuguese` dictionary; pin whichever behavior is real so it is a known contract rather than a surprise. (Task 6)
3. **Whitespace-only localized field.** `"   "` passes `IsNullOrEmpty` and would render a visually blank language block. The block-visibility check must treat whitespace as empty. (Task 1)
4. **Mixed population within one language.** Title present, summary empty — the whole language block disappears, silently dropping content a reader was meant to see. Pin the chosen behavior. (Task 1)
5. **HTML-special characters in titles.** A title containing `&`, `<`, or quotes reaches both the subject line and the HTML body; the body must escape and the subject must not. (Task 2)

---

### Task 1: Bilingual newsletter view model

Pure logic, no database or Razor. Owns the subject format, block visibility, and degradation rules from spec §4.1 and §4.3.

**Files:**
- Create: `MeuSitePessoal.Application/Common/Models/Email/NewPostNotificationViewModel.cs`
- Test: `MeuSitePessoal.Tests/Unit/Application/Newsletter/NewPostNotificationViewModelTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces:
  ```csharp
  namespace MeuSitePessoal.Application.Common.Models.Email;

  public sealed record NewPostNotificationViewModel(
      string? TitleEn, string? SummaryEn,
      string? TitlePt, string? SummaryPt,
      string ArticleUrl, string UnsubscribeUrl)
  {
      public bool ShowEnglish { get; }
      public bool ShowPortuguese { get; }
      public bool HasContent { get; }
      public string Subject { get; }
  }
  ```
  `ShowEnglish` is true when `TitleEn` and `SummaryEn` are both non-whitespace; `ShowPortuguese` likewise for the Portuguese pair. `HasContent` is `ShowEnglish || ShowPortuguese`. `Subject` follows spec §4.1 and is `string.Empty` when `HasContent` is false.

- [ ] **Step 1: Write the failing tests**

```csharp
[Fact] Subject_WithDistinctTitles_JoinsWithPipe()
    // ("Hello","s","Olá","r") => Subject == "Hello | Olá"

[Fact] Subject_WithIdenticalTitles_CollapsesToOne()
    // ("Hello","s","Hello","r") => Subject == "Hello"

[Fact] Subject_WithOnlyEnglishPopulated_UsesEnglishTitle()
    // ("Hello","s", null, null) => Subject == "Hello", ShowPortuguese == false

[Fact] Subject_WithOnlyPortuguesePopulated_UsesPortugueseTitle()
    // (null, null, "Olá","r") => Subject == "Olá", ShowEnglish == false

[Fact] HasContent_WithBothLanguagesEmpty_IsFalse()
    // (null,null,null,null) => HasContent == false, Subject == string.Empty

[Theory] [InlineData("   ")] [InlineData("\t")] [InlineData("")]
ShowEnglish_WithWhitespaceOnlyTitle_IsFalse(string title)
    // (title,"summary","Olá","resumo") => ShowEnglish == false, Subject == "Olá"
    // Review Focus 3

[Fact] ShowEnglish_WithTitleButEmptySummary_IsFalse()
    // ("Hello","", "Olá","resumo") => ShowEnglish == false, Subject == "Olá"
    // Review Focus 4 — pins that a half-populated language is suppressed, not half-rendered
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~NewPostNotificationViewModelTests"`
Expected: FAIL — `NewPostNotificationViewModel` does not exist.

- [ ] **Step 3: Implement the record**

Computed properties are initialized in the record body. Use `string.IsNullOrWhiteSpace` for every emptiness check (not `IsNullOrEmpty`) and `StringComparison.Ordinal` for the title equality test.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~NewPostNotificationViewModelTests"`
Expected: PASS (7 tests).

- [ ] **Step 5: Commit**

```bash
git add MeuSitePessoal.Application/Common/Models/Email/NewPostNotificationViewModel.cs MeuSitePessoal.Tests/Unit/Application/Newsletter/NewPostNotificationViewModelTests.cs
git commit -m "feat(newsletter): add bilingual notification view model"
```

---

### Task 2: Bilingual email template

Rewrites the template against the view model and proves it renders through the real engine.

**Files:**
- Modify: `MeuSitePessoal.Infrastructure/Templates/Email/NewPostNotification.cshtml` (full rewrite)
- Test: `MeuSitePessoal.Tests/Integration/Infrastructure/Services/NewPostNotificationTemplateTests.cs`

**Interfaces:**
- Consumes: `NewPostNotificationViewModel` from Task 1.
- Produces: the template now declares `@model MeuSitePessoal.Application.Common.Models.Email.NewPostNotificationViewModel` and no longer references `MeuSitePessoal.Domain.Entities`.

- [ ] **Step 1: Write the failing tests**

Instantiate `RazorEmailTemplateService` directly with `NullLogger<RazorEmailTemplateService>.Instance` and render `"Email/NewPostNotification"`. Templates are copied to the output directory, so no fixture setup is needed.

```csharp
[Fact] Render_WithBothLanguages_ContainsAllFourStrings()
    // asserts html contains TitleEn, SummaryEn, TitlePt, SummaryPt, and both CTA labels
    // "Read Full Article" and "Ler Artigo Completo"

[Fact] Render_WithOnlyEnglish_OmitsPortugueseBlock()
    // ShowPortuguese == false => html does NOT contain "Ler Artigo Completo"

[Fact] Render_EscapesHtmlSpecialCharactersInTitle()
    // TitleEn = "Tips & <Tricks>" => html contains "Tips &amp; &lt;Tricks&gt;"
    // and does NOT contain the raw "<Tricks>"
    // Review Focus 5

[Fact] Render_AlwaysContainsUnsubscribeUrl()
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~NewPostNotificationTemplateTests"`
Expected: FAIL — the template's model type does not match.

- [ ] **Step 3: Rewrite the template**

Structure per spec §4.2: masthead, English block guarded by `@if (Model.ShowEnglish)`, separator, Portuguese block guarded by `@if (Model.ShowPortuguese)`, footer with unsubscribe link. Both calls-to-action link to `@Model.ArticleUrl`. Preserve the existing inline-CSS class names (`container`, `button`, `footer`) so PreMailer inlining and the established visual identity carry over. Use `@Model.X` interpolation throughout — never `@Html.Raw` — so escaping holds.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~NewPostNotificationTemplateTests"`
Expected: PASS (4 tests).

- [ ] **Step 5: Commit**

```bash
git add MeuSitePessoal.Infrastructure/Templates/Email/NewPostNotification.cshtml MeuSitePessoal.Tests/Integration/Infrastructure/Services/NewPostNotificationTemplateTests.cs
git commit -m "feat(newsletter): render bilingual article notification template"
```

---

### Task 3: Wire the event handler

Completes roadmap item 17.5.1. After this task the production defect is fixed and shippable on its own.

**Files:**
- Modify: `MeuSitePessoal.Application/Articles/Events/ArticlePublishedEventHandler.cs:38-39` (log), `:78` (template model), `:83` (subject)
- Modify: `MeuSitePessoal.Tests/Unit/Application/Articles/Events/ArticlePublishedEventHandlerTests.cs`

**Interfaces:**
- Consumes: `NewPostNotificationViewModel` from Task 1.
- Produces: no signature change — `Handle` keeps its `INotificationHandler<ArticlePublishedEvent>` contract.

- [ ] **Step 1: Strengthen the existing tests and add new ones**

The two existing tests assert `It.IsAny<string>()` for the subject, which passes with a blank subject — that weak assertion is why this defect reached production. Replace the subject argument matcher in `Handle_WithActiveSubscribers_ShouldSendEmails` with one that captures and asserts content, then add:

```csharp
[Fact] Handle_SendsSubjectContainingBothTitles()
    // article ("Hello","Olá",...) => captured subject == "Hello | Olá"

[Fact] Handle_WithIdenticalTitles_SendsSingleTitleSubject()
    // article built with the same title in both languages (the shape every
    // pre-Track-17 row has) => captured subject == "Hello", not "Hello | Hello"

[Fact] Handle_PassesViewModelCarryingLocalizedFields_ToTemplateService()
    // verifies RenderTemplateAsync is called with a NewPostNotificationViewModel
    // whose TitleEn/TitlePt/SummaryEn/SummaryPt match the article
```

Capture arguments with `Moq`'s `Callback` or `It.Is<string>(s => ...)`.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~ArticlePublishedEventHandlerTests"`
Expected: FAIL — subject is currently `"New Post:  - hcioffi.dev"`, carrying no title.

- [ ] **Step 3: Update the handler**

Build a `NewPostNotificationViewModel` from the article and the two URLs, pass it to `RenderTemplateAsync` in place of the current tuple, and send `viewModel.Subject`. Skip the subscriber loop entirely and log a warning naming the article id when `HasContent` is false (spec §4.3). Change the log statements at lines 38-39 to report `TitleEn`.

- [ ] **Step 4: Run the full suite**

Run: `dotnet test`
Expected: PASS. A failure in `GetAllArticlesHandler` tests here would mean an unrelated regression — investigate before continuing.

- [ ] **Step 5: Commit**

```bash
git add MeuSitePessoal.Application/Articles/Events/ArticlePublishedEventHandler.cs MeuSitePessoal.Tests/Unit/Application/Articles/Events/ArticlePublishedEventHandlerTests.cs
git commit -m "fix(newsletter): send bilingual subject and body from localized fields"
```

---

### Task 4: Pre-drop safety gate

No code. Produces two artifacts that make the next task reversible. **Do not proceed to Task 5 until both steps below have succeeded against production data.**

- [ ] **Step 1: Take and verify a production backup of the Articles table**

```bash
docker exec <postgres-container> pg_dump -U <user> -d <database> -t '"Articles"' \
  > backups/articles-pre-17.5-$(date +%Y%m%d).sql
```

Confirm the container name, user, and database against the deployed `docker-compose.yml` before running — do not assume. Verify the dump is non-empty and contains a `COPY "Articles"` block with the expected row count.

- [ ] **Step 2: Run the data audit query**

Proves no row carries legacy text that exists nowhere else, which is what makes the drop safe:

```sql
SELECT COUNT(*) AS rows_at_risk FROM "Articles"
WHERE (BTRIM(COALESCE("Title", ''))   <> '' AND BTRIM(COALESCE("TitleEn", ''))   = '' AND BTRIM(COALESCE("TitlePt", ''))   = '')
   OR (BTRIM(COALESCE("Summary", '')) <> '' AND BTRIM(COALESCE("SummaryEn", '')) = '' AND BTRIM(COALESCE("SummaryPt", '')) = '')
   OR (BTRIM(COALESCE("Content", '')) <> '' AND BTRIM(COALESCE("ContentEn", '')) = '' AND BTRIM(COALESCE("ContentPt", '')) = '');
```

Expected: `rows_at_risk = 0`.

**If the count is non-zero, stop.** Those rows hold text that the drop would destroy. Report the ids and await a decision on backfilling them before continuing.

- [x] **Step 3: Record the result**

**Completed and verified against production on 2026-11-20:**

| Item | Result |
|---|---|
| Backup | `C:\Users\henri\backups\MySitePRD\articles-pre-17.5-20261120.sql` (stored outside the repository) |
| `total_articles` | 4 |
| `rows_at_risk` | **0** |

All four production rows already carry complete `TitleEn`/`TitlePt`, `SummaryEn`/`SummaryPt`, and `ContentEn`/`ContentPt` values, so dropping the legacy columns destroys no text that exists nowhere else. The gate is satisfied and Task 5 is unblocked.

---

### Task 5: Remove legacy columns (model and migration)

Model change and migration ship together: the database columns are `NOT NULL` with no default, so unmapping them in code without dropping them in the same deployment makes every `INSERT` fail.

**Files:**
- Modify: `MeuSitePessoal.Domain/Entities/Article.cs` — remove lines 20-25 (properties), 62-66 (constructor assignments), and the `[Obsolete]` constructor at 73-101
- Modify: `MeuSitePessoal.Infrastructure/Data/BlogDbContext.cs:41-44` — remove the three legacy `Property` configurations
- Modify: `MeuSitePessoal.Application/Common/Mappings/MappingProfile.cs:21-23` — remove the three `.ForMember(... Ignore())` clauses
- Modify: `MeuSitePessoal.Application/Articles/Queries/GetAllArticles/GetAllArticlesHandler.cs:33-53` — remove the legacy fallbacks
- Create: `MeuSitePessoal.Infrastructure/Migrations/<timestamp>_RemoveLegacyArticleColumns.cs` (scaffolded)
- Test: `MeuSitePessoal.Tests/Unit/Application/Articles/Queries/GetAllArticlesHandlerTests.cs`

**Interfaces:**
- Consumes: nothing from earlier tasks.
- Produces: `Article` exposes only the six localized content properties and a single public eight-argument constructor. `ArticleResponse` keeps its current positional signature unless Step 1 finds no consumer for its resolved members.

**Verified before planning:** the `[Obsolete]` five-argument constructor has **zero call sites** — all twenty `new Article(...)` usages across `CreateArticleHandler`, `DbInitializer`, and the test suite already pass the eight-argument localized form. Removing it is a deletion, not a migration of callers.

- [ ] **Step 1: Determine the `ArticleResponse` decision (spec §7)**

Check whether the frontend reads the resolved `title`/`content`/`summary` members: search `meupessoal-web/src` for uses of the article detail and dashboard payloads. Keep the members as values resolved from the active language with no fallback if they are consumed; remove them and update the corresponding TypeScript types in the same commit if they are not. Record the choice in a comment on the task.

- [ ] **Step 2: Write the failing tests**

```csharp
[Fact] Handle_ResolvesPortugueseFields_WhenLanguageIsPt()
    // language provider returns "pt-BR"; asserts the projected response carries
    // TitlePt/SummaryPt/ContentPt values

[Fact] Handle_ResolvesEnglishFields_WhenLanguageIsEn()

[Fact] Handle_WithIdenticalLocalizedValues_ReturnsThatValue()
    // the pre-Track-17 backfilled row shape: TitleEn == TitlePt
```

- [ ] **Step 3: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~GetAllArticlesHandlerTests"`
Expected: FAIL (or fail to compile once Step 4 lands — either is a valid RED).

- [ ] **Step 4: Apply the model changes**

Remove the properties, constructor assignments, obsolete constructor, EF configurations, AutoMapper ignores, and handler fallbacks listed under **Files**. Resolve the handler projection to the plain ternary in spec §3.3. Fix every resulting compile error; the compiler is the authority on which call sites remain.

- [ ] **Step 5: Verify the solution compiles**

Run: `dotnet build`
Expected: Build succeeded, 0 errors.

- [ ] **Step 6: Scaffold the migration**

```bash
dotnet ef migrations add RemoveLegacyArticleColumns \
  --project MeuSitePessoal.Infrastructure --startup-project MeuSitePessoal.Api
```

EF generates the three `DropColumn` calls. Hand-edit the generated `Up` to drop the dependent index **first**, so the dependency is explicit rather than relying on PostgreSQL's implicit cascade:

```csharp
migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_articles_search_gin;");
// ... generated DropColumn calls follow ...
```

Make `Down` restore the three columns as nullable (the original text is not recoverable from the schema, and the Task 4 backup is the real restore path) and recreate `idx_articles_search_gin` against them.

- [ ] **Step 7: Verify no model drift**

Run: `dotnet ef migrations has-pending-model-changes --project MeuSitePessoal.Infrastructure --startup-project MeuSitePessoal.Api`
Expected: "No changes have been made to the model since the last migration."

- [ ] **Step 8: Run the full suite**

Run: `dotnet test`
Expected: PASS. Integration tests re-migrate the database per test, so a schema error surfaces here immediately.

- [ ] **Step 9: Commit**

```bash
git add -A
git commit -m "refactor(articles): remove legacy single-language columns"
```

---

### Task 6: Per-language search dictionary and index

Creates the two GIN indexes and matches the query to them. Completes roadmap items 17.5.2 and 17.5.3.

**Files:**
- Create: `MeuSitePessoal.Infrastructure/Migrations/<timestamp>_AddLocalizedSearchIndexes.cs` (empty scaffold, hand-written SQL)
- Modify: `MeuSitePessoal.Infrastructure/Services/ArticleSearchService.cs:27-43`
- Test: `MeuSitePessoal.Tests/Integration/Articles/ArticleSearchTests.cs`

**Interfaces:**
- Consumes: the localized-only `Article` from Task 5.
- Produces: no signature change to `IArticleSearchService.SearchAsync`.

- [ ] **Step 1: Write the failing tests**

Add to the existing integration test class, which already runs against real PostgreSQL:

```csharp
[Fact] Search_InPortuguese_MatchesPortugueseText()
[Fact] Search_InEnglish_MatchesEnglishText()

[Fact] Search_InPortuguese_DoesNotMatchEnglishOnlyText()
    // pins the deliberate scoping change in spec §5.2

[Theory] [InlineData("pt")] [InlineData("pt-BR")] [InlineData("PT-BR")] [InlineData("pt_BR")]
Search_ResolvesPortuguese_ForLanguageHeaderVariants(string lang)
    // Review Focus 1 — "PT-BR" currently falls through to English

[Fact] Search_WithAccentedPortugueseTerm_BehavesConsistently()
    // Review Focus 2 — assert the real behavior of the 'portuguese' dictionary
    // for "programação" against content containing "programação"; if unaccented
    // input does not match, assert that explicitly so the contract is recorded
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~ArticleSearchTests"`
Expected: FAIL on the language-variant and cross-language cases.

- [ ] **Step 3: Write the index migration**

Scaffold an empty migration, then hand-write `Up` with the two `CREATE INDEX` statements exactly as given in spec §5.1 — no `coalesce`, `english` dictionary over the `En` pair, `portuguese` over the `Pt` pair. `Down` drops both.

- [ ] **Step 4: Update the search service**

Replace the concatenated-vector `Where` clause at lines 31-34 with a branch on the active language that queries one language's vector with the matching dictionary, per spec §5.2. Normalize the language check to be case-insensitive (`StartsWith("pt", StringComparison.OrdinalIgnoreCase)`) so region and case variants resolve correctly. Leave the non-Npgsql `Contains` fallback branch untouched.

- [ ] **Step 5: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~ArticleSearchTests"`
Expected: PASS.

- [ ] **Step 6: Verify the planner actually uses the index**

This is the step that distinguishes a real fix from a cosmetic one. Capture the SQL the application emits — enable EF command logging or read it from the test output — then run it against the test database prefixed with `EXPLAIN (ANALYZE, BUFFERS)`.

Expected: a **Bitmap Index Scan** on `idx_articles_search_en` or `idx_articles_search_pt`.
If the output shows `Seq Scan`, the emitted expression does not match the index expression — compare them character by character (EF may emit `COALESCE` around concatenation) and reconcile the migration's DDL to what EF actually produces, rather than adjusting the query to match a guess.

Record the confirmed plan output in the commit body.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "perf(search): add per-language GIN indexes and match query dictionary"
```

---

### Task 7: Record outcomes in the roadmap

**Files:**
- Modify: `docs/roadmap.md` — Track 17.5 checkboxes, the Track 13 and Track 17 regression notes, Phase 4 status, and a new Track 19 item

- [ ] **Step 1: Update Track 17.5**

Tick 17.5.1 through 17.5.4. Replace the Track 17 `[✗→!]` heading and its regression bullet with a resolved note pointing at this plan and the spec. Remove the audit P1 half of the Track 13 known-regression bullet, leaving the P9 debounce item, which this track does not address.

- [ ] **Step 2: Add the deferred follow-up as Track 19.8**

```markdown
- [ ] **19.8**: Capture `PreferredLanguage` at newsletter signup (frontend sends the active
  i18next language; `SubscribeToNewsletterCommand` accepts and persists it) and switch the
  article notification from a single bilingual email to a per-subscriber localized one.
  Deferred from Track 17.5: the column exists but is populated for no subscriber, so
  localizing per recipient today would deliver English to everyone.
```

- [ ] **Step 3: Re-evaluate the Phase 4 status line**

Phase 4 was marked REOPENED pending 17.5 and 18.0. With 17.5 closed, update the heading and the progress note to reflect that 18.0 is the sole remaining blocker.

- [ ] **Step 4: Commit**

```bash
git add docs/roadmap.md docs/plans/track-17.5-implementation-plan.md
git commit -m "docs: close Track 17.5 and defer per-subscriber newsletter language to 19.8"
```

---

## Verification Summary

Run before declaring the track complete:

```bash
dotnet build
dotnet test
dotnet ef migrations has-pending-model-changes --project MeuSitePessoal.Infrastructure --startup-project MeuSitePessoal.Api
cd meupessoal-web && npm run lint && npm run build
```

Expected: build succeeds; full suite passes; no pending model changes; frontend lints and builds clean.

Acceptance criteria 1-9 in spec §6 are satisfied by Tasks 1-6; criterion 4 (planner uses the index) is specifically Task 6 Step 6 and is the one that cannot be confirmed from a passing test alone.
