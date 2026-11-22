# RES-06 — Decline curve analysis

| Field | Value |
|---|---|
| Stage | STG-06 — Reservoir, PVT and forecasting |
| Area | Reservoir |
| Status | Gate met - see [evidence](../../evidence/STG-06-RES-06/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProductionForecasting` |
| Desktop project | `Modules/Reservoir/DeclineCurve/` |
| External packages | `TheTechIdea.OilGas.Production` (`DeclineFitter`) |
| Depends on | PRD-01 |
| Blocks | RES-07, ECO-01 |
| Web reference | `Pages/PPDM39/Calculations/DeclineCurveAnalysis.razor` (`/analysis/decline-curve`) |

## 1. Purpose

Fit a decline model to real production history and show the fit quality, the parameters and the
forecast band — with a second, independent fitter as a cross-check.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProductionForecasting/DCA/Services/DCAAnalysisService.cs`, `.Advanced` | fitting, Monte Carlo, sensitivity |
| `ProductionForecasting/Services/ProductionHistoryLoader.cs` | history from PPDM |
| `ProductionForecasting/DCA/Interaction/DCAInteractionHandler.cs` | plot interaction (framework-free) |
| `ProductionForecasting/Constants/ForecastAlgorithmConstants.cs` | b bounds, defaults, minimum history |
| `Production` `DeclineFitter.Fit` | independent cross-check |
| Table `PDEN_VOL_SUMMARY` | history |

## 3. Screens

`Reservoir → Decline curve`

- History panel: well/pool selector, date range, fluid, data-quality flags (gaps, zeroes, outliers).
- Fit panel: model (exponential / harmonic / hyperbolic / modified hyperbolic), automatic or fixed b,
  resulting qi, Di, b, EUR, R².
- Plot: rate vs time on log scale, history points, fitted curve, forecast, with the interaction
  handler attached.
- Diagnostics: residual plot, excluded points with reason.
- Comparison: the second fitter's parameters side by side.
- Save as forecast input (RES-07).

## 4. Data and contracts

`OgDeclineFit { Model, Qi, Di, B, Eur, R2, PointsUsed, PointsExcluded, Bounded }`
`OgForecastBand { P90, P50, P10 }` when Monte Carlo is run.

## 5. States

| State | Behaviour |
|---|---|
| Too few points | fit refused with the minimum stated |
| b outside bounds | fit refused, bounds shown |
| Zero or negative rates | excluded with a reason, counted |
| Fit non-converged | shown as failed, no curve drawn |
| Two fitters disagree beyond tolerance | both shown, disagreement flagged |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Fit | `forecasting.dca.read` | no |
| Save analysis | `forecasting.dca.write` | yes |

## 7. Acceptance criteria

- AC-1 A synthetic history with known parameters is recovered within tolerance.
- AC-2 b outside the configured bounds is refused.
- AC-3 Excluded points are listed with reasons.
- AC-4 The tools' `DeclineFitter` agrees within the stated tolerance, or the disagreement is shown.
- AC-5 A non-converged fit draws no curve.
- AC-6 Results are deterministic for the same inputs and seed.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~DeclineCurve"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~DeclineCurveGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-06-RES-06/observed-evidence.json). The fit is read and written
through `UnitofWork<T>` over the ProductionForecasting-owned `DECLINE_FIT`; the history is the PPDM `PDEN_VOL_SUMMARY`
through `PDEN_WELL` at PRD-01's grain.

**Which implementation is authoritative (section 9), answered:** the ProductionForecasting module's `DCAGenerator` is
authoritative and the sibling tools' `TheTechIdea.OilGas.Production.DeclineFitter` is the independent cross-check. The
data layer references that tools project so the cross-check really is a second implementation; they are compared at b
0.05 absolute and qi 5% relative, and a disagreement is shown with both parameter sets rather than resolved.

**A defect this feature found and fixed:** `DCAAnalysisService.AnalyzeDeclineAsync` threw
`ConvergenceException` ("Final Error: 1.186775E+139") on a textbook history at qi 1000, Di 0.01/day, b 0.5, because
`DCAGenerator.FitCurve` builds a basis that uses the same parameter as both the coefficient and the exponent. Two
additive methods fix it without touching `FitCurve`:

- `TryFitDecline` - qi and Di by exact least squares on the b-transformed rate (`q^-b = qi^-b (1 + b Di t)`, `b = 0`
  being the exponential case), b found by golden-section search **inside the caller's bounds**, with both bounds
  evaluated explicitly so a fit the data pushes to a bound is reported as pinned.
- `TryFitDeclineAtB` - the fixed-b form used by the exponential, harmonic and fixed-b hyperbolic models.

The reference history is recovered **exactly** (qi 1000.0000, Di 0.010000, b 0.500000, R2 1.000000), and the tools'
fitter returns the same three parameters on the same points.

**b within bounds (AC-2, SG-06-6):** the bounds are `ForecastAlgorithmConstants.ArpsBdfMinB` and `ArpsBdfMaxB` (0 and 1,
with `ArpsBdfSoftMaxB` 0.95 as the flag). A fixed b outside them is refused with `decline-b-outside-bounds`, and a
history generated at b = 1.5 is refused with `decline-fit-unbounded`: the data wanting more than 1.0 is never reported as
1.0.

**What is not built:** the rate-vs-time plot (these calculation screens still have no chart host), the Monte Carlo band
(optional in the plan, and it would need a seeded generator), and a terminal-decline segment for the modified hyperbolic
model (that belongs to the forecast RES-07 builds). A refused fit is shown with its code and message but is not saved,
because keeping a refusal would let a later reader mistake it for an analysis.

## 9. Open questions and risks

- Two independent implementations exist; the cross-check is a strength, not a duplicate. Record
  which one is authoritative.
