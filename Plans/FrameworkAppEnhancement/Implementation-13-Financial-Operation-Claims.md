# Atomic financial operation claims - 2026-09-21

## Implemented

Added FinancialOperationClaim to the canonical TheTechIdea.Data.OilGas model and FinancialOperationClaimStore to the existing app Repository. The control database stores ownership metadata only, not duplicated accounting calculations or journals. SQL Server, PostgreSQL and Oracle migrations add FINANCIAL_OPERATION_CLAIM; all three model snapshots were regenerated through EF Core tooling.

Initialization creates an idle version-zero row. Acquisition is a single conditional database UPDATE requiring the expected version and no owner/token. It sets a new random token, owner, actor/time and incremented version. Release requires the exact key, version, owner and token and increments the version again. Affected-row counts must be zero or one; an unconfirmed count fails explicitly. Missing rows and stale attempts do not acquire ownership. Concurrent initialization can accept an already-created row without resetting it. Other provider errors propagate.

There is no expiry or forced takeover. Disposing the context/process does not free ownership. A future caller must retain the returned handle, use a canonical key including the module binding and obligation identity, and release only after all protected work has completed. This prevents a time-based replacement owner from overlapping an original operation that may still be running.

## Validation

Four tests pass, including a real file-backed SQLite test with independent contexts/connections racing to acquire the same version. Exactly one succeeds. Tests also reject wrong-owner, wrong-token and stale-version releases; reject stale acquisition after release/reacquisition; preserve ownership across context disposal; and reject missing records and invalid versions.

Full API suite: **954 passed, 10 skipped, zero failed** (964 total). Evidence: Beep.OilandGas.ApiService.Tests/TestResults/financial-operation-claims-full.trx. Web build: **zero errors, 310 warnings**. Logs: `%TEMP%/beep-financial-claims-full.log`, `%TEMP%/beep-financial-claims-web.log`. `git diff --check` passed.

Generated FinancialOperationClaims migrations for SqlServerRepositoryDbContext, PostgreSqlRepositoryDbContext and OracleRepositoryDbContext using dotnet-ef 10.0.9. `migrations has-pending-model-changes --no-build` reports no pending model changes for all three contexts. Migrations were scaffolded and inspected; they were not applied to live databases. SQLite validates claim semantics but does not certify other providers' affected-row behavior.

## Integration gate / next work

The store is an implemented, tested prerequisite; it is not yet registered in or called by royalty posting. Existing reservation protections remain the active posting mechanism. No recovery endpoint or automatic takeover was added.

Before wiring financial mutations, choose and prove the transaction/fencing boundary between this repository and the separately bound module database. A repository claim alone cannot prevent an old writer from updating another database after a forced takeover. Recovery must not be enabled merely because acquisition is atomic. Add a shared adapter used by accrual, payment and recovery, enforce ownership at the financial write boundary, then test crash points and stale writers against deployed providers. Add approved, audited recovery transitions with period/segregation-of-duties enforcement; never delete reservations to retry. The current actor/time fields describe the latest claim transition, not a complete financial recovery audit trail.

Repository readiness will report MigrationRequired until the generated migration is applied through the existing setup process. No database was modified in this slice. FG-41 remains in progress.
