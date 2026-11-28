# Track 18.0 — Security Hardening: Implementation Plan

> **Status: complete and verified (2026-11-20).** All nine tasks implemented on `feature/track-18.0-security-hardening`, released as `v2026.11.20.02`, and the three out-of-band acceptance checks carried out against the deployed system — results recorded under Verification Summary. Criterion 11 failed in production and was closed by `v2026.11.20.03`. Residual scope is recorded as roadmap items 19.10 and 19.11. Task 2 took effect in production only on 2026-11-28; see the correction under Verification Summary.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Source the JWT signing key from the environment with fail-fast validation, narrow ingress to the web tier, resolve the real client address behind the proxy chain, throttle seven sensitive endpoints across six per-endpoint policies, enable Identity lockout with an administrative exemption, and restrict media management to administrators.

**Architecture:** Nine tasks in three movements. Tasks 1-2 move the signing key and close the published ports — Task 1 is the riskiest change in the track, because removing the key from `appsettings.json` breaks host construction for every integration test unless the fixture is updated in the same commit. Task 3 establishes accurate client identification in both nginx and the application, which Tasks 4-5 depend on: throttling partitioned on an address the caller controls is worse than no throttling, and it is also the control the Task 6 administrative exemption leans on, so Tasks 3 and 6 must ship together or neither should. Task 7 is independent of everything before it. Tasks 8-9 narrow the public documentation surface and record the outcome.

**Tech Stack:** .NET 8, ASP.NET Core Identity, `Microsoft.AspNetCore.RateLimiting` (framework-provided, no package reference needed), EF Core + Npgsql, MediatR, xUnit + Moq + FluentAssertions, Docker Compose, nginx.

**Spec:** `docs/specs/track-18.0-security-hardening.md`

## Global Constraints

- No secret value enters any committed file. The signing key is removed and never replaced by a functioning placeholder.
- The rate limiter stays registered and active in every environment, including Testing. Only thresholds differ. Never gate the middleware itself on environment.
- Integration tests run against real PostgreSQL via `BaseIntegrationTest`. Lockout state lives on the user row and is asserted there, never against a mock.
- The suite is sequential by necessity; introduce no parallel-execution assumptions (roadmap 18.1.1 owns that).
- Rate-limiter counters are per-process. Any test asserting rejection owns its own host instance.
- Forwarded-headers middleware must be registered first in the pipeline, before request logging.
- Comments in English, third person. Conventional Commits. No AI attribution anywhere.
- Commit subjects and bodies describe the capability added, not any prior weakness.

## Review Focus

Five conditions the spec implies but does not pin. Each has a test assigned to the task owning the code.

1. **Rejection status is 429, not 503.** `RejectionStatusCode` defaults to `503 ServiceUnavailable`. Left at the default, throttling is indistinguishable from an outage to monitoring and to the Track 18.2 suite. (Task 4)
2. **A policy referenced but not configured must fail at startup, not per request.** .NET 8 does not silently serve such an endpoint unprotected — it throws `InvalidOperationException` naming the policy, but it does so on the first request to that endpoint, which means an unexercised endpoint can ship broken. Validating eagerly converts a latent per-request failure into a deployment that refuses to start. (Task 4)
3. **Distinct forwarded addresses must land in distinct partitions.** This is the only assertion that actually proves Task 3 works; everything else about forwarded headers is invisible from outside. (Task 5)
4. **Key length is measured in bytes, not characters.** A 31-character ASCII key must fail and a 32-character one must pass, but a 20-character key of multi-byte characters may exceed 32 bytes and must pass. (Task 1)
5. **A locked-out response and a wrong-password response must be byte-identical.** Status, body, and every field. A difference anywhere reintroduces account enumeration. (Task 6)
6. **The administrative lockout exemption must converge for an account that already exists in the role.** Every integration test seeds a freshly created administrator, so an implementation that only sets the flag on the creation or promotion branch passes the whole suite while leaving the deployed account lockable. The test for this must construct the production shape explicitly: an existing administrator, already in the role, with the flag set wrongly. (Task 6)

---

### Task 1: Source the signing key from the environment

The highest-risk task in the track. `WebApplicationFactory<Program>` builds the real host, so the moment the key leaves `appsettings.json` every integration test fails to construct unless the fixture supplies one. Both halves ship in one commit.

**Files:**
- Create: `MeuSitePessoal.Api/Configuration/JwtKeyResolver.cs`
- Modify: `MeuSitePessoal.Api/Program.cs:71-72`
- Modify: `MeuSitePessoal.Api/appsettings.json:17-22` — remove the `Key` member only
- Modify: `MeuSitePessoal.Tests/Integration/BaseIntegrationTest.cs:98-106` — add `JwtSettings:Key` to the in-memory configuration
- Modify: `README.md` — document the local development requirement
- Test: `MeuSitePessoal.Tests/Unit/Api/Configuration/JwtKeyResolverTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces:
  ```csharp
  namespace MeuSitePessoal.Api.Configuration;

  public static class JwtKeyResolver
  {
      public const int MinimumKeyBytes = 32;

      // Returns the UTF-8 encoded signing key.
      // Throws InvalidOperationException naming "JwtSettings:Key" when absent,
      // whitespace, or shorter than MinimumKeyBytes once encoded.
      public static byte[] Resolve(IConfiguration configuration);
  }
  ```

- [x] **Step 1: Write the failing tests**

```csharp
[Theory] [InlineData(null)] [InlineData("")] [InlineData("   ")]
Resolve_WithMissingKey_ThrowsNamingTheConfigurationPath(string? key)
    // asserts the InvalidOperationException message contains "JwtSettings:Key"

