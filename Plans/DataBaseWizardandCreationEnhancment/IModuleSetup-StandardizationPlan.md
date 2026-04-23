# IModuleSetup — Database Creation & Seeding Standardization Plan

## Purpose

Define a plugin-style `IModuleSetup` contract so every domain module can declare:
1. The PPDM entity types it owns (used by the migration manager for schema DDL)
2. The seed data it requires at startup

Any new module can be added by implementing `IModuleSetup` and registering it in DI — no central list to update, no SQL scripts.

---

## Current State Analysis

### What Exists

| Class | Location | Responsibility | Problem |
|---|---|---|---|
| `IPPDMSeeder` | `PPDM39.DataManagement/SeedData` | Minimal — CSV path only | No module identity, no entity list, no error model |
| `PPDMReferenceDataSeeder` | Same | Seeds R_*, RA_*, LIST_OF_VALUE via LOVManagementService | Standalone, no lifecycle hook |
| `EnumReferenceDataSeeder` | Same | Seeds enum-backed R_* tables via SeedEnumAsync<T> | Standalone, all enums in one class regardless of module |
| `WellStatusFacetSeeder` | Same | Seeds R_WELL_STATUS_* facets; returns `FacetSeedResult` | Standalone; good pattern to promote |
| `PPDMDemoDataSeeder` | Same | Seeds demo/test data | Delegates to other seeders; no module boundary |
| `DefaultSecuritySeedService` | `UserManagement/Services` | Seeds BA, USER, ROLE, PERMISSION | Isolated from PPDM seeders; no shared contract |
| `PPDM39SetupService` | `PPDM39.DataManagement/Services` | Schema migration via MigrationPlanArtifact | Discovers entity types ad hoc; no module registry |

### Key Gaps

1. **No module boundary** — `EnumReferenceDataSeeder.SeedAllEnumsAsync` seeds enums from 5+ domains in a single method. Adding a new domain means editing this class.
2. **No entity type registry per module** — `PPDM39SetupService` discovers entity types from the entire `Beep.OilandGas.PPDM39.Models` namespace. There is no way to know which types belong to which domain or which are required for a specific feature.
3. **No shared resilience model** — Errors in one seeder can abort others. Each class has its own result type (`FacetSeedResult`, `SeedDataResult`, `DefaultSecuritySeedResult`, `int`, ...).
4. **No ordering guarantee** — If Seeder A has a FK dependency on Seeder B, there is no explicit mechanism to enforce ordering.
5. **No DI plug-in point** — Adding a new module requires editing `PPDM39SetupService`, `PPDM39SetupController`, and the manual seeder invocations.

---

## Proposed Architecture

### Core Contracts

#### `IModuleSetup`

Defined in `Beep.OilandGas.PPDM39/Core/Interfaces/IModuleSetup.cs`

```
Purpose:
  - Declare the entity types this module owns
  - Provide the ordered seed data for this module
  - Report a stable identity and execution order

Properties:
  string ModuleId        — stable, unique identifier (e.g. "CORE_REFERENCES", "WELL_STATUS_FACETS")
  string ModuleName      — human-readable label
  int    Order           — execution order across modules; lower = earlier (FK dependencies first)
  IReadOnlyList<Type> EntityTypes — PPDM entity classes this module adds to the schema

Method:
  Task<ModuleSetupResult> SeedAsync(string connectionName, string userId, CancellationToken ct)
```

#### `ModuleSetupResult`

```
Properties:
  bool   Success
  string ModuleId
  string ModuleName
  int    RecordsInserted
  int    TablesSeeded
  List<string> Errors     — per-table error messages; non-empty does not block overall Success
                            (partial-success model: insert as many rows as possible)
  string? SkipReason      — non-null when module was intentionally skipped
```

#### `ModuleSetupContext`

Passed to seeders; carries the 4-dependency injection payload so seeders do not take constructor arguments individually:

```
IDMEEditor                    Editor
ICommonColumnHandler          CommonColumnHandler
IPPDM39DefaultsRepository     Defaults
IPPDMMetadataRepository       Metadata
string                        ConnectionName
ILogger                       Logger
```

