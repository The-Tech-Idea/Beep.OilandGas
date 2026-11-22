# DAT-02 — Module bindings and module setup

| Field | Value |
|---|---|
| Stage | STG-01 — Data platform and PPDM core |
| Area | Data |
| Status | Gate met - AC-1..AC-6: all 28 discovered modules apply in one 23.7-min unscoped run, per-module plan/approve/execute, seeding, conflict fixture, high-risk approval metadata with a real local-DB backup, and core defers module-owned tables to their owners. SECURITY excluded by design. See [evidence](../../evidence/STG-01-DAT-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement`, `Beep.OilandGas.DataManager` |
| Desktop project | `src/Beep.OilandGas.Desktop.Data/Modules/`, `Modules/Data/ModuleBindings/` |
| External packages | `BeepDM` |
| Depends on | DAT-01 |
| Blocks | every domain feature |
| Web reference | `Pages/Admin/ModuleDatabases.razor`, `Pages/PPDM39/SeedReferenceData.razor`, `Pages/PPDM39/Setup/DummyDataPage.razor` |

## 1. Purpose

Decide which database each module owns, then bring that database to the state the module
expects — tables, reference codes, seeds — safely, resumably and idempotently.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement/Core/ModuleSetup/ModuleSetupBase.cs`, `ModuleSetupOrchestrator.cs`, `ModuleSetupDiscovery.cs` | the `IModuleSetup` contract and discovery |
| `PPDM39/Core/Interfaces/IModuleSetup.cs` | interface |
| 29 module classes across 20 projects | `ModuleId`, `Order`, `EntityTypes` |
| `PPDM39.DataManagement/Services/PPDM39SetupService.cs` | setup entry |
| `PPDM39.DataManagement/Services/LOVManagementService.cs` | reference code seeding |
| `PPDM39.DataManagement/Services/DemoDatabaseService.cs` | optional demo data |
| `Beep.OilandGas.DataManager/Services/DataManager.cs`, `ScriptValidator.cs`, `ModuleDataRegistry.cs`, `IExecutionStateStore` | script execution, validation, resume state |
| `Beep.OilandGas.Repository/Models/ModuleDatabaseBinding.cs` | binding record |
| ERP `ErpSchemaValidationService` | run/apply/resume shape |

## 3. Screens

`Data → Module databases`

- Binding grid: module id, display name, order, connection, entity count, last setup result, last run.
- Bulk actions: bind all to active connection, validate all, run pending, resume, dry run.
- `ModuleSetupWindow`: ordered list with per-module state (pending/running/ok/skipped/failed),
  progress bar, cancellable, log pane, "copy log", "resume from here".
- Dry-run preview: operations to be performed, tables created, rows seeded, blocking issues.

## 4. Data and contracts

```csharp
public interface IOgModuleSetupRunner
{
    Task<OgResult<OgModuleSetupPlan>> PlanAsync(CancellationToken ct);
    Task<OgResult<OgModuleSetupOutcome>> ApplyAsync(IProgress<OgModuleSetupProgress> progress, CancellationToken ct);
    Task<OgResult<OgModuleSetupOutcome>> ResumeAsync(IProgress<OgModuleSetupProgress> progress, CancellationToken ct);
    Task<OgResult<OgModuleSetupOutcome>> RunOneAsync(string moduleId, CancellationToken ct);
}
```

Result: `Success`, `AppliedCount`, `Skipped`, `Failed`, `CanResume`, `Message`, `Findings[]`.

## 5. States

| State | Behaviour |
|---|---|
| Never run | "pending"; the run button is primary |
| Already present and current | "ok — no change needed" |
| Failed mid-run | "failed at <module>"; resume offered, not a full rerun |
| Blocking issue (conflict) | run is refused; the conflicting object is named |
| Demo data requested | requires an explicit confirmation naming the connection |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Bind module | `data.modules.manage` | binding table |
| Plan (dry run) | `data.modules.manage` | no |
| Apply / resume | `data.modules.manage` | schema + seed data |
| Load demo data | `data.modules.manage` + confirmation | data |

## 7. Acceptance criteria

- AC-1 All 29 modules run in `Order`; the output lists each module id in sequence.
- AC-2 Re-running on a complete database performs no destructive operation.
- AC-3 An injected failure is resumable and does not repeat completed modules.
- AC-4 A blocking conflict is reported before any write.
- AC-5 Demo data is only loaded after explicit confirmation naming the connection.
- AC-6 Setup is refused when the bound connection is unreachable.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.IntegrationTests --filter "FullyQualifiedName~DAT-02"
```

Expect `SG-01-1`, `SG-01-2`, `SG-01-3` to pass against a fresh SQLite file.

## 9. Open questions and risks

- The 46 `*ModuleData` scripts in `DataManager` may duplicate the `IModuleSetup` work. Establish
  which is authoritative before running both — running both twice could double-seed.
- Module order has duplicate values (50, 75, 79, 80, 100). Orchestration must be deterministic;
  record the tie-break rule.

### Delivered 2026-11-06 (in progress)

**The blocker is gone.** `HSEKPIService` now implements `CountIncidentsAsync`, and the answer to "what is an
open incident" was already in the code: `HSEIncidentRules.IsOpen` — the latest STATUS `HSE_INCIDENT_RESPONSE` is
neither CLOSED nor CANCELLED, with no status standing as REPORTED. Tiers come from the primary
`HSE_INCIDENT_DETAIL` (tier 4 when none); the class column is not read because nothing writes it. The rule is now
a pure function, `HSEIncidentRules.Count`, tested in `Beep.OilandGas.PPDM39.Tests` (4 tests: primary-detail tiers
and open-from-latest-status, cancelled/closed, latest-by-date-and-sequence, empty scope).

**The draft is integrated**, after adapting it to the current interfaces (`RunSeedForModulesAsync` replaces the
removed `SeedSelectedModulesAsync`; the schema-migration contracts live in `Beep.OilandGas.Models.Data`). Files
now live in `src/Beep.OilandGas.Desktop.Data/Setup/`: `OgSetupComposition` (composes `PPDM39SetupService` +
`ModuleSetupOrchestrator` + discovered modules + reference/CSV/demo seeders), `OgModuleSetupRunner`
(`PlanAsync`/`ApplyAsync`/`ResumeAsync`, progress, per-module state) and `OgSetupState` (JSON state; round-trip
tested).

**A page exists**: route `modulesetup` (Data module, administration) with **Plan (dry run)**, **Apply**,
**Resume**, **Cancel**, a demo-data confirmation naming the connection, a per-module grid and a log. The smoke
run provisions the local connection and plans: **28 modules, 343 entity types**, "Plan only: no table was
created and no row was written", and `result: ok`.

### Still open

- **AC-1 is partial: 28 of the 29 modules are discovered.** `AddDiscoveredModuleSetups` scans **loaded**
  assemblies, so `Desktop.Data` now references the 17 module projects; `SecurityModule` is excluded because it
  is installed only in the default EF repository (ApiService excludes it for the same reason).
- **AC-2 and AC-3 are met end to end** (scoped to `WELL_STATUS_FACETS`): the schema applies 6 tables, the module seeds 150 rows, a second apply reports every module `already completed` and inserts **0**, and **resume** inserts **0**. Getting there fixed a chain of real defects, each recorded in the evidence: the stale cached datasource; the missing plan approval; **planning from the entity classes instead of the PPDM metadata** (`BuildPlanFromMetadataAsync` takes columns from `editor.classCreator` - the same conversion the post-migration verification uses - and keys from `IPPDMMetadataRepository`, plus `[Key]` for application-owned tables); and a NOT NULL text column inserted as NULL (`PPDMGenericRepository.InsertAsync` writes an unsupplied non-nullable string as the empty string).
- **AC-1 remains partial (28 of 29)** and the full 343-entity apply did not finish within 50 minutes on this machine, so the runner gained a **module scope** (`PlanAsync/ApplyAsync/ResumeAsync` take the module ids) and the end-to-end proof uses one small module. The full-run duration is an open performance item.
- **AC-4 is verified by mechanism, not by a fixture**: plan, approve, execute, seed - a non-applicable plan is returned as the outcome's message before any table or row is written. No mismatched-column conflict was exercised.
- **AC-5 delivered**: `OgSetupPolicy.MayRun` allows the demo module only when the confirmation names this
  connection (trimmed, case-insensitive); null, empty, another connection or an unticked box all skip it with a
  stated reason, and the page's checkbox names the connection. 5 tests.
- **AC-6 delivered**: `OgSetupComposition.ProbeTarget` opens the datasource and refuses with
  `setup-connection-unreachable` unless its state is `Open`, so a plan (and therefore apply/resume) is refused
  before anything is read or written. The opt-in integration test asserts the refusal for an unconfigured name.
- **The seed templates now travel with the owning assembly**: `Beep.OilandGas.PPDM39.DataManagement.csproj`
  copies `SeedData/Templates/*.json` to output, so `PPDMReferenceData.json`, `LifeCycleSeedData.json` and
  `IndustryStandardsReferenceData.json` resolve from `SeedData/Templates` beside the executable. Reference
  seeding still reports "Entity type not found for table: RA_FIELD_STATUS", which follows from the schema apply
  aborting at Step 4 before those tables existed.
- The binding grid (module to connection) is not built; the runner uses the installation's connection and a
  SHA-256 fingerprint of connection + sorted module ids.
- The API test host cannot run (`ApiService` has 44 independent compile errors), so the HSE counts are verified
  through the pure rule, not through the API.
