# Spec: Track 18.0 — Security Hardening

**Status**: Approved
**Date**: 2026-11-20
**Related**: `docs/roadmap.md` Track 18.0
**Implementation plan**: `docs/plans/track-18.0-implementation-plan.md`

This document describes the **target state**: the contracts the system must satisfy once Track 18.0 lands. It is written to be implementable from the described end state alone. Sequencing lives in the roadmap.

---

## 1. Intent

Track 18.0 moves five platform concerns from implicit to explicit: where the JWT signing key comes from, how the application is reached, how it identifies a client behind a proxy, how often a client may call a sensitive endpoint, and who may manage uploaded media.

**Why now:** Track 18.2 runs an automated browser suite against a shared environment and will exercise authentication repeatedly and at speed. Every behavior in this track is a precondition for that suite producing meaningful results rather than noise.

### Goals

- The JWT signing key is supplied exclusively by the deployment environment. The application does not start without a usable one.
- The API is reachable only through the reverse proxy. Service-to-service traffic stays on the Docker bridge network.
- The application resolves the originating client address accurately when running behind Cloudflare and nginx.
- Sign-in, registration, password recovery, comment creation, and newsletter subscription enforce a per-client request budget.
- Failed password attempts count toward Identity's lockout counter for every account that has lockout enabled.
- Media upload and deletion are restricted to the `Admin` role.

### Non-goals

- Edge throttling at Cloudflare or in `nginx.conf`. Viable as later defense in depth; deliberately excluded so that one enforcement point owns the behavior and the test suite can assert on it.
- Token refresh and revocation. Tracked separately.
- Security response headers, the production CORS origin, and error-response shaping. Each is independently scoped and none is a precondition for Track 18.2.
- Distributed (Redis-backed) rate limiting. The API runs as a single instance; this is revisited only alongside horizontal scaling.
- An ownership model for uploaded media. Role restriction fully covers the current authorship model.

### Anti-goals

Outcomes that would make this track a failure even if the code compiles and the suite passes:

- A limiter that cannot distinguish one client from another, so a single caller's traffic degrades service for every reader. This is worse than no limiter, and it is the default outcome if client-address resolution is not solved first.
- A lockout policy that allows the sole administrative account to be rendered unusable by an unauthenticated third party.
- Thresholds a genuine reader meets during normal use.
- A deployment that starts successfully with an unconfigured or placeholder signing key.
- A signing key removed from configuration whose absence then surfaces as an unrelated error.

---

## 2. Approaches considered

### Client address resolution

| Approach | Trade-off |
|---|---|
| **nginx normalizes, the application trusts one hop** (chosen) | nginx resolves the true client address once and presents it as a single-entry `X-Forwarded-For`; the application trusts exactly one upstream hop. No external address list to maintain in .NET, and one place owns the decision. Splits the logic across `nginx.conf` and `Program.cs`. |
| Application parses the full chain, trusting Cloudflare ranges directly | Keeps the logic in one file, but requires Cloudflare's published ranges to be embedded in application configuration and kept current; a stale list silently degrades to misattribution. Rejected. |
| Application reads the vendor header directly, with no forwarded-headers middleware | Fewest moving parts, but `HttpContext.Connection.RemoteIpAddress` stays wrong for logging and for any future consumer, and the header is trusted with no hop model at all. Rejected. |

### Request throttling

| Approach | Trade-off |
|---|---|
| **Stock `AddRateLimiter` with named per-endpoint policies** (chosen) | No new dependency, per-endpoint thresholds, and — decisively — assertable from the existing xUnit suite, so the behavior is pinned by tests the way every other contract in this repository is. In-memory, so counters reset on restart and do not span instances. Endpoints added later are unprotected until explicitly annotated. |
| One global partitioned limiter across all endpoints | Covers endpoints nobody remembered to annotate, but article listing and search are the hot read paths; a single threshold either sits too high to matter for sign-in or too low for normal reading. Rejected. |
| Edge throttling (Cloudflare rules or nginx `limit_req`) | Rejects traffic before it reaches the origin and survives restarts, but is not exercisable by the test suite, and addresses only the throttling half of this track. Retained as a later complement, not a substitute. Deferred. |

### Administrative account lockout

