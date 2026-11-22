# STG-14 — Process engine, workflow and tasks

| Field | Value |
|---|---|
| Stage | STG-14 |
| Features | 6 (WFL-01 … WFL-06) |
| Depends on | STG-01, STG-02 |
| Status | Gate met - WFL-01..WFL-06 gate met ([WFL-01](../evidence/STG-14-WFL-01/observed-evidence.json), [WFL-02](../evidence/STG-14-WFL-02/observed-evidence.json), [WFL-03](../evidence/STG-14-WFL-03/observed-evidence.json), [WFL-04](../evidence/STG-14-WFL-04/observed-evidence.json), [WFL-05](../evidence/STG-14-WFL-05/observed-evidence.json), [WFL-06](../evidence/STG-14-WFL-06/observed-evidence.json)); exit-gate items **SG-14-1..SG-14-7 met**. The exit gate holds: a process definition can be inspected, an instance started and followed, a step executed, a gate approved, and the work appears in a task inbox. |
| Exit gate | A process definition can be inspected, an instance started, a step executed, a gate approved, and the work appears in a task inbox |

---

## 1. Objective

The engine every other stage uses for approvals. `Beep.OilandGas.LifeCycle` already implements
process definitions, instances, step instances, history, approvals, delegation of authority,
segregation of duties, SLA monitoring and versioning. The desktop needs to surface all of it.

This stage is deliberately early because STG-03, STG-04, STG-09, STG-11 and STG-12 all need gate
approvals. Building it late forces either duplicate approval code or disabled buttons.

---

## 2. Scope

**In**

- Process dashboard and analytics (throughput, SLA, bottlenecks).
- Definition catalogue with version and step graph.
- Instance list and detail with the step timeline and history.
- Gate reviews: evidence, approve, return for correction, reject.
- Task inbox: assigned work by persona, with overdue and unscheduled handling.
- Workflow administration: versions, dependencies, triggers, SoD rules, delegation.

**Out**

- Editing a definition's logic in the desktop (definitions are code/config in the owning project).
- A visual process designer.

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.LifeCycle` | `PPDMProcessService` (+30 partials), `ApprovalWorkflowEngine`, `DoAEvaluationService`, `SlaMonitorService`, `SodEvaluationEngine`, `WorkflowVersioningService`, `MultiEntityWorkflowOrchestrator`; tables `PROCESS_DEFINITION/INSTANCE/STEP_INSTANCE/HISTORY/APPROVAL`, `WORKFLOW_VERSION/DEPENDENCY`, `ROLE_HANDOFF_CONTRACT`, `BUSINESS_EVENT_TRIGGER`, `CROSS_PERSONA_TASK`, `SOD_RULE/CONFLICT`, `COMPENSATING_CONTROL`, `DELEGATION_OF_AUTHORITY` |
| `Beep.OilandGas.UserManagement` | Persona/role resolution for assignment |
| `Beep.OilandGas.Client` | `.LifeCycle` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| WFL-01 | Process dashboard and analytics | [WFL-01](../features/WFL/WFL-01-Process-Dashboard-and-Analytics.md) |
| WFL-02 | Process definitions | [WFL-02](../features/WFL/WFL-02-Process-Definitions.md) |
| WFL-03 | Process instances and timeline | [WFL-03](../features/WFL/WFL-03-Process-Instances-and-Timeline.md) |
| WFL-04 | Gate reviews and approvals | [WFL-04](../features/WFL/WFL-04-Gate-Reviews-and-Approvals.md) |
| WFL-05 | Task inbox | [WFL-05](../features/WFL/WFL-05-Task-Inbox.md) |
| WFL-06 | Workflow administration | [WFL-06](../features/WFL/WFL-06-Workflow-Administration.md) |

---

## 5. New code introduced

`.Modules/Workflow` — six pairs. `.Controls` gains `Timeline` (already sketched in STG-01) and
`StepGraph` (a simple dependency renderer, not a full designer).

---

## 6. Architecture notes

- **Approval is a three-part act:** evidence, decision, rationale. A rationale is mandatory for
  reject and return.
- **Segregation of duties is checked before enabling approve.** `SodEvaluationEngine` decides;
  the view model obeys.
- **Delegation of authority is explicit.** An approver acting under delegation shows both names.
- **Never pick "the latest" instance.** The Web had a defect where a controller selected the
  newest workflow; the desktop must select by exact identity and refuse ambiguity.
- **Unscheduled vs overdue.** A missing deadline is "unscheduled"; a timestamp without a
  confirmed time zone is "unconfirmed". Only confirmed UTC deadlines are overdue. This exact rule
  is already established in the repository tracker and must be preserved.

---

## 7. Dependencies and sequencing

WFL-01 → WFL-02 → WFL-03 → WFL-04. WFL-05 after WFL-03. WFL-06 after WFL-02.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-14-1 | Definition list shows version and step count | test `WFL-02/T01` |
| SG-14-2 | An ambiguous instance identity is refused, not resolved by recency | test `WFL-03/T03` |
| SG-14-3 | Approve is disabled when SoD detects a conflict | test `WFL-04/T04` |
| SG-14-4 | Reject without a rationale is refused | test `WFL-04/T02` |
| SG-14-5 | A missing deadline shows unscheduled, not overdue | test `WFL-05/T04` |
| SG-14-6 | A timestamp without a confirmed zone shows unconfirmed | test `WFL-05/T05` |
| SG-14-7 | Delegated approval shows delegator and delegate | test `WFL-04/T06` |

---

## 9. Test plan

`WFL-01/T02`, `WFL-02/T01`, `WFL-03/T03`, `WFL-04/T02`, `WFL-04/T04`, `WFL-04/T06`,
`WFL-05/T04`, `WFL-05/T05`, `WFL-06/T02`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| `PPDMProcessService` has 30 partials | Enumerate the public surface first; the desktop needs perhaps 15 methods |
| Approval behaviour is the most-verified area in the repo | Reuse the repository's focused test scenarios as desktop acceptance criteria |
| Definition editing is out of scope but users will ask | WFL-06 shows definitions read-only and links to the owning project's configuration |
