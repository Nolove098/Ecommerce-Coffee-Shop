# Public cloud deployment

## Live environment

- Platform: Railway
- Service: `coffeeshop-demo`
- Region: `sfo`
- URL: `https://coffeeshop-demo-production.up.railway.app`
- Health: `https://coffeeshop-demo-production.up.railway.app/health`
- Runtime: ASP.NET Core .NET 10 in a Linux Docker container

Azure is not used for this deployment.

## Status and architecture

Azure is not used. Railway is the primary target and Render is the portable fallback. The deployment path is Browser → Railway HTTPS proxy → ASP.NET Core .NET 10 container → EF Core/Npgsql → CoffeeShop-Demo Supabase PostgreSQL.

The repository uses a multi-stage `Dockerfile`: the .NET 10 SDK image restores and publishes the application, and the smaller ASP.NET Core 10 runtime image runs the published output as the image's non-root application user. Railway builds this Dockerfile through `railway.json`.

## Runtime configuration

Railway provides `PORT`; the application binds it to `0.0.0.0`. When `PORT` is absent, local launch settings and `--urls http://localhost:5005` continue to work. The container defaults to Production and port 8080.

Set these non-secret variables:

- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_FORWARDEDHEADERS_ENABLED`
- `DemoSeed__Enabled`
- `DataInitialization__EnableBootstrapUsers`

Production values keep demo seeding and account bootstrap disabled. The forwarded-header switch is enabled only behind the managed proxy so HTTPS redirects and Secure cookies use the original browser scheme.

Protected variable names:

- `ConnectionStrings__DefaultConnection`
- `Gemini__ApiKey`
- `VNPAY__TmnCode`
- `VNPAY__HashSecret`
- `VNPAY__BaseUrl`
- `VNPAY__ReturnUrl`
- `VNPAY__IpnUrl`

Only the database connection is required for process startup. Gemini is required to demonstrate AI features. VNPay variables are required only for sandbox payment operations. Bootstrap variables are not needed because the protected Admin and Staff accounts already exist.

Enter protected values through the platform's Variables/Secrets UI. Never place values in source, Docker layers, CLI arguments, documentation, or deployment scripts.

## Deployment

Authenticate with Railway, create or link only the dedicated CoffeeShop-Demo project/service, configure variables, and run `scripts/deploy-cloud.ps1`. The script performs Release restore/build/publish, deploys through Railway, and optionally checks a supplied public URL. It contains no credentials.

Railway must expose an HTTPS-generated domain and use `/health` for deployment health checks. WebSockets and fallback transports remain available through the same-origin SignalR endpoint `/hubs/order`.

## Supabase and persistence

The service connects only to Supabase project `zkbcwtjoycbofdtmhzwk` using the protected server-side PostgreSQL Session Pooler connection. It does not create a Railway database, run migrations, or seed on restart.

Production also enables a narrowly scoped connection safeguard: if that protected value still names the exact direct endpoint for `zkbcwtjoycbofdtmhzwk`, the application switches only its endpoint and username format to the project's Sydney Session Pooler while retaining the protected password in memory. An already configured pooler value is left unchanged. Development does not force this behavior.

## VNPay sandbox callbacks

Configure the implemented callback routes as:

- Return: `https://coffeeshop-demo-production.up.railway.app/Cart/VnPayReturn`
- IPN: `https://coffeeshop-demo-production.up.railway.app/Cart/VnPayIpn`

Keep the VNPay base URL and credentials in sandbox mode.

## Validation

Validated publicly: `/health`, home/database-backed products, static assets, anonymous authorization behavior, customer registration/login, cart, COD checkout/history, Admin, Staff, Gemini, authenticated SignalR negotiation, and the private realtime order update. Production authentication cookies were verified as HttpOnly and Secure.

Order-status writes use an antiforgery-protected MVC POST, then publish the private SignalR event only after the database write succeeds. This avoids coupling the core staff workflow to a Blazor Server circuit while retaining WebSocket and fallback support for customer notifications.

Run `tests/critical-demo.spec.js` by setting `PLAYWRIGHT_BASE_URL` to the generated HTTPS origin; do not hard-code it in tests.

Final public recruiter-critical result on 2026-09-04 after restart: **5 passed, 0 failed, 0 skipped (57.7 seconds)**. The SignalR scenario also verifies that a customer cannot join another customer's private order group.

The service was redeployed after this validation. Health, Supabase-backed products, HTTPS, and authenticated role access remained operational; persistent demo data was unchanged and automatic demo seeding remained disabled.

VNPay remains an optional sandbox integration. Its credential variables are not required for the recruiter-critical COD flow. When enabled, use only sandbox credentials and the callback URLs documented above.

## Troubleshooting and rollback

Use Railway deployment and runtime logs, avoiding variable dumps. Missing configuration errors name only the setting. For database failures, check TLS, DNS, IPv4/IPv6, and Supabase session-pooler compatibility without displaying the connection string.

Rollback by selecting the last known-good Railway deployment or redeploying its known-good source/artifact. A rollback never changes, deletes, or reseeds Supabase data. Render can use the same Dockerfile, variables, health path, and startup behavior only if Railway is genuinely unavailable.
