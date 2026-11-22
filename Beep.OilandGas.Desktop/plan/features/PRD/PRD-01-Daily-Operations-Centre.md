# PRD-01 — Daily operations centre

| Field | Value |
|---|---|
| Stage | STG-05 — Production operations and allocation |
| Area | Production |
| Status | Gate met - see [evidence](../../evidence/STG-05-PRD-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProductionOperations` |
| Desktop project | `Modules/Production/OperationsCentre/` |
| External packages | `TheTechIdea.OilGas.Production` (`DeclineFitter` for trend lines) |
| Depends on | WEL-04 |
| Blocks | PRD-03, PRD-06 |
| Web reference | `Pages/PPDM39/Production/ProductionDashboard.razor`, `ProductionManagerDashboard.razor`, `Pages/PPDM39/FieldDashboard.razor`, `Components/ProductionFieldOverview.razor` |

## 1. Purpose

The single page a production supervisor opens first: today's volumes, what is down, what changed,
and what needs a decision.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProductionOperations/Services/ProductionManagementService.cs`, `PdenFacilityQueries.cs` | volume reads |
| `ProductionOperations/Services/IProductionOperationsService.cs` | operations contract |
| `ProductionAccounting/Services/AllocationService.cs` (read-only summary) | allocated vs measured |
| `PPDM.Models/39/PDEN_VOL_SUMMARY`, `PDEN_FLOW_MEASUREMENT`, `WELL`, `FIELD` | persistence |

## 3. Screens

- Header: field, date, data-completeness indicator.
- KPI cards: oil, gas, water, water cut, deferred volume, active wells / total wells.
- Volume strip: last 30 days stacked by fluid, with the current day highlighted.
- Downtime table: well, reason, hours, deferred volume, since when.
- Top movers: wells with the largest positive/negative change vs the previous 7-day average.
- Alerts: wells with missing production data for the day.

## 4. Data and contracts

`OgProductionDay { Date, Oil, Gas, Water, Units, DeferredVolume, ActiveWells, TotalWells }`
Volumes carry the field's configured unit; conversion only via the unit catalog.

## 5. States

| State | Behaviour |
|---|---|
| Read failed | failure panel; **never** zero cards |
| Partial data | completeness indicator with the missing well count |
| No data for the day | explicit "not yet reported", not zero |
| Allocation not run | card shows measured only, labelled |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| View | `production.read` |
| Drill into a well | `wells.read` |
| Open deferment log | `production.deferment.read` |

## 7. Acceptance criteria

- AC-1 A failed read renders the failure panel, not zeros.
- AC-2 "Not yet reported" is distinguishable from zero production.
- AC-3 Volumes carry a unit and are converted only through the catalog.
- AC-4 The completeness indicator counts wells missing data.
- AC-5 Top movers requires at least 7 days of history, otherwise it is hidden with a reason.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~OperationsCentre"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~OperationsCentreGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-05-PRD-01/observed-evidence.json). The day is read through
`UnitofWork` reads over the PPDM `FIELD`, `WELL`, `PDEN_WELL` and `PDEN_VOL_SUMMARY` entity models; the feature writes
nothing and adds no table.

**The grain, recorded (section 9 risk):** `PDEN_VOL_SUMMARY` per production entity (`PDEN_ID`), per
`VOLUME_DATE` (calendar day) and per `ACTIVITY_TYPE`, taking the highest `AMENDMENT_SEQ_NO`, so an amendment supersedes
its original. Injection rows, rows without a volume date, superseded rows and rows in another unit are excluded **and
counted on the page**, so the reader can see what the total does not include.

**What is not this feature:** the deferred-volume card, the downtime table and allocated-versus-measured belong to PRD-03,
PRD-04 and PRD-05. They are rendered as "not recorded: the deferment log is PRD-03" and "measured only: allocation is
PRD-04", never as zero.

## 9. Open questions and risks

- The Web's field overview was substituted in its own tests, so its data behaviour is unverified.
  Design from `PdenFacilityQueries`, not from the Web component.
