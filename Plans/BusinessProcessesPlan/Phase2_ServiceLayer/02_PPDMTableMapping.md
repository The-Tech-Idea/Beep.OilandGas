# Phase 2 — PPDM39 Table Mapping
## Column-Level Mapping for All 12 Process Categories

> All read/write operations use `PPDMGenericRepository` with `AppFilter`.  
> Column names follow PPDM 3.9 standard (UPPERCASE).  
> Tables prefixed with `R_` are reference/lookup tables — read-only via `LOVManagementService`.

---

## Reading Guide

Each section shows:
- **Write tables**: written by service when step is started or completed
- **Read tables**: queried to validate guard conditions or populate UI
- **Lookup tables**: `R_*` tables providing controlled vocabulary values
- **Audit tables**: always written on every state change

**Universal audit entry** (all transitions):
- `PPDM_AUDIT_HISTORY`: columns `TABLE_NAME`, `ROW_ID`, `CHANGE_TYPE`, `OLD_VALUE`, `NEW_VALUE`, `CHANGE_USER`, `CHANGE_DATE`

---

## Category 1: Exploration

### EXP-LEAD-ASSESS — Lead to Prospect

| Step | Write Columns | Read/Guard Columns | Lookup Values |
|---|---|---|---|
| Create Lead | `POOL.POOL_ID`, `POOL.POOL_NAME`, `POOL.POOL_STATUS = 'LEAD'`, `POOL.ACTIVE_IND = 'Y'` | `FIELD.FIELD_ID` (must exist), `AREA.AREA_ID` | `R_POOL_TYPE`, `R_POOL_STATUS` |
| G&G Data Capture | `RM_INFORMATION_ITEM.INFO_ITEM_ID`, `RM_INFORMATION_ITEM.INFO_ITEM_SUBTYPE = 'G&G_REPORT'`, `SEIS_SURVEY.SURVEY_ID` | — | `R_INFO_ITEM_SUBTYPE`, `R_SEIS_DATA_TYPE` |
| Risk & Resource Assessment | `POOL_VERSION.POOL_OBS_NO`, `POOL_VERSION.GROSS_RESOURCE_VOL`, `CLASS_LEVEL.CLASS_LEVEL_ID = 'PROSPECTIVE'` | `POOL.POOL_ID` | `R_CLASS_LEVEL`, `R_CLASS_SYSTEM` |
| Partner Consultation | `NOTIFICATION.NOTIF_ID`, `NOTIF_BA.BA_ID` (all WI partners) | `INT_SET_PARTNER.BA_ID`, `INTEREST_SET.INT_SET_ID` | `R_NOTIFICATION_TYPE`, `R_BA_ROLE_TYPE` |
| Internal Approval | `PROJECT_STEP_BA.STEP_ID`, `PROJECT_STEP_BA.BA_ID`, `PROJECT_STEP_BA.BA_ROLE = 'APPROVER'` | `BA_AUTHORITY.AUTH_TYPE = 'GATE_APPROVER'` | `R_BA_ROLE_TYPE` |
| Register Prospect | `POOL.POOL_STATUS = 'PROSPECT'`, `POOL_ALIAS.ALIAS_NAME`, `POOL_VERSION` update | — | `R_POOL_STATUS` |

### EXP-LICENSE-ACQUIRE — Exploration License

| Step | Write Columns | Read/Guard Columns | Regulatory Reference |
|---|---|---|---|
| Identify License Block | `LAND_RIGHT.LAND_RIGHT_ID`, `LAND_RIGHT.LAND_RIGHT_TYPE = 'EXPLORATION'`, `AREA.AREA_ID` | `AREA.COUNTRY_CODE` determines jurisdiction | — |
| Prepare Application | `RM_INFORMATION_ITEM.INFO_ITEM_SUBTYPE = 'LICENSE_APPLICATION'` | `BUSINESS_ASSOCIATE.BA_TYPE = 'OPERATOR'` | BLM 43 CFR §3100; AER D-056 §2 |
| Submit to Regulator | `BA_LICENSE.LICENSE_ID`, `BA_LICENSE.LICENSE_STATUS = 'APPLIED'`, `NOTIFICATION.NOTIF_ID` | — | BLM ePlanning; AER OneStop |
| Regulator Review | `BA_LICENSE.LICENSE_STATUS = 'UNDER_REVIEW'` | `OBLIGATION.OBLIG_TYPE = 'LICENSE_CONDITION'` | — |
| Receive Grant | `BA_LICENSE.LICENSE_STATUS = 'ACTIVE'`, `BA_LICENSE.EFFECTIVE_DATE`, `BA_LICENSE.EXPIRY_DATE` | — | BLM License issuance; AER License |
| Activate License | `LAND_RIGHT.LAND_RIGHT_STATUS = 'ACTIVE'`, `INTEREST_SET` partner notifications | — | JOA/CAPL notification obligation |

