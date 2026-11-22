# DAT-02 - UNBLOCKED 2026-11-06: integrated into src/Beep.OilandGas.Desktop.Data/Setup

**Date:** 2026-11-05
**Status:** Blocked on a defect in `Beep.OilandGas.PPDM39.DataManagement` (not in the desktop).

## The defect

```
Beep.OilandGas.PPDM39.DataManagement\Services\HSE\HSEKPIService.cs(16,30): error CS0535:
'HSEKPIService' does not implement interface member
'IHSEKPIService.CountIncidentsAsync(string?, DateTime?, DateTime?)'
```

Reproduce, independently of the desktop:

```powershell
cd H:\Source\Repos\The-Tech-Idea\Beep.OilandGas
dotnet build Beep.OilandGas.PPDM39.DataManagement\Beep.OilandGas.PPDM39.DataManagement.csproj -c Debug
```

The project fails on its own, so this is a pre-existing breakage in the working tree, not caused by
the desktop work.

## Why it blocks DAT-02

DAT-02 must run the 29 `IModuleSetup` registrations and `PPDM39SetupService`. Both live in that
assembly, so the desktop cannot reference it until it builds.

## Why the fix is not mine to guess

- `IHSEKPIService` declares `CountIncidentsAsync(string? fieldId, DateTime? start, DateTime? end)`
  returning `IncidentCounts(Total, Open, Tier1, Tier2, Tier3, Tier4)`, documented as *"active incidents
  … how many are open and how many in each tier"*.
- The class only has a **private** `CountIncidentsAsync(string fieldId, DateRangeFilter range, int? tier)`
  returning `int`.
- `HSE_INCIDENT` (PPDM 3.9) has **no status/state column** — columns are `INCIDENT_ID`, `ACTIVE_IND`,
  `EFFECTIVE_DATE`, `EXPIRY_DATE`, `INCIDENT_CLASS_ID`, `INCIDENT_DATE`, `INCIDENT_DURATION`,
  `LOST_TIME_IND`, `REPORTED_IND`, `WORK_RELATED_IND`, audit columns. So "open" must come from the
  incident's workflow (`PROCESS_INSTANCE`) or `HSE_INCIDENT_CLOSURE`.
- HSE counts feed safety KPIs. Inventing the "open" definition would put a number on a screen that
  nobody has specified.

**The decision needed:** which record defines an incident as *open* (workflow instance state, the
closure row, or an existing helper), and whether "every field" means all `ACTIVE_IND = 'Y'` rows. With
that answer the implementation is roughly fifteen lines next to the existing private helper.

## What is already written (draft, not compiled)

`plan/pending/DAT-02/`:

| File | Purpose |
|---|---|
| `OgSetupComposition.cs` | Composes `PPDM39SetupService` + `ModuleSetupOrchestrator` + discovered `IModuleSetup` over `OgBeepHost` (mirrors `ApiService/Program.cs`) |
| `OgModuleSetupRunner.cs` | `PlanAsync` / `ApplyAsync` / `ResumeAsync`: schema plan+apply, then seeding per module with progress and per-module state |
| `OgSetupState.cs` | JSON state so a failed run resumes instead of repeating |

**Done 2026-11-06.** The HSE count rule was implemented (`HSEIncidentRules.Count` + the service method, 4 tests),
the `ProjectReference` was added, and the three files were copied into
`src/Beep.OilandGas.Desktop.Data/Setup/` and adapted to the current interfaces. The originals here are kept
only as the record of the draft; the live files are the ones under `src/`. See
`plan/evidence/STG-01-DAT-02/observed-evidence.json` for what is delivered and what is still open.

## Build state while blocked

Solution builds 0 warnings / 0 errors, 30/30 tests, `--smoke` `result: ok` — DAT-01 remains delivered
and the tree is green.
