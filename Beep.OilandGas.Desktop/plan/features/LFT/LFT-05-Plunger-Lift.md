# LFT-05 — Plunger lift

| Field | Value |
|---|---|
| Stage | STG-07 — Well performance and artificial lift |
| Area | Lift |
| Status | Gate met - see [evidence](../../evidence/STG-07-LFT-05/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PlungerLift` |
| Desktop project | `Modules/Lift/PlungerLift/` |
| External packages | none |
| Depends on | RES-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Pumps/PlungerLift.razor` |

## 1. Purpose

Assess whether plunger lift suits a well, and if so design the cycle: fall, rise, shut-in, gas
requirement and resulting rate.

## 2. Source components

| Source | Reuse |
|---|---|
| `PlungerLift/Services/PlungerLiftService.cs`, `.Advanced` (`IPlungerLiftService`) | service |
| `PlungerLift/Calculations/PlungerLiftCalculator.cs` | cycle and rate |
| `PlungerLift/Constants/PlungerLiftConstants.cs` | velocities and limits |

## 3. Screens

`Lift → Plunger lift`

- Well inputs: depth, tubing, casing pressure, line pressure, gas rate, liquid rate, GLR.
- Feasibility panel: pass/fail per criterion with the deciding value.
- Plunger type selector (from the owning constant set).
- Cycle design: fall velocity, rise velocity, shut-in time, total cycle, cycles per day,
  production per cycle.
- Optimization: recommended cycle vs current with the delta.
- Result: feasible / infeasible / feasible but uneconomic.

## 4. Data and contracts

`OgPlungerDesign { PlungerType, FallTime, RiseTime, ShutInTime, CyclesPerDay, RatePerCycle, Rate,
RateUnit, Feasibility, Economic }`.

## 5. States

| State | Behaviour |
|---|---|
| Infeasible | stated with the failing criterion; no rate is shown |
| Feasible but uneconomic | distinct state with the economic basis |
| Missing GLR | refused with the reason |
| Velocity outside the constant range | clamped value is not shown; the range is stated |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Assess | `lift.plunger.read` | no |
| Save design | `lift.plunger.write` | yes |

## 7. Acceptance criteria

- AC-1 Feasibility is decided per criterion and every criterion is shown.
- AC-2 "Infeasible" never displays a production rate.
- AC-3 "Feasible but uneconomic" is a separate state.
- AC-4 Plunger types come from the owning constants.
- AC-5 Cycle times are internally consistent (fall + rise + shut-in = cycle).
- AC-6 A saved design records the plunger type and the criteria outcome.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LFT-05"
```

## 9. Open questions and risks

- `PlungerLiftCalculator` contains fall velocities per plunger type as literals. Surface them from
  constants, not from the view.
