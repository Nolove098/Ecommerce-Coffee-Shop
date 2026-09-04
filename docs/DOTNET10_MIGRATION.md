# .NET 10 migration

The Phase 3 baseline was repaired before Phase 4 configuration work.

- Target framework: `net10.0`
- SDK pin: `10.0.400` with feature-band roll-forward
- EF Core Design: 10.0.11
- Npgsql EF Core provider: 10.0.0
- EFCore.NamingConventions: 10.0.0
- SignalR client: 10.0.11
- Npgsql's obsolete `TrustServerCertificate` assignment was removed; PostgreSQL URI normalization still requires TLS.

No migration was generated or applied, and no schema operation was performed. Restore, build, migration model comparison, application startup, and the critical Playwright flows are the validation gates.
