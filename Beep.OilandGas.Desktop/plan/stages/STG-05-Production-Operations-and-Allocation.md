# STG-05 — Production operations and allocation

| Field | Value |
|---|---|
| Stage | STG-05 |
| Features | 6 (PRD-01 … PRD-06) |
| Depends on | STG-02, STG-04 |
| Status | Gate met - PRD-01..PRD-06 met ([PRD-01](../evidence/STG-05-PRD-01/observed-evidence.json), [PRD-02](../evidence/STG-05-PRD-02/observed-evidence.json), [PRD-03](../evidence/STG-05-PRD-03/observed-evidence.json), [PRD-04](../evidence/STG-05-PRD-04/observed-evidence.json), [PRD-05](../evidence/STG-05-PRD-05/observed-evidence.json), [PRD-06](../evidence/STG-05-PRD-06/observed-evidence.json)). The exit gate holds: a day of production is allocated to wells and pools (PRD-02, PRD-04), reconciled against sales (PRD-05) and reported (PRD-01, PRD-05), and the intervention that follows from poor performance is diagnosed, decided and turned into a work order (PRD-03, PRD-06). |
| Exit gate | A day of production can be allocated to wells and pools, reconciled, and reported |

---

## 1. Objective

Give the desktop a working daily operations surface: field and pool registers, well status and
deferment, allocation of measured volumes to wells, reconciliation against sales, and the
intervention decisions that follow from poor performance.

---

## 2. Scope

**In**

- Daily operations centre with production, downtime and deferment KPI cards.
- Field and pool register with performance summaries.
- Well status board and deferment logger.
- Allocation workbench: measured → allocated, with method and factor visibility.
- Volume reconciliation against run tickets / sales.
- Production reporting by period, field, pool, well.
- Intervention decisions and the production engineer workspace.

**Out**

- Revenue recognition and royalty (STG-10).
- Forecasting (RES-07).
- SCADA / live telemetry (no such capability; the Web has none either).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.ProductionOperations` | `IProductionOperationsService`, `IProductionOperationsAdvancedService`, `IProductionManagementService`, `ProductionManagementService.PdenFacilityQueries`, `IFacilityManagementService` |
| `Beep.OilandGas.ProductionAccounting` | `IAllocationService`, `AllocationEngine`, `MeasurementService`, `VolumeReconciliation` sources |
| `Beep.OilandGas.LifeCycle` | `Production*Service`, `ProductionProcesses`, `FieldManagementService` |
| `Beep.OilandGas.PPDM.Models` | `PDEN_FLOW_MEASUREMENT`, `PDEN_VOL_SUMMARY`, `PDEN_WELL`, `FACILITY`, `POOL`, `FIELD` |
| `Beep.OilandGas.Client` | `.Production`, `.Operations`, `.Field` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| PRD-01 | Daily operations centre | [PRD-01](../features/PRD/PRD-01-Daily-Operations-Centre.md) |
| PRD-02 | Field and pool register | [PRD-02](../features/PRD/PRD-02-Field-and-Pool-Register.md) |
| PRD-03 | Well status board and deferment log | [PRD-03](../features/PRD/PRD-03-Well-Status-Board-and-Deferment-Log.md) |
| PRD-04 | Allocation workbench | [PRD-04](../features/PRD/PRD-04-Allocation-Workbench.md) |
| PRD-05 | Volume reconciliation and reporting | [PRD-05](../features/PRD/PRD-05-Volume-Reconciliation-and-Reporting.md) |
| PRD-06 | Intervention decisions and engineer workspace | [PRD-06](../features/PRD/PRD-06-Intervention-Decisions-and-Engineer-Workspace.md) |

---

## 5. New code introduced

`.Modules/Production` — six pairs. `.Controls` gains `VolumeStrip` (stacked daily volumes) and
`VarianceCell` (allocated vs measured with tolerance highlighting).

---

## 6. Architecture notes

- **Allocation is never silent.** The workbench shows method, factor source and residual for
  every allocation row. A row with no method is shown as unallocated, not zero.
- **Deferment has a reason code** from the reference table; free text is additive, not a
  substitute.
- **KPI cards refuse to render on a failed read.** Show the failure panel instead — the Web had
  a defect where failed reads became zero counts.
- **Units are explicit.** Volumes carry a unit from `PPDM_UNIT_OF_MEASURE`; conversion only
  through the environment's unit catalog.

---

## 7. Dependencies and sequencing

PRD-01 → PRD-02 → PRD-03. PRD-04 → PRD-05. PRD-06 after PRD-01 and WTA-01 (needs test data).

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-05-1 | Operations centre shows real counts and refuses on failed read | test `PRD-01/T03` |
| SG-05-2 | Deferment requires a reference reason code | test `PRD-03/T02` |
| SG-05-3 | Allocation shows method and residual for every row | test `PRD-04/T02` |
| SG-05-4 | Allocation total equals measured total within the stated tolerance or is flagged | test `PRD-04/T04` |
| SG-05-5 | Reconciliation distinguishes run-ticket, allocated and sold volumes | test `PRD-05/T01` |
| SG-05-6 | Monthly report totals equal the sum of daily rows | test `PRD-05/T04` |

---

## 9. Test plan

`PRD-01/T03`, `PRD-02/T04`, `PRD-03/T02`, `PRD-04/T02`, `PRD-04/T04`, `PRD-05/T01`,
`PRD-05/T04`, `PRD-06/T02`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| `PDEN_VOL_SUMMARY` semantics differ per installation | Read `PdenFacilityQueries` and record the exact grain in PRD-01's data section |
| Allocation engine has 11 partials | Identify the single public entry point before designing the workbench |
| Field overview used a substituted component in Web tests | Do not assume the Web behaviour is verified; design from the service contract |