[Fact] Resolve_WithKeyShorterThanMinimum_Throws()
    // 31 ASCII characters => throws

[Fact] Resolve_WithKeyAtExactlyMinimum_Succeeds()
    // 32 ASCII characters => returns a 32-byte array
    // Review Focus 4 — pins the boundary as inclusive

[Fact] Resolve_MeasuresLengthInBytesNotCharacters()
    // 20 multi-byte characters encoding to more than 32 bytes => succeeds
    // Review Focus 4

[Fact] Resolve_ExceptionMessage_DoesNotEchoTheConfiguredValue()
    // a short but distinctive key => the message must not contain it
```

Build the `IConfiguration` with `ConfigurationBuilder().AddInMemoryCollection(...)`.

- [x] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~JwtKeyResolverTests"`
Expected: FAIL — `JwtKeyResolver` does not exist.

- [x] **Step 3: Implement the resolver and wire it into startup**

Implement `Resolve`. In `Program.cs`, replace `var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);` with `var key = JwtKeyResolver.Resolve(builder.Configuration);`. The `TokenValidationParameters` construction at `Program.cs:89` is unchanged — it already consumes `key`.

Leave `TokenService.cs:33-34` alone for now; it reads the key independently at token-generation time and still resolves correctly from configuration. Changing it is not required by any acceptance criterion and would widen this commit.

- [x] **Step 4: Remove the key from configuration and supply it to the fixture**

Delete the `"Key"` line from `appsettings.json`. Keep `Issuer`, `Audience`, and `ExpiresInMinutes`.

In `BaseIntegrationTest.cs`, publish the key to the **process environment** from a static constructor, so it is set before any host is constructed:

```csharp
private const string TestSigningKey = "integration-test-signing-key-not-a-production-secret";

static BaseIntegrationTest()
{
    Environment.SetEnvironmentVariable("JwtSettings__Key", TestSigningKey);
}
```

**The fixture's `AddInMemoryCollection` is not sufficient on its own, and was the original instruction here — it does not work.** `JwtKeyResolver.Resolve(builder.Configuration)` runs in top-level statements before `builder.Build()`, while `WebApplicationFactory` merges its `ConfigureAppConfiguration` sources at `Build()`. A build-time read therefore never observes them, and every integration test fails at `CreateClient()` with the resolver's own "not configured" message. The existing `AdminSetup` and `ClientSettings` entries are unaffected because they are read at request time, after the merge.

Also add a matching `["JwtSettings:Key"] = TestSigningKey` entry to the in-memory dictionary, so any consumer resolving the key after the host is built sees the same value.

This is test-only configuration for an in-memory host that issues tokens consumed only by the same process. It is not a production credential and does not require rotation.

- [x] **Step 5: Run the full suite**

Run: `dotnet test`
Expected: PASS, all tests. A sweeping failure here means the fixture configuration key is misspelled — the host cannot build and every integration test fails identically.

**Caveat on this diagnosis:** `Program.cs:27` loads user secrets whenever the `ASPNETCORE_ENVIRONMENT` *process* variable is empty, which it is under `dotnet test` — the fixture sets the environment through `UseEnvironment`, not the variable. So once Step 6 sets a local user secret, a misspelled fixture key will still build the host on this machine and fail only in CI. Run this step **before** Step 6, or temporarily clear the secret, to get a trustworthy signal.

- [x] **Step 6: Document the local development requirement**

Add to `README.md`, under setup:

```bash
dotnet user-secrets set "JwtSettings:Key" "<a value of at least 32 bytes>" --project MeuSitePessoal.Api
```

State that the application will not start without it, and that the value must come from the environment rather than from a committed file.

- [x] **Step 7: Verify no key value remains in the working tree**

Run: `git grep -n "JwtSettings" -- ':!docs'`
Expected: references to the section and its non-secret members only; no key value in `appsettings.json`.

- [x] **Step 8: Commit**

```bash
git add MeuSitePessoal.Api/Configuration/JwtKeyResolver.cs MeuSitePessoal.Api/Program.cs MeuSitePessoal.Api/appsettings.json MeuSitePessoal.Tests/Unit/Api/Configuration/JwtKeyResolverTests.cs MeuSitePessoal.Tests/Integration/BaseIntegrationTest.cs README.md
git commit -m "feat(config): source JWT signing key from environment with startup validation"
```

---

### Task 2: Narrow the deployment ingress

Infrastructure only, no application code and no test surface. Verified by rendering the merged Compose configuration.

**Files:**
- Modify: `docker-compose.yml` — `database.ports` (remove), `backend.ports` (remove), `backend.environment` (add the key)

- [x] **Step 1: Apply the Compose changes**

Remove the `ports` block from `database` and from `backend` entirely. Leave `frontend.ports` as `80:80`.

Add to `backend.environment`, alongside the existing substitutions:

```yaml
      - JwtSettings__Key=${JWT_SIGNING_KEY}
```

- [x] **Step 2: Verify the rendering**

Run: `docker compose config`
Expected: `database` and `backend` show no `ports`; `backend` shows `JwtSettings__Key`. Locally the value resolves empty with a warning because no `.env` is present in this worktree — that is correct and expected here; the value exists only on the server.

- [x] **Step 3: Confirm nothing in the repository depended on the published ports**

Run: `git grep -nE "localhost:(5000|5432)" -- ':!docs' ':!*.lock'`
Expected: no results. The frontend resolves its API base to `/api` outside development mode and targets `25683` in development.

- [x] **Step 4: Commit**

