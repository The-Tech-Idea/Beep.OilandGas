# DEV-06 — Drilling execution and daily reports

| Field | Value |
|---|---|
| Stage | STG-04 — Development and drilling |
| Area | Development |
| Status | Gate met - see [evidence](../../evidence/STG-04-DEV-06/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.DrillingAndConstruction` |
| Desktop project | `Modules/Development/DrillingExecution/` |
| External packages | `TheTechIdea.OilGas.Drilling` (`DrillingPerformance`, `TimeDepthView`) |
| Depends on | DEV-05 |
| Blocks | DEV-07, ENG-09 |
| Web reference | `Pages/PPDM39/Drilling/DrillingExecution.razor`, `DrillingExecutionAssigned.razor`, `DrillingExecutionAssignment.razor`, `DrillingDailyReports.razor`, `DrillingReportingResponsibility.razor`, `DrillingReportingTransfer.razor` |

## 1. Purpose

Run the approved program: record daily progress, costs and events, and keep the reporting
responsibility auditable as shifts and contractors change.

## 2. Source components

| Source | Reuse |
|---|---|
| `DrillingAndConstruction/Services/DrillingOperationService.Execution*.cs` | execution state |
| `…DailyReport*.cs` | daily report authoring |
| Tables `DRILLING_ACTIVITY`, `DRILLING_PROGRAM`, `WORK_ORDER` linkage | persistence |
| `TheTechIdea.OilGas.Drilling/DrillingPerformance.cs` | NPT analysis (ENG-09) |
| `TheTechIdea.OilGas.Drilling/TimeDepthView.cs` | time-depth chart (ENG-09) |

## 3. Screens

`Development → Drilling execution`

- Execution header: well, program version, current depth, current activity, days on well, cost to date.
- Daily report editor: date, shift, 24-hour breakdown with activity code (IADC), depth at start/end,
  metres drilled, NPT with reason, cost, HSE events, personnel, equipment.
- Report list with submission state and author.
- Reporting responsibility panel: current responsible person/contractor, assignment history,
  transfer action with reason.
- Time-depth chart panel.

## 4. Data and contracts

`OgDailyReport { WellId, Date, Shift, Activities[], StartDepth, EndDepth, DepthUnit, Npt[], Cost,
Currency, SubmittedBy, State }`
`OgReportingResponsibility { AssignedTo, Role, From, To, Reason }`.

## 5. States

| State | Behaviour |
|---|---|
| Report in progress | draft, editable |
| Report submitted | read-only; correction requires a new revision with a reason |
| Acknowledgement lost | retained original; the report is not shown as submitted |
| Responsibility transfer pending | execution entry disabled for the previous holder |
| Duplicate date+shift | refused |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Author daily report | `drilling.report.write` | yes |
| Submit | `drilling.report.submit` | yes |
| Transfer responsibility | `drilling.report.transfer` | yes |

## 7. Acceptance criteria

- AC-1 A daily report cannot be submitted twice for the same date and shift.
- AC-2 A submitted report is read-only; correction is a new revision with a reason.
- AC-3 A lost acknowledgement retains the original and does not show success.
- AC-4 Responsibility transfer is recorded with actor, receiver and reason.
- AC-5 NPT requires a reason code from the reference set.
- AC-6 Depth at start equals the previous report's depth at end, or the discrepancy is flagged.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~DrillingExecution"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~DrillingExecutionGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-04-DEV-06/observed-evidence.json). The execution is read and written
through `UnitofWork<T>` over the DrillingAndConstruction-owned `DEVELOPMENT_DRILLING_EXECUTION`,
`DEVELOPMENT_DAILY_REPORT`, `DEVELOPMENT_REPORT_RESPONSIBILITY` and `R_DRILLING_NPT_CODE`, with the breakdown in the
existing `DRILLING_ACTIVITY` rows keyed by `DDR_ID`.

**Storage decision (recorded here because it changes the source components in section 2):** the module's
`DRILLING_OPERATION` and `DRILLING_DAILY_REPORT` are the company execution records; their schema requires the program
review instance, the execution-start proof, the recorder's role/persona and the bounded original report proof, none of
which a plan-stage execution has. The feature therefore writes the plan-stage tables above (the same decision DEV-05
made for the program), and the company flow starts its own record once the governance gate is met.

**Route mapping for the six Web pages (section 9's open question):** `DrillingExecution.razor`,
`DrillingExecutionAssigned.razor`, `DrillingExecutionAssignment.razor`, `DrillingDailyReports.razor`,
`DrillingReportingResponsibility.razor` and `DrillingReportingTransfer.razor` are one workspace at route
`drillingexecution`: the header covers the execution pages, the report grid and editor cover the daily-report pages, and
the transfer panel covers the two responsibility pages.


## 9. Open questions and risks

- Six Web pages cover this area; this feature consolidates them into one workspace and records the
  route mapping.
- Daily reports are high-volume; the grid must be virtualised from the first version.
