# Phase 1 — Calculators, correlations, service alignment, and units

## Objectives

1. Make **black-oil** math **predictable and auditable**: one place for Standing / Beggs–Robinson constants, explicit **unit conversions**, and documented **applicability ranges**.
2. Align **`OilPropertiesService`** with **`OilPropertyCalculator`** (no duplicated magic numbers where **`OilPropertyConstants`** already exists).
3. Resolve **Rankine vs Fahrenheit** end-to-end (see phase 0 gap).

## Industry / technical references (for implementation comments and tests)

- Standing correlations for **Pb**, **Rs**, **Bo** (scf/stb, psia, °F in original publications — convert at API boundary if storing **°R** in PPDM).
- Beggs–Robinson **dead oil viscosity**; saturated oil viscosity form as implemented.
- McCain / Ahmed texts for **undersaturated** extensions (**Bo**, **co**, **μo** above bubble point) when product requires them.

## TODO checklist

| # | Task | Target files / types |
|---|------|------------------------|
| 1.1 | **Decide canonical temperature** for **`OIL_PROPERTY_CONDITIONS.TEMPERATURE`** and **`IOilPropertiesService`** parameters; document on interface + entity. If canonical is **°R**, add **`OilPropertyCalculator` internal helpers** or wrapper methods **`...FromRankine`**. | `IOilPropertiesService.cs`, `OIL_PROPERTY_CONDITIONS` (summary/XML if partial), `OilPropertyCalculator.cs`, `OilPropertiesService.Advanced.cs` |
| 1.2 | Wire **`CalculateBlackOilPropertiesAsync`** through **`OilPropertyValidator.ValidateOilPropertyConditions`** (opt-in flag or always validate). | `OilPropertiesService.Advanced.cs`, `OilPropertyValidator.cs` |
| 1.3 | Replace hardcoded **`0.65m`** gas gravity in **`CalculateFormationVolumeFactor`** with parameter from **`OilComposition`** / overload / conditions row when available; keep default only as last resort. | `OilPropertiesService.cs`, optional `IOilPropertiesService` overload (coordinate with API) |
| 1.4 | Centralize **141.5 / (131.5 + API)** via **`OilPropertyConstants`** in both service and calculator where duplicated. | `OilPropertyCalculator.cs`, `OilPropertiesService.cs` |
| 1.5 | **Undersaturated Bo** (e.g. Vasquez–Beggs **co** integration) and **undersaturated viscosity** — implement or explicitly **defer** with `NotSupportedException` / applicability warning DTO. | New partial `OilPropertyCalculator.*.cs` or dedicated calculator class |
| 1.6 | Audit **Advanced** branch logic when only **P** and **Pb** / **Rs** partially known; add state diagram in comments; fix any unreachable or contradictory branches. | `OilPropertiesService.Advanced.cs` |
| 1.7 | Tag **placeholder** analyses (phase diagram, oil “Z”, IFT) in XML as **non-regression screening** or move to separate **`Screening`** namespace/class to avoid implying lab-grade PVT. | `OilPropertiesService.cs` |

## Verification criteria

- [ ] Single documented temperature basis from **API → DB → calculator**.
- [ ] **`dotnet build`** on **`Beep.OilandGas.OilProperties`** succeeds.
- [ ] No duplicate Standing coefficients in service vs calculator (constants in one place).

## Exit criteria

Phase 1 complete when **1.1** is implemented and reviewed, and checklist items **1.2–1.4** are either done or explicitly deferred with tickets in **`MASTER-TODO-TRACKER.md`**.