```bash
git add docker-compose.yml
git commit -m "chore(docker): route all ingress through the web tier and pass the signing key from the environment"
```

---

### Task 3: Resolve the client address behind the proxy chain

Establishes the identity that Task 5 partitions on. Without it every caller resolves to the nginx container and throttling degrades into a single shared budget.

**Files:**
- Modify: `MeuSitePessoal.Api/Program.cs` — forwarded-headers options near the other service registrations, and `UseForwardedHeaders` as the first middleware (before `UseSerilogRequestLogging` at line 195)
- Modify: `meupessoal-web/nginx.conf:23-24` and the `/uploads` block
- Test: `MeuSitePessoal.Tests/Integration/Api/ForwardedHeadersTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: `HttpContext.Connection.RemoteIpAddress` reflects the forwarded chain for every downstream consumer.

- [x] **Step 1: Write the failing test**

The resolved address is not observable from any existing response, and no production endpoint should be added to expose it. Capture it with a test-only terminal middleware registered through the fixture:

Register the capturing middleware through an `IStartupFilter` added in `builder.ConfigureServices`. Inside the filter, call `next(app)` **first** and add the capturing middleware after it, so it runs inside the pipeline `Program.cs` builds rather than replacing it. Probe a path that matches no controller route.

**Do not use `builder.Configure(...)`.** On a `WebApplicationFactory<Program>` over minimal hosting it replaces the pipeline defined in `Program.cs` entirely, so the test would assert on middleware the test itself registered and would pass whether or not the application is wired correctly.

```csharp
[Fact] RemoteIpAddress_ReflectsForwardedHeader_WhenSingleHopPresented()
    // request with X-Forwarded-For: 203.0.113.9 => captured address is 203.0.113.9

[Fact] RemoteIpAddress_IgnoresSecondHop_WhenChainExceedsTrustedLimit()
    // X-Forwarded-For: 198.51.100.4, 203.0.113.9 => the address attributed is the
    // nearest trusted hop, not the leftmost entry
    // pins ForwardLimit = 1 so that a caller cannot prepend an address of their choosing
```

- [x] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~ForwardedHeadersTests"`
Expected: FAIL — forwarded headers are not processed, so the captured address is the test host's peer.

- [x] **Step 3: Configure forwarded headers**

Register the options:

```csharp
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    // Fully qualified: .NET 8 introduced System.Net.IPNetwork, so the bare name is
    // ambiguous (CS0104) once both System.Net and the HttpOverrides namespace are imported.
    options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(
        System.Net.IPAddress.Parse(bridgeNetworkAddress), bridgePrefixLength));
});
```

The default known networks cover loopback only, which never matches a container peer on the bridge. Bind the network address and prefix length from configuration rather than hard-coding them, so the deployed value can be corrected without a code change, and log the resolved value once at startup.

**On `KnownIPNetworks`:** the newer `options.KnownIPNetworks` collection, which takes `System.Net.IPNetwork` and supersedes `KnownNetworks`, was introduced in **ASP.NET Core 9.0**. This solution targets `net8.0` against the 8.0.25 runtime, where the member does not exist — a probe build fails with `CS1061`. `KnownNetworks` is the correct and only API here, and it is not deprecated on this target, so it produces no warning. Switch to `KnownIPNetworks` when the solution moves to .NET 9 or later; until then this is not a deprecation to clean up.

**This step cannot be validated by the test suite, and the tests will not tell you if it is wrong.** Under `TestServer` the connection has no remote address, and `ForwardedHeadersMiddleware` deliberately skips the known-address check for the first hop in that case — so both tests in Step 1 pass for *any* declared range, including a wrong one. If the declared range does not contain the nginx container's address in production, forwarded headers are silently ignored there, every request partitions on the nginx container address, and the limiter collapses into one shared global budget. That is spec §1's first anti-goal, shipped green.

Confirm the real value on the server before the release branch is cut:

```bash
docker network inspect blog-network --format '{{range .IPAM.Config}}{{.Subnet}}{{end}}'
```

Record the observed subnet in the commit body.

Add `app.UseForwardedHeaders();` as the **first** middleware, above `app.UseSerilogRequestLogging()`, so request logs carry the resolved address.

- [x] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~ForwardedHeadersTests"`
Expected: PASS.

- [x] **Step 5: Normalize the forwarded address in nginx**

Use the real-IP module, which is compiled into the stock nginx image, so that `CF-Connecting-IP` is honored **only** when the request actually arrives from Cloudflare (spec §4.4):

```nginx
# One set_real_ip_from per published Cloudflare range.
set_real_ip_from 173.245.48.0/20;
# ... remaining IPv4 and IPv6 ranges ...
real_ip_header CF-Connecting-IP;
```

With this in place `$remote_addr` is already the true client address for traffic arriving through Cloudflare, and remains the observed peer for anything else. In both the `/api` and `/uploads` blocks, replace the accumulating `$proxy_add_x_forwarded_for` with `proxy_set_header X-Forwarded-For $remote_addr;` so the chain the application receives has exactly one entry. Leave `X-Real-IP` and `X-Forwarded-Proto` as they are.

A plain `map` on `$http_cf_connecting_ip` is **not** an acceptable substitute: it trusts the header from any peer, which is the inversion spec §4.4 exists to prevent.

`meupessoal-web/Dockerfile` copies this file to `/etc/nginx/conf.d/default.conf`, which the stock image includes inside `http {}`, so both `set_real_ip_from` and a `map` are valid at the top of this file.

Fetch the current ranges from Cloudflare's published list at implementation time rather than copying them from this document, and record the retrieval date in a comment above the block.

