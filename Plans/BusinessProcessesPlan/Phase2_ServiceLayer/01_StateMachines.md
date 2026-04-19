# Phase 2 — State Machine Definitions
## All Business Process State Machines

> **Owner**: Backend Dev + Domain SME sign-off  
> **Standard references**: ISO 55001, SPE Stage-Gate, API RP 754, IOGP 2022e, ONRR/NEB compliance frameworks

---

## SM-01: Work Order State Machine

**Applies to**: `ProcessType = "WORK_ORDER"` (all 6 WO sub-types)  
**Standard**: ISO 55001:2018 §8.3–8.4 Asset Management; SAP PM process alignment

### State Transition Diagram

```
                          ┌────────────────────────────────────────────┐
                          │                                            │
  ┌────────────┐  plan   ┌┴──────────┐  start  ┌──────────────┐      │
  │   DRAFT    ├────────►│  PLANNED  ├────────►│ IN_PROGRESS  │      │
  └──────┬─────┘         └─────┬─────┘         └──────┬───────┘      │
         │ cancel               │ cancel                │ hold         │
         │                      │                       ▼             │
         │               ┌──────▼──────┐         ┌─────────────┐     │
         └──────────────►│  CANCELLED  │         │   ON_HOLD   │─────┘
                         └─────────────┘         └──────┬──────┘ resume
                                ▲                       │ cancel
                         cancel │                       ▼
                         ┌──────┘──────────────────────►│
                         │                        ┌─────▼──────┐
                         │              complete  │  COMPLETED │
                         └────────────────────────┤            │
                  IN_PROGRESS ──complete──────────►└────────────┘
```

### Transition Table

| From State | Transition | To State | Guard Condition | Regulatory Reference |
|---|---|---|---|---|
| `DRAFT` | `plan` | `PLANNED` | None | ISO 55001 §8.3 |
| `DRAFT` | `cancel` | `CANCELLED` | Cancellation reason required | ISO 55001 |
| `PLANNED` | `start` | `IN_PROGRESS` | Equipment assigned AND ≥1 BA (contractor) linked via `PROJECT_STEP_BA` | IOGP S-501 §3.2 |
| `PLANNED` | `cancel` | `CANCELLED` | Cancellation reason required | — |
| `IN_PROGRESS` | `hold` | `ON_HOLD` | Hold reason required; writes `PPDM_AUDIT_HISTORY` | ISO 55001 §8.4 |
| `IN_PROGRESS` | `complete` | `COMPLETED` | All `PROJECT_STEP_CONDITION` rows for current step = `SATISFIED` | BSEE SEMS §250.1920 |
| `IN_PROGRESS` | `cancel` | `CANCELLED` | Cancellation reason required | — |
| `ON_HOLD` | `resume` | `IN_PROGRESS` | Resume reason required | — |
| `ON_HOLD` | `cancel` | `CANCELLED` | Cancellation reason required | — |

### Terminal States

`COMPLETED`, `CANCELLED` — no further transitions allowed.

### Guard Implementation Notes

```
Guard: PLANNED → start
  PPDM check: SELECT COUNT(*) FROM PROJECT_STEP_BA WHERE STEP_ID = :stepId > 0
  PPDM check: instance.EntityId (EquipmentId) IS NOT NULL
  Failure: throw ProcessGuardException("start", "Contractor assignment (PROJECT_STEP_BA)", "IOGP S-501 §3.2")

Guard: IN_PROGRESS → complete
  PPDM check: SELECT COUNT(*) FROM PROJECT_STEP_CONDITION
              WHERE STEP_ID = :stepId AND COND_STATUS <> 'SATISFIED' → must = 0
  Failure: throw ProcessGuardException("complete",
           "{N} inspection conditions still open (PROJECT_STEP_CONDITION)", "BSEE SEMS §250.1920")
```

### Sub-type Variations

