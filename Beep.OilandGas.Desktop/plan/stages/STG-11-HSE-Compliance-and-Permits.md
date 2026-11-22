# STG-11 — HSE, compliance and permits

| Field | Value |
|---|---|
| Stage | STG-11 |
| Features | 7 (HSE-01 … HSE-07) |
| Depends on | STG-02, STG-14 |
| Status | Gate met - HSE-01..HSE-07 gate met ([HSE-01](../evidence/STG-11-HSE-01/observed-evidence.json), [HSE-02](../evidence/STG-11-HSE-02/observed-evidence.json), [HSE-03](../evidence/STG-11-HSE-03/observed-evidence.json), [HSE-04](../evidence/STG-11-HSE-04/observed-evidence.json), [HSE-05](../evidence/STG-11-HSE-05/observed-evidence.json), [HSE-06](../evidence/STG-11-HSE-06/observed-evidence.json), [HSE-07](../evidence/STG-11-HSE-07/observed-evidence.json)); exit-gate items **SG-11-1..SG-11-8 met**. The exit gate holds: an incident is reported, investigated and closed with evidence; an obligation is discharged; a permit is applied for and decided; and a GHG report is produced from verified factors or refused with its reasons. |
| Exit gate | An incident is reported, investigated and closed with evidence; an obligation is discharged; a permit is applied for and decided; a GHG report is produced from verified factors |

---

## 1. Objective

Safety and regulatory work. This stage carries the highest audit and legal exposure in the
product and the strongest existing verification history in the repository — the
`MASTER-TODO-TRACKER.md` records dozens of focused HSE gates. The desktop must not weaken any of
them.

---

## 2. Scope

**In**

- HSE workspace: assigned actions, KPIs from recorded facts, source links.
- Incident reporting and management with company/field revalidation.
- Investigation, RCA handoff, corrective action plan, closure.
- HAZOP studies: nodes, deviations, actions.
- Compliance calendar, obligations, report process.
- GHG: factor catalogs, jurisdiction selection, review state, report.
- Permits: inbox, new application, detail, decision, compliance check.

**Out**

- Regulator delivery (no integration exists — the Web explicitly states this).
- Legal interpretation of deadlines.

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.LifeCycle` | `PPDMHSEService`, `HSE` process services, `ApprovalWorkflowEngine` |
| `Beep.OilandGas.PPDM39.DataManagement` | `HSEIncidentService`, `RCAService`, `CorrectiveActionService`, `HAZOPService`, `HSEKPIService`, `BarrierManagementService`, `ComplianceService`, `GHGReportingService`, `RoyaltyCalculationService` |
| `Beep.OilandGas.HSE` | `ICompanyHseWorkflowCommands`, `CompanyHseAssignedActions`, `CompanyHseCorrection`, `CompanyHseRcaHandoffReview`, `GhgArithmeticCheck`, `GhgFactorCatalog`, `InvestigatorAssignmentReviewValidator` |
| `Beep.OilandGas.PermitsAndApplications` | `IPermitApplicationLifecycleService`, `IPermitApplicationWorkflowService`, `IPermitComplianceCheckService`, `IPermitComplianceReportService`, `IPermitStatusHistoryService`, `PermitStatusTransitionRules`, `Forms/`, `DataMapping/` |
| `Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf` | Permit PDF rendering |
| `Beep.OilandGas.Models` | `IHSEService` |
| `Beep.OilandGas.Client` | `.Permits`, `.LifeCycle`, `.AccessControl` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| HSE-01 | HSE workspace | [HSE-01](../features/HSE/HSE-01-HSE-Workspace.md) |
| HSE-02 | Incident management | [HSE-02](../features/HSE/HSE-02-Incident-Management.md) |
| HSE-03 | Investigation, RCA and corrective action | [HSE-03](../features/HSE/HSE-03-Investigation-RCA-and-Corrective-Action.md) |
| HSE-04 | HAZOP studies | [HSE-04](../features/HSE/HSE-04-HAZOP-Studies.md) |
| HSE-05 | Compliance calendar and obligations | [HSE-05](../features/HSE/HSE-05-Compliance-Calendar-and-Obligations.md) |
| HSE-06 | GHG factors and reporting | [HSE-06](../features/HSE/HSE-06-GHG-Factors-and-Reporting.md) |
| HSE-07 | Permit applications | [HSE-07](../features/HSE/HSE-07-Permit-Applications.md) |

---

## 5. New code introduced

`.Modules/Hse` — seven pairs. Reuses `Timeline` (obligations), `ApprovalBar` (decisions),
`OgFormBuilder` (incident and permit forms).

---

## 6. Architecture notes

- **No invented jurisdiction.** The Web removed a default-to-USA bug; the desktop must require an
  explicit jurisdiction and show "not specified" when absent.
- **No invented deadlines or legal status.** Obligations show the *recorded* due date and state.
  Interpreted deadlines are out of scope.
- **GHG refuses to compute from unverified factors.** The repository already refuses when factors
  or activity data are unverified; the screen shows the reason and the required action.
- **Reads are FIELD-scoped and revalidated before and after.** Company/field change during a load
  discards the response.
- **Permit decisions are ordered.** `PermitStatusTransitionRules` defines legal transitions; the UI
  must only offer legal ones.

---

## 7. Dependencies and sequencing

HSE-01 → HSE-02 → HSE-03. HSE-04, HSE-05, HSE-06, HSE-07 independent after HSE-01.
Needs STG-14 for gate/approval steps; until then the approval command is disabled with a reason.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-11-1 | Incident report requires type, tier, explicit jurisdiction and ISO timestamp with offset | test `HSE-02/T02` |
| SG-11-2 | Missing jurisdiction is shown as unspecified, never defaulted | test `HSE-02/T03` |
| SG-11-3 | A field change during a load discards the late response | test `HSE-02/T05` |
| SG-11-4 | Investigator assignment retains the original on a lost acknowledgement | test `HSE-03/T04` |
| SG-11-5 | HAZOP action cannot be raised without a node and a deviation | test `HSE-04/T02` |
| SG-11-6 | Obligation shows recorded due date and state, never an inferred one | test `HSE-05/T02` |
| SG-11-7 | GHG report refuses when the factor catalog revision does not match the company review | test `HSE-06/T04` |
| SG-11-8 | Only legal permit transitions are offered | test `HSE-07/T03` |

---

## 9. Test plan

`HSE-01/T02`, `HSE-02/T02`, `HSE-02/T03`, `HSE-02/T05`, `HSE-03/T04`, `HSE-04/T02`,
`HSE-05/T02`, `HSE-06/T04`, `HSE-07/T03`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| The repository's HSE behaviour is heavily verified; a desktop port could regress it | Treat the existing focused tests as the specification; re-run their equivalents against the desktop view models |
| Permit PDF path uses `Wkhtmltopdf` | Confirm the native binary can be shipped in the desktop installer (REL-01) |
| Compliance/GHG are FIELD-scoped | Same context rule as STG-02; reuse `WellContext` |
