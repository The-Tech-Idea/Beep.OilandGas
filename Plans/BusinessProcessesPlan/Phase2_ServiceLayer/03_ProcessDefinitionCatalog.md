# Phase 2 — Process Definition Catalog
## All 96 Business Process Definitions

> All definitions are seeded by `ProcessDefinitionInitializer` at startup.  
> Each definition maps to a leaf node in `BusinessProcessCategoryNode`.  
> `ProcessId` values are stable identifiers; never change them once seeded.

---

## Category 1: Exploration Workflows (`ProcessCategory = "1"`)

| ProcessId | ProcessName | Steps | JurisdictionTags | Primary Standard |
|---|---|---|---|---|
| `EXP-LEAD-ASSESS` | Lead-to-Prospect Assessment | 6 | USA, CANADA, INTERNATIONAL | SPE PRMS §2 |
| `EXP-PROSPECT-DISC` | Prospect-to-Discovery Evaluation | 7 | USA, CANADA, INTERNATIONAL | SPE PRMS §3 |
| `EXP-EXPL-WELL-AUTH` | Exploration Well Authorization | 5 | USA, CANADA, INTERNATIONAL | BSEE 30 CFR 250; AER D-056 |
| `EXP-GG-DATA-ACQUIRE` | G&G Data Acquisition | 5 | USA, CANADA, INTERNATIONAL | PPDM Seismic Category |
| `EXP-LICENSE-ACQUIRE` | Exploration License Acquisition | 6 | USA, CANADA, INTERNATIONAL | BLM/OCS; AER; C-NLOPB |
| `EXP-FARMOUT` | Farmout / Farm-in Agreement | 5 | USA, CANADA, INTERNATIONAL | CAPL; AAPL |
| `EXP-WELL-RESULTS` | Exploration Well Results Processing | 4 | USA, CANADA, INTERNATIONAL | SPE PRMS §3 |
| `EXP-PROSPECT-APPROVE` | Prospect Investment Approval | 5 | USA, CANADA, INTERNATIONAL | SPE Stage-Gate; IPA FEL-0 |

### `EXP-LEAD-ASSESS` Step Detail

| StepId | StepName | Seq | Transitions | Primary Table | Secondary Tables | Regulatory Ref |
|---|---|---|---|---|---|---|
| `EXP-LA-01` | Create Lead Record | 1 | `capture_gg` | `POOL` | `AREA`, `FIELD` | SPE PRMS §2.1 |
| `EXP-LA-02` | Capture G&G Data | 2 | `assess_risk`, `reject` | `RM_INFORMATION_ITEM` | `SEIS_SURVEY`, `SEIS_3D` | PPDM Seismic |
| `EXP-LA-03` | Risk & Resource Assessment | 3 | `approve`, `reject` | `POOL_VERSION` | `CLASS_LEVEL`, `CLASS_SYSTEM` | SPEE Risked Resources |
| `EXP-LA-04` | Partner Consultation | 4 | `submit_for_approval`, `reject` | `INTEREST_SET` | `INT_SET_PARTNER`, `BA_ROLE_TYPE` | JOA / CAPL |
| `EXP-LA-05` | Internal Approval | 5 | `upgrade_to_prospect`, `reject` | `PROJECT_STEP_BA` | `BA_AUTHORITY` | SPE PRMS §2.2 |
| `EXP-LA-06` | Register as Prospect | 6 | — | `POOL`, `POOL_ALIAS` | `POOL_VERSION`, `CLASS_LEVEL` | SPE PRMS §3 |

---

## Category 2: Development Workflows (`ProcessCategory = "2"`)

