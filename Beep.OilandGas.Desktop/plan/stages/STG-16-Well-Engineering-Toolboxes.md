# STG-16 — Well engineering toolboxes

| Field | Value |
|---|---|
| Stage | STG-16 |
| Features | 10 (ENG-01 … ENG-10) |
| Depends on | STG-15 |
| Status | Gate met - ENG-01..ENG-10 gate met ([ENG-01](../evidence/STG-16-ENG-01/observed-evidence.json), [ENG-02](../evidence/STG-16-ENG-02/observed-evidence.json), [ENG-03](../evidence/STG-16-ENG-03/observed-evidence.json), [ENG-04](../evidence/STG-16-ENG-04/observed-evidence.json), [ENG-05](../evidence/STG-16-ENG-05/observed-evidence.json), [ENG-06](../evidence/STG-16-ENG-06/observed-evidence.json), [ENG-07](../evidence/STG-16-ENG-07/observed-evidence.json), [ENG-08](../evidence/STG-16-ENG-08/observed-evidence.json), [ENG-09](../evidence/STG-16-ENG-09/observed-evidence.json), [ENG-10](../evidence/STG-16-ENG-10/observed-evidence.json)). The exit gate holds. |
| Exit gate | Survey/anti-collision, casing design, integrity, well plan, torque & drag, hydraulics, drilling performance, pay summary and log editing all run against a real well |

---

## 1. Objective

Expose the `OilandGasTools` well-engineering calculations that the `Beep.OilandGas` repository
does not have: survey uncertainty, anti-collision, API 5C3 casing design, metal-loss and
re-rating, annular pressure and MAASP, the integrity board, well planning, torque & drag,
hydraulics, drilling performance, pay summary / petrophysics, and log editing.

These are the screens that make the desktop genuinely more capable than the Web.

---

## 2. Scope

**In**

- Survey uncertainty (ISCWSA) and anti-collision scan with separation factor and MASD.
- Casing design: burst, collapse, tension, design factors, grade selection.
- Metal loss (MFC), re-rating, remaining life.
- Casing pressure diagnostics (API RP 90 classes), MAWOP/MAASP.
- Field integrity board with worst-first ordering and CSV export.
- Well plan: stations, build/turn, target.
- Torque & drag along a string.
- Hydraulics: ECD, bit loss, velocities, nozzle selection.
- Drilling performance: time-depth, NPT, performance analysis from DDR CSV.
- Pay summary, petrophysics (Vsh, porosity, Archie Sw), log editing (depth shift, splice).
- Crossplots (density-neutron, Pickett) as part of ENG-10.

**Out**

- Directional drilling control (no such capability).
- Cementing design beyond `CEMENT_JOB` records (DEV-05).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `TheTechIdea.OilGas.Wellbore.Data` | `SurveyUncertainty`, `AntiCollision` (+ `SeparationFactor`, `Masd`, `ToCsv`), `CasingDesign` (`Rate`, `Check`, `Burst`, `Collapse`, `Tension`, `YieldOf`), `PaySummary`, `Petrophysics`, `LogEditing`, `LeakPathAnalysis`, `BarrierValidator`, `MaaspScreening`, `Annuli`, `PressureAnalysis`, `TubularCatalog`, `IscwsaErrorModel`, `WorldMagneticModel`, `Units` |
| `TheTechIdea.OilGas.Integrity` | `MfcAnalysis`, `Rerating`, `CasingPressureDiagnostic`, `Mawop`, `IntegrityStatus`, `FieldIntegrity`, `IntegrityBoardView`, `CorrosionTrack`, `IntegrityToolbox` |
| `TheTechIdea.OilGas.Drilling` | `WellPlan`, `TorqueDrag`, `Hydraulics`, `DrillingPerformance`, `TimeDepthView`, `DrillingToolbox` |
| `TheTechIdea.OilGas.WellLogs` | `Crossplots`, `CrossplotView`, `CementEvaluation`, `CurveEditing`, `WellLogTrack.*` |
| `Beep.OilandGas.Drawing` | `CasingDesignTrack`, `WellborePathCalculator` where a repo renderer is preferable |
| `Beep.OilandGas.PPDM39` | Well, tubular, survey, log records |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| ENG-01 | Survey uncertainty and anti-collision | [ENG-01](../features/ENG/ENG-01-Survey-Uncertainty-and-Anti-Collision.md) |
| ENG-02 | Casing design | [ENG-02](../features/ENG/ENG-02-Casing-Design.md) |
| ENG-03 | Metal loss and re-rating | [ENG-03](../features/ENG/ENG-03-Metal-Loss-and-Re-Rating.md) |
| ENG-04 | Casing pressure and MAASP | [ENG-04](../features/ENG/ENG-04-Casing-Pressure-and-MAASP.md) |
| ENG-05 | Integrity status board | [ENG-05](../features/ENG/ENG-05-Integrity-Status-Board.md) |
| ENG-06 | Well plan | [ENG-06](../features/ENG/ENG-06-Well-Plan.md) |
| ENG-07 | Torque and drag | [ENG-07](../features/ENG/ENG-07-Torque-and-Drag.md) |
| ENG-08 | Hydraulics | [ENG-08](../features/ENG/ENG-08-Hydraulics.md) |
| ENG-09 | Drilling performance | [ENG-09](../features/ENG/ENG-09-Drilling-Performance.md) |
| ENG-10 | Pay summary, petrophysics and log editing | [ENG-10](../features/ENG/ENG-10-Pay-Summary-Petrophysics-and-Log-Editing.md) |