---

## Category 2: Development

### DEV-POOL-DEFINE — Field Development Plan

| Step | Write Columns | Read/Guard Columns | Reference |
|---|---|---|---|
| Confirm Pool Boundaries | `POOL.POOL_ID`, `POOL_AREA.AREA_ID`, `POOL.POOL_STATUS = 'DEVELOPMENT'` | `POOL_VERSION.GROSS_RESOURCE_VOL` baseline | SPE PRMS §5 |
| Prepare FDP Document | `RM_INFORMATION_ITEM.INFO_ITEM_SUBTYPE = 'FIELD_DEVELOPMENT_PLAN'`, `PROJECT.PROJECT_TYPE = 'FDP'` | — | ISO 17779 |
| Engineering Basis | `PROJECT_PLAN.PLAN_TYPE`, `PROJECT_STEP` (phases) | `FACILITY.FACILITY_TYPE` availability | IPA FEL-1 |
| Economic Evaluation | `FINANCE.FINANCE_TYPE = 'CAPEX_ESTIMATE'`, `FINANCE.NET_PRESENT_VALUE` | — | SEC Rule S-X; NI 51-101; DCF |
| Regulatory Submission | `NOTIFICATION.NOTIF_ID`, `BA_PERMIT.PERMIT_TYPE = 'FDP_APPROVAL'` | `COUNTRY_CODE` → jurisdiction routing | BSEE DPP; AER FDP |
| Partner Approval | `PROJECT_STEP_BA` (all WI partners), `NOTIFICATION` to each | `INT_SET_PARTNER.WORKING_INTEREST_PERC` | CAPL JOA Art. VII |
| FDP Gate Review | Links to `GATE-3-FID` process instance | `GATE_REVIEW` status = `APPROVED` | IPA FEL-2 |
| Sanction | `PROJECT.PROJECT_STATUS = 'SANCTIONED'`, `FINANCE.FINANCE_STATUS = 'APPROVED'` | — | SEC R4-1 proved reserves |

---

## Category 3: Production

### PRD-PRODUCTION-RPT — Monthly Production Reporting

| Step | Write Columns | Read/Guard Columns | Jurisdiction Variation | Reference |
|---|---|---|---|---|
| Aggregate Volumes | `PDEN_VOL_SUMMARY.VOL_OBS_NO`, `PDEN_VOL_SUMMARY.PRODUCT_TYPE`, `PDEN_VOL_SUMMARY.VOLUME` | `PDEN_ALLOC_FACTOR.ALLOC_FACTOR` all strings | All | EIA-914 §3 |
| Validate Measurement | `PROJECT_STEP_CONDITION.COND_STATUS = 'SATISFIED'` | `INSTRUMENT.CALIBRATION_DATE <= today` | USA: API MPMS; Canada: AER D-058 | IOGP Report 456 |
| Disposition Breakdown | `PDEN_VOL_DISPOSITION.DISPOSITION_TYPE` (sales, flare, shrinkage) | `SUBSTANCE_TYPE` per product | USA: ONRR SPE-93; Canada: AER ST-39 | EIA-914 |
| Submit to Regulator | `OBLIGATION.OBLIG_STATUS = 'SUBMITTED'`, `NOTIFICATION.NOTIF_ID` | `OBLIG_TYPE = 'PROD_REPORT'` | USA: OGOR/EIA; Canada: AER portal | EIA Form 914 |
| Regulator Confirmation | `OBLIGATION.OBLIG_STATUS = 'FULFILLED'`, `OBLIG_PAYMENT.PAYMENT_DATE` (if royalty attached) | — | — | ONRR; AER |