| ProcessId | ProcessName | Steps | JurisdictionTags | Primary Standard |
|---|---|---|---|---|
| `DEV-POOL-DEFINE` | Pool Definition & Field Development Plan | 8 | USA, CANADA, INTERNATIONAL | ISO 17779; SPE PRMS §5 |
| `DEV-FACILITY-CONCEPT` | Facility Concept Selection | 6 | USA, CANADA, INTERNATIONAL | IPA FEL-0; ISO 15663 |
| `DEV-WELL-ENGINEERING` | Development Well Engineering | 7 | USA, CANADA, INTERNATIONAL | API RP 100-1; ISO 16530-2 |
| `DEV-CONSTRUCTION-MGMT` | Construction Management | 6 | USA, CANADA, INTERNATIONAL | IOGP S-501 |
| `DEV-PIPELINE-DESIGN` | Pipeline Design & Routing | 6 | USA, CANADA, INTERNATIONAL | ASME B31.8; CSA Z662 |
| `DEV-AFE-APPROVAL` | AFE Approval Process | 5 | USA, CANADA, INTERNATIONAL | CAPL JOA Art. IX; COPAS |
| `DEV-PARTNER-CONSENT` | Working Interest Partner Consent | 4 | USA, CANADA, INTERNATIONAL | CAPL / AAPL 610 |
| `DEV-PSSR` | Pre-Start-up Safety Review (PSSR) | 5 | USA, CANADA, INTERNATIONAL | IEC 61511 §5.3; IOGP RP 70 |

### `DEV-AFE-APPROVAL` Step Detail

| StepId | StepName | Seq | Transitions | Primary Table | Secondary Tables | Regulatory Ref | Doc Required |
|---|---|---|---|---|---|---|---|
| `DEV-AFE-01` | Prepare AFE Package | 1 | `circulate` | `FINANCE` | `FIN_COMPONENT` | COPAS PGAS | Yes (`AFE_FORM`) |
| `DEV-AFE-02` | Circulate to WI Partners | 2 | `partners_responded`, `withdraw` | `NOTIF_BA` | `NOTIFICATION`, `INT_SET_PARTNER` | CAPL Art. IX §2 | No |
| `DEV-AFE-03` | Tally Partner Approvals | 3 | `approved`, `rejected` | `PROJECT_STEP_BA` | `BA_AUTHORITY` | CAPL Art. IX §3 | Yes (`PARTNER_APPROVAL_LETTERS`) |
| `DEV-AFE-04` | Issue AFE Authorization | 4 | `fund_commitments` | `FINANCE` | `CONTRACT` | COPAS §600 | No |
| `DEV-AFE-05` | Commit Funds | 5 | — | `FINANCE` | `FIN_COST_SUMMARY` | SEC 10-K CAPEX | No |

---

## Category 3: Production Workflows (`ProcessCategory = "3"`)

| ProcessId | ProcessName | Steps | JurisdictionTags | Primary Standard |
|---|---|---|---|---|
| `PRD-WELL-STARTUP` | Well Start-up Procedure | 6 | USA, CANADA, INTERNATIONAL | API RP 19D; BSEE SEMS |
| `PRD-ALLOCATION` | Production Allocation & Measurement | 5 | USA, CANADA, INTERNATIONAL | EIA Form 914; NEB/CER |
| `PRD-WORKOVER-AUTH` | Workover Authorization | 5 | USA, CANADA, INTERNATIONAL | API RP 100-1 §10; AER D-036 |
| `PRD-DECLINE-MGMT` | Decline Management & DCA | 5 | USA, CANADA, INTERNATIONAL | SEC proved reserves DCA |
| `PRD-EMERGENCY-SD` | Emergency Shutdown Response | 4 | USA, CANADA, INTERNATIONAL | OSHA PSM; IOGP 456 |
| `PRD-PRODUCTION-RPT` | Monthly Production Reporting | 5 | USA, CANADA, INTERNATIONAL | EIA OGOR; AER ST-39 |
| `PRD-SURVEILLANCE` | Well Surveillance Program | 4 | USA, CANADA, INTERNATIONAL | API RP 90-2 |
| `PRD-FIELD-OPT` | Field Optimization Review | 5 | USA, CANADA, INTERNATIONAL | SPE PRMS §5 |

---

## Category 4: Decommissioning Workflows (`ProcessCategory = "4"`)

| ProcessId | ProcessName | Steps | JurisdictionTags | Primary Standard |
|---|---|---|---|---|
| `DEC-PA-PLAN` | Plug & Abandonment Planning | 6 | USA, CANADA, INTERNATIONAL | API RP 100-2; NORSOK D-010 |
| `DEC-PA-EXEC` | Plug & Abandonment Execution | 7 | USA, CANADA, INTERNATIONAL | BSEE NTL 2010-G05; AER D-020 |
| `DEC-FACILITY-DECOM` | Facility Decommissioning | 7 | USA, CANADA, INTERNATIONAL | OSPAR Dec. 98/3; IMO |
| `DEC-ENV-CLOSURE` | Environmental Closure & Remediation | 6 | USA, CANADA, INTERNATIONAL | EPA RCRA; ECCC CEPA |
| `DEC-COST-ACCOUNTING` | ARO Cost Accounting | 4 | USA, CANADA, INTERNATIONAL | FASB ASC 410-20; IFRS 16 |
| `DEC-REG-NOTIFICATION` | Regulatory Decommission Notification | 5 | USA, CANADA, INTERNATIONAL | BSEE Form 124; AER Form 6 |