- [x] **Step 5a: Verify the conditional trust by request**

From outside, send a request to the origin carrying a forged `CF-Connecting-IP` and confirm it is not honored, then confirm a request through Cloudflare is attributed correctly. Spec acceptance criterion 14 is not satisfied by reading the configuration file.

- [x] **Step 6: Commit**

```bash
git add MeuSitePessoal.Api/Program.cs meupessoal-web/nginx.conf MeuSitePessoal.Tests/Integration/Api/ForwardedHeadersTests.cs
git commit -m "feat(api): resolve client address from forwarded headers behind the proxy chain"
```

---

### Task 4: Rate-limiting infrastructure

Registers the policies and the rejection contract. No endpoint is annotated yet, so the suite must stay green throughout.

**Files:**
- Create: `MeuSitePessoal.Api/Configuration/RateLimitingOptions.cs`
- Create: `MeuSitePessoal.Api/Configuration/RateLimiterSetup.cs` (extension method registering the policies)
- Modify: `MeuSitePessoal.Api/Program.cs` — registration, and `UseRateLimiter` after routing
- Modify: `MeuSitePessoal.Api/appsettings.json` — the `RateLimiting` section
- Modify: `MeuSitePessoal.Tests/Integration/BaseIntegrationTest.cs` — generous Testing thresholds and an overridable configuration hook
- Test: `MeuSitePessoal.Tests/Unit/Api/Configuration/RateLimitingOptionsTests.cs`

**Interfaces:**
- Consumes: the resolved client address from Task 3.
- Produces:
  ```csharp
  namespace MeuSitePessoal.Api.Configuration;

  public sealed class RateLimitPolicyOptions
  {
      public int PermitLimit { get; init; }
      public int WindowMinutes { get; init; }
  }

  public sealed class RateLimitingOptions
  {
      public const string SectionName = "RateLimiting";
      public Dictionary<string, RateLimitPolicyOptions> Policies { get; init; } = new();
  }

  public static class RateLimitPolicies
  {
      public const string AuthSignIn = "auth-signin";
      public const string AuthVerify = "auth-verify";
      public const string AuthRegister = "auth-register";
      public const string AuthRecovery = "auth-recovery";
      public const string CommentsWrite = "comments-write";
      public const string NewsletterSubscribe = "newsletter-subscribe";
  }
  ```
  And on `BaseIntegrationTest`:
  ```csharp
  protected virtual IDictionary<string, string?> AdditionalConfiguration
      => new Dictionary<string, string?>();
  ```
  merged into the fixture's in-memory collection so a derived test class can override thresholds without duplicating the database and mail setup.

- [x] **Step 1: Write the failing tests**

```csharp
[Fact] Registration_WithMissingPolicyEntry_ThrowsNamingThePolicy()
    // configuration omits "comments-write" => building the limiter throws,
    // and the message names the missing policy
    // Review Focus 2

[Fact] Registration_WithAllPoliciesPresent_Succeeds()

[Fact] Options_BindFromConfigurationSection()
    // asserts PermitLimit and WindowMinutes round-trip for one policy
```

- [x] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~RateLimitingOptionsTests"`
Expected: FAIL — the types do not exist.

- [x] **Step 3: Implement options and policy registration**

Register one fixed-window limiter per policy name, each partitioned on `httpContext.Connection.RemoteIpAddress?.ToString()` falling back to a single constant key when the address is null (spec §4.3). `QueueLimit` is 0 — reject rather than delay.

Set the rejection contract explicitly:

```csharp
options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
```

This is Review Focus 1. The framework default is 503.

In `OnRejected`, set `Retry-After` from the limiter's `MetadataName.RetryAfter` metadata when available, write a short fixed body that discloses no threshold or counter state, and log at Warning with the endpoint and the partition key.

Validate that every name in `RateLimitPolicies` has a configured entry, throwing and naming the missing policy if not. Read the configuration section and perform this check **eagerly**, in the extension method body before `AddRateLimiter` is called — not inside the `AddRateLimiter` delegate, which is deferred until the options are materialized at pipeline build.

Without the eager check, .NET 8 does not serve such an endpoint unprotected — it throws `InvalidOperationException` naming the policy — but it throws on the first request to that endpoint, so an endpoint no test exercises ships broken and fails in production. Eager validation converts that into a deployment that refuses to start.

- [x] **Step 4: Add the configuration and the pipeline call**

Add the `RateLimiting` section to `appsettings.json` exactly as given in spec §5.3.

Add `app.UseRateLimiter();` after routing and before `app.UseAuthentication()`. Endpoint-level policy metadata requires routing to have run; placing it before authentication means unauthenticated floods are rejected before any identity work.

- [x] **Step 5: Add Testing thresholds and the fixture hook**

In `BaseIntegrationTest`, add generous thresholds to the in-memory configuration — a permit limit far above anything the suite can reach within a window long enough to span a full run:

```csharp
["RateLimiting:Policies:auth-signin:PermitLimit"] = "100000",
["RateLimiting:Policies:auth-signin:WindowMinutes"] = "60",
// ... and the same for the other four policies
```

Then merge `AdditionalConfiguration` over that dictionary so a derived class can override individual values.

- [x] **Step 6: Run the full suite**

Run: `dotnet test`
Expected: PASS, all tests. The limiter is now active in every test but no endpoint is annotated yet, so behavior is unchanged. A failure here means registration throws — check that every policy name has a configured entry.

- [x] **Step 7: Commit**