---

## Category 4: Decommissioning

### DEC-PA-EXEC — P&A Execution

| Step | Write Columns | Read/Guard Columns | Reference |
|---|---|---|---|
| Prepare P&A Programme | `RM_INFORMATION_ITEM.INFO_ITEM_SUBTYPE = 'PA_PROGRAM'`, `PROJECT_STEP` (P&A operations) | `WELL.WELL_STATUS` = `SUSPENDED` or `SHUT-IN` | API RP 100-2 §4; NORSOK D-010 |
| Obtain Regulatory Approval | `BA_PERMIT.PERMIT_TYPE = 'PA_PERMIT'`, `BA_PERMIT.PERMIT_STATUS = 'APPLIED'` | `COUNTRY_CODE` → routing | USA: BSEE Form 124; CA: AER Form 6 |
| Mobilize Well Services | `PROJECT_STEP_BA.BA_ID` (contractor), `EQUIPMENT.EQUIPMENT_ID` (rig) | `BA_LICENSE.LICENSE_STATUS = 'ACTIVE'` (contractor) | IOGP S-501 §3 |
| Execute P&A Operations | `WELL.WELL_STATUS = 'ABANDONMENT'`, `CASING_PROGRAM` update, `PROD_STRING.STRING_STATUS = 'ABANDONED'` | All `PROJECT_STEP_CONDITION` = `SATISFIED` at completion | API RP 100-2 §6; BSEE 30 CFR 250.1703 |
| Restore Location | `HSE_INCIDENT` (any spills during ops), `CONSENT` (site restoration) | `OBLIGATION.OBLIG_TYPE = 'SITE_RESTORE'` | EPA RCRA; ECCC CEPA §§95-99 |
| Final Certification | `WELL.WELL_STATUS = 'ABANDONED'`, `BA_PERMIT.PERMIT_STATUS = 'CLOSED'`, `NOTIFICATION` to regulator | — | API RP 100-2 §8 |
| Update Records | `RM_INFORMATION_ITEM.INFO_ITEM_SUBTYPE = 'PA_RECORD'`, `PPDM_AUDIT_HISTORY` final entry | — | PPDM records management |

---

## Category 5: Work Orders

### WO-TURNAROUND — Turnaround Work Order

| Step | Write Columns | Read/Guard Columns | Reference |
|---|---|---|---|
| Scope Definition | `PROJECT.PROJECT_TYPE = 'TURNAROUND'`, `PROJECT_PLAN.PLAN_TYPE = 'TA_WORK_LIST'` | `FACILITY.FACILITY_STATUS` = `OPERATIONAL` | SMRP MWP |
| Shutdown Authorization | `FACILITY_STATUS.STATUS_TYPE = 'SHUTDOWN'`, `NOTIFICATION` issued to all stakeholders | `HSE_INCIDENT` count last 30 days (inform only) | OSHA 1910.119 shutdown procedure |
| Equipment Isolation | `EQUIPMENT.EQUIPMENT_STATUS = 'ISOLATED'`, `BA_PERMIT.PERMIT_TYPE = 'PTW'` (Permit to Work) | All `PTW` permits confirmed active | API RP 505; IEC 60079 |
| Execute Maintenance | `EQUIPMENT_MAINTAIN.MAINT_TYPE`, `FACILITY_MAINTAIN.MAINT_ACTIVITY`, `PROJECT_STEP_CONDITION` per WO item | `EQUIPMENT_MAINT_STATUS` tracking | ISO 55001 §8.4 |
| Inspection | `PROJECT_STEP_CONDITION.COND_STATUS = 'SATISFIED'` per stream, `RM_INFORMATION_ITEM` (inspection certs) | All conditions satisfied before restart approval | BSEE SEMS §250.1928 |
| Restart | `FACILITY_STATUS.STATUS_TYPE = 'OPERATIONAL'`, `PROD_STRING.STRING_STATUS = 'PRODUCING'` | PSSR certificate issued; `GATE-5-OPS` approved | IEC 61511 PSSR |
| Cost Reconciliation | `FINANCE.FINANCE_TYPE = 'TA_COST'`, `FIN_COMPONENT` per cost code | `AFE` variance within approved tolerance | COPAS PGAS §500 |
| Close-out | `PROJECT.PROJECT_STATUS = 'COMPLETED'`, lessons-learned in `RM_INFORMATION_ITEM` | — | ISO 55001 §9.1 |

