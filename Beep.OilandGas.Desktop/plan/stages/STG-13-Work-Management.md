# STG-13 — Work management

| Field | Value |
|---|---|
| Stage | STG-13 |
| Features | 4 (WRK-01 … WRK-04) |
| Depends on | STG-04, STG-11 |
| Status | Gate met - WRK-01, WRK-02, WRK-03 and WRK-04 gate met ([WRK-01](../evidence/STG-13-WRK-01/observed-evidence.json), [WRK-02](../evidence/STG-13-WRK-02/observed-evidence.json), [WRK-03](../evidence/STG-13-WRK-03/observed-evidence.json), [WRK-04](../evidence/STG-13-WRK-04/observed-evidence.json)); exit-gate items **SG-13-1..SG-13-5 met**. The exit gate holds: a work order is raised, scheduled, executed with a permit, inspected and closed with its cost. |
| Exit gate | A work order can be raised, scheduled, executed with a permit, inspected, and closed with cost |

---

## 1. Objective

The maintenance and intervention execution layer: work orders, scheduling calendar, maintenance
plans, inspections, contractors, and the permit-to-work gate that must precede field execution.

---

## 2. Scope

**In**

- Work order dashboard with queue, priority and status.
- Work order detail with the full transition set, labour, materials and cost.
- Scheduling calendar (day/week/month) with resource conflicts.
- Maintenance plans and inspection schedules.
- Contractor register, competency and allocation.
- Permit-to-work issuance and execution readiness checks.

**Out**

- Time-sheet capture beyond work-order labour rows.
- Mobile field execution (separate product).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.PPDM39.DataManagement` | `WorkOrderService`, `InspectionService`, `ContractorManagementService`, `SchedulingService`, `WorkOrderExecutionPermitService` |
| `Beep.OilandGas.ProductionOperations` | `FacilityManagementService` maintenance lifecycle partials, `WELL_MAINTENANCE_PLAN`, `WELL_SERVICE_JOB` |
| `Beep.OilandGas.DevelopmentPlanning` | `CreateWellMaintenancePlan`, `CreateWellServiceJob` contracts |
| `Beep.OilandGas.Accounting` | `InventoryService` for material allocation, `FixedAssetLifecycleService` |
| `Beep.OilandGas.Client` | `.Operations`, `.Field` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| WRK-01 | Work order dashboard and calendar | [WRK-01](../features/WRK/WRK-01-Work-Order-Dashboard-and-Calendar.md) |
| WRK-02 | Work order detail and transitions | [WRK-02](../features/WRK/WRK-02-Work-Order-Detail-and-Transitions.md) |
| WRK-03 | Maintenance plans and inspections | [WRK-03](../features/WRK/WRK-03-Maintenance-Plans-and-Inspections.md) |
| WRK-04 | Contractors and permit-to-work | [WRK-04](../features/WRK/WRK-04-Contractors-and-Permit-to-Work.md) |

---

## 5. New code introduced

`.Modules/Work` — four pairs. `.Controls` gains `CalendarBoard` (shared with WRK-01 and
`LSE-03` reminders) and `ChecklistGate` (permit readiness).

---

## 6. Architecture notes

- **Transition legality comes from the service.** The UI offers only transitions the service
  reports as available; it does not encode the state machine.
- **Permit readiness is a gate, not a warning.** Execution start is disabled until every
  readiness item is satisfied.
- **Material allocation checks stock** before allowing a reservation; a shortfall is shown with
  the short quantity.
- **Cost rolls up from labour and materials**, never typed by the user.

---

## 7. Dependencies and sequencing

WRK-01 → WRK-02. WRK-03 and WRK-04 independent after WRK-01.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-13-1 | Only service-reported transitions are offered | test `WRK-02/T02` |
| SG-13-2 | Execution start is refused without a valid permit | test `WRK-04/T03` |
| SG-13-3 | Material reservation refuses more than available stock | test `WRK-02/T05` |
| SG-13-4 | Work order cost equals the sum of labour and material lines | test `WRK-02/T06` |
| SG-13-5 | Calendar shows a resource conflict explicitly | test `WRK-01/T04` |

---

## 9. Test plan

`WRK-01/T04`, `WRK-02/T02`, `WRK-02/T05`, `WRK-02/T06`, `WRK-04/T03`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| Web has 60+ work-order component panels | Implement the core loop first; record the remaining panels as deferred inside WRK-02 |
| Permit readiness items are not enumerated anywhere | Read `WorkOrderExecutionPermitService` and write the list into WRK-04 §4 |
