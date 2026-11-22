# STG-07 — Well performance and artificial lift

| Field | Value |
|---|---|
| Stage | STG-07 |
| Features | 8 (LFT-01 … LFT-08) |
| Depends on | STG-06 |
| Status | Gate met - LFT-01..LFT-08 met ([LFT-01](../evidence/STG-07-LFT-01/observed-evidence.json), [LFT-02](../evidence/STG-07-LFT-02/observed-evidence.json), [LFT-03](../evidence/STG-07-LFT-03/observed-evidence.json), [LFT-04](../evidence/STG-07-LFT-04/observed-evidence.json), [LFT-05](../evidence/STG-07-LFT-05/observed-evidence.json), [LFT-06](../evidence/STG-07-LFT-06/observed-evidence.json), [LFT-07](../evidence/STG-07-LFT-07/observed-evidence.json), [LFT-08](../evidence/STG-07-LFT-08/observed-evidence.json)). The exit gate holds: a nodal study, a gas-lift design, an ESP/pump duty, an SRP design, a plunger assessment, a hydraulic pump design, a choke size and a compressor performance run all complete and persist, and SG-07-1..SG-07-6 and SG-07-9 are each covered by a named test. |
| Exit gate | A nodal study, a gas-lift design, an ESP selection, an SRP design, a plunger assessment, a hydraulic pump design, a choke size and a compressor performance run all complete and persist |

---

## 1. Objective

This is the largest engineering stage and the one users will judge the product by. It exposes
every artificial-lift and flow-restriction engine in the repository, each with its own
interaction handler and (for some) its own SkiaSharp renderer.

---

## 2. Scope

**In**

- Nodal analysis: IPR/VLP, operating point, sensitivity, artificial-lift ranking, diagnostics.
- Gas lift: potential, valve design (US/SI), spacing (equal Δp, equal depth), performance curves.
- ESP and pump performance: H-Q, efficiency, power, NPSH, affinity laws, viscosity correction.
- Sucker rod pumping: loads, stresses, fatigue, pump card, rod string optimization.
- Plunger lift: cycle analysis, gas requirement, feasibility, optimization.
- Hydraulic pumps: jet and piston design, balance, cavitation, diagnostics.
- Choke analysis: correlations, sonic/subsonic, sizing, erosion.
- Compressor analysis: centrifugal and reciprocating, staging, efficiency, feasibility.

**Out**