| WO Sub-type `ProcessType` | Additional Guards on `start` |
|---|---|
| `WO_EMERGENCY` | No additional guards — must be startable immediately |
| `WO_TURNAROUND` | `FACILITY_STATUS = 'SHUTDOWN'` must be confirmed before start |
| `WO_CAPITAL` | `FINANCE` AFE row exists and `FINANCE_STATUS = 'APPROVED'` |
| `WO_PREVENTIVE` | `PROJECT_STEP_TIME.PLANNED_DATE` must be set |
| `WO_CORRECTIVE` | `EQUIPMENT_MAINTAIN` row with `FAILURE_DATE` is required |
| `WO_INSPECTION` | Document of type `PTW` (Permit to Work) attached to `RM_INFORMATION_ITEM` |

---

## SM-02: Gate Review State Machine

**Applies to**: `ProcessType = "GATE_REVIEW"` (Gate 0–5, MOC, AFE)  
**Standard**: SPE Stage-Gate; IPA Front-End Loading (FEL) index; CAPL JOA Article IX

### State Transition Diagram

```
  ┌─────────┐  submit  ┌──────────────┐
  │ PENDING ├─────────►│ UNDER_REVIEW │
  └─────────┘          └──────┬───────┘
       ▲                      │
       │ resubmit    ┌─────────┼──────────┐
       │             │         │          │
       │          approve   reject      defer
       │             │         │          │
       │             ▼         ▼          ▼
       │        ┌─────────┐ ┌────────┐ ┌──────────┐
       └────────┤DEFERRED │ │REJECTED│ │ APPROVED │
                └─────────┘ └────────┘ └──────────┘
```

### Transition Table

| From State | Transition | To State | Guard Condition | Min Approvers | Ref |
|---|---|---|---|---|---|
| `PENDING` | `submit` | `UNDER_REVIEW` | All required documents attached to `RM_INFORMATION_ITEM`; `RM_INFO_DOC_TYPE` matches gate checklist | — | IPA FEL |
| `UNDER_REVIEW` | `approve` | `APPROVED` | `PROJECT_STEP_BA` approver count ≥ `MinApproverCount`; approver has `BA_AUTHORITY.AUTH_TYPE = 'GATE_APPROVER'` | Gate 0–2: 1; Gate 3 (FID): 2; Gate 4–5: 1 | CAPL JOA Art. IX |
| `UNDER_REVIEW` | `reject` | `REJECTED` | Rejection reason text required; writes `NOTIFICATION` to submitter | — | — |
| `UNDER_REVIEW` | `defer` | `DEFERRED` | Deferral reason + `TARGET_DATE` required; `TARGET_DATE` must be > today | — | — |
| `DEFERRED` | `resubmit` | `UNDER_REVIEW` | `TARGET_DATE` has passed OR override reason provided | — | — |
| `REJECTED` | `resubmit` | `UNDER_REVIEW` | Rejection comments addressed (free-text reason required) | — | — |

### Gate-Specific Checklist Requirements

| Gate | `ProcessId` | Required Document Types in `RM_INFORMATION_ITEM` | Reference |
|---|---|---|---|
| Gate 0 — Opportunity ID | `GATE-0-OPP` | `PROSPECT_REPORT`, `RESOURCE_ESTIMATE` | SPE PRMS §2 |
| Gate 1 — Concept Select | `GATE-1-CONCEPT` | `CONCEPT_STUDY`, `COST_ESTIMATE_CLASS5` | IPA FEL-0 |
| Gate 2 — Pre-FEED | `GATE-2-PREFEED` | `PRE_FEED_STUDY`, `COST_ESTIMATE_CLASS4`, `ENVIRONMENTAL_SCREEN` | IPA FEL-1 |
| Gate 3 — FID / FEED | `GATE-3-FID` | `FEED_PACKAGE`, `COST_ESTIMATE_CLASS3`, `EIA_REPORT`, `AFE_FORM` | IPA FEL-2; SEC Rule S-X |
| Gate 4 — Execution Auth | `GATE-4-EXEC` | `EXECUTION_PLAN`, `CONTRACT_AWARD`, `COST_ESTIMATE_CLASS2` | IOGP S-501 |
| Gate 5 — Ops Readiness | `GATE-5-OPS` | `PSSR_CERTIFICATE`, `OPERATING_MANUAL`, `TRAINING_RECORD` | IEC 61511 §5.3; IOGP RP 70 |
| MOC | `GATE-MOC` | `MOC_FORM`, `HAZOP_MOC_REVIEW`, `P&ID_REDLINE` | OSHA PSM 1910.119(l) |
| AFE | `GATE-AFE` | `AFE_FORM`, `PARTNER_APPROVAL_LETTERS` | CAPL JOA Art. IX; COPAS |

