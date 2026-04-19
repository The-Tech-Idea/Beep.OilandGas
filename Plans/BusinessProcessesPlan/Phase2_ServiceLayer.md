# Phase 2 — Service Layer Enhancements
## Business Process Branch — Detailed Implementation Plan

> **Status**: 🔲 Not Started  
> **Depends on**: Phase 1 (Complete ✅)  
> **Blocks**: Phase 3 (API Layer)  
> **Owner**: Backend / Data Layer Team  
> **Estimated Effort**: 8–10 sprints (2-week sprints)  
> **Standards References**:
>
> | Standard | Body | Relevance |
> |---|---|---|
> | SPE-PRMS 2018 | SPE/WPC/AAPG/SPEE | Resource & reserves classification for all process definitions |
> | API RP 100-1 / 100-2 | API | Well design, drilling, P&A state machine guards |
> | API RP 754 | API | Tier 1–4 incident classification for HSE state machine |
> | BSEE 30 CFR 250 | BSEE | Federal offshore permit/obligation workflow triggers |
> | EPA 40 CFR 98 Subpart W | EPA | GHG event triggers in compliance workflow |
> | AER Directive 056 / 023 | AER | Alberta well permit & production reporting triggers |
> | NEB/CER OPR-99 | NEB/CER | Canadian federal pipeline obligation workflows |
> | ISO 55001:2018 | ISO TC 251 | Work order asset management state machine design |
> | ISO 14224:2016 | ISO | Equipment failure/maintenance data taxonomy (WO) |
> | IEC 61882:2016 | IEC | HAZOP record structure for HSE process steps |
> | IOGP 2022e | IOGP | HSE KPI statistical protocol; incident closure criteria |
> | IOGP S-501 | IOGP | Contractor management in work order steps |
> | ISO 31000:2018 | ISO | Risk assessment step structure for HSE/Gate processes |

---

---

## 2.1 — Objective

Extend `PPDMProcessService` and `ProcessDefinitionInitializer` so that **all 12
business process categories** have fully seeded `ProcessDefinition` objects with
correct state machines, steps, approvals, and PPDM39 table bindings.

Currently only Exploration, Development, Production, and Decommissioning are
partially wired. The following 8 categories are **missing**:

| Category | Process Count | Priority |
|---|---|---|
| Work Order Workflows | 6 | HIGH |
| Approval & Gate Reviews | 8 | HIGH |
| HSE & Safety Workflows | 8 | HIGH |
| Compliance & Regulatory | 8 | HIGH |
| Well Lifecycle Workflows | 8 | MEDIUM |
| Facility Lifecycle Workflows | 8 | MEDIUM |
| Reservoir Management Workflows | 8 | MEDIUM |
| Pipeline & Infrastructure | 8 | MEDIUM |

---

## 2.2 — PPDM39 Table Mapping Per Category

### Category 1: Exploration Workflows

| Process Step | Primary PPDM Tables | Secondary / Lookup Tables | Notes |
|---|---|---|---|
| Lead Creation | `POOL`, `AREA`, `FIELD` | `AREA_TYPE`, `POOL_TYPE`, `POOL_STATUS` | SPE PRMS §2 — Lead = unrisked resource |
| G&G Data Capture | `RM_INFORMATION_ITEM`, `SEIS_3D`, `SEIS_SURVEY` | `SEIS_SURVEY_TYPE`, `SEIS_DATA_TYPE` | Seismic records per PPDM Category 39 |
| Prospect Registration | `POOL`, `POOL_ALIAS`, `POOL_VERSION` | `CLASS_LEVEL`, `CLASS_SYSTEM` | PRMS Prospect classification |
| Risk Assessment | `PPRM_ASSESSMENT`, `ANL_ANALYSIS_REPORT` | `TEST_TYPE`, `INTEREST_TYPE` | SPEE risking guidelines |
| Prospect Approval | `INTEREST_SET`, `INT_SET_PARTNER` | `INT_SET_STATUS`, `BA_ROLE_TYPE` | Working interest partner notification |
| Exploration License | `BA_LICENSE`, `LAND_RIGHT`, `CONSENT` | `LICENSE_TYPE`, `LICENSE_STATUS` | USA: BLM/OCS lease; Canada: AER/C-NLOPB |
| Exploration Well Plan | `WELL`, `WELL_VERSION`, `WELL_PURPOSE` | `WELL_TYPE`, `WELL_STATUS` | API RP 96 (well design) |
| Spud Authorization | `WELL`, `OBLIGATION`, `NOTIFICATION` | `NOTIF_BA`, `OBLIG_TYPE` | BSEE APD (USA); AER Directive 56 (CA) |

### Category 2: Development Workflows

| Process Step | Primary PPDM Tables | Secondary / Lookup Tables | Notes |
|---|---|---|---|
| Pool Definition | `POOL`, `POOL_VERSION`, `POOL_AREA` | `POOL_TYPE`, `POOL_STATUS`, `STRAT_TYPE` | NRCan SEC Rule S-X definition; CER rules |
| FDP Preparation | `PROJECT`, `PROJECT_PLAN`, `PROJECT_STEP` | `PROJECT_TYPE`, `PROJECT_STATUS` | ISO 17779 FDP requirements |
| Facility Design FEED | `FACILITY`, `FACILITY_VERSION` | `FACILITY_TYPE`, `FACILITY_STATUS` | IEC 61511 (SIL); ISO 13702 (fire) |
| Well Engineering | `WELL`, `WELL_VERSION`, `CASING_PROGRAM` | `WELL_TYPE`, `WELL_STATUS` | API RP 100-1/2; ISO 16530 |
| Construction Management | `PROJECT_STEP`, `PROJECT_STEP_BA`, `EQUIPMENT` | `EQUIPMENT_TYPE`, `EQUIPMENT_STATUS` | IOGP S-501 construction standards |
| Pipeline Engineering | `PIPE_STRING`, `FACILITY` | `FACILITY_TYPE`, `SUBSTANCE_TYPE` | ASME B31.8 (USA); CSA Z662 (Canada) |
| Interest Set Registration | `INTEREST_SET`, `INT_SET_PARTNER` | `INT_SET_STATUS` | JOA/CAPL operating procedure |
| AFE Approval | `FINANCE`, `FIN_COMPONENT` | `FINANCE_TYPE` | SEC 10-K capital expenditure disclosure |

### Category 3: Production Workflows

