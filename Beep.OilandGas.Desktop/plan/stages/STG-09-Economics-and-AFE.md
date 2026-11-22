# STG-09 — Economics and AFE

| Field | Value |
|---|---|
| Stage | STG-09 |
| Features | 3 (ECO-01 … ECO-03) |
| Depends on | STG-04, STG-05, STG-06 |
| Status | Gate met - ECO-01, ECO-02 and ECO-03 met ([ECO-01](../evidence/STG-09-ECO-01/observed-evidence.json), [ECO-02](../evidence/STG-09-ECO-02/observed-evidence.json), [ECO-03](../evidence/STG-09-ECO-03/observed-evidence.json)). The exit gate holds: an economic evaluation runs from a forecast, an AFE is raised, supplemented and approved, and an investment decision is recorded and handed off as one AFE. Exit-gate items SG-09-1..SG-09-5 are met. |
| Exit gate | An economic evaluation runs from a forecast, an AFE is raised and approved, and an investment decision is recorded |

---

## 1. Objective

Turn engineering output into money: economic evaluation over a forecast, AFE raising and
approval, scenario comparison, and the investment review queue that connects engineering
proposals to accounting commitments.

---

## 2. Scope

**In**

- Economic evaluation: cash flow, NPV, IRR, payback, fiscal regime, sensitivity.
- Scenario and schedule management (`R_ECONOMIC_SCENARIO`, `R_ECONOMIC_SCHEDULE`).
- AFE: raise, review, approve, revise, close; link to development plan and work order.
- Investment review queue with handoff to accounting.

**Out**

- Tax computation (ACC-08 owns tax).
- Portfolio capital allocation optimization (not implemented).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.EconomicAnalysis` | `IEconomicAnalysisService`, `ICalculationService`, `EconomicAnalysisService.Advanced`, `EconomicRenderer`, `EconomicInteractionHandler`, `R_ECONOMIC_METRIC/SCENARIO/SCHEDULE` |
| `Beep.OilandGas.ProductionAccounting` | `AfeService` and 5 partials |
| `Beep.OilandGas.ProductionForecasting` | Forecast volumes as the economic input |
| `Beep.OilandGas.LifeCycle` | `Accounting*Service`, approval engine for ECO-03 |
| `Beep.OilandGas.Client` | `.Analysis`, `.Accounting` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| ECO-01 | Economic evaluation | [ECO-01](../features/ECO/ECO-01-Economic-Evaluation.md) |
| ECO-02 | AFE management | [ECO-02](../features/ECO/ECO-02-AFE-Management.md) |
| ECO-03 | Investment review and handoff queue | [ECO-03](../features/ECO/ECO-03-Investment-Review-and-Handoff-Queue.md) |

---

## 5. New code introduced

`.Modules/Economics` — three pairs. `.Controls` gains `CashflowGrid` (period rows with
year/quarter/month grain) and `SensitivityTornado`.

---

## 6. Architecture notes

- **All money is `decimal`.** Never `double`. The repo already refuses unrepresentable values in
  financial paths; keep that behaviour.
- **Fiscal terms come from configuration**, not literals. A missing term blocks the run with a
  named reason.
- **AFE numbering is server-owned.**
- **Sensitivity output is a table plus a chart of the same data** — never two independent
  calculations.

---

## 7. Dependencies and sequencing

ECO-01 needs RES-07 (forecast) and RES-05 (reserves). ECO-02 needs ACC-05 (procurement) for
commitment linkage — or accepts a manual reference until STG-10 lands. ECO-03 needs WFL-04.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-09-1 | NPV of a flat cash-flow reference case matches the closed-form value | test `ECO-01/T02` |
| SG-09-2 | A missing fiscal term blocks the run with a named reason | test `ECO-01/T04` |
| SG-09-3 | Scenario comparison uses one forecast per scenario and states which | test `ECO-01/T06` |
| SG-09-4 | AFE revision supersedes without mutating the approved version | test `ECO-02/T03` |
| SG-09-5 | Investment decision records actor, date, rationale and evidence | test `ECO-03/T02` |

---

## 9. Test plan

`ECO-01/T02`, `ECO-01/T04`, `ECO-01/T06`, `ECO-02/T03`, `ECO-03/T02`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| Fiscal regime model is not documented | Read `EconomicAnalysisService.Advanced` and write the regime shape into ECO-01 §4 |
| AFE has five partials and unclear state machine | Extract the transition list before building the UI |
