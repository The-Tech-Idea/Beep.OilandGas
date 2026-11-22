# STG-04 — Development and drilling

| Field | Value |
|---|---|
| Stage | STG-04 |
| Features | 8 (DEV-01 … DEV-08) |
| Depends on | STG-02, STG-03 |
| Status | Gate met - DEV-01..DEV-08 met. The exit gate holds: a field development plan is drafted, reviewed and approved with a baseline (DEV-02), its wells are designed (DEV-03) and its pools and facilities planned (DEV-04), the drilling program is authored and independently reviewed (DEV-05), executed with daily reports (DEV-06), the well is completed and accepted on evidence (DEV-07), and the facility is tracked to construction and handed to operations (DEV-08). |
| Exit gate | A field development plan can be drafted, reviewed, approved, drilled, completed and handed to construction |

---

## 1. Objective

Port the plan-to-hole sequence: field development plan, well design, pools and facilities, the
drilling program and its independent review, execution with daily reports, completion tests and
acceptance, and construction handover.

---

## 2. Scope

**In**

- Development workspace and FDP wizard with versioned baselines.
- Development plan review, assigned review, and review history.
- Well design evidence per development well.
- Pool and facility planning records.
- Drilling program drafting, submission, and independent review.
- Drilling execution: assignments, daily reports, reporting-responsibility transfer.
- Completion tests, acceptance submission, decision and evidence.
- Construction progress and handover.

**Out**

- Torque & drag, hydraulics, well plan geometry — those are `ENG-06…09` (OilandGasTools).
- Physical drilling data acquisition (no WITSML server).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.DevelopmentPlanning` | `IFieldDevelopmentService`, `IDevelopmentPlanService`, `DevelopmentPlanService.Advanced/.Baseline`, contracts `CreateWellPlan`, `CreateWellServiceJob`, `CreateWellMaintenancePlan`; tables `FIELD_DEVELOPMENT_PLAN`, `DEVELOPMENT_COSTS`, `DEVELOPMENT_WELL_SCHEDULE`, `FACILITY_INVESTMENT` |
| `Beep.OilandGas.DrillingAndConstruction` | `IDrillingOperationService`, `DrillingEngineeringService`, partials `Program*`, `Execution*`, `DailyReport*`, `CompletionTests`; tables `DRILLING_PROGRAM`, `DRILLING_ACTIVITY`, `DRILLING_FLUID`, `DRILLING_BIT`, `CASING_PROGRAM`, `CEMENT_JOB` |
| `Beep.OilandGas.ProductionOperations` | `IFacilityManagementService` for DEV-04 and DEV-08 |
| `Beep.OilandGas.LifeCycle` | `Development*Service`, `Operations*Service`, approval engine |
| `Beep.OilandGas.PermitsAndApplications` | Drilling permit linkage (read-only here; HSE-07 owns it) |
| `Beep.OilandGas.Client` | `.Drilling`, `.Field`, `.LifeCycle` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| DEV-01 | Development workspace | [DEV-01](../features/DEV/DEV-01-Development-Workspace.md) |
| DEV-02 | Field development plan wizard and review | [DEV-02](../features/DEV/DEV-02-Field-Development-Plan-Wizard-and-Review.md) |
| DEV-03 | Development well design | [DEV-03](../features/DEV/DEV-03-Development-Well-Design.md) |
| DEV-04 | Pools and facilities planning | [DEV-04](../features/DEV/DEV-04-Pools-and-Facilities-Planning.md) |
| DEV-05 | Drilling program authoring and review | [DEV-05](../features/DEV/DEV-05-Drilling-Program-Authoring-and-Review.md) |
| DEV-06 | Drilling execution and daily reports | [DEV-06](../features/DEV/DEV-06-Drilling-Execution-and-Daily-Reports.md) |
| DEV-07 | Completion tests and acceptance | [DEV-07](../features/DEV/DEV-07-Completion-Tests-and-Acceptance.md) |
| DEV-08 | Construction progress and handover | [DEV-08](../features/DEV/DEV-08-Construction-Progress-and-Handover.md) |

---

## 5. New code introduced

`.Modules/Development` — eight view-model/view pairs. `.Controls` gains a `WizardHost` (multi-
step with save-and-resume) used by DEV-02, INT-05 and FND-07.

---

## 6. Architecture notes

- **Writes are retained-original (ADR-010).** FDP submission, program submission, daily report
  submission, completion acceptance and construction handover all use the
  `Ready → Sending → Confirmed | Unconfirmed` state.
- **Review separation.** The author of a drilling program cannot be its reviewer. Enforce in the
  view model and record the refusal in the audit log; the service may or may not enforce it.
- **Daily report numbering is server-owned.** The desktop never invents a number.
- **FDP baselines are immutable.** A revision creates a new baseline; editing a submitted plan is
  refused, not silently versioned.

---

## 7. Dependencies and sequencing

DEV-01 → DEV-02 → DEV-03 → DEV-04. DEV-05 → DEV-06 → DEV-07. DEV-08 after DEV-04.
DEV-05 needs EXP-05 approval or an explicit "unapproved" state.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-04-1 | FDP draft saves, submits, and cannot be edited after submission | test `DEV-02/T02`, `T04` |
| SG-04-2 | FDP revision creates a new baseline without mutating the old one | test `DEV-02/T05` |
| SG-04-3 | Well design is scoped to one development well | test `DEV-03/T01` |
| SG-04-4 | Drill program author cannot approve it | test `DEV-05/T04` |
| SG-04-5 | Daily report submission retains the original on lost acknowledgement | test `DEV-06/T03` |
| SG-04-6 | Completion acceptance requires a passed test record | test `DEV-07/T02` |
| SG-04-7 | Handover shows facility evidence and refuses a foreign facility | test `DEV-08/T03` |

---

## 9. Test plan

`DEV-02/T02`, `DEV-02/T04`, `DEV-02/T05`, `DEV-03/T01`, `DEV-05/T04`, `DEV-06/T03`,
`DEV-07/T02`, `DEV-08/T03`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| Five Web pages for completion review with overlapping scope | Consolidate to one workspace with tabs; map old routes in the feature doc |
| `DEVELOPMENT_COSTS` is financial | Use `decimal`, integrate with ECO-01 rather than duplicating cost maths |
| Drilling daily reports are high-volume | Virtualised grid + server paging from day one |
