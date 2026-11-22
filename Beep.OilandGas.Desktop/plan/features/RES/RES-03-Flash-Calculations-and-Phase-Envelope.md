# RES-03 — Flash calculations and phase envelope

| Field | Value |
|---|---|
| Stage | STG-06 — Reservoir, PVT and forecasting |
| Area | Reservoir |
| Status | Gate met - see [evidence](../../evidence/STG-06-RES-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.FlashCalculations` |
| Desktop project | `Modules/Reservoir/Flash/` |
| External packages | none |
| Depends on | RES-01, RES-02 |
| Blocks | RES-04 |
| Web reference | `Pages/PPDM39/Calculations/FlashCalculations.razor` (`/analysis/flash-calculations`) |

## 1. Purpose

Phase behaviour: isothermal flash, phase composition, separator stages, and the phase envelope —
with convergence reported honestly.

## 2. Source components

| Source | Reuse |
|---|---|
| `FlashCalculations/Services/FlashCalculationService.cs`, `.Advanced` | flash engine (concrete class — wrap behind a desktop interface) |
| `FlashCalculations/Services/FlashFeedCatalogMerge.cs` | feed composition merge |
| `FlashCalculations/Calculations/FlashCalculator.cs` | Wilson K + Rachford-Rice |
| `FlashCalculations/Calculations/MultiComponentFlash.cs`, `PhaseEnvelope.cs` | multi-component and envelope |
| `FlashCalculations/Calculations/AdvancedEOS/*` | PR / SRK |
| `FlashCalculations/Constants/FlashConstants.cs`, `FlashEquationOfStateMapping.cs` | convergence, EOS names |
| Table `R_FLASH_CALCULATION_REFERENCE_CODE` | reference codes |

## 3. Screens

`Reservoir → Flash`

- Feed tab: component table with mole fractions (must sum to 1 within tolerance), or import a
  composition from RES-01/RES-02.
- Conditions tab: pressure, temperature, EOS selection (`PR`, `SRK`, `SRK_MODIFIED`, `IDEAL_K`),
  convergence settings (read-only defaults unless advanced is enabled).
- Results tab: phase split, compositions, densities, K-values, iteration count, convergence status.
- Envelope tab: phase envelope plot with the operating point.
- Separator tab: multi-stage scenario with stage conditions and resulting liquid/gas.
- History.

## 4. Data and contracts

`OgFlashResult { Converged, Iterations, VapourFraction, PhaseComposition[], Eos, StatusCode }`.
`StatusCode` comes from `R_FLASH_CALCULATION_REFERENCE_CODE` (`SUCCESS`, `CONVERGENCE_FAILED`,
`FAILED`).

## 5. States

| State | Behaviour |
|---|---|
| Mole fractions do not sum to 1 | refused with the current sum |
| Convergence failed | result is failed; no composition is shown as if valid |
| EOS not applicable | refused naming the component |
| Envelope only partially computed | the computed portion is drawn and the limit stated |
| History unavailable | failed state |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Run | `properties.flash.read` | no |
| Save | `properties.flash.write` | yes |
| Advanced settings | `properties.flash.advanced` | no |

## 7. Acceptance criteria

- AC-1 A reference flash converges and matches the expected vapour fraction within tolerance.
- AC-2 A non-converged run is shown as failed with the iteration count.
- AC-3 Mole fractions must sum to 1 within the stated tolerance.
- AC-4 EOS names come from the mapping constants, not literals in the view.
- AC-5 The envelope never draws beyond computed points.
- AC-6 Same inputs give identical output (determinism).

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~Flash"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~FlashGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-06-RES-03/observed-evidence.json). The flash and the envelope call
the owning engine directly (`FlashCalculator.PerformIsothermalFlash`, `MultiComponentFlash.PerformEnhancedFlash`,
`PhaseEnvelope.CalculateEnvelope`); the run is read and written through `UnitofWork<T>` over the FlashCalculations-owned
`FLASH_CALCULATION_RESULT`, with `EOS_CODE` and `STABILITY_TEXT` added to it.

**The open question in section 9, answered:** `FlashCalculationService` is a concrete class, so the desktop wraps it -
`IOgFlash` / `OgFlashService`. No view references the engine types.

**The reference case (AC-1) was computed by hand first.** With C1 0.50, C2 0.20, C3 0.15 and nC4 0.15 at 500 psia and
600 R, Wilson's K-values are 13.646078, 2.319017, 0.625939 and 0.186554 and the hand solution of
`sum z(K-1)/(1+beta(K-1)) = 0` is **beta = 0.94143858**. The engine returns 0.9414386, and the test also checks that each
phase sums to one, that every component closes `z = (1-beta)x + beta y`, and that `K = y/x`.

**Convergence, honestly (SG-06-3, SG-06-4):** the engine's own flag becomes the run's state. A non-converged run is
`CONVERGENCE_FAILED`, keeps its iteration count, shows "not computed" for both fractions and exposes no composition. The
engine's solver proved robust on every valid feed tried, so the rule is tested where it lives - the mapping from the
engine's flag to the result - rather than behind a fabricated failing input.

**Two data gaps closed:** the reference set the feature names had no `SUCCESS` / `CONVERGENCE_FAILED` / `FAILED` rows, so
a `FLASH_STATUS_CODE` set and its three rows were seeded; and `SRK_MODIFIED`, which the reference set offers, has no alpha
function in `AdvancedEOS.EosType`, so it is refused with that reason instead of being run as plain SRK.

**An engine defect this feature found and fixed:** `AdvancedEOS.GetBinaryInteractionParameters` passed
`StringComparer.OrdinalIgnoreCase.GetHashCode()` as a dictionary **capacity**, so the cubic path threw
`ArgumentOutOfRangeException "capacity"` whenever that random per-process hash was negative or too large - intermittent
between processes, and the lookup was never case-insensitive. It now uses a real pair comparer.

**Not built:** the separator tab (the multi-stage entry point needs its own record), composition import from RES-01/RES-02,
and an editable convergence preset; the envelope is presented as the computed point list because there is still no chart
host for these screens.

## 9. Open questions and risks

- `FlashCalculationService` has no interface. `LocalServiceComposer` must expose it behind a
  desktop-owned interface; do not let a view model reference the concrete type.
