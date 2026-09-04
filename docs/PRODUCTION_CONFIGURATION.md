# Production configuration

## Configuration architecture

ASP.NET Core loads `appsettings.json`, the environment-specific file, and then environment variables. Production secrets belong in the hosting platform's protected settings, never in tracked JSON. Nested keys use double underscores in environment-variable names.

Development uses .NET user-secrets and `appsettings.Development.json`. Production uses `appsettings.Production.json`, whose logging and data-initialization values are non-secret defaults.

## Required production settings

- `ConnectionStrings__DefaultConnection`

The PostgreSQL connection supports Npgsql format and hosted `postgres://` or `postgresql://` URIs. URI connections are normalized with TLS required. The application fails at startup with the configuration key name, but never the value, when this setting is absent or malformed.

## Optional integration settings

- `Gemini__ApiKey`
- `VNPAY__TmnCode`
- `VNPAY__HashSecret`
- `VNPAY__BaseUrl`
- `VNPAY__ReturnUrl`
- `VNPAY__IpnUrl`

Gemini runs server-side and degrades safely when its key is absent. VNPay configuration is validated only when a payment operation is requested. Keep `VNPAY__BaseUrl` on the sandbox endpoint for the portfolio deployment. Set the public HTTPS return and IPN URLs after the Azure hostname is assigned in Phase 6.

## Managed cloud reverse proxy

Set `ASPNETCORE_ENVIRONMENT=Production`. Railway or Render should also set `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` so the framework accepts the managed proxy's forwarded scheme and client address. The application processes one forwarded hop before HTTPS redirection. Do not configure arbitrary CORS origins; browser traffic, APIs, and SignalR are same-origin.

Production enables HSTS, HTTPS redirection, a generic error page, secure cookies, console logging, and reduced framework/EF command logging. Development retains useful diagnostics and HTTP support at `http://localhost:5005`.

## Cookie and SignalR security

Authentication and session cookies are HttpOnly, SameSite=Lax, and always Secure in Production. `/Auth/CurrentUser` returns minimal JSON with 200 for an authenticated identity and JSON 401 otherwise. The order hub remains authenticated; joining `order-{id}` requires Admin/Staff membership or ownership of that order. SignalR keeps WebSocket negotiation and fallback transports enabled.

## Seed and bootstrap policy

Production defaults both flags to false:

- `DataInitialization__EnableBootstrapUsers`
- `DemoSeed__Enabled`

Bootstrap is configuration-driven and requires the corresponding `BootstrapAdmin__*` or `BootstrapStaff__*` values. It does not overwrite existing accounts. Enable it only for a deliberate initial setup, then disable it. The deterministic demo dataset requires explicit opt-in with `DemoSeed__Enabled`; `DemoSeed__AnchorDate` is an optional non-secret `yyyy-MM-dd` anchor. Seeding is insert-only, idempotent, and must remain disabled during normal Production startup.

EF migrations are deployment operations. Runtime startup does not call `Database.Migrate`, execute DDL, or recreate schema.

## Database security model

The intended path is Browser → ASP.NET Core → EF Core/Npgsql → Supabase PostgreSQL. The connection string is server-only and must use TLS. Use a dedicated least-privileged PostgreSQL runtime role. Do not ship database credentials to HTML or JavaScript, and do not grant Supabase anon/authenticated Data API privileges on private application tables unless a later design explicitly needs them. RLS and privilege changes are deferred and must be tested separately.

## Logging and health

Logs include method, path, status, duration, and client address; request bodies, headers, cookies, authorization data, and query strings are excluded. Gemini upstream bodies and keys are not logged. `GET /health` is an unauthenticated liveness endpoint and exposes no configuration or database details.

## Phase 6 mapping

Map the environment-variable names above to protected Railway or Render variables. Do not place their values in deployment manifests or repository files. See `docs/CLOUD_DEPLOYMENT.md` for the container deployment model.
