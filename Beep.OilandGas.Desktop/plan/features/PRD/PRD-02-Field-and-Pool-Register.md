# PRD-02 — Field and pool register

| Field | Value |
|---|---|
| Stage | STG-05 — Production operations and allocation |
| Area | Production |
| Status | Gate met - see [evidence](../../evidence/STG-05-PRD-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProductionOperations` |
| Desktop project | `Modules/Production/FieldPool/` |
| External packages | none |
| Depends on | DEV-04 |
| Blocks | PRD-04 |
| Web reference | `Pages/PPDM39/Production/Fields.razor`, `Pages/PPDM39/Production/Pools.razor`, `Pages/PPDM39/AssetPortfolio.razor` (do **not** port its hardcoded KPIs) |

## 1. Purpose

The register of producing fields and pools with their real, recorded performance — replacing the
Web asset portfolio page, which shows hardcoded demo figures.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProductionOperations/Services/ProductionManagementService.cs` | field and pool performance |
| `PPDM.Models/39/FIELD`, `POOL`, `RESERVOIR`, `PDEN_VOL_SUMMARY` | entities |
| `LifeCycle/Services/FieldManagement/FieldManagementService.cs` | field lifecycle state |

## 3. Screens

`Production → Fields and pools`

- Field grid: name, basin, discovery, first production, status, wells, oil/gas/water, water cut,
  uptime.
- Pool grid for the selected field: pool, reservoir, drive, wells, current rate, cumulative.
- Trend panel for the selection.
- Actions: open field dashboard, open pool detail, export.

## 4. Data and contracts

`OgFieldSummary { Id, Name, Basin, DiscoveryDate, FirstProduction, Status, WellCount, Oil, Gas,
Water, Units, WaterCut, Uptime }`.

## 5. States

| State | Behaviour |
|---|---|
| No production yet | status "pre-production"; rates show "—" |
| Failed read | failed state, no zeroed figures |
| Missing unit | rates shown as unspecified and excluded from aggregates |
| Hardcoded demo data | must not exist anywhere in this feature |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| View | `production.read` |
| Export | `data.export` |
| Edit field metadata | `field.write` |

## 7. Acceptance criteria

- AC-1 Every figure on this page comes from a record (guard: no literal in the view model).
- AC-2 A failed read never shows zero.
- AC-3 Aggregates exclude rows with unknown units and say how many were excluded.
- AC-4 The field grid pages beyond 500 fields.
- AC-5 No demo KPI constants exist (guard test greps the feature folder).

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~FieldPoolRegister"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~FieldPoolRegisterGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-05-PRD-02/observed-evidence.json). The register is read through
`UnitofWork` reads over the PPDM `FIELD`, `POOL`, `PDEN_FIELD`, `PDEN_POOL`, `PDEN_WELL` and `PDEN_VOL_SUMMARY` entity
models and the DevelopmentPlanning `DEVELOPMENT_POOL_PLAN`; it writes nothing and adds no table.

**The two guards this feature asked for are tests, not comments:**

- *AC-1* `Every_figure_on_a_field_row_is_an_argument_and_never_a_default` reads the service source, extracts
  `OgFieldSummary(`'s parameter list and fails if it contains `=` - a default value is the only way a figure could stand in
  for a record.
- *AC-5* `The_feature_contains_no_demo_kpi_constants` reads the feature's two source files and fails on `MMBOE`, `1.2B`,
  `$1.2` or `245` - the Web portfolio page's constants - and asserts it found exactly two files first, so it cannot pass
  vacuously. That is how gap G-07 is closed.

**Grain and honest labels:** PPDM records the field's area, not a basin, so the column is *Area* and reads
`FIELD.AREA_ID`. Field oil/gas/water are 30-day window totals (the headers say *"(30 d)"*), the pool rate is the pool's
last reported day, and the pool's drive mechanism comes from DEV-04's plan row because `POOL` has no such column. No unit
is converted: a row in another unit is excluded and counted.

## 9. Open questions and risks

- `Pages/PPDM39/AssetPortfolio.razor` renders 12 fields / 245 MMBOE / $1.2B as constants. It must
  not be ported. Recorded as gap G-07.