| Approach | Trade-off |
|---|---|
| **Per-user `LockoutEnabled` flag** (chosen) | Uses Identity's own mechanism. `CheckPasswordSignInAsync` is called with `lockoutOnFailure: true` unconditionally and Identity honors the per-user flag, so the handler carries no branching and no extra role query per sign-in. The policy is visible as data on the user row. |
| Role lookup in the handler, `lockoutOnFailure: !isAdmin` | Equivalent outcome, but adds a role query to every sign-in and embeds account policy in request-handling code. Rejected. |
| Lockout enabled for all accounts | Uniform, but allows an unauthenticated third party who knows the administrative username to make the site's only administrative account unusable on demand. Rejected — the control becomes the outage. |

### Media endpoint authorization

| Approach | Trade-off |
|---|---|
| **Role restriction to `Admin`** (chosen) | One attribute. Matches the authorship model exactly: only administrators write articles, and the article editor is the only producer of uploads. |
| Ownership model (an `UploadedImage` record checked on delete) | More principled, and necessary the moment readers gain upload ability, but introduces an entity, a migration, and a data-access path to express a rule with no current beneficiary. Deferred until readers can upload. |

---

## 3. Contract: configuration and deployment surface

### 3.1 `JwtSettings`

`appsettings.json` retains `Issuer`, `Audience`, and `ExpiresInMinutes`. It carries **no `Key` member**. The key is supplied per environment:

| Environment | Source |
|---|---|
| Production | `JwtSettings__Key` environment variable, provided by `docker-compose.yml` from `${JWT_SIGNING_KEY}` |
| Local development | .NET user secrets (`UserSecretsId` is already declared in `MeuSitePessoal.Api.csproj`) |
| Testing | `JwtSettings__Key` environment variable, set by the integration-test fixture before any host is constructed |

**Why Testing uses the environment rather than the fixture's in-memory configuration:** the key is resolved by startup code that runs while the application builder is being configured — before `builder.Build()`. `WebApplicationFactory` merges its `ConfigureAppConfiguration` sources *at* `Build()`, so a build-time read never observes them. This is why the fixture's existing `AdminSetup` and `ClientSettings` entries work: those are read at request time, after the merge. An environment variable is read by `WebApplication.CreateBuilder` immediately, so it is the only source that reaches a build-time read, and it exercises the same mechanism production uses.

The fixture also keeps a matching in-memory entry so that any consumer resolving the key after the host is built sees the same value.

**The test host must also declare its environment.** `Program.cs:27` loads .NET user secrets whenever the `ASPNETCORE_ENVIRONMENT` process variable is absent, which it is under `dotnet test` — the fixture sets the environment through `UseEnvironment`, which is not the same thing. User secrets rank above environment variables, so a developer with a local `JwtSettings:Key` would have that key validate tokens while the fixture's key signed them, and every authenticated test would fail with 401 on a machine where the documented local setup had been followed. The fixture therefore sets `ASPNETCORE_ENVIRONMENT=Testing` before building any host, which keeps the suite hermetic from local configuration. This is a pre-existing property of the startup sequence that this track's build-time key read makes consequential.

### 3.2 Startup validation

`Program.cs` validates the key before building the authentication pipeline, and throws a descriptive exception when it is absent or too short:

- Missing, empty, or whitespace: fail.
- Present but under 32 bytes when UTF-8 encoded: fail. HMAC-SHA256 requires a key at least as long as its 256-bit output; a shorter key is otherwise rejected by the signing library later, at a less obvious point.

The exception message names the configuration path and the acceptable sources. It must not echo any observed value.

**Current behavior, for contrast:** `Program.cs:72` already dereferences the key during startup, so an absent key aborts startup today — but as an `ArgumentNullException` giving no indication of what is missing. This contract replaces an opaque failure with a named one; it does not change *when* the failure occurs.

### 3.3 Network exposure

`docker-compose.yml` publishes no host ports for `database` or `backend`. Both remain on the `blog-network` bridge and are addressed by service name. `frontend` continues to publish `80` and remains the only ingress.

| Service | Before | After |
|---|---|---|
| `database` | `5432:5432` | no published port |
| `backend` | `5000:8080` | no published port |
| `frontend` | `80:80` | unchanged |

**Verified:** no script, configuration file, or frontend source references host port `5000`. Outside development mode the frontend resolves its API base to the relative path `/api` (`api.ts:8-11`) and reaches the backend through nginx; local development targets the .NET development server on `25683` and never the container port.

