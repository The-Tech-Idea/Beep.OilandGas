# WFL-05 — Task inbox

| Field | Value |
|---|---|
| Stage | STG-14 — Process engine, workflow and tasks |
| Area | Workflow |
| Status | Gate met - see [evidence](../../evidence/STG-14-WFL-05/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle`, `Beep.OilandGas.UserManagement` |
| Desktop project | `Modules/Workflow/TaskInbox/` |
| External packages | none |
| Depends on | WFL-03 |
| Blocks | HSE-01, DEV-01, PRD-06 (persona landings) |
| Web reference | `Pages/PPDM39/Workflow/TaskInbox.razor`, `Components/Workflow/PersonaTaskInbox.razor`, `PersonaAssignedWorkPanel.razor`, `HseAssignedActions.razor` |

## 1. Purpose

One list of everything assigned to the signed-in person, across all modules, with honest handling of
missing deadlines and unconfirmed time zones.

## 2. Source components

| Source | Reuse |
|---|---|
| Table `CROSS_PERSONA_TASK` | assigned work |
| `PPDMProcessService` task partials | process tasks |
| `SlaMonitorService.cs` | deadlines |
| `Repository/RepositoryAccessService.cs` | persona and role resolution |
| `Components/Workflow/PersonaTaskInbox.razor` | behaviour |

## 3. Screens

`Work` group → **My tasks** (also the default landing for every persona)

- Queue tabs: Today · Overdue · Unscheduled · All · Completed recently.
- Grid: task, module, subject, due (UTC plus company zone), state, source link.
- Detail pane: description, evidence required, actions.
- Bulk: open, mark read.
- Filter by module and by field.

## 4. Data and contracts

`OgTask { Id, Kind, Module, SubjectKind, SubjectId, DueAtUtc?, DueConfirmed, State, SourceRoute }`.
Overdue rule: only a **confirmed** UTC deadline in the past is overdue. A missing deadline is
unscheduled. A timestamp without a confirmed zone is unconfirmed.

## 5. States

| State | Behaviour |
|---|---|
| No deadline | Unscheduled tab |
| Unconfirmed zone | shown with a warning and excluded from Overdue |
| Failed read | failed state |
| Source record missing | shown with a broken-link marker |
| Persona changed | inbox reloads; late responses discarded |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `tasks.read` | no |
| Open source | the source page's gate | no |
| Act | the source page's command permission | yes |

## 7. Acceptance Criteria

- AC-1 A missing deadline appears as unscheduled, never overdue.
- AC-2 A timestamp without a confirmed zone is not counted as overdue.
- AC-3 Counts per tab match the filtered grid.
- AC-4 A source record that cannot be found is visible as a broken link.
- AC-5 A persona change reloads the inbox and discards late responses.
- AC-6 Display shows both UTC and the company zone where a deadline exists.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~TaskInbox"
OG_RUN_SETUP_INTEGRATION=1 dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~TaskInboxGatewayTests"
```

Evidence: [observed-evidence.json](../../evidence/STG-14-WFL-05/observed-evidence.json). The inbox reads
`CROSS_PERSONA_TASK` through `UnitofWork<T>` and merges the LSE-03 lease-expiry projection; it contains no statement text.

## 9. Open questions and risks

- Display clear/update callbacks must carry the selection generation, as the Web learned, so a late
  response cannot render under a newer selection.