| Process Step | Primary PPDM Tables | Secondary / Lookup Tables | Notes |
|---|---|---|---|
| Well Start-up | `PROD_STRING`, `PROD_STR_STAT_HIST` | `STRING_STATUS`, `STRING_TYPE` | API RP 19D; BSEE SEMS BOEM |
| Production Allocation | `PDEN_VOL_SUMMARY`, `PDEN_ALLOC_FACTOR` | `PDEN_VOL_REGIME`, `VOLUME_REGIME` | EIA Form 914; NEB/CER reporting |
| Workover Authorization | `WELL`, `WELL_VERSION`, `OBLIGATION` | `WELL_STATUS`, `WORK_ORDER_TYPE` | API RP 100-1; AER Directive 36 |
| Decline Management | `PDEN_DECLINE_SEGMENT`, `PDEN_DECLINE_CAGE` | `PDEN_DECLINE_CONDITION` | SEC proved reserves (DCA basis) |
| Emergency Shutdown | `HSE_INCIDENT`, `FACILITY_STATUS` | `HSE_INCIDENT_TYPE`, `HSE_INCIDENT_SEVERITY` | OSHA PSM 29 CFR 1910.119; IOGP 456 |
| Production Reporting | `PDEN_VOL_SUMMARY`, `PDEN_VOL_DISPOSITION` | `PRODUCT_TYPE`, `VOLUME_REGIME` | USA: OGOR EIA; Canada: AER ST-39 |

### Category 4: Decommissioning Workflows

| Process Step | Primary PPDM Tables | Secondary / Lookup Tables | Notes |
|---|---|---|---|
| P&A Planning | `WELL`, `WELL_VERSION`, `OBLIGATION` | `WELL_TYPE`, `WELL_STATUS` | API RP 100-2; BSEE NTL 2010-G05 |
| P&A Execution | `WELL`, `BA_PERMIT`, `NOTIFICATION` | `NOTIF_BA`, `OBLIG_TYPE` | Norway: NORSOK D-010; UK: OGA guidelines |
| Facility Decommission | `FACILITY_STATUS`, `FACILITY_MAINTAIN` | `FACILITY_TYPE`, `FACILITY_STATUS` | OSPAR Decision 98/3; IMO guidelines |
| Environmental Closure | `HSE_INCIDENT`, `CONSENT`, `OBLIGATION` | `RESTRICTION_TYPE`, `OBLIGATION_TYPE` | EPA RCRA; NEB CER DOGGR |
| Cost Accounting | `FINANCE`, `FIN_COST_SUMMARY` | `FINANCE_TYPE`, `OBLIGATION_STATUS` | FASB ASC 410-20 ARO disclosure |
| Regulatory Notification | `NOTIFICATION`, `NOTIF_BA`, `OBLIGATION` | `NOTIFICATION_TYPE`, `OBLIG_TYPE` | BSEE Form 124; AER Form 6 |

### Category 5: Work Order Workflows

| Process Step | Primary PPDM Tables | Secondary / Lookup Tables | Notes |
|---|---|---|---|
| WO Creation | `PROJECT`, `PROJECT_PLAN`, `PROJECT_STEP` | `PROJECT_TYPE`, `WORK_ORDER_TYPE` | ISO 55001 Asset Management |
| Equipment Assignment | `EQUIPMENT`, `EQUIPMENT_USE_STAT` | `EQUIPMENT_TYPE`, `EQUIPMENT_STATUS` | ISO 14224 reliability data |
| BA Service Assignment | `PROJECT_STEP_BA`, `BUSINESS_ASSOCIATE` | `BA_ROLE_TYPE`, `SERVICE_TYPE` | IOGP S-501 contractor management |
| WO Execution | `FACILITY_MAINTAIN`, `EQUIPMENT_MAINTAIN` | `FACILITY_MAINT_STATUS`, `EQUIPMENT_MAINT_TYPE` | SAP PM alignment; ISO 55001 |
| Inspection Sign-off | `PROJECT_STEP_CONDITION`, `RM_INFORMATION_ITEM` | `TEST_TYPE`, `TEST_STATUS` | BSEE SEMS §250.1920 |
| Cost Capture | `FINANCE`, `FIN_COMPONENT` | `FINANCE_TYPE` | COPAS PGAS accounting; IOGP Report 461 |

### Category 6: Approval & Gate Reviews

| Gate | Primary PPDM Tables | Secondary / Lookup Tables | Decision Standard |
|---|---|---|---|
| Gate 0 — Opportunity ID | `POOL`, `AREA`, `FIELD` | `POOL_TYPE`, `AREA_TYPE` | SPE PRMS possible resource class |
| Gate 1 — Concept Select | `PROJECT`, `PROJECT_PLAN` | `PROJECT_TYPE`, `PROJECT_STATUS` | SPE PRMS contingent resource |
| Gate 2 — Pre-FEED | `FACILITY`, `INTEREST_SET` | `FACILITY_TYPE`, `INT_SET_STATUS` | ISO 15663 lifecycle cost; IPA FEL index |
| Gate 3 — FID / FEED | `PROJECT`, `FINANCE`, `CONT_BA` | `PROJECT_STATUS`, `FINANCE_TYPE` | SEC Rule S-X proved reserves sanctioned |
| Gate 4 — Execution | `PROJECT_STEP`, `EQUIPMENT`, `CONTRACT` | `PROJECT_TYPE`, `CONT_TYPE` | IOGP S-501 project execution |
| Gate 5 — Ops Readiness | `PROD_STRING`, `FACILITY_STATUS` | `STRING_STATUS`, `FACILITY_STATUS` | IEC 61511 PSSR; IOGP RP 70 |
| MOC Approval | `NOTIFICATION`, `CONSENT`, `FACILITY` | `NOTIFICATION_TYPE`, `CONSENT_COND` | OSHA 1910.119 MOC; IOGP 423 |
| AFE Approval | `FINANCE`, `CONTRACT`, `INTEREST_SET` | `FINANCE_TYPE`, `CONT_TYPE` | COPAS; CAPL JOA Article IX |

### Category 7: HSE & Safety Workflows

| Process Step | Primary PPDM Tables | Secondary / Lookup Tables | Standards |
|---|---|---|---|
| Incident Report | `HSE_INCIDENT`, `HSE_INCIDENT_BA` | `HSE_INCIDENT_TYPE`, `HSE_INCIDENT_SEVERITY` | IOGP Report 2022e; OSHA 300 log |
| HAZOP/HAZID | `PROJECT_STEP`, `RM_INFORMATION_ITEM` | `PROC_STATUS_TYPE`, `TEST_TYPE` | IEC 61882; IOGP S-509 |
| Risk Assessment | `PROJECT_STEP`, `OBLIGATION` | `OBLIGATION_TYPE`, `RISK_TYPE` | ISO 31000; IOGP RP 1015 |
| Root Cause Analysis | `HSE_INCIDENT_CAUSE`, `HSE_INCIDENT_DETAIL` | `HSE_INCIDENT_CLASS`, `HSE_INCIDENT_TYPE` | API RP 754 Tier 1-4 classification |
| Corrective Actions | `PROJECT_STEP`, `OBLIGATION`, `NOTIFICATION` | `OBLIG_TYPE`, `TRACKING_STATUS` | BSEE SEMS; CSA Z767 |
| Incident Close-out | `HSE_INCIDENT`, `HSE_INCIDENT_COMPONENT` | `HSE_INCIDENT_SEVERITY`, `PHYSICAL_STATUS` | IOGP 2022e statistical protocol |
| SIMOPS Coordination | `FACILITY`, `PROJECT_STEP_CONDITION` | `FACILITY_TYPE`, `PROC_STATUS_TYPE` | IOGP RP 70; NORSOK Z-013 |
| Emergency Response | `HSE_INCIDENT`, `FACILITY`, `NOTIFICATION` | `HSE_INCIDENT_TYPE`, `NOTIFICATION_TYPE` | API RP 74; OGP Report 434 |

