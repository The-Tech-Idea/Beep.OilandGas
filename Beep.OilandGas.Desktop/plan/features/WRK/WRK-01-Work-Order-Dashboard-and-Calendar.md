# WRK-01 — Work order dashboard and calendar

| Field | Value |
|---|---|
| Stage | STG-13 — Work management |
| Area | Work |
| Status | Gate met - see [evidence](../../evidence/STG-13-WRK-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement` |
| Desktop project | `Modules/Work/Dashboard/` |
| External packages | none |
| Depends on | DEV-08, PRD-06 |
| Blocks | WRK-02 |
| Web reference | `Pages/PPDM39/WorkOrder/WorkOrderDashboard.razor`, `WorkOrderCalendar.razor`, `Components/BusinessProcess/WorkOrderQueuePanel.razor` |

## 1. Purpose

See all maintenance and service work in one place, by queue and by date, and pick the next job
deliberately.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement/Services/WorkOrder/WorkOrderService.cs` | work orders |
| `PPDM39.DataManagement/Services/WorkOrder/SchedulingService.cs` | calendar and conflicts |
| Tables `WORK_ORDER`, `WORK_ORDER_*` | persistence |

## 3. Screens

`Work → Work orders`

- Queue panel: mine, unassigned, overdue, awaiting approval, at risk.
- Grid: number, type, asset (well/facility), priority, status, assignee, planned dates, cost.
- Calendar: day/week/month, by resource or by asset, with conflict markers.
- Filter bar: asset, type, priority, status, date range.
- Actions: open, assign, reschedule, export.

## 4. Data and contracts

`OgWorkOrderSummary { Number, Kind, AssetId, Priority, State, Assignee, PlannedStart, PlannedEnd,
Cost, Currency }`.

## 5. States

| State | Behaviour |
|---|---|
| No work | "no work in this view" with a last-refreshed time |
| Failed read | failed state |
| Resource conflict | shown on the calendar with both orders |
| Unassigned and overdue | own queue, not merged with assigned overdue |
| Permission limited | queue hidden with a reason |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View | `work.read` | no |
| Assign | `work.assign` | yes |
| Reschedule | `work.schedule` | yes |

## 7. Acceptance Criteria

- AC-1 Queue counts equal the filtered grid totals.
- AC-2 A resource conflict is displayed explicitly.
- AC-3 Overdue is computed from planned end.
- AC-4 Calendar and grid agree on the selected work.
- AC-5 A failed read is not an empty queue.
- AC-6 Unassigned and assigned work are distinguishable.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WRK-01"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-13-WRK-01/observed-evidence.json). Nothing was created: the dashboard
reads the owner's `WORK_ORDER`, `WORK_ORDER_COMPONENT`, `R_WO_TYPE` and `PROJECT_STATUS`, and writes nothing.

**The header is not the state.** The owner's own `WorkOrderService.TransitionStateAsync` refuses a header transition with *"A
native work-order header does not establish operational state. Use the controlled execution, verification and return workflow
with its original approved evidence."* The table agrees: `WORK_ORDER` has a number, a type, the request/due/complete dates, the
rush flag and instructions - and **no status, priority, assignee or cost**. The state is therefore the latest
`PROJECT_STATUS` of the project the component links through `PROJECT_ID`, and an order with no such row reads "no recorded
state" instead of being shown as a draft.

| What the page shows | Recorded in |
|---|---|
| Number, type (with its long name), request and due dates, rush | `WORK_ORDER` + `R_WO_TYPE` |
| Field, well, facility, equipment, assigned person, linked project | `WORK_ORDER_COMPONENT` |
| State | latest `PROJECT_STATUS` of the linked project |
| Cost | not recorded - WRK-02 rolls it up from labour and material lines |

**The legacy mapper was not repeated.** The same service's `MapDynamicToSummary` reads `WO_SUBTYPE`, `PROJECT_STATUS`,
`EQUIPMENT_ID`, `PLAN_START_DATE` and `PLAN_END_DATE`, none of which are columns on the table, so those reads return empty
values. The desktop reads what exists and derives the rest.

**AC-1 holds by construction.** The queues are predicates over the same list the grid binds, and `In(key)` is the function the
count is computed from - so a queue cannot disagree with the work it lists. The gateway asserts every queue.

**AC-2** is an overlap of the planned spans of one assignee, naming both orders, the person and the interval; a completed order
is not a resource demand and an unassigned one has no resource. **AC-3** computes overdue at read time and exempts completed
and cancelled work. **AC-4** needed a correction the unit test caught: the calendar total first summed the per-day lists, so a
three-day order counted three times and the total could not be compared with the grid - it now counts distinct orders, with the
day view kept as it is.

**Not built, and stated as gaps:** a graded priority (only the rush marker exists); assignment recorded on the execution
workflow rather than the component; an "at risk" queue (no risk indicator or published threshold, so inventing one was
refused); a detailed schedule beyond request-to-due; cost (WRK-02); the stage's shared `CalendarBoard` control (the calendar is
a text list of days); assign/reschedule commands (WRK-02); and per-queue permission gating.

## 9. Open questions and risks

- The Web has 60+ work-order component panels. This feature covers the queue and calendar only;
  the rest are mapped in WRK-02.
