# PRD-04 — Allocation workbench

| Field | Value |
|---|---|
| Stage | STG-05 — Production operations and allocation |
| Area | Production |
| Status | Gate met - see [evidence](../../evidence/STG-05-PRD-04/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProductionAccounting` |
| Desktop project | `Modules/Production/Allocation/` |
| External packages | none |
| Depends on | PRD-02 |
| Blocks | PRD-05, ACC-07 |
| Web reference | `Pages/PPDM39/Production/AllocationWorkbench.razor` |

## 1. Purpose

Turn measured volumes at a facility into allocated volumes per well and pool, with the method and
the residual visible so the numbers can be defended.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProductionAccounting/Services/AllocationEngine/AllocationService.cs` | allocation run |
| `ProductionAccounting/Core/Interfaces/IAllocationService.cs` | contract |
| `ProductionAccounting/Services/MeasurementService.cs` | measured inputs |
| Tables `R_ALLOCATION_TYPE`, allocation result tables, `PDEN_VOL_SUMMARY` | persistence |

## 3. Screens

`Production → Allocation`

- Run panel: field/facility, period, allocation type, method, dry run vs commit.
- Preview grid: well, measured, allocated, factor, residual, method, status.
- Residual summary: total measured vs total allocated vs difference, with tolerance.
- Explanation panel: per-well basis (potential test, downtime, prior period, manual override).
- Commit action with retained-original semantics.
- History of runs with inputs hash.

## 4. Data and contracts

`OgAllocationRow { WellId, Measured, Allocated, Unit, Factor, Residual, Method, Override? }`
`OgAllocationRun { Id, Period, FacilityId, Method, Rows, TotalMeasured, TotalAllocated, Hash }`.

## 5. States

| State | Behaviour |
|---|---|
| No method | row is "unallocated", not zero |
| Residual beyond tolerance | the run is flagged and cannot be committed without acknowledgement |
| Missing potential test | row uses the documented fallback and says which |
| Commit acknowledgement lost | retained original; the run is not shown as committed |
| Dry run | clearly labelled; nothing persisted |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Preview | `production.allocation.run` | no |
| Override a row | `production.allocation.override` | no (until commit) + reason |
| Commit | `production.allocation.commit` | yes |

## 7. Acceptance criteria

- AC-1 Every row shows its method and residual.
- AC-2 Total allocated equals total measured within tolerance, or the run is flagged.
- AC-3 A manual override requires a reason and is recorded.
- AC-4 Commit is idempotent: repeating the same run does not double-post.
- AC-5 Dry run persists nothing.
- AC-6 The run stores a hash of its inputs.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~Allocation"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~AllocationGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-05-PRD-04/observed-evidence.json). The run reads the PPDM `WELL`,
`PDEN_WELL` and `PDEN_VOL_SUMMARY` rows and the ProductionOperations `FACILITY_MEASUREMENT` total, and writes the
ProductionAccounting `ALLOCATION_RUN` and `ALLOCATION_RUN_ROW` rows through `UnitofWork<T>`.

**The single public entry point (section 9):** `IAllocationService.AllocateAsync(RUN_TICKET, method, userId,
connectionName)`, which allocates a run ticket into `ALLOCATION_RESULT`/`ALLOCATION_DETAIL`.

**Why the run has its own tables:** section 2 names the accounting result tables as the persistence, and that was tried
first. `ALLOCATION_RESULT` declares **both** `ALLOCATION_METHOD` (a string) and `METHOD` (the `AllocationMethod` enum), and
the framework's field builder folds them onto one column name, so any entity read of it throws *"An item with the same key
has already been added. Key: METHOD"*. The accounting entity was left untouched; the workbench owns its run pair in the
same module and the accounting flow converts a committed run. That test-verified property is recorded in the evidence so
the next accounting screen does not rediscover it.

**How a residual happens:** the factors are normalised over the wells that have a basis, so a run with no override
allocates the measured total exactly. The residual therefore comes from a manual override (or a row left unallocated),
which is what AC-2's acknowledgement is for.

## 9. Open questions and risks

- `AllocationService` has a large partial set; identify the single public entry point and record it
  before designing the run panel.