---

## 5. New code introduced

`.Modules/Wells` (extension) and a new `.Modules/Engineering` folder for the toolbox screens.
`.WellTools/Toolboxes` holds one view-model-facing wrapper per toolbox. `.Controls` gains
`DesignFactorGrid` and `ViolationList`.

---

## 6. Architecture notes

- **Toolbox classes are settings registries, not facades.** `CasingDesign.Rate(...)` is a static
  call; `IntegrityToolbox.Settings` supplies thresholds. Do not look for an
  `IWellboreToolbox`.
- **Every result carries its inputs and standard reference.** A casing check must show API 5C3
  and the design factor used.
- **Units come from the environment.** `Units.TryConvert` with the loaded `IUnitCatalog`; no
  hardcoded conversion factors.
- **Integrity status is a category, not a score.** Show Green/Yellow/Orange/Red with the rule
  that produced it.
- **Log editing is audited.** Depth shift and splice produce `TrackEditHistory` entries that can
  be undone; the desktop must surface undo/redo rather than hiding the history.

---

## 7. Dependencies and sequencing

All depend on `VSB-02`. ENG-03 → ENG-04 → ENG-05. The rest are independent.
Suggested order: ENG-01, ENG-02, ENG-10 (highest value), then ENG-03…05, then ENG-06…09.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-16-1 | Anti-collision matches a published ISCWSA reference case | test `ENG-01/T03` |
| SG-16-2 | Casing burst/collapse/tension reproduce API 5C3 worked examples | test `ENG-02/T02` |
| SG-16-3 | Re-rating uses the measured wall and states the remaining life basis | test `ENG-03/T03` |
| SG-16-4 | Casing pressure classification names the API RP 90 class | test `ENG-04/T02` |
| SG-16-5 | Integrity board orders worst-first and exports CSV | test `ENG-05/T03` |
| SG-16-6 | Well plan stations close on the target within tolerance | test `ENG-06/T02` |
| SG-16-7 | Torque & drag reports drag at each station and the limiting factor | test `ENG-07/T02` |
| SG-16-8 | Hydraulics ECD matches a reference case | test `ENG-08/T03` |
| SG-16-9 | Time-depth view renders from a DDR CSV | screenshot |
| SG-16-10 | Pay summary values match a reference log interval | test `ENG-10/T03` |
| SG-16-11 | Depth shift is undoable and audited | test `ENG-10/T05` |

---

## 9. Test plan

`ENG-01/T03`, `ENG-02/T02`, `ENG-03/T03`, `ENG-04/T02`, `ENG-05/T03`, `ENG-06/T02`,
`ENG-07/T02`, `ENG-08/T03`, `ENG-09/T02`, `ENG-10/T03`, `ENG-10/T05`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| Reference cases live in `OilandGasTools` tests, not in this repository | Port the specific reference assertions into the desktop test project rather than re-deriving values |
| `WorldMagneticModel` needs `WMM2025.COF` at runtime | Confirm the embedded resource is copied by the package; test it |
| `CasingDesign.Check` may need a `CasingLoadCase` that comes from configuration | Provide a load-case editor in ENG-02 rather than hardcoding |
| Environment settings silently change results | Show the active environment file path on every engineering screen |
