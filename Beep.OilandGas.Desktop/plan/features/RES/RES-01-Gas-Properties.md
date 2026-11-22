# RES-01 — Gas properties

| Field | Value |
|---|---|
| Stage | STG-06 — Reservoir, PVT and forecasting |
| Area | Reservoir |
| Status | Gate met - see [evidence](../../evidence/STG-06-RES-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.GasProperties` |
| Desktop project | `Modules/Reservoir/GasProperties/` |
| External packages | none |
| Depends on | DAT-04 |
| Blocks | RES-03, LFT-01, LFT-07, LFT-08, WTA-03 |
| Web reference | `Pages/PPDM39/Calculations/GasProperties.razor` (`/analysis/gas-properties`) |

## 1. Purpose

Compute gas Z-factor, viscosity, pseudo-pressure and average properties with the correlation named
and the inputs preserved, so any result can be reproduced.

## 2. Source components

| Source | Reuse |
|---|---|
| `GasProperties/Services/GasPropertiesService.cs`, `.Advanced` | `IGasPropertiesService` |
| `GasProperties/Calculations/ZFactorCalculator.cs` | Brill-Beggs, Hall-Yarborough, DAK / Standing-Katz |
| `GasProperties/Calculations/GasViscosityCalculator.cs` | Carr-Kobayashi-Burrows, Lee-Gonzalez-Eakin |
| `GasProperties/Calculations/PseudoPressureCalculator.cs` | integration |
| `GasProperties/Calculations/AveragePropertiesCalculator.cs` | averaging |
| `GasProperties/Constants/GasPropertiesConstants.cs` | 14.696 psia, 519.67 R, 10.7316 |
| Tables `GAS_COMPOSITION`, `GAS_COMPOSITION_COMPONENT` | persistence |

## 3. Screens

`Reservoir → Gas properties` using `CalculationShell`:

- Inputs tab: composition (or a simple gas gravity), pressure, temperature, unit selection per
  field, correlation choice per property.
- Results tab: Z, viscosity, density, compressibility, pseudo-pressure, each with its correlation.
- Charts tab: Z vs pressure at constant temperature; viscosity vs pressure.
- History tab: previous runs with inputs and actor; "load inputs" to rerun.

## 4. Data and contracts

Inputs and results are the owning service's DTOs; the view model holds them, never a copy of the
model class. `CalculationShell<TInput, TResult>` supplies run/save/history.

## 5. States

| State | Behaviour |
|---|---|
| Missing input | run disabled with the missing field named |
| Correlation not applicable | refused with the reason (e.g. composition out of range) |
| Non-convergence | failed result, not a number |
| Unit mismatch | refused; offer the compatible unit |
| History unavailable | failed state; new runs still allowed |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Run | `properties.gas.read` | no |
| Save result | `properties.gas.write` | yes |
| Export chart/data | `data.export` | no |

## 7. Acceptance criteria

- AC-1 A reference case reproduces the expected Z within the stated tolerance.
- AC-2 The correlation used is displayed with the result.
- AC-3 A non-converged calculation is shown as failed.
- AC-4 Units are stored and converted through the unit catalog only.
- AC-5 Every saved result records inputs, correlation and actor.
- AC-6 Runs with the same inputs produce identical results (determinism).

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~GasProperties"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~GasPropertiesGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-06-RES-01/observed-evidence.json). The calculation calls the
owning project's `ZFactorCalculator`, `GasViscosityCalculator`, `PseudoPressureCalculator` and
`GasPropertiesValidator` directly; the run is read and written through `UnitofWork<T>` over the GasProperties-owned
`GAS_PROPERTY_RUN`.

**The reference case (AC-1), and what it found:** with a specific gravity of 0.65 the owner's Sutton pseudo-criticals
give Ppc = 670.129 psia and Tpc = 365.11 R, so Ppr 3.0 / Tpr 1.5 is 2010.387 psia / 547.665 R. The
Dranchuk-Abou-Kassem value there is **0.7761**, and the engine's own Brill-Beggs correlation returns **0.7765** at the
same point - two correlations that previously disagreed by 157%. `CalculateStandingKatz` was returning exactly **2.0**,
its own clamp ceiling, at that point. It now solves the published DAK relation; `CalculateStandingKatz` keeps its
signature and clamp, and a new `TryCalculateStandingKatz` reports whether the equation converged, so a screen can show a
failure rather than a clamped number (AC-3).

**A second engine defect, reported not fixed:** `GasViscosityCalculator.CalculateCarrKobayashiBurrows` returns
**0.00124 cp** at that point where the correlation expects about **0.017 cp**. Its own atmospheric term computes the right
0.0112 cp, so the ratio term is about an order of magnitude low. The published ratio coefficients could not be verified
from a primary source here, so the value is shown as the engine returned it and the defect is recorded rather than
papered over with a tolerance.

**AC-4's limit, stated:** there is no unit catalog in this application (DAT-04 is the PPDM table workbench), so the
conversion lives in one type, `GasUnits`, and every run stores both the typed value with its unit and the converted
psia/Rankine pair the engine actually used - which is also what makes a saved result reproducible.

**Not built:** the Z-vs-pressure and viscosity-vs-pressure charts (the plan's `CurveCanvas` control does not exist), a
composition-based run, and gas compressibility `cg`.

## 9. Open questions and risks

- `GasPropertiesConstants` uses US customary units. The screen must convert, not change the
  constants.