---

### Orchestrator

`ModuleSetupOrchestrator` in `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSetupOrchestrator.cs`

**Responsibilities:**
- Receive `IEnumerable<IModuleSetup>` via DI
- Sort modules by `Order` ascending
- For schema: collect `EntityTypes` from all modules and pass to migration manager
- For seeding: call `SeedAsync` on each module in order, continuing even if one module fails (error isolation)
- Return aggregate `OrchestratorSetupResult`

**Resilience rules:**
- Each module runs inside its own `try/catch`
- A thrown exception becomes a `ModuleSetupResult { Success=false, Errors=[ex.Message] }`
- The orchestrator logs the failure and moves to the next module
- Only abort-all if a module explicitly throws `ModuleSetupAbortException` (reserved for unrecoverable states like lost DB connection)

**Schema flow:**
```
1. Collect EntityTypes = modules.SelectMany(m => m.EntityTypes).Distinct()
2. Open datasource for connectionName
3. Build MigrationPlanArtifact from EntityTypes
4. Evaluate policy (dry-run for SQLite; require approval for server DBs)
5. Execute migration plan
6. Return schema creation summary
```

**Seed flow:**
```
foreach module in modules.OrderBy(m => m.Order):
    try:
        result = await module.SeedAsync(connectionName, userId, ct)
        aggregate.ModuleResults.Add(result)
    catch ModuleSetupAbortException ex:
        throw   // propagate up
    catch Exception ex:
        aggregate.ModuleResults.Add(new ModuleSetupResult { Success=false, Errors=[ex.Message] })
        continue
```

---

### Base Class

`ModuleSetupBase` in `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSetupBase.cs`

Abstract base. Implementors override:
- `ModuleId`, `ModuleName`, `Order`, `EntityTypes`
- `SeedAsync`

Base provides helpers:
- `CreateRepo(Type entityType, string tableName)` → `PPDMGenericRepository` wired with the context
- `GetRepo<T>(string tableName)` → typed convenience wrapper calling `CreateRepo(typeof(T), tableName)`
- `SkipIfExistsAsync<T>(PPDMGenericRepository repo, AppFilter idFilter)` → bool; returns true if at least one matching row exists (idempotency helper)
- `TryInsertAsync(PPDMGenericRepository repo, object entity, string userId, ref ModuleSetupResult result)` → inserts with try/catch; on failure appends to `result.Errors`

---

## Module Catalog (Initial Set)

| Order | ModuleId | Module Class | Tables Owned | Seeder Action |
|---|---|---|---|---|
| 0 | `PPDM_CORE` | `CorePpdmModule` | `PPDM_DEFAULT_VALUE`, `LIST_OF_VALUE`, `PPDM_GUID_TABLE` | LOV defaults |
| 10 | `R_SHARED_REFERENCES` | `SharedReferenceModule` | `R_FLUID_TYPE`, `R_SEVERITY`, `R_UNIT_OF_MEASURE`, `R_QC_STATUS`, `R_COORD_SYSTEM`, `R_COUNTRY`, ... | Enum + reference seed |
| 20 | `WELL_STATUS_FACETS` | `WellStatusFacetModule` | `R_WELL_STATUS_TYPE`, `R_WELL_STATUS`, `R_WELL_STATUS_QUAL`, `R_WELL_STATUS_QUAL_VALUE`, `RA_WELL_STATUS_TYPE`, `RA_WELL_STATUS` | Promote `WellStatusFacetSeeder.SeedAllAsync` |
| 30 | `WELL_REFERENCES` | `WellReferenceModule` | `R_WELL_CLASS`, `R_WELL_PROFILE_TYPE`, `R_WELL_XREF_TYPE`, `R_WELL_STATUS` | Enum seed for well-specific R_* |
| 40 | `SECURITY` | `SecurityModule` | `BUSINESS_ASSOCIATE`, `BA_ORGANIZATION`, `USER`, `ROLE`, `PERMISSION`, `USER_ROLE`, `ROLE_PERMISSION` | Promote `DefaultSecuritySeedService.SeedDefaultsAsync` |
| 50 | `EXPLORATION` | `ExplorationModule` | `PROSPECT`, `PLAY`, `PROSPECT_WELL_XREF`, `SEISMIC_SURVEY`, ... | Exploration reference + LOV seed |
| 60 | `DEVELOPMENT` | `DevelopmentModule` | `WELL`, `WELL_STATUS`, `WELL_XREF`, `WELL_ACTIVITY`, ... | Well reference defaults |
| 70 | `PRODUCTION` | `ProductionModule` | `PDEN`, `PDEN_VOL_SUMMARY`, `POOL`, `WELL_TEST`, ... | Production reference seed |
| 80 | `HSE` | `HseModule` | HSE entity types | HSE reference seed |
| 90 | `ECONOMICS` | `EconomicsModule` | `CONTRACT`, financial entity types | Economics reference seed |
| 100 | `DEMO_DATA` | `DemoDataModule` | (none — uses existing tables) | Promote `PPDMDemoDataSeeder` |

