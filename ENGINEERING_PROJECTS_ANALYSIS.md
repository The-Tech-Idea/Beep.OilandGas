# Engineering & Calculation Projects — Comprehensive Analysis & Constants Refactoring Plan

**Generated:** 2026-05-03  
**Scope:** All Engineering/Calculation projects in Beep.OilandGas solution  
**Rule Applied:** `.cursor/rules/constants-and-reference-values.mdc`

---

## Executive Summary

This document describes each Engineering/Calculation project in the Beep.OilandGas solution, identifies hardcoded constants that violate the constants-and-reference-values rule, and provides a prioritized refactoring plan.

**Key Findings:**
- **15 projects** scanned
- **2 projects** have proper `*WellKnown` classes (WellTestAnalysis, CompressorAnalysis)
- **7 projects** have reference data organization (`*ReferenceSets` + `*ReferenceCodeSeed`)
- **5 projects** require HIGH priority refactoring
- **6 projects** require MEDIUM priority refactoring
- **Common pattern gap:** All projects using `FormatIdForTable` with string literals should centralize table names

---

## Project Descriptions & Constants Analysis

### 1. Beep.OilandGas.WellTestAnalysis

**Purpose:**  
Pressure transient analysis (PTA) library for well test analysis. Implements Horner, MDH, and drawdown semi-log methods with derivative analysis, reservoir model identification, and SkiaSharp-based visualization.

**Key Components:**
| Component | Location |
|-----------|----------|
| `WellTestAnalyzer` | `WellTestAnalyzer.cs` |
| `BuildUpAnalysis` | `Calculations/BuildUpAnalysis.cs` |
| `DrawdownAnalysis` | `Calculations/DrawdownAnalysis.cs` |
| `DerivativeAnalysis` | `Calculations/DerivativeAnalysis.cs` |
| `GasWellAnalysis` | `Calculations/GasWellAnalysis.cs` |
| `WellTestAnalysisService` | `Services/WellTestAnalysisService.cs` |
| `WellTestRenderer` | `Rendering/WellTestRenderer.cs` |

**Existing Constants:**
- `WellTestConstants` (`Constants/WellTestConstants.cs`) — physical constants (162.6 oil coefficient, permeability/skin multipliers, unit conversions)
- `WellTestAnalysisWellKnown` (in **`Beep.OilandGas.Models`**) — analysis classification tokens (`BUILDUP`, `DRAWDOWN`), method tokens (`HORNER`, `MDH`), PPDM test type fragments

**PPDM Reference Alignments:**
- `TEST_TYPE` → `R_WELL_TEST_TYPE` (seeded in data-management)
- `WELL_TEST_ANALYSIS_RESULT` table entity

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"BUILDUP"`, `"DRAWDOWN"` (in docs/comments) | `README.md` | 54 | Documentation references; already covered by `WellTestAnalysisWellKnown` |
| Magic number `162.6` | `Constants/WellTestConstants.cs` | (defined) | Already centralized — OK |

**Assessment:**  
**Well-structured.** Constants are already centralized in `WellTestConstants` and `WellTestAnalysisWellKnown`. No significant refactoring needed.

---

### 2. Beep.OilandGas.CompressorAnalysis

**Purpose:**  
Centrifugal and reciprocating compressor power, pressure, and efficiency calculations. Supports multi-stage, polytropic/adiabatic head, volumetric efficiency, and feasibility analysis.

**Key Components:**
| Component | Location |
|-----------|----------|
| `CentrifugalCompressorCalculator` | `Calculations/CentrifugalCompressorCalculator.cs` |
| `ReciprocatingCompressorCalculator` | `Calculations/ReciprocatingCompressorCalculator.cs` |
| `CompressorPressureCalculator` | `Calculations/CompressorPressureCalculator.cs` |
| `CompressorAnalysisService` | `Services/CompressorAnalysisService.cs` |
| `CompressorValidator` | `Validation/CompressorValidator.cs` |
| `CompressorAnalysisModule` | `Modules/CompressorAnalysisModule.cs` |

**Existing Constants:**
- `CompressorConstants` (`Data/Constants/CompressorConstants.cs`) — standard values, conversion factors
- `CompressorAnalysisWellKnown` (in **`Beep.OilandGas.Models`**) — `AnalysisType` (`POWER`, `PRESSURE`, `EFFICIENCY`), `CompressorType` (`CENTRIFUGAL`, `RECIPROCATING`)
- `CompressorAnalysisReferenceSets` (`Data/Constants/CompressorAnalysisReferenceSets.cs`) — reference set names
- `R_COMPRESSOR_ANALYSIS_REFERENCE_CODE` extension table

**PPDM Reference Alignments:**
- `COMPRESSOR_ANALYSIS_TYPE` → `R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`
- `COMPRESSOR_KIND` → `R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`
- Extension table: `R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"COMPRESSOR_DESIGN"`, `"COMPRESSOR_PERFORMANCE"`, etc. | `Services/CompressorAnalysisService.cs` | 119, 225, 317, 410, 507, 579 | Format ID table names — should use constants |
| `"COMPRESSOR_ANALYSIS"` (ModuleId) | `Modules/CompressorAnalysisModule.cs` | 32 | Should use a constant |

**Assessment:**  
**Good.** Has `CompressorAnalysisWellKnown` and `CompressorConstants`. Table name IDs in service could be centralized.

---

### 3. Beep.OilandGas.ChokeAnalysis

**Purpose:**  
Gas choke flow calculations using industry-standard correlations (Gilbert, Ros, Achong, Pilehvari, Sachdeva, Baxendell). Supports sonic/subsonic flow regime determination, choke sizing, and pressure calculations.

**Key Components:**
| Component | Location |
|-----------|----------|
| `GasChokeCalculator` | `Calculations/GasChokeCalculator.cs` |
| `ChokeAnalysisService` | `Services/ChokeAnalysisService.cs` |
| `ChokeAnalysisService.Advanced` | `Services/ChokeAnalysisService.Advanced.cs` |
| `ChokeValidator` | `Validation/ChokeValidator.cs` |
| `ChokeAnalysisModule` | `Modules/ChokeAnalysisModule.cs` |
| `ChokeConstants` | `Constants/ChokeConstants.cs` |

**Existing Constants:**
- `ChokeConstants` (`Constants/ChokeConstants.cs`) — choke calculation constants
- `ChokeAnalysisReferenceCodes` (`Constants/ChokeAnalysisReferenceCodes.cs`) — status codes (`COMPLETED`, `FAILED`, `RUNNING`), choke types (`BEAN`, `ADJUSTABLE`, `POSITIVE`), correlation methods, flow regimes (`SONIC`, `SUBSONIC`)
- `ChokeAnalysisReferenceSets` (`Constants/ChokeAnalysisReferenceSets.cs`) — reference set names
- `ChokeFlowRegime` (`Constants/ChokeFlowRegime.cs`) — enum
- `ChokeType` (`Constants/ChokeType.cs`) — enum

**PPDM Reference Alignments:**
- `CHOKE_TYPE` → `R_CHOKE_ANALYSIS_REFERENCE_CODE`
- `CHOKE_CORRELATION_METHOD` → `R_CHOKE_ANALYSIS_REFERENCE_CODE`
- `CHOKE_FLOW_REGIME_LABEL` → `R_CHOKE_ANALYSIS_REFERENCE_CODE`
- Extension table: `R_CHOKE_ANALYSIS_REFERENCE_CODE`

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"ESP"` (lift system type check) | `Services/ChokeAnalysisService.Advanced.cs` | 372, 641 | Magic string — should be in a `LiftSystemType` constant class |
| `"GasLift"` (lift system type check) | `Services/ChokeAnalysisService.Advanced.cs` | 641 | Magic string — same as above |
| `1.1m`, `0.95m` (choke adjustment factors) | `Services/ChokeAnalysisService.Advanced.cs` | 366 | Magic numbers — should be in `ChokeConstants` |
| `0.2m`, `1.5m` (choke size bounds) | `Services/ChokeAnalysisService.Advanced.cs` | 367 | Magic numbers — should be in `ChokeConstants` |
| `5000` (discharge pressure limit) | `Services/ChokeAnalysisService.Advanced.cs` | 374 | Magic number — should be in constants |
| `3000` (GOR limit) | `Services/ChokeAnalysisService.Advanced.cs` | 375 | Magic number — should be in constants |
| `70m`, `60m`, `50m` (base efficiency values) | `Services/ChokeAnalysisService.Advanced.cs` | 641 | Magic numbers — should be in constants |
| `0.7m`, `500m` (erosion formula factors) | `Services/ChokeAnalysisService.Advanced.cs` | 636 | Magic numbers — should be in constants |
| `0.0001m`, `100m` (settling velocity factors) | `Services/ChokeAnalysisService.Advanced.cs` | 648 | Magic numbers — should be in constants |
| `"CHOKE_ANALYSIS"` (ModuleId) | `Modules/ChokeAnalysisModule.cs` | 56 | Should use a constant |

