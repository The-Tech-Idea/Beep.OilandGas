# STG-03 — Exploration and seismic

| Field | Value |
|---|---|
| Stage | STG-03 |
| Features | 5 (EXP-01 … EXP-05) |
| Depends on | STG-02 |
| Status | Gate met - EXP-01..05 all met: exploration workspace, prospect board, prospect editor, seismic register and well program gate |
| Exit gate | A prospect can be created, matured through stages with risk assessment, and approved into a well program |

---

## 1. Objective

Port the exploration half of the lifecycle: prospect and play/lead maturation, risk and chance
of success, seismic survey tracking, and the well program approval gate that hands work to
drilling.

---

## 2. Scope

**In**

- Exploration workspace with persona inbox and KPI cards.
- Prospect board as a Kanban-style maturation board over `PROSPECT_WORKFLOW_STAGE`.
- Prospect editor: identity, trap, reservoir, source rock, volume estimate, risk assessment.
- Play and lead registers.
- Seismic survey register, lines, acquisition and processing status.
- Well program approval workflow with evidence.

**Out**

- Lease acquisition — separate stage `STG-03a`.
- 3D/2D seismic interpretation (no such capability in either repository).
- Seismic data loading (no SEG-Y reader exists; state this explicitly).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.ProspectIdentification` | `IProspectIdentificationService`, `IFieldExplorationService`, `IProspectRiskEconomicAnalysisService`, `IProspectTechnicalMaturationService`, `IProspectPortfolioOptimizationService`, `ProspectEvaluationService`, `SeismicAnalysisService`, `PPDMExplorationService`, `ILeadExplorationService`; 27 `PROSPECT*` tables, `PLAY`, `LEAD`, `EXPLORATION_*`, `R_PLAY_TYPE`, `R_LEAD_STATUS` |
| `Beep.OilandGas.LifeCycle` | `Exploration*Service`, `Processes/Exploration*`, approval engine for EXP-05 |
| `Beep.OilandGas.Client` | `.Field`, `.LifeCycle` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| EXP-01 | Exploration workspace | [EXP-01](../features/EXP/EXP-01-Exploration-Workspace.md) |
| EXP-02 | Prospect maturation board | [EXP-02](../features/EXP/EXP-02-Prospect-Maturation-Board.md) |
| EXP-03 | Prospect editor and risk assessment | [EXP-03](../features/EXP/EXP-03-Prospect-Editor-and-Risk-Assessment.md) |
| EXP-04 | Seismic surveys, lines and processing | [EXP-04](../features/EXP/EXP-04-Seismic-Surveys-Lines-and-Processing.md) |
| EXP-05 | Well program approval | [EXP-05](../features/EXP/EXP-05-Well-Program-Approval.md) |

---

## 5. New code introduced

`.Modules/Exploration` — five view-model/view pairs; `ProspectCard` control; a Kanban board
control in `.Controls` (`StageBoard`) reused by WFL-03.

---

## 6. Architecture notes

- **No inferred chance.** Probability of success must come from a recorded risk assessment with
  one row per risk element. If elements are missing, show "incomplete", not a computed guess.
  Web removed exactly this kind of invention in its exploration landing.
- **Stage transitions are workflow events**, not property writes. Advancing a stage goes through
  the process engine (STG-14) so the audit trail is real. Until STG-14 lands, EXP-02 must refuse
  to advance rather than perform a direct update — the same decision Web reached.
- **Seismic has no data-plane.** The register tracks surveys and processing status only. Say so
  in the UI so a user does not expect a SEG-Y viewer.

---

## 7. Dependencies and sequencing

EXP-01 → EXP-02 → EXP-03. EXP-04 independent. EXP-05 needs WFL-04 (gate approvals) or a
documented interim that only *records* the decision.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-03-1 | Prospect created with all mandatory risk elements | test `EXP-03/T01` |
| SG-03-2 | Missing risk elements show incomplete, never a number | test `EXP-03/T03` |
| SG-03-3 | Board groups prospects by recorded stage and refuses an unrecorded stage | test `EXP-02/T02` |
| SG-03-4 | Stage advance is refused until the process engine is wired | test `EXP-02/T04` |
| SG-03-5 | Seismic register lists surveys with acquisition and processing state | screenshot |
| SG-03-6 | Well program approval records decision, actor and evidence | test `EXP-05/T02` |

---

## 9. Test plan

`EXP-01/T01`, `EXP-02/T02`, `EXP-02/T04`, `EXP-03/T01`, `EXP-03/T03`, `EXP-04/T01`,
`EXP-05/T02`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| `PROSPECT` is a 27-table cluster; a naive editor becomes unusable | Split into tabs; only the identity + risk tabs are editable in 1.0 |
| Portfolio optimization output shape unknown | Inspect `ProspectPortfolioOptimization` before designing any chart |
| No SEG-Y support anywhere | State as a known limitation in the feature doc and UI |
