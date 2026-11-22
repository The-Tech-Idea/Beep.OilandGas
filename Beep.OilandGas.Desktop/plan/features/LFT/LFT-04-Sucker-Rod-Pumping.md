# LFT-04 — Sucker rod pumping

| Field | Value |
|---|---|
| Stage | STG-07 — Well performance and artificial lift |
| Area | Lift |
| Status | Gate met - see [evidence](../../evidence/STG-07-LFT-04/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.SuckerRodPumping` |
| Desktop project | `Modules/Lift/SuckerRod/` |
| External packages | none |
| Depends on | RES-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Pumps/SuckerRodPumping.razor` |

## 1. Purpose

Design a sucker rod string that lifts the required rate without exceeding stress or fatigue limits,
and show the pump card that proves it.

## 2. Source components

| Source | Reuse |
|---|---|
| `SuckerRodPumping/Services/SuckerRodPumpingService.cs` (`ISuckerRodPumpingService`) | service |
| `SuckerRodPumping/Calculations/SuckerRodLoadCalculator.cs` | peak/min load, fluid load, dynamic load, stress |
| `SuckerRodPumping/Calculations/SuckerRodFlowRatePowerCalculator.cs` | rate, power |
| `SuckerRodPumping/Calculations/RodStringOptimization.cs` | grade and taper selection |
| `SuckerRodPumping/Calculations/FatigueAnalysis.cs` | fatigue and safety factor |
| `SuckerRodPumping/Constants/SuckerRodConstants.cs` | steel density, defaults |

## 3. Screens

`Lift → Sucker rod`

- Well and fluid inputs.
- Pump and stroke: plunger size, stroke length, SPM, pump depth.
- Rod string editor: sections with grade, diameter, length; auto-optimize action.
- Results: peak/min polished rod load, fluid load, stress, safety factor, rate, power.
- Pump card plot: surface and downhole card.
- Fatigue panel: modified Goodman position and margin.

## 4. Data and contracts

`OgRodString { Sections[] { Grade, Diameter, DiameterUnit, Length, LengthUnit }, TotalLength }`
`OgSrpResult { PeakLoad, MinLoad, Stress, SafetyFactor, Rate, RateUnit, Power, CardPoints[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Stress above limit | refused; the limiting section is named |
| Grade unknown | refused (grades come from the owning constants) |
| Optimizer finds no valid string | stated as infeasible with the binding constraint |
| Missing density | default offered but must be accepted explicitly |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Design | `lift.srp.read` | no |
| Save design | `lift.srp.write` | yes |

## 7. Acceptance criteria

- AC-1 An unsafe string is refused and names the limiting section.
- AC-2 Rod grades come from the owning constant class, not from literals in the view.
- AC-3 The pump card is produced and matches the computed loads.
- AC-4 Optimizer output is reproducible.
- AC-5 Lengths and diameters carry units.
- AC-6 A saved design records grade, taper and the safety factor achieved.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LFT-04"
```

## 9. Open questions and risks

- `RodStringOptimization` uses rod-grade literals `"C"`, `"D"`, `"HL"`, `"HY"`, `"AUTO"`. Bind the
  combo box to a constant list rather than repeating the strings.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-07-LFT-04/observed-evidence.json). Designs are read and written
through `UnitofWork<T>` over the new SuckerRodPumping-owned `SRP_DESIGN`; the project's other types are calculation objects
and are not tables.

**The open question above, answered:** `RodGrades.All` is built from `SuckerRodConstants` (C, D, HL, HY) and excludes
`AUTO`, which is the optimizer's instruction rather than a grade. The combo box binds to the workspace's list, and a source
guard asserts the view names no grade itself. An unknown grade is refused.

**A gap this feature closed:** the SuckerRodPumping project had **no module setup and no design table**, so a provisioned
database had nowhere to keep a rod design. `SuckerRodPumpingModule` now registers `SRP_DESIGN`; smoke reports 30 modules and
399 entity types.

**The refusal (AC-1, SG-07-5)** compares the string's lowest safety factor against `SuckerRodConstants.MinimumSafetyFactor`
(1.5) and names the limiting section with its diameter, stress and Goodman ratio. It runs again in `SaveAsync`, so an unsafe
result cannot be constructed by a caller and stored.

**Two readings worth stating:** the fatigue is the module's section-by-section analysis over the whole-string peak and
minimum loads (not a per-section wave-equation solution); and three system properties the load calculator needs are fixed
screening values (wellhead 100 psia, bottom hole 1,000 psia, fluid density 62.4 lb/ft3) because this screen does not take
them as inputs.

**Not built:** the taper optimizer (`RodStringOptimization`) is not surfaced - the engineer enters the string and it is
checked - and the pump card is shown as its extremes and a point count rather than plotted, because there is still no chart
host for these calculation screens.