> Modules at order 0–40 are **core** — always required. Modules at 50+ are **domain** — optional, discoverable via DI.

---

## File Layout

```
Beep.OilandGas.PPDM39/
└── Core/
    └── Interfaces/
        └── IModuleSetup.cs              ← interface + ModuleSetupResult + ModuleSetupContext

Beep.OilandGas.PPDM39.DataManagement/
└── Core/
    └── ModuleSetup/
        ├── ModuleSetupBase.cs           ← abstract base with PPDMGenericRepository helpers
        ├── ModuleSetupOrchestrator.cs   ← DI-collected, sorted, resilient orchestrator
        └── ModuleSetupAbortException.cs ← reserved for unrecoverable abort

└── Modules/
    ├── CorePpdmModule.cs
    ├── SharedReferenceModule.cs         ← absorbs EnumReferenceDataSeeder (shared enums)
    ├── WellStatusFacetModule.cs         ← wraps WellStatusFacetSeeder
    ├── WellReferenceModule.cs           ← well-specific enum seed
    ├── ExplorationModule.cs
    ├── DevelopmentModule.cs
    ├── ProductionModule.cs
    ├── HseModule.cs
    ├── EconomicsModule.cs
    └── DemoDataModule.cs

Beep.OilandGas.UserManagement/
└── Modules/
    └── SecurityModule.cs                ← wraps DefaultSecuritySeedService
```

---

## Implementation Status (Current)

### Completed

- Added `IModuleSetup`, `ModuleSetupResult`, `OrchestratorSetupResult`, and `ModuleSetupContext`.
- Added `ModuleSetupBase`, `ModuleSetupOrchestrator`, and `ModuleSetupAbortException`.
- Added modules:
  - `CorePpdmModule`
  - `SharedReferenceModule`
  - `WellStatusFacetModule`
  - `WellReferenceModule`
  - `SecurityModule` (in UserManagement project)
  - `ExplorationModule`
  - `DevelopmentModule`
  - `ProductionModule`
  - `HseModule`
  - `EconomicsModule`
  - `DemoDataModule`
- Wired module registrations into API DI container.
- Wired `PPDM39SetupService` to:
  - use orchestrator entity manifest for migration plan type collection (with legacy fallback),
  - use orchestrator execution path for `SeedAllReferenceDataAsync` (with legacy fallback).
- **NEW: Implemented selective module seeding API endpoints:**
  - Added `ModuleSeedingRequest`, `ModuleSeedingResponse`, `ModuleExecutionDetail`, `AvailableModulesResponse`, `ModuleInfo` DTOs
  - Added `ModuleSetupOrchestrator.RunSeedForModulesAsync(ids)` and `GetModuleMetadata()` methods
  - Added `PPDM39SetupService.SeedSelectedModulesAsync()` and `GetAvailableModules()` methods
  - Added `/api/ppdm39/setup/available-modules` GET endpoint (list all modules)
  - Added `/api/ppdm39/setup/seed/selected-modules` POST endpoint (seed selected modules only)
  - Validates module Order is respected regardless of request list order
  - Supports partial success workflows (207 Multi-Status for mixed results)
  - See `SelectiveModuleSeeding-Endpoints.md` for detailed endpoint documentation

