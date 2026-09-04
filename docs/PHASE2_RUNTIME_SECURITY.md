# Phase 2 Runtime Security

## Explicit role bootstrap

Admin and Staff bootstrap runs only when all three required values for that role
are supplied. Existing users matched by either username or normalized email are
left unchanged. Passwords are hashed with the application's `PasswordHasher` and
are never logged.

Required configuration key names:

- `BootstrapAdmin__Username`
- `BootstrapAdmin__Email`
- `BootstrapAdmin__Password`
- `BootstrapStaff__Username`
- `BootstrapStaff__Email`
- `BootstrapStaff__Password`

Optional display-name keys:

- `BootstrapAdmin__FullName`
- `BootstrapStaff__FullName`

Use protected environment configuration or .NET User Secrets locally. Do not put
values in tracked configuration files.

## SignalR order privacy

The order hub requires authentication. A customer can join an order group only
when the order's `CreatedByUserId` matches the authenticated user ID. Admin and
Staff roles may join order groups, while the dashboard group is restricted to
Admin. Status broadcasts contain only the order ID and status labels.

Customer history and detail pages reconnect automatically and rejoin their
authorized groups. Their server-rendered content remains the fallback when a
real-time connection is unavailable.

## Supabase exposure and RLS recommendation

Repository inspection found no Supabase JavaScript client, PostgREST call, anon
key, or browser-to-database access. Browser code calls same-origin ASP.NET
routes; application tables are accessed by server-side EF Core through Npgsql.

For public deployment:

1. Use a dedicated least-privileged PostgreSQL login for the application.
2. Keep its connection string exclusively in protected hosting configuration and
   require TLS.
3. Do not grant application-table privileges to Supabase `anon` or
   `authenticated` roles.
4. Prefer a private, non-Data-API-exposed schema. If tables remain in an exposed
   schema, test deny-by-default RLS in staging before enabling it.
5. Restrict database network access where the hosting topology permits it.

No RLS or database permission change is performed during Phase 2.
