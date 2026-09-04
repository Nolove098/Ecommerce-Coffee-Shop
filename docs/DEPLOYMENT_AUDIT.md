# Deployment Audit

## Scope and date

- Phase: 0 (repository audit only)
- Audit date: 2026-08-28
- Application behavior was not changed.
- Secret values are intentionally omitted. Findings name only paths and configuration keys.
- Git status/history could not be audited because this workspace is not a Git working tree (`.git` is absent or unavailable to Git).

## Executive summary

The repository is a single-project ASP.NET Core application using MVC, Razor Pages, server-side Blazor, Entity Framework Core, PostgreSQL, cookie authentication, SignalR, Gemini, ML.NET, and VNPay. The solution restores and builds when the solution file is named explicitly. The build completes with zero errors and one warning.

The application currently targets unsupported .NET 6 and has critical credential exposure risks in current source/configuration. Phase 1 should remove the tracked values and require rotation of every exposed external credential before any public deployment.

## Repository inventory

| Area | Finding |
|---|---|
| Solution | `Ecommerce-Coffee-Shop.sln` |
| Web project | `SaleStore.csproj` |
| Entry point | `Program.cs`; no `Startup.cs` |
| UI | ASP.NET Core MVC/Razor views, Razor Pages, and one server-side Blazor component |
| Application areas | Customer controllers/views plus `Areas/Admin` and `Areas/Staff` |
| Persistence | EF Core `ApplicationDbContext`, migrations, startup SQL initialization, and seed data |
| Tests | JavaScript Playwright specifications under `tests/`; no .NET test project found |
| Node setup | `package.json`, `package-lock.json`, and `playwright.config.js` |
| Deployment automation | No Dockerfile, publish profile, Azure configuration, or `.github` workflow found |

## Current architecture

Request handling is configured in `Program.cs`. MVC controllers, Razor Pages, and area routes provide customer, admin, and staff features. Business/integration code is primarily in `Services/`. EF Core accesses PostgreSQL through `Data/ApplicationDbContext.cs`. Authentication uses an application-owned user table and ASP.NET Core cookie authentication. Role authorization protects admin and staff controllers.

At process startup, `Program.cs` creates a service scope and calls `AuthDbInitializer.EnsureCreatedAsync` and `MlDataSeeder.SeedAsync`. The initializer executes schema SQL and may seed application data, so merely starting the application mutates the configured database.

### Runtime and NuGet packages

- Target framework: `net6.0`
- SDK pin in `global.json`: `6.0.0` with `latestFeature` roll-forward
- SDK selected during audit: `6.0.100`
- Host runtime reported by `dotnet --info`: `9.0.6`

| Package | Version |
|---|---:|
| EFCore.NamingConventions | 6.0.0 |
| Microsoft.AspNetCore.SignalR.Client | 6.0.0 |
| Microsoft.EntityFrameworkCore.Design | 6.0.0 |
| Microsoft.ML | 3.0.1 |
| Microsoft.ML.Recommender | 0.21.1 |
| Microsoft.ML.TimeSeries | 3.0.1 |
| Newtonsoft.Json | 13.0.4 |
| Npgsql.EntityFrameworkCore.PostgreSQL | 6.0.29 |

### Database

- Provider: PostgreSQL through `Npgsql.EntityFrameworkCore.PostgreSQL`
- Naming: snake_case through `EFCore.NamingConventions`
- Configuration key: `ConnectionStrings:DefaultConnection`
- Migrations: one payment-fields migration and an EF model snapshot are present.
- Additional schema management: `Data/AuthDbInitializer.cs` runs raw idempotent DDL/updates at every startup.
- Seed behavior: authentication/product seed logic and ML seed data are invoked at startup.

### Authentication and authorization

- ASP.NET Core cookie authentication with cookie name `salestore.auth`
- Login path: `/Auth/Login`; access-denied path: `/Auth/AccessDenied`
- Sliding expiration: 14 days
- Each cookie validation reloads the user and role from PostgreSQL.
- Roles are application-defined (`Admin`, `Staff`, and user/customer role) and enforced with `[Authorize(Roles = ...)]` on admin/staff controllers.
- Password hashing is application-owned in `Services/PasswordHasher.cs`.