`BaseIntegrationTest.cs:25` does reference `localhost:5432`, in a connection string rather than a URL. It is unaffected: CI supplies PostgreSQL as a GitHub Actions service container, and the Compose database declares a different role name than the one that connection string uses, so the suite was never reaching the Compose container. A developer who had been pointing local tooling at the published database port must now connect through `docker exec` instead.

**Operational consequence:** diagnostic checks that previously ran against `http://localhost:5000/health` on the host must address the container directly (`docker exec meu_site_api ...`) or route through nginx. This is a deliberate narrowing of the ingress surface.

### 3.4 Compose secret pattern

`JwtSettings__Key=${JWT_SIGNING_KEY}` joins the existing `${VAR}` substitutions on the `backend` service, matching the established pattern for the database connection string and mail credentials. No secret value appears in any committed file.

---

## 4. Contract: client address resolution

### 4.1 Resolution chain

The deployed request path is `client → Cloudflare → nginx → API`. Each hop must hand the next an unambiguous client address.

1. **nginx** sets `X-Forwarded-For` to the address Cloudflare attributes to the client (`CF-Connecting-IP`) when that header is present **and its own peer is a Cloudflare address** (§4.4), and to its observed peer address otherwise. The result is a single-entry chain, not an accumulated list.
2. **The application** enables forwarded-headers processing for `X-Forwarded-For` and `X-Forwarded-Proto`, trusting exactly **one** hop, with the container bridge network as the known network.

After processing, `HttpContext.Connection.RemoteIpAddress` is the client address for every consumer — throttling partitions, request logs, and anything added later.

### 4.2 Pipeline position

Forwarded-headers processing is the **first** middleware, ahead of request logging, so log entries carry the resolved address. Throttling is registered after routing, so that endpoint-level policy metadata is available, and before authentication, so that unauthenticated floods are rejected before any identity work is done.

Resulting order: forwarded headers → request logging → routing → rate limiter → authentication → authorization → endpoints.

### 4.3 Unresolvable address

When no address can be resolved — which includes the in-memory test host, where there is no real connection — throttling partitions under a single constant key. Production traffic always carries a resolved address, because nginx is the only ingress.

### 4.4 Conditional trust of the vendor header

nginx honors `CF-Connecting-IP` **only when its own peer address belongs to Cloudflare's published ranges.** For any other peer the header is ignored and the observed peer address is used instead.

This is not optional refinement. Without it the trust chain inverts: a caller reaching the origin's HTTP port directly, bypassing Cloudflare, can present any `CF-Connecting-IP` they like and be attributed to an address of their choosing. Rotating that header per request yields an unlimited number of distinct partitions, and every budget in §5 becomes unenforceable — the limiter would still run, still log, still return 429 under test, and still protect nothing.

The interaction with §6.3 is what makes this load-bearing rather than theoretical. The administrative account is deliberately exempt from lockout, which leaves throttling as the only control applying to it. A forwarded address that the caller chooses is therefore an unbounded password-guessing budget against the one account that cannot lock — a worse position than before this track, where that account at least had no exemption. The exemption and the conditional trust must ship together or neither should ship.

Implemented with nginx's real-IP module (`set_real_ip_from` over Cloudflare's ranges, `real_ip_header CF-Connecting-IP`), which is compiled into the stock image. The range list requires periodic refresh; it changes rarely, and a stale entry degrades to using the observed peer address rather than to misplaced trust.

**Remaining residual risk:** a caller reaching the origin directly is still attributed to their own address rather than being refused outright. Restricting origin ingress on `80`/`443` to Cloudflare's ranges at the network layer closes that and is recorded as follow-up work. Removing the backend's published port (§3.3) eliminates the equivalent path to the API itself.

---

## 5. Contract: request rate limiting

### 5.1 Policies

Six named fixed-window policies, applied as endpoint metadata on specific actions and partitioned by resolved client address (§4).

| Policy | Endpoint | Permits | Window |
|---|---|---|---|
| `auth-signin` | `POST /api/auth/login` | 10 | 15 min |
| `auth-verify` | `POST /api/auth/reset-password` | 5 | 15 min |
| `auth-register` | `POST /api/auth/register` | 5 | 60 min |
| `auth-recovery` | `POST /api/auth/forgot-password` | 5 | 60 min |
| `comments-write` | `POST /api/comments` | 10 | 5 min |
| `newsletter-subscribe` | `POST /api/newsletter/subscribe` | 3 | 60 min |