---

## Category 6: Gate Reviews

### GATE-3-FID — Final Investment Decision

| Step | Write Columns | Read/Guard Columns | Required Document Type | Approver Count |
|---|---|---|---|---|
| Prepare Gate Pack | `RM_INFORMATION_ITEM` — 4 required docs | See checklist in SM doc | `FEED_PACKAGE`, `COST_ESTIMATE_CLASS3`, `EIA_REPORT`, `AFE_FORM` | — |
| Circulate to Approvers | `NOTIFICATION.NOTIF_ID`, `NOTIF_BA.BA_ID` for each approver | `PROJECT_STEP_BA` reviewer list set | — | — |
| Technical Review | `PROJECT_STEP.STEP_STATUS = 'UNDER_REVIEW'` | `RM_INFORMATION_ITEM` documents complete (guard) | — | — |
| Commercial Review | `PROJECT_STEP.STEP_STATUS = 'UNDER_REVIEW'` | `FINANCE.NET_PRESENT_VALUE` positive (inform) | — | — |
| Board / Management Decision | `PROJECT_STEP_BA.DECISION = 'APPROVE'` for ≥2 approvers | `BA_AUTHORITY.AUTH_TYPE = 'GATE_APPROVER'` | — | min 2 |
| Issue Decision | `PROJECT_STATUS.STATUS_TYPE = 'SANCTIONED'`, `NOTIFICATION` to WI partners, `POOL_VERSION.CLASS_LEVEL = 'PROVED'` | — | — | — |

---

## Category 7: HSE

### HSE-INCIDENT-T12 — Tier 1/2 Incident

| Step | Write Columns | Read/Guard Columns | Reference |
|---|---|---|---|
| Report Incident | `HSE_INCIDENT.INCIDENT_ID`, `HSE_INCIDENT.INCIDENT_DATE`, `HSE_INCIDENT.INCIDENT_TIER = '1' or '2'`, `HSE_INCIDENT.LOCATION`, `HSE_INCIDENT_BA.BA_ID` (injured persons) | `FACILITY.FACILITY_ID` must exist | API RP 754 §3; OSHA 300 |
| Assign Investigator | `HSE_INCIDENT_BA.BA_ROLE = 'INVESTIGATOR'`, `HSE_INCIDENT.INVESTIGATOR_BA_ID` | `BA.BA_TYPE` must be employee or contractor on site | IOGP 2022e §2.4 |
| Investigate | `HSE_INCIDENT_DETAIL.DETAIL_TYPE = 'TIMELINE'`, `HSE_INCIDENT_DETAIL.DETAIL_TEXT` | — | IOGP 2022e §4 |
| Root Cause Analysis | `HSE_INCIDENT_CAUSE.IMMEDIATE_CAUSE`, `HSE_INCIDENT_CAUSE.ROOT_CAUSE`, `HSE_INCIDENT_CAUSE.CONTRIBUTING_FACTOR` | Text fields must be non-empty (guard) | API RP 754 §4; UK HSE RIDDOR |
| Raise Corrective Actions | `PROJECT_STEP.STEP_NAME` (action description), `PROJECT_STEP.RESP_BA_ID`, `PROJECT_STEP.DUE_DATE` | ≥1 corrective action created (guard) | IOGP 2022e §5 |
| Track Actions | `PROJECT_STEP.STEP_STATUS` updates | All actions `COMPLETED` before close (guard) | BSEE SEMS §250.1932 |
| Close Incident | `HSE_INCIDENT.CLOSE_DATE = NOW()`, `HSE_INCIDENT.CLOSE_USER`, `RM_INFORMATION_ITEM` (final report) | Tier 1: Close by `SafetyOfficer` AND `Manager`; lessons-learned text required | IOGP 2022e §6 |

---

## Category 8: Compliance

### COMPL-GHG-REPORT — GHG Reporting