### Pending

- Expand domain module seed implementations from placeholders to full idempotent table-level seed catalogs (Exploration, Development, Production, HSE, Economics).
- Add integration tests that validate module ordering and resilience behavior (`continue-on-error`, abort semantics).
- Optional: Async progress polling endpoint `/api/ppdm39/setup/operation/{operationId}` for long-running selective seeds.

---

## Migration Manager Integration

The orchestrator collects `EntityTypes` from all registered modules and passes them to the migration pipeline.

```
var allTypes = modules.SelectMany(m => m.EntityTypes).Distinct().ToList();

// Existing PPDM39SetupService API
var plan = await _schemaMigration.BuildPlanAsync(connectionName, allTypes);
await _schemaMigration.ApproveAndExecuteAsync(plan, userId);
```

Key rules:
- **Only IModuleSetup owns the entity type registry** — `PPDM39SetupService` no longer discovers types from the full `Beep.OilandGas.PPDM39.Models` namespace.
- **Incremental modules** — If a new module is registered, `BuildPlanAsync` only adds new tables; existing tables are left unchanged by the migration manager (additive only for production).
- **No SQL scripts** — DDL comes entirely from `MigrationPlanArtifact` driven by the entity type list.

---

## Seeding Pattern — PPDMGenericRepository

All seed inserts go through `PPDMGenericRepository`. No raw SQL, no CSV files for reference data.

Pattern inside a module's `SeedAsync`:

```csharp
var repo = GetRepo<R_WELL_STATUS>("R_WELL_STATUS");
foreach (var row in _catalog)
{
    var idFilter = new AppFilter { FieldName = "STATUS_TYPE", Operator = "=", FilterValue = row.STATUS_TYPE };
    if (await SkipIfExistsAsync<R_WELL_STATUS>(repo, idFilter)) continue;
    await TryInsertAsync(repo, row, userId, ref result);
}
```

`TryInsertAsync` wraps each row in `try/catch`:
- On success: `result.RecordsInserted++`
- On failure: `result.Errors.Add($"[R_WELL_STATUS/{row.STATUS_TYPE}] {ex.Message}")`

This means a table with 100 rows will insert as many as possible even if 3 rows fail (partial success).

---

## Error Resilience Model

### Levels

| Level | Behavior |
|---|---|
| Row-level | `TryInsertAsync` catches exception, appends to `Errors`, continues next row |
| Table-level | Optional — module may loop tables and continue to next table on failure |
| Module-level | Orchestrator `try/catch` wraps entire `SeedAsync`; failure becomes `ModuleSetupResult.Success=false`; orchestrator continues to next module |
| Orchestrator-level | Only stops if `ModuleSetupAbortException` is thrown (e.g. DB connection is gone) |

### Partial Success

`ModuleSetupResult.Success` is `true` even if `Errors.Count > 0` as long as the module completed its loop. The distinction:
- `Success=true, Errors.Count > 0` → partial insert (some rows skipped due to constraint violations, duplicates, etc.)
- `Success=false` → module threw an unhandled exception and did not complete

This allows downstream reporting to show "Wells module seeded 340/342 rows (2 errors)" without blocking the security module from running.

---

## Registration Pattern