### Category 8: Compliance & Regulatory

| Process Step | Primary PPDM Tables | Secondary / Lookup Tables | Jurisdiction |
|---|---|---|---|
| Well Permit (USA) | `BA_PERMIT`, `OBLIGATION`, `NOTIFICATION` | `LICENSE_TYPE`, `OBLIG_TYPE` | BSEE APD / state OGR |
| Well Permit (Canada) | `BA_LICENSE`, `BA_LICENSE_AREA` | `LICENSE_STATUS`, `LICENSE_TYPE` | AER D-056; BCOGC; NLOPB C-NLOPB |
| Production Report (USA) | `PDEN_VOL_SUMMARY`, `OBLIGATION` | `VOLUME_REGIME`, `OBLIG_TYPE` | EIA-914; OGOR; state OGRs |
| Production Report (Canada) | `PDEN_VOL_SUMMARY`, `NOTIFICATION` | `NOTIFICATION_TYPE`, `PRODUCT_TYPE` | AER ST-39; B.C. OGC; CAODC |
| Royalty Reporting | `OBLIGATION`, `OBLIG_CALC`, `OBLIG_PAYMENT` | `OBLIGATION_TYPE`, `RATE_SCHEDULE_TYPE` | USA: ONRR; Canada: NEB; Norway: Petoro |
| Environmental Compliance | `CONSENT`, `CONSENT_COND`, `OBLIGATION` | `RESTRICTION_TYPE`, `OBLIGATION_TYPE` | EPA; ECCC; OSPAR; ISO 14001 |
| GHG / Emissions | `PDEN_VOL_SUMMARY`, `OBLIGATION` | `SUBSTANCE_TYPE`, `PRODUCT_TYPE` | EPA 40CFR98; GHG Protocol; Canada OBPS |
| Export Certification | `FACILITY_LICENSE`, `CONTRACT` | `LICENSE_TYPE`, `CONT_TYPE` | FERC NGA; NEB Export; GIIGNL LNG |

### Category 9: Well Lifecycle Workflows

| Lifecycle Stage | Primary PPDM Tables | Secondary / Lookup Tables | Notes |
|---|---|---|---|
| Well Design | `WELL`, `WELL_VERSION` | `WELL_TYPE`, `WELL_STATUS` | API RP 100-1; ISO 16530-2 |
| Spud Authorization | `WELL`, `BA_PERMIT`, `NOTIFICATION` | `NOTIF_BA`, `OBLIG_TYPE` | BSEE Form 123; AER Directive 56 |
| Drilling Operations | `WELL`, `CASING_PROGRAM`, `PPDM_AUDIT_HISTORY` | `WELL_STATUS` | API Spec 5CT; ISO 11960 |
| Completion | `PROD_STRING`, `PR_STR_FORM_COMPLETION` | `STRING_TYPE`, `STRING_STATUS` | API RP 10A; ISO 10426 |
| Well Tie-in | `PROD_STRING`, `FACILITY`, `PDEN_WELL` | `STRING_STATUS`, `FACILITY_TYPE` | API RP 500; NFPA 59A |
| Production Phase | `PROD_STR_STAT_HIST`, `PDEN_VOL_SUMMARY` | `STRING_STATUS`, `VOLUME_REGIME` | PRMS producing status |
| Workover | `WELL`, `WELL_VERSION`, `PROJECT_STEP` | `WELL_STATUS`, `WORK_ORDER_TYPE` | API RP 100-1 §10; AER D-036 |
| Well Suspension | `WELL`, `WELL_VERSION`, `NOTIFICATION` | `WELL_STATUS`, `NOTIF_BA` | BSEE NTL 2010-G05 |
| P&A Execution | `WELL`, `PROD_STRING`, `OBLIGATION` | `WELL_STATUS`, `OBLIG_TYPE` | API RP 100-2; NORSOK D-010 |

### Category 10: Facility Lifecycle Workflows

| Lifecycle Stage | Primary PPDM Tables | Secondary / Lookup Tables | Notes |
|---|---|---|---|
| Concept & Screening | `FACILITY`, `PROJECT`, `AREA` | `FACILITY_TYPE`, `PROJECT_TYPE` | IPA FEL 0 front-end loading |
| FEED & Engineering | `FACILITY_VERSION`, `PROJECT_PLAN` | `FACILITY_STATUS`, `PROJECT_STATUS` | ISO 15663; IPA FEL 1-2 |
| Procurement | `EQUIPMENT`, `CONTRACT`, `PROJECT_STEP` | `EQUIPMENT_TYPE`, `CONT_TYPE` | IOGP S-501 |
| PSSR | `FACILITY`, `PROJECT_STEP_CONDITION` | `FACILITY_STATUS`, `PROC_STATUS_TYPE` | IEC 61511 §5.3; IOGP RP 70 |
| Commissioning | `FACILITY`, `FACILITY_STATUS`, `PROD_STRING` | `FACILITY_STATUS`, `STRING_STATUS` | ISO 20815 production assurance |
| Operations & Maintenance | `FACILITY_MAINTAIN`, `EQUIPMENT_MAINTAIN` | `FACILITY_MAINT_STATUS`, `EQUIPMENT_MAINT_TYPE` | ISO 55001; API RP 580 RBI |
| Modification / MOC | `FACILITY_VERSION`, `NOTIFICATION`, `CONSENT` | `FACILITY_TYPE`, `NOTIFICATION_TYPE` | OSHA 1910.119(l); IOGP 423 |
| Shutdown & Decommission | `FACILITY_STATUS`, `OBLIGATION` | `FACILITY_STATUS`, `OBLIG_TYPE` | OSPAR; MMS NTL; ISO 13702 |

### Category 11: Reservoir Management Workflows

