# Implementation 03: persona routing and seed accounting

Date: 2026-09-21. Continues [implementation 02](Implementation-02-Revocation-and-Build.md). Authoritative status: [master tracker](../../MASTER-TODO-TRACKER.md).

## Implemented

LifeCycle's task router now consumes `IPersonaRoleResolver` from the existing Models interface layer. UserManagement's existing PersonaProfileService implements it through IPersonaProfileService; both registration entry points resolve that same scoped service. LifeCycle no longer references UserManagement's persona entity or constructs dynamic mapping records. No second persona store or role model was introduced.

Role-name mappings select active catalog personas by PERSONA_ID, return current catalog codes and deduplicate destinations. Inactive and missing catalog entries cannot become routing destinations through stale denormalized persona codes. The resolver grants no authorization; app-owned roles and resource checks remain responsible for access.

Lifecycle seed results now have separate delegation-threshold, business-trigger and SoD counters included in the computed total. TablesSeeded derives from categories with inserts. SoD seeding returns the number inserted rather than an assumed 25, skips existing rule names and checks cancellation before writes. SoD failures are recorded in the overall result instead of being logged as a harmless skip.

## Verified scope

- API build: **5 errors / 697 warnings**, 21.89 seconds. All five errors are in PPDMAccountingService. Persona reverse-dependency errors and writes to computed seed totals are cleared.
- **22 focused tests passed** in the existing temporary source-linked harness: 18 notification integration cases and four new persona/seeding tests. New tests execute production PersonaProfileService, SodEvaluationEngine and PPDMGenericRepository with an in-memory mocked IDataSource, testing active catalog resolution, repeat/partial seed runs, insertion exceptions and cancellation before writes.
- Real database/provider semantics, full LifeCycleSeedService orchestration, API dependency-container startup, normal API/Web test projects and browser journeys remain unverified. Passing focused tests does not close FG-00 or FG-20.

Tests live in `Beep.OilandGas.ApiService.Tests/PersonaRoutingAndSeedTests.cs`. `%TEMP%/beep-notification-harness-path.txt` identifies the temporary harness. Its `TestResults/persona-seed-notifications.trx` and `%TEMP%/beep-persona-seed-tests.log` record results. `%TEMP%/beep-oilgas-build-current.log` records the API build. Temporary files are local evidence, not reproducible CI artifacts.

## Remaining build work

PPDMAccountingService has two nullable-volume assignments, an invalid cost allocation overload, a nonexistent royalty-query call and an unsupported result property. These require semantic correction as well as compilation repairs:

- Reconciliation currently passes a field ID to allocation-ID lookup, ignores the requested dates and uses the same value as both measured and allocated volume before declaring a match.
- Cost allocation calls IAllocationService, whose actual contract allocates a RUN_TICKET's production volumes. ICostAllocationService instead accepts an ACCOUNTING_COST and explicit COST_ALLOCATION targets; field/date requests need authoritative cost and target selection.
- Royalty calculation must distinguish querying stored calculations from performing a calculation, account for all applicable products/records and preserve quantity/rate units. Replacing the nonexistent call alone would retain the current first-record and gross-volume-as-royalty-volume errors.

Next: define and implement these adapter mappings against existing production/accounting services and stored data, with focused tests for missing inputs, field/date boundaries, mixed products and failure propagation. Then rerun the build to expose and resolve any subsequent errors before attempting the normal test suites. FG-11's standard app-role bridge remains open.