### SignalR

- Hub: `Hubs/OrderHub.cs`
- Route: `/hubs/order`
- Groups: per-order groups and `admin-dashboard`
- Publisher: `Components/OrderStatusUpdater.razor` sends `OrderStatusChanged` and `OrderUpdated` events through `IHubContext<OrderHub>`.
- Server-side Blazor is registered and mapped. There is no explicit `AddSignalR()` call; hub services appear to be supplied transitively by the server-side Blazor registration. This should be made explicit/reviewed in a later implementation phase.

### Gemini

- Main integration: `Services/GeminiChatService.cs`
- Registration: singleton `IChatBotService` in `Program.cs`
- Configuration key: `Gemini:ApiKey`
- Consumers: `Controllers/ChatBotController.cs` and `Controllers/AiController.cs`
- Risk: Gemini error response bodies are written to application logs and need a production-safety review.

### ML.NET

- Product recommendations: `Services/ProductRecommendService.cs`, using matrix factorization over delivered-order purchase data with a best-seller fallback and hourly retraining.
- Sales forecasting: `Services/SalesForecastService.cs`, using SSA over up to 90 days of delivered-order revenue.
- Seed support: `Data/MlDataSeeder.cs`.
- API/UI access: `Controllers/AiController.cs` and admin reporting/dashboard code.

### Payments

- Active VNPay integration: `Services/VnPayService.cs`, `Services/IVnPayService.cs`, and `Controllers/CartController.cs`.
- VNPay configuration keys: `VNPAY:TmnCode`, `VNPAY:HashSecret`, `VNPAY:BaseUrl`, `VNPAY:ReturnUrl`, and `VNPAY:IpnUrl`.
- Callback routes found in source: `/Cart/VnPayReturn` and `/Cart/VnPayIpn`.
- MoMo implementation files exist (`Services/MoMoService.cs` and `Services/IMoMoService.cs`) and reference `Momo:*` settings, but the service is not registered and the cart controller says it was removed. It appears dormant.

### Playwright

- Test runner: `@playwright/test`
- Accessibility helper: `@axe-core/playwright`
- Installed Playwright version after `npm install`: `1.58.2`
- Configuration: Chromium project, fully parallel, HTML/list reporters, base URL `http://localhost:5005`, and a `dotnet run` web server.
- Coverage present: responsive layouts, desktop/tablet/mobile behavior, Bootstrap components, accessibility, performance, and preservation properties.
- Phase 0 requires setup/version validation only; the browser suite was not run.

## Secret-location audit

The following are potential or confirmed credential locations. Values were not copied into this report.

| Severity | Path | Key/location |
|---|---|---|
| Critical | `Program.cs` | fallback `ConnectionStrings:DefaultConnection` credential |
| Critical | `appsettings.json` | `ConnectionStrings:DefaultConnection` (non-empty) |
| Critical | `appsettings.json` | `Gemini:ApiKey` (non-empty) |
| Critical | `appsettings.json` | `VNPAY:TmnCode` (non-empty) |
| Critical | `appsettings.json` | `VNPAY:HashSecret` (non-empty) |
| High | `Data/AuthDbInitializer.cs` | hard-coded seeded administrator password literal |
| Review | `appsettings.json` | `VNPAY:BaseUrl`, `VNPAY:ReturnUrl`, and `VNPAY:IpnUrl` are non-empty endpoints, not secrets by themselves |
| Review | `Services/MoMoService.cs` | expected `Momo:AccessKey` and `Momo:SecretKey`; no corresponding settings were found in `appsettings*.json` |

Because Git metadata is unavailable, this audit cannot determine whether these files are tracked, whether the values were previously committed, or whether additional values exist in Git history.

## External dependencies