```bash
git add MeuSitePessoal.Api/Configuration/RateLimitingOptions.cs MeuSitePessoal.Api/Configuration/RateLimiterSetup.cs MeuSitePessoal.Api/Program.cs MeuSitePessoal.Api/appsettings.json MeuSitePessoal.Tests/Unit/Api/Configuration/RateLimitingOptionsTests.cs MeuSitePessoal.Tests/Integration/BaseIntegrationTest.cs
git commit -m "feat(api): add configurable per-endpoint request rate limiting"
```

---

### Task 5: Apply the policies to the endpoints

Attaches each policy and proves rejection, selectivity, and partitioning.

**Files:**
- Modify: `MeuSitePessoal.Api/Controllers/AuthController.cs:78` (login), `:99` (register), `:133` (forgot-password), `:145` (reset-password), `:163` (verify-2fa)
- Modify: `MeuSitePessoal.Api/Controllers/CommentsController.cs:56` (create)
- Modify: `MeuSitePessoal.Api/Controllers/NewsletterController.cs:33` (subscribe)
- Test: `MeuSitePessoal.Tests/Integration/Api/RateLimitingTests.cs`

**Interfaces:**
- Consumes: the policy names and the fixture hook from Task 4.
- Produces: no signature changes. `[EnableRateLimiting(RateLimitPolicies.X)]` metadata on seven actions.

**Scope decision, settled:** `verify-2fa` and `reset-password` are in scope, under their own `auth-verify` policy rather than sharing `auth-signin` — a two-factor sign-in calls login and then verification, so a shared budget would charge two permits per successful attempt (spec §5.1).

- [x] **Step 1: Write the failing tests**

A test class deriving from `BaseIntegrationTest` and overriding `AdditionalConfiguration` with a permit limit of 2 and a window of 60 minutes for every policy. Each test method gets a fresh host, so counters do not carry across tests.

```csharp
[Theory]
[InlineData("/api/auth/login")]
[InlineData("/api/auth/register")]
[InlineData("/api/auth/forgot-password")]
[InlineData("/api/auth/reset-password")]
[InlineData("/api/auth/verify-2fa")]
[InlineData("/api/newsletter/subscribe")]
Endpoint_ReturnsTooManyRequests_OnceBudgetIsExhausted(string path)
    // three POSTs; the third asserts 429 specifically, never merely "not 2xx",
    // so a 503 default would fail this test
    // Review Focus 1

[Fact] Comments_ReturnTooManyRequests_OnceBudgetIsExhausted()
    // authenticated; separate because it requires a token and an article

[Fact] TwoFactorVerification_DoesNotConsumeTheSignInBudget()
    // exhaust auth-verify against /verify-2fa, then assert /login still answers
    // normally — pins that the two policies hold separate budgets, which is the
    // whole reason auth-verify exists rather than reusing auth-signin

[Fact] RequestWithinBudget_ReturnsNormalStatus()
    // the first request returns its ordinary status, proving the limiter
    // rejects selectively rather than universally

[Fact] DistinctForwardedAddresses_ConsumeSeparateBudgets()
    // exhaust the budget presenting X-Forwarded-For: 203.0.113.9,
    // then one request as 198.51.100.4 succeeds
    // Review Focus 3 — the only assertion that proves Task 3 is load-bearing

[Fact] RejectedResponse_CarriesRetryAfter()
```

- [x] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~RateLimitingTests"`
Expected: FAIL — no endpoint is annotated, so every request succeeds.

- [x] **Step 3: Annotate the endpoints**

Add `[EnableRateLimiting(RateLimitPolicies.X)]` to each of the seven actions, using the constants rather than string literals so a typo is a compile error rather than a silently unprotected endpoint.

- [x] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~RateLimitingTests"`
Expected: PASS.

- [x] **Step 5: Run the full suite**

Run: `dotnet test`
Expected: PASS. This is the step that proves the Testing thresholds from Task 4 are generous enough — `AuthenticateAsync` calls the now-limited login endpoint from many test classes. A 429 surfacing in an unrelated test means the Testing thresholds are too low or the window too short.

- [x] **Step 6: Commit**

```bash
git add MeuSitePessoal.Api/Controllers/ MeuSitePessoal.Tests/Integration/Api/RateLimitingTests.cs
git commit -m "feat(api): apply rate limit policies to authentication, comment and subscription endpoints"
```

---

### Task 6: Account lockout with administrative exemption

Independent of Tasks 1-5.

**Files:**
- Modify: `MeuSitePessoal.Api/Program.cs:46-55` — explicit lockout options
- Modify: `MeuSitePessoal.Application/Auth/Commands/Login/LoginCommandHandler.cs:60`
- Modify: `MeuSitePessoal.Infrastructure/Configuration/DbInitializer.cs:50-82` — set the flag on both administrative paths
- Modify: `MeuSitePessoal.Tests/Unit/Application/Auth/Commands/LoginCommandHandlerTests.cs:72,114,133`
- Test: `MeuSitePessoal.Tests/Integration/Auth/AccountLockoutTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: no signature changes.

**Note on the existing unit tests:** three substitute setups specify `CheckPasswordSignInAsync(..., false)`. Once the handler passes `true` those setups stop matching and return a default result, so the tests fail. Updating the argument to `true` reflects a deliberate contract change, not a weakened assertion. The file uses **NSubstitute** (`Arg.Any<...>()` / `.Returns(...)`), not Moq — match the surrounding style.

- [x] **Step 1: Write the failing tests**

```csharp
// Integration, against real PostgreSQL — lockout state is a persisted column.

[Fact] FailedSignIn_IncrementsLockoutCounter_ForLockoutEnabledAccount()
    // register and confirm a Reader; one wrong password;
    // assert AccessFailedCount == 1 on the user row

