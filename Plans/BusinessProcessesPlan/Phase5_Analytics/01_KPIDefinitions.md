# Phase 5 — KPI Definitions
## All Key Performance Indicators with Formulas and PPDM Sources

---

## Category 1: Work Order KPIs

| KPI ID | Name | Formula | PPDM Source |
|---|---|---|---|
| WO-01 | WO Completion Rate | `WOs completed / WOs started × 100` | `PROJECT` (WO ProcessType); filter `ACTIVE_IND='Y'` |
| WO-02 | On-Time Completion Rate | `WOs completed before due date / WOs with due date × 100` | `PROJECT.EARLIEST_START_DATE` vs `PROJECT_STEP_TIME.END_DATE` |
| WO-03 | Mean Time to Complete (MTTC) | Average duration from DRAFT→COMPLETED state | `PPDM_AUDIT_HISTORY`: `MIN(AUDIT_DATE)` where TRIGGER=draft to `MAX(AUDIT_DATE)` where TRIGGER=complete |
| WO-04 | WO Overdue Count | WOs in IN_PROGRESS or PLANNED past scheduled end date | `PROJECT`, join `PROJECT_PLAN`. Count where `PLAN_END_DATE < GETDATE()` and state ≠ COMPLETED |
| WO-05 | Contractor Utilization | WOs with contractor assigned / total WOs × 100 | `PROJECT_STEP_BA` row count join on `PROJECT` |

---

## Category 2: Gate Review KPIs

| KPI ID | Name | Formula | PPDM Source |
|---|---|---|---|
| GR-01 | Gate Cycle Time | Average days from INITIATED → APPROVED per gate type | `PPDM_AUDIT_HISTORY` delta on GATE_REVIEW process type |
| GR-02 | Gate Pass Rate | Gate reviews reaching APPROVED / total initiated × 100 | `PROJECT` filtered on ProcessType=GATE_REVIEW; group by category |
| GR-03 | Average Approver Count | Average `PROJECT_STEP_BA` rows per gate review | `PROJECT_STEP_BA` join `PROJECT` |

---

## Category 3: HSE KPIs (IOGP 2022e aligned)

| KPI ID | Name | Formula | IOGP Reference | PPDM Source |
|---|---|---|---|---|
| HSE-01 | Tier 1 Process Safety Event Rate | (Tier 1 PSE count / exposure hours) × 1,000,000 | API RP 754 Tier 1 | `HSE_INCIDENT` where `INCIDENT_TIER=1` |
| HSE-02 | Tier 2 Process Safety Event Rate | Same formula, Tier 2 | API RP 754 Tier 2 | `HSE_INCIDENT` where `INCIDENT_TIER=2` |
| HSE-03 | Total Recordable Injury Rate (TRIR) | (Recordable injuries × 200,000) / exposure hours | OSHA 300 | `HSE_INCIDENT` + `HSE_INCIDENT_BA` |
| HSE-04 | Near Miss Reporting Rate | Near misses reported / total incidents × 100 | IOGP 2022e §3 | `HSE_INCIDENT` where `INCIDENT_TYPE='NEAR_MISS'` |
| HSE-05 | Mean Time to Close Corrective Actions | Average days from `CA_ASSIGNED` → `CA_CLOSED` per RCA | `PROJECT_STEP` corrective actions join `HSE_INCIDENT` |

---

## Category 4: Compliance KPIs

| KPI ID | Name | Formula | PPDM Source |
|---|---|---|---|
| COMP-01 | Obligation On-Time Fulfillment Rate | Obligations met by due date / total due × 100 | `OBLIGATION` where `OBLIG_STATUS='FULFILLED'` and `FULFILL_DATE <= DUE_DATE` |
| COMP-02 | Overdue Obligations Count | Count where `DUE_DATE < GETDATE()` and `OBLIG_STATUS != 'FULFILLED'` | `OBLIGATION` |
| COMP-03 | Royalty Payment Timeliness | Royalty payments within payment window (state-specific) | `OBLIG_PAYMENT` join `OBLIGATION` where `OBLIG_TYPE LIKE 'ROYALTY%'` |

---

## Category 5: Production KPIs

| KPI ID | Name | Formula | PPDM Source |
|---|---|---|---|
| PROD-01 | Monthly Gross Production (BOE) | Sum of oil + gas (converted to BOE) per month | `PDEN_VOL_SUMMARY.PROD_BOE` grouped by `PROD_PERIOD_DATE` |
| PROD-02 | Production Decline Rate | % change in BOE from prior month | Rolling calculation on `PDEN_VOL_SUMMARY` |
| PROD-03 | Uptime Rate per Well | Hours producing / total hours × 100 | `PROD_STRING_WELL` downtime intervals |

---

## Date-Range Filter Convention

All KPI calculations accept `DateRangeFilter`:

```csharp
public record DateRangeFilter(DateTime From, DateTime To)
{
    public static DateRangeFilter Last30Days  => new(DateTime.UtcNow.AddDays(-30),  DateTime.UtcNow);
    public static DateRangeFilter Last90Days  => new(DateTime.UtcNow.AddDays(-90),  DateTime.UtcNow);
    public static DateRangeFilter Last365Days => new(DateTime.UtcNow.AddDays(-365), DateTime.UtcNow);
}
```
