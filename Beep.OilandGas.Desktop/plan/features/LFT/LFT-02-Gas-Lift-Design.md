# LFT-02 — Gas lift design

| Field | Value |
|---|---|
| Stage | STG-07 — Well performance and artificial lift |
| Area | Lift |
| Status | Gate met - see [evidence](../../evidence/STG-07-LFT-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.GasLift` |
| Desktop project | `Modules/Lift/GasLift/` |
| External packages | none |
| Depends on | LFT-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Calculations/GasLift.razor` |

## 1. Purpose

Design and check a gas lift installation: potential, valves, spacing and performance, with every
design limit reported from the owning project's message catalog.

## 2. Source components

| Source | Reuse |
|---|---|
| `GasLift/Services/GasLiftService.cs`, `.Advanced`, `.WireAsync` | service |
| `GasLift/Mapping/GasLiftPotentialWireMapper.cs` | potential curve mapping |
| `GasLift/Calculations/GasLiftPotentialCalculator.cs`, `GasLiftValveDesignCalculator.cs`, `GasLiftValveSpacingCalculator.cs` | design |
| `GasLift/Constants/GasLiftConstants.cs`, `GasLiftDesignLimitMessages.cs` | guardrails and messages |
| `GasLift/Data/Constants/GasLiftReferenceSets.cs`, `GasLiftReferenceCodeSeed.cs` | codes |
| Tables `GAS_LIFT_DESIGN`, `GAS_LIFT_PERFORMANCE`, `R_GAS_LIFT_REFERENCE_CODE` | persistence |

## 3. Screens

`Lift → Gas lift`

- Well and fluid inputs (from LFT-01 or explicit).
- Potential tab: injection rate vs liquid rate, with the operating recommendation.
- Valve design tab: port size, bellows, dome pressure, spread, opening/closing pressures; editable
  rows with a `DesignTable` control.
- Spacing tab: equal Δp or equal depth, valve list with depth and pressure per valve.
- Limits panel: a `LimitChip` for each violated rule, with the catalog message.
- Performance tab: unloading and producing gradients.
- Save / version.

## 4. Data and contracts

`OgGasLiftDesign { InjectionRate, InjectionUnit, PortSize, ValveCount, SpacingMethod, Valves[],
LimitViolations[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Limit violated | shown as a chip with the catalog message; save allowed but flagged |
| Blocking limit | save refused (e.g. valve count below minimum) |
| No injection available | design disabled with the reason |
| Two-phase unit mismatch | refused |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Design | `lift.gaslift.read` | no |
| Save design | `lift.gaslift.write` | yes |

## 7. Acceptance criteria

- AC-1 Every violated limit shows the message from `GasLiftDesignLimitMessages`.
- AC-2 A blocking limit prevents save with the specific message.
- AC-3 Valve spacing is reproducible for the chosen method.
- AC-4 Port sizes come from the reference set.
- AC-5 Injection and production units are explicit.
- AC-6 A saved design records the method and all limits evaluated.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LFT-02"
```

## 9. Open questions and risks

- `GasLiftService` is a concrete class; wrap it behind a desktop interface in `LocalServiceComposer`.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-07-LFT-02/observed-evidence.json). Designs are read and written
through `UnitofWork<T>` over the GasLift-owned `GAS_LIFT_DESIGN`, and the port sizes and limits come from
`R_GAS_LIFT_REFERENCE_CODE`.

**The open question above, answered:** `GasLiftService` is not used. Its calculators are static and are called directly
through a desktop interface (`IOgGasLift` / `OgGasLiftService`), the same way the other engineering screens wrap their
owners.

**The guardrails live in the owner.** `GasLiftDesignLimits` was added to the GasLift module: it evaluates the six limits
in `GasLiftConstants`, builds every sentence from `GasLiftDesignLimitMessages`, exposes the complete evaluated set and
decides which violations block a save (rate and valve count) and which are screening warnings (depth and spacing). The
screen prints them; it cannot invent one.

**Defects this feature found and fixed:**

- Three calculators passed the well's **Fahrenheit** temperatures into the Brill-Beggs Z-factor, which takes **Rankine**.
  Every ordinary well was refused as "temperature too low" (100 degF read as 100 R). The potential, spacing and
  valve-design calculators now add 459.67 at their own call sites, which is what the rest of the module already did.
- `AnalyzeGasLiftPotential` divided by the computed production rate without a guard, so the lowest injection rates threw
  `DivideByZeroException` and the whole analysis failed. A point that produces nothing is skipped, and an empty sweep
  returns without a recommendation instead of throwing on `First()`.

**A blocking limit does not stop the design, it stops the save (AC-2).** The engine can only place its own maximum of 20
valves, so a request for 25 is designed at 20 while the limit reports the request - the placed count and the violation are
both shown rather than one hiding the other.

**Not built:** `GRADIENT_BASED` is in the reference set but has no implementation and is refused with that reason; the
performance tab (unloading and producing gradients) is not surfaced; and the plan's `DesignTable` and `LimitChip` controls
were not added to the shared library - the valve list is a grid and the limits are a text panel.