- .NET 6 SDK/runtime and NuGet.org
- PostgreSQL database (the current configuration appears intended for a remote hosted instance)
- Google Gemini HTTP API
- VNPay sandbox/payment service
- ML.NET libraries
- Node.js/npm and the npm registry
- Playwright Chromium browser runtime (browser installation was not requested in Phase 0)
- External static/image resources referenced by the UI and seed data
- MoMo HTTP API code exists but appears inactive

## Validation results

| Command | Result | Notes |
|---|---|---|
| `dotnet --info` | PASS | Selected SDK 6.0.100 from `global.json`; installed SDKs include 6.0.100 and 9.0.301. |
| `dotnet restore` | FAIL | `MSB1011`: the repository root contains both a project and solution, so an explicit target is required. |
| `dotnet restore Ecommerce-Coffee-Shop.sln` | PASS | Required NuGet network access; restore completed successfully. |
| `dotnet build` | FAIL | Same `MSB1011` ambiguous-target issue. |
| `dotnet build Ecommerce-Coffee-Shop.sln --no-restore` | PASS | Zero errors, one `CS1998` warning at `Controllers/CartController.cs:140`. |
| `npm install` | PASS | Added/audited six packages; npm reported zero vulnerabilities. The first restricted attempt failed due to registry/cache access, then passed with scoped permission. |
| `npx playwright --version` | FAIL | PowerShell execution policy blocks `npx.ps1`. |
| `npx.cmd playwright --version` | PASS | Reports Playwright 1.58.2. |

Current build status: **buildable with an explicit solution/project target; zero errors and one warning**. The exact root-level commands documented in the plan are not reproducible as written because of CLI target ambiguity.

## Risks

1. Current configuration/source contains non-empty credentials, including a database credential fallback in executable source. Assume exposed credentials are compromised until rotated.
2. A seeded administrator account uses a hard-coded password literal and is created automatically when absent.
3. .NET 6 is out of support, and the repository pins a very old 6.0 SDK feature band.
4. Application startup connects to and mutates the configured database before serving requests. A wrong production connection string could affect real data.
5. No production exception-handler, HSTS, HTTPS-redirection, forwarded-header, or explicit production cookie policy was found in `Program.cs`.
6. Gemini upstream error bodies are logged; external response content may disclose sensitive request/account details.
7. Configuration safety cannot be guaranteed by `.gitignore`: it does not exclude local appsettings/secret variants listed in the deployment plan.
8. No automated backend tests, CI workflow, deployment manifest, or cloud configuration exists.
9. Playwright starts `dotnet run` without naming a project and may encounter the same ambiguous-target failure. It also depends on a configured reachable database during startup.
10. The declared Playwright dependency uses a caret range, so `npm install` resolved a substantially newer version than the minimum shown in `package.json`; test compatibility has not yet been verified.
11. Git tracking/history and repository cleanliness are unknown because this copy has no usable Git metadata.

## Blockers

- Security hardening and credential rotation are mandatory before any public deployment or live-flow testing.
- Git history cannot be audited from this workspace. A real clone with `.git` metadata is needed to establish what was committed and whether history contains secrets.
- The application should not be started against the currently configured database during this audit because startup performs database writes and the user prohibited production-data risk.
- Exact root-level `dotnet restore`, `dotnet build`, and PowerShell `npx` commands require reproducibility fixes or documented explicit alternatives in a later approved phase.

## Recommended next step

Proceed only with user approval to Phase 1 (Security Hardening). Phase 1 should first create the prescribed security branch in a real Git working tree, remove all live credential values and the database fallback from tracked source/configuration, replace the seeded administrator credential strategy, extend `.gitignore`, and add safe configuration documentation. The database password, Gemini API key, and VNPay credentials found in current files should be manually rotated by their owners; Git history should then be audited without printing values.

Do not deploy, start the application against the current database, rewrite Git history, or begin the framework upgrade until the security work and required user confirmations are complete.

## Phase 0 acceptance criteria

- Repository audited: **PASS**
- Current build status documented: **PASS**
- Secret locations documented safely: **PASS**
- External dependencies identified: **PASS**
