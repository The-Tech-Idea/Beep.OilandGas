# ProductionAccounting Phase 6 - Post-5.5 Execution Plan

## Scope

- Follow-up work after Phase 5.5 completion.
- Focus on remaining open items from Phase 2/3/5 plans.
- Keep compatibility behavior explicit while continuing modernization.

## Phase 6.1 - Compatibility Contract Finalization

### Goals

- Make compatibility-only behaviors explicit and non-ambiguous.
- Eliminate any remaining implicit no-op semantics.

### TODO Checklist

- [x] Inventory `ProductionAccountingService.ControllerFacade` compatibility methods and classify each as `active`, `staged`, or `fallback-only`.
- [x] Add/update XML docs for compatibility methods missing explicit behavior class.
- [x] For `fallback-only` methods, ensure invocation-level warning logs exist with business identifiers.
- [x] Add at least one guard/failure test per compatibility path that persists records.

### Target Files

- `Beep.OilandGas.ProductionAccounting/Services/ProductionAccountingService.ControllerFacade.cs`
- `Beep.OilandGas.ApiService.Tests/*ProductionAccounting*Tests.cs`
- `.cursor/commands/api-endpoints-reference.md`

### Verification

- [x] Focused `ApiService.Tests` compatibility test subset passes.
- [x] No compatibility persistence path lacks explicit behavior labeling.

### Progress Notes

- Added focused failure-path compatibility tests in `ApiService.Tests` for persistence-affecting compatibility wrappers (`PurchaseOrder`, `AR Invoice`, `AP Invoice`, `Inventory Transaction`, `GL Account`, `Journal Entry`, `Exchange Contract`, `Royalty Payment`, `Division Order`) under metadata/repository failure conditions.
- Focused test run passes: `ProductionAccountingControllerFacadeCompatibilityTests` (9/9).

## Phase 6.2 - Repository Convergence Follow-Through

### Goals

- Maintain canonical repository usage in all touched paths.
- Validate idempotent module setup and seeded-value discoverability.

### TODO Checklist

- [x] Re-run module setup idempotency check and verify no duplicate/unsafe side effects.
- [x] Validate seeded reference values remain discoverable through active accounting services/APIs.
- [x] Reconfirm no new sync-over-async (`.Result`/`.Wait()`) usage in `ProductionAccounting/Services`.

### Target Files

- `Beep.OilandGas.ProductionAccounting/ProductionAccountingModuleSetup.cs`
- `Beep.OilandGas.ProductionAccounting/Services/*`
- `Beep.OilandGas.ProductionAccounting/.plans/05_Canonical_Repository_Usage_Rules.md`

### Verification

- [x] `dotnet build Beep.OilandGas.ProductionAccounting\Beep.OilandGas.ProductionAccounting.csproj`
- [x] `dotnet build Beep.OilandGas.ApiService\Beep.OilandGas.ApiService.csproj --no-restore`

### Progress Notes

- Confirmed module-seed idempotency mechanics in `ProductionAccountingModuleSetup`: upsert-by-key (`REFERENCE_SET` + `REFERENCE_CODE`) with insert-only-on-missing behavior.
- Added `ProductionAccountingReferenceSeedCatalogTests` to validate unique seed key pairs and required active API reference families.
- Focused seed-catalog tests pass and broader ProductionAccounting-focused test subset passes (`FullyQualifiedName~ProductionAccounting`: 30/30).
- Reduced sync-over-async usage for compatibility paths that already expose async surface (`TradingCompatibilityService`) by introducing async repository helper methods (`GetEntityByIdAsync`, `TryInsertEntityAsync`) and removing blocking `.GetAwaiter().GetResult()` from `GetContractAsync` / `RegisterContractAsync`.
- Centralized remaining sync bridges into `RunSyncCompatibility(...)` helpers, replacing scattered direct `.GetAwaiter().GetResult()` calls across compatibility managers; direct calls in `ControllerFacade` are now reduced to 2.
- Added explicit `OperationCanceledException` handling in `ProductionAccountingService` orchestration and aggregate helper paths so cancellations are logged and rethrown (instead of being swallowed by generic fallback catches).
- Extended cancellation hardening into `PeriodClosingService` by adding explicit `OperationCanceledException` passthrough + warning logs across period-closing orchestration, reconciliation helpers, close-marking steps, and optional year/quarter-end adjustment paths.
- Extended cancellation hardening into `JointInterestBillingService` by adding explicit `OperationCanceledException` passthrough + warning logs for participant allocation, statement generation, and lease validation paths.
- Extended cancellation hardening into `InventoryService` by adding explicit `OperationCanceledException` passthrough + warning logs for inventory update, retrieval, and validation paths.
- Extended cancellation hardening into `RevenueService` by adding explicit `OperationCanceledException` passthrough + warning logs for revenue recognition, allocation validation, and commodity-price lookup paths.
- Extended cancellation hardening into `ImbalanceService` by adding explicit `OperationCanceledException` passthrough + warning logs for reconciliation, outstanding-imbalance lookup, and adjustment validation paths.

## Phase 6.3 - Tracker and Exit Review

### Goals

- Close remaining open checklist items in older phases where work is complete.
- Keep `MASTER-TODO-TRACKER.md` as the authoritative phase rollup.

### TODO Checklist

- [x] Reconcile Phase 2 and Phase 3 open checklist items with current implementation state.
- [x] Mark completed items and carry forward only truly pending items.
- [x] Update `MASTER-TODO-TRACKER.md` with final post-5.5 status note.

### Verification

- [x] All open checklist items map to concrete pending engineering work.
- [x] Tracker and `.plans` are consistent on phase state.