| Process Step | Primary PPDM Tables | Secondary / Lookup Tables | Notes |
|---|---|---|---|
| Annual Performance Review | `POOL`, `POOL_VERSION`, `PDEN_VOL_SUMMARY` | `POOL_STATUS`, `VOLUME_REGIME` | SPE PRMS §5 — reserves update |
| History Match | `PDEN_MATERIAL_BAL`, `PDEN_VOLUME_ANALYSIS` | `PDEN_VOL_REGIME` | SPE-197049 simulation best practices |
| Reserves Certification | `POOL_VERSION`, `PDEN_VOL_SUMMARY` | `POOL_STATUS`, `RESENT_TYPE` | NI 51-101 (Canada); SEC 17 CFR 229 (USA) |
| IOR/EOR Screening | `PDEN_VOL_SUMMARY`, `PROJECT` | `PROJECT_TYPE`, `PRODUCT_TYPE` | API RP 19D IOR screening matrix |
| Pressure Maintenance | `PDEN_MATERIAL_BAL`, `PROD_STRING` | `STRING_TYPE`, `VOLUME_REGIME` | SPE 23311 pressure maintenance criteria |
| Infill Drilling Decision | `WELL`, `POOL`, `PROJECT_PLAN` | `WELL_TYPE`, `POOL_STATUS` | SPE PRMS developed undeveloped classification |
| Pool Reclassification | `POOL`, `POOL_VERSION`, `CLASS_LEVEL` | `POOL_TYPE`, `POOL_STATUS` | PRMS §3 reclassification criteria |
| EOR Sanction | `PROJECT`, `INTEREST_SET`, `FINANCE` | `PROJECT_STATUS`, `FINANCE_TYPE` | SPE PRMS §5.3.7 EOR |

### Category 12: Pipeline & Infrastructure Workflows

| Process Step | Primary PPDM Tables | Secondary / Lookup Tables | Notes |
|---|---|---|---|
| Integrity Assessment | `FACILITY`, `FACILITY_MAINTAIN`, `EQUIPMENT` | `FACILITY_TYPE`, `EQUIPMENT_TYPE` | ASME B31.8S (USA); CSA Z662-15 (Canada) |
| Pigging Campaign | `FACILITY_MAINTAIN`, `PROJECT_STEP` | `FACILITY_MAINT_STATUS`, `PROC_STATUS_TYPE` | NACE SP0169; IOGP S-509 |
| Corrosion Programme | `FACILITY_MAINTAIN`, `CAT_ADDITIVE` | `CAT_ADDITIVE_TYPE`, `FACILITY_MAINT_STATUS` | NACE MR0175/ISO 15156 |
| Tie-in Authorization | `FACILITY`, `BA_PERMIT`, `NOTIFICATION` | `FACILITY_TYPE`, `LICENSE_TYPE` | FERC Form 2; NEB Regulation OPR-99 |
| Pressure Testing | `FACILITY`, `PROJECT_STEP_CONDITION` | `PROC_STATUS_TYPE`, `TEST_TYPE` | ASME B31.8 §841.3; API RP 1110 |
| Pipeline Expansion | `FACILITY_VERSION`, `PROJECT`, `INTEREST_SET` | `FACILITY_TYPE`, `PROJECT_STATUS` | FERC CEII; NEB/CER GH-001 |
| Emergency Repair | `FACILITY_MAINTAIN`, `HSE_INCIDENT`, `NOTIFICATION` | `FACILITY_MAINT_STATUS`, `HSE_INCIDENT_TYPE` | API RP 1161; DOT PHMSA regulations |
| Pipeline Abandonment | `FACILITY_STATUS`, `OBLIGATION`, `CONSENT` | `FACILITY_STATUS`, `OBLIG_TYPE` | FERC Order 7157; NEB OPR-99 §60-61 |

---

## 2.3 — Responsibility Matrix (RACI)

> R = Responsible (does the work) | A = Accountable (owns the outcome)  
> C = Consulted (input required) | I = Informed (notified)

### Phase 2 — Service Layer

| Task | Backend Dev | Data Architect | QA/Test | Domain SME | Ops | Compliance |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| 2.1 — Extend ProcessDefinitionInitializer | R | C | I | C | I | I |
| 2.2 — Gate-Review state machine | R/A | C | C | C | I | C |
| 2.3 — Work Order state machine | R/A | C | C | I | C | I |
| 2.4 — HSE Workflow state machine | R | C | C | R(SME) | C | C |
| 2.5 — Compliance Workflow state machine | R | C | C | I | I | R/A |
| 2.6 — Well/Facility/Res/Pipeline lifecycle SMs | R | C | C | C | C | I |
| 2.7 — GetProcessDefinitionByNameAsync helper | R/A | I | C | I | I | I |
| 2.8 — Unit tests for new state machines | C | I | R/A | I | I | I |
| PPDM table mapping review | R | R/A | I | C | I | C |
| International standards alignment | C | C | I | R/A | I | R |

---

## 2.4 — State Machine Definitions

### Work Order State Machine (ISO 55001 / SAP PM aligned)

```
DRAFT ──plan──► PLANNED ──start──► IN_PROGRESS
                                       │
                      ┌────hold────────┘
                      ▼
                   ON_HOLD ──resume──► IN_PROGRESS
                      │
                   cancel
                      ▼
                 CANCELLED

IN_PROGRESS ──complete──► COMPLETED
IN_PROGRESS ──cancel────► CANCELLED
DRAFT ──cancel──────────► CANCELLED
```

**Guard conditions**:
- `start`: equipment + BA service must be assigned (fields non-null)
- `complete`: all `PROJECT_STEP_CONDITION` rows marked `SATISFIED`
- `hold`: reason text required (written to `PPDM_AUDIT_HISTORY`)

### Gate-Review State Machine (SPE Stage-Gate / IPA FEL)

```
PENDING ──submit──► UNDER_REVIEW
                        │
           ┌────────────┼────────────┐
         approve      reject        defer
           │            │            │
           ▼            ▼            ▼
        APPROVED     REJECTED     DEFERRED ──resubmit──► UNDER_REVIEW
```

**Guard conditions**:
- `submit`: all required documents attached to `RM_INFORMATION_ITEM`
- `approve`: minimum approver count met (per gate definition)
- `defer`: deferral reason + target date required

### HSE Incident State Machine (IOGP 2022e / API RP 754)

```
REPORTED ──assign──► INVESTIGATING
                          │
                    ──rca_complete──►  ROOT_CAUSE_ANALYSIS
                                              │
                                      ──actions_raised──►  ACTIONS_OPEN
                                                               │
                                                       ──all_closed──►  CLOSED
```

Every HSE state transition writes to `HSE_INCIDENT_COMPONENT` (audit log).

### Compliance State Machine (ONRR / NEB / EPA aligned)

```
DRAFT ──submit──► SUBMITTED ──review_start──► UNDER_REVIEW
                                                    │
                                    ┌───────────────┴──────────────┐
                               compliant                     non_compliant
                                    │                              │
                                    ▼                              ▼
                                COMPLIANT                   NON_COMPLIANT
                                                                   │
                                                           remediation_start
                                                                   │
                                                                   ▼
                                                             REMEDIATION ──remediation_complete──► CLOSED
```

---

## 2.5 — Todo Tracker