Thresholds are chosen so a genuine user does not meet them: ten sign-in attempts in a quarter hour exceeds normal mistyping, ten comments in five minutes exceeds conversational pace, and registration, recovery, and subscription each trigger an outbound email and are naturally rare.

`auth-verify` covers the endpoints that verify a secret the caller supplies rather than a password. It is **deliberately a separate policy rather than a reuse of `auth-signin`**, so that a caller burning through reset attempts never consumes the sign-in allowance of a legitimate user sharing the same address. Five attempts per quarter hour is generous for a user following a reset link once and ruinous for anyone enumerating tokens.

> **2026-11-21 amendment.** As written in Track 18.0, this policy also covered `POST /api/auth/verify-2fa`, and its separation from `auth-signin` was argued from the two-step cost of a two-factor sign-in. Roadmap 18.1.5 removed two-factor authentication, so that endpoint no longer exists and that argument no longer applies. The policy and its thresholds are unchanged; only the endpoint list and the rationale above were updated. Six named policies now cover six endpoints rather than seven.

No queueing. A request over budget is rejected immediately rather than delayed.

### 5.2 Rejection behavior

- Status **429**. This must be set explicitly; the framework default is 503, which would misrepresent throttling as unavailability and mislead both monitoring and the Track 18.2 suite.
- `Retry-After` is emitted from the limiter's metadata. For a fixed-window limiter this value is the **full window length**, not the remaining time — the framework exposes no remainder. The header is still emitted, because a well-behaved client needs a backoff signal and a caller can measure the window by probing regardless. The window length is therefore not treated as information worth withholding.
- The body is a short, fixed message disclosing no threshold or counter state.
- Each rejection logs at Warning with the endpoint and the partition key.

### 5.3 Configuration

Thresholds bind from configuration under `RateLimiting`, never from literals:

```json
"RateLimiting": {
  "Policies": {
    "auth-signin":          { "PermitLimit": 10, "WindowMinutes": 15 },
    "auth-verify":          { "PermitLimit": 5,  "WindowMinutes": 15 },
    "auth-register":        { "PermitLimit": 5,  "WindowMinutes": 60 },
    "auth-recovery":        { "PermitLimit": 5,  "WindowMinutes": 60 },
    "comments-write":       { "PermitLimit": 10, "WindowMinutes": 5  },
    "newsletter-subscribe": { "PermitLimit": 3,  "WindowMinutes": 60 }
  }
}
```

Startup fails if a policy referenced by an endpoint has no configured entry. An endpoint annotated with a policy that silently does not exist would appear protected and enforce nothing.

### 5.4 Behavior under test

The limiter is **registered and active in every environment**, including Testing, so that a wiring regression is caught rather than hidden. What differs is the thresholds.

xUnit constructs a new test-class instance per test method, and `BaseIntegrationTest.InitializeAsync` builds a fresh host for each one, so limiter state does **not** accumulate across a run. The exposure is narrower than a whole suite sharing one budget: it is a single test method making several calls to a limited endpoint, where `AuthenticateAsync` and the test's own requests both land in the same partition, because the in-memory host resolves no client address (§4.3) and everything therefore shares one constant key.

The contract:

- The Testing environment supplies deliberately generous thresholds, high enough that no single test can reach them. This is cheap insurance rather than a response to a known failure, and it also holds if a shared-fixture pattern is ever introduced.
- Tests that assert rejection supply their own deliberately small thresholds and exercise the real limiter.
- Disabling the limiter under Testing is **not** an acceptable alternative: it would leave the middleware, the policy wiring, and the rejection path unexercised.

**Consequence for Track 18.2:** a browser suite authenticating from one runner address shares a single partition against production thresholds and would exhaust `auth-signin` within ten sign-ins. Thresholds bind from configuration (§5.3) precisely so the environment that suite runs against can declare its own. That environment's configuration is 18.2's to set, not this track's — but the suite must not be pointed at production thresholds and expected to pass.

---

## 6. Contract: account lockout

### 6.1 Policy

Identity lockout options are declared explicitly rather than inherited from framework defaults:

| Option | Value |
|---|---|
| `MaxFailedAccessAttempts` | 5 |
| `DefaultLockoutTimeSpan` | 15 minutes |
| `AllowedForNewUsers` | `true` |

### 6.2 Sign-in

`LoginCommandHandler` calls `CheckPasswordSignInAsync` with `lockoutOnFailure: true` unconditionally. Whether a given account locks is governed by that account's `LockoutEnabled` column, which is Identity's own mechanism for the distinction.