**Assessment:**  
**Needs refactoring.** `ChokeAnalysisService.Advanced.cs` has numerous magic strings and numbers that should be centralized. The reference codes are well-organized.

---

### 4. Beep.OilandGas.NodalAnalysis

**Purpose:**  
IPR/VLP nodal analysis engine. Supports Vogel, Fetkovich, Wiggins, composite, and gas-well IPR correlations; Hagedorn-Brown, Beggs-Brill, Duns-Ros, Orkiszewski, Aziz-Govier-Fogarasi VLP correlations. Full service layer with persistence, optimization, sensitivity, artificial lift ranking, and diagnostics.

**Key Components:**
| Component | Location |
|-----------|----------|
| `NodalAnalyzer` | `NodalAnalyzer.cs` |
| `IPRCalculator` | `Calculations/IPRCalculator.cs` |
| `VLPCalculator` | `Calculations/VLPCalculator.cs` |
| `NodalCalculator` | `Calculations/NodalCalculator.cs` |
| `OperatingPointCalculator` | `Calculations/OperatingPointCalculator.cs` |
| `NodalAnalysisService` (partial) | `Services/NodalAnalysisService.cs`, `.Advanced.cs`, `.SensitivityScenarios.cs`, `.ArtificialLiftScoring.cs` |
| `NodalRenderer` | `Rendering/NodalRenderer.cs` |
| `NodalAnalysisModule` | `Modules/NodalAnalysisModule.cs` |

**Existing Constants:**
- `NodalConstants` (`Constants/NodalConstants.cs`) — calculation constants
- `NodalAnalysisReferenceCodes` (`Constants/NodalAnalysisReferenceCodes.cs`) — reference codes
- `NodalAnalysisReferenceSets` (`Constants/NodalAnalysisReferenceSets.cs`) — `NODAL_ANALYSIS_STATUS`, `NODAL_OPTIMIZATION_TYPE`, `NODAL_LIFT_METHOD`, `NODAL_DIAGNOSIS_STATUS`
- `NodalArtificialLiftPolicyConstants` (`Constants/NodalArtificialLiftPolicyConstants.cs`)
- `NodalAnalysisReferenceCodeSeed` (`Constants/NodalAnalysisReferenceCodeSeed.cs`) — LOV seed rows (ESP, GAS_LIFT, SUCKER_ROD, PLUNGER_LIFT, HYDRAULIC_JET, PCP)

