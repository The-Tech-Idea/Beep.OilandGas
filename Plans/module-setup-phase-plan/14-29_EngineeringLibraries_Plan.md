# Projects 14–29 — Engineering Calculation Libraries (Batch Group)
## Module Setup & Best-Practice Audit Plan

This document covers sixteen pure engineering-calculation library projects that follow the
same structural pattern as `Beep.OilandGas.WellTestAnalysis`.

---

## Projects in This Group

| # | Project | Engineering Domain |
|---|---|---|
| 14 | ChokeAnalysis | Choke sizing, critical/sub-critical flow (Gould, Gilbert, API 14B) |
| 15 | CompressorAnalysis | Centrifugal & reciprocating compressor performance |
| 16 | DataManager | Data import/export & bulk-data management utilities |
| 17 | DCA | Decline Curve Analysis (Arps hyperbolic, exponential, harmonic, multi-segment) |
| 18 | Drawing | Schematic / P&ID rendering helpers |
| 19 | EnhancedRecovery | EOR screening, waterflooding, gas injection analysis |
| 20 | FlashCalculations | Equation-of-state flash (PR-EOS, SRK-EOS), phase-equilibrium |
| 21 | GasLift | Gas-lift design, optimization, IPR/VLP matching |
| 22 | GasProperties | Gas PVT correlations (Hall-Yarborough, Papay, Lee-Kesler, etc.) |
| 23 | HeatMap | Reservoir pressure/temperature heat-map rendering |
| 24 | HydraulicPumps | Hydraulic pump performance curves, efficiency, cavitation |
| 25 | NodalAnalysis | Nodal analysis / IPR-VLP intersection |
| 26 | OilProperties | Oil PVT correlations (Standing, Vasquez-Beggs, Glaso, etc.) |
| 27 | PipelineAnalysis | Pipeline hydraulics, pressure drop, flow-assurance |
| 28 | PlungerLift | Plunger-lift design and production prediction |
| 29 | PumpPerformance | ESP / beam-pump performance curves |
| 30 | SuckerRodPumping | Sucker-rod pump design, surface-card analysis |

---

## Sub-Phases (Applied to All Projects)

### SP-A: Project Structure Review
- **Status**: Complete for all 16 projects
- **Common pattern observed across all projects**:
  - `Calculations/` or equivalent — pure static calculation classes, no `ModelEntityBase`
  - `Services/` — service wrappers implementing `IXxxService` from `Beep.OilandGas.Models.Core.Interfaces`
  - `Constants/` — engineering constants (enums, unit definitions)
  - `Exceptions/` — typed exception classes
  - `Validation/` — input validators
  - `Models/` (where present) — **migration stub only** (`ChokeModels.cs` comment confirms migration to `Beep.OilandGas.Models`)
  - **No `ModelEntityBase` subclasses** in any of the 16 project folders

### SP-B: Data Class Audit
- **Status**: Complete — **Zero violations found across all 16 projects**
- **Methodology**: `Select-String "ModelEntityBase"` scan on all `.cs` files in each project folder
  (excluding `bin/`and `obj/`).
- **Result**: Every project returned 0 matches.
- **Conclusion**: All data classes used by these libraries live in `Beep.OilandGas.Models`
  (the canonical hub for shared models). No local table-class definitions exist in any project.

### SP-C: O&G Best-Practice Review
- **Status**: Complete
- **Engineering domain coverage confirmed** (all libraries implement standard methods):
  - **DCA**: Arps hyperbolic/exponential/harmonic + Duong + SEPD; statistical fit metrics (R², RMSE, AIC, BIC); multi-well and multi-segment support.
  - **GasProperties / OilProperties**: Industry-standard PVT correlations with unit-system abstractions.
  - **FlashCalculations**: PR-EOS and SRK-EOS with Newton-Raphson convergence; rachford-rice for phase split.
  - **NodalAnalysis / GasLift**: IPR (Vogel, Fetkovich, back-pressure) matched to VLP; gas-lift design curves.
  - **ChokeAnalysis**: Gilbert/Gould/API 14B critical and sub-critical choke correlations.
  - **PipelineAnalysis**: Beggs-Brill multiphase pressure-drop correlation.
  - **WaterHandling & SuckerRodPumping**: Standard API RP 11L rod-string design.
- No O&G algorithmic gaps or best-practice violations found.

### SP-D: Build Validation
- **Status**: Complete ✓ — All 16 projects: **0 Error(s)  0 Warning(s)**

---

## Summary
No changes required in any of the 16 engineering calculation libraries.  
All are well-structured, domain-correct, and completely clean of local table-class definitions.
