# WFL-06 — Workflow administration

| Field | Value |
|---|---|
| Stage | STG-14 — Process engine, workflow and tasks |
| Area | Workflow |
| Status | Gate met - see [evidence](../../evidence/STG-14-WFL-06/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle`, `Beep.OilandGas.UserManagement` |
| Desktop project | `Modules/Workflow/Admin/` |
| External packages | none |
| Depends on | WFL-02 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Admin/WorkflowAdminWorkspace.razor` (`/lifecycle/workflow/admin`) |

## 1. Purpose

Let an administrator see and maintain the workflow configuration that drives every approval: versions,
dependencies, triggers, SoD rules, delegation and compensating controls.

## 2. Source components

| Source | Reuse |
|---|---|
| `WorkflowVersioningService.cs` | versions |
| `SodEvaluationEngine.cs`, `SodConflictDetector.cs` | SoD rules |
| `DoAEvaluationService.cs` | delegation |
| `MultiEntityWorkflowOrchestrator.cs` | orchestration config |
| Tables `WORKFLOW_VERSION`, `WORKFLOW_DEPENDENCY`, `BUSINESS_EVENT_TRIGGER`, `SOD_RULE`, `DELEGATION_OF_AUTHORITY`, `COMPENSATING_CONTROL` | persistence |

## 3. Screens

`Processes → Administration`

Tabs: Versions · Dependencies · Triggers · SoD rules · Delegations · Compensating controls ·
Diagnostics.

- Versions: activate, retire, compare.
- Dependencies: which definitions call which; cycle detection.
- Triggers: business event → process; enable/disable with reason.
- SoD rules: role pairs that cannot combine on a decision; test a combination.
- Delegations: who acts for whom, window, authority limit.
- Diagnostics: orphaned instances, instances without a definition, broken handoffs.

## 4. Data and contracts

`OgSodRule { Id, RoleA, RoleB, Scope, Active }`
`OgDelegation { From, To, FromDate, ToDate, Limit, Currency, Active }`.

## 5. States

| State | Behaviour |
|---|---|
| Cycle in dependencies | refused, cycle shown |
| Delegation outside its window | inactive and marked |
| Orphaned instance | listed; repair is explicit |
| Failed read | failed state |
| Change without a reason | refused |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Change configuration | `workflow.admin` | yes |
| Test a SoD combination | `workflow.admin` | no |
| View | `workflow.read` | no |

## 7. Acceptance Criteria

- AC-1 A dependency cycle is refused before saving.
- AC-2 Every configuration change records actor, reason and previous value.
- AC-3 A delegation outside its window is inactive.
- AC-4 The SoD tester returns a pass/fail with the conflicting rule named.
- AC-5 Orphaned instances are listed with their cause where known.
- AC-6 Only administrators reach this page.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WFL-06"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-14-WFL-06/observed-evidence.json). Two additions were needed for the
feature to be true rather than asserted: a change record, and somewhere for a person delegation to live.

**AC-2 needed a table.** None of the owner's configuration tables keeps a change history - `WORKFLOW_VERSION` describes its own
creation, but flipping `SOD_RULE.IS_BLOCKING` or adding a dependency left no actor, reason or previous value anywhere. So
`WORKFLOW_CONFIGURATION_CHANGE` was added in the owning module and registered: area, target, actor, reason, previous value, new
value and timestamp. A change is refused without a reason **and** without the confirmation that names the affected definitions,
which is the feature's own open question made concrete.

**AC-3 exposed a modelling gap.** `DELEGATION_OF_AUTHORITY` is an authority ladder - entity type, threshold, approval level,
required role - with **no delegator, delegate or window**. Five additive columns carry the person delegation
(`DELEGATOR_USER_ID`, `DELEGATE_USER_ID`, `VALID_FROM_UTC`, `VALID_TO_UTC`, `IS_ACTIVE`), and the threshold columns record that
the row *is* a person delegation rather than being left blank.

| Area | Written to | Refused when |
|---|---|---|
| Version activation | `PROCESS_DEFINITION.VERSION` + `PROCESS_CONFIG_JSON` from the snapshot | the version has no snapshot |
| Dependency | `WORKFLOW_DEPENDENCY` | it would close a cycle, or depend on itself |
| Trigger | `BUSINESS_EVENT_TRIGGER.IS_ACTIVE` | it is already in that state |
| Segregation rule | `SOD_RULE.IS_BLOCKING` | it is already in that state |
| Delegation | `DELEGATION_OF_AUTHORITY` + its person columns | missing name, self-delegation, no window, or a negative limit |

**AC-1 reuses the owner's own graph walk** rather than a second one: the same dependency-cycle detection WFL-02 uses decides
whether the new edge closes a loop, and the refusal prints the path.

**AC-4 keeps the distinction that matters.** A blocking match fails and names the rule; an advisory match is reported as
*advisory rather than blocking* instead of passing silently. The gateway makes the rule advisory and re-tests in the same run, so
the tester and the write cannot disagree.

**AC-5 names what is broken and why:** an instance whose definition does not exist, an instance with no step instance, and a
handoff naming a step the instance does not record.

**Three corrections came out of the work.** The first cycle scenario added `PD-3 → PD-1` against a recorded `PD-1 → PD-2`, which
is a *chain, not a loop* - the validator rightly accepted it and the test was wrong; it now closes the recorded pair. A no-op
change would have written an empty change record claiming work that did not happen, so it is refused. And the catalogue entry was
first added with **Everyone**, which the unit test caught: the page is gated on `SystemAdmin` and `TenantAdmin`.

**Not built, and stated as gaps:** editing a definition's steps (WFL-02 states they are code and configuration); a per-version
retirement flag (the definition's `IS_ACTIVE` is used instead); enforcement of a delegation's authority limit (WFL-04 does not
evaluate the ladder); creating or reviewing a compensating control; a repair command for the findings (the feature says repair is
explicit, and it is not implemented); checking a handoff for a definition with no live instance; role-pair testing (the owner
stores conflicting *permissions*); a transactional change + audit pair; paging; and per-command permission enforcement.

## 9. Open questions and risks

- Configuration writes affect every approval in the product. Require an explicit confirmation naming
  the affected definitions before saving.
