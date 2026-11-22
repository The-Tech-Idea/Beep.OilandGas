# PRD-05 — Volume reconciliation and reporting

| Field | Value |
|---|---|
| Stage | STG-05 — Production operations and allocation |
| Area | Production |
| Status | Gate met - see [evidence](../../evidence/STG-05-PRD-05/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProductionAccounting` |
| Desktop project | `Modules/Production/Reconciliation/` |
| External packages | none |
| Depends on | PRD-04 |
| Blocks | ACC-07, ACC-08 |
| Web reference | `Pages/PPDM39/Production/Reporting.razor`, `Pages/PPDM39/Accounting/VolumeReconciliation.razor` |

## 1. Purpose

Explain the gap between what the wells produced, what was measured, what was sold and what was
booked — and produce the report that proves it.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProductionAccounting/Services/ProductionAccountingService.cs` and `.Reconciliation` | reconciliation |
| `ProductionAccounting/Services/MeasurementService.cs` | measured |
| `ProductionAccounting/Services/TakeOrPayService.cs` | contractual volumes |
| Tables run tickets, `PDEN_VOL_SUMMARY`, allocation results | persistence |

## 3. Screens

`Production → Reconciliation`

- Period and field selector.
- Waterfall: produced → measured → allocated → sold → booked, with the difference at each step.
- Variance grid by well/pool: each stage, difference, percentage, reason code.
- Reason assignment per variance line.
- Report preview and export (PDF/XLSX) via RPT-01/02.

## 4. Data and contracts

`OgReconciliationLine { Dimension, Produced, Measured, Allocated, Sold, Booked, Unit,
VarianceReason }`.

## 5. States

| State | Behaviour |
|---|---|
| Missing stage data | that step is "—" and the waterfall stops there |
| Variance without a reason | export is blocked until every material variance has one |
| Failed read | failed state |
| Period open vs closed | closed periods are read-only with a clear marker |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `production.read` | no |
| Assign variance reason | `production.reconciliation.write` | yes |
| Export | `data.export` | no |

## 7. Acceptance criteria

- AC-1 The waterfall is arithmetically consistent (each step = previous ± difference).
- AC-2 A material variance requires a reason before export.
- AC-3 Units are consistent across all stages or the comparison is refused.
- AC-4 A closed period cannot be edited.
- AC-5 Monthly totals equal the sum of daily rows.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~Reconciliation"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ReconciliationGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-05-PRD-05/observed-evidence.json). The waterfall reads the PPDM
`WELL`, `PDEN_WELL`, `PDEN_VOL_SUMMARY` and `PDEN_VOL_DISPOSITION` rows, the ProductionAccounting `PRODUCTION_ALLOCATION`
rows and PRD-04's committed `ALLOCATION_RUN`/`ALLOCATION_RUN_ROW`; the reasons, their code book and the open/closed month
are written through `UnitofWork<T>` over the feature's own `RECONCILIATION_VARIANCE`, `R_RECONCILIATION_REASON` and
`RECONCILIATION_PERIOD`.

**The join key (section 9), answered:** field + well + date. Produced is the well's own measurement; measured and
allocated are PRD-04's committed run rows; sold is the disposition record; booked is
`PRODUCTION_ALLOCATION.ALLOCATED_VOLUME`.

**Why sold is not the accounting run ticket:** the plan names `RUN_TICKET`, and reading it first threw *"An item with the
same key has already been added. Key: TEMPERATURE"* - its compatibility partial declares `Temperature` beside the PPDM
`TEMPERATURE` column, so the entity cannot be read at all. The sold stage therefore reads the PPDM
`PDEN_VOL_DISPOSITION` rows, which record the same handover, and the page prints that decision. The accounting entity was
left untouched and the defect is recorded for its owner, beside PRD-04's `ALLOCATION_RESULT` (`ALLOCATION_METHOD` vs
`METHOD`).

**How a material variance is found:** a step's difference as a percentage of the stage before it, against the month's
materiality (1% by default). A variance on the first stage has no previous stage, so it is never material by itself.

## 9. Open questions and risks

- "Booked" figures come from accounting; confirm the join key before building the waterfall.
