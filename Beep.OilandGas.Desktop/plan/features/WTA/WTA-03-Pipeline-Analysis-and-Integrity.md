# WTA-03 — Pipeline analysis and integrity

| Field | Value |
|---|---|
| Stage | STG-08 — Well test and pipeline |
| Area | Well test |
| Status | Gate met - see [evidence](../../evidence/STG-08-WTA-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PipelineAnalysis` |
| Desktop project | `Modules/WellTest/Pipeline/` |
| External packages | none |
| Depends on | RES-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Calculations/PipelineAnalysis.razor` |

## 1. Purpose

Size a pipeline and check it: capacity, flow regime, friction, erosion, leak detection, and the
optimization options — with the standard behind each threshold named.

## 2. Source components

| Source | Reuse |
|---|---|
| `PipelineAnalysis/Services/PipelineAnalysisService.cs` + partials `Advanced`, `DataManagement`, `ErosionCorrosion`, `Integrity`, `LeakDetection`, `Optimization`, `Reporting`, `Simulation`, `SlugFlow` | `IPipelineAnalysisService` |
| `PipelineAnalysis/Calculations/PipelineCapacityCalculator.cs`, `PipelineFlowCalculator.cs`, `PipelineCalculator.cs`, `ErosionPrediction.cs` | engine |
| `PipelineAnalysis/Constants/PipelineConstants.cs` | roughness, standards, conversions |
| `PipelineAnalysis/Validation/PipelineValidator.cs` | validation |
| Tables `PIPELINE`, `PIPELINE_ANALYSIS_RESULT` | persistence |

## 3. Screens

`Well test → Pipeline` with tabs:

- **Capacity** — gas (Weymouth) or liquid (Darcy-Weisbach); inputs and maximum rate.
- **Flow** — rate/pressure profile along the line, friction and Reynolds number, regime.
- **Erosion** — erosion rate, threshold and its source standard.
- **Leak detection** — pressure/flow anomaly summary.
- **Integrity** — wall loss, remaining life, re-rating reference.
- **Optimization** — diameter/pressure options ranked by cost.
- **Reporting** — export the analysis.
- **Slug flow / Simulation** — labelled experimental; disabled unless explicitly enabled.

## 4. Data and contracts

`OgPipelineResult { Mode, Capacity, CapacityUnit, Regime, Reynolds, FrictionFactor, ErosionRate,
ErosionThreshold, ThresholdSource, Feasible, Notes[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Flow regime not determined | refused with the input responsible |
| Erosion threshold without a source | result marked "threshold source not configured" |
| Experimental tab | disabled with an explanation |
| Missing fluid properties | refused, links to RES-01 |
| Failed read | failed state |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Analyse | `pipeline.read` | no |
| Save result | `pipeline.write` | yes |
| Enable experimental tabs | `pipeline.experimental` | no |

## 7. Acceptance criteria

- AC-1 A reference capacity case matches within tolerance.
- AC-2 Erosion always names its threshold source or says it is unconfigured.
- AC-3 Flow regime is displayed with the result.
- AC-4 Experimental tabs are disabled by default and labelled.
- AC-5 Export produces the same numbers as the screen.
- AC-6 Saved results record mode and inputs.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WTA-03"
```

## 9. Open questions and risks

- `PipelineAnalysisService` has ten partials; implement capacity, flow, erosion, leak and reporting
  in 1.0, and record the rest as deferred.
- `PipelineAnalysis` has no module setup; results persist through `LifeCycle` mappers.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-08-WTA-03/observed-evidence.json). Analyses are read and written
through `UnitofWork<T>` over the new PipelineAnalysis-owned `PIPELINE_ANALYSIS_REGISTER`.

**Both open questions, answered.** The module *does* have a module setup (`PipelineAnalysisModule`), and it already
registered `PIPELINE` and `PIPELINE_ANALYSIS_RESULT`, so the second note was out of date. Of the ten partials, capacity,
flow, erosion and reporting were read: **capacity and erosion are real calculations, reporting is a stub**, and leak,
integrity, optimisation, slug flow and simulation are not offered here. Deferred means not offered - not half-wired.

**A model defect that had to be recorded rather than fixed.** `PIPELINE_ANALYSIS_RESULT` declares **ten pairs of properties
whose names differ only in case** (`STATUS`/`Status`, `FLOW_RATE`/`FlowRate`, `PRESSURE_DROP`/`PressureDrop`,
`VELOCITY`/`Velocity`, `INLET_PRESSURE`/`InletPressure`, `OUTLET_PRESSURE`/`OutletPressure`,
`FLOW_REGIME`/`FlowRegime`, `ANALYSIS_ID`/`AnalysisId`, `PIPELINE_ID`/`PipelineId`, `ANALYSIS_DATE`/`AnalysisDate`). The
unit of work builds its property map with a case-insensitive dictionary and throws
`An item with the same key has already been added. Key: Status` **before it writes anything**, so no row can be persisted
through that entity at all, and a case-insensitive database cannot hold one table for it either. Deleting the
unused-looking duplicates broke the owner's own build - which is exactly what proved both sets are live
(`PipelineAnalysisService.DataManagement` writes the upper-case names, `PPDMCalculationService` the mixed-case ones). The
model was restored untouched and `PIPELINE_ANALYSIS_REGISTER` carries the screen's results until the owner collapses the
duplicates.

**The reporting service is a stub, so the export is built from the result.** `PipelineAnalysisService.Reporting` returns
canned findings ("Pipeline operating within normal parameters", "All safety factors within acceptable limits") whatever
analysis it is given. Exporting through it would print invented conclusions beside real numbers, so `ExportText` is
composed from the same properties the panels print and cannot disagree with them.

**A units trap:** the single `LENGTH` column is **miles** for the Weymouth relation and **feet** for the liquid
Darcy-Weisbach relation, so the screen takes miles once, converts for the liquid path, and states the conversion.

**The liquid capacity is a screening figure, and says so:** the module evaluates the liquid friction factor at a
substitute velocity of 5 ft/s rather than at the velocity its own answer implies, so the friction factor and the capacity
are not a converged Darcy-Weisbach solution. The test verifies the two identities the module does solve
(`v = sqrt(2 g D h / (f L))` and `Q = v A`) from the friction factor it reports, and the result's notes state the limit.

**Not built:** leak detection, integrity (wall loss and remaining life), optimisation, reporting, and the experimental
slug-flow/simulation tabs.
