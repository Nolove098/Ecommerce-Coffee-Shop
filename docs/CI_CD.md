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
5. any .NET tests discoverable from the current project.

It has no Railway token, no production environment, and no deployment job. The mutable hosted Playwright suite and the larger extended UI suite are deliberately excluded from untrusted pull requests.

## Production deployment

Pushes to `main` and manual dispatches run the production workflow. Its validation job repeats the Release gate without production credentials. Only the dependent deploy job is attached to the GitHub Environment `CoffeeShop-Demo`.

The deploy job uses Railway CLI `5.49.1` with an environment-scoped project token. Project and service identifiers are supplied explicitly so CI cannot deploy an unrelated service. One deployment runs at a time through the `railway-production` concurrency group.

Railway runtime secrets are neither passed through the deployment command nor duplicated in GitHub.

## GitHub configuration

GitHub Environment: `CoffeeShop-Demo`

Required secret name:

- `RAILWAY_TOKEN`

Required variable names:

- `RAILWAY_PROJECT_ID`
- `RAILWAY_SERVICE_ID`
- `PRODUCTION_URL`

The public URL variable points at the canonical Railway HTTPS origin. The workflow validates `/health`, the database-backed home page, and a static stylesheet. Authentication behavior remains covered by the credentialed critical Playwright suite rather than the infrastructure smoke gate.

## Production role E2E

Credentialed Admin/Staff Playwright validation is optional and is not configured in GitHub yet. It must never run for pull requests or untrusted forks. If later enabled in the protected `CoffeeShop-Demo` Environment, use secret names only and run the five-test suite after successful deployment and health validation.

The Phase 6 public validation was completed independently with 5 passed, 0 failed, and 0 skipped.

## Database and seed policy

CI/CD never runs EF migration commands, `Update-Database`, database reset, or seed reset. Production keeps `DemoSeed__Enabled=false`; Railway's existing runtime variables remain authoritative. Schema migrations require a separate reviewed operation.

## Failure and rollback

An upload is not considered successful until Railway CLI exits successfully and the public health/smoke checks pass. All checks have bounded timeouts and retries.

For rollback, revert the faulty commit and push the revert through the same pipeline, or redeploy a known-good Railway revision. Never roll back by deleting Supabase data or applying an unreviewed schema change.

## Troubleshooting

- Restore/build failure: reproduce the exact Release commands locally.
- Railway authentication failure: confirm the project-scoped token exists in the `production` Environment without printing it.
- Wrong target: confirm the three GitHub variable names and their Environment scope.
- Deployment accepted but unhealthy: inspect safe Railway startup logs, then verify `/health` and the public origin.
- Do not dump GitHub secrets, Railway variables, environment values, connection strings, or authentication state into logs or artifacts.
