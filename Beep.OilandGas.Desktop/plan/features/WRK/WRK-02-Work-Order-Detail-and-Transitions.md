# WRK-02 — Work order detail and transitions

| Field | Value |
|---|---|
| Stage | STG-13 — Work management |
| Area | Work |
| Status | Gate met - see [evidence](../../evidence/STG-13-WRK-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement`, `Beep.OilandGas.Accounting` |
| Desktop project | `Modules/Work/WorkOrder/` |
| External packages | none |
| Depends on | WRK-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/WorkOrder/WorkOrderDetail.razor`, `Components/BusinessProcess/WorkOrder*.razor` (labour, material, cost, completion, approval panels) |

## 1. Purpose

Execute a work order: record labour and materials, track cost, submit for approval, complete — with
only the transitions the service allows.

## 2. Source components

| Source | Reuse |
|---|---|
| `WorkOrderService.cs` | transitions |
| `PPDM39.DataManagement` work order labour/material records | lines |
| `Accounting/Services/InventoryService.cs` | material reservation |
| `Components/BusinessProcess/WorkOrderLaborUsagePanel.razor`, `…LaborCostPanel`, `…MaterialUsagePanel`, `…MaterialAllocationPanel`, `…MaterialCostPanel`, `…CompletionReviewPanel`, `…CompletionDecisionPanel`, `…RecordedApprovalPanel` | behaviour |

## 3. Screens

`Work → Work order`

Tabs: Summary · Scope · Materials · Labour · Cost · Approvals · Execution · Completion · History.

- Summary: number, asset, type, priority, state, dates, assignee.
- Materials: item, requested, reserved, issued, returned, cost.
- Labour: person, hours, rate, cost.
- Cost: rolled-up total with currency and variance to estimate.
- Approvals: recorded approvals with actor and date.
- Execution: start with permit check (WRK-04), progress notes, completion submission.

## 4. Data and contracts

`OgWorkOrder { Number, AssetId, Kind, State, Estimate, Actual, Currency, Materials[], Labour[] }`.

## 5. States

| State | Behaviour |
|---|---|
| Material above availability | refused with the short quantity |
| Execution without permit | refused |
| Completion without evidence | refused |
| Transition unavailable | not offered |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Add labour/material | `work.execute` | yes |
| Submit for approval | `work.submit` | yes |
| Approve | `work.approve` + SoD | yes |
| Complete | `work.complete` | yes |

## 7. Acceptance Criteria

- AC-1 Only service-reported transitions are offered.
- AC-2 Actual cost equals the sum of labour and material lines.
- AC-3 Execution start is refused without a valid permit.
- AC-4 Completion requires evidence.
- AC-5 Author cannot approve their own completion.
- AC-6 Every line records its actor and date.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~WRK-02"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-13-WRK-02/observed-evidence.json). The lines are the owner's own tables;
only the labour rate needed an additive column.

**The owner had the lines already.** `WORK_ORDER_LABOR_USAGE` records who worked, on what duty, from and to when, the quantity
and unit and the executor; `WORK_ORDER_MATERIAL_USAGE` records the item, the allocation it came from, the quantity and the usage
date; `WORK_ORDER_MATERIAL_ALLOCATION` is the reservation. Material cost needs no new column at all: `INVENTORY_ITEM` already
records `UNIT_COST` and `CURRENCY_CODE`.

**Labour was the exception.** Its report is deliberately cost-free - the owner's own remark says *"no wage, valuation, currency or
journal posting is inferred"* - and the class was not partial. It was made partial and two **additive** columns, `UNIT_COST` and
`CURRENCY_CODE`, carry the rate a line is costed at. Nothing recorded changed, and a report without a rate stays readable and is
counted as **uncosted** rather than costed at zero.

| What the page shows | Recorded in |
|---|---|
| Labour: person, duty, interval, quantity, executor, rate | `WORK_ORDER_LABOR_USAGE` (+ the added rate columns) |
| Materials: item, quantity, unit cost, currency, used-at | `WORK_ORDER_MATERIAL_USAGE` + `INVENTORY_ITEM` |
| Reservations | `WORK_ORDER_MATERIAL_ALLOCATION` against `INVENTORY_ITEM.QUANTITY_ON_HAND` |
| Permit gate | `WORK_ORDER_EXECUTION_PERMIT` status and validity window |
| Completion evidence | a `WORK_ORDER_CONDITION` of type `CLOSE_OUT` with an inspector, a date and a remark |
| State and its recorder | the linked project's latest `PROJECT_STATUS` row |

**AC-1 is the owner's graph.** `WorkOrderDetailRules.Transitions` calls `WorkOrderWorkflow.Available` for the recorded state and
subtype; the picker offers exactly that and the command refuses anything else. The chosen state is appended to the linked
project's status history - the store the owner's workflow uses - rather than written to the header, which the owner says does
not establish operational state.

**AC-2, AC-3, AC-4 and AC-5 are gates with reasons.** The cost is the lines' own sum per currency and says "partial" when a line
is uncosted; the start is refused without a permit inside its window and not in a refusing state; the completion is refused
without a recorded close-out; and the person who recorded the current state cannot move the order to review or completion.
**SG-13-3** refuses a reservation above what is on hand less what is already allocated, naming the short quantity.

**A finding shaped the evidence rule.** `WORK_ORDER_CONDITION` records the condition's type, associate, remark and date but has
no status or result column, so the owner's `InspectionCondition` cannot be materialised from it. A recorded `CLOSE_OUT` with an
inspector, a date and a remark therefore *is* the evidence - stated as the rule rather than inferred from a missing status.

**A validation catch.** The owner marks `PROCESS_INSTANCE_ID`, `START_ID`, `ASSIGNMENT_ID` and `LOCATION_ID` as required, and this
build has no process instance to record, so the writes store explicit placeholders (`NO-PROCESS`, `NO-START`,
`NO-ASSIGNMENT`, and the warehouse or `UNSPECIFIED`). That is recorded as a limit, not presented as a workflow link.

**Not built, and stated as gaps:** the owner's controlled execution workflow commands (start, verification, return with their own
evidence); an evidence file store; an approval record beyond the recorded state; a line with no currency (excluded from every
total); the permit's positive status vocabulary (the rule names what refuses); the Web's nine tabs (scope, approvals, execution
notes and history are deferred, and the Web paths are in the feature table); and per-command permission enforcement beyond the
submitter/approver rule.

## 9. Open questions and risks

- 60+ Web panels exist. Implement the nine tabs above and record the remaining panels as deferred
  with their Web paths.
