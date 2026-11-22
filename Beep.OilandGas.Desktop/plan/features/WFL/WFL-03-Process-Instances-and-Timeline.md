# WFL-03 — Process instances and timeline

| Field | Value |
|---|---|
| Stage | STG-14 — Process engine, workflow and tasks |
| Area | Workflow |
| Status | Gate met - see [evidence](../../evidence/STG-14-WFL-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle` |
| Desktop project | `Modules/Workflow/Instances/` |
| External packages | none |
| Depends on | WFL-02 |
| Blocks | WFL-04, WFL-05 |
| Web reference | `Pages/PPDM39/BusinessProcess/ProcessInstances.razor`, `ProcessInstanceDetail.razor`, `Components/Shared/ProcessTimeline.razor`, `Components/Workflow/WorkflowDagVisualizer.razor` |

## 1. Purpose

Follow one document through its process: what happened, when, by whom, and what is expected next.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDMProcessService.cs` instance partials | instances and history |
| `SlaMonitorService.cs` | due dates |
| `Components/Shared/ProcessTimeline.razor` | timeline behaviour |
| Tables `PROCESS_INSTANCE`, `PROCESS_STEP_INSTANCE`, `PROCESS_HISTORY` | persistence |

## 3. Screens

`Processes → Instances`

- Grid: instance id, definition, subject (document/entity), state, current step, assignee, started,
  age, SLA state.
- Detail: header, `Timeline` of steps with outcome and actor, current step panel with actions,
  history table.
- Graph tab: `StepGraph` with the traversed path highlighted.
- Link to the originating business record.

## 4. Data and contracts

`OgProcessInstance { Id, DefinitionId, SubjectKind, SubjectId, State, CurrentStepKey, Assignee,
StartedAt, DueAt?, Age }`
`OgProcessEvent { StepKey, Outcome, Actor, At, Comment }`.

## 5. States

| State | Behaviour |
|---|---|
| Ambiguous instance identity | refused; never pick the latest |
| Due date missing | unscheduled |
| Timestamp without confirmed zone | unconfirmed |
| Subject record missing | shown with a broken-link warning, not hidden |
| Failed read | failed state |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `workflow.read` | no |
| Act on the current step | step role + `workflow.act` | yes |

## 7. Acceptance Criteria

- AC-1 An instance is selected by exact identity; ambiguity is refused.
- AC-2 A missing due date shows unscheduled, not overdue.
- AC-3 A timestamp without a confirmed zone shows unconfirmed.
- AC-4 The timeline is ordered and complete for the instance.
- AC-5 Every event names its actor.
- AC-6 The originating record is reachable and its absence is visible.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WFL-03"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-14-WFL-03/observed-evidence.json). Nothing was created and nothing is
written: the list and the detail read the four process tables.

**AC-1 is the stage's own scar.** The Web had a controller pick **the newest workflow**, so this view refuses ambiguity twice:
an **identity** that matches more than one row, and a **subject** that owns more than one instance. Both refusals name the ids
and say in words that the newest is deliberately not preferred.

| What the page shows | Recorded in |
|---|---|
| Definition, subject, state, started | `PROCESS_INSTANCE` |
| Current step, assignee, SLA, outcome, notes | the open `PROCESS_STEP_INSTANCE` |
| Due date | the open step's start plus its `SLA_HOURS` |
| Timeline and history | the steps merged with `PROCESS_HISTORY` |
| Subject reachability | the data source's own entity list |

**AC-3 exposed an owner gap.** Neither `PROCESS_HISTORY` nor `PROCESS_STEP_INSTANCE` records a **time zone**, and the provider
materialises the values as `Unspecified`. So the rule is applied as the plan states it - only a UTC instant is *confirmed* - and
**no stored timestamp can currently be shown as confirmed**. The page says the value carries no confirmed zone instead of
assuming UTC, and the evidence records it as a limit.

**AC-2 reuses WFL-01's SLA rule**, so the two screens cannot disagree about what "breached" means. The due date is the open
step's start plus its SLA and nothing else, which makes a step with no SLA **unscheduled** rather than overdue.

**AC-6 needed a source of truth.** There is no foreign key from an instance to its subject, so reachability is judged against
`IDataSource.GetEntitesList()`: a missing entity type is a **broken link** naming the type, and an instance with no subject says
so. The absence is visible rather than the row being hidden.

**Two corrections came out of the work.** The first gateway scenario seeded two rows with the same primary key to create an
ambiguity - which the schema forbids, so the ambiguity never existed; it now uses `PI-X` and `pi-x`, which an identity
comparison that ignores case genuinely matches twice. And the scenario first expected the current step to be the last
*completed* step, whereas it is the **open** step with the highest sequence.

**Not built, and stated as gaps:** acting on the current step (WFL-04 owns gate reviews and approvals); the graph tab (WFL-02
draws the definition's graph, and this view shows the traversed path as an ordered timeline); opening the originating record
(the link proves the entity type exists, not that the row does); a confirmed zone, which the owner does not record; paging; and
per-read permission enforcement.

## 9. Open questions and risks

- The Web had a controller select the newest workflow. Preserve the refusal of ambiguity here.