| Step | Write Columns | Read/Guard Columns | Jurisdiction Formula | Reference |
|---|---|---|---|---|
| Calculate Combustion Emissions | `PDEN_VOL_SUMMARY.SUBSTANCE_TYPE = 'CO2_COMBUSTION'`, volume in tonnes CO2-eq | `PDEN_VOL_SUMMARY.PRODUCT_TYPE = 'GAS'` volumes, fuel gas volumes | USA: 40 CFR 98 App. A EF×HHV; Canada: NIR Protocol | EPA 40 CFR 98 |
| Calculate Fugitive Emissions | `PDEN_VOL_SUMMARY.SUBSTANCE_TYPE = 'CH4_FUGITIVE'` | `EQUIPMENT` count by type (EPA component count method) | USA: 40 CFR 98 Subpart W Tab W-1A; Canada: ECCC Method 21 | EPA 40 CFR 98.233 |
| Calculate Flaring Emissions | `PDEN_VOL_SUMMARY.SUBSTANCE_TYPE = 'CO2_FLARING'` | `PDEN_VOL_DISPOSITION.DISPOSITION_TYPE = 'FLARE'` | All: CO2 = flare_vol×EF_flare | IOGP Report 455 |
| Quality Assurance | `PROJECT_STEP_CONDITION.COND_STATUS` per QA check | Calculated values cross-checked against prior year ±30% (inform if flagged) | — | EPA 40 CFR 98 §98.3(c) |
| Submit Report | `OBLIGATION.OBLIG_STATUS = 'SUBMITTED'`, `NOTIFICATION` | `OBLIGATION.DUE_DATE` not passed (guard) | USA: EPA e-GGRT portal; Canada: ECCC portal; EU: EU ETS registry | 40 CFR 98 §98.3(b) |

---

## Category 9: Well Lifecycle

### WELL-DRILL-OPS — Drilling Operations

| Step | Write Columns | Read/Guard Columns | Reference |
|---|---|---|---|
| Spud | `WELL.WELL_STATUS = 'DRILLING'`, `WELL.SPUD_DATE = NOW()` | `BA_PERMIT.PERMIT_STATUS = 'ACTIVE'` (drill permit) | BSEE Form 125; AER D-056 |
| Run Casing | `CASING_PROGRAM.CASING_SET`, `CASING_PROGRAM.CASING_TYPE`, `CASING_PROGRAM.TOP_DEPTH`, `CASING_PROGRAM.BOTTOM_DEPTH` | `WELL.CURRENT_DEPTH` ≥ `CASING_PROGRAM.BOTTOM_DEPTH` | API Spec 5CT; ISO 11960 |
| Cement | `CASING_PROGRAM.CEMENT_TYPE`, `CASING_PROGRAM.TOP_OF_CEMENT` | `PPDM_AUDIT_HISTORY` — cement test result | API RP 10 Series |
| Log & Core | `RM_INFORMATION_ITEM.INFO_ITEM_SUBTYPE = 'LAS_LOG'`, `SEIS_SURVEY` (if applicable) | — | SPWLA petrophysics |
| TD Decision | `WELL.TD_DEPTH`, `WELL.WELL_STATUS = 'TD_REACHED'` | `WELL.PLANNED_TD_DEPTH` reached (inform if different) | — |
| Move / Rig Release | `WELL.WELL_STATUS = 'COMPLETION'` or `'SUSPENDED'`, rig release notification `NOTIFICATION` | All casing programs cemented (guard) | BSEE Form 131 |

---

## Category 10: Facility Lifecycle

### FAC-PSSR — Pre-Start-up Safety Review

