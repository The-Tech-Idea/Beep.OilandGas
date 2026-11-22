# PRD-03 — Well status board and deferment log

| Field | Value |
|---|---|
| Stage | STG-05 — Production operations and allocation |
| Area | Production |
| Status | Gate met - see [evidence](../../evidence/STG-05-PRD-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProductionOperations`, `Beep.OilandGas.PPDM39.DataManagement` |
| Desktop project | `Modules/Production/WellStatus/` |
| External packages | none |
| Depends on | PRD-01, WEL-04 |
| Blocks | — |
| Web reference | `Components/Production/WellStatusCard.razor`, `Components/Production/DefermentLogger.razor`, `Pages/PPDM39/WellDetails/SetWellStatus.razor` |

## 1. Purpose

Show which wells are producing, which are down and why, and record deferment with a reason code so
lost volume is explainable.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProductionOperations/Services/IProductionOperationsService.cs` | well status and deferment |
| `PPDM39.DataManagement/Repositories/IWellStatusService.cs` | status reference |
| `R_WELL_STATUS`, `WELL_STATUS`, production/deferment records | persistence |

## 3. Screens

`Production → Well status`

- Status board grouped by status with counts, filterable by field/pool.
- Well cards: name, status, rate, last change, reason.
- Deferment logger: well, start, end (or ongoing), reason code, volume, comment, reporter.
- Deferment list with totals by reason.

## 4. Data and contracts

`OgDeferment { WellId, Start, End?, ReasonCode, DeferredVolume, VolumeUnit, Comment, Reporter }`.

## 5. States

| State | Behaviour |
|---|---|
| Reason code missing | save is refused; the reference list is offered |
| Ongoing deferment | shown as "ongoing" with elapsed hours |
| Overlapping deferments for one well | flagged, not silently merged |
| Failed read | failed state |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Log deferment | `production.deferment.write` | yes |
| Close deferment | `production.deferment.write` | yes |
| View | `production.read` | no |

## 7. Acceptance criteria

- AC-1 A deferment cannot be saved without a reference reason code.
- AC-2 Overlapping deferments are flagged.
- AC-3 Deferred volume carries a unit.
- AC-4 Totals by reason equal the sum of rows.
- AC-5 An ongoing deferment is distinct from a closed one.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~Deferment"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~DefermentGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-05-PRD-03/observed-evidence.json). The board reads the PPDM
`WELL`, `WELL_STATUS`, `PDEN_WELL` and `PDEN_VOL_SUMMARY` rows; the deferments are read and written through
`UnitofWork<T>` over the ProductionOperations `PRODUCTION_DEFERMENT` and `R_DEFERMENT_REASON` rows.

**The open question in section 9, answered:** a deferment is **not** derived from the status history. `WELL_STATUS` has
the status, a status type and a start/end time, but no deferred volume and no coded reason, so a record that explains lost
volume cannot be a status row. The status board still reads `WELL_STATUS` (and WEL-04 owns changing it); the deferment is
its own row, linked to the well by `WELL_ID`. That is why the feature has two tables rather than one.

**Why the reason table is empty:** `R_DEFERMENT_REASON` is the operating company's code book, not the application's. Until
the operator's rows exist the page says which table to fill and refuses every deferment, rather than accepting free text -
the same rule DEV-06 applies to NPT codes.

**The page's route is `deferments`,** not `wellstatus`: the status board and its change form belong to WEL-04, and this
page is the deferment record. (While adding it, a first version named `WellStatusPageView` overwrote the WEL-04 page; that
page was rewritten against its untouched service and the incident is recorded in the evidence.)

## 9. Open questions and risks

- Deferment may be derived from status history rather than stored separately; confirm with the
  service before creating a second record type.
