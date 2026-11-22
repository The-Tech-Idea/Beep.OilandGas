# HSE-03 — Investigation, RCA and corrective action

| Field | Value |
|---|---|
| Stage | STG-11 — HSE, compliance and permits |
| Area | HSE |
| Status | Gate met - see [evidence](../../evidence/STG-11-HSE-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement`, `Beep.OilandGas.HSE` |
| Desktop project | `Modules/Hse/Investigation/` |
| External packages | none |
| Depends on | HSE-02 |
| Blocks | WRK-01 |
| Web reference | `Pages/PPDM39/HSE/InvestigatorAssignment.razor`, `CorrectivePlanWorkspace.razor`, `Pages/PPDM39/BusinessProcess/HSEIncidentDetail.razor`, `Components/Workflow/HseCauseReview.razor`, `HseCorrectiveAction.razor`, `HseClosureWorkflow.razor`, `HseAssignedActions.razor` |

## 1. Purpose

Assign an investigator, establish cause, and drive corrective actions to verified closure — with
the original decision retained if the network fails.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement/Services/HSE/RCAService.cs` | root cause analysis |
| `PPDM39.DataManagement/Services/HSE/CorrectiveActionService.cs` | actions and closure |
| `Beep.OilandGas.HSE/Models/InvestigatorAssignmentReviewValidator.cs`, `CompanyHseRcaHandoffReview.cs`, `CompanyHseCorrection.cs` | command contracts |
| `Components/Workflow/HseAssignedActions.razor` | assigned actions presentation |

## 3. Screens

`HSE → Investigation`

- Assignment panel: candidate investigators from the recorded roster with role observations, select
  one, submit; outcome review before another change.
- RCA panel: timeline, contributing factors, root cause, evidence.
- Corrective action panel: plan name/number, actions with owner and due date, verification of
  effectiveness.
- Closure workflow: checklist, closing decision, reason.
- Retained-original banner when an acknowledgement is unknown.

## 4. Data and contracts

`OgInvestigatorAssignment { IncidentId, ParticipantId, Role, Rationale, State }`
`OgCorrectiveAction { PlanId, Action, Owner, DueDate, State, VerifiedBy, VerifiedAt }`.

## 5. States

| State | Behaviour |
|---|---|
| Assignment unconfirmed | the original is retained; a second assignment is blocked |
| Field changed after review | the original is not cleared; the recorded investigator must match |
| Roster changed | does not clear a pending original |
| Action overdue | red chip; still editable by its owner |
| Closure without verified actions | refused |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Assign investigator | `hse.investigation.assign` | yes |
| Record RCA | `hse.investigation.write` | yes |
| Manage actions | `hse.action.write` | yes |
| Close | `hse.incident.close` + SoD | yes |

## 7. Acceptance criteria

- AC-1 A lost acknowledgement retains the original and blocks a second assignment.
- AC-2 The original cannot be cleared by a different context.
- AC-3 A roster change does not clear a pending original.
- AC-4 All actions must be verified before closure.
- AC-5 The closer cannot be the investigator.
- AC-6 Every action records owner and due date.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~HSE-03"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-11-HSE-03/observed-evidence.json). Nothing new was created: the owner
already holds the whole investigation in `HSE_INCIDENT_CAUSE` and the `PROJECT` family, and the investigator is an incident
component with the INVESTIGATOR role. `HSE_INCIDENT_CAUSE` and `PROJECT_STEP_BA` were added to `HseModule` (smoke 425 -> 427).

**The assignment is retained, and the roster is not consulted.** The investigator is the component row whose
`COMPONENT_ROLE` is `INVESTIGATOR`; its `COMPONENT_TYPE` is the role and its `REMARK` the rationale. Once recorded:

- the **same participant** is a retry that adopts the original;
- a **different participant** is refused with the recorded name and *"the original is retained"* (AC-1);
- a **different field context** is refused before anything is written (AC-2);
- a **roster change** cannot clear or replace it, because the roster is never read for this decision (AC-3).

**Verification needed a vocabulary the owner does not have.** The owner's corrective-action statuses are `OPEN` and
`COMPLETED`; there is no verified state. Verification of effectiveness is therefore a second `PROJECT_STEP_BA` row with a
`VERIFIER` role and its effective date, stated once in `InvestigationRules`. An action is **closed out** only when completed
**and** verified, the owner cannot verify their own action, and closure names **every** outstanding action rather than the
first.

**The root-cause gate is the owner's own rule.** The owner's `RCAService.IsRCACompleteAsync` requires a `ROOT` cause with a
description of at least 20 characters; the desktop applies exactly that rather than inventing a different threshold.

**One tension, resolved explicitly.** HSE-02 refused state changes because the workflow engine is not wired, yet HSE-03's
criteria require a closure. Closure is therefore **the one terminal, rule-guarded transition this build performs** - root
cause, every action verified, a closer who is not the investigator - and it appends a `CLOSED` status response, exactly as
the owner's own API does in `PPDMCompanyCorrectiveActionCommands.Closure`. Every other transition remains refused by HSE-02.

**One link serves two screens.** The plan is linked to the incident through the component's `PROJECT_ID`, which HSE-02 already
counts open actions from, so the incident's open-action count and this page's actions cannot drift apart.

**A cleanup along the way.** The earlier HSE-02 wiring had left duplicate English and Arabic entries in OgLoc's dictionaries;
the indexer initializer allowed them so the build stayed green. They are removed and each key appears once.

**Completion is the status row.** The first version also set the step's ACTUAL_END_DATE, but an update through the unit of work needs the table's key in the provider structure; without it the write was silently skipped while the command still reported success. The COMPLETED PROJECT_STATUS row is the authoritative record, so the redundant write is gone (found while building HSE-04).

**Not built, and stated as gaps:** reassignment (blocked rather than offered, because there is no workflow review step);
a signed verification record (verification is a role and a timestamp); RCA evidence as files (text only - there is no
document store); a cross-source chronological timeline (the timeline is the cause sequence); a single transaction across the
plan, step, owner, status and verification writes; and per-command permission enforcement for `hse.incident.close`.

## 9. Open questions and risks

- This is the most heavily verified area in the repository. Port its scenarios as desktop tests
  rather than inventing new ones.