### 6.3 Administrative exemption

Accounts holding the `Admin` role have `LockoutEnabled = false`.

This is applied in `DbInitializer.SeedAsync` as an **unconditional convergence step** on the configured administrative account, evaluated on every startup: read the current value, and write only when it differs.

Attaching it to the creation and promotion branches instead would not work, and the reason is worth stating because the resulting defect is invisible to the test suite. `DbInitializer.cs:77` guards promotion with `!await userManager.IsInRoleAsync(adminUser, "Admin")`. The deployed administrator is already in the role, so that branch never runs again — the flag would never be applied in production. Meanwhile every integration test drops and re-seeds the database, so the administrator is always *created* fresh, the creation branch always runs, and the test asserting `LockoutEnabled == false` passes. A green suite would certify a production account that remained lockable.

**Verified:** `DbInitializer.cs:80` is the only site in non-test code that assigns the `Admin` role. The two other role assignments (`RegisterCommandHandler.cs:70`, `AuthController.cs:244`) assign `Reader`. There is no runtime promotion path, so the seeder is the only place this rule needs to exist — but it must be placed outside the role-assignment conditional, not inside it.

**Rationale, recorded so it is reviewed rather than discovered:** the administrative username is discoverable and the site has a single administrator. With lockout active for that account, any unauthenticated caller could make the site unadministrable at will by submitting failed attempts. Throttling (§5) is the control that applies to this account: it expires without intervention and cannot be aimed at a specific identity.

### 6.4 Response to a locked-out account

The response to a locked-out sign-in is identical to the response to a wrong password: no token, no distinguishing field, no distinguishing status. The existing account-enumeration-safe posture is preserved.

The two cases are distinguished **in logs only**, at Warning, naming the account.

### 6.5 Counter state on exempt accounts

`UserManager.AccessFailedAsync` increments `AccessFailedCount` and, at the threshold, writes a future `LockoutEnd` without consulting `LockoutEnabled`; only `IsLockedOutAsync` honors the flag. An exempt administrator therefore signs in successfully while carrying lockout state on its row.

This is functionally harmless today and actively misleading later: anyone reading the row sees what looks like a locked account, and re-enabling `LockoutEnabled` on that account at any future point would lock it immediately with no failed attempt in between.

The seeder's convergence step (§6.3) therefore also clears `LockoutEnd` and `AccessFailedCount` whenever it finds lockout state present on an exempt account, so the row reflects the policy that actually applies to it.

---

## 7. Contract: media endpoint authorization

`ImagesController` requires the `Admin` role at the controller level, covering both upload and delete.

| Caller | Upload | Delete |
|---|---|---|
| Anonymous | 401 | 401 |
| Authenticated, `Reader` | 403 | 403 |
| Authenticated, `Admin` | 200 | 204 |

**Explicitly unchanged:** `LocalStorageService.DeleteFileAsync` passes the supplied URL through `Path.GetFileName`, which collapses any directory component, so a traversal attempt resolves to a bare filename inside the uploads directory. Path handling is already contained; the gap this track closes is authorization alone. No defensive path-parsing code is added, because adding it would imply a containment problem that does not exist.

---

## 8. Contract: public documentation surface

This repository is public. Two committed documents currently describe weaknesses in the deployed system in operational detail: `docs/TECHNICAL_AUDIT_REPORT.md` §3.3, and `docs/roadmap.md` Track 18.0 and Track 14.

For the items this track closes, those descriptions become historical the moment the release deploys, and ticking the roadmap boxes is sufficient. For the items it does not close, they remain a current description of the live system.

Target state:

- `docs/TECHNICAL_AUDIT_REPORT.md` §3.3 is replaced by a short note stating that security findings are tracked privately, with the finding count and audit date retained so the rest of the report's cross-references still make sense.
- The `S#` identifiers remain referenced from `docs/roadmap.md` and `CLAUDE.md` as opaque labels. They are the key by which the private record is addressed; without them, existing cross-references dangle.
- `docs/roadmap.md` Track 18.0 entries lose their parenthetical descriptions of current weakness and are written as completed capability.
- The Track 14 known-gap note is rewritten as closed.
- Every other section of the audit report — performance, architecture, test health, the strengths section, the sequencing proposal — is retained unchanged. Those are not descriptions of exploitable weakness and they are load-bearing for Tracks 18.1, 18.2 and 19.