### Notifications Triggered

| Transition | Notification Type | Recipients (from `INTEREST_SET` / `INT_SET_PARTNER`) |
|---|---|---|
| `submit` | `GATE_SUBMITTED` | All designated reviewers in `PROJECT_STEP_BA` |
| `approve` | `GATE_APPROVED` | Submitter + all WI partners in `INT_SET_PARTNER` |
| `reject` | `GATE_REJECTED` | Submitter only |
| `defer` | `GATE_DEFERRED` | Submitter only, with `TARGET_DATE` |

---

## SM-03: HSE Incident State Machine

**Applies to**: `ProcessType = "HSE_INCIDENT"`  
**Standard**: API RP 754 (Tier 1–4), IOGP Report No. 2022e, OSHA 300 log requirements

### State Transition Diagram

```
  ┌──────────┐  assign  ┌──────────────────┐
  │ REPORTED ├─────────►│  INVESTIGATING   │
  └──────────┘          └────────┬─────────┘
                                 │ rca_complete
                                 ▼
                      ┌──────────────────────┐
                      │  ROOT_CAUSE_ANALYSIS │
                      └──────────┬───────────┘
                                 │ actions_raised
                                 ▼
                      ┌──────────────────────┐
                      │    ACTIONS_OPEN      │
                      └──────────┬───────────┘
                                 │ all_actions_closed
                                 ▼
                      ┌──────────────────────┐
                      │  PENDING_CLOSE_OUT   │
                      └──────────┬───────────┘
                                 │ close  (role: SafetyOfficer or Manager)
                                 ▼
                      ┌──────────────────────┐
                      │       CLOSED         │
                      └──────────────────────┘
```

### Transition Table

| From State | Transition | To State | Guard Condition | Regulatory Reference |
|---|---|---|---|---|
| `REPORTED` | `assign` | `INVESTIGATING` | Investigator `BA_ID` set in `HSE_INCIDENT_BA`; `INCIDENT_DATE` populated | IOGP 2022e §2 |
| `INVESTIGATING` | `rca_complete` | `ROOT_CAUSE_ANALYSIS` | `HSE_INCIDENT_CAUSE` row exists; immediate cause + root cause text filled | API RP 754 §4; OSHA §1904 |
| `ROOT_CAUSE_ANALYSIS` | `actions_raised` | `ACTIONS_OPEN` | ≥1 corrective action in `PROJECT_STEP` linked to incident; each action has `RESP_BA_ID` | IOGP 2022e §5 |
| `ACTIONS_OPEN` | `all_actions_closed` | `PENDING_CLOSE_OUT` | All linked corrective action `PROJECT_STEP` rows have `STEP_STATUS = 'COMPLETED'` | BSEE SEMS; CSA Z767 |
| `PENDING_CLOSE_OUT` | `close` | `CLOSED` | Role must be `SafetyOfficer` or `Manager`; lessons-learned text required | IOGP 2022e §6 |

### Fast-Track Path (Tier 3 / Tier 4 incidents)

For Tier 3 (near-miss, unsafe condition) and Tier 4 (positive observations) incidents, an
abbreviated path is permitted:

```
REPORTED ──quick_close──► CLOSED
```

**Guard**: `HSE_INCIDENT.INCIDENT_TIER` must be `3` or `4`; close reason required.