---

## Category 5: Work Order Workflows (`ProcessCategory = "5"`)

| ProcessId | ProcessName | Steps | JurisdictionTags | Primary Standard |
|---|---|---|---|---|
| `WO-PREVENTIVE` | Preventive Maintenance Work Order | 6 | USA, CANADA, INTERNATIONAL | ISO 55001; RCM |
| `WO-CORRECTIVE` | Corrective Maintenance Work Order | 6 | USA, CANADA, INTERNATIONAL | ISO 14224; SAP PM |
| `WO-INSPECTION` | Inspection Work Order | 5 | USA, CANADA, INTERNATIONAL | BSEE SEMS §250.1920 |
| `WO-TURNAROUND` | Turnaround / Shutdown Work Order | 8 | USA, CANADA, INTERNATIONAL | IOGP S-501; ISO 55001 |
| `WO-EMERGENCY` | Emergency Work Order | 4 | USA, CANADA, INTERNATIONAL | API RP 1161; IOGP S-509 |
| `WO-CAPITAL` | Capital Work Order | 7 | USA, CANADA, INTERNATIONAL | COPAS; CAPL JOA Art. IX |

---

## Category 6: Approval & Gate Reviews (`ProcessCategory = "6"`)

| ProcessId | ProcessName | Steps | MinApprovers | JurisdictionTags |
|---|---|---|---|---|
| `GATE-0-OPP` | Gate 0 — Opportunity Identification | 4 | 1 | USA, CANADA, INTERNATIONAL |
| `GATE-1-CONCEPT` | Gate 1 — Concept Selection | 5 | 1 | USA, CANADA, INTERNATIONAL |
| `GATE-2-PREFEED` | Gate 2 — Pre-FEED Approval | 5 | 1 | USA, CANADA, INTERNATIONAL |
| `GATE-3-FID` | Gate 3 — Final Investment Decision (FID) | 6 | 2 | USA, CANADA, INTERNATIONAL |
| `GATE-4-EXEC` | Gate 4 — Execution Authorization | 5 | 1 | USA, CANADA, INTERNATIONAL |
| `GATE-5-OPS` | Gate 5 — Operations Readiness Review | 5 | 1 | USA, CANADA, INTERNATIONAL |
| `GATE-MOC` | Management of Change (MOC) | 5 | 1 | USA, CANADA, INTERNATIONAL |
| `GATE-AFE` | AFE Partner Approval | 4 | 1 | USA, CANADA, INTERNATIONAL |

---

## Category 7: HSE & Safety Workflows (`ProcessCategory = "7"`)

| ProcessId | ProcessName | Steps | JurisdictionTags | Primary Standard |
|---|---|---|---|---|
| `HSE-INCIDENT-T12` | Tier 1/2 Incident Management | 6 | USA, CANADA, INTERNATIONAL | API RP 754; IOGP 2022e |
| `HSE-INCIDENT-T34` | Tier 3/4 Observation / Near-Miss | 3 | USA, CANADA, INTERNATIONAL | API RP 754 |
| `HSE-HAZOP` | HAZOP / HAZID Study | 5 | USA, CANADA, INTERNATIONAL | IEC 61882; IOGP S-509 |
| `HSE-RISK-ASSESS` | Formal Risk Assessment | 5 | USA, CANADA, INTERNATIONAL | ISO 31000; IOGP RP 1015 |
| `HSE-SIMOPS` | SIMOPS Coordination | 5 | USA, CANADA, INTERNATIONAL | IOGP RP 70; NORSOK Z-013 |
| `HSE-EMERGENCY-RESP` | Emergency Response Activation | 4 | USA, CANADA, INTERNATIONAL | OPA 90; CEPA; API RP 74 |
| `HSE-SAFETY-CASE` | Safety Case Preparation | 7 | UK, NORWAY, INTERNATIONAL | UK DCR; NORSOK Z-013 |
| `HSE-CORRECTIVE-TRACK` | Corrective Action Tracking | 4 | USA, CANADA, INTERNATIONAL | BSEE SEMS; CSA Z767 |