### Sprint 1 — State Machines & Helpers

| ID | Task | Priority | File | Status | Notes |
|---|---|---|---|---|---|
| 2.1.1 | Extend `InitializeDefaultProcessDefinitionsAsync` — add 8 missing category calls | HIGH | `LifeCycle/Services/Processes/ProcessDefinitionInitializer.cs` | 🔲 | Add `InitializeWorkOrderProcessesAsync`, `InitializeGateReviewProcessesAsync`, `InitializeHSEProcessesAsync`, `InitializeComplianceProcessesAsync`, `InitializeWellLifecycleProcessesAsync`, `InitializeFacilityLifecycleProcessesAsync`, `InitializeReservoirMgmtProcessesAsync`, `InitializePipelineProcessesAsync` |
| 2.1.2 | Seed Work Order category (6 definitions) | HIGH | same | 🔲 | Preventive, Corrective, Inspection, Turnaround, Emergency, Capital |
| 2.1.3 | Seed Gate Review category (8 definitions) | HIGH | same | 🔲 | Gate 0–5, MOC, AFE; each with `RequiresApproval=true` and role guard |
| 2.1.4 | Seed HSE category (8 definitions) | HIGH | same | 🔲 | Map steps to `HSE_INCIDENT` columns |
| 2.1.5 | Seed Compliance category (8 definitions) | HIGH | same | 🔲 | Separate USA / Canada / International variants via `ProcessType` tag |
| 2.1.6 | Seed Well/Facility/Reservoir/Pipeline lifecycles (32 definitions) | MEDIUM | same | 🔲 | Cross-reference existing `WellLifecycleService` |
| 2.2 | Register Gate-Review state machine in `PPDMProcessService._getStateMachine` | HIGH | `LifeCycle/Services/Processes/PPDMProcessService.cs` | 🔲 | See §2.4 above |
| 2.3 | Register Work Order state machine | HIGH | same | 🔲 | |
| 2.4 | Register HSE state machine | HIGH | same | 🔲 | |
| 2.5 | Register Compliance state machine | HIGH | same | 🔲 | |
| 2.6 | Register Well/Facility/Res/Pipeline lifecycle state machines | MEDIUM | same | 🔲 | Reuse `WellLifecycleService` transitions via adapter |
| 2.7 | Add `GetProcessDefinitionByNameAsync(string processName)` to `IProcessService` | HIGH | `IProcessService.cs` + `ProcessServiceBase.cs` | 🔲 | Used by `BusinessProcessNode.ExecuteBranchAction` |
| 2.8 | Unit tests — Gate-Review SM: valid and invalid transitions | HIGH | `Tests/Services/Processes/GateReviewStateMachineTests.cs` | 🔲 | |
| 2.9 | Unit tests — Work Order SM: full DRAFT→COMPLETED lifecycle | HIGH | `Tests/Services/Processes/WorkOrderStateMachineTests.cs` | 🔲 | |
| 2.10 | Unit tests — HSE SM: incident lifecycle + guard conditions | MEDIUM | `Tests/Services/Processes/HSEStateMachineTests.cs` | 🔲 | |
| 2.11 | Unit tests — Compliance SM: non-compliant path + remediation | MEDIUM | `Tests/Services/Processes/ComplianceStateMachineTests.cs` | 🔲 | |

### Sprint 2 — PPDM Table Binding

| ID | Task | Priority | Status | Notes |
|---|---|---|---|---|
| 2.12 | Add `PrimaryPPDMTable` and `SecondaryPPDMTables` properties to `ProcessStepDefinition` | HIGH | 🔲 | Enables UI to show related tables per step |
| 2.13 | Populate PPDM table bindings for all 12 process category step definitions | MEDIUM | 🔲 | Use table mapping in §2.2 above |
| 2.14 | Add `JurisdictionTags` to `ProcessDefinition` (`USA`, `CANADA`, `INTERNATIONAL`) | MEDIUM | 🔲 | Used by compliance filter; maps to `COUNTRY` and `STATE` reference tables |
| 2.15 | Add `RegulatoryReference` string field to `ProcessStepDefinition` | LOW | 🔲 | e.g. `"BSEE 30 CFR 250.425"`, `"AER Directive 56"`, shown in step help text |

---

## 2.6 — International Standards Alignment

| Jurisdiction | Regulatory Body | Key Standards | PPDM Tables Affected |
|---|---|---|---|
| **USA — Federal Offshore** | BSEE / BOEM / ONRR | 30 CFR 250, 30 CFR 1218, API RP 100, EPA 40 CFR 98 | `BA_PERMIT`, `OBLIGATION`, `PDEN_VOL_SUMMARY` |
| **USA — State (Texas)** | RRC Texas | Statewide Rule 5/6/7 | `BA_PERMIT`, `WELL`, `NOTIFICATION` |
| **USA — State (North Dakota)** | NDIC | ND Admin Code Art. 43-02-03 | `WELL`, `OBLIGATION` |
| **USA — State (Colorado)** | ECMC / COGCC | 2 CCR 404-1 | `CONSENT`, `HSE_INCIDENT` |
| **Canada — Alberta** | AER | Directive 056, 023, 036, SP 2014-01 | `BA_LICENSE`, `WELL`, `NOTIFICATION` |
| **Canada — BC** | BCOGC / BC OGC | OGC Manual 5; OGAA | `BA_LICENSE`, `OBLIGATION` |
| **Canada — NB / NFLD (Offshore)** | C-NLOPB / CNSOPB | Accord Acts | `BA_LICENSE`, `INTEREST_SET` |
| **Canada — Federal** | NEB / CER | OPR-99; CPR-99; Filling Manual | `OBLIGATION`, `PDEN_VOL_SUMMARY`, `CONTRACT` |
| **UK** | NSTA / OGA | MER UK Strategy, UKCS Well Regulations 2015 | `WELL`, `FACILITY`, `OBLIGATION` |
| **Norway** | NPD / PTIL | PSA Framework; NCS Petroleum Resources Act | `WELL`, `OBLIGATION`, `HSE_INCIDENT` |
| **Netherlands** | SodM / EZK | Mining Act; Mining Decree | `BA_LICENSE`, `POOL_VERSION`, `FIELD` |
| **Australia** | NOPSEMA / DMIRS | OPGGS Act; OHS(OI) Regs | `FACILITY`, `HSE_INCIDENT`, `OBLIGATION` |
| **International (Generic)** | SPE / IOGP / ISO | PRMS; IOGP S-501; ISO 55001/15663/31000 | All categories |
| **Middle East** | Aramco / ADNOC / NOC | GSP standards; IOGP equivalents | `WELL`, `FACILITY`, `CONTRACT` |

---

## 2.7 — Definition of Done (Phase 2)