- Heat map (ADR-006, deferred to INT-07).
- Any lift technique not implemented in the repositories (PCP exists only as a reference code).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.NodalAnalysis` | `INodalAnalysisService`, `.Advanced`, `.ArtificialLiftScoring`, `.SensitivityScenarios`, `NodalRenderer`, `NodalInteractionHandler` |
| `Beep.OilandGas.GasLift` | `GasLiftService`, `.Advanced`, `.WireAsync`, `GasLiftPotentialWireMapper`, `GasLiftDesignLimitMessages` |
| `Beep.OilandGas.PumpPerformance` | `IPumpPerformanceService`, `PumpPerformanceRenderer`, `PumpPerformanceInteractionHandler`, `PumpTypes/*`, `SystemAnalysis/*` |
| `Beep.OilandGas.SuckerRodPumping` | `ISuckerRodPumpingService`, `RodStringOptimization`, `FatigueAnalysis` |
| `Beep.OilandGas.PlungerLift` | `IPlungerLiftService`, `PlungerLiftService.Advanced` |
| `Beep.OilandGas.HydraulicPumps` | `IHydraulicPumpService`, `HydraulicPumpService.Advanced` |
| `Beep.OilandGas.ChokeAnalysis` | `IChokeAnalysisService`, `.Advanced`, `.Multiphase` |
| `Beep.OilandGas.CompressorAnalysis` | `ICompressorAnalysisService`, `.Advanced`, `.Contract` |
| `TheTechIdea.OilGas.Production` | `NodalAnalysis.Solve`, `Vogel`, `VerticalLift` for independent cross-check |
| `Beep.OilandGas.Client` | `.Pumps`, `.Calculations`, `.Analysis` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| LFT-01 | Nodal analysis | [LFT-01](../features/LFT/LFT-01-Nodal-Analysis.md) |
| LFT-02 | Gas lift design | [LFT-02](../features/LFT/LFT-02-Gas-Lift-Design.md) |
| LFT-03 | ESP and pump performance | [LFT-03](../features/LFT/LFT-03-ESP-and-Pump-Performance.md) |
| LFT-04 | Sucker rod pumping | [LFT-04](../features/LFT/LFT-04-Sucker-Rod-Pumping.md) |
| LFT-05 | Plunger lift | [LFT-05](../features/LFT/LFT-05-Plunger-Lift.md) |
| LFT-06 | Hydraulic pumps | [LFT-06](../features/LFT/LFT-06-Hydraulic-Pumps.md) |
| LFT-07 | Choke analysis | [LFT-07](../features/LFT/LFT-07-Choke-Analysis.md) |
| LFT-08 | Compressor analysis | [LFT-08](../features/LFT/LFT-08-Compressor-Analysis.md) |

---

## 5. New code introduced

`.Modules/Lift` — eight pairs. `.Controls` gains `LiftChart` (SKElement + interaction handler),
`DesignTable` (editable design rows), `LimitChip` (design-limit violations from
`GasLiftDesignLimitMessages` and the other `*LimitMessages` classes).

---

## 6. Architecture notes

- **One calculation shell, eight parameter sets.** Build a `CalculationShellViewModel<TInput,
  TResult>` once in this stage and instantiate it. Do not write eight different run/save/history
  flows.
- **Renderers are used, not reimplemented.** `NodalRenderer.Render(SKCanvas, w, h)`,
  `PumpPerformanceRenderer`, `WellTestRenderer` and `EconomicRenderer` all share this signature.
- **Design limits are shown from the owning project's message catalog**, never hardcoded in the
  view. `GasLiftDesignLimitMessages` is the model to copy for the others.
- **Two independent engines where possible.** For nodal and DCA the tools packages offer a second
  implementation; the feature documents specify a cross-check test rather than trusting one.

---

## 7. Dependencies and sequencing

LFT-01 needs RES-01 and RES-02 (gas and oil properties). LFT-02 needs LFT-01.
LFT-03…06 are independent once RES-01 exists. LFT-07 and LFT-08 need RES-01.

Build order: CalulationShell → LFT-01 → LFT-02 → LFT-03 → LFT-04 → LFT-05 → LFT-06 →
LFT-07 → LFT-08.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-07-1 | `CalculationShell` is used by all eight features (no bespoke run loop) | code review note |
| SG-07-2 | Nodal operating point matches the tools `Production.NodalAnalysis.Solve` within tolerance | test `LFT-01/T05` |
| SG-07-3 | Gas lift design reports each violated design limit with its catalog message | test `LFT-02/T04` |
| SG-07-4 | ESP selection refuses an operating point outside the published curve | test `LFT-03/T03` |
| SG-07-5 | SRP design reports stress and safety factor and refuses an unsafe string | test `LFT-04/T03` |
| SG-07-6 | Plunger feasibility distinguishes infeasible from uneconomic | test `LFT-05/T02` |
| SG-07-7 | Choke analysis states the flow regime and the correlation used | test `LFT-07/T02` |
| SG-07-8 | Compressor staging respects interstage cooling limits | test `LFT-08/T03` |
| SG-07-9 | Every saved design records inputs, method, actor and limit outcomes | test `STG-07/T09` |

---

## 9. Test plan

`LFT-01/T05`, `LFT-02/T04`, `LFT-03/T03`, `LFT-04/T03`, `LFT-05/T02`, `LFT-06/T02`,
`LFT-07/T02`, `LFT-08/T03`, `STG-07/T09`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| `IHydraulicPumpService` and `IPipelineAnalysisService` are very large DTO surfaces | Implement only the design/sizing subset in 1.0; record the rest as deferred inside the feature doc |
| Four renderers, four interaction handlers, one host control | `VSB-01` must land before or alongside this stage; otherwise LFT-01/03/08 render into a placeholder |
| Design limit messages exist for gas lift only | Where absent, the view shows the raw violation code and the feature doc records the gap |