**PPDM Reference Alignments:**
- `NODAL_ANALYSIS_STATUS` → `R_NODAL_ANALYSIS_REFERENCE_CODE`
- `NODAL_LIFT_METHOD` → `R_NODAL_ANALYSIS_REFERENCE_CODE`
- Extension table: `R_NODAL_ANALYSIS_REFERENCE_CODE`

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"RUNNING"`, `"SUCCESS"`, `"NO_INTERSECTION"` (status) | `Services/NodalAnalysisService.Advanced.cs` | 27, 63, 67 | Magic strings — should use `NodalAnalysisReferenceCodes` |
| `"NODAL_ANALYSIS"`, `"OPTIMIZATION"`, `"PERF_MATCH"`, etc. (table names) | `Services/NodalAnalysisService.cs` | 90, 123, 332, 369, 420, 464, 531, 589 | Format ID table names — should be centralized |
| `3000m`, `200m`, `8000m`, `1000m` (default values) | `Services/NodalAnalysisService.Advanced.cs` | 33, 52, 53, 46 | Magic numbers — should be in `NodalConstants` |
| `"IPR"`, `"VLP"` (curve labels) | `Rendering/NodalRenderer.cs` | 410, 413 | Minor — acceptable as display labels |
| `"NODAL_ANALYSIS"` (ModuleId) | `Modules/NodalAnalysisModule.cs` | 33 | Should use a constant |

**Assessment:**  
**Moderate refactoring needed.** Status strings in `NodalAnalysisService.Advanced.cs` should use existing reference codes. Default fallback values should be in `NodalConstants`.

---

### 5. Beep.OilandGas.GasLift

**Purpose:**  
Gas lift analysis and design: potential analysis, valve design (US/SI units), valve spacing (equal pressure drop, equal depth), performance curves, and system optimization.

**Key Components:**
| Component | Location |
|-----------|----------|
| `GasLiftPotentialCalculator` | `Calculations/GasLiftPotentialCalculator.cs` |
| `GasLiftValveDesignCalculator` | `Calculations/GasLiftValveDesignCalculator.cs` |
| `GasLiftValveSpacingCalculator` | `Calculations/GasLiftValveSpacingCalculator.cs` |
| `GasLiftService` (partial) | `Services/GasLiftService.cs`, `.Advanced.cs`, `.WireAsync.cs` |
| `GasLiftValidator` | `Validation/GasLiftValidator.cs` |
| `GasLiftModule` | `Modules/GasLiftModule.cs` |

**Existing Constants:**
- `GasLiftConstants` (`Constants/GasLiftConstants.cs`) — calculation constants, guardrails (min/max injection, valve count, depth, spacing), standard port sizes
- `GasLiftDesignLimitMessages` (`Constants/GasLiftDesignLimitMessages.cs`) — maps reference codes to user-facing validation text
- `GasLiftReferenceSets` (`Data/Constants/GasLiftReferenceSets.cs`) — reference set names (port sizes, operating mode, design method, valve service, injection gas source, design limits)
- `GasLiftReferenceCodeSeed` (`Data/Constants/GasLiftReferenceCodeSeed.cs`) — LOV seed rows

**PPDM Reference Alignments:**
- `GAS_LIFT_PORT_SIZE_IN`, `GAS_LIFT_OPERATING_MODE`, `GAS_LIFT_DESIGN_METHOD`, `GAS_LIFT_VALVE_SERVICE`, `GAS_LIFT_INJECTION_SOURCE`, `GAS_LIFT_DESIGN_LIMIT` → `R_GAS_LIFT_REFERENCE_CODE`
- Extension table: `R_GAS_LIFT_REFERENCE_CODE`

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"MIN_INJECTION_MSCFD"`, `"MAX_INJECTION_MSCFD"`, etc. | `Constants/GasLiftDesignLimitMessages.cs` | 19-29 | Already aligned with seeded reference codes — acceptable pattern |
| `"GAS_LIFT"` (ModuleId) | `Modules/GasLiftModule.cs` | 47 | Should use a constant |
| `"GAS_LIFT_PERFORMANCE"`, `"GAS_LIFT_DESIGN"` (table names) | `Services/GasLiftService.cs` | 361, 486, 491, 528 | Format ID table names — should be centralized |

**Assessment:**  
**Well-structured.** Excellent reference data organization with `GasLiftReferenceSets`, `GasLiftReferenceCodeSeed`, and `GasLiftDesignLimitMessages`. Minor table name constants could be added.

---

### 6. Beep.OilandGas.PipelineAnalysis

**Purpose:**  
Gas and liquid pipeline capacity and flow analysis. Weymouth equation for gas, Darcy-Weisbach for liquid. Includes friction factor, Reynolds number, flow regime, erosion prediction, leak detection, and reporting.

**Key Components:**
| Component | Location |
|-----------|----------|
| `PipelineCapacityCalculator` | `Calculations/PipelineCapacityCalculator.cs` |
| `PipelineFlowCalculator` | `Calculations/PipelineFlowCalculator.cs` |
| `PipelineCalculator` | `Calculations/PipelineCalculator.cs` |
| `ErosionPrediction` | `Calculations/ErosionPrediction.cs` |
| `PipelineAnalysisService` (partial) | `Services/PipelineAnalysisService.cs`, `.Advanced.cs`, `.Reporting.cs`, `.LeakDetection.cs`, `.DataManagement.cs` |
| `PipelineValidator` | `Validation/PipelineValidator.cs` |

**Existing Constants:**
- `PipelineConstants` (`Constants/PipelineConstants.cs`) — standard values, roughness values, conversion factors

