# Secure Configuration

The repository contains only empty secret placeholders. Supply local secrets with .NET user-secrets or environment variables. Never commit real credentials, connection strings, publish profiles, private certificates, or local secret files.

## Required settings

| Environment variable | Purpose | Required |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | Yes |
| `Gemini__ApiKey` | Google Gemini API key | For Gemini features |
| `VNPAY__TmnCode` | VNPay merchant code | For VNPay |
| `VNPAY__HashSecret` | VNPay signature secret | For VNPay |
| `VNPAY__BaseUrl` | VNPay payment endpoint | For VNPay; sandbox default is non-secret |
| `VNPAY__ReturnUrl` | Browser return callback URL | For VNPay |
| `VNPAY__IpnUrl` | Server notification callback URL | For VNPay |

Production bootstrap and deterministic demo data are disabled by default. Their explicit opt-in flags are `DataInitialization__EnableBootstrapUsers` and `DemoSeed__Enabled`.

The actual callback routes implemented by the application are `/Cart/VnPayReturn` and `/Cart/VnPayIpn`.

## Optional bootstrap administrator

No administrator with a source-controlled password is created. To create the first administrator when none exists, set all required bootstrap fields before the first application start:

| Environment variable | Purpose |
|---|---|
| `BootstrapAdmin__Username` | Initial administrator username |
| `BootstrapAdmin__FullName` | Initial administrator display name (optional) |
| `BootstrapAdmin__Email` | Initial administrator email |
| `BootstrapAdmin__Password` | Initial administrator password |
| `BootstrapStaff__Username` | Initial staff username |
| `BootstrapStaff__FullName` | Initial staff display name (optional) |
| `BootstrapStaff__Email` | Initial staff email |
| `BootstrapStaff__Password` | Initial staff password |

Set `DataInitialization__EnableBootstrapUsers=true` only during an intentional bootstrap. Remove the bootstrap variables and disable the flag after confirming that the account exists. Do not publish administrator credentials in documentation. Deterministic demo data requires `DemoSeed__Enabled=true`; `DemoSeed__AnchorDate` optionally selects its fixed `yyyy-MM-dd` anchor. Keep demo seeding disabled during normal Production startup.

See `docs/PRODUCTION_CONFIGURATION.md` for production cookies, proxy handling, logging, health checks, and the server-side Supabase security model.

## Dormant MoMo integration

`Services/MoMoService.cs` exists but is not registered by the application. If it is intentionally re-enabled later, it expects these settings:

- `Momo__Endpoint`
- `Momo__PartnerCode`
- `Momo__AccessKey`
- `Momo__SecretKey`
- `Momo__ReturnUrl`
- `Momo__NotifyUrl`
- `Momo__PartnerName`

## Local development with user-secrets

Initialize user-secrets for the web project:

```powershell
dotnet user-secrets init --project SaleStore.csproj
```

Set each required value interactively on your own machine. Do not paste real values into issue comments, chat transcripts, screenshots, or committed scripts.

The application reads standard ASP.NET Core configuration, so environment variables override `appsettings.json`. Use double underscores (`__`) to represent nested configuration sections.

## Local files

The following local-only patterns are ignored by Git:

- `appsettings.Local.json`
- `appsettings.Secrets.json`
- `.env`
- `.env.*`
- `*.publishsettings`
- `secrets.json`

`appsettings.json` remains committed as the safe configuration template and contains empty secret fields.
