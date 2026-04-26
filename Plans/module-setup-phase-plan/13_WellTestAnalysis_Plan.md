# Project 13 — Beep.OilandGas.WellTestAnalysis
## Module Setup & Best-Practice Audit Plan

### Purpose
Pressure-transient analysis (PTA) calculation library.  
Provides build-up, drawdown, derivative, gas-well, and type-curve matching algorithms aligned with
petroleum-engineering standards (Horner, MDH, Bourdet derivative, dimensionless type-curves).

---

## Sub-Phases

### SP-A: Project Structure Review
- **Status**: Complete
- **Folders**:
  - `Calculations/` — static algorithm classes: `BuildUpAnalysis`, `DrawdownAnalysis`,
    `DerivativeAnalysis`, `GasWellAnalysis`, `TypeCurveLibrary`.
  - `Services/` — `WellTestAnalysisService` (implements `IWellTestAnalysisService`).
  - `Constants/` — engineering constants (`WellTestType`, units, limits).
  - `Exceptions/` — `InvalidWellTestDataException`.
  - `Interaction/` — result interaction helpers.
  - `Rendering/` — plot/curve rendering utilities.
  - `Validation/` — `WellTestDataValidator`.
  - `WellTestAnalyzer.cs` — facade over the calculation statics.
- **Data models** live in `Beep.OilandGas.Models.Data.WellTestAnalysis` (separate project).

### SP-B: Data Class Audit
- **Status**: Complete
- **Key classes reviewed**:

| Class | Location | Shape | Notes |
|---|---|---|---|
| `WELL_TEST_DATA` | Models project | Extends `ModelEntityBase`; scalar PPDM columns + `List<double> Time`, `List<double> Pressure` | Collections are **Analysis Compatibility Properties** — explicitly not persisted. Class is used as calculation input DTO; never passed to `InsertAsync`/`UpdateAsync`. Acceptable. |
| `WELL_TEST_ANALYSIS_RESULT` | Models project | Extends `ModelEntityBase`; all scalar properties | Clean table class — persisted to `WELL_TEST_ANALYSIS_RESULT` table. |
| `PRESSURE_TIME_POINT` | Models project | Extends `ModelEntityBase`; all scalars | Clean — links individual time/pressure samples to `WELL_TEST_DATA_ID`. |

- **Verdict**: No rule violations that require fixes.  
  The `WELL_TEST_DATA.Time` and `.Pressure` properties are projection-only (never serialised to DB
  columns); the class is correctly treated as a calculation-input DTO by the service layer.

### SP-C: O&G Best-Practice Review
- **Status**: Complete
- **Pressure-Transient Analysis coverage**:
  - **Build-up tests**: Horner method, MDH method — standard PTA techniques. ✓
  - **Drawdown tests**: semi-log straight-line analysis. ✓
  - **Bourdet derivative**: dual-log plot with `dp'` smoothing parameter `L`. ✓
  - **Gas-well analysis**: pseudo-pressure (`m(p)`) transformation, Agarwal time. ✓
  - **Type-curve matching**: dimensionless curves for skin, storage, boundary detection. ✓
  - **Variable-rate analysis**: superposition/convolution for multi-rate tests. ✓
- **PTA parameters computed** (all present in `WELL_TEST_ANALYSIS_RESULT`):
  - Permeability (`PERMEABILITY`), skin (`SKIN_FACTOR`), initial reservoir pressure
    (`RESERVOIR_PRESSURE`), radius of investigation (`RADIUS_OF_INVESTIGATION`),
    productivity index (`PRODUCTIVITY_INDEX`), flow efficiency (`FLOW_EFFICIENCY`),
    damage ratio (`DAMAGE_RATIO`), identified reservoir model (`IDENTIFIED_MODEL`).
- **Validation**: `WellTestDataValidator` guards all inputs before calculations.
- **Exception handling**: `InvalidWellTestDataException` typed exceptions with clear messages.
- **Service pattern**: `WellTestAnalysisService` wraps static algorithms with async/await,
  structured logging, and argument validation per project conventions.

### SP-D: Build Validation
- **Status**: Complete ✓
- **Result**: `0 Error(s)  0 Warning(s)`

---

## Summary
No data-class changes required.  Library is well-structured and covers the standard PTA algorithm
suite used in petroleum engineering practice.
