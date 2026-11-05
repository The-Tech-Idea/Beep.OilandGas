# Implementation 53 — Lease transaction and readback

Date: 2026-10-05. Owners: CA-03/05/06; FG-01/10/11/12/20. Status: implemented command orchestration with focused transaction-fixture evidence. Installed-provider conformance, durable retries/history concurrency and full lease experience remain open. No phase closure.

## Existing owner and command behavior

Implementation 52 established exact company/FIELD authority and complete LAND identities but its writes were sequential and returned without persisted readback. Extend the same canonical LeaseAcquisitionService, shared company lease gate and PPDMGenericRepository. No alternate lease store, SQL command or EF business persistence is introduced. Business/module schema ownership remains Beep MigrationManager; no schema or live migration is required for this change.

The [shared company lease contract](../../TheTechIdea.Data/OilGas/CompanyLeaseDataAccess.cs) adds ExecuteCommandAsync. The [existing API gate](../../Beep.OilandGas.ApiService/Services/PPDMCompanyLeaseDataAccess.cs) captures the reviewed datasource, independently checks current company Administrator/FIELD WRITE authority and executes writes plus readback in one owned Beep transaction. Before commit it rechecks authority/binding. After commit it requires another fresh readback and current authority before a command response can indicate success.

The [existing service](../../Beep.OilandGas.LeaseAcquisition/Services/LeaseAcquisitionService.ModelsCoreImpl.cs) captures immutable complete-key/business-value proofs before common-column preparation. Readback requires one canonical record per full identity, exact requested business values and the authenticated ROW_CREATED_BY or ROW_CHANGED_BY. Status rereads its company/FIELD evidence and canonical records after entering the transaction; header/agreement changes and appended history share that transaction. Creation registers all five canonical records in the same transaction. Raw readback also supports explicitly future-dated registration without pretending that its company association is currently effective.

UTC request/status dates are converted to the company's reviewed storage date zone before writing PPDM DateTime columns. This preserves the intended instant when a relational provider materializes those columns without DateTime.Kind. Unspecified input dates keep their reviewed-zone interpretation; invalid/ambiguous local dates remain refused by the existing evidence rules.

## Beep transaction owner and shared repository coordination

The [PPDM datasource transaction boundary](../../Beep.OilandGas.PPDM39/Core/PPDMDataSourceTransaction.cs) uses IDataSource.BeginTransaction, Commit and EndTransaction (rollback). It requires the driver's public ActiveTransaction and connection DbConn evidence, an open physical connection, no existing borrowed transaction and exact ownership of the same transaction/connection throughout the command. Reflection reads the existing driver's public properties because IDataSource does not declare that ownership evidence; no compatibility fallback or no-op success path is added.

The existing repository's typed SDK reads and direct CRUD/batch SDK calls share a per-datasource execution lane. Other operations on that same cached instance wait until the owned command finishes; operations inside it retain the captured transaction and serialize provider calls. Nested commands cannot borrow/commit the outer transaction. Datasource replacement or physical transaction/connection loss refuses subsequent operations. Finishing waits for outstanding SDK work and closes the command context so late inherited tasks cannot publish another write. Later repository operations cannot join a borrowed/unconfirmed transaction left by a failed rollback.

Provider failure or precommit readback/authority mismatch rolls back the owned transaction and rethrows the failure. A failed rollback reports both the command and rollback failure; it does not claim restoration. Commit failure or failed postcommit confirmation remains an explicit unconfirmed result without an automatic retry. An already released/committed transaction is never rolled back as if it were still owned.

This lane covers the existing generic repository's direct SDK paths. Remaining advanced/unscoped UnitOfWork/direct-driver paths still require the broader owner audit; these checks do not certify all datasource consumers or cross-process concurrency.

## Actual driver evidence and acceptance boundary

The sibling BeepDataSources RDBSource transaction source exposes ActiveTransaction, retains the IDbTransaction and associates commands with it; its EndTransaction rolls back. However, the actual cached package binary selected by this API differs. The inspected net10.0 RDBDataSource 2.0.21 binary has no declared public ActiveTransaction property. The inspected SQL Server 1.0.1 and PostgreSQL 1.0.2 classes also do not declare that property. Declared-property inspection alone does not prove inherited members or real provider behavior.

