# RES-07 — Production forecasting

| Field | Value |
|---|---|
| Stage | STG-06 — Reservoir, PVT and forecasting |
| Area | Reservoir |
| Status | Gate met - see [evidence](../../evidence/STG-06-RES-07/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProductionForecasting` |
| Desktop project | `Modules/Reservoir/Forecasting/` |
| External packages | none |
| Depends on | RES-06 |
| Blocks | ECO-01 |
| Web reference | `Pages/PPDM39/Production/ProductionForecasting.razor`, `Forecasts.razor`, `Components/ProductionForecastDialog.razor` |

## 1. Purpose

Produce, compare and persist production forecasts with their basis, so a plan or an economic run
can point at an exact forecast.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProductionForecasting/Services/ProductionForecastingService.cs`, `.ForecastGeneration`, `.Economics` | generation |
| `ProductionForecasting/Services/ProductionForecastResultMapper.cs` | persistence mapping |
| Tables `PRODUCTION_FORECAST`, `PRODUCTION_FORECAST_POINT`, `R_PRODUCTION_FORECASTING_REFERENCE_CODE` | persistence |
| `Constants/ProductionForecastingReferenceSets.cs` | method, run status, risk rating |

## 3. Screens

`Reservoir → Forecasting`

- Forecast list: name, entity (well/pool/field), method (`DCA`, transient, PSS, gas well), period,
  status, risk rating, created by, version.
- Generator: source (DCA fit from RES-06, or manual), horizon, constraints (max rate, min rate,
  economic limit), profile.
- Result: monthly and annual table, cumulative, chart with bands.
- Comparison: up to four forecasts on one chart with a delta table.
- Persistence: save, version, archive; every point stored.

## 4. Data and contracts

`OgForecast { Id, Entity, Method, StartDate, EndDate, Points[], Status, RiskRating, Version }`
`OgForecastPoint { Date, Oil, Gas, Water, Unit }`.

## 5. States

| State | Behaviour |
|---|---|
| No source | generator disabled with the reason (run DCA first) |
| Constraint makes the profile negative | refused |
| Save acknowledgement lost | retained original |
| Version conflict | both versions listed, no overwrite |
| Unverified method | status "draft" until reviewed |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Generate | `forecasting.read` | no |
| Save | `forecasting.write` | yes |
| Approve | `forecasting.approve` + SoD | yes |

## 7. Acceptance criteria

- AC-1 A saved forecast reloads with identical points.
- AC-2 Every forecast records its method and source fit.
- AC-3 Versions never overwrite each other.
- AC-4 A forecast used by ECO-01 is immutable while referenced.
- AC-5 Constraints are enforced and violations refused, not clamped.
- AC-6 Volumes carry units per stream.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~Forecast"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ForecastGatewayTests"
```

## 9. Open questions and risks

- `ProductionForecastingService` uses `"CSV"` and `"DCA"` literals internally; surface codes from
  the reference sets in the UI rather than repeating those strings.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-06-RES-07/observed-evidence.json). The forecast and its points are
read and written through `UnitofWork<T>` over the ProductionForecasting-owned `PRODUCTION_FORECAST` and
`PRODUCTION_FORECAST_POINT`; the source is the RES-06 `DECLINE_FIT`.

**The open question above, answered:** the method list is read from `R_PRODUCTION_FORECASTING_REFERENCE_CODE` under
`PRODUCTION_FORECAST_METHOD` (the `ForecastType` names the seed already writes), statuses from
`PRODUCTION_FORECAST_RUN_STATUS` and risk ratings from `PRODUCTION_FORECAST_RISK_RATING`; when the table has no rows the
workspace says so and falls back to the `ForecastType` set. No `"DCA"` or `"CSV"` literal appears in the view.

**The profile** is the owner's arithmetic: qi, Di and b come from the fit, each month's rate is
`ArpsDeclineMethods` at the month start and its volume is the difference of the owner's closed-form cumulative between
the month's two ends. The four Arps methods can be generated; the other `ForecastType` codes are listed and refused with
the reason that this screen has no source for them.

**Versions and approval (AC-3, AC-4):** a save always inserts a new version with `VERSION_OF` pointing at the previous
one, so nothing is overwritten. Approval is the only state transition: it refuses when the approver is the creator
(separation of duties), then sets `Approved` and `IMMUTABLE_IND`. ECO-01 does not exist, so "immutable while referenced"
is represented by approval - recorded as a stated substitution rather than claimed as a reference check.

**Constraints (AC-5):** a ceiling the profile breaks is refused, naming the month and both values; the profile is never
clamped. An economic limit is not a violation - it is where the profile ends - and that reading is stated on the page.

**A gap in RES-06 closed:** `DECLINE_FIT` could not say which stream its fit described. It gained `FLUID`, RES-06's save
writes it, and RES-06's gateway test asserts it, because a forecast of 1000 has no unit and no meaning without it.

**Not built:** the comparison view (up to four forecasts with a delta table) and the chart, because these calculation
screens still have no chart host; the CSV export, which belongs to the export page; and per-command permissions, which
this app does not enforce (pages are gated). Separation of duties on approval is enforced.