| Step | Write Columns | Read/Guard Columns | Reference |
|---|---|---|---|
| Scope PSSR | `PROJECT_STEP.STEP_NAME = 'PSSR'`, `PROJECT_STEP.RESPONSIBLE_BA_ID` | All MOC actions from `FAC-MOC` are `COMPLETED` | IEC 61511 §5.3 |
| Review P&IDs | `PROJECT_STEP_CONDITION.COND_NAME = 'PID_REVIEWED'`, `COND_STATUS` | `RM_INFORMATION_ITEM.INFO_ITEM_SUBTYPE = 'PID_AS_BUILT'` exists | IEC 61882 |
| Check Instruments / SIS | `INSTRUMENT.CALIBRATION_DATE`, `INSTRUMENT.INSTRUMENT_STATUS = 'CALIBRATED'` | All `SIS_FUNCTION` instruments calibrated within schedule | IEC 61511 SIL verification |
| Review Operating Procedures | `PROJECT_STEP_CONDITION.COND_NAME = 'SOP_REVIEWED'` | `RM_INFORMATION_ITEM.INFO_ITEM_SUBTYPE = 'OPERATING_PROCEDURE'` rev-controlled | ISO 20815 §7 |
| Issue PSSR Certificate | `RM_INFORMATION_ITEM.INFO_ITEM_SUBTYPE = 'PSSR_CERTIFICATE'`, `FACILITY.FACILITY_STATUS = 'PSSR_COMPLETE'` | All PSSR `PROJECT_STEP_CONDITION` rows `SATISFIED` (hard guard) | IOGP RP 70 §5 |

---

## Category 11: Reservoir Management

### RES-RESERVES-CERT — Reserves Certification

| Step | Write Columns | Read/Guard Columns | Jurisdiction Standard |
|---|---|---|---|
| Prepare Reserves Summary | `POOL_VERSION.POOL_OBS_NO` new revision, `PDEN_VOL_SUMMARY.VOLUME` per category | Prior year `POOL_VERSION` for comparison | SPE PRMS 2018 §3 |
| Internal Technical Review | `PROJECT_STEP_CONDITION.COND_STATUS = 'SATISFIED'` per reviewer | Reviewer list in `PROJECT_STEP_BA` | SPE PRMS §5 |
| External Auditor / QRI Review | `PROJECT_STEP_BA.BA_ROLE = 'QUALIFIED_RESERVES_EVALUATOR'`, `RM_INFORMATION_ITEM` (auditor report) | `BA.BA_TYPE = 'RESERVES_AUDITOR'` | NI 51-101 F-2; SEC Reg S-X |
| Reconcile to Prior Year | `PDEN_VOL_SUMMARY` variance calculation, written as `AUDIT_HISTORY` note | Variance > 20% triggers additional QA step | SEC Reg S-K Item 102 |
| Board Approval | `PROJECT_STEP_BA.DECISION = 'APPROVE'` (Board Director), `POOL_VERSION.CERTIFIED_IND = 'Y'` | ≥1 Board Director approver | NI 51-101 §2.1; SEC CEA rule |
| Filing | `OBLIGATION.OBLIG_TYPE = 'RESERVES_FILING'`, `OBLIGATION.OBLIG_STATUS = 'SUBMITTED'`, `NOTIFICATION` | `OBLIGATION.DUE_DATE` met | NI 51-101 annual AIF; SEC 10-K |

---

## Category 12: Pipeline

### PIPE-INTEGRITY — Integrity Assessment

| Step | Write Columns | Read/Guard Columns | Reference |
|---|---|---|---|
| Risk Assessment | `FACILITY_MAINTAIN.MAINT_TYPE = 'INTEGRITY_ASSESSMENT'`, threat identification text | `PIPE_STRING.PIPE_TYPE`, `PIPE_STRING.YEAR_INSTALLED` | ASME B31.8S §4 |
| Select Assessment Method | `PROJECT_STEP_CONDITION.COND_NAME = 'METHOD_SELECTED'` (ILI/DA/pressure test) | `PIPE_STRING.DIAMETER`, wall thickness available | ASME B31.8S §6 |
| Execute ILI / Direct Assessment | `FACILITY_MAINTAIN.MAINT_RESULT`, `PROJECT_STEP_CONDITION` per anomaly found | — | NACE SP0102; ASME B31.8S |
| Evaluate Anomalies | `EQUIPMENT_MAINTAIN.ANOMALY_CODE`, `EQUIPMENT_MAINTAIN.ANOMALY_SEVERITY` | Severity > threshold triggers immediate repair WO | ASME B31.8G |
| Remediation (if required) | Links new `WO-CORRECTIVE` instance for each critical anomaly | — | ASME B31.8S §7 |
| Reassessment Interval | `FACILITY_MAINTAIN.NEXT_MAINT_DATE` calculated per ASME B31.8S §7.4 | — | ASME B31.8S §7.4 |