- [ ] All 12 `Initialize*ProcessesAsync` methods exist and produce valid `ProcessDefinition` objects
- [ ] All 4 new state machines compile, registered, and unit-tested
- [ ] `GetProcessDefinitionByNameAsync` returns correct result for all 96 process names
- [ ] `PrimaryPPDMTable` populated on all step definitions covering §2.2 mapping
- [ ] `JurisdictionTags` present on all 96 definitions
- [ ] All unit tests green (`dotnet test`)
- [ ] `dotnet build Beep.OilandGas.sln` — 0 errors

---

## 2.8 — Architecture Decision Records (ADRs)

### ADR-2-01: ProcessDefinition Persistence Strategy

**Context**: Process definitions are seeded at startup. Two options:
1. Seed into PPDM `PROJECT` / `PROJECT_PLAN` rows using `PPDMGenericRepository`
2. Seed into an in-memory `ConcurrentDictionary` inside `PPDMProcessService`

**Decision**: **Option 1** — persist to PPDM tables so definitions survive service restarts,
are auditable via `PPDM_AUDIT_HISTORY`, and can be edited by privileged users at runtime.

**Consequences**:
- `PROJECT.PROJECT_TYPE = 'PROCESS_DEF'` is the discriminator
- `PROJECT_PLAN.PLAN_TYPE = 'STATE_MACHINE'` holds transition JSON
- `PROJECT_STEP` rows represent individual steps; `SEQUENCE_NUMBER` gives ordering
- `ProcessDefinitionInitializer` must be idempotent (check `PROJECT_OBS_NO` before insert)

---

### ADR-2-02: State Machine Implementation Strategy

**Context**: Where should state transition logic live?

**Options**:
A. `ProcessStateMachine` stores transitions as a hard-coded switch statement per process type  
B. `ProcessStateMachine` loads allowed transitions from `PROJECT_PLAN` JSON column at runtime  
C. Stateless function dictionary keyed by `(currentState, transitionName)`

**Decision**: **Option C** — stateless transition dictionaries registered per `ProcessType` in
`PPDMProcessService._transitionRegistry`. The registry is populated by each
`Initialize*Async` method, making each state machine independently testable.

**Structure**:
```csharp
// key: (ProcessType, CurrentState, TransitionName) → NextState
Dictionary<(string, string, string), string> _transitionRegistry;

// guard conditions:
Dictionary<(string, string, string), Func<ProcessInstance, Task<bool>>> _guardRegistry;
```

---

### ADR-2-03: Jurisdiction Tagging

**Context**: The same process (e.g. "Well Permit") has different required fields and
regulatory references depending on jurisdiction.

**Decision**: `ProcessDefinition.JurisdictionTags` is a `List<string>` with values
`USA`, `CANADA`, `UK`, `NORWAY`, `AUSTRALIA`, `INTERNATIONAL`.
`ProcessStepDefinition.RegulatoryReference` holds the specific citation, e.g.
`"BSEE 30 CFR 250.425"` or `"AER Directive 056 §4.1"`.

The UI filter uses `JurisdictionTags` to narrow the definition list.
Compliance reports use `RegulatoryReference` to pre-fill form headers.

---

## 2.9 — Data Model Extensions Required

### `ProcessDefinition` — New Fields

```csharp
// Beep.OilandGas.Models/Data/Process/ProcessDefinition.cs  (ADD to existing class)
public List<string> JurisdictionTags { get; set; } = new();
// e.g. ["USA", "CANADA"]

public string ProcessCategory { get; set; } = string.Empty;
// Maps to BusinessProcessCategories.Id (1–12)

public bool RequiresApproval { get; set; }
// true for Gate Reviews and AFE processes

public int MinApproverCount { get; set; }
// Gate 3 (FID) typically requires ≥2 approvers per JOA

public string DefaultTimeoutDays { get; set; } = "30";
// SLA default; overridden per instance
```

### `ProcessStepDefinition` — New Fields

```csharp
// Beep.OilandGas.Models/Data/Process/ProcessStepDefinition.cs  (ADD to existing class)
public string PrimaryPPDMTable { get; set; } = string.Empty;
// e.g. "HSE_INCIDENT"

public List<string> SecondaryPPDMTables { get; set; } = new();
// e.g. ["HSE_INCIDENT_BA", "HSE_INCIDENT_CAUSE"]

public string RegulatoryReference { get; set; } = string.Empty;
// citation shown in step help text

public bool IsDocumentRequired { get; set; }
// if true, a RM_INFORMATION_ITEM attachment is mandatory before advance

public string RequiredDocumentType { get; set; } = string.Empty;
// e.g. "HAZOP_REPORT", "AFE_FORM", "WELL_PERMIT"
```

### `ProcessInstance` — New Fields

```csharp
// Beep.OilandGas.Models/Data/Process/ProcessInstance.cs  (ADD to existing class)
public string JurisdictionTag { get; set; } = string.Empty;
// Set at instance creation from field's country code

public DateTime? DueDate { get; set; }
// Computed from ProcessDefinition.DefaultTimeoutDays at StartProcess

public List<string> ApproverUserIds { get; set; } = new();
// Populated from PROJECT_STEP_BA rows

public bool IsOverdue => DueDate.HasValue && DateTime.UtcNow > DueDate.Value
    && Status == ProcessStatus.IN_PROGRESS;
```

---

## 2.10 — Sequence Diagrams

### Starting a Process Instance

```
Caller                    PPDMProcessService              PPDMGenericRepository          PPDM DB
  │                              │                                 │                       │
  ├─StartProcessAsync()─────────►│                                 │                       │
  │                              ├─GetProcessDefinitionAsync()────►│                       │
  │                              │◄────ProcessDefinition────────────│                       │
  │                              ├─Validate(IsActive, JurisdictionTag)                     │
  │                              ├─new ProcessInstance()           │                       │
  │                              │  CurrentStep = firstStep        │                       │
  │                              │  DueDate = now + timeout        │                       │
  │                              ├─InsertAsync(instance)──────────►│──INSERT PROJECT───────►│
  │                              │                                 │◄─OK────────────────────│
  │                              ├─InsertAsync(auditRow)──────────►│──INSERT PPDM_AUDIT────►│
  │◄────ProcessInstance──────────│                                 │                       │
```

### State Transition (with Guard)

```
Caller                    PPDMProcessService         GuardRegistry         PPDM DB
  │                              │                        │                    │
  ├─TransitionAsync(id,trans)───►│                        │                    │
  │                              ├─GetInstance(id)───────────────────────────►│
  │                              │◄────instance────────────────────────────────│
  │                              ├─LookupNextState(type,cur,trans)             │
  │                              ├─EvaluateGuard()───────►│                   │
  │                              │◄─guardPassed(bool)──────│                   │
  │                              │ [if false] throw InvalidOperationException  │
  │                              ├─instance.CurrentState = nextState           │
  │                              ├─UpdateAsync(instance)─────────────────────►│
  │                              ├─WriteAuditRow(old→new)────────────────────►│
  │                              ├─[if RequiresApproval] WriteNotification────►│
  │◄────TransitionResult─────────│                                             │
```