[Fact] Account_LocksOut_AfterConfiguredFailedAttempts()
    // five wrong passwords => IsLockedOut is true and LockoutEnd is in the future

[Fact] SeededAdministrator_HasLockoutDisabled()
    // asserts LockoutEnabled == false on the admin user row after seeding

[Fact] Administrator_DoesNotLockOut_AfterRepeatedFailedAttempts()
    // ten wrong passwords against the admin account, then a correct one succeeds

[Fact] ExistingAdministrator_AlreadyInRole_HasLockoutDisabledOnNextSeed()
    // THE test that distinguishes a correct implementation from a plausible one.
    // Arrange the production shape the suite never otherwise produces: take the
    // seeded admin, set LockoutEnabled = true, persist, then run SeedAsync again.
    // Assert LockoutEnabled == false.
    // An implementation that sets the flag only on the creation or promotion branch
    // passes every other test in this file and fails this one.
    // Review Focus 6

[Fact] ExemptAdministrator_HasLockoutCountersCleared()
    // set AccessFailedCount = 3 and LockoutEnd in the future on the admin, persist,
    // run SeedAsync again, assert both are cleared (spec §6.5)

[Fact] LockedOutResponse_IsIndistinguishableFromWrongPassword()
    // capture status and full body for a wrong-password attempt on an unlocked
    // account and for an attempt on a locked account; assert both are equal
    // Review Focus 5
```

- [x] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~AccountLockoutTests"`
Expected: FAIL — the counter never increments and the administrator has lockout enabled.

- [x] **Step 3: Declare the lockout policy**

In the `AddIdentity` options block, add `MaxFailedAccessAttempts = 5`, `DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15)`, `AllowedForNewUsers = true`. These match the framework defaults for the first and third, and are stated explicitly so the policy is visible rather than inherited.

- [x] **Step 4: Enable lockout on sign-in**

Change `lockoutOnFailure: false` to `lockoutOnFailure: true` at `LoginCommandHandler.cs:60`. Add a distinct Warning log for `result.IsLockedOut`, naming the account. Do **not** branch the returned `LoginResult` — the response stays identical to the wrong-password path (spec §6.4).

- [x] **Step 5: Exempt administrators in the seeder**

In `DbInitializer.SeedAsync`, converge the administrative account's lockout state on every startup. Place this **outside** the `if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))` block at `DbInitializer.cs:77`, after the role is known to be assigned — not inside the creation branch and not inside the promotion branch.

The reason is spec §6.3: the deployed administrator is already in the role, so the promotion branch never runs again, while every integration test creates the administrator fresh and therefore always takes the creation branch. An implementation attached to either branch passes the suite and leaves production lockable.

Converge three fields, writing only when something differs so a normal boot performs no write:

- `LockoutEnabled` to `false`
- `LockoutEnd` to `null` when set
- `AccessFailedCount` to `0` when non-zero

The last two are spec §6.5 — `AccessFailedAsync` writes them without consulting `LockoutEnabled`, so an exempt account accumulates lockout state that only looks alarming today but would lock the account instantly if the exemption were ever lifted.

- [x] **Step 6: Update the three existing mock setups**

Change the `false` argument to `true` at `LoginCommandHandlerTests.cs:72,114,133`.

- [x] **Step 7: Run the full suite**

Run: `dotnet test`
Expected: PASS.

- [x] **Step 8: Commit**

```bash
git add MeuSitePessoal.Api/Program.cs MeuSitePessoal.Application/Auth/Commands/Login/LoginCommandHandler.cs MeuSitePessoal.Infrastructure/Configuration/DbInitializer.cs MeuSitePessoal.Tests/
git commit -m "feat(auth): enable account lockout with an administrative exemption"
```

---

### Task 7: Restrict media management to administrators

Independent of every preceding task.

**Files:**
- Modify: `MeuSitePessoal.Api/Controllers/ImagesController.cs:13`
- Test: `MeuSitePessoal.Tests/Integration/Api/ImagesAuthorizationTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: no signature changes.

Follow the existing precedent in `ArticleAuthorizationTests.cs`, which already asserts 403 for non-administrative callers on article mutations.

- [x] **Step 1: Write the failing tests**

```csharp
[Fact] Upload_AsReader_ReturnsForbidden()
[Fact] Delete_AsReader_ReturnsForbidden()
[Fact] Upload_AsAnonymous_ReturnsUnauthorized()
[Fact] Delete_AsAnonymous_ReturnsUnauthorized()
[Fact] Upload_AsAdministrator_Succeeds()
[Fact] Delete_AsAdministrator_Succeeds()
```

Post a small in-memory `MultipartFormDataContent` with an allowed extension for the upload cases.

- [x] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~ImagesAuthorizationTests"`
Expected: FAIL on the two Reader cases — both currently succeed. The anonymous and administrator cases already pass.

- [x] **Step 3: Apply the role restriction**

Change `[Authorize]` to `[Authorize(Roles = "Admin")]` on the controller.

Make no change to `LocalStorageService.DeleteFileAsync` (spec §7). Its `Path.GetFileName` call already collapses directory components.

- [x] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~ImagesAuthorizationTests"`
Expected: PASS.

- [x] **Step 5: Commit**

```bash
git add MeuSitePessoal.Api/Controllers/ImagesController.cs MeuSitePessoal.Tests/Integration/Api/ImagesAuthorizationTests.cs
git commit -m "feat(images): restrict media management to administrators"
```

---

### Task 8: Narrow the public documentation surface