**Recorded honestly:** this reduces casual discovery, nothing more. The content remains in git history and in any existing clone, fork, or archive. It is not a remedy, and it must not be described as one, in the PR or anywhere else. The actual mitigation for the unpatched items is patching them.

---

## 9. Acceptance criteria

The track is complete when all of the following hold:

1. `appsettings.json` contains no `JwtSettings:Key` member, and no committed file in the repository contains a signing key value.
2. Startup fails with a message naming `JwtSettings:Key` when the key is absent, empty, whitespace, or shorter than 32 bytes, and succeeds when a valid key is supplied. Covered by tests over the validation routine.
3. `docker-compose.yml` passes `JwtSettings__Key` from `${JWT_SIGNING_KEY}` and publishes no host port for `database` or `backend`.
4. The resolved client address reflects the forwarded chain rather than the immediate peer, asserted by a test that sends a forwarded header and observes the partition the request is attributed to.
5. Each of the seven endpoints in §5.1 returns **429**, not 503, on the request following its configured budget, asserted against a host configured with small thresholds.
6. A request under budget on the same endpoint returns its normal status, proving the limiter rejects selectively rather than universally.
7. The full existing suite passes with the limiter active, demonstrating that the Testing thresholds (§5.4) do not interfere.
8. A non-administrative authenticated caller receives 403 from both image endpoints; an administrator succeeds on both; an anonymous caller receives 401.
9. Failed sign-in increments the Identity lockout counter for an account with lockout enabled, and the account is locked after the configured number of attempts.
10. An account holding the `Admin` role has `LockoutEnabled == false` after seeding, and repeated failed sign-in against it does not lock it.
11. **An administrative account that already exists and is already in the `Admin` role, carrying `LockoutEnabled == true`, has it set to `false` by the next seeding run.** This is the criterion that distinguishes a correct implementation from one that only converges for freshly created accounts, which is the shape every integration test produces (§6.3).
12. An exempt administrative account carrying `AccessFailedCount > 0` or a future `LockoutEnd` has both cleared by the next seeding run (§6.5).
13. A locked-out sign-in and a wrong-password sign-in produce indistinguishable responses.
14. nginx ignores `CF-Connecting-IP` presented by a peer outside the configured trusted ranges, and honors it from a peer inside them (§4.4). Verified against the rendered nginx configuration and by request, not by inspection of the source file alone.
15. The audit report and roadmap match the target state in §8, and no retained section of either describes a weakness that is still live.
16. The full suite passes, and the frontend builds and lints clean.

---

## 10. Left to the builder

- The exact nginx expression selecting between the vendor header and the observed peer address. A `map` block and an inline conditional are both acceptable; the contract is §4.1, not the syntax.
- The bridge network range expressed as a known network, and whether it is held in configuration or declared in `Program.cs`. It must tolerate container addresses being assigned dynamically.
- Whether the rate-limiting options bind through a typed options class or are read directly during policy registration. The requirement is that no threshold is a literal and that a missing policy entry fails startup.
- The mechanism by which a test class supplies its own configuration to the host. Extending the existing fixture with an overridable configuration hook is preferred over duplicating its database and mail setup in a second fixture.
- Test file placement, following the established `Unit/...` and `Integration/...` conventions.
- Whether the startup validation lives inline in `Program.cs` or in a small testable helper. Acceptance criterion 2 requires it to be reachable from a test, which effectively favors the helper.

---

## 11. Constraints

- **No secret value is introduced into any committed file by this track.** The signing key is removed from configuration and is never replaced by a literal, placeholder, or example value that would function.
- The integration fixture builds the real application host. Removing the signing key from `appsettings.json` while adding startup validation makes host construction fail for **every** integration test unless the fixture supplies a key. This is the single highest-risk interaction in the track, and it must be handled in the same change that introduces the validation.
- Local development inherits `JwtSettings` from `appsettings.json` today; `appsettings.Development.json` declares none. After this track a developer must set a user secret before the application will start. This must be documented in `README.md`.
- Integration tests run against real PostgreSQL via `BaseIntegrationTest`. Lockout state is persisted on the user row and must be asserted there, not against a mock.
- The suite is sequential by necessity and every test re-creates the database. This track must not introduce parallel-execution assumptions; roadmap 18.1.1 owns that change.
- Rate-limiter state is per-process and in-memory. Any test asserting rejection must own its own host instance, so that counters do not carry across tests.
- Comments in English, third person. No AI attribution in commits.
