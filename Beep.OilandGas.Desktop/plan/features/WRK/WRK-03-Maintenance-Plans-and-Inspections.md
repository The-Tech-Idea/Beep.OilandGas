# WRK-03 — Maintenance plans and inspections

| Field | Value |
|---|---|
| Stage | STG-13 — Work management |
| Area | Work |
| Status | Gate met - see [evidence](../../evidence/STG-13-WRK-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProductionOperations`, `Beep.OilandGas.PPDM39.DataManagement`, `Beep.OilandGas.DevelopmentPlanning` |
| Desktop project | `Modules/Work/Maintenance/` |
| External packages | none |
| Depends on | WRK-01 |
| Blocks | — |
| Web reference | `Components/Workflow/MaintenanceRecordedWorkQueue.razor`, `Components/BusinessProcess/MaintenanceAssetEvidencePanel.razor`, `…AssetReturnPanel`, `…ClosurePanel` |

## 1. Purpose

Keep assets maintained on schedule: plans, service jobs, inspection schedules and their results,
and the evidence that a maintenance action actually happened.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProductionOperations/Services/FacilityManagementService.*` maintenance lifecycle partials | maintenance |
| `DevelopmentPlanning/Data/Contracts/CreateWellMaintenancePlan.cs`, `CreateWellServiceJob.cs` | plan and job contracts |
| `PPDM39.DataManagement/Services/WorkOrder/InspectionService.cs` | inspections |
| Tables `WELL_MAINTENANCE_PLAN`, `WELL_SERVICE_JOB`, inspection records | persistence |

## 3. Screens

`Work → Maintenance`

- Plan list: asset, plan name, frequency, next due, last done, compliance state.
- Plan editor: tasks, frequency, tolerance window, required skills, materials.
- Service job list with generation from a plan when due.
- Inspection list: asset, inspection type, due date, result, findings, next due.
- Evidence panel: recorded maintenance with documents and outcome.

## 4. Data and contracts

`OgMaintenancePlan { AssetId, Name, Frequency, FrequencyUnit, NextDue, LastDone, Tasks[] }`
`OgInspection { AssetId, Kind, DueDate, CompletedDate, Result, Findings[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Due but not generated | "due"; generation offered |
| Overdue | red chip with days overdue |
| Inspection with findings | cannot close without actions |
| Frequency without a unit | refused |
| Failed read | failed state |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Edit plan | `work.maintenance.write` | yes |
| Generate job | `work.maintenance.generate` | yes |
| Record inspection | `work.inspection.write` | yes |

## 7. Acceptance Criteria

- AC-1 Next due is computed from last done plus frequency and unit.
- AC-2 An inspection with findings cannot close without linked actions.
- AC-3 A generated job references its plan and the generation date.
- AC-4 Compliance state is derived, not stored.
- AC-5 Frequency always carries a unit.
- AC-6 Every recorded action names its actor and date.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WRK-03"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-13-WRK-03/observed-evidence.json). The plan record stays where the owner
keeps it; only its schedule was added.

**The stage's open question, answered.** *Which project owns the plan record?* This one: the plan is `WELL_MAINTENANCE_PLAN` in
DevelopmentPlanning, the inspections are PPDM's `WORK_ORDER_INSPECTION`, and the generated jobs are `WELL_SERVICE_JOB`. No
second plan table and no second inspection table were created.

**The plan had no frequency at all.** It recorded the asset, the maintenance type, a status, a priority and a planned window -
and nothing that says how often the work repeats, so *"next due"* had no recorded source. `FREQUENCY`, `FREQUENCY_UNIT`,
`LAST_DONE_DATE`, `TASK_LIST`, `SCHEDULE_EDITED_BY` and `SCHEDULE_EDITED_DATE` are additive columns on the plan's own partial
class, and **next due and compliance are computed on every read** - never stored, so they cannot go stale.

| What the page shows | Recorded in |
|---|---|
| Asset, type, status, priority, planned window | `WELL_MAINTENANCE_PLAN` |
| Frequency and its unit, last done, tasks, edit actor/date | the added plan columns |
| Next due date and compliance state | computed from the last done date and the frequency |
| Job, its status and its planned window | `WELL_SERVICE_JOB` |
| Which plan a job came from, and when | the added `MAINT_PLAN_ID` / `GENERATED_ON` |
| Inspection result, notes, inspector, inspected-at | `WORK_ORDER_INSPECTION` |
| An inspection's due date | the linked `WORK_ORDER.DUE_DATE` |
| Linked corrective action | the `CORRECTIVE_ACTION` `PROJECT` reached through `WORK_ORDER_COMPONENT.PROJECT_ID` |

**AC-2's link was already in the schema.** A corrective action is a `PROJECT` row of type `CORRECTIVE_ACTION`, reached through
the work order's component - the same link HSE-03 and WRK-02 use. So no second action table was invented: an inspection whose
notes record findings (or whose result is `FAIL`) is refused until such a project is linked to its work order, and the refusal
names the result it cannot close as.

**Generation is gated, not automatic.** A job is generated only from a plan that is `due` or `overdue`; a `current` or
`not scheduled` plan is refused and the refusal names its next due date. The generated job records the plan id and the date it
was generated, which is what makes the schedule auditable.

**Two owner quirks surfaced.** `WORK_ORDER_INSPECTION` carries **no due date** - the due date belongs to the work order, so the
inspection row shows that one rather than inventing a date - and it marks `JURISDICTION` and `REQUIREMENT_ACTOR` as required, so
a seeded row has to carry them before an update will commit.

**Not built, and stated as gaps:** structured task rows with their own skills and materials (the owner has no task table, so
tasks are a free-text list); a frequency tolerance window (the feature mentions one, nothing in the records defines it, so none
was invented); individual finding rows (findings are the result notes); a confirmation that the linked action addresses *this*
finding (the rule proves an action exists on the work order, not that it matches); scheduling and resource assignment for a
generated job; inspection filtering for a work order with no component; per-command permission enforcement; and the Web's four
panels (recorded work, asset evidence, asset return, closure) in place of this page's single editor bar.

## 9. Open questions and risks

- Maintenance lives across two projects; choose one owner for the plan record and record it here.
