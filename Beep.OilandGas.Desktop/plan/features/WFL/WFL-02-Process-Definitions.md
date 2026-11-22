# WFL-02 — Process definitions

| Field | Value |
|---|---|
| Stage | STG-14 — Process engine, workflow and tasks |
| Area | Workflow |
| Status | Gate met - see [evidence](../../evidence/STG-14-WFL-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle` |
| Desktop project | `Modules/Workflow/Definitions/` |
| External packages | none |
| Depends on | WFL-01 |
| Blocks | WFL-03, WFL-06 |
| Web reference | `Pages/PPDM39/BusinessProcess/ProcessDefinitions.razor`, `ProcessDefinitionDetail.razor`, `Components/BusinessProcess/ProcessDefinitionReviewPanel.razor` |

## 1. Purpose

Show exactly what a process does: its steps, transitions, approvers and conditions — so a user can
understand why a document is stuck.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDMProcessService.cs` definition partials | definitions |
| `WorkflowVersioningService.cs` | versions |
| Tables `PROCESS_DEFINITION`, `WORKFLOW_VERSION`, `WORKFLOW_DEPENDENCY`, `ROLE_HANDOFF_CONTRACT` | persistence |

## 3. Screens

`Processes → Definitions`

- List: id, name, category, version, step count, active, last changed.
- Detail: steps in order with the role required, the decision types, and the conditions.
- Graph tab: simple `StepGraph` rendering of nodes and dependency edges.
- Versions tab: version history with what changed.
- "Where is this used" tab: definitions referenced by processes and documents.

## 4. Data and contracts

`OgProcessDefinition { Id, Name, Category, Version, Active, Steps[] }`
`OgProcessStep { Key, Name, RequiresRole, Decisions[], NextStepKeys[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Definition has no steps | shown as invalid with the reason |
| Version superseded | listed and read-only |
| Dependency cycle | highlighted, not hidden |
| Failed read | failed state |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| View | `workflow.read` |
| View versions | `workflow.read` |
| Edit | not in 1.0 (definitions are code/config) |

## 7. Acceptance Criteria

- AC-1 The step list is ordered and each step names its required role.
- AC-2 A cycle in dependencies is displayed.
- AC-3 Versions are shown with their differences.
- AC-4 Definitions are read-only in the desktop (guard: no write call in this feature).
- AC-5 Counts of instances using a definition match WFL-01.
- AC-6 The graph matches the step list exactly.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WFL-02"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-14-WFL-02/observed-evidence.json). Nothing was created, and the viewer
reads the definition through the engine's own contract.

**The steps are not a table.** There is no `PROCESS_DEFINITION_STEP`: a definition's steps live in
`PROCESS_DEFINITION.PROCESS_CONFIG_JSON`, which the owner already models as `ProcessDefinition` /
`ProcessStepDefinition`. The catalogue deserializes that same contract, so the viewer and the engine cannot disagree about what
a step is - and a configuration that does not parse is reported through the failure reporter and shown as the definition's
**invalid reason** instead of disappearing from the list.

| What the page shows | Recorded in |
|---|---|
| Name, category, version, active | `PROCESS_DEFINITION` |
| Ordered steps, roles, decisions, SLAs, transitions | `PROCESS_DEFINITION.PROCESS_CONFIG_JSON` |
| Version history and what each version changed | `WORKFLOW_VERSION` (+ its snapshot) |
| Dependencies between definitions | `WORKFLOW_DEPENDENCY` |
| Instances using the definition | `PROCESS_INSTANCE`, with WFL-01's field scope |

**AC-2 catches both kinds of cycle.** A depth-first walk of the next-step graph reports the path that closes - `S-3 → S-4 →
S-3` - and the same walk over `WORKFLOW_DEPENDENCY` reports `PD-2 → PD-3 → PD-2`. They are reported as separate kinds so a step
loop and a definition loop are never confused, and neither is hidden.

**AC-3 measures the version, not its description.** The added and removed step ids come from consecutive `PROCESS_CONFIG_SNAPSHOT`
values, so a version whose note says "added the approval step" is only believed if a step id is actually new. A version with no
snapshot says so rather than claiming no change.

**AC-4 is structural, not a promise.** The service opens **no unit of work at all**, which is what the guard test asserts; the
page prints the notice that definitions are code and configuration, and that editing is workflow administration rather than a
viewer.

**AC-5 reconciles against the previous feature.** Every definition's instance count, and the catalogue total, are compared with
WFL-01's dashboard for the same field, so the two screens cannot drift.

**AC-6 cannot drift.** The graph's nodes and edges are projected from the step list itself, so an edge cannot exist without a
recorded transition and a step cannot be missing from the graph.

**Three corrections came out of the work.** The read-only guard first flagged `cycles.Add(new ...)` - a list append, not a
database write - and now asserts the write surface instead; the gateway confused the live four-step configuration with the
three-step snapshot it is diffed against; and it assumed the first listed definition was the one it wanted, whereas the list is
ordered by name - the detail is now opened **by identity**, which is the stage's own rule against picking by position or recency.

**Not built, and stated as gaps:** editing definitions (WFL-06); `ROLE_HANDOFF_CONTRACT` (the handoff rules belong to WFL-06);
role, SLA or decision changes on an unchanged step id are not shown as a version difference; `CONDITION_EXPRESSION` is not
evaluated, so a conditional dependency counts as present; the graph is a node/edge list rather than the stage's drawn `StepGraph`;
"where is this used" is instance counts only, because no table links a document to a definition; paging; and per-read permission
enforcement.

## 9. Open questions and risks

- Users will ask to edit definitions. WFL-06 covers administration; a definition editor is out of
  scope and must be stated in the UI.
