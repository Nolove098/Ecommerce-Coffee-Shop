# Automated testing

## Test pyramid and current validation status

`SaleStore.Tests` is the isolated backend layer: xUnit 2.9.3, the .NET 10 SDK
template's VSTest stack (`Microsoft.NET.Test.Sdk` 17.14.1 and Visual Studio
runner 3.1.4), targeting `net10.0`. It references the real web project.
No mocking framework or alternate database provider is installed.

Local validation on 2026-09-06 passed: solution restore, Release build with
zero warnings/errors, and 74 executed tests across six classes. Three consecutive
post-fix runs each reported 74 passed, 0 failed, 0 skipped, with VSTest total times
of 0.8791s, 0.8284s, and 0.8779s. No test assertions needed weakening or removal.

Coverlet collector 6.0.4 also executed all 74 tests successfully. Its Cobertura
report measured the entire instrumented SaleStore assembly, including uncovered
application/UI code: 208/6,033 lines (3.44%) and 93/1,908 branches (4.87%). This is
informational whole-assembly coverage, not coverage of just the selected helpers
or end-to-end coverage. Results remain ignored under artifacts/coverage.

The local Windows Schannel client failed with SEC_E_NO_CREDENTIALS. Node HTTPS
reached the official NuGet service with certificate validation enabled and
downloaded official package/index/signature/vulnerability responses into an
ignored workspace HTTP cache. Package sources and security validation were not
changed. A workspace package directory avoided the read-only user package cache;
the existing user cache was used only as a fallback. Hosted CI uses normal restore.

Microsoft.EntityFrameworkCore.Relational is explicitly aligned to the existing
Design package at 10.0.11 so project-reference consumers resolve the same runtime
EF assemblies. This removes MSB3277 version conflicts without changing app logic.

Phase 8 is not yet complete: this session cannot write repository Git metadata
(fetch fails with permission denied for .git/FETCH_HEAD). No Phase 8 commit/push
or hosted gate execution has occurred. Remote main is 18 commits ahead of local
HEAD; integrate those commits in a Git-writable session before committing and
rerun local gates on that integrated tree. The reviewed remote Phase 7 deployment
job has been preserved in the local workflow, including its exact 5/0/0 E2E gate.

The middle integration layer is intentionally deferred. `Program.cs` requires
PostgreSQL configuration at startup and can initialize data when enabled.
The EF model includes a computed order-item total and relational constraints;
an EF InMemory substitute would not validate PostgreSQL behavior. A full test
host requires deliberate startup isolation and a separate persistence strategy.

Existing Playwright critical tests remain the end-to-end layer. Extended tests
cover responsive layouts, Bootstrap behavior, preservation, accessibility,
performance, and Tailwind removal. They are not replaced with MVC mocks.

## Backend coverage implemented in source

- Password hashing: ASCII/Unicode/empty-password round trips, case/whitespace
  mismatches, tampered hashes, malformed Base64, and invalid hash lengths.
  Malformed Base64 currently throws `FormatException`; these are characterization
  tests, not a claim that login recovers gracefully from corrupt stored hashes.
- PostgreSQL normalization: URI schemes, decoded username/password/database,
  default/custom ports, TLS defaults, quoted Npgsql values and options,
  invalid configuration with sanitized errors, scoped Supabase session-pooler
  rewriting, untouched unrelated/already-pooled hosts, and missing pooler settings.
  All connection values are synthetic and no connection is opened.
- Strong password validation: the eight-character boundary and each required
  character class, non-string/null values, Unicode digits, and whitespace as a
  special character. Tests preserve the actual policy rather than strengthen it.
- Order status: Vietnamese labels, badge selection, and the unknown/legacy-value
  pending-display fallback. These do not validate transitions or authorize updates.
- Chatbot controller: null/empty history rejection without invoking the service,
  preservation of message order, newest-20 history limit, and reply projection.
- Gemini service: missing-key behavior, outbound JSON conversation content/roles,
  successful response extraction, missing/malformed responses, HTTP errors,
  transport exceptions, and suppression of provider detail/key values in errors
  and logs. A custom `HttpMessageHandler` answers every request in memory.

Each test owns its fakes and input data. There are no sleeps, shared application
state, configuration-file reads, environment configuration providers, or live
network transports. Hash tests accept generated salts without depending on any
particular random bytes. The application entry point is never started.

## Run locally

From the repository root, with .NET 10 and NuGet access:

```powershell
dotnet restore ./Ecommerce-Coffee-Shop.sln
dotnet build ./Ecommerce-Coffee-Shop.sln -c Release --no-restore
dotnet test ./Ecommerce-Coffee-Shop.sln -c Release --no-build --no-restore
```

