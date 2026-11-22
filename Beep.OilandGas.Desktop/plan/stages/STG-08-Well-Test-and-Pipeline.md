# STG-08 — Well test and pipeline

| Field | Value |
|---|---|
| Stage | STG-08 |
| Features | 3 (WTA-01 … WTA-03) |
| Depends on | STG-06, STG-07 |
| Status | Gate met - WTA-01, WTA-02 and WTA-03 met ([WTA-01](../evidence/STG-08-WTA-01/observed-evidence.json), [WTA-02](../evidence/STG-08-WTA-02/observed-evidence.json), [WTA-03](../evidence/STG-08-WTA-03/observed-evidence.json)). The exit gate holds: a pressure transient analysis produces permeability and skin with an independent cross-check, a well test register links each test to exactly one well and its analyses, and a line is sized and checked for erosion with the standard named. Exit-gate items SG-08-1..SG-08-6 are met. |
| Exit gate | A pressure transient analysis produces permeability and skin, a well test register links to it, and a pipeline can be sized and checked for integrity |

---

## 1. Objective

Complete the well performance picture with pressure transient analysis, add the well test
register that stores and links tests, and expose pipeline capacity, flow, erosion, leak and
integrity analysis.

---

## 2. Scope

**In**

- Well test analysis: Horner, MDH, drawdown, derivative, gas well, model identification.
- Well test register: recorded tests, units, results, linkage to wells and analyses.
- Pipeline analysis: Weymouth/Darcy-Weisbach capacity and flow, friction, Reynolds, regime.
- Pipeline integrity: erosion prediction, leak detection, slug flow, simulation, optimization,
  reporting.

**Out**

- Real-time pipeline SCADA.
- Any flow assurance not implemented (hydrate, wax — not present in either repo).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.WellTestAnalysis` | `IWellTestAnalysisService`, `WellTestAnalyzer`, `BuildUpAnalysis`, `DrawdownAnalysis`, `DerivativeAnalysis`, `GasWellAnalysis`, `WellTestRenderer`, `WellTestInteractionHandler`, `WellTestConstants` |
| `Beep.OilandGas.PipelineAnalysis` | `IPipelineAnalysisService`, 10 partials (`Advanced`, `DataManagement`, `ErosionCorrosion`, `Integrity`, `LeakDetection`, `Optimization`, `Reporting`, `Simulation`, `SlugFlow`) |
| `Beep.OilandGas.LifeCycle` | `WellManagementService` for WTA-02 records, `PPDMCalculationService` (WellTest partial) |
| `TheTechIdea.OilGas.Production` | `HornerAnalysis` for cross-check |
| `Beep.OilandGas.Models` | `WellTestAnalysisWellKnown` tokens |
| `Beep.OilandGas.Client` | `.Analysis`, `.Calculations` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| WTA-01 | Well test analysis | [WTA-01](../features/WTA/WTA-01-Well-Test-Analysis.md) |
| WTA-02 | Well test register | [WTA-02](../features/WTA/WTA-02-Well-Test-Register.md) |
| WTA-03 | Pipeline analysis and integrity | [WTA-03](../features/WTA/WTA-03-Pipeline-Analysis-and-Integrity.md) |

---

## 5. New code introduced

`.Modules/WellTest` — three pairs. Reuses `CalculationShell` and `LiftChart` from STG-07.

---

## 6. Architecture notes

- **Analysis type and method are recorded tokens**, from `WellTestAnalysisWellKnown` and
  `R_WELL_TEST_TYPE` — never free text.
- **P/T data must carry units.** A build-up analysis on mixed units is refused.
- **Cross-check the Horner slope** against `TheTechIdea.OilGas.Production.HornerAnalysis` on the
  same readings; a discrepancy beyond tolerance is surfaced, not hidden.
- **Pipeline integrity results are advisory.** The screen must state which standard the erosion
  threshold came from; the repo has uncentralized thresholds (see
  `ENGINEERING_PROJECTS_ANALYSIS.md` §6).

---

## 7. Dependencies and sequencing

WTA-01 → WTA-02. WTA-03 independent after RES-01.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-08-1 | Horner analysis on a reference build-up returns permeability within 1 % of the published value | test `WTA-01/T02` |
| SG-08-2 | Horner slope agrees with the tools `HornerAnalysis` within tolerance | test `WTA-01/T05` |
| SG-08-3 | A non-converged derivative is shown as unusable, not plotted as data | test `WTA-01/T04` |
| SG-08-4 | Test record links to exactly one well and one analysis | test `WTA-02/T02` |
| SG-08-5 | Pipeline capacity matches a reference case | test `WTA-03/T02` |
| SG-08-6 | Erosion result names its threshold source | test `WTA-03/T04` |

---

## 9. Test plan

`WTA-01/T02`, `WTA-01/T04`, `WTA-01/T05`, `WTA-02/T02`, `WTA-03/T02`, `WTA-03/T04`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| `IPipelineAnalysisService` exposes a very large surface | Implement capacity, flow, erosion, leak and reporting only; document the rest as deferred |
| `WellTestAnalysisService.Unimplemented.cs` suggests partial implementation | Enumerate which analyses are real before designing the UI; record in WTA-01 |
| Pipeline has no module setup | Results persist through `LifeCycle` mappers; state this in the feature doc |