---

## Category 8: Compliance & Regulatory (`ProcessCategory = "8"`)

| ProcessId | ProcessName | Steps | JurisdictionTags | Regulatory Body |
|---|---|---|---|---|
| `COMPL-WELL-PERMIT-USA` | Well Permit Application (USA) | 6 | USA | BSEE; State OGRs |
| `COMPL-WELL-PERMIT-CAN` | Well Permit Application (Canada) | 6 | CANADA | AER; BCOGC; C-NLOPB |
| `COMPL-PROD-RPT-USA` | Production Report Submission (USA) | 5 | USA | EIA; ONRR; state OGRs |
| `COMPL-PROD-RPT-CAN` | Production Report Submission (Canada) | 5 | CANADA | AER ST-39; NEB/CER |
| `COMPL-ROYALTY-RPT` | Royalty Calculation & Reporting | 5 | USA, CANADA | ONRR; AER; Petoro (NO) |
| `COMPL-ENV-CONSENT` | Environmental Consent / Authorization | 6 | USA, CANADA, INTERNATIONAL | EPA; ECCC; OSPAR |
| `COMPL-GHG-REPORT` | GHG & Emissions Reporting | 5 | USA, CANADA, INTERNATIONAL | EPA 40 CFR 98; OBPS; EU ETS |
| `COMPL-EXPORT-CERT` | Export Certification | 4 | USA, CANADA, INTERNATIONAL | FERC NGA; NEB; GIIGNL |

---

## Category 9: Well Lifecycle Workflows (`ProcessCategory = "9"`)

| ProcessId | ProcessName | Steps | JurisdictionTags | Primary Standard |
|---|---|---|---|---|
| `WELL-DESIGN` | Well Design & Engineering | 5 | USA, CANADA, INTERNATIONAL | API RP 100-1; ISO 16530-2 |
| `WELL-SPUD-AUTH` | Spud Authorization | 4 | USA, CANADA, INTERNATIONAL | BSEE Form 123; AER D-056 |
| `WELL-DRILL-OPS` | Drilling Operations | 6 | USA, CANADA, INTERNATIONAL | API Spec 5CT; ISO 11960 |
| `WELL-COMPLETION` | Well Completion | 5 | USA, CANADA, INTERNATIONAL | API RP 10A; ISO 10426 |
| `WELL-TIE-IN` | Well Tie-in & Commissioning | 4 | USA, CANADA, INTERNATIONAL | API RP 500; NFPA 59A |
| `WELL-WORKOVER` | Workover Operations | 5 | USA, CANADA, INTERNATIONAL | API RP 100-1 §10; AER D-036 |
| `WELL-SUSPEND` | Well Suspension | 4 | USA, CANADA, INTERNATIONAL | BSEE NTL 2010-G05 |
| `WELL-PA` | Plug & Abandonment | 7 | USA, CANADA, INTERNATIONAL | API RP 100-2; NORSOK D-010 |
| `WELL-REACTIVATE` | Well Reactivation | 4 | USA, CANADA, INTERNATIONAL | API RP 100-1; AER D-020 |

---

## Category 10: Facility Lifecycle Workflows (`ProcessCategory = "10"`)

| ProcessId | ProcessName | Steps | JurisdictionTags | Primary Standard |
|---|---|---|---|---|
| `FAC-CONCEPT` | Facility Concept & Screening | 5 | USA, CANADA, INTERNATIONAL | IPA FEL-0; ISO 15663 |
| `FAC-FEED` | FEED & Detailed Engineering | 6 | USA, CANADA, INTERNATIONAL | IPA FEL-2; ISO 15663 |
| `FAC-PROCUREMENT` | Equipment Procurement | 5 | USA, CANADA, INTERNATIONAL | IOGP S-501 |
| `FAC-PSSR` | Pre-Start-up Safety Review | 5 | USA, CANADA, INTERNATIONAL | IEC 61511 §5.3; IOGP RP 70 |
| `FAC-COMMISSION` | Commissioning | 6 | USA, CANADA, INTERNATIONAL | ISO 20815 |
| `FAC-OPERATIONS` | Operations & Maintenance | 5 | USA, CANADA, INTERNATIONAL | ISO 55001; API RP 580 |
| `FAC-MOC` | Management of Change (MOC) | 5 | USA, CANADA, INTERNATIONAL | OSHA 1910.119(l); IOGP 423 |
| `FAC-DECOM` | Facility Decommissioning | 7 | USA, CANADA, INTERNATIONAL | OSPAR; ISO 13702 |

