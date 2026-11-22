# WFL-04 — Gate reviews and approvals

| Field | Value |
|---|---|
| Stage | STG-14 — Process engine, workflow and tasks |
| Area | Workflow |
| Status | Gate met - see [evidence](../../evidence/STG-14-WFL-04/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle`, `Beep.OilandGas.UserManagement` |
| Desktop project | `Modules/Workflow/Gates/` |
| External packages | none |
| Depends on | WFL-03 |
| Blocks | STG-03 EXP-05, STG-04 DEV-02, STG-09 ECO-03, STG-11 gates |
| Web reference | `Pages/PPDM39/BusinessProcess/GateReviews.razor`, `GateReviewDetail.razor`, `Components/Workflow/ApprovalActionBar.razor`, `Components/BusinessProcess/GateChecklistTable.razor` |

## 1. Purpose

The one approval surface every other stage calls: evidence, decision, rationale, conditions — with
segregation of duties and delegation handled correctly.

## 2. Source components

| Source | Reuse |
|---|---|
| `LifeCycle/Services/Processes/ApprovalWorkflowEngine.cs` | approval |
| `DoAEvaluationService.cs` | delegation of authority |
| `SodEvaluationEngine.cs` | segregation of duties |
| `RoleHandoffContract` table | handoff rules |
| `Repository/FinancialOperationClaimStore.cs` | effective claims |
| Tables `PROCESS_APPROVAL`, `SOD_RULE`, `SOD_CONFLICT`, `DELEGATION_OF_AUTHORITY` | persistence |

## 3. Screens

`Processes → Gates`

- Queue: gate, subject, step, requested by, age, due, my role.
- Detail: subject summary, `GateChecklistTable` of required evidence with satisfied state,
  attachments, prior decisions.
- Decision panel: approve / approve with conditions / return / reject; rationale mandatory except
  approve; conditions editor.
- Delegation banner: acting for whom, under which delegation, validity.
- Post-decision view: recorded outcome, no further change.

## 4. Data and contracts

`OgGateReview { InstanceId, StepKey, SubjectId, Checklist[], PriorDecisions[] }`
`OgDecision { Kind, Rationale, Conditions[], DecidedBy, ActingFor?, DecidedAt }`.

## 5. States

| State | Behaviour |
|---|---|
| Checklist incomplete | approve disabled with the missing items |
| SoD conflict | approve disabled, naming the conflicting role/claim |
| Acting under delegation | both names shown |
| Decision already made | read-only |
| Lost acknowledgement | retained original; no second decision |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Decide | step role + `workflow.approve` | yes |
| Add conditions | same | yes |
| View | `workflow.read` | no |

## 7. Acceptance Criteria

- AC-1 Approve is disabled when the checklist is incomplete or SoD conflicts.
- AC-2 Reject and return require a rationale.
- AC-3 A delegated decision shows delegator and delegate.
- AC-4 A second decision for the same step is refused.
- AC-5 Conditions are recorded with the decision and propagate to the subject.
- AC-6 The author of the subject cannot approve it.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WFL-04"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-14-WFL-04/observed-evidence.json). No table was created: the gate already
exists as an approval row, and the row already carries everything a decision needs to record.

**A gate is not its own table.** It is a `PROCESS_APPROVAL` row, which already holds the requester (`REQUESTED_BY`), the
required action, the decision fields and - importantly - the delegation columns `DELEGATED_FROM`, `DELEGATED_TO` and
`DELEGATED_BY`. So a decision writes the row the queue already reads, and the delegation is recorded rather than invented.

| What the page shows | Recorded in |
|---|---|
| Gate, subject, step, requester, age, due | `PROCESS_APPROVAL` + `PROCESS_STEP_INSTANCE` + `PROCESS_INSTANCE` |
| Checklist (other required approvals) | the same step's other `PROCESS_APPROVAL` rows |
| Blocks: incomplete checklist and segregation | the checklist plus `SOD_CONFLICT` / `SOD_RULE` |
| Delegation banner | `DELEGATED_FROM` / `DELEGATED_TO` on the approval |
| Prior decisions | decided approvals of the same step |
| Conditions | `APPROVAL_NOTES`, the instance's `INSTANCE_DATA_JSON`, and a `GATE_*` history row |

**The checklist cannot include the gate itself.** It is built from the *other* approvals of the same step instance, which is what
makes "approve disabled while the checklist is incomplete" meaningful rather than a permanent deadlock.

**SG-14-3 honours the owner's own distinction.** The owner records `IS_BLOCKING` per `SOD_RULE` and a resolution date per
`SOD_CONFLICT`, so only a blocking, unresolved conflict disables the decision - the gateway flips the flag to `N` and the
decision becomes available again, which proves it is the rule's flag that decides and not an invented severity threshold.

**AC-2, AC-4 and AC-6 are refusals with reasons**, each naming what is wrong: a reject without a rationale, a second decision on a
decided gate (the original is retained), and a decision by the subject's author or by the approval's requester.

**AC-3 keeps both names.** The banner reads "Acting for *delegator* ... *delegate*", the decision writes `APPROVED_BY` as the
delegate and `DELEGATED_BY` as the delegator, and the prior-decision row reads "nadia acting for sara".

**AC-5 needed more than a note.** Conditions are written to `APPROVAL_NOTES` *and* merged into the instance's
`INSTANCE_DATA_JSON` under `gateConditions` (the original payload is preserved), and the decision is appended to
`PROCESS_HISTORY`. A failure to write either is reported as degraded so the approval is never lost.

**Two corrections came out of the run.** The wrong-delegate test used `omar`, who is also the requester, so the refusal came back
as `gate-author-cannot-approve`; the order of the checks is now stated as a rule and the test uses a third user. And the approval
after the blocking rule was re-enabled was *correctly* refused, so the scenario resolves the conflict - with a recorded
compensating control - before approving.

**Not built, and stated as gaps:** delegation-of-authority thresholds (`DELEGATION_OF_AUTHORITY` keys on an amount the instance
does not record); attachments (no document store); a gate-evidence table for non-approval evidence; conflict *detection* at
decision time (`SodEvaluationEngine` is not run, so only recorded conflicts block); the decision and its propagation are two
writes; and per-command permission enforcement.

## 9. Open questions and risks

- This is the shared approval surface for five stages. It must land before EXP-05, DEV-02, ECO-03 and
  the HSE gates are claimed complete.
