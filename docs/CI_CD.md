# GitHub Actions CI/CD

## Architecture

Pull requests run the non-production CI workflow in `.github/workflows/ci.yml`. A push to `main` runs `.github/workflows/deploy-production.yml`: Release validation must pass before the existing Railway `coffeeshop-demo` service is deployed, followed by bounded health and public smoke checks.

Azure is not used. Railway remains the production platform and continues to own all application runtime configuration, including the Supabase Session Pooler connection and Gemini key.

## Pull request validation

The `CI` workflow has read-only repository permissions and performs:

1. checkout;
2. setup of .NET 10;
3. `dotnet restore`;
4. Release build;
5. all backend tests from `Ecommerce-Coffee-Shop.sln`, including `SaleStore.Tests`.

Both restore and Release build target the solution. Tests use `--no-build
--no-restore` and produce isolated TRX results under `artifacts/backend-tests`,
uploaded with seven-day retention even when tests fail.

It has no Railway token, no production environment, and no deployment job. The mutable hosted Playwright suite and the larger extended UI suite are deliberately excluded from untrusted pull requests.

## Production deployment

Pushes to `main` and manual dispatches run the production workflow. Its validation job restores the solution, builds Release, and runs the backend
.NET tests without production credentials. Deployment has `needs: validate`, so
a failed restore, build, or test blocks Railway deployment. Only the dependent deploy job is attached to the GitHub Environment `CoffeeShop-Demo`.

The deploy job uses Railway CLI `5.49.1` with an environment-scoped project token. Project and service identifiers are supplied explicitly so CI cannot deploy an unrelated service. One deployment runs at a time through the `railway-production` concurrency group. A failed CLI upload/deploy receives one bounded retry to tolerate transient network failures; the job remains failed if both attempts fail.

Railway runtime secrets are neither passed through the deployment command nor duplicated in GitHub.

## GitHub configuration

GitHub Environment: `CoffeeShop-Demo`

Required secret name:

- `RAILWAY_TOKEN`

Required variable names:

- `RAILWAY_PROJECT_ID`
- `RAILWAY_SERVICE_ID`
- `PRODUCTION_URL`

The public URL variable points at the canonical Railway HTTPS origin: `https://coffeeshop-demo-production.up.railway.app`. The workflow validates `/health`, the database-backed home page, and a static stylesheet. Authentication behavior remains covered by the credentialed critical Playwright suite rather than the infrastructure smoke gate.

## Production role E2E

After Railway deploy, health, and public smoke validation succeed, the protected deployment job installs Chromium and runs `tests/critical-demo.spec.js` serially against `PRODUCTION_URL`. It covers public/customer flows, Admin, Staff, Gemini, and the private SignalR order update. The suite requires exactly five passing tests with no failures or skips.

The following secret names exist only in the protected `CoffeeShop-Demo` Environment:

- `BOOTSTRAP_ADMIN_USERNAME`
- `BOOTSTRAP_ADMIN_PASSWORD`
- `BOOTSTRAP_STAFF_USERNAME`
- `BOOTSTRAP_STAFF_PASSWORD`

They are mapped only for the E2E presence-check and execution steps to the `BootstrapAdmin__Username`, `BootstrapAdmin__Password`, `BootstrapStaff__Username`, and `BootstrapStaff__Password` variables consumed by the tests. They are not available to the pull-request workflow and are never written to files or artifacts.

Only Chromium is installed. Each run uses a unique output directory with retries and tracing disabled to avoid retaining credential-bearing browser traces. A JSON summary gate requires exactly five passes with zero failures and zero skips. If the critical step fails, screenshots and page-context diagnostics are retained for seven days; the workflow never creates or uploads reusable authentication storage state.

The first complete production gate passed in GitHub Actions run 21: five tests passed, with zero failures and zero skips. The tests targeted the canonical Railway origin through `PRODUCTION_URL`; providing this variable disables Playwright's local web server.

## Database and seed policy

CI/CD never runs EF migration commands, `Update-Database`, database reset, or seed reset. Production keeps `DemoSeed__Enabled=false`; Railway's existing runtime variables remain authoritative. Schema migrations require a separate reviewed operation.

## Failure and rollback

An upload is not considered successful until Railway CLI exits successfully and the public health/smoke checks pass. All checks have bounded timeouts and retries.

For rollback, revert the faulty commit and push the revert through the same pipeline, or redeploy a known-good Railway revision. Never roll back by deleting Supabase data or applying an unreviewed schema change.

## Troubleshooting

- Restore/build failure: reproduce the exact Release commands locally.
- Railway authentication failure: confirm the project-scoped token exists in the `CoffeeShop-Demo` Environment without printing it.
- Wrong target: confirm the three GitHub variable names and their Environment scope.
- Deployment accepted but unhealthy: inspect safe Railway startup logs, then verify `/health` and the public origin.
- Do not dump GitHub secrets, Railway variables, environment values, connection strings, or authentication state into logs or artifacts.

## Phase 8 verified hosted results

Phase 8 hosted validation passed on 2026-09-06. [PR #3](https://github.com/Nolove098/Ecommerce-Coffee-Shop/pull/3)
was validated by [CI run #1](https://github.com/Nolove098/Ecommerce-Coffee-Shop/actions/runs/34007722746)
and merged normally as `755bfab196b34d9e0937f17368e5875e000b1f48`.
[Deploy production run #23](https://github.com/Nolove098/Ecommerce-Coffee-Shop/actions/runs/34008065465)
then completed successfully on that merge commit:

- Hosted restore and Release build passed with zero warnings and zero errors.
- Hosted .NET tests: 74 passed, 0 failed, 0 skipped, matching PR CI and the local baseline.
- Railway deployed the existing `CoffeeShop-Demo` project / `coffeeshop-demo` service;
  a read-only Railway status check confirmed a successful active deployment and a running instance.
- HTTPS health returned HTTP 200; production smoke passed for `/` and `/css/custom.css`.
- Production critical Playwright: 5 passed, 0 failed, 0 skipped. Customer registration,
  login, cart, COD checkout/history, Admin/Staff pages, Gemini, and private customer
  SignalR updates passed against `vars.PRODUCTION_URL`, with no localhost fallback.

The GitHub Environment remains `CoffeeShop-Demo`. PR CI uses no production secrets,
production database, live Gemini, or payment integration. Only the protected production
E2E job exercises the existing demo application and creates its own users/orders.
No schema changes, automatic migrations, or production reseeding were introduced or run.
Read-only runtime verification confirmed `DemoSeed__Enabled=false` and
`DataInitialization__EnableBootstrapUsers=false`; demo-data initialization retains
its production configuration default of false. Runtime secrets remain in Railway.
No secrets were introduced or exposed, and no force push was used.

Local Release validation also passed with zero warnings/errors and 74 passed, 0 failed,
0 skipped on three consecutive runs. Coverage is informational; see [TESTING.md](TESTING.md).
Gitleaks 8.30.1 found no leaks with full redaction. Actionlint 1.7.12 passed both
workflows with optional external ShellCheck/Pyflakes disabled. Generated logs, test
results, coverage, and audit artifacts remain excluded from commits.
