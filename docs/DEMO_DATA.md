# Deterministic demo data

## Purpose

The CoffeeShop-Demo dataset supplies a stable menu and enough historical sales activity for recruiter demonstrations of dashboards, forecasting, product recommendations, order operations, and SignalR updates. It is deliberately small and quick to query.

## Seed architecture

`Data/DeterministicDemoSeeder.cs` runs only when `DemoSeed__Enabled` is explicitly enabled. Both Development and Production tracked configuration default it to off. Schema migrations and account bootstrap are separate operations; the demo seed never runs migrations or creates login credentials.

Configuration names:

- `DemoSeed__Enabled`
- `DemoSeed__AnchorDate`

The optional anchor uses `yyyy-MM-dd`. A fixed anchor makes dates and generated records repeatable.

## Dataset composition

The seed ensures a usable active menu, ten deterministic non-login customer records, and ninety orders spanning ninety days. Each order contains two to four items. Most orders are delivered, with a small current mix of pending, ready, and cancelled states. Orders use COD and never create VNPay transactions. Order totals are calculated from item quantities and unit prices.

Products are added only when the existing active menu has fewer than twelve entries. Existing products are never overwritten. The current CoffeeShop-Demo menu already exceeds that threshold, so its existing products remain authoritative.

Reviews are not fabricated because the schema requires a real application user. Recommendations train from delivered order/product interactions instead.

## Determinism and duplicate prevention

Customers use stable demo phone identifiers and orders use stable `COFFEE-DEMO-V1:` note markers. The seeder looks these keys up before inserting anything. It does not rely on database-generated numeric IDs and does not alter user-created rows.

Rerunning with the same anchor is a no-op. To reseed intentionally, provide the two configuration names above through protected runtime configuration and start the application once, then disable the flag again. Removing or replacing an existing seeded generation is a separate, reviewed data-maintenance operation; normal application startup never resets data.

## Recruiter and test-data policy

Admin and Staff remain protected configuration-driven accounts. No public customer password is stored or documented. Browser tests create uniquely marked `phase2-...@example.test` accounts and must not depend on fixed product, customer, or order IDs.

Automated records may be removed only by an exact marker-based transaction after verifying expected counts and foreign-key relationships. Never treat all `example.test` addresses or all customer orders as disposable. Permanent demo records use the stable seed identifiers above and are excluded from automation cleanup.