Reproduce the exact binary/hash evidence with [Get-BeepTransactionProviderEvidence.ps1](../../Scripts/Get-BeepTransactionProviderEvidence.ps1), passing the actual installed assembly paths. Recorded evidence is `%TEMP%/oilgas-beep-transaction-driver-evidence.json`; RDBDataSource SHA256 is `3C8EBC82078BBB90C4DC12D22B033B0B229C92D05D14D58B2CD150A54029CD60` for the inspected local binary. No package version is guessed or silently downgraded. No external checkout, package cache, provider deployment or live connection is changed.

The command refuses a provider that cannot expose/open an owned transaction before any lease write. A conformant Beep provider must be acquired/built through its existing owner and accepted with actual datasource/connection/CRUD enlistment/commit/rollback behavior. Neither the source implementation nor a recording fixture proves that the installed runtime driver supplies that behavior. This is a concrete delivery requirement, not a completed provider upgrade.

## Focused verification

The [lease suite](../../Beep.OilandGas.Repository.Tests/CompanyLeaseDataTests.cs) exercises the actual API gate, canonical service and PPDM repository/transaction boundary. The existing [recording Beep fixture](../../Beep.OilandGas.Repository.Tests/CompanyPPDMRowAccessTests.cs) now clones LAND read rows, persists inserted/updated records and implements explicit transaction evidence/undo behavior for lease cases. It retains attempted writes separately from committed records. App membership/grant/role authority still uses real focused SQLite persistence; no business SQL or substitute production datasource is introduced.

Coverage includes every creation/status write failure, full undo of earlier records, no-op/failed/borrowed transaction refusal, missing ownership evidence, business-value/actor/date/missing-row readback mismatches, rollback failure, commit failure before/after simulated commit, fresh postcommit confirmation, postcommit mismatch uncertainty, nested transactions and a normal repository read blocked by the command's datasource lane. Final cases additionally cover authority/configuration changes after the last write, datasource replacement and refusal to expose a failed rollback's uncommitted records. Server UTC status time is checked against the reviewed Kuwait date zone.

Final focused gate: **1332 passed, 0 failed, 0 skipped**, adding 30 cases to the previous 1302, in 4 minutes 5 seconds. Earlier targeted transaction/readback run passed 92 checks before the final coordination/authority cases. No Razor source changes in this implementation; Implementation 52's isolated markup and Implementation 51's static rendering evidence remain historical.

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger "console;verbosity=minimal"
git diff --check
```

Log: `%TEMP%/oilgas-lease-transaction-focused.log`. A recording transaction fixture establishes orchestration/refusal/readback behavior; real provider atomicity, isolation and FK enforcement remain unverified. Full host/DI/controller/auth/HTTP/client/browser execution is still open, and unrelated missing projects are not replaced.

## Remaining required work

Follow-up: [Implementation 54](Implementation-54-Lease-Creation-Intent-and-Reconciliation.md) implements retained-record stable-ID replay/reconciliation and the active SDK/dialog state. The remaining list below records this slice's original handoff; the current tracker preserves provider, lifetime ledger/fencing, status concurrency and integrated experience gaps.

1. Resolve actual Beep driver ownership/enlistment and real-provider transaction/rollback/commit uncertainty acceptance, including canonical LAND_AREA/parent/FK requirements and explicit area identity. FIELD_ID must never stand in for AREA_ID.
2. Add durable command identity/idempotency, expected revision/history concurrency and canonical reconciliation/readback envelopes through existing owners. Same-process datasource serialization does not protect a second server process or prevent an external writer from changing a record.
3. Align the actual lease SDK routes/local-mode branches and Web typed command/selection/role/error/readback consumers, and the server current-field endpoint assertion. An unconfirmed command must not invite an unqualified retry or report success.
4. Replace fabricated advanced prospect/title/rights/compliance/financial outputs and repair the existing economic-interest/JIB/joint-arrangement owner boundaries. Continue remaining foundation/provisioning/domain/local-mode/cache and wider CA/release work; the whole approved plan remains active.