### Tier Classification (API RP 754)

| Tier | Description | `INCIDENT_TIER` Value | Required Steps |
|---|---|---|---|
| 1 | Process Safety Event: Loss of primary containment above threshold quantity | `1` | Full RCA + actions + SafetyOfficer close |
| 2 | Process Safety Event: Below threshold quantity | `2` | Full RCA + actions + Manager close |
| 3 | Demand on Safety System (near miss, barrier activation) | `3` | Fast-track or full path |
| 4 | Positive process safety performance indicator | `4` | Fast-track close allowed |

---

## SM-04: Compliance Reporting State Machine

**Applies to**: `ProcessType = "COMPLIANCE_REPORT"`  
**Standard**: ONRR (USA), NEB/CER (Canada), EPA 40 CFR 98, ISO 14001

### State Transition Diagram

```
  ┌────────┐  submit  ┌───────────┐  review_start  ┌──────────────┐
  │ DRAFT  ├─────────►│ SUBMITTED ├───────────────►│ UNDER_REVIEW │
  └────────┘          └───────────┘                └──────┬───────┘
                                                          │
                                         ┌────────────────┼────────────────┐
                                         │                                 │
                                    compliant                        non_compliant
                                         │                                 │
                                         ▼                                 ▼
                                   ┌───────────┐                  ┌────────────────┐
                                   │ COMPLIANT │                  │ NON_COMPLIANT  │
                                   └─────┬─────┘                  └───────┬────────┘
                                         │ close                          │ remediation_start
                                         ▼                                ▼
                                   ┌──────────┐               ┌────────────────────┐
                                   │  CLOSED  │               │    REMEDIATION     │
                                   └──────────┘               └──────────┬─────────┘
                                                                          │ remediation_complete
                                                                          ▼
                                                                    ┌──────────┐
                                                                    │  CLOSED  │
                                                                    └──────────┘
```

### Transition Table

| From State | Transition | To State | Guard Condition | Reference |
|---|---|---|---|---|
| `DRAFT` | `submit` | `SUBMITTED` | `PERIOD_START` and `PERIOD_END` populated; `PRODUCT_TYPE` specified | ONRR; AER ST-39 |
| `SUBMITTED` | `review_start` | `UNDER_REVIEW` | External reference number received (e.g. ONRR confirmation); written to `OBLIGATION.OBLIG_REF_NO` | — |
| `UNDER_REVIEW` | `compliant` | `COMPLIANT` | Regulator status = `ACCEPTED`; update `OBLIGATION.OBLIG_STATUS = 'FULFILLED'` | — |
| `UNDER_REVIEW` | `non_compliant` | `NON_COMPLIANT` | Deficiency notice received; `DEFICIENCY_TEXT` required | ONRR NOD; AER Non-compliance Notice |
| `COMPLIANT` | `close` | `CLOSED` | None | — |
| `NON_COMPLIANT` | `remediation_start` | `REMEDIATION` | Remediation plan text + `TARGET_DATE` required; role: `ComplianceOfficer` or `Manager` | | 
| `REMEDIATION` | `remediation_complete` | `CLOSED` | All `OBLIGATION` remediation rows `OBLIG_STATUS = 'FULFILLED'`; `OBLIG_PAYMENT` rows reconciled | ONRR 30 CFR §1218 |

---

## SM-05: Well Lifecycle State Machine

**Applies to**: `ProcessType = "WELL_LIFECYCLE"`  
**Standard**: API RP 100-1/100-2, ISO 16530-2

### State Transition Diagram

```
 PLANNING ──design_approved──► PERMITTED ──spud──► DRILLING
                                                       │
                                     ┌─────────────────┤
                                     │ td_reached       │ side_track
                                     ▼                  ▼
                                 COMPLETING         DRILLING (loop)
                                     │
                                     │ completion_done
                                     ▼
                                 TIE_IN ──tied_in──► PRODUCING
                                                         │
                               ┌─────────────────────────┤
                               │                         │
                           workover               suspend_well
                               │                         │
                               ▼                         ▼
                           WORKOVER             SUSPENDED ──reactivate──► PRODUCING
                               │                         │
                    wkover_complete               decommission
                               │                         │
                               ▼                         ▼
                          PRODUCING                 P&A ──pa_complete──► ABANDONED
```

