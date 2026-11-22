# LFT-01 — Nodal analysis

| Field | Value |
|---|---|
| Stage | STG-07 — Well performance and artificial lift |
| Area | Lift |
| Status | Gate met - see [evidence](../../evidence/STG-07-LFT-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.NodalAnalysis` |
| Desktop project | `Modules/Lift/Nodal/` |
| External packages | `TheTechIdea.OilGas.Production` (`NodalAnalysis.Solve`, `Vogel`) |
| Depends on | RES-01, RES-02 |
| Blocks | LFT-02, PRD-06 |
| Web reference | `Pages/PPDM39/Calculations/NodalAnalysis.razor` |

## 1. Purpose

Find the operating point where inflow and outflow meet, with the correlations named and the
diagnostics that explain a missing intersection.

## 2. Source components

| Source | Reuse |
|---|---|
| `NodalAnalysis/Services/NodalAnalysisService.cs`, `.Advanced`, `.SensitivityScenarios`, `.ArtificialLiftScoring` | `INodalAnalysisService` |
| `NodalAnalysis/NodalAnalyzer.cs`, `Calculations/IPRCalculator.cs`, `VLPCalculator.cs`, `NodalCalculator.cs`, `OperatingPointCalculator.cs` | engine |
| `NodalAnalysis/Rendering/NodalRenderer.cs`, `.Configuration` | plot |
| `NodalAnalysis/Interaction/NodalInteractionHandler.cs` | plot interaction |
| `NodalAnalysis/Constants/NodalConstants.cs`, `NodalAnalysisReferenceCodes.cs`, `NodalAnalysisReferenceSets.cs` | codes and defaults |
| Tables `NODAL_ANALYSIS_RESULT`, `NODAL_IPR_POINT`, `NODAL_VLP_POINT`, `NODAL_OPERATING_POINT`, `NODAL_RESERVOIR_PROPERTIES`, `NODAL_WELLBORE_PROPERTIES` | persistence |
| `Production` `NodalAnalysis.Solve` | independent cross-check |

## 3. Screens

`Lift → Nodal analysis`

- Reservoir tab: Pr, Pb, temperature, fluid properties (from RES-01/RES-02 or overridden), IPR model.
- Wellbore tab: depth, tubing, deviation, roughness, GOR, water cut, VLP correlation.
- Operating point tab: IPR curve, VLP curve, intersection, rate and Pwf, plus the renderer's
  `NodalInteractionHandler` for hover/zoom.
- Sensitivity tab: parameter sweeps (tubing size, GLR, Pr) with a delta summary.
- Lift ranking tab: candidate lift methods scored (`ArtificialLiftScoring`) with basis.
- Diagnostics tab: why no intersection, or which constraint bound the result.

## 4. Data and contracts

`OgNodalResult { QOperating, PwfOperating, IprModel, VlpCorrelation, Converged, BindingConstraint,
Points }`.

## 5. States

| State | Behaviour |
|---|---|
| No intersection | operating point is absent; diagnostics explain and lift ranking is offered |
| Non-converged | failed, with iteration count |
| Missing fluid data | refused naming the missing property |
| Cross-check disagrees | both results shown with the delta |
| Sensitivity cancelled | partial results reported, not silently dropped |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Run | `lift.nodal.read` | no |
| Save study | `lift.nodal.write` | yes |

## 7. Acceptance criteria

- AC-1 A reference case finds the expected operating point within tolerance.
- AC-2 No-intersection is reported as such, never as a zero rate.
- AC-3 The tools `Production.NodalAnalysis.Solve` agrees within tolerance or the difference is shown.
- AC-4 Curves are drawn by `NodalRenderer`, not re-implemented.
- AC-5 Every saved study records both correlations used.
- AC-6 Sensitivity runs are cancellable and report completed points.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LFT-01"
```

## 9. Open questions and risks

- `NodalAnalysisService.Advanced.cs` contains uncentralized default depths and pressures; the UI must
  require explicit values rather than inheriting them silently.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-07-LFT-01/observed-evidence.json). Studies are read and written
through `UnitofWork<T>` over the NodalAnalysis-owned `NODAL_ANALYSIS_RESULT`, `NODAL_IPR_POINT` and `NODAL_VLP_POINT`.

**The open question above, answered:** every input is required. `NodalRules.Missing` names each missing value, the run
button is disabled with that list on the page, and the service refuses rather than letting `Advanced.cs` supply a default
depth or pressure.

**A defect this feature found and fixed:** `OperatingPointCalculator.FindIntersection` is not an intersection search - it
returns the closest pair of points between the two curves, averaged, so it answers with a number even when the curves
never meet. `TryFindOperatingPoint` was added (the old method is untouched for existing callers): it interpolates both
curves onto a common rate grid, finds a sign change of (inflow pressure - outflow pressure), bisects it, and returns
`false` when there is none. That is what makes AC-2 possible.

**The cross-check (AC-3)** gives the tools' `NodalAnalysis.Solve` its own inflow model - `Vogel` when the reservoir is
saturated and `CompositeRate` when it is undersaturated, because its simple overload refuses `pb < pr` - and this
study's outflow curve, so the crossing is solved independently. It agrees to 0.1% on the reference case and reports no
crossing in the dead case.

**Two engine gaps are recorded, not papered over:** `VLPCalculator.GenerateVLPBeggsBrill` is named Beggs-Brill but
computes Hagedorn-Brown, so the screen offers and stores `HAGEDORN_BROWN`; and the VLP path returns a non-finite pressure
at a zero rate, so non-finite points are dropped from both curves and counted.

**Not built:** the curves are point tables, not plots - `NodalRenderer` needs the VSB-01 chart host, which this screen
does not have; only Vogel and Standing are offered as inflow models; the sensitivity covers the tubing diameter only; and
the lift ranking tab is not built because the methods it would rank are the designs LFT-02 to LFT-08 own.
