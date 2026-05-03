# Phase 0 — Gas properties overview and baseline

## Purpose

**Natural gas** Z-factor, viscosity, pseudo-pressure, averaging, and mixing utilities used across nodal analysis, gas lift, pipeline, compressor, and well-test flows. Core numerics live in **`Beep.OilandGas.GasProperties`**; shared wire / table shapes live in **`Beep.OilandGas.Models.Data.GasProperties`** (and related **`Models`** namespaces). This project is **not** a `ModuleSetupBase` domain (no feature **`R_*`** catalog here — unlike **`Beep.OilandGas.GasLift`** / **`FlashCalculations`**).

**Industry coverage** (wells, facilities, reservoirs, correlation choice, and “when not to use single-phase Z”) is captured in **[`04_Industry_Scenarios_Wells_Facilities_Reservoirs.md`](04_Industry_Scenarios_Wells_Facilities_Reservoirs.md)** and should drive validator warnings, **README** scope, and **phase 2** test tagging.

## Inventory (this assembly)

| Area | Location |
|------|-----------|
| Z-factor | **`Calculations/ZFactorCalculator.cs`** (Brill–Beggs, Hall–Yarborough, Standing–Katz / DAK, pseudo-critical from composition) |
| Viscosity | **`Calculations/GasViscosityCalculator.cs`** (Carr–Kobayashi–Burrows, Lee–Gonzalez–Eakin) |
| Pseudo-pressure | **`Calculations/PseudoPressureCalculator.cs`** |
| Averages | **`Calculations/AveragePropertiesCalculator.cs`** |
| Other | **`Calculations/GasPropertyCalculator.cs`**, **`GasMixingRuleCalculator.cs`** |
| Validation | **`Validation/GasPropertiesValidator.cs`** |
| Constants | **`Constants/GasPropertiesConstants.cs`** |
| Exceptions | **`Exceptions/GasPropertiesException.cs`** |
| Persistence / orchestration | **`Services/GasPropertiesService.cs`**, **`GasPropertiesService.Advanced.cs`** |

## Shared contracts (outside this assembly)

| Area | Location |
|------|-----------|
| Service contract | **`IGasPropertiesService`** — `Beep.OilandGas.Models.Core.Interfaces` |
| Composition / DTOs | **`Beep.OilandGas.Models.Data.GasProperties`** (e.g. **`GasComposition`**) |

## Baseline gaps

| Gap | Notes | Phase |
|-----|--------|--------|
| No **`Beep.OilandGas.GasProperties.Tests`** project | Regression risk for Z, viscosity, pseudo-pressure | 2 |
| **`README.md`** / **`IMPLEMENTATION_SUMMARY.md`** still reference **`Beep.OilandGas.GasProperties.Models`** | Models live under **`Beep.OilandGas.Models`**; samples should use **`Beep.OilandGas.Models.Data.GasProperties`** | 3 |
| Correlation string contracts (`Standing-Katz`, etc.) | Document and test allowed values used by **`GasPropertiesService`** | 1 |
| **`ENHANCEMENT_PLAN.md`** backlog | Optional correlations / Bg / compressibility — track against product priority | 3 |
| Scenario / basis documentation | README does not yet spell out **well vs facility vs reservoir** applicability and **Flash/EOS** boundary | 3–4 |
| Sour / acid / CO₂-rich guardrails | Validator may not warn when **γg**-only masks high **inerts** | 1, 4 |

## Exit criteria

- [ ] Phase docs **00–04** + **08** accepted as source of truth for gas-properties work.
- [ ] **`README.md`** links **`.plans`** + **`MASTER-TODO-TRACKER.md`**.
- [ ] At least smoke **tests** for **`ZFactorCalculator`** + **`GasPropertiesValidator`** (phase 2).