**PPDM Reference Alignments:**
- No dedicated extension table or module setup identified. Uses `PIPELINE_FLOW_ANALYSIS_RESULT` projection.

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"GAS_TRANSMISSION"` (flow regime) | `Services/PipelineAnalysisService.Advanced.cs` | 30 | Magic string — should be in a `FlowRegime` constant class |
| `"SUCCESS"` (status) | `Services/PipelineAnalysisService.Advanced.cs` | 31, 71 | Magic string — should be in constants |
| `"TURBULENT"`, `"LAMINAR"` (flow regime) | `Services/PipelineAnalysisService.Advanced.cs` | 70 | Magic strings — should be in constants |
| `"LOW"`, `"HIGH"` (risk levels) | `Calculations/ErosionPrediction.cs` | 268 | Magic strings — should be in constants |
| `"CSV"`, `"JSON"` (export formats) | `Services/PipelineAnalysisService.Reporting.cs` | 176, 194 | Magic strings — should be in constants |
| `"SCFH"` (default unit) | `Services/PipelineAnalysisService.LeakDetection.cs` | 195 | Magic string — should be in constants |
| `"PDF"` (default format) | `Services/IPipelineAnalysisService.cs` | 1238 | Magic string — should be in constants |
| `"PIPELINE_ANALYSIS"` (table name) | `Services/PipelineAnalysisService.cs` | 68 | Format ID table name — should be centralized |
| `4000` (Reynolds number threshold) | `Services/PipelineAnalysisService.Advanced.cs` | 70 | Magic number — should be in `PipelineConstants` |

**Assessment:**  
**Needs moderate refactoring.** Missing a dedicated `FlowRegime` constant class and export format constants. No module setup or extension table — consider adding if pipeline analysis needs persistence.

---

### 7. Beep.OilandGas.PumpPerformance

**Purpose:**  
Pump performance screening: H-Q curves, efficiency, power, system curves, affinity laws, NPSH, viscosity corrections, ESP design helpers, pump-type abstractions (centrifugal, PD, ESP, jet), and SkiaSharp rendering.

**Key Components:**
| Component | Location |
|-----------|----------|
| `PumpPerformanceCalc` | `PumpPerformanceCalc.cs` |
| `HeadQuantityCalculations` | `Calculations/HeadQuantityCalculations.cs` |
| `EfficiencyCalculations` | `Calculations/EfficiencyCalculations.cs` |
| `PowerCalculations` | `Calculations/PowerCalculations.cs` |
| `AffinityLaws` | `Calculations/AffinityLaws.cs` |
| `NPSHCalculations` | `Calculations/NPSHCalculations.cs` |
| `ESPDesignCalculator` | `Calculations/ESPDesignCalculator.cs` |
| `PumpPerformanceService` | `Services/PumpPerformanceService.cs` |
| `PumpDataValidator` | `Validation/PumpDataValidator.cs` |
| Pump types | `PumpTypes/` (Centrifugal, PositiveDisplacement, ESP, Jet) |

**Existing Constants:**
- `PumpConstants` (`Constants/PumpConstants.cs`) — conversion factors, default NPSH safety margin (2 ft), unit conversions

**PPDM Reference Alignments:**
- ESP DTOs in `Beep.OilandGas.Models.Data.PumpPerformance`
- No dedicated module setup or extension table

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"NPSH"` (validation context) | `Validation/PumpDataValidator.Npsh.cs` | 32 | Minor — acceptable as context label |
| `1.0` (default specific gravity) | `README.md` | 97 | Default parameter — acceptable |

**Assessment:**  
**Well-structured.** Constants are centralized in `PumpConstants`. No significant magic strings or numbers identified.

---

### 8. Beep.OilandGas.HydraulicPumps

**Purpose:**  
Hydraulic jet and hydraulic piston pump performance calculations. Shared screening math for liquid density, tubing friction, discharge pressure. System efficiency clamped to [0,1].

**Key Components:**
| Component | Location |
|-----------|----------|
| `HydraulicJetPumpCalculator` | `Calculations/HydraulicJetPumpCalculator.cs` |
| `HydraulicPistonPumpCalculator` | `Calculations/HydraulicPistonPumpCalculator.cs` |
| `HydraulicPumpSharedCalculations` | `Calculations/HydraulicPumpSharedCalculations.cs` |
| `HydraulicPumpService` (partial) | `Services/HydraulicPumpService.cs`, `.Advanced.cs`, `.ModelsCoreImpl.cs` |
| `HydraulicPumpValidator` | `Validation/HydraulicPumpValidator.cs` |

**Existing Constants:**
- `HydraulicPumpConstants` (`Constants/HydraulicPumpConstants.cs`) — physical constants

**PPDM Reference Alignments:**
- Wire types in `Beep.OilandGas.Models.Data.HydraulicPumps`
- No dedicated module setup or extension table

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"WARNING_CAVITATION"`, `"SUCCESS"` (status) | `Services/HydraulicPumpService.Advanced.cs` | 42, 47 | Magic strings — should be in constants |
| `"JET"` (pump type default) | `Services/HydraulicPumpService.Advanced.cs` | 60 | Magic string — should be in constants |
| `"DRAFT"` (design status) | `Services/HydraulicPumpService.Advanced.cs` | 73 | Magic string — should be in constants |
| `"PUMP_DESIGN"`, `"PUMP"`, `"ROD_DESIGN"` (table names) | `Services/HydraulicPumpService.cs` | 52, 61, 97 | Format ID table names — should be centralized |
| `"CSV"` (export format) | `Services/HydraulicPumpService.cs` | 470 | Magic string — should be in constants |
| `"PDF"` (default format) | `Services/IHydraulicPumpService.cs` | 1357 | Magic string — should be in constants |
| `50m` (NPSH required default) | `Services/HydraulicPumpService.Advanced.cs` | 39 | Magic number — should be in `HydraulicPumpConstants` |

**Assessment:**  
**Needs moderate refactoring.** Status strings and pump type defaults should be centralized. Missing a module setup.

---

### 9. Beep.OilandGas.SuckerRodPumping

**Purpose:**  
Sucker rod pumping analysis: load analysis (peak/min loads, rod string weight, fluid load, dynamic load, stress, safety factor), production rate, power requirements, pump card generation, and rod string optimization.

**Key Components:**
| Component | Location |
|-----------|----------|
| `SuckerRodLoadCalculator` | `Calculations/SuckerRodLoadCalculator.cs` |
| `SuckerRodFlowRatePowerCalculator` | `Calculations/SuckerRodFlowRatePowerCalculator.cs` |
| `RodStringOptimization` | `Calculations/RodStringOptimization.cs` |
| `FatigueAnalysis` | `Calculations/FatigueAnalysis.cs` |
| `SuckerRodPumpingService` | `Services/SuckerRodPumpingService.cs` |
| `SuckerRodValidator` | `Validation/SuckerRodValidator.cs` |

**Existing Constants:**
- `SuckerRodConstants` (`Constants/SuckerRodConstants.cs`) — standard values, conversion factors

**PPDM Reference Alignments:**
- Uses `WELL_ACTIVITY` table for persistence (not extension)

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"C"`, `"D"`, `"HL"`, `"HY"`, `"AUTO"` (API rod grades) | `Calculations/RodStringOptimization.cs` | 57-60, 85, 94, 101, 111, 116 | Magic strings — should be in a `RodGrade` constant class |
| `"C"`, `"D"`, `"HL"`, `"HY"` (fatigue analysis materials) | `Calculations/FatigueAnalysis.cs` | 66-70 | Magic strings — should use same `RodGrade` constants |
| `"BEEP"` (source) | `Services/SuckerRodPumpingService.cs` | 281 | Magic string — should be in constants |
| `"SUCKER_ROD_PUMP"` (activity type) | `Services/SuckerRodPumpingService.cs` | 282 | Magic string — should be in constants |
| `"SRP_DESIGN"` (table name) | `Services/SuckerRodPumpingService.cs` | 68 | Format ID table name — should be centralized |
| `55.0` (default fluid density) | `Calculations/RodStringOptimization.cs` | 92 | Magic number — should be in `SuckerRodConstants` |
| `490` (steel density, lb/ft³) | `Calculations/RodStringOptimization.cs` | 63 | Magic number — should be in `SuckerRodConstants` |

