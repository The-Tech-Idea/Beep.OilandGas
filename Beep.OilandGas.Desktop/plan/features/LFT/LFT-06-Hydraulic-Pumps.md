# LFT-06 — Hydraulic pumps

| Field | Value |
|---|---|
| Stage | STG-07 — Well performance and artificial lift |
| Area | Lift |
| Status | Gate met - see [evidence](../../evidence/STG-07-LFT-06/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.HydraulicPumps` |
| Desktop project | `Modules/Lift/HydraulicPumps/` |
| External packages | none |
| Depends on | RES-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Pumps/HydraulicPump.razor` |

## 1. Purpose

Design a hydraulic jet or piston pump installation: nozzle/throat sizing, power fluid rate and
pressure, discharge conditions, and cavitation checks.

## 2. Source components

| Source | Reuse |
|---|---|
| `HydraulicPumps/Services/HydraulicPumpService.cs`, `.Advanced`, `.ModelsCoreImpl` (`IHydraulicPumpService`) | service |
| `HydraulicPumps/Calculations/HydraulicJetPumpCalculator.cs`, `HydraulicPistonPumpCalculator.cs`, `HydraulicPumpSharedCalculations.cs` | engine |
| `HydraulicPumps/Constants/HydraulicPumpConstants.cs` | constants |
| `HydraulicPumps/Validation/HydraulicPumpValidator.cs` | validation |

## 3. Screens

`Lift → Hydraulic pumps`

- Type selector: jet or piston.
- Jet tab: nozzle and throat sizes, area ratio, power fluid rate and pressure, production rate,
  efficiency, cavitation index.
- Piston tab: engine and pump piston sizes, ratio, stroke rate, power fluid rate, discharge.
- System tab: tubing friction, surface pressure requirement, system efficiency.
- Validation panel: `HydraulicPumpValidator` findings with severity.

## 4. Data and contracts

`OgHydraulicPumpDesign { Kind, Nozzle, Throat, AreaRatio, PowerFluidRate, PowerFluidPressure,
ProductionRate, Efficiency, CavitationIndex, Findings[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Cavitation predicted | warning with the cavitation index and the threshold |
| Invalid combination of nozzle/throat | refused by the validator |
| System efficiency outside [0, 1] | refused (the engine clamps; the UI must not present a clamped value as real) |
| Missing power fluid pressure | refused with the reason |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Design | `lift.hydraulic.read` | no |
| Save design | `lift.hydraulic.write` | yes |

## 7. Acceptance criteria

- AC-1 A reference jet design reproduces the expected efficiency within tolerance.
- AC-2 Cavitation is reported with its index and threshold.
- AC-3 A clamped efficiency is surfaced as a validation finding, not shown silently.
- AC-4 Validator findings are listed with severity.
- AC-5 Nozzle and throat sizes come from the standard set.
- AC-6 A saved design records the type and all inputs.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LFT-06"
```

## 9. Open questions and risks

- `HydraulicPumpService.Advanced.cs` uses `"JET"`, `"DRAFT"`, `"SUCCESS"` literals. Map them to
  constants when surfacing.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-07-LFT-06/observed-evidence.json). Designs are read and written
through `UnitofWork<T>` over the new HydraulicPumps-owned `HYDRAULIC_PUMP_DESIGN`.

**The open question above, answered by not consuming the service.** `HydraulicPumpService.Advanced` also sets
`RecommendedNozzleSize = 0` ("0 = not sized here"), a fixed `Efficiency = 0.30m` and a `"DRAFT"` status without sizing
anything, so it is not a design path at all. The screen calls the calculators and the findings catalog directly, and the
kind names live in `HydraulicPumpKinds`, so no `"JET"`, `"DRAFT"` or `"SUCCESS"` literal is repeated in the desktop.

**A clamp that had to become visible (AC-3):** `HydraulicPumpCalculator.CalculateJetPumpPerformance` clamps the
efficiency into [0, 1] and returns only the clamped value. `TryCalculateJetPumpPerformance` was added beside it (the
original is unchanged) and returns the raw M x N, the clamped value and the area ratio, so the design can compare them and
raise `JET_EFFICIENCY_CLAMPED` with both numbers.

**The validator only throws, so the owner got a list form (AC-4):** `HydraulicPumpFindings` evaluates the same rules in
the same order, keeps the validator's own sentences, and adds a severity (Info/Warning/Error). A unit test asserts one
finding's message equals the `InvalidPumpPropertiesException` the validator throws for the same input. An error finding
refuses the run before its numbers are shown - an invalid nozzle/throat combination has no business producing a rate - and
refuses the save; warnings (cavitation, a clamp, a low pump efficiency) are listed and kept.

**A gap closed:** the HydraulicPumps project had **no module setup**, so its five calculation entities were never created
by a provisioned database. `HydraulicPumpModule` now registers them with the new design table; smoke reports 32 modules and
406 entity types.

**What the numbers are:** the jet production rate and efficiency are the module's screening expressions (rate from area
ratio x density ratio x pressure differential; efficiency from three clamped factors), not a manufacturer's jet-pump
curve, and the repository has no pump catalogue. The cavitation index uses the module's single 50 psi required NPSH and
the water vapour pressure at 60 degF, both printed rather than assumed.

**Not built:** the system tab as its own panel (friction and discharge pressure are the owner's shared calculations and
appear inside the result, but are not broken out), and a surface-pressure requirement.