```csharp
// In Program.cs — after IDMEEditor, metadata, defaults are registered

// Core modules (always present)
builder.Services.AddScoped<IModuleSetup>(sp => new CorePpdmModule(BuildContext(sp, conn)));
builder.Services.AddScoped<IModuleSetup>(sp => new SharedReferenceModule(BuildContext(sp, conn)));
builder.Services.AddScoped<IModuleSetup>(sp => new WellStatusFacetModule(BuildContext(sp, conn)));
builder.Services.AddScoped<IModuleSetup>(sp => new WellReferenceModule(BuildContext(sp, conn)));
builder.Services.AddScoped<IModuleSetup>(sp => new SecurityModule(BuildContext(sp, conn)));

// Domain modules (opt-in per deployment)
builder.Services.AddScoped<IModuleSetup>(sp => new ExplorationModule(BuildContext(sp, conn)));
builder.Services.AddScoped<IModuleSetup>(sp => new DevelopmentModule(BuildContext(sp, conn)));
builder.Services.AddScoped<IModuleSetup>(sp => new ProductionModule(BuildContext(sp, conn)));
builder.Services.AddScoped<IModuleSetup>(sp => new HseModule(BuildContext(sp, conn)));

// Orchestrator receives all IModuleSetup registrations
builder.Services.AddScoped<ModuleSetupOrchestrator>();

static ModuleSetupContext BuildContext(IServiceProvider sp, string conn) => new()
{
    Editor              = sp.GetRequiredService<IDMEEditor>(),
    CommonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>(),
    Defaults            = sp.GetRequiredService<IPPDM39DefaultsRepository>(),
    Metadata            = sp.GetRequiredService<IPPDMMetadataRepository>(),
    ConnectionName      = conn,
    Logger              = sp.GetRequiredService<ILoggerFactory>().CreateLogger("ModuleSetup")
};
```

`ModuleSetupOrchestrator` is then injected into `PPDM39SetupService` or `PPDM39SetupController` replacing the current ad hoc seeder calls.

---

## Integration with PPDM39SetupService / Controller

### Current flow (to be replaced)

```
PPDM39SetupController.CreateDatabaseAsync
  → PPDM39SetupService.CreateSchemaFromMigrationAsync  // schema
  → EnumReferenceDataSeeder.SeedAllEnumsAsync           // seed
  → WellStatusFacetSeeder.SeedAllAsync                  // seed
  → DefaultSecuritySeedService.SeedDefaultsAsync        // seed
```

### New flow

```
PPDM39SetupController.CreateDatabaseAsync
  → ModuleSetupOrchestrator.RunSchemaAsync(connectionName, userId, ct)
      ← gathers EntityTypes from all IModuleSetup
      ← calls PPDM39SetupService.BuildAndExecuteMigrationAsync(entityTypes)
  → ModuleSetupOrchestrator.RunSeedAsync(connectionName, userId, ct)
      ← iterates modules ordered by .Order
      ← calls module.SeedAsync per module
      ← collects OrchestratorSetupResult
```

The controller summary response becomes:

```json
{
  "schemaSuccess": true,
  "modulesRun": 5,
  "modulesSucceeded": 5,
  "totalRecordsInserted": 847,
  "moduleResults": [
    { "moduleId": "PPDM_CORE", "success": true, "recordsInserted": 12 },
    { "moduleId": "WELL_STATUS_FACETS", "success": true, "recordsInserted": 340 },
    { "moduleId": "SECURITY", "success": true, "recordsInserted": 15 }
  ]
}
```

---

## Implementation Sequence

The implementation should follow this order to maintain a buildable state at every step.

### Step 1 — Contracts (no behavioural change)
- Create `IModuleSetup.cs` in `Beep.OilandGas.PPDM39/Core/Interfaces/`
- Create `ModuleSetupResult.cs` and `ModuleSetupContext.cs` as plain DTOs
- Create `OrchestratorSetupResult.cs`
- Build green.

### Step 2 — Base class and orchestrator
- Create `ModuleSetupBase.cs` (abstract; helpers only — no modules yet)
- Create `ModuleSetupOrchestrator.cs` (orchestrator with empty module list still works)
- Create `ModuleSetupAbortException.cs`
- Build green.

### Step 3 — Refactor existing seeders as reference implementations
- `WellStatusFacetModule` wrapping `WellStatusFacetSeeder`
- `SharedReferenceModule` splitting out cross-domain enums from `EnumReferenceDataSeeder`
- `SecurityModule` wrapping `DefaultSecuritySeedService`
- `CorePpdmModule` for LOV defaults
- **Do not delete** old seeders yet — keep them intact and delegate to them from the new modules
- Build green, run existing API tests.