**Assessment:**  
**Needs moderate refactoring.** Rod grade strings (`"C"`, `"D"`, `"HL"`, `"HY"`) should be centralized in a `RodGrade` class. Activity type strings should be constants.

---

### 10. Beep.OilandGas.PlungerLift

**Purpose:**  
Plunger lift analysis: cycle analysis (fall, rise, shut-in times), gas requirements, production rate predictions, system feasibility, and performance optimization.

**Key Components:**
| Component | Location |
|-----------|----------|
| `PlungerLiftCalculator` | `Calculations/PlungerLiftCalculator.cs` |
| `PlungerLiftService` (partial) | `Services/PlungerLiftService.cs`, `.Advanced.cs` |
| `PlungerLiftValidator` | `Validation/PlungerLiftValidator.cs` |

**Existing Constants:**
- `PlungerLiftConstants` (`Constants/PlungerLiftConstants.cs`) — standard values and limits

**PPDM Reference Alignments:**
- Uses `WELL_ACTIVITY` table for persistence

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"BAR"`, `"PAD"`, `"BRUSH"`, `"CONTINUOUS"` (plunger types) | `Calculations/PlungerLiftCalculator.cs` | 45-48 | Magic strings — should be in a `PlungerType` constant class |
| `1.5m`, `15.0m`, `8.0m`, `10.0m`, `12.0m` (fall velocities) | `Calculations/PlungerLiftCalculator.cs` | 40-48 | Magic numbers — should be in `PlungerLiftConstants` |
| `12.5m` (default rise velocity) | `Calculations/PlungerLiftCalculator.cs` | 60 | Magic number — should be in `PlungerLiftConstants` |
| `"BEEP"` (source) | `Services/PlungerLiftService.cs` | 856, 954 | Magic string — should be in constants |
| `"PLUNGER_LIFT"`, `"PLUNGER_LIFT_PERF"` (activity types) | `Services/PlungerLiftService.cs` | 857, 886, 955, 983 | Magic strings — should be in constants |
| `"CSV"` (export format) | `Services/PlungerLiftService.cs` | 1038 | Magic string — should be in constants |
| `"PDF"` (default format) | `Services/IPlungerLiftService.cs` | 908 | Magic string — should be in constants |

**Assessment:**  
**Needs moderate refactoring.** Plunger type strings are the most critical to centralize. Activity type strings should also be constants.

---

### 11. Beep.OilandGas.EnhancedRecovery

**Purpose:**  
Enhanced Oil Recovery (EOR) services: PDEN-backed scheme and injection operations, economics, and screening-level analytics for waterflooding, gas injection, chemical EOR, and thermal recovery.

**Key Components:**
| Component | Location |
|-----------|----------|
| `EnhancedRecoveryService` (partial) | `Services/EnhancedRecoveryService.cs`, `.Advanced.cs` |
| `EnhancedRecoveryModule` | `Modules/EnhancedRecoveryModule.cs` |
| `EnhancedRecoveryController` | (in `ApiService`) |

**Existing Constants:**
- `EnhancedRecoveryConstants` (`Data/Constants/EnhancedRecoveryConstants.cs`) — EOR constants
- `EnhancedRecoveryReferenceSets` (`Data/Constants/EnhancedRecoveryReferenceSets.cs`) — reference set names
- `EnhancedRecoveryReferenceCodeSeed` (`Data/Constants/EnhancedRecoveryReferenceCodeSeed.cs`) — LOV seed rows

**PPDM Reference Alignments:**
- `PDEN` with `ENHANCED_RECOVERY_TYPE`, `PDEN_SUBTYPE`
- `PDEN_FLOW_MEASUREMENT` for injection rates
- Extension table: `R_ENHANCED_RECOVERY_REFERENCE_CODE`

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"WATER"`, `"FLOOD"`, `"CO2"`, `"MISCIBLE"`, `"GAS_INJECTION"`, `"POLYMER"`, `"ASP"`, `"CHEMICAL"`, `"STEAM"`, `"THERMAL"`, `"INJECTION"` (EOR type checks) | `Services/EnhancedRecoveryService.cs` | 49-65 | Magic strings — should use `EnhancedRecoveryReferenceSets` constants |
| `"WATER_FLOOD"`, `"GAS_INJECTION"` (PDEN subtype filters) | `Services/EnhancedRecoveryService.cs` | 453, 479, 495 | Magic strings — should use reference code constants |
| `"SURFACTANT"`, `"POLYMER"`, `"ALKALI"` (chemical types) | `Services/EnhancedRecoveryService.Advanced.cs` | 714-756 | Magic strings in switch statements — should be in a `ChemicalType` constant class |
| `0.001`, `0.5`, `0.01`, `0.1` (IFT reduction factors) | `Services/EnhancedRecoveryService.Advanced.cs` | 714-717 | Magic numbers — should be in `EnhancedRecoveryConstants` |
| `0.20`, `0.10`, `0.15`, `0.08` (recovery increments) | `Services/EnhancedRecoveryService.Advanced.cs` | 726-729 | Magic numbers — should be in `EnhancedRecoveryConstants` |
| `15.0`, `8.0`, `5.0`, `10.0` (chemical costs $/bbl) | `Services/EnhancedRecoveryService.Advanced.cs` | 739-742 | Magic numbers — should be in `EnhancedRecoveryConstants` |
| `20m`, `25m`, `18m`, `22m`, `30m`, `15m` (recovery factor defaults) | `Services/EnhancedRecoveryService.cs` | 46-66 | Magic numbers — should be in `EnhancedRecoveryConstants` |
| `"ENHANCED_RECOVERY"` (ModuleId) | `Modules/EnhancedRecoveryModule.cs` | (defined) | Should use a constant |