---

## SM-06: Facility Lifecycle State Machine

**Applies to**: `ProcessType = "FACILITY_LIFECYCLE"`  
**Standard**: IPA FEL, IEC 61511, ISO 20815, ISO 55001

### State Transitions

| From State | Transition | To State | Key Guard | Reference |
|---|---|---|---|---|
| `CONCEPT` | `feed_start` | `FEED` | FEED contract in `CONTRACT`; execution budget in `FINANCE` | IPA FEL-1 |
| `FEED` | `fid` | `PROCUREMENT` | Gate 3 (FID) `GATE_REVIEW` instance is `APPROVED` | IPA FEL-2 |
| `PROCUREMENT` | `construction_start` | `CONSTRUCTION` | ≥1 `EQUIPMENT` row assigned; `PROJECT_STEP_BA` contractor set | IOGP S-501 |
| `CONSTRUCTION` | `pssr` | `PSSR` | Commissioning docs in `RM_INFORMATION_ITEM`; P&ID markups finalized | IEC 61511 §5.3 |
| `PSSR` | `commission` | `COMMISSIONING` | PSSR certificate signed; `FACILITY_STATUS = 'PSSR_COMPLETE'` | IOGP RP 70 |
| `COMMISSIONING` | `start_ops` | `OPERATING` | `PROD_STRING` first-production date set; `FACILITY_STATUS = 'OPERATIONAL'` | ISO 20815 |
| `OPERATING` | `moc_start` | `MOC` | MOC form attached to `RM_INFORMATION_ITEM` | OSHA 1910.119(l) |
| `MOC` | `moc_complete` | `OPERATING` | MOC Gate Review `GATE-MOC` instance is `APPROVED` | IOGP 423 |
| `OPERATING` | `shutdown` | `SHUTDOWN` | Shutdown authorization in `NOTIFICATION`; `FACILITY_STATUS = 'SHUT_DOWN'` | OSPAR; ISO 13702 |
| `SHUTDOWN` | `decommission_start` | `DECOMMISSIONING` | Decommissioning plan in `RM_INFORMATION_ITEM`; regulatory consent in `CONSENT` | OSPAR Dec. 98/3 |
| `DECOMMISSIONING` | `close` | `ABANDONED` | Regulatory closure consent received; `OBLIGATION` rows fulfilled | IMO Guidelines |

---

## SM-07: Reservoir Management State Machine

**Applies to**: `ProcessType = "RESERVOIR_MGMT"`  
**Standard**: SPE PRMS 2018 §5, NI 51-101 (Canada), SEC 17 CFR §229.1202 (USA)

### State Transitions

| From State | Transition | To State | Key Guard | Reference |
|---|---|---|---|---|
| `ACTIVE` | `annual_review_start` | `UNDER_REVIEW` | Annual review date reached (typically Jan 1 or field anniversary) | SPE PRMS §5.3 |
| `UNDER_REVIEW` | `approve_update` | `ACTIVE` | `POOL_VERSION` row created with new volumes; `PDEN_VOL_SUMMARY` reconciled | NI 51-101 |
| `UNDER_REVIEW` | `reclassify` | `RECLASSIFICATION` | Resource class change prepared (e.g. Contingent → Proved) | PRMS §3 |
| `RECLASSIFICATION` | `reclassify_complete` | `ACTIVE` | New `CLASS_LEVEL` set on `POOL_VERSION`; SEC audit trail preserved | SEC 17 CFR §229.1202 |
| `ACTIVE` | `eor_evaluate` | `EOR_STUDY` | `PDEN_DECLINE_SEGMENT` recovery factor < EOR threshold | SPE PRMS §5.3.7 |
| `EOR_STUDY` | `eor_sanction` | `EOR_ACTIVE` | Gate 3 review `APPROVED`; WI partner consent; `FINANCE` AFE approved | — |
| `EOR_ACTIVE` | `return_to_active` | `ACTIVE` | EOR project `PROJECT_STATUS = 'COMPLETED'` | — |