Implements spec §8. This repository is public, and two committed documents describe weaknesses in the deployed system in operational detail. Ships in the same squashed commit as the fixes, so the removal is never visible as a standalone change.

**Files:**
- Modify: `docs/TECHNICAL_AUDIT_REPORT.md` — §3.3 only
- Modify: `docs/roadmap.md` — Track 18.0 entries and the Track 14 known-gap note

- [x] **Step 1: Replace audit report §3.3**

Replace the Security & Resilience findings table with a short note: the audit date, the number of findings in that category, and a statement that security findings are tracked privately rather than enumerated here. Keep the section heading and its number so §1's pillar table, §4, §5, and the roadmap's cross-references still resolve.

Retain every other section unchanged — §2 strengths, §3.1 performance, §3.2 architecture, §3.4 test health, §4, §5, §6. Those describe design and performance debt, not exploitable weakness, and Tracks 18.1, 18.2 and 19 are all scoped against them.

- [x] **Step 2: Rewrite the Track 18.0 roadmap entries**

Lines 146-149 currently carry parenthetical descriptions of live weakness — for example that any authenticated reader can delete any uploaded image. Rewrite each as completed capability in the register the rest of the roadmap uses, retaining the `S#` label as an opaque reference:

```markdown
- [x] **18.0.1**: JWT signing key sourced from the environment, with fail-fast startup
  validation and the production value supplied through `docker-compose.yml` (audit S1).
- [x] **18.0.2**: Database credential rotated (audit S2). Completed operationally against
  production on 2026-11-20, not by a code change in this track.
- [x] **18.0.3**: Proxy-aware client address resolution, per-endpoint request rate limiting,
  and Identity lockout with an administrative exemption (audit S3).
- [x] **18.0.4**: Media upload and deletion restricted to administrators (audit S5).
```

Rewrite the Track 14 known-gap note at line 99 as closed.

- [x] **Step 3: Verify nothing retained still describes a live weakness**

Re-read the retained sections of the audit report against the current state of the code. Any sentence written in the present tense about a weakness this track did not close is still an accurate description of the deployed system, and belongs in the private record instead.

Run: `git diff --stat docs/`
Expected: only the two intended files, with no change to the performance, architecture, or test-health sections.

- [x] **Step 4: Commit**

```bash
git add docs/TECHNICAL_AUDIT_REPORT.md docs/roadmap.md
git commit -m "docs: track security findings privately rather than in the repository"
```

**Recorded honestly, and not to be overstated in the PR:** this reduces casual discovery and nothing more. The content remains in git history and in any existing clone or fork. It is not a remedy for the items still open, and describing it as one would be worse than leaving it alone.

---

### Task 9: Record outcomes in the roadmap

**Files:**
- Modify: `docs/roadmap.md` — Phase 5 status and the Track 19 additions
- Modify: `docs/plans/track-18.0-implementation-plan.md` — this file

- [x] **Step 1: Add the Track 18.2 threshold note**

Task 8 already rewrote the Track 18.0 entries. Add one line under Track 18.2 recording a constraint it will otherwise discover the hard way:

```markdown
> **Threshold note for 18.2**: a browser suite authenticates from a single client
> address and therefore shares one rate-limit partition. Production thresholds
> (`auth-signin`: 10 per 15 minutes) are exhausted within ten sign-ins. The
> environment the suite runs against must declare its own `RateLimiting`
> configuration; thresholds bind from configuration for exactly this reason.
```

- [x] **Step 2: Add the deferred follow-ups**

Add to Track 19, in the neutral register the rest of the roadmap uses:

```markdown
- [ ] **19.10**: Restrict origin ingress on 80/443 to the CDN's published address ranges,
  so the forwarded-address chain cannot be presented by a caller reaching the origin
  directly. Recorded during Track 18.0 as residual scope (spec §4.4).
- [ ] **19.11**: Add edge throttling (CDN rules or nginx `limit_req`) as a complement to
  the in-application limiter, rejecting excess traffic before it reaches the origin.
  Deferred from Track 18.0 so one enforcement point owned the behavior first.
```

- [x] **Step 3: Update the Phase 5 status line**

Track 18.0 was the sole remaining blocker for Track 18.2. Update the milestone heading and the global progress note so 18.1 and 18.2 are the open work.

- [x] **Step 4: Commit**

```bash
git add docs/roadmap.md docs/plans/track-18.0-implementation-plan.md
git commit -m "docs: close Track 18.0 and record deferred ingress and edge-throttling follow-ups"
```

---

## Squash and Release

All nine task commits land on `feature/track-18.0-security-hardening` and are **squash-merged** into `dev` as a single commit, so no intermediate commit describing a prior weakness survives in the public history.

Squashed subject, in the neutral register the rest of this plan uses:

```
feat(security): harden configuration, proxy awareness and access control
```

The body lists capabilities added — environment-sourced signing key with startup validation, ingress narrowed to the web tier, proxy-aware client address resolution, per-endpoint request rate limiting, account lockout with an administrative exemption, administrator-only media management. It does not describe what any of them replaced.

The PR description carries the attribution blockquote required by `CLAUDE.md`, and the same neutral framing.

Cut `release/vYYYY.MM.DD.NN` promptly after the merge. The window between a merged fix and a deployed one is the only interval in which the squashed diff is public and production is still unpatched — and in this case the diff also includes Task 8's redaction, which is more informative than any single code change in it.

---

## Verification Summary

Run before declaring the track complete:

```bash
dotnet build
dotnet test
docker compose config
cd meupessoal-web && npm run lint && npm run build
```

Expected: build succeeds; full suite passes; the rendered Compose configuration shows no published ports for `database` or `backend` and carries `JwtSettings__Key`; frontend lints and builds clean.

