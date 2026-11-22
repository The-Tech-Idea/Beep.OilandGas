# LFT-03 — ESP and pump performance

| Field | Value |
|---|---|
| Stage | STG-07 — Well performance and artificial lift |
| Area | Lift |
| Status | Gate met - see [evidence](../../evidence/STG-07-LFT-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PumpPerformance` |
| Desktop project | `Modules/Lift/PumpPerformance/` |
| External packages | none |
| Depends on | RES-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Calculations/PumpPerformance.razor` |

## 1. Purpose

Select and check a pump against its published curve: head, rate, efficiency, power, NPSH and
affinity-law behaviour, and design an ESP string.

## 2. Source components

| Source | Reuse |
|---|---|
| `PumpPerformance/Services/PumpPerformanceService.cs`, `IPumpPerformanceService.cs` | service |
| `PumpPerformance/Rendering/PumpPerformanceRenderer.cs`, `.Configuration` | plot |
| `PumpPerformance/Interaction/PumpPerformanceInteractionHandler.cs` | plot interaction |
| `PumpPerformance/Calculations/HeadQuantityCalculations.cs`, `EfficiencyCalculations.cs`, `PowerCalculations.cs`, `AffinityLaws.cs`, `NPSHCalculations.cs`, `ESPDesignCalculator.cs` | engine |
| `PumpPerformance/PumpTypes/*` (Centrifugal, PositiveDisplacement, ESP, Jet) | type behaviour |
| `PumpPerformance/SystemAnalysis/*` | system curve |
| `PumpPerformance/Constants/PumpConstants.cs` | conversions and NPSH margin |

## 3. Screens

`Lift → Pump performance`

- Pump selection: type, published curve (or entered points), stages.
- Duty point: rate, head, efficiency, power, NPSH available vs required, with a warning band.
- Curve plot: H-Q, efficiency, power, system curve overlay, duty point marker via the renderer.
- Affinity panel: speed change or impeller trim effect.
- ESP design: stages, motor selection, cable loss, voltage drop, temperature check.
- Save / version.

## 4. Data and contracts

`OgPumpResult { Rate, RateUnit, Head, HeadUnit, Efficiency, Power, NpshAvailable, NpshRequired,
OperatingPointValid, Notes[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Duty point outside the curve | refused, with the valid range shown |
| NPSH margin below the configured minimum | warning chip with both values |
| Missing curve | selection disabled with the reason |
| Affinity result outside published range | marked "extrapolated" |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Analyse | `lift.pump.read` | no |
| Save design | `lift.pump.write` | yes |

## 7. Acceptance criteria

- AC-1 A duty point outside the published curve is refused.
- AC-2 NPSH margin uses the configured value from `PumpConstants`.
- AC-3 Extrapolated affinity results are labelled.
- AC-4 Curves are drawn by `PumpPerformanceRenderer`.
- AC-5 Every result states its unit.
- AC-6 A saved design records the curve source and stage count.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LFT-03"
```

## 9. Open questions and risks

- `IPumpPerformanceService` surface is broad; implement selection, duty point, affinity, NPSH and
  ESP design in 1.0 and record the rest as deferred.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-07-LFT-03/observed-evidence.json). Duty points are read and written
through `UnitofWork<T>` over the PumpPerformance-owned `PUMP_DESIGN`.

**The open question, answered in part:** selection, the duty point, affinity and NPSH are implemented; the **ESP design
tab** (motor selection, cable loss, voltage drop, temperature check) is **not**, and that is recorded as deferred rather
than claimed. Its engine (`ESPDesignCalculator.DesignESP`) and its tables exist and are now registered.

**A registration gap this feature closed:** the PumpPerformance project had **no module setup at all**, so a provisioned
database never created `ESP_DESIGN_PROPERTIES`, `ESP_DESIGN_RESULT`, `ESP_MOTOR_PROPERTIES`, `ESP_CABLE_PROPERTIES` or
`ESP_PUMP_POINT`. `PumpPerformanceModule` now registers those five together with the new `PUMP_DESIGN`; smoke reports 29
modules and 398 entity types.

**The curve is entered, not looked up.** This repository has no pump-curve catalog, so the "published curve" is the one the
engineer types and every saved design records `CURVE_SOURCE = ENTERED`. The page states that rather than implying a
catalog.

**Two readings worth stating:** the stage count multiplies the per-stage head and the page says "head per stage X, total
Y"; and a positive suction head is subtracted from the available NPSH by the owner's formula (it is treated as a lift), so
the margin sentence prints both values for the sign to be visible.

**Not built:** `PumpPerformanceRenderer` and its interaction handler need the chart host these screens do not have, so the
curve is a point list; and the ESP design tab is deferred.