---

## Category 11: Reservoir Management Workflows (`ProcessCategory = "11"`)

| ProcessId | ProcessName | Steps | JurisdictionTags | Primary Standard |
|---|---|---|---|---|
| `RES-ANNUAL-REVIEW` | Annual Reservoir Performance Review | 6 | USA, CANADA, INTERNATIONAL | SPE PRMS §5.3 |
| `RES-HISTORY-MATCH` | History Match & Simulation Update | 5 | USA, CANADA, INTERNATIONAL | SPE-197049 |
| `RES-RESERVES-CERT` | Reserves Certification | 6 | USA, CANADA, INTERNATIONAL | NI 51-101; SEC 17 CFR §229 |
| `RES-IOR-SCREEN` | IOR/EOR Screening | 5 | USA, CANADA, INTERNATIONAL | API RP 19D; SPE PRMS §5.3.7 |
| `RES-PRESSURE-MAINT` | Pressure Maintenance Programme | 5 | USA, CANADA, INTERNATIONAL | SPE 23311 |
| `RES-INFILL-DRILL` | Infill Drilling Decision | 5 | USA, CANADA, INTERNATIONAL | SPE PRMS Proved Undeveloped |
| `RES-POOL-RECLASS` | Pool Reclassification | 4 | USA, CANADA, INTERNATIONAL | PRMS §3; NI 51-101 |
| `RES-EOR-SANCTION` | EOR Project Sanction | 6 | USA, CANADA, INTERNATIONAL | SPE PRMS §5.3.7; Gate 3 |

---

## Category 12: Pipeline & Infrastructure Workflows (`ProcessCategory = "12"`)

| ProcessId | ProcessName | Steps | JurisdictionTags | Primary Standard |
|---|---|---|---|---|
| `PIPE-INTEGRITY` | Pipeline Integrity Assessment | 6 | USA, CANADA, INTERNATIONAL | ASME B31.8S; CSA Z662 |
| `PIPE-PIGGING` | Intelligent Pigging Campaign | 5 | USA, CANADA, INTERNATIONAL | NACE SP0169; IOGP S-509 |
| `PIPE-CORROSION` | Corrosion Management Programme | 5 | USA, CANADA, INTERNATIONAL | NACE MR0175; ISO 15156 |
| `PIPE-TIE-IN` | Pipeline Tie-in Authorization | 5 | USA, CANADA, INTERNATIONAL | FERC Form 2; NEB OPR-99 |
| `PIPE-PRESSURE-TEST` | Pressure Testing | 4 | USA, CANADA, INTERNATIONAL | ASME B31.8 §841.3; API RP 1110 |
| `PIPE-EXPAND` | Pipeline Expansion & Upgrade | 6 | USA, CANADA, INTERNATIONAL | FERC CEII; NEB/CER GH-001 |
| `PIPE-EMERGENCY` | Emergency Repair | 4 | USA, CANADA, INTERNATIONAL | API RP 1161; DOT PHMSA |
| `PIPE-ABANDON` | Pipeline Abandonment | 6 | USA, CANADA, INTERNATIONAL | FERC Order 7157; NEB OPR-99 §60 |

---

## ProcessId Stability Note

`ProcessId` values are the stable primary key for all process definitions. They:
- Are hardcoded in `ProcessDefinitionInitializer` and must never be changed post-deployment
- Map to `PROJECT.PROJECT_OBS_NO` via the `ProcessId` column  
- Are referenced by `BusinessProcessCategoryNode.GetProcessNamesForCategory()` through the `ProcessName` ↔ `ProcessId` lookup in `GetProcessDefinitionByNameAsync`
- Must be globally unique across all 12 categories
