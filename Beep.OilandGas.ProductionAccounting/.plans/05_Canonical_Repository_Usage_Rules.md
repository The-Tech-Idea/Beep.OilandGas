# ProductionAccounting Canonical Repository Usage Rules

## Scope

- Applies to `Beep.OilandGas.ProductionAccounting` service-layer data access.
- Covers orchestrator paths (`ProductionAccountingService`) and compatibility surfaces (`ProductionAccountingService.ControllerFacade` and manager-style wrappers).
- Defines what is allowed, what is transitional, and what must be migrated.

## Phase 5.1 - Baseline Rules (immediately enforced)

### Goals

- Ensure all new data access follows one canonical repository pattern.
- Keep reference-data usage aligned with constants and module seed configuration.
- Preserve compatibility behavior without expanding technical debt.

### Canonical Rules

- Use `PPDMGenericRepository` resolved from metadata for table operations; avoid direct inline SQL for normal CRUD/query paths.
- Resolve filters through `AppFilter` and avoid hardcoded delimiter-dependent SQL parameterization.
- Use `IPPDM39DefaultsRepository` for active indicators/boolean-style defaults where touched.
- Keep audit field handling through `ICommonColumnHandler` and service-level write methods.
- Keep table classes scalar-only and projection classes separate from persisted table entities.
- Use `WellServices` for any well-domain table operations (`WELL`, `WELL_STATUS`, `WELL_XREF`) instead of ad-hoc repositories.

### TODO Checklist

- [ ] Reject new inline SELECT SQL in service code reviews unless there is a documented, non-repository exception.
- [ ] Confirm each new service method uses metadata + repository creation pattern.
- [ ] Confirm each touched `ACTIVE_IND`/boolean-style field uses `_defaults` accessors.

## Phase 5.2 - Orchestrator Rules (`ProductionAccountingService`)

### Goals

- Keep orchestrator workflows deterministic and repository-consistent.
- Prevent compatibility-only shortcuts from leaking into active orchestration paths.

### Rules

- Orchestrator write paths must call domain services or repositories asynchronously (`await`); no `.Result`/`.Wait()`.
- Cross-step workflow methods (run ticket -> allocation -> royalty -> revenue -> GL -> close) must use explicit guard checks and return structured failure context.
- Orchestrator queries that read persisted entities must use repository-backed methods with constants-backed filter values.
- New orchestrator helper methods must not depend on compatibility manager in-memory state.

### TODO Checklist

- [ ] Audit orchestrator methods for sync-over-async calls and replace with async equivalents.
- [ ] Ensure orchestrator filter values come from constants/defaults where families exist.
- [ ] Add/extend guard-path tests for async failure branches.

## Phase 5.3 - Compatibility Surface Rules (`ControllerFacade`)

### Goals

- Keep compatibility surfaces stable for staged clients.
- Make persistence-vs-fallback behavior explicit and testable.

### Rules

- Compatibility endpoints may keep shape-preserving wrappers, but must be labeled as compatibility/staged in docs.
- If a compatibility path persists data, it must use canonical repository/service methods (not ad-hoc in-memory state).
- In-memory fallback is allowed only when explicitly documented as fallback-only behavior.
- New compatibility methods should be additive wrappers around canonical services, not new isolated business logic.

### TODO Checklist

- [ ] Tag compatibility-only methods in XML docs with behavior class: `active`, `staged`, or `fallback-only`.
- [ ] Replace persistence-affecting sync wrappers with async-safe service/repository calls.
- [ ] Add at least one guard/failure test per compatibility path that persists records.

## Phase 5.4 - Verification and Exit Criteria

### Verification Checklist

- [x] `dotnet build Beep.OilandGas.ProductionAccounting\Beep.OilandGas.ProductionAccounting.csproj` succeeds with no new warnings.
- [x] `dotnet build Beep.OilandGas.ApiService\Beep.OilandGas.ApiService.csproj` succeeds after controller changes.
- [x] New/updated tests cover one happy path and one failure path per changed active API area.
- [x] API and planning docs clearly separate active endpoints from compatibility/staged endpoints.

## Phase 5.5 - Exception Handling Policy (Hardening)

### Goals

- Eliminate silent failures in active and compatibility paths.
- Keep fallback behavior explicit, intentional, and observable.
- Make each catch block fit one of a small number of approved patterns.

### Catch Categories (Required)

- **`Rethrow`**: log at error level and rethrow or wrap in a domain exception when operation integrity is at risk.
- **`Fallback`**: return safe default only with explicit warning log that includes entity/context identifiers.
- **`Optional-table skip`**: for optional schema artifacts, log warning with table/action context and continue.
- **`Compatibility staged no-op`**: log warning that behavior is staged/no-op and include identifiers for traceability.

### Rules

- No empty `catch` blocks in `Beep.OilandGas.ProductionAccounting/Services`.
- Any `catch` that returns default values must include a reason-oriented log message and the affected business key.
- Optional-table catches must clearly say operation is skipped by design, not silently ignored.
- Compatibility no-op methods must emit a warning at invocation time.

### TODO Checklist

- [x] Audit remaining `catch` blocks in `Services/*` and tag each with one category above.
- [x] Standardize log message templates per category (operation, key id, fallback/skip reason).
- [x] Add/adjust tests for representative fallback and rethrow paths in at least 3 services beyond orchestrator.

### Exit Criteria

- No new direct inline CRUD/query SQL introduced in active service paths.
- No new sync-over-async repository calls introduced in orchestrator/compatibility paths.
- All newly touched reference values are constants-backed and mapped to seed rows where applicable.