### Step 4 — Wire orchestrator into PPDM39SetupService
- Inject `ModuleSetupOrchestrator` into `PPDM39SetupService`
- Replace manual seeder calls in `SeedDefaultsAsync` / `CreateDatabaseAsync` with `orchestrator.RunSeedAsync`
- Update `PPDM39SetupController` summary to use `OrchestratorSetupResult`
- Build green, run all API tests.

### Step 5 — Schema migration via orchestrator
- `ModuleSetupOrchestrator.RunSchemaAsync` collects entity types from all modules
- Pass to existing `MigrationPlanArtifact` pipeline in `PPDM39SetupService`
- Remove namespace-discovery-based entity type collection
- Build green, full solution build + tests.

### Step 6 — Domain modules
- Add `ExplorationModule`, `DevelopmentModule`, `ProductionModule`, `HseModule` as empty stubs with entity types declared
- Each stub seeds only its R_* reference tables
- Register optional domain modules in `Program.cs`
- Build green.

### Step 7 — Cleanup
- Remove dead seeder call sites replaced by modules
- Retire `IPPDMSeeder` (CSV-path only) after confirming no live usage
- Update `MASTER-TODO-TRACKER.md` with completion checkpoints

---

## Constraints

- **No SQL scripts** — All DDL via MigrationPlanArtifact. All DML via PPDMGenericRepository.
- **No CSV seeding for reference data** — `PPDMCSVSeeder` remains for user-data import only; module seed data is code-embedded (like `WellStatusFacetSeeder.FACET_CATALOG`).
- **Additive only for production** — Module entity type lists must be additive; removing a type from a module should not trigger a drop-table migration.
- **Idempotent** — Every module's `SeedAsync` must be safe to run multiple times (skip-if-exists for all rows).
- **Order stability** — Module `Order` values are fixed constants; do not use dynamic ordering.

---

## Files to Create

| File | Action |
|---|---|
| `Beep.OilandGas.PPDM39/Core/Interfaces/IModuleSetup.cs` | Create |
| `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSetupBase.cs` | Create |
| `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSetupOrchestrator.cs` | Create |
| `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSetupAbortException.cs` | Create |
| `Beep.OilandGas.PPDM39.DataManagement/Modules/CorePpdmModule.cs` | Create |
| `Beep.OilandGas.PPDM39.DataManagement/Modules/SharedReferenceModule.cs` | Create |
| `Beep.OilandGas.PPDM39.DataManagement/Modules/WellStatusFacetModule.cs` | Create |
| `Beep.OilandGas.PPDM39.DataManagement/Modules/WellReferenceModule.cs` | Create |
| `Beep.OilandGas.PPDM39.DataManagement/Modules/SecurityModule.cs` | Create |
| `Beep.OilandGas.PPDM39.DataManagement/Modules/ExplorationModule.cs` | Create (stub) |
| `Beep.OilandGas.PPDM39.DataManagement/Modules/DevelopmentModule.cs` | Create (stub) |
| `Beep.OilandGas.PPDM39.DataManagement/Modules/ProductionModule.cs` | Create (stub) |
| `Beep.OilandGas.PPDM39.DataManagement/Modules/HseModule.cs` | Create (stub) |
| `Beep.OilandGas.PPDM39.DataManagement/Modules/EconomicsModule.cs` | Create (stub) |
| `Beep.OilandGas.PPDM39.DataManagement/Modules/DemoDataModule.cs` | Create |

## Files to Modify

| File | Change |
|---|---|
| `Beep.OilandGas.PPDM39.DataManagement/Services/PPDM39SetupService.cs` | Inject `ModuleSetupOrchestrator`; replace manual seeder calls |
| `Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39SetupController.cs` | Use `OrchestratorSetupResult` in summary responses |
| `Beep.OilandGas.ApiService/Program.cs` | Register all modules and orchestrator |
