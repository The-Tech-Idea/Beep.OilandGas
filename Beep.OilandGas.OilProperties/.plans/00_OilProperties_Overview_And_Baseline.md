# Phase 0 — Oil properties overview and baseline

## Purpose

**Black-oil** screening correlations (bubble point, solution GOR, oil FVF, dead/saturated viscosity) and **composition / result persistence** for workflows that need **API / GOR / P / T** driven estimates without a full EOS table generator. Core numerics live in **`Beep.OilandGas.OilProperties`**; shared wire / table shapes live in **`Beep.OilandGas.Models.Data`** (e.g. **`OilComposition`**, **`OilPropertyResult`**, **`OIL_PROPERTY_CONDITIONS`**). **`IOilPropertiesService`** is defined in **`Beep.OilandGas.Models.Core.Interfaces`**; **`Beep.OilandGas.ApiService`** registers **`Beep.OilandGas.OilProperties.Services.OilPropertiesService`**.

This assembly is **not** a **`ModuleSetupBase`** domain unless product adds **feature-local `R_*` catalogs** (e.g. correlation LOVs) beyond standard PPDM — same pattern as **`GasProperties`**.

**Industry coverage** (where black oil is acceptable, units, saturated vs undersaturated paths) is expanded in **[`04_Industry_Black_Oil_PVT_Scenarios.md`](04_Industry_Black_Oil_PVT_Scenarios.md)**.

## Inventory (this assembly)

| Area | Location |
|------|-----------|
| Correlations | **`Calculations/OilPropertyCalculator.cs`** — Standing (Pb, Rs, Bo), Beggs–Robinson dead + saturated viscosity |
| Validation | **`Validation/OilPropertyValidator.cs`** |
| Constants / clamps | **`Constants/OilPropertyConstants.cs`** |
| Exceptions | **`Exceptions/OilPropertyException.cs`** |
| Persistence / orchestration | **`Services/OilPropertiesService.cs`**, **`OilPropertiesService.Advanced.cs`** (`CalculateBlackOilPropertiesAsync` on **`OIL_PROPERTY_CONDITIONS`**) |

## Shared contracts (outside this assembly)

| Area | Location |
|------|----------|
| Service contract | **`IOilPropertiesService`** — `Beep.OilandGas.Models.Core.Interfaces` |
| DTOs / conditions | **`Beep.OilandGas.Models.Data`**, **`Beep.OilandGas.Models.Data.OilProperties`** |

## Related code (watch for drift)

| Area | Notes |
|------|--------|
| **`Beep.OilandGas.Properties`** | Contains another **`OilPropertiesService`** / **`IOilPropertiesService`** — ensure API and docs refer to **`Beep.OilandGas.OilProperties`** as the registered implementation |

## Baseline gaps (prioritized)

| Gap | Notes | Phase |
|-----|--------|--------|
| **Temperature unit mismatch risk** | **Addressed:** **`OilPropertyUnits.RankineToFahrenheit`**, service FVF/viscosity and **`CalculateBlackOilPropertiesAsync`** convert before **`OilPropertyCalculator`**; validator enforces minimum **°R** for correlations. | Done |
| **No `Beep.OilandGas.OilProperties.Tests` project** | **Added** — calculator / units / validator coverage; service + API tests still light. | 2 |
| **Saturated vs undersaturated Bo/Rs logic** | Advanced path has branches; undersaturated viscosity note in code; align with textbook **Rs = Rsb** for **P > Pb** and optional **undersaturated Bo / μ** correlations | 1 |
| **Hardcoded gas gravity** | **`CalculateFormationVolumeFactor`** accepts optional **`gasSpecificGravity`** (default 0.65); wire from **`CalculateFVFRequest`** / composition in a follow-up. | 1 |
| **`OilPropertyConstants` vs LOV** | Numeric limits are **screening clamps**, not PPDM LOVs — correlation **names** may later move to **`R_*`** if product requires DB-driven pick lists | 3–4 |
| **Unused `GasProperties` project reference** | **Removed** from **`Beep.OilandGas.OilProperties.csproj`**. | Done |
| **Placeholder / illustrative methods** | Phase diagram, Pitzer “Z” for oil, IFT parachor, etc. in **`OilPropertiesService`** may not match published correlations — document as **screening** or replace with cited methods | 1, 4 |

## Exit criteria

- [ ] Phase docs **00–04** + **08** accepted as source of truth for oil-properties work.
- [ ] **`MASTER-TODO-TRACKER.md`** links **`.plans/README.md`** and reflects status.
- [ ] **Unit temperature** decision recorded in **`01_...`** and reflected in code or XML docs on **`OIL_PROPERTY_CONDITIONS`**.