Acceptance criteria 1-16 in spec §9 map to tasks as follows: 1-2 to Task 1, 3 to Task 2, 4 to Tasks 3 and 5, 5-7 to Tasks 4-5, 8 to Task 7, 9-13 to Task 6, 14 to Task 3, 15 to Task 8, 16 to the commands above.

**Three criteria cannot be confirmed by a green suite**, and each has a named out-of-band check:

| Criterion | Why the suite cannot confirm it | Check |
|---|---|---|
| 4 (forwarded address) | `TestServer` connections have no remote address, so the middleware skips the known-network check and the tests pass for any declared range | `docker network inspect blog-network` on the server; compare with the configured value |
| 11 (exemption converges) | Every test seeds a freshly created administrator, the one shape that makes a wrong implementation look right | the dedicated test in Task 6 Step 1 constructs the production shape explicitly; also verify the deployed row after release |
| 14 (conditional header trust) | nginx is not in the test host's path at all | Task 3 Step 5a, by request against the deployed origin |


### Out-of-band verification results (2026-11-20, against the deployed system)

All three were carried out after `v2026.11.20.02` deployed. One failed and produced a patch release.

| Criterion | Result | Evidence |
|---|---|---|
| 4 (forwarded address) | **Pass** | `blog-network` subnet is `172.18.0.0/16` and the nginx container peer is `172.18.0.4`, inside the configured `172.16.0.0/12` (which spans 172.16.0.0–172.31.255.255). No `ForwardedHeaders` override is set in the deployed environment, so the code default is what applies; the startup line confirms it. |
| 11 (exemption converges) | **Failed, then fixed** | Both `Admin` rows carried `LockoutEnabled = true`. The convergence step sat inside the block guarded by the `AdminSetup` credentials, which a deployed environment without those credentials skips in full, and it was keyed on the single configured account. Rows were corrected directly, then `v2026.11.20.03` moved the convergence outside that guard and keyed it on the role. |
| 14 (conditional header trust) | **Pass** | `nginx.conf` sets `X-Forwarded-For $remote_addr` rather than appending, so a supplied chain is replaced. Confirmed by request: once the sign-in budget was exhausted, requests presenting other forged `X-Forwarded-For` values — including a multi-hop chain and no header at all — stayed at 429 rather than receiving a fresh budget. The application logged a single partition equal to the real client address, and none of the forged values ever appeared as a partition key. A forged `CF-Connecting-IP` is rejected at the Cloudflare edge with 403 and never reaches the origin. |

**Note on 11.** The defect was invisible to the suite because every integration test configures `AdminSetup` through the shared fixture, so the guarded block always ran under test. Two tests now construct the missing shapes: a seed run against a configuration declaring no `AdminSetup` section, and a second administrator that configuration never names.

**Note on roadmap 19.10.** The verification clarified its status as defence in depth rather than an open gap. A caller reaching the origin directly does not match `set_real_ip_from`, so `$remote_addr` stays that caller's own address and the forwarded header carries it; no spoofing is available on either path. 19.10 removes the direct-reach path, it does not close a trust gap.

### Correction (2026-11-28): Task 2 reached production eight days late

Task 2 (narrow the deployment ingress) was complete in the repository on 2026-11-20 but did not take effect on the server until 2026-11-28. Until then ports 5000 and 5432 stayed published on the host, and port 5000 answered from the internet without passing through Cloudflare.

- **Cause.** The server checkout had not moved since April and carried a hand-edited `docker-compose.yml`, so `git pull` failed on every deploy. The deploy script had no `set -e`, so the step reported the exit code of `docker compose up -d` and showed success. Images updated through `docker compose pull`, which is why the rest of this track shipped while compose changes did not. The Deployment Notes below expected a failed `git pull` to stop the deploy; it did not.
- **Why the checks above missed it.** None of the three out-of-band checks covered published ports, and `docker compose config` ran against the repository file, not the server's checkout.
- **Fix.** On 2026-11-28 the server checkout was reset to `dev`, `docker-compose.override.yml` was removed and the containers were recreated. Only port 80 is published now; ports 5000 and 5432 are closed from outside, and `/`, `/health` and `/api/articles` return 200. The deploy workflow now runs with `set -e` and moves the checkout with `git fetch` and `git reset --hard` instead of `git pull`.
## Deployment Notes

Not part of the implementation, but required for the release to land cleanly:

- `JWT_SIGNING_KEY` must already exist in the server's `.env` before this release deploys. It was placed there during the pre-release configuration work on 2026-11-20, but **confirm it with `grep JWT_SIGNING_KEY ~/blog-portfolio/.env` before cutting the release branch** — the application will now refuse to start without it, and `restart: always` turns that into a crash loop.
- **Inspect `docker-compose.override.yml` on the server before deploying.** Compose auto-merges it, and `ports` entries in an override are appended rather than replaced — so if the override happens to publish a port, Task 2's narrowing is undone in production while `docker compose config` in this repository shows it correctly removed. Verify against the merged output on the server, not the repository file.
- Remove the override **after** confirming the base file supplies the key, not before. Order matters: if the override is currently the only source of `JwtSettings__Key`, deleting it first takes the API down. Deploy, run `docker compose config` on the server to confirm the key resolves from the base file, then remove the override and re-check.
- Host-level diagnostics that addressed `localhost:5000` must move to `docker exec meu_site_api ...` or route through nginx.
- The deploy pulls this repository's `docker-compose.yml` onto the server. Any file on the server tracked by git must have no local modifications, or `git pull` fails and the deploy stops.