**Assessment:**  
**Needs significant refactoring.** The EOR type matching logic uses substring checks against magic strings. Chemical type switch statements should use constants. Recovery factor defaults and economic parameters should be centralized.

---

### 12. Beep.OilandGas.ProductionForecasting

**Purpose:**  
Production forecasting engine: decline-curve analysis (exponential, harmonic, hyperbolic, modified-hyperbolic), deterministic forecasts, history-based parameter fitting from PPDM, and PPDM persistence.

**Key Components:**
| Component | Location |
|-----------|----------|
| `DeclineForecast` | `Calculations/DeclineForecast.cs` |
| `PseudoSteadyStateForecast` | `Calculations/PseudoSteadyStateForecast.cs` |
| `TransientForecast` | `Calculations/TransientForecast.cs` |
| `GasWellForecast` | `Calculations/GasWellForecast.cs` |
| `ProductionForecastingService` (partial) | `Services/ProductionForecastingService.cs`, `.ForecastGeneration.cs`, `.DCA.cs`, `.Economics.cs` |
| `ProductionHistoryLoader` | `Services/ProductionHistoryLoader.cs` |
| `ProductionForecastResultMapper` | `Services/ProductionForecastResultMapper.cs` |
| `DCAManager`, `DCAAnalysisService` | `DCA/` |
| `ProductionForecastingModule` | `Modules/ProductionForecastingModule.cs` |

**Existing Constants:**
- `ForecastAlgorithmConstants` (`Constants/ForecastAlgorithmConstants.cs`) — b bounds, default qi/Di/b, Dlim, history point minimums
- `ForecastConstants` (`Constants/ForecastConstants.cs`) — forecasting constants
- `ProductionForecastingReferenceCodes` (`Constants/ProductionForecastingReferenceCodes.cs`) — reference codes
- `ProductionForecastingReferenceSets` (`Constants/ProductionForecastingReferenceSets.cs`) — `PRODUCTION_FORECAST_METHOD`, `PRODUCTION_FORECAST_RUN_STATUS`, `PRODUCTION_FORECAST_RISK_RATING`
- `ProductionForecastingReferenceCodeSeed` (`Constants/ProductionForecastingReferenceCodeSeed.cs`) — LOV seed rows

**PPDM Reference Alignments:**
- `PDEN_WELL`, `PDEN_VOL_SUMMARY` for history
- `PRODUCTION_FORECAST`, `PRODUCTION_FORECAST_POINT` for persistence
- Extension table: `R_PRODUCTION_FORECASTING_REFERENCE_CODE`

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"CSV"` (export format default) | `Services/ProductionForecastingService.cs` | 215 | Magic string — should be in constants |
| `"DCA"` (method ID) | `Services/ProductionForecastingService.cs` | 250 | Magic string — should use reference code constant |
| `"SYSTEM"` (default user) | `README.md` | 234 | Magic string — should be in constants |
| `"RISK_ANALYSIS"`, `"FORECAST_REPORT"`, `"FORECAST_COMPARE"`, etc. (table names) | `Services/ProductionForecastingService.cs` | 92, 225, 238 | Format ID table names — should be centralized |
| `"PRODUCTION_FORECASTING"` (ModuleId) | `Modules/ProductionForecastingModule.cs` | 30 | Should use a constant |

**Assessment:**  
**Well-structured.** Good constants organization with `ForecastAlgorithmConstants` and reference data. Minor table name constants could be added.

---

### 13. Beep.OilandGas.FlashCalculations

**Purpose:**  
Phase equilibrium and flash calculations: isothermal flash (Wilson K + Rachford-Rice), phase composition, phase property calculations, cubic EOS (PR/SRK) helpers, multi-stage separator flash, and phase envelope support.

**Key Components:**
| Component | Location |
|-----------|----------|
| `FlashCalculator` | `Calculations/FlashCalculator.cs` |
| `AdvancedEOS` | `Calculations/AdvancedEOS/` |
| `MultiComponentFlash` | `Calculations/MultiComponentFlash.cs` |
| `PhaseEnvelope` | `Calculations/PhaseEnvelope.cs` |
| `FlashFeedCatalogMerge` | `Services/FlashFeedCatalogMerge.cs` |
| `FlashCalculationService` (partial) | `Services/FlashCalculationService.cs`, `.Advanced.cs` |
| `FlashValidator` | `Validation/FlashValidator.cs` |
| `FlashCalculationsModule` | `Modules/FlashCalculationsModule.cs` |

**Existing Constants:**
- `FlashConstants` (`Constants/FlashConstants.cs`) — convergence parameters, maximum iterations, tolerance
- `FlashEquationOfStateMapping` (`Constants/FlashEquationOfStateMapping.cs`) — EOS model mapping (`PR`, `SRK`, `SRK_MODIFIED`, `IDEAL_K`)
- `FlashReferenceSets` (`Data/Constants/FlashReferenceSets.cs`) — reference set names
- `FlashReferenceCodeSeed` (`Data/Constants/FlashReferenceCodeSeed.cs`) — LOV seed rows

**PPDM Reference Alignments:**
- Extension table: `R_FLASH_CALCULATION_REFERENCE_CODE`
- EOS models aligned with `FLASH_EOS_MODEL` seeded codes

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"PT_FLASH_RIGOROUS"` (calculation type) | `Services/FlashCalculationService.Advanced.cs` | 37 | Magic string — should use reference code constant |
| `"SUCCESS"`, `"CONVERGENCE_FAILED"`, `"FAILED"` (status) | `Services/FlashCalculationService.Advanced.cs` | 113, 124 | Magic strings — should use reference code constants |
| `"PR"`, `"SRK"`, `"SRK_MODIFIED"`, `"IDEAL_K"` (EOS codes) | `Constants/FlashEquationOfStateMapping.cs` | 30-48 | Already centralized — OK |
| `"FLASH_CALCULATIONS"` (ModuleId) | `Modules/FlashCalculationsModule.cs` | 29 | Should use a constant |
| `"FLASH_CALCULATION"`, `"PVT_ENVELOPE"`, `"BUBBLE_POINT"`, etc. (table names) | `Services/FlashCalculationService.cs` | 35, 186, 246, 310, 374, 435, 498, 548 | Format ID table names — should be centralized |