---

## SM-08: Pipeline Integrity State Machine

**Applies to**: `ProcessType = "PIPELINE_INTEGRITY"`  
**Standard**: ASME B31.8S (USA), CSA Z662-15 (Canada), NACE SP0169, DOT PHMSA

### State Transitions

| From State | Transition | To State | Key Guard | Reference |
|---|---|---|---|---|
| `IN_SERVICE` | `integrity_assessment_start` | `UNDER_ASSESSMENT` | Assessment plan in `RM_INFORMATION_ITEM`; `FACILITY_MAINTAIN` record created | ASME B31.8S §4 |
| `UNDER_ASSESSMENT` | `assessment_complete` | `IN_SERVICE` | All `PROJECT_STEP_CONDITION` rows `SATISFIED`; no anomalies OR all anomalies remediated | ASME B31.8S §6 |
| `IN_SERVICE` | `emergency_shutdown` | `SHUTDOWN` | `HSE_INCIDENT` row created; `FACILITY_STATUS = 'EMERGENCY_SHUTDOWN'` | API RP 1161 |
| `SHUTDOWN` | `safe_restart` | `IN_SERVICE` | Repair WO `COMPLETED`; pressure test certificate in `RM_INFORMATION_ITEM` | ASME B31.8 §841.3 |
| `IN_SERVICE` | `pigging_start` | `UNDER_PIGGING` | Pig type and run sheet attached; `FACILITY_MAINTAIN` record created | NACE SP0169 |
| `UNDER_PIGGING` | `pigging_complete` | `IN_SERVICE` | Pig run report in `RM_INFORMATION_ITEM`; anomalies logged if found | IOGP S-509 |
| `IN_SERVICE` | `tie_in_start` | `TIE_IN` | Regulatory permit in `BA_PERMIT`; tie-in design doc in `RM_INFORMATION_ITEM` | FERC Form 2; NEB OPR-99 |
| `TIE_IN` | `tie_in_complete` | `IN_SERVICE` | Hydrostatic test `SATISFIED`; `FACILITY_VERSION` updated | ASME B31.8 §841.3 |
| `IN_SERVICE` | `abandon` | `ABANDONED` | FERC Order 7157 / NEB OPR-99 §60-61 consent; `OBLIGATION` fulfilled | FERC; NEB OPR-99 |

---

## Guard Implementation Reference

Each guard is a `Func<ProcessInstance, Task<bool>>` stored in `_guardRegistry`.
All guards must:

1. Read from PPDM via `PPDMGenericRepository` using `AppFilter` (no raw SQL)
2. Throw `ProcessGuardException` on failure (never return `false` silently)
3. Include the `RegulatoryReference` string in the exception

Template:

```csharp
_guardRegistry[("WORK_ORDER", "PLANNED", "start")] = async instance =>
{
    // 1. Check equipment is assigned
    if (string.IsNullOrWhiteSpace(instance.EntityId))
        throw new ProcessGuardException(
            transitionName: "start",
            requiredField:  "EntityId (EquipmentId or FacilityId)",
            regulatoryRef:  "ISO 55001:2018 §8.4");

    // 2. Check at least one contractor is assigned
    var baFilters = new List<AppFilter>
    {
        new AppFilter { FieldName = "STEP_ID", Operator = "=", FilterValue = instance.CurrentStepId }
    };
    var assignedBAs = await _baStepRepo.GetAsync(baFilters);
    if (!assignedBAs.Any())
        throw new ProcessGuardException(
            transitionName: "start",
            requiredField:  "Contractor assignment (PROJECT_STEP_BA)",
            regulatoryRef:  "IOGP S-501 §3.2");

    return true;
};
```
