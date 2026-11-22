# STG-06 — Reservoir, PVT and forecasting

| Field | Value |
|---|---|
| Stage | STG-06 |
| Features | 7 (RES-01 … RES-07) |
| Depends on | STG-01, STG-02 |
| Status | Gate met - RES-01..RES-07 met ([RES-01](../evidence/STG-06-RES-01/observed-evidence.json), [RES-02](../evidence/STG-06-RES-02/observed-evidence.json), [RES-03](../evidence/STG-06-RES-03/observed-evidence.json), [RES-04](../evidence/STG-06-RES-04/observed-evidence.json), [RES-05](../evidence/STG-06-RES-05/observed-evidence.json), [RES-06](../evidence/STG-06-RES-06/observed-evidence.json), [RES-07](../evidence/STG-06-RES-07/observed-evidence.json)). The exit gate holds: a PVT study, a flash calculation, an EOR screen, a reserves classification, a DCA fit and a forecast all run and persist, and SG-06-1..SG-06-8 are each covered by a named test. |
| Exit gate | A PVT study, a flash calculation, an EOR screen, a reserves classification, a DCA fit and a forecast all run and persist |

---

## 1. Objective

Expose the fluid and reservoir calculation engines. These are the first screens where the
desktop does real engineering, and they are also the foundation for STG-07 (lift) and STG-08
(test analysis), which consume gas and oil properties.

---

## 2. Scope

**In**

- Gas properties: Z-factor, viscosity, pseudo-pressure, average properties.
- Oil properties: bubble point, solution GOR, FVF, viscosities, black-oil screening.
- Flash: isothermal flash, multi-component, phase envelope, separator stages.
- EOR screening: water flood, gas injection, chemical, thermal.
- Reserves classification and reservoir characterization summary.
- Decline curve analysis (both the repo engine and the tools `DeclineFitter` for cross-check).
- Production forecasting with scenarios and economics hooks.

**Out**

- Reservoir simulation (neither repo has it).
- Volumetric estimation beyond the recorded `PROSPECT_VOLUME_ESTIMATE` and `RESERVES` records.

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.GasProperties` | `IGasPropertiesService`, `GasPropertiesService.Advanced`, `ZFactorCalculator`, `GasViscosityCalculator`, `PseudoPressureCalculator` |
| `Beep.OilandGas.OilProperties` | `IOilPropertiesService`, `OilPropertiesService.Advanced/.CompositionScreening`, `OilPropertyCalculator`, `BlackOilScreening` |
| `Beep.OilandGas.FlashCalculations` | `FlashCalculationService`, `.Advanced`, `FlashFeedCatalogMerge`, `PhaseEnvelope`, `AdvancedEOS`, `MultiComponentFlash` |
| `Beep.OilandGas.EnhancedRecovery` | `IEnhancedRecoveryOperationsService`, `EnhancedRecoveryService.Advanced` |
| `Beep.OilandGas.ProductionForecasting` | `IProductionForecastingService`, `DCAAnalysisService.Advanced`, `ProductionHistoryLoader`, `ProductionForecastResultMapper`, `DCAInteractionHandler` |
| `Beep.OilandGas.LifeCycle` | `ReservoirLifecycleService`, `PPDMCalculationService` (18 partials) for cross-module orchestration |
| `TheTechIdea.OilGas.Production` | `DeclineFitter`, `Vogel` — graphics-free cross-check |
| `Beep.OilandGas.Client` | `.Properties`, `.Calculations`, `.Analysis`, `.Production` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| RES-01 | Gas properties | [RES-01](../features/RES/RES-01-Gas-Properties.md) |
| RES-02 | Oil properties | [RES-02](../features/RES/RES-02-Oil-Properties.md) |
| RES-03 | Flash calculations and phase envelope | [RES-03](../features/RES/RES-03-Flash-Calculations-and-Phase-Envelope.md) |
| RES-04 | EOR screening | [RES-04](../features/RES/RES-04-EOR-Screening.md) |
| RES-05 | Reserves classification and characterization | [RES-05](../features/RES/RES-05-Reserves-Classification-and-Characterization.md) |
| RES-06 | Decline curve analysis | [RES-06](../features/RES/RES-06-Decline-Curve-Analysis.md) |
| RES-07 | Production forecasting | [RES-07](../features/RES/RES-07-Production-Forecasting.md) |

---

## 5. New code introduced

`.Modules/Reservoir` — seven pairs. `.Controls` gains `CurveCanvas` (SKElement chart host bound to
an interaction handler) and `ResultTabs` (calculation inputs / results / diagnostics / audit).

---

## 6. Architecture notes

- **Calculation screens follow one shape:** inputs (with units) → run → results tabs → save →
  history. Every calculation records its inputs and the correlation/model used, so a result can
  be reproduced.
- **No default-to-zero.** If an input is missing, the run button is disabled with a reason; the
  engine is never called with a guessed value.
- **Correlation names come from the reference tables** (`R_*` codes), not literals.
- **Charts are interaction handlers, not re-implementations.** `DCAInteractionHandler`,
  `NodalInteractionHandler` etc. already exist and are UI-framework-free.

---

## 7. Dependencies and sequencing

RES-01 and RES-02 → RES-03 (flash needs composition and properties).
RES-04, RES-05 independent. RES-06 → RES-07.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-06-1 | A gas property run reproduces a known reference case | test `RES-01/T02` |
| SG-06-2 | An oil property run reproduces a known reference case | test `RES-02/T02` |
| SG-06-3 | Flash converges and reports convergence status honestly | test `RES-03/T03` |
| SG-06-4 | A non-converged flash is shown as failed, not as a result | test `RES-03/T04` |
| SG-06-5 | EOR screen shows method-specific economics from constants, not literals in the view | test `RES-04/T02` |
| SG-06-6 | DCA fit reports b within bounds and refuses an unbounded fit | test `RES-06/T03` |
| SG-06-7 | Forecast persists points and can be reloaded identically | test `RES-07/T04` |
| SG-06-8 | Every saved calculation records inputs, model and actor | test `STG-06/T08` |

---

## 9. Test plan

`RES-01/T02`, `RES-02/T02`, `RES-03/T03`, `RES-03/T04`, `RES-04/T02`, `RES-05/T01`,
`RES-06/T03`, `RES-07/T04`, `STG-06/T08`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| `NodalConstants` / `ForecastAlgorithmConstants` defaults could mask missing input | Always require explicit input; defaults only shown as placeholders |
| `ChokeAnalysisService.Advanced` and `EnhancedRecoveryService.Advanced` contain uncentralized magic numbers (see `ENGINEERING_PROJECTS_ANALYSIS.md`) | Do not surface those values in the UI; file a constants-refactor task separately |
| Flash engine has no interface (concrete class) | Wrap it in `.Data`/`LocalServiceComposer` behind a desktop interface |
