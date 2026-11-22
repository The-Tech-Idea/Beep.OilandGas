# WFL-01 — Process dashboard and analytics

| Field | Value |
|---|---|
| Stage | STG-14 — Process engine, workflow and tasks |
| Area | Workflow |
| Status | Gate met - see [evidence](../../evidence/STG-14-WFL-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.LifeCycle` |
| Desktop project | `Modules/Workflow/Dashboard/` |
| External packages | none |
| Depends on | DAT-04 |
| Blocks | WFL-02 … WFL-06 |
| Web reference | `Pages/PPDM39/BusinessProcess/ProcessDashboard.razor`, `ProcessAnalyticsDashboard.razor`, `ReservesMaturationReport.razor` |

## 1. Purpose

Show whether the organisation's processes are moving: throughput, ageing, SLA breaches and
bottlenecks — from process records, not estimates.

## 2. Source components

| Source | Reuse |
|---|---|
| `LifeCycle/Services/Processes/PPDMProcessService.cs` (+30 partials) | process reads |
| `LifeCycle/Services/Processes/SlaMonitorService.cs` | SLA facts |
| Tables `PROCESS_DEFINITION`, `PROCESS_INSTANCE`, `PROCESS_STEP_INSTANCE`, `PROCESS_HISTORY` | persistence |

## 3. Screens

`Processes → Dashboard`

- KPI cards: running, completed this period, at risk, breached, average cycle time.
- Throughput chart by definition over time.
- Ageing table: instance, definition, current step, days in step, SLA state.
- Bottleneck panel: step with the longest median dwell.
- Definition breakdown.
- Analytics tab: reserves maturation report when the definition is the reserves one.

## 4. Data and contracts

`OgProcessKpis { Running, Completed, AtRisk, Breached, AvgCycleTime }`
`OgStepDwell { Definition, Step, MedianDays, Count }`.

## 5. States

| State | Behaviour |
|---|---|
| No instances in the period | "no activity", distinct from failed |
| SLA not configured | shown as "no SLA", not as met |
| Failed read | failure panel |
| Partial definitions | counts stated per definition, no blended number |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| View | `workflow.read` |
| Open an instance | `workflow.read` |

## 7. Acceptance Criteria

- AC-1 Throughput comes from `PROCESS_HISTORY`, not from status counts.
- AC-2 "No SLA" is distinct from "SLA met".
- AC-3 At-risk and breached follow the configured SLA, including unscheduled handling.
- AC-4 Bottleneck names the step and median dwell.
- AC-5 A failed read is never zero.
- AC-6 Counts per definition reconcile to the instance list.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WFL-01"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-14-WFL-01/observed-evidence.json). Nothing was created and no owner
service was duplicated: the dashboard reads the four process tables through the same unit of work every screen uses.

**The open question, answered.** `PPDMProcessService` is 45 partials exposing 119 public methods. The reads this feature needs
are four of them - `GetProcessDefinitionsByTypeAsync`, `GetProcessInstancesForFieldAsync`, `GetProcessInstanceAsync` and
`GetProcessHistoryAsync` - and the rest are command-side (approve, complete, hand over) and belong to WFL-04 and WFL-06.

| Figure | Recorded in |
|---|---|
| Throughput (instances that moved in the period) | `PROCESS_HISTORY.EVENT_DATE`, joined to the definition through the instance |
| Instance counts, running and completed | `PROCESS_INSTANCE` (`COMPLETED_DATE`) |
| Current step and its dwell | the open `PROCESS_STEP_INSTANCE` with the highest sequence |
| SLA state | `PROCESS_STEP_INSTANCE.SLA_HOURS` and its start |
| Cycle time | `STARTED_DATE` and `COMPLETED_DATE` |
| Median dwell per step | completed `PROCESS_STEP_INSTANCE` rows with both dates |

**AC-1 is a data choice, and the test proves it.** Throughput counts the distinct instances that recorded a `PROCESS_HISTORY`
event inside the period, never a status count. The gateway seeds a definition whose only event falls **outside** the period:
it reports **zero moved** although it has an instance, and the definition beside it reports **three**.

**The honest gap is at-risk.** The owner's `SlaMonitorService` detects one thing only - `elapsed > SLA_HOURS` is a breach and is
escalated - and **no at-risk threshold is configured anywhere in the repository**. So breached reproduces the owner's rule
exactly, and at risk is **derived from the same configured SLA** (a step past half of its own window). The rule is printed on
the page in words, so a derived rule is never presented as a configured one, and the evidence records it as the feature's main
limit.

**AC-4 needed a correction.** A first draft compared the bottleneck by **mean** dwell, which let one 10-day instance name a step
that is normally quick. It is a **median** now - the middle value, or the mean of the two middle ones - with the count shown
beside it, and the unit test asserts a skewed set where the mean (23.2) and the median (3) disagree.

**Nothing is a zero by default.** A step with no `SLA_HOURS` is *"no SLA"* and never *"met"*; a step that has not started is
*"unscheduled"*; the average cycle time is **null**, not zero, when no completed instance records both dates; and a failed read
returns no dashboard at all, with the page clearing every figure.

**Not built, and stated as gaps:** the reserves maturation report the feature lists for the reserves definition (it needs that
definition's own configuration and is outside the six KPIs); opening an instance, its timeline and gate reviews (WFL-03 and
WFL-04); a configured at-risk threshold; a completion-only throughput series (movement and completions are reported separately
and are never added together); a chart (the throughput is a grid, and the shared chart control belongs to RPT); paging; and
per-read permission enforcement beyond the page gate.

## 9. Open questions and risks

- `PPDMProcessService` has 30 partials. Enumerate its public surface first; this feature needs maybe
  six methods.