Repeat the final command three times to check stability after the first green
run. It must actually discover `SaleStore.Tests`; testing only `SaleStore.csproj`
does not execute this suite.

If the CLI home is not writable in a restricted local session, set
`DOTNET_CLI_HOME` to an ignored directory under `artifacts/` before running the
commands. This does not grant package network access.

For this restricted session, restore used these process-local cache settings
after securely filling the HTTP cache from official NuGet URLs:

```powershell
$env:NUGET_HTTP_CACHE_PATH = Join-Path (Get-Location) 'artifacts/nuget-http-cache'
$env:NUGET_PACKAGES = Join-Path (Get-Location) 'artifacts/nuget-packages'
dotnet restore ./Ecommerce-Coffee-Shop.sln '-p:RestoreAdditionalProjectFallbackFolders=C:\Users\ASUS TUF\.nuget\packages'
```

The fallback path is specific to this machine; normal developer/CI restores do
not need it. No NuGet configuration or trusted-source setting was modified.

Optional informational coverage uses Coverlet collector 6.0.4:

```powershell
dotnet test ./SaleStore.Tests/SaleStore.Tests.csproj -c Release --no-build --no-restore --collect:"XPlat Code Coverage" --results-directory ./artifacts/coverage
```

The output is Cobertura XML for instrumented production code reached by the
backend suite, not end-to-end/browser coverage. Report the actual tool, assembly
scope, and measured result when collected. No percentage threshold is enforced.

## Playwright

```powershell
npm ci
npx playwright install chromium
npm run test:critical
npm run test:extended
```

By default Playwright starts the local application at `http://localhost:5005`.
That application needs a separately provisioned safe database and configuration;
these browser tests are not credential-free unit tests. `PLAYWRIGHT_BASE_URL`
selects an existing authorized deployment and disables automatic local startup.

The critical suite contains five tests: public/protected routes; customer
registration/login/cart/COD checkout/history; Gemini; authenticated Admin/Staff
pages and AI endpoints; private order SignalR updates. It creates demo users and
orders and updates its own order through the UI. It calls live Gemini. Run it
against a hosted demo only as explicitly authorized; never real payment flows.
Admin/Staff tests need the existing bootstrap account environment configuration.
Missing credentials can produce local skips; a production acceptance result
must be **5 passed, 0 failed, 0 skipped**. The deployment workflow checks
presence before running E2E so missing account configuration fails visibly.

Extended tests are optional regressions and are not deployment gates. Browser
traces/reports can contain sessions or request details; do not commit or upload
them. Phase 8 adds only isolated .NET TRX uploads; the existing protected production
workflow retains its Phase 7 screenshot/page-context failure diagnostics.

## Intentionally deferred behavior

- `AppRoles` contains constants only; constant-equality tests add no value.
- Order state changes, ownership enforcement, cart totals, EF queries and payment
  callbacks need separate domain/persistence isolation. No transition state
  machine currently exists; tests must not invent one. Admin status parsing uses
  `Enum.TryParse` without `Enum.IsDefined`, so numeric undefined values deserve
  a future isolated workflow regression test and minimal fix.
- Forecast preprocessing/managed fallback are private within an EF/clock/native
  ML workflow. Recommendation training and fallback both query EF. Their
  algorithms and native dependency recovery are not covered by this suite.
- No WebApplicationFactory, PostgreSQL integration, real Gemini, Railway,
  VNPay, or MoMo tests are included in the .NET project.
- Password storage corruption still propagates exceptions. Any change to that
  runtime contract needs authentication regression validation.

## CI and safety

Both workflows restore/build/test the solution. PR CI never deploys and requires
no application secrets. Main deployment depends on the test job succeeding;
existing Railway targeting is retained. See [CI_CD.md](CI_CD.md).

Never load production credentials into the unit suite, use Supabase as a
disposable database, apply migrations, reset schemas, or reseed production.
DemoSeed must stay off in production. Phase 8 introduces no database or runtime
behavior changes. `bin/`, `obj/`, `TestResults/`, `test-results/`, and `artifacts/`
are ignored. The web project excludes `SaleStore.Tests/**` from its default items
so test sources, packages and results are not included in application publish.

Gitleaks 8.30.1 scanned all 14 intended Phase 8 files with full redaction and
reported no leaks. Manual review confirmed all credential-like test inputs are
synthetic. Actionlint 1.7.12 passed both workflows (optional external ShellCheck
and Pyflakes integrations were disabled). Neither result substitutes for hosted
CI or the pending Git integration/commit/push.