---

## 2.11 — Error Handling Rules

| Error Scenario | Exception Type | HTTP Mapping | Recovery |
|---|---|---|---|
| Process definition not found | `KeyNotFoundException` | 404 | Return empty list to caller; log WARNING |
| Transition not valid for current state | `InvalidOperationException` | 422 | Return allowed transitions list in error payload |
| Guard condition failed (e.g. no document attached) | `ProcessGuardException` (new) | 422 | Include `RequiredField` in error response |
| Instance already CLOSED / CANCELLED | `InvalidOperationException` | 409 | Explain instance is terminal state |
| PPDM write fails (optimistic concurrency) | `DataException` | 503 | Retry once; then return 503 with `Retry-After: 5` |
| Approver not in `BA_AUTHORITY` | `UnauthorizedAccessException` | 403 | Include required role in error message |

**`ProcessGuardException`** (new class):

```csharp
// Beep.OilandGas.LifeCycle/Services/Processes/Exceptions/ProcessGuardException.cs
public class ProcessGuardException : InvalidOperationException
{
    public string TransitionName { get; }
    public string RequiredField { get; }
    public string RegulatoryReference { get; }

    public ProcessGuardException(
        string transitionName, string requiredField, string regulatoryRef = "")
        : base($"Guard failed for '{transitionName}': {requiredField} is required. Ref: {regulatoryRef}")
    {
        TransitionName = transitionName;
        RequiredField = requiredField;
        RegulatoryReference = regulatoryRef;
    }
}
```

---

## 2.12 — Code Templates

### `InitializeWorkOrderProcessesAsync` template

```csharp
private async Task InitializeWorkOrderProcessesAsync(string userId)
{
    var definitions = new List<ProcessDefinition>
    {
        new ProcessDefinition
        {
            ProcessId       = "WO-PREVENTIVE",
            ProcessName     = "Preventive Maintenance Work Order",
            ProcessType     = "WORK_ORDER",
            ProcessCategory = "5",                          // BusinessProcessCategories.WorkOrders
            Description     = "Scheduled preventive maintenance per ISO 55001 §8.3",
            IsActive        = true,
            RequiresApproval = false,
            JurisdictionTags = new List<string> { "USA", "CANADA", "INTERNATIONAL" },
            Steps = new List<ProcessStepDefinition>
            {
                new ProcessStepDefinition
                {
                    StepId              = "WO-PREV-01",
                    StepName            = "Draft Work Order",
                    SequenceNumber      = 1,
                    AllowedTransitions  = new List<string> { "plan", "cancel" },
                    PrimaryPPDMTable    = "PROJECT",
                    SecondaryPPDMTables = new List<string> { "PROJECT_PLAN", "PROJECT_STEP" },
                    RegulatoryReference = "ISO 55001:2018 §8.3",
                    IsDocumentRequired  = false
                },
                new ProcessStepDefinition
                {
                    StepId              = "WO-PREV-02",
                    StepName            = "Plan & Schedule",
                    SequenceNumber      = 2,
                    AllowedTransitions  = new List<string> { "start", "cancel" },
                    PrimaryPPDMTable    = "EQUIPMENT",
                    SecondaryPPDMTables = new List<string> { "EQUIPMENT_USE_STAT", "PROJECT_STEP_TIME" },
                    RegulatoryReference = "ISO 14224:2016 §5.2",
                    IsDocumentRequired  = false
                },
                new ProcessStepDefinition
                {
                    StepId              = "WO-PREV-03",
                    StepName            = "Assign Contractor / Crew",
                    SequenceNumber      = 3,
                    AllowedTransitions  = new List<string> { "start_work", "hold" },
                    PrimaryPPDMTable    = "PROJECT_STEP_BA",
                    SecondaryPPDMTables = new List<string> { "BUSINESS_ASSOCIATE" },
                    RegulatoryReference = "IOGP S-501 §3.2",
                    IsDocumentRequired  = false
                },
                new ProcessStepDefinition
                {
                    StepId              = "WO-PREV-04",
                    StepName            = "Execute Maintenance",
                    SequenceNumber      = 4,
                    AllowedTransitions  = new List<string> { "inspect", "hold" },
                    PrimaryPPDMTable    = "EQUIPMENT_MAINTAIN",
                    SecondaryPPDMTables = new List<string> { "FACILITY_MAINTAIN", "EQUIPMENT_MAINT_STATUS" },
                    RegulatoryReference = "ISO 55001:2018 §8.4",
                    IsDocumentRequired  = false
                },
                new ProcessStepDefinition
                {
                    StepId              = "WO-PREV-05",
                    StepName            = "Inspection & Sign-off",
                    SequenceNumber      = 5,
                    AllowedTransitions  = new List<string> { "complete", "rework" },
                    PrimaryPPDMTable    = "PROJECT_STEP_CONDITION",
                    SecondaryPPDMTables = new List<string> { "RM_INFORMATION_ITEM" },
                    RegulatoryReference = "BSEE SEMS §250.1920",
                    IsDocumentRequired  = true,
                    RequiredDocumentType = "INSPECTION_CERTIFICATE"
                },
                new ProcessStepDefinition
                {
                    StepId              = "WO-PREV-06",
                    StepName            = "Cost Capture & Close",
                    SequenceNumber      = 6,
                    AllowedTransitions  = new List<string> { "close" },
                    PrimaryPPDMTable    = "FINANCE",
                    SecondaryPPDMTables = new List<string> { "FIN_COMPONENT" },
                    RegulatoryReference = "COPAS PGAS; IOGP Report 461",
                    IsDocumentRequired  = false
                }
            }
        },
        // ... (Corrective, Inspection, Turnaround, Emergency, Capital follow same pattern)
    };

    foreach (var def in definitions)
    {
        var existing = await GetProcessDefinitionAsync(def.ProcessId);
        if (existing == null)
            await CreateProcessDefinitionAsync(def, userId);
    }
}
```

### `RegisterWorkOrderStateMachine` template