**Assessment:**  
**Well-structured.** EOS mapping and reference data are well-organized. Status strings in the advanced service should use reference codes.

---

### 14. Beep.OilandGas.OilProperties

**Purpose:**  
Black-oil screening correlations: Standing bubble point/solution GOR/oil FVF, Beggs-Robinson dead and saturated oil viscosity. Persistence helpers for `OIL_COMPOSITION`/`OIL_PROPERTY_RESULT`.

**Key Components:**
| Component | Location |
|-----------|----------|
| `OilPropertyCalculator` | `Calculations/OilPropertyCalculator.cs` |
| `BlackOilScreening` | `Calculations/BlackOilScreening.cs` |
| `OilPropertiesService` (partial) | `Services/OilPropertiesService.cs`, `.Advanced.cs` |
| `OilPropertyValidator` | `Validation/OilPropertyValidator.cs` |

**Existing Constants:**
- `OilPropertyConstants` (`Constants/OilPropertyConstants.cs`) — default gas specific gravity (0.65), correlation defaults
- `OilPropertyUnits` (`Constants/OilPropertyUnits.cs`) — unit conversion (Rankine ↔ Fahrenheit)

**PPDM Reference Alignments:**
- `OIL_COMPOSITION`, `OIL_PROPERTY_RESULT` tables
- No dedicated module setup or extension table

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"STANDING_BEGGS"` (correlation method) | `Services/OilPropertiesService.cs` | 160, `Advanced.cs` | 36 | Magic string — should be in a `CorrelationMethod` constant class |
| `"OIL_COMPOSITION"`, `"OIL_PROPERTY"` (table names) | `Services/OilPropertiesService.cs` | 150, 179, 184, 250, 290, 328, 333 | Format ID table names — should be centralized |
| `"PHASE_DIAGRAM"`, `"COMPRESSIBILITY"`, `"IFT_ANALYSIS"`, etc. (table names) | `Services/OilPropertiesService.cs` | 377, 446, 490, 521, 552, 636, 693 | Format ID table names — should be centralized |
| `0.65` (default gas gravity) | `Constants/OilPropertyConstants.cs` | (defined) | Already centralized — OK |

**Assessment:**  
**Moderate refactoring needed.** Correlation method strings should be centralized. Table name IDs should use constants.

---

### 15. Beep.OilandGas.GasProperties

**Purpose:**  
Gas property calculations: Z-factor (Brill-Beggs, Hall-Yarborough, Dranchuk-Abu-Kassem/Standing-Katz), gas viscosity (Carr-Kobayashi-Burrows, Lee-Gonzalez-Eakin), pseudo-pressure integration, and average properties.

**Key Components:**
| Component | Location |
|-----------|----------|
| `ZFactorCalculator` | `Calculations/ZFactorCalculator.cs` |
| `GasViscosityCalculator` | `Calculations/GasViscosityCalculator.cs` |
| `PseudoPressureCalculator` | `Calculations/PseudoPressureCalculator.cs` |
| `AveragePropertiesCalculator` | `Calculations/AveragePropertiesCalculator.cs` |
| `GasPropertiesService` | `Services/GasPropertiesService.cs` |
| `GasPropertiesValidator` | `Validation/GasPropertiesValidator.cs` |

**Existing Constants:**
- `GasPropertiesConstants` (`Constants/GasPropertiesConstants.cs`) — standard pressure (14.696 psia), standard temperature (519.67 R), universal gas constant (10.7316)

**PPDM Reference Alignments:**
- `GAS_COMPOSITION`, `GAS_COMPOSITION_COMPONENT` tables
- No dedicated module setup or extension table

**Hardcoded Values Needing Refactoring:**
| Value | File | Line | Issue |
|-------|------|------|-------|
| `"GAS_COMPOSITION"`, `"GAS_COMPONENT"`, etc. (table names) | `Services/GasPropertiesService.cs` | 107, 112, 170, 186, 222, 252 | Format ID table names — should be centralized |
| `"GAS_VISCOSITY"`, `"GAS_COMPRESSIBILITY"`, etc. (table names) | `Services/GasPropertiesService.cs` | 414, 454, 495, 526, 577, 605 | Format ID table names — should be centralized |

**Assessment:**  
**Well-structured.** Physical constants are centralized. Table name IDs could be centralized but are consistent with the pattern used across services.

---

## Priority Refactoring Actions

### HIGH PRIORITY

| Project | Action | Files | Lines |
|---------|--------|-------|-------|
| **EnhancedRecovery** | Create `EorMethodConstants` class with all EOR type strings (`WATER_FLOOD`, `GAS_INJECTION`, `CO2_MISCIBLE`, `POLYMER`, `CHEMICAL`, `STEAM`, `THERMAL`, `SURFACTANT`, `ALKALI`) | `Services/EnhancedRecoveryService.cs` | 49-65 |
| **EnhancedRecovery** | Create `ChemicalTypeConstants` class for chemical EOR types | `Services/EnhancedRecoveryService.Advanced.cs` | 714-756 |
| **EnhancedRecovery** | Centralize recovery factor defaults (20, 25, 18, 22, 30, 15) and chemical economics parameters | `Services/EnhancedRecoveryService.cs`, `Services/EnhancedRecoveryService.Advanced.cs` | 46-66, 714-742 |
| **ChokeAnalysis** | Create `LiftSystemType` constants class (`ESP`, `GasLift`) and centralize magic numbers in `ChokeAnalysisService.Advanced.cs` | `Services/ChokeAnalysisService.Advanced.cs` | 366-648 |
| **PlungerLift** | Create `PlungerType` constants class (`BAR`, `PAD`, `BRUSH`, `CONTINUOUS`) and centralize velocity defaults | `Calculations/PlungerLiftCalculator.cs` | 40-60 |
| **SuckerRodPumping** | Create `RodGrade` constants class (`C`, `D`, `HL`, `HY`, `AUTO`) | `Calculations/RodStringOptimization.cs`, `Calculations/FatigueAnalysis.cs` | 57-60, 66-70, 85-116 |

### MEDIUM PRIORITY

| Project | Action | Files | Lines |
|---------|--------|-------|-------|
| **NodalAnalysis** | Use existing `NodalAnalysisReferenceCodes` for status strings instead of literals | `Services/NodalAnalysisService.Advanced.cs` | 27, 63, 67 |
| **PipelineAnalysis** | Create `FlowRegime` and `ExportFormat` constants classes | `Services/PipelineAnalysisService.Advanced.cs`, `Services/PipelineAnalysisService.Reporting.cs` | 30, 70, 176, 194 |
| **HydraulicPumps** | Create `PumpStatus` and `PumpType` constants classes | `Services/HydraulicPumpService.Advanced.cs` | 42, 47, 60, 73 |
| **OilProperties** | Create `CorrelationMethod` constants class | `Services/OilPropertiesService.cs`, `Services/OilPropertiesService.Advanced.cs` | 160, 36 |
| **FlashCalculations** | Use reference codes for status strings | `Services/FlashCalculationService.Advanced.cs` | 113, 124 |

### LOW PRIORITY

| Project | Action | Files |
|---------|--------|-------|
| **All projects** | Centralize `FormatIdForTable` table name strings into a shared `TableNames` class per project | Various `*Service.cs` files |
| **All projects with ModuleSetup** | Centralize `ModuleId` strings into constants | `Modules/*Module.cs` files |
| **All projects** | Centralize export format defaults (`CSV`, `PDF`, `JSON`) into a shared `ExportFormats` class | Various service interfaces |

---

## Cross-Cutting Observations

1. **WellKnown Pattern**: Only `WellTestAnalysis` and `CompressorAnalysis` have `*WellKnown` classes in `Beep.OilandGas.Models`. Other projects should follow this pattern for cross-project-shared wire tokens.

2. **Module Setup Consistency**: Projects with extension tables (`ChokeAnalysis`, `CompressorAnalysis`, `GasLift`, `FlashCalculations`, `EnhancedRecovery`, `NodalAnalysis`, `ProductionForecasting`) properly implement `ModuleSetupBase`. Projects without extension tables (`PipelineAnalysis`, `PumpPerformance`, `HydraulicPumps`, `SuckerRodPumping`, `PlungerLift`, `OilProperties`, `GasProperties`) correctly do not have module setups.

3. **Reference Data Alignment**: Projects with LOV seeding (`ChokeAnalysis`, `GasLift`, `FlashCalculations`, `EnhancedRecovery`, `NodalAnalysis`, `ProductionForecasting`, `CompressorAnalysis`) follow the consistent pattern of `*ReferenceSets` + `*ReferenceCodeSeed` + `R_*` extension table.

4. **Magic Number Density**: `ChokeAnalysisService.Advanced.cs` and `EnhancedRecoveryService.Advanced.cs` have the highest concentration of uncentralized magic numbers and strings.

5. **Table Name Pattern**: All projects using `FormatIdForTable` with string literals should consider a `TableNames` static class to avoid typos and enable refactoring safety.

---

## Recommended Next Steps

1. **Start with HIGH priority projects** — EnhancedRecovery, ChokeAnalysis, PlungerLift, SuckerRodPumping
2. **Create `*WellKnown` classes** in `Beep.OilandGas.Models` for cross-cutting wire tokens
3. **Centralize table names** into `TableNames` classes per project
4. **Add XML documentation** to all new constants linking to PPDM R_* reference tables where applicable
5. **Update all comparisons** to use `string.Equals(value, Constants.Class.Property, StringComparison.OrdinalIgnoreCase)` pattern
6. **Run tests** after each refactoring to ensure no regressions

---

**Document Version:** 1.0  
**Last Updated:** 2026-05-03  
**Maintainer:** Engineering Team
