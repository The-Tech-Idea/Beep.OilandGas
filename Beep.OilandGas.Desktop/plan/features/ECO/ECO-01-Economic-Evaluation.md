# ECO-01 — Economic evaluation

| Field | Value |
|---|---|
| Stage | STG-09 — Economics and AFE |
| Area | Economics |
| Status | Gate met - see [evidence](../../evidence/STG-09-ECO-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.EconomicAnalysis` |
| Desktop project | `Modules/Economics/Evaluation/` |
| External packages | none |
| Depends on | RES-05, RES-07 |
| Blocks | ECO-03, RPT-01 |
| Web reference | `Pages/PPDM39/Calculations/EconomicAnalysis.razor`, `Pages/PPDM39/Economics/EconomicEvaluation.razor` |

## 1. Purpose

Turn a forecast and a fiscal regime into cash flow, NPV, IRR and payback, with sensitivity — and
show which inputs drive the answer.

## 2. Source components

| Source | Reuse |
|---|---|
| `EconomicAnalysis/Services/EconomicAnalysisService.cs`, `.Advanced` | `IEconomicAnalysisService`, `ICalculationService` |
| `EconomicAnalysis/Core/Interfaces/IEconomicAnalysisService.cs` | contract |
| `EconomicAnalysis/Rendering/EconomicRenderer.cs`, `.Configuration` | cash-flow chart |
| `EconomicAnalysis/Interaction/EconomicInteractionHandler.cs` | interaction |
| Tables `R_ECONOMIC_METRIC`, `R_ECONOMIC_SCENARIO`, `R_ECONOMIC_SCHEDULE` | reference and schedule |

## 3. Screens

`Economics → Evaluation`

- Scenario panel: name, entity (well/pool/field/prospect), forecast (RES-07), fiscal regime,
  discount rate, currency, start date.
- Inputs: capex schedule, opex per unit, price deck, escalation, abandonment cost.
- Results: cash-flow table by period, NPV, IRR, payback, peak funding, undiscounted total.
- Charts: cash flow and cumulative via `EconomicRenderer`.
- Sensitivity: tornado on price, opex, capex, rate, discount.
- Save / version / approve.

## 4. Data and contracts

`OgEconomicResult { Npv, Irr, PaybackMonths, PeakFunding, Currency, Periods[], Drivers[] }`
All money is `decimal`.

## 5. States

| State | Behaviour |
|---|---|
| Missing fiscal term | run blocked with the term named |
| No forecast | blocked with a link to RES-07 |
| IRR undefined (no sign change) | shown as "undefined", never as 0 |
| Currency mismatch | refused |
| Lost acknowledgement on save | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Run | `economics.read` | no |
| Save | `economics.write` | yes |
| Approve | `economics.approve` + SoD | yes |

## 7. Acceptance criteria

- AC-1 A flat cash-flow reference case matches the closed-form NPV exactly.
- AC-2 IRR is `undefined` when there is no sign change, not zero.
- AC-3 All money values are `decimal` end to end.
- AC-4 A missing fiscal term blocks the run with its name.
- AC-5 Sensitivity uses the same engine as the base run.
- AC-6 An approved evaluation is immutable and identifies its forecast version.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ECO-01"
```

## 9. Open questions and risks

- Fiscal regime shape is undocumented; read `EconomicAnalysisService.Advanced` and record it in §4
  before designing inputs.