```csharp
private void RegisterWorkOrderStateMachine()
{
    const string type = "WORK_ORDER";

    // Allowed transitions: (ProcessType, CurrentState, TransitionName) → NextState
    _transitionRegistry[(type, "DRAFT",       "plan")]         = "PLANNED";
    _transitionRegistry[(type, "DRAFT",       "cancel")]       = "CANCELLED";
    _transitionRegistry[(type, "PLANNED",     "start")]        = "IN_PROGRESS";
    _transitionRegistry[(type, "PLANNED",     "cancel")]       = "CANCELLED";
    _transitionRegistry[(type, "IN_PROGRESS", "hold")]         = "ON_HOLD";
    _transitionRegistry[(type, "IN_PROGRESS", "complete")]     = "COMPLETED";
    _transitionRegistry[(type, "IN_PROGRESS", "cancel")]       = "CANCELLED";
    _transitionRegistry[(type, "ON_HOLD",     "resume")]       = "IN_PROGRESS";
    _transitionRegistry[(type, "ON_HOLD",     "cancel")]       = "CANCELLED";

    // Guard conditions
    _guardRegistry[(type, "PLANNED", "start")] = async instance =>
    {
        // Equipment must be assigned  AND  at least one BA service assigned
        bool hasEquipment = !string.IsNullOrWhiteSpace(instance.EntityId);
        bool hasBA        = instance.ApproverUserIds.Count > 0;
        if (!hasEquipment)
            throw new ProcessGuardException("start", "EntityId (EquipmentId)", "ISO 55001 §8.4");
        if (!hasBA)
            throw new ProcessGuardException("start", "Assigned Contractor (PROJECT_STEP_BA)", "IOGP S-501 §3.2");
        return true;
    };

    _guardRegistry[(type, "IN_PROGRESS", "complete")] = async instance =>
    {
        // All PROJECT_STEP_CONDITION rows must be SATISFIED
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "STEP_ID",    Operator = "=", FilterValue = instance.CurrentStepId },
            new AppFilter { FieldName = "COND_STATUS", Operator = "<>", FilterValue = "SATISFIED" }
        };
        var unsatisfied = await _conditionsRepo.GetAsync(filters);
        if (unsatisfied.Any())
            throw new ProcessGuardException("complete",
                $"{unsatisfied.Count} inspection conditions not yet SATISFIED",
                "BSEE SEMS §250.1920");
        return true;
    };
}
```

### `GetProcessDefinitionByNameAsync` — new interface method

```csharp
// IProcessService.cs — ADD
Task<ProcessDefinition?> GetProcessDefinitionByNameAsync(string processName);

// ProcessServiceBase.cs — BASE IMPLEMENTATION
public virtual async Task<ProcessDefinition?> GetProcessDefinitionByNameAsync(string processName)
{
    if (string.IsNullOrWhiteSpace(processName))
        return null;

    var allDefs = await GetProcessDefinitionsByTypeAsync(string.Empty); // all types
    return allDefs.FirstOrDefault(d =>
        string.Equals(d.ProcessName, processName, StringComparison.OrdinalIgnoreCase));
}
```

---

## 2.13 — Sprint Planning

### Sprint 1 (Weeks 1–2) — Model Extensions + Work Order

| Story | Points | Owner |
|---|---|---|
| Add `JurisdictionTags`, `ProcessCategory`, `RequiresApproval`, `MinApproverCount` to `ProcessDefinition` | 3 | Backend Dev |
| Add `PrimaryPPDMTable`, `SecondaryPPDMTables`, `RegulatoryReference`, `IsDocumentRequired` to `ProcessStepDefinition` | 3 | Backend Dev |
| Create `ProcessGuardException` class | 1 | Backend Dev |
| Implement `_transitionRegistry` + `_guardRegistry` in `PPDMProcessService` | 5 | Backend Dev |
| `InitializeWorkOrderProcessesAsync` (6 definitions + guards) | 8 | Backend Dev |
| Unit tests — Work Order SM (DRAFT→PLANNED→IN_PROGRESS→COMPLETED) | 5 | QA |
| Unit tests — Work Order guard failures (no equipment, unsatisfied conditions) | 3 | QA |

### Sprint 2 (Weeks 3–4) — Gate Reviews + HSE

| Story | Points | Owner |
|---|---|---|
| `InitializeGateReviewProcessesAsync` (8 definitions: Gate 0–5 + MOC + AFE) | 8 | Backend Dev |
| Gate-Review state machine + approver count guard | 5 | Backend Dev |
| `InitializeHSEProcessesAsync` (8 definitions mapped to HSE_INCIDENT tables) | 8 | Backend Dev |
| HSE incident state machine + API RP 754 Tier classification mapping | 5 | Backend Dev + SME |
| Unit tests — Gate review (approve / reject / defer paths) | 5 | QA |
| Unit tests — HSE lifecycle (REPORTED→INVESTIGATING→CLOSED) | 5 | QA |

### Sprint 3 (Weeks 5–6) — Compliance + Remaining Lifecycles

| Story | Points | Owner |
|---|---|---|
| `InitializeComplianceProcessesAsync` (8 definitions with jurisdiction variants) | 8 | Backend Dev + Compliance |
| Compliance state machine (DRAFT→COMPLIANT / NON_COMPLIANT→REMEDIATION) | 5 | Backend Dev |
| `InitializeWellLifecycleProcessesAsync` (9 definitions) | 5 | Backend Dev |
| `InitializeFacilityLifecycleProcessesAsync` (8 definitions) | 5 | Backend Dev |
| `InitializeReservoirMgmtProcessesAsync` (8 definitions) | 5 | Backend Dev |
| `InitializePipelineProcessesAsync` (8 definitions) | 5 | Backend Dev |
| Unit tests — Compliance (non-compliant + remediation path) | 5 | QA |

### Sprint 4 (Weeks 7–8) — PPDM Bindings + Jurisdiction Tagging

| Story | Points | Owner |
|---|---|---|
| `GetProcessDefinitionByNameAsync` implementation + tests | 3 | Backend Dev |
| Populate `PrimaryPPDMTable` + `SecondaryPPDMTables` on all 96 step definitions | 8 | Data Architect |
| Populate `RegulatoryReference` on all step definitions (see §2.2 mapping) | 5 | Compliance Officer |
| Populate `JurisdictionTags` on all 96 definitions | 3 | Compliance Officer |
| Integration test: full lifecycle start→complete using in-memory PPDM dataset | 8 | QA |
| `ProcessDefinitionInitializer` idempotency test (re-run produces no duplicates) | 3 | QA |
| Tech review: verify `dotnet build` clean across all target frameworks | 2 | Backend Dev |

---

## 2.14 — Risk Register

| ID | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R2-01 | `ProcessStateMachine` refactor breaks existing Exploration/Development flows | Medium | HIGH | Run existing unit tests before and after; add regression suite first |
| R2-02 | PPDM table column names differ between SQLite (dev) and SQL Server (prod) | Low | HIGH | Use `AppFilter` throughout; never hardcode identifiers |
| R2-03 | 96 definition seeds cause slow startup | Low | Medium | Idempotency check uses indexed `PROJECT_NAME` lookup; only inserts missing |
| R2-04 | Domain SME availability for HSE / compliance guard rule sign-off | Medium | Medium | Stub definitions with placeholder guards; flag as `Draft=true` until SME review |
| R2-05 | `ProcessDefinition` model change breaks `BusinessProcessNode` branch (Phase 1) | Low | High | Add null-safe getters to `BusinessProcessCategoryNode.GetProcessNamesForCategory` |
