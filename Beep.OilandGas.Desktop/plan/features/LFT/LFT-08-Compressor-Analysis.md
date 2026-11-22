# LFT-08 — Compressor analysis

| Field | Value |
|---|---|
| Stage | STG-07 — Well performance and artificial lift |
| Area | Lift |
| Status | Gate met - see [evidence](../../evidence/STG-07-LFT-08/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.CompressorAnalysis` |
| Desktop project | `Modules/Lift/Compressor/` |
| External packages | none |
| Depends on | RES-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Calculations/CompressorAnalysis.razor` (`/analysis/compressor`) |

## 1. Purpose

Size and check a gas compressor — centrifugal or reciprocating — including staging, head, power,
efficiency and feasibility.

## 2. Source components

| Source | Reuse |
|---|---|
| `CompressorAnalysis/Services/CompressorAnalysisService.cs`, `.Advanced`, `.Contract` | `ICompressorAnalysisService` |
| `CompressorAnalysis/Calculations/CentrifugalCompressorCalculator.cs`, `ReciprocatingCompressorCalculator.cs`, `CompressorPressureCalculator.cs` | engine |
| `CompressorAnalysis/Validation/CompressorValidator.cs` | validation |
| `CompressorAnalysis/Data/Constants/CompressorConstants.cs`, `CompressorAnalysisReferenceSets.cs` | constants and codes |
| `Beep.OilandGas.Models` `CompressorAnalysisWellKnown` | analysis and type tokens |
| Tables `COMPRESSOR_OPERATING_CONDITIONS`, `CENTRIFUGAL_COMPRESSOR_PROPERTIES`, `RECIPROCATING_COMPRESSOR_PROPERTIES`, `COMPRESSOR_POWER_RESULT`, `COMPRESSOR_PRESSURE_RESULT` | persistence |

## 3. Screens

`Lift → Compressor`

- Inputs: inlet/outlet pressure, inlet temperature, flow, gas composition, compressor type.
- Staging: computed stage count, pressure ratio per stage, interstage cooling.
- Centrifugal tab: polytropic/adiabatic head, efficiency, speed, power, surge margin.
- Reciprocating tab: volumetric efficiency, clearance, rod load, power.
- Feasibility panel: reasons a machine cannot meet the duty.
- Result and save.

## 4. Data and contracts

`OgCompressorResult { Kind, Stages, PressureRatio, Head, HeadUnit, Efficiency, Power, PowerUnit,
Feasible, Reasons[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Infeasible | reasons listed; no power figure presented as achieved |
| Interstage temperature above limit | refused with the limit |
| Type not selected | analysis disabled with the reason |
| Out of validity range | marked |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Analyse | `lift.compressor.read` | no |
| Save result | `lift.compressor.write` | yes |

## 7. Acceptance criteria

- AC-1 A reference centrifugal case reproduces head and power within tolerance.
- AC-2 Staging respects the interstage temperature limit.
- AC-3 Infeasibility lists every reason.
- AC-4 Analysis and type tokens come from `CompressorAnalysisWellKnown`.
- AC-5 Power carries a unit.
- AC-6 Saved results record type, staging and efficiency.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~LFT-08"
```

## 9. Open questions and risks

- `CompressorAnalysisService` uses table-name literals (`COMPRESSOR_DESIGN`,
  `COMPRESSOR_PERFORMANCE`) that should be constants; do not replicate them in the desktop.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-07-LFT-08/observed-evidence.json). Results are read and written
through `UnitofWork<T>` over CompressorAnalysis's own `COMPRESSOR_POWER_RESULT`, extended additively with the analysis
columns.

**The open question above, answered:** the desktop never names either table. It writes `COMPRESSOR_POWER_RESULT` through
the unit of work and a source guard asserts that neither `COMPRESSOR_DESIGN` nor `COMPRESSOR_PERFORMANCE` appears in the
view.

**A serious engine defect this feature found and fixed.** The polytropic exponent was computed as
`n = k·η / (k − η·(k−1))` in **four places** (`CentrifugalCompressorCalculator`'s head and discharge temperature,
`MultistageCompressor.AnalyzeMultistage`, and `CompressorOptimization`). For natural gas at k = 1.3 and η = 0.75 that is
**n = 0.907** - an exponent below 1, which means the gas cools as it is compressed. The symptoms were visible from the
desk: the discharge temperature came back **equal to the suction temperature** (the method clamps with `Math.Max`, so it
silently returned the suction value), the head was built from **two negative factors** (-9.75 and -0.106) that happened
to multiply to a positive number of the wrong magnitude, and **the interstage temperature check could never fire** - the
one thing AC-2 depends on. The textbook relation `n/(n−1) = k/(k−1)·η_p`, i.e. `n = k·η/(k·η − (k−1))`, now applies in
all four places, and the isentropic special case is unchanged. The same duty now gives 784.8 R (325 °F) at one stage.

**Why the centrifugal path uses the staging engine:** the staged machine is what `AnalyzeMultistage` describes, so its
result is the answer - the summed stage head, the total motor power and the real final discharge temperature. A stage
count the engineer fixes is judged against the same limits rather than assumed acceptable.

**AC-1 without re-implementing the engine:** the test recomputes the staged head and motor power from the published
relations, taking **only the Z-factor** from the module's own correlation, and agrees within 1%.

**Not applied to a reciprocating machine:** the 660 R (200 °F) limit. The module states it for its centrifugal staging
plan, and a reciprocating discharge of 261.5 °F at a ratio of 3 is ordinary by API 618 practice. The screen reports that
temperature and does not judge it, because inventing a threshold would have been worse than leaving it out.

**Not built:** the surge margin (it needs a manufacturer's curve this repository does not have), and a composition-based Z
or k (both calculators take a single gas specific gravity).
