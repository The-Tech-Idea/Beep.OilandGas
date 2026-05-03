# Phase 1 — Calculations, validation, services, and PPDM alignment

## Objectives

1. **Single unit story** — Document **time** (hours vs seconds), **pressure** (psia), **rate** (STB/d, MSCF/d), **permeability** (md), and **radius** (ft) on public APIs; add conversion helpers only when dual units are intentionally supported.
2. **Analyzer vs service** — **`WellTestAnalyzer`** remains the pure static façade; **`WellTestAnalysisService`** adds **UWI**, **userId**, **timestamps**, logging, and correlation IDs on **`WELL_TEST_ANALYSIS_RESULT`**. Avoid duplicating formulas in the service.
3. **Validation** — **`WellTestDataValidator`** covers monotonic time, non-negative rates where required, minimum point count, and physically plausible **ct**, **φ**, **h**, **rw** bands; split **`partial`** files if the validator grows (e.g. `WellTestDataValidator.Gas.cs`).
4. **PPDM** — Map analysis outputs to fields on **`WELL_TEST_ANALYSIS_RESULT`**; document which PPDM tables are **read** vs **written** by API hosts (product decision).

## TODO checklist

| # | Task | Target |
|---|------|--------|
| 1.1 | Audit **`BuildUpAnalysis`** / **`DrawdownAnalysis`** for shared radial-flow assumptions and document **limits** (vertical well, constant μB, single-phase screening). | **Drafted** — see **Radial-flow assumptions** below; keep in sync with code. |
| 1.2 | Centralize smoothing / window parameters on **`WellTestConstants`**; no magic doubles in derivative path. | **Partial** — moving-average, flow-regime, **Bourdet** default **L** = **`DefaultDerivativeSmoothing`**; drawdown extended-analysis floors aligned; **`TypeCurveLibrary`** literals deferred. |
| 1.3 | **`WellTestAnalysisService`** — factory DI per repo pattern; ensure no swallowed exceptions without log (already logs + rethrow in places—normalize). | **Stub policy** — fake type-curve / empty persistence removed; **`NotImplementedException`** + **`ValidateTestDataAsync`** real outcome; Horner/MDH/drawdown/derivative wired. |
| 1.4 | Optional **partial** **`WellTestAnalyzer`**: `WellTestAnalyzer.BuildUp.cs`, `WellTestAnalyzer.Gas.cs` only if file grows. | Root |

## Verification criteria

- [ ] `dotnet build Beep.OilandGas.WellTestAnalysis/Beep.OilandGas.WellTestAnalysis.csproj` — 0 errors.
- [ ] No duplicate radial-flow constants outside **`WellTestConstants`** (allow small literals only where documented).

## Exit criteria

Phase 1 “done” when **1.1**, **1.2**, and validator audit (**WellTestDataValidator** vs **`WELL_TEST_DATA`** shape) are complete or deferred with rationale in **`MASTER-TODO-TRACKER.md`**.

---

## Radial-flow assumptions and screening limits (task 1.1)

The **Horner**, **MDH**, and constant-rate **drawdown** semi-log paths in this library implement classical **radial, infinite-acting** line-source style screening equations (field-style **162.6** oil coefficient, **log₁₀** time, **md / ft / psi / cp / STB/d** consistency as implemented in **`WellTestConstants`** and **`BuildUpAnalysis`** / **`DrawdownAnalysis`**).

**Intended screening use (not full simulator replacement)**

- **Single-phase oil** (or oil-like liquid) for the **Horner / MDH / drawdown** oil correlations: **`OIL_VISCOSITY`**, **`OIL_FORMATION_VOLUME_FACTOR`**, and **`TOTAL_COMPRESSIBILITY`** are treated as **slowly varying** inputs; **μ** and **B** are not recomputed vs pressure along the transient in these paths.
- **Vertical / radial symmetry** — no explicit **fracture linear** or **horizontal well** geometry in the permeability/skin equations; derivative **flow-regime** helpers are diagnostic hints only.
- **WBS / skin** — early-time **wellbore storage** and **damage** can distort semi-log straight lines; the code uses **heuristic windows** (middle/late segments) that may fail on noisy or non-standard data (**`AnalysisConvergenceException`**).
- **Gas** — **`AnalyzeGasBuildUp`** uses a simplified **m(p)** table and correlations; rates are treated as **Mscf/d** in the implemented formula comments; align field units with your data steward before production decisions.

**Units (current wire contract on `WELL_TEST_DATA` series)**

- **`Time`**: **hours**; **`Pressure`**: **psi**; **`FLOW_RATE`**: **STB/d** for oil Horner/MDH/drawdown (gas path documented as **Mscf/d** in **`GasWellAnalysis`**); **`FORMATION_THICKNESS`**: **ft**; **`WELLBORE_RADIUS`**: **ft**; **`PERMEABILITY`** on results: **md**.
