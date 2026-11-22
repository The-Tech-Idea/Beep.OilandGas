# EXP-05 — Well program approval

| Field | Value |
|---|---|
| Stage | STG-03 — Exploration and seismic |
| Area | Exploration |
| Status | Gate met - AC-1..AC-6: decision records actor/date/rationale/conditions, incomplete prospect risk blocks the gate, the author cannot decide, a second decision is refused, conditions are stored on the approved program for DEV-05, and an unacknowledged decision is retained and retried unchanged. New LifeCycle-owned WELL_PROGRAM / WELL_PROGRAM_DECISION tables; the generic engine (WFL-04) is not wired and the page says so. See [evidence](../../evidence/STG-03-EXP-05/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle` |
| Desktop project | `Modules/Exploration/WellProgram/` |
| External packages | none |
| Depends on | EXP-03, WFL-04 |
| Blocks | DEV-05 |
| Web reference | `Pages/PPDM39/Exploration/WellProgramApproval.razor` |

## 1. Purpose

The gate between "we want to drill this prospect" and "we may spend money drilling it". Records
the program, the evidence, the decision and the conditions.

## 2. Source components

| Source | Reuse |
|---|---|
| `LifeCycle/Services/Exploration/ExplorationProcessService.cs` and `Processes/Exploration*` | program workflow |
| `LifeCycle/Services/Processes/ApprovalWorkflowEngine.cs` | approval |
| `ProspectIdentification` | prospect linkage |
| Tables `PROCESS_*`, `PROSPECT`, `EXPLORATION_PROGRAM`, `EXPLORATION_BUDGET` | persistence |

## 3. Screens

`Exploration → Well program approval`

- Queue of programs awaiting decision, with age and owner.
- Detail: prospect summary (read-only), program objectives, target, depth, cost, risks, evidence
  attachments, conditions.
- Decision panel: approve / approve with conditions / return / reject, rationale (mandatory except
  approve), conditions list.
- History: every decision with actor, date, rationale, conditions and resulting state.

## 4. Data and contracts

`OgWellProgram { Id, ProspectId, Name, Objective, TargetDepth, DepthUnit, EstimatedCost, Currency,
State, Conditions[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Prospect risk incomplete | approval is refused with the missing elements named |
| Decision without rationale | refused for anything but approve |
| Concurrent decision | second decision refused; the winner is named |
| Approval service unavailable | decision panel disabled with the reason |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `exploration.read` | no |
| Approve / return / reject | `exploration.approve` + SoD pass | yes |

## 7. Acceptance criteria

- AC-1 Approval records actor, date, rationale and conditions.
- AC-2 Approval is refused while the prospect risk is incomplete.
- AC-3 Author cannot approve their own program.
- AC-4 A second concurrent decision is refused.
- AC-5 Conditions appear on the resulting drill program (DEV-05) as constraints.
- AC-6 A lost acknowledgement retains the decision original.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~EXP-05"
```

## 9. Open questions and risks

- Approval depends on STG-14. Until it lands, this screen can *record* a decision but must label it
  "recorded, not workflow-approved".
