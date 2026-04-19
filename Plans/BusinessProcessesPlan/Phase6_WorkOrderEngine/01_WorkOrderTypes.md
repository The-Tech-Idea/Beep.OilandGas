# Phase 6 — Work Order Types
## 6 Sub-Types with Process Definitions, Jurisdiction Variations, and Required Fields

---

## Sub-Type Catalogue

| WO Type | ProcessId | Description | BSEE / SEMS Ref | Canada AER Ref | International |
|---|---|---|---|---|---|
| Preventive Maintenance | `WO-PREVENTIVE` | Scheduled PM per equipment manufacturer intervals | SEMS §250.1914 | AER Directive 038 | IOGP S-501 |
| Corrective Maintenance | `WO-CORRECTIVE` | Unplanned repair of failed equipment | SEMS §250.1915 | AER Directive 038 | IOGP S-501 |
| Safety Critical | `WO-SAFETY` | Maintenance of safety critical elements (SCE) | SEMS §250.1917, API RP 14C | AER Directive 017 | ISO 13702 |
| Environmental | `WO-ENVIRONMENTAL` | Spill response, remediation, containment | EPA 40 CFR 254 | AER Directive 055 | MARPOL |
| Regulatory Inspection | `WO-REGULATORY` | Government-mandated inspection | BSEE 30 CFR 250 | AER IR schedules | NOGEPA |
| Turnaround | `WO-TURNAROUND` | Planned facility shutdown for overhaul | SEMS §250.1920–.1932 | AER Directive 071 | IOGP S-624 |

---

## WO-PREVENTIVE: Preventive Maintenance

**States**: DRAFT → PLANNED → IN_PROGRESS → COMPLETED  
**Required fields before transition to IN_PROGRESS**:
- `PROJECT_STEP_BA`: At least one contractor assigned
- `PROJECT_PLAN.PLAN_START_DATE`: Must be set
- `FINANCE.BUDGET_AMT`: AFE budget > 0 if WO cost > $50,000

**Jurisdiction differences**:

| Field | USA | Canada | International |
|---|---|---|---|
| `PROJECT.PPDM_GUID` tag | Required for SEMS records | — | — |
| Inspection frequency | Per API RP 14C equipment tables | Per AER Directive 038 Table 1 | Per OEM manual |
| Min qualification (`BA_LICENSE.LICENSE_TYPE`) | `BSEE_CERT` | `AER_CONTRACTOR_CERT` | `ISO_9001` |

---

## WO-SAFETY: Safety Critical Element Maintenance

**States**: DRAFT → PLANNED → IN_PROGRESS → UNDER_REVIEW → COMPLETED  
**UNDER_REVIEW added** because SCE WOs require HSE officer sign-off before close-out.

**Required fields before transition to COMPLETED**:
- `PROJECT_STEP_CONDITION` rows: All inspection items with result = `PASS`
- `PROJECT_STEP_BA`: HSE officer (`BA_TYPE = 'HSE_OFFICER'`) in closing approval step

**Guard code** (in `ProcessStateMachine.RegisterSafetyCriticalStateMachine`):
```csharp
machine.Configure(UNDER_REVIEW)
    .Permit(COMPLETE_TRIGGER, COMPLETED)
    .OnEntry(() => ValidateHSESignOff(instanceId))
    .Guard(() => HasHSEOfficerApproval(instanceId),
        "HSE officer approval required before completing SCE work [SEMS §250.1917]");
```

---

## WO-TURNAROUND: Facility Turnaround

**States**: DRAFT → SCOPED → PLANNED → IN_PROGRESS (SHUTDOWN) → IN_PROGRESS (MAINTENANCE) → IN_PROGRESS (RESTART) → COMPLETED  
(Uses sub-phase labels in `PROJECT_STEP.STEP_NAME`; state machine remains WORK_ORDER type with extended step count)

**Key requirements**:
- `FINANCE.BUDGET_AMT`: Mandatory; typical range $500K–$10M
- `PROJECT_STEP` rows: minimum 5 steps (Shutdown, P&ID update, Maintenance, Inspection, Restart)
- `PROJECT_STEP_CONDITION` rows: Pre-shutdown safety checks (minimum 12 items from SEMS §250.1920)
- All process lines `INSPECTION_DATE` updated in `PDEN_EQUIP_MAINT` before RESTART step

**Jurisdiction differences**:

| Requirement | USA | Canada | International |
|---|---|---|---|
| Regulatory notification | BSEE OCS-G form 0143 (30 days prior) | AER DIR 071 §3.2 (20 days prior) | OSPAR notification if offshore |
| SCE verification method | SEMS Table B-3 | AER SCE self-assessment | ISO 13702 §9.5 |

---

## Common Attributes Across All WO Types

All work orders share these PPDM column requirements:

| Column | Table | When Required |
|---|---|---|
| `FIELD_ID` | `PROJECT` | Always |
| `ACTIVE_IND = 'Y'` | `PROJECT` | Always |
| `PROJECT_TYPE = 'WORK_ORDER'` | `PROJECT` | Always; set by service |
| `WO_SUBTYPE` | `PROJECT` | Set from WO type ProcessId |
| `CREATE_USER` | `PROJECT` | Set by `CommonColumnHandler` |
| `ROW_CHANGED_DATE` | `PROJECT` | Auto-updated by `CommonColumnHandler` |
