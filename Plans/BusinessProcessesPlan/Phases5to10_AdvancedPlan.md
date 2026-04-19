# Phases 5–10 — Advanced Implementation Plan
## Business Process Branch — Detailed Implementation Plan

> **Status**: 🔲 Not Started  
> **Depends on**: Phases 2–4

---

# Phase 5 — Process Analytics & Reporting

> **Standards**: SPE PRMS §7 (reporting), API RP 97 metrics, IOGP KPI Report 2022e, NI 51-101 disclosures

## 5.1 — Objective

Provide field-level process performance analytics: KPI dashboards for cycle times,
gate pass rates, HSE tier distribution, compliance obligation fulfillment rates,
and reserves maturation velocity (Exploration → Production).

## 5.2 — PPDM Table Mapping

| Metric | Source PPDM Tables | Aggregation |
|---|---|---|
| Process cycle time | `PROJECT_STEP`, `PPDM_AUDIT_HISTORY` | `START_DATE` → `END_DATE` diff per step |
| Gate pass rate | `PROJECT_STATUS`, `PROJECT_STEP_BA` | `APPROVED / TOTAL` per gate type |
| HSE Tier distribution | `HSE_INCIDENT`, `HSE_INCIDENT_SEVERITY` | Count by `INCIDENT_TYPE` + `SEVERITY_CODE` |
| Compliance on-time rate | `OBLIGATION`, `OBLIG_PAYMENT` | `DUE_DATE < ACTUAL_DATE` ratio |
| Reserves maturation | `POOL_VERSION`, `PDEN_VOL_SUMMARY` | Volume movement from Prospective → Contingent → Proved |
| Work order backlog | `PROJECT_STEP` (WO category) | Count by `PROJECT_STEP_STATUS` |
| Emergency response time | `HSE_INCIDENT`, `HSE_INCIDENT_COMPONENT` | Time from `REPORTED_DATE` → first action |
| Well stock turnover | `WELL_STATUS`, `PROD_STR_STAT_HIST` | Spud to first-production duration |

## 5.3 — Responsibility Matrix (RACI)

| Task | Analytics Dev | Data Architect | Backend Dev | QA | Domain SME |
|---|:---:|:---:|:---:|:---:|:---:|
| 5.1 — KPI query service | R | C | C | C | A |
| 5.2 — Analytics API endpoints | R | I | A | C | I |
| 5.3 — Dashboard page | R | I | C | R(QA) | C |
| 5.4 — CSV / Excel export | R/A | I | C | C | I |
| 5.5 — NI 51-101 summary report | C | C | R/A | C | A |

## 5.4 — Todo Tracker

| ID | Task | Priority | Status | Notes |
|---|---|---|---|---|
| 5.1 | Create `IProcessAnalyticsService` interface | HIGH | 🔲 | Methods: `GetCycleTimesAsync`, `GetGatePassRateAsync`, `GetHSETierDistributionAsync` |
| 5.2 | Implement `PPDMProcessAnalyticsService` using `PPDMGenericRepository` | HIGH | 🔲 | Complex aggregations via `RunQuery` |
| 5.3 | Expose `/api/field/current/process/analytics` GET endpoints | MEDIUM | 🔲 | Query params: `fromDate`, `toDate`, `categoryId` |
| 5.4 | Create `ProcessAnalyticsDashboard.razor` page | MEDIUM | 🔲 | MudChart line + pie charts |
| 5.5 | NI 51-101 / SEC 17 CFR 229 reserves maturation report page | HIGH | 🔲 | Export to PDF (wkhtmltopdf already in solution) |
| 5.6 | IOGP KPI benchmark comparison table | LOW | 🔲 | Compare field KPIs vs IOGP industry median |

---

# Phase 6 — Work Order Engine (ISO 55001 / SAP PM)

> **Standards**: ISO 55001 (Asset Management), ISO 14224 (Reliability), IOGP S-501

## 6.1 — Objective

Full work order engine with preventive maintenance scheduling, breakdown work orders,
turnaround planning, and contractor management — all backed by PPDM `EQUIPMENT`,
`FACILITY_MAINTAIN`, `PROJECT` tables.

## 6.2 — PPDM Table Mapping

| WO Feature | PPDM Tables |
|---|---|
| WO Header | `PROJECT` (`PROJECT_TYPE = 'WORK_ORDER'`) |
| Equipment | `EQUIPMENT`, `EQUIPMENT_USE_STAT` |
| Maintenance Record | `EQUIPMENT_MAINTAIN`, `EQUIPMENT_MAINT_STATUS` |
| Facility Maintenance | `FACILITY_MAINTAIN`, `FACILITY_MAINT_STATUS` |
| Contractor Assignment | `PROJECT_STEP_BA`, `BUSINESS_ASSOCIATE` (`BA_TYPE = 'CONTRACTOR'`) |
| Spare Parts / Materials | `CAT_EQUIPMENT_COMPONENT`, `EQUIPMENT_COMPONENT` |
| Cost Codes | `FINANCE`, `FIN_COMPONENT` |
| Planned Dates | `PROJECT_STEP_TIME` |
| Instrument Calibration | `INSTRUMENT`, `INSTRUMENT_CALIBRATION` |
| Permits to Work | `BA_PERMIT`, `CONSENT` |

## 6.3 — Todo Tracker

| ID | Task | Priority | Status | Notes |
|---|---|---|---|---|
| 6.1 | `IWorkOrderService` + `PPDMWorkOrderService` | HIGH | 🔲 | CRUD for WO with ISO 55001 criticality ranking |
| 6.2 | Preventive maintenance schedule generator | MEDIUM | 🔲 | Generates `PROJECT_STEP` rows based on `EQUIPMENT.MAINT_FREQ` |
| 6.3 | Contractor management API (`/api/field/current/workorders/contractors`) | MEDIUM | 🔲 | Links `BUSINESS_ASSOCIATE` to WO step |
| 6.4 | `WorkOrderList.razor` + `WorkOrderDetail.razor` pages | MEDIUM | 🔲 | Kanban board (MudDropContainer) by status |
| 6.5 | Gantt chart for turnaround planning | LOW | 🔲 | `MudTimeline` or 3rd-party Blazor Gantt |
| 6.6 | Spare parts lookup component | LOW | 🔲 | Search `EQUIPMENT_COMPONENT` by part number |

---

# Phase 7 — HSE & Safety Management System (ISO 45001 / IOGP)

> **Standards**: ISO 45001:2018, IOGP Report 2022e, API RP 754, OSHA PSM 29 CFR 1910.119, IOGP S-509, NORSOK Z-013

## 7.1 — Objective

Full HSSE management module: incident tracking (API RP 754 Tier 1-4), HAZOP/HAZID
recording, corrective action tracking, emergency response plans, and performance
KPIs published to the field dashboard.

## 7.2 — PPDM Table Mapping

| HSE Feature | PPDM Tables | Standard References |
|---|---|---|
| Incident Header | `HSE_INCIDENT` | API RP 754 classification |
| Incident People | `HSE_INCIDENT_BA` | Name, role, employer, contractor flag |
| Incident Classification | `HSE_INCIDENT_CLASS` | Tier 1/2/3/4 + IOGP type code |
| Root Cause | `HSE_INCIDENT_CAUSE` | UK HSE / Tripod Beta taxonomy |
| Corrective Actions | `PROJECT_STEP` (linked to incident) | |
| HAZOP Record | `RM_INFORMATION_ITEM` (HAZOP study doc) | IEC 61882 document type |
| Emergency Plan | `RM_INFORMATION_ITEM` | OPA 90 (USA); CEPA (Canada) |
| Safety KPIs | `HSE_INCIDENT_SEVERITY` + aggregation | IOGP 2022e TRIR / LTIR |
| Barrier Log | `PROJECT_STEP_CONDITION` | Bowtie / Swiss Cheese barrier |
| MOC Safety Review | `NOTIFICATION`, `CONSENT` | OSHA 1910.119(l) |

## 7.3 — Responsibility Matrix (RACI)

| Task | Safety Officer | HSE Dev | API Dev | QA | Regulatory |
|---|:---:|:---:|:---:|:---:|:---:|
| 7.1 — Incident CRUD + API | C | R/A | C | C | I |
| 7.2 — API RP 754 Tier logic | R/A | C | I | C | C |
| 7.3 — HAZOP record storage | C | R | C | C | I |
| 7.4 — Emergency Response Plan integration | R/A | C | I | C | C |
| 7.5 — Corrective Action tracker | C | R/A | C | C | I |
| 7.6 — HSE KPI dashboard page | C | R | C | R(QA) | I |

## 7.4 — Todo Tracker

| ID | Task | Priority | Status | Notes |
|---|---|---|---|---|
| 7.1 | `IHSEManagementService` interface | HIGH | 🔲 | Split from `PPDMProcessService` |
| 7.2 | `PPDMHSEManagementService` implementation | HIGH | 🔲 | Maps directly to `HSE_INCIDENT_*` tables |
| 7.3 | API RP 754 Tier classification enum + validation logic | HIGH | 🔲 | Tier 1: PSER / Release >TQ; Tier 2: etc. |
| 7.4 | Corrective action auto-deadline from incident date + severity | MEDIUM | 🔲 | Business rule: Tier 1 = 30 days cap |
| 7.5 | HAZOP document link to `RM_INFORMATION_ITEM` | MEDIUM | 🔲 | Store study ID, revision, P&ID reference |
| 7.6 | `HSEKPIDashboard.razor` with TRIR / LTIR trend charts | MEDIUM | 🔲 | Rolling 12-month + YTD vs IOGP benchmark |
| 7.7 | Safety Management Plan upload / versioning | LOW | 🔲 | PDF upload → `RM_INFORMATION_ITEM` |
| 7.8 | Emergency Response Plan viewer / download | LOW | 🔲 | Per `OPA 90` / Canada CEPA plan structure |

---

# Phase 8 — Compliance & Regulatory Reporting Engine

> **Standards**: USA: ONRR, BSEE, EIA; Canada: AER, NEB/CER; International: ISO 14001, EU ETS, OSPAR

## 8.1 — Objective

Automate generation and submission of regulatory reports across jurisdictions.
Track obligations by due date, penalty risk, and fulfillment status.

## 8.2 — PPDM Table Mapping

| Compliance Feature | PPDM Tables | Jurisdiction |
|---|---|---|
| Obligation tracking | `OBLIGATION`, `OBLIG_TYPE`, `OBLIG_BA_SERVICE` | All |
| Royalty calculation | `OBLIG_CALC`, `OBLIG_PAYMENT`, `RATE_SCHEDULE` | USA (ONRR); Canada (NEB) |
| Environmental consent tracking | `CONSENT`, `CONSENT_COND`, `RESTRICTION_TYPE` | All |
| Well permit tracking | `BA_PERMIT`, `BA_LICENSE`, `LICENSE_TYPE` | All |
| Production report volumes | `PDEN_VOL_SUMMARY`, `PDEN_VOL_DISPOSITION` | EIA-914; AER ST-39 |
| GHG emissions | `PDEN_VOL_SUMMARY` (`SUBSTANCE_TYPE` CO2/CH4) | EPA 40CFR98; ECCC; EU ETS |
| Partner notifications | `NOTIFICATION`, `NOTIF_BA`, `INTEREST_SET` | JOA Article IX; CAPL |
| Offshore safety case | `HSE_INCIDENT`, `FACILITY`, `OBLIGATION` | BSEE SEMS; UK DCR; Norway PTIL |

## 8.3 — Responsibility Matrix (RACI)

| Task | Compliance Officer | Dev | Data Architect | QA | Legal |
|---|:---:|:---:|:---:|:---:|:---:|
| 8.1 — Obligation engine | A | R | C | C | C |
| 8.2 — Royalty calculator | A | R | R | C | C |
| 8.3 — USA report templates | R/A | C | I | C | C |
| 8.4 — Canada report templates | R/A | C | I | C | C |
| 8.5 — GHG calculation | R | C | C | C | I |
| 8.6 — Due-date alert service | C | R/A | I | C | I |

## 8.4 — Todo Tracker

| ID | Task | Priority | Status | Notes |
|---|---|---|---|---|
| 8.1 | `IComplianceTrackingService` interface | HIGH | 🔲 | Methods: `GetOverdueObligationsAsync`, `CalculateRoyaltyAsync`, `GenerateReportAsync` |
| 8.2 | `PPDMComplianceTrackingService` implementation | HIGH | 🔲 | |
| 8.3 | Obligation due-date alert background service (IHostedService) | HIGH | 🔲 | Checks daily; writes `NOTIFICATION` rows for overdue |
| 8.4 | USA EIA-914 gas report template | HIGH | 🔲 | XML output; maps volume from `PDEN_VOL_SUMMARY` |
| 8.5 | AER ST-39 (Alberta) report template | HIGH | 🔲 | CSV output aligned to AER field format |
| 8.6 | GHG calculation engine (EPA 40 CFR 98 Subpart W) | HIGH | 🔲 | CH4 + CO2eq from `PDEN_VOL_SUMMARY.FLARE_VOLUME` etc. |
| 8.7 | Canada NEB export report template | MEDIUM | 🔲 | NEB Filling Manual format |
| 8.8 | OSPAR emission reporting skeleton | MEDIUM | 🔲 | OSPAR Recommendation 2012/5 format |
| 8.9 | `ComplianceCalendar.razor` with obligation due dates as events | MEDIUM | 🔲 | Color by overdue / due-soon / complete |
| 8.10 | Royalty calculator UI page | LOW | 🔲 | Shows ONRR / AER royalty rate + calculated value |

---

# Phase 9 — Integration & Field Orchestration

> **Standards**: OGC API Standards, WITSML 2.0, PRODML 2.1, OSDU Data Platform

## 9.1 — Objective

Connect the business process engine to external systems — SCADA, WITSML well
data, PRODML production data, document management, and third-party ERP (SAP/Oracle).

## 9.2 — Integration Points

| System | Protocol | PPDM Tables Updated | Standards |
|---|---|---|---|
| WITSML 1.4.1 / 2.0 | REST / SOAP | `WELL`, `CASING_PROGRAM`, `PROD_STRING` | Energistics WITSML standard |
| PRODML 2.1 | REST | `PDEN_VOL_SUMMARY`, `PROD_STRING` | Energistics PRODML |
| SCADA / Historian | MQTT / OPC-UA | `PDEN_VOL_SUMMARY` (real-time) | IEC 61968; OPC-UA 40000 |
| Document Management | CMIS 1.1 / SharePoint | `RM_INFORMATION_ITEM` | ISO 82045; PPDM RM category |
| SAP PM (Work Orders) | SAP RFC / REST | `PROJECT_STEP`, `EQUIPMENT_MAINTAIN` | SAP PM PN-API |
| Land Management | REST | `LAND_RIGHT`, `CONTRACT`, `INTEREST_SET` | AER Land file integration |
| ERP / Finance | REST | `FINANCE`, `OBLIG_PAYMENT` | COPAS Chart of Accounts |
| OSDU Data Platform | REST / Kafka | All PPDM tables (bidirectional) | OSDU R3 data model |

## 9.3 — Responsibility Matrix (RACI)

| Task | Integration Dev | Backend Dev | Data Architect | QA | IT/Ops |
|---|:---:|:---:|:---:|:---:|:---:|
| 9.1 — WITSML adapter | R/A | C | C | C | I |
| 9.2 — PRODML adapter | R/A | C | C | C | I |
| 9.3 — SCADA bridge | R | C | C | C | R/A |
| 9.4 — Document management | R/A | C | I | C | I |
| 9.5 — SAP PM adapter | R | C | C | C | A |
| 9.6 — OSDU integration | R | R | R/A | C | I |

## 9.4 — Todo Tracker

| ID | Task | Priority | Status | Notes |
|---|---|---|---|---|
| 9.1 | `IWITSMLSyncService` to pull well events into `WELL` table | HIGH | 🔲 | Nightly batch |
| 9.2 | `IPRODMLSyncService` to pull production volumes into `PDEN_VOL_SUMMARY` | HIGH | 🔲 | |
| 9.3 | `IDocumentManagementService` to store RM_INFORMATION_ITEM attachments | MEDIUM | 🔲 | Filesystem or SharePoint backend |
| 9.4 | SAP PM work order import adapter | MEDIUM | 🔲 | Maps SAP order type → PPDM `PROJECT_TYPE` |
| 9.5 | OSDU R3 data ingestion pipeline | LOW | 🔲 | Kafka consumer → PPDM tables |
| 9.6 | Integration health dashboard page | MEDIUM | 🔲 | Shows last sync time + error count per adapter |

---

# Phase 10 — Deployment, Hardening & International Rollout

> **Standards**: ISO/IEC 27001 (Security), SOC 2 Type II, NIST 800-53, CSA STAR

## 10.1 — Objective

Production-ready deployment with security hardening, audit logging, performance
tuning, multi-region support, and documentation per jurisdiction.

## 10.2 — Security & Compliance Hardening

| Control | Standard | Implementation |
|---|---|---|
| Authentication | NIST 800-53 IA-2 | JWT + Identity Server (existing) |
| Authorization | OWASP API-1, NIST AC-5 | Role-based `[Authorize]` on all endpoints |
| Audit trail | SOC 2 CC7.4 | `PPDM_AUDIT_HISTORY` on ALL write operations |
| Data encryption at rest | NIST 800-53 SC-28 | AES-256 at DB level |
| TLS in transit | NIST 800-53 SC-8 | TLS 1.3 between API and Web layers |
| Injection prevention | OWASP A03 | AppFilter parameterized queries throughout |
| PPDM data masking | GDPR Art. 25 | Mask `BA_*` PII columns for non-privileged roles |
| Activity logging | BSEE SEMS §250.1925 | Serilog to file + Azure Monitor |

## 10.3 — Responsibility Matrix (RACI)

| Task | DevOps | Security | Backend Dev | QA | Compliance |
|---|:---:|:---:|:---:|:---:|:---:|
| 10.1 — Production deployment | R/A | C | C | C | I |
| 10.2 — Security hardening | C | R/A | C | C | I |
| 10.3 — Performance load test | C | I | C | R/A | I |
| 10.4 — Jurisdiction documentation | I | I | I | I | R/A |
| 10.5 — Disaster recovery test | R | C | C | C | I |
| 10.6 — SOC 2 / ISO 27001 evidence | C | R | C | C | A |

## 10.4 — Todo Tracker

| ID | Task | Priority | Status | Notes |
|---|---|---|---|---|
| 10.1 | Production `appsettings.Production.json` environment configuration | HIGH | 🔲 | All connection strings from Key Vault |
| 10.2 | `PPDM_AUDIT_HISTORY` write on ALL state transitions (verify 100% coverage) | HIGH | 🔲 | |
| 10.3 | Load test: 100 concurrent process instances | HIGH | 🔲 | k6 or NBomber |
| 10.4 | Role matrix documentation (per jurisdiction) | HIGH | 🔲 | USA: BSEE/ONRR roles; Canada: AER roles |
| 10.5 | GDPR / PIPEDA PII masking for `BUSINESS_ASSOCIATE` columns | HIGH | 🔲 | Affects EU users of pipeline/contracts screens |
| 10.6 | Penetration test report (OWASP Top 10) | HIGH | 🔲 | Especially API input validation + JWT |
| 10.7 | Multi-region failover documentation | MEDIUM | 🔲 | Azure paired regions for offshore ops |
| 10.8 | Operator training manual (USA, Canada, International variants) | MEDIUM | 🔲 | Docusaurus site or PDF |
| 10.9 | DR drill — restore from backup + replay 24h of process events | MEDIUM | 🔲 | RTO ≤ 4h, RPO ≤ 1h target |
| 10.10 | SOC 2 Type II evidence package | LOW | 🔲 | Audit trail export + access review |

---

# Master Todo Tracker — All Phases

> ✅ Complete | 🔄 In Progress | 🔲 Not Started | ⛔ Blocked

| Phase | ID | Task | Priority | Status |
|---|---|---|---|---|
| 1 | — | Branch Infrastructure (Categories, Root, Category, Leaf nodes, SVGs) | HIGH | ✅ Complete |
| 2 | 2.1.1 | Extend ProcessDefinitionInitializer — 8 missing categories | HIGH | 🔲 |
| 2 | 2.1.2 | Seed Work Order category | HIGH | 🔲 |
| 2 | 2.1.3 | Seed Gate Review category | HIGH | 🔲 |
| 2 | 2.1.4 | Seed HSE category | HIGH | 🔲 |
| 2 | 2.1.5 | Seed Compliance category | HIGH | 🔲 |
| 2 | 2.1.6 | Seed Well/Facility/Reservoir/Pipeline lifecycles (32 defs) | MEDIUM | 🔲 |
| 2 | 2.2–2.6 | Register 4 new state machines | HIGH | 🔲 |
| 2 | 2.7 | `GetProcessDefinitionByNameAsync` helper | HIGH | 🔲 |
| 2 | 2.8–2.11 | Unit tests per state machine | MEDIUM | 🔲 |
| 2 | 2.12–2.15 | PPDM table binding on step definitions | MEDIUM | 🔲 |
| 3 | 3.1–3.4 | 4 API controllers | HIGH | 🔲 |
| 3 | 3.5–3.8 | Request/response model classes | HIGH | 🔲 |
| 3 | 3.9 | ProgramCS controller registration | HIGH | 🔲 |
| 3 | 3.10 | Swagger docs | MEDIUM | 🔲 |
| 3 | 3.11–3.13 | Integration tests | MEDIUM | 🔲 |
| 4 | 4.1–4.11 | 11 Blazor pages | HIGH | 🔲 |
| 4 | 4.12–4.19 | 8 reusable components | HIGH | 🔲 |
| 4 | 4.20 | NavMenu update | MEDIUM | 🔲 |
| 4 | 4.21–4.22 | E2E tests | MEDIUM | 🔲 |
| 5 | 5.1–5.6 | Analytics service + dashboard | MEDIUM | 🔲 |
| 6 | 6.1–6.6 | Work order engine + UI | MEDIUM | 🔲 |
| 7 | 7.1–7.8 | HSE HSSE module | HIGH | 🔲 |
| 8 | 8.1–8.10 | Compliance + reporting engine | HIGH | 🔲 |
| 9 | 9.1–9.6 | External integrations | LOW | 🔲 |
| 10 | 10.1–10.10 | Security hardening + rollout | HIGH | 🔲 |

---

# Appendix A — International Standards Quick Reference

| Standard | Body | Scope | Applicable Phases |
|---|---|---|---|
| SPE PRMS 2018 | SPE/WPC/AAPG/SPEE | Petroleum Resources Classification | 2, 5, 11 |
| API RP 100-1/100-2 | API | Well Engineering; P&A | 2, 9 |
| API RP 754 | API | Process Safety — Tier 1-4 Incident KPIs | 2, 7 |
| API RP 580/581 | API | Risk-Based Inspection | 10 |
| ISO 55001:2018 | ISO/TC 251 | Asset Management Systems | 2, 6 |
| ISO 14224:2016 | ISO | Reliability & Maintenance Data | 6 |
| ISO 14001:2015 | ISO/TC 207 | Environmental Management | 8, 10 |
| ISO 45001:2018 | ISO/TC 283 | Occupational H&S | 7 |
| ISO 15663:2021 | ISO | Life Cycle Costing | 5 |
| ISO 20815:2018 | ISO | Production Assurance | 4, 10 |
| IEC 61511:2016 | IEC | Functional Safety (SIL) | 2, 4 |
| IEC 61882:2016 | IEC | HAZOP | 7 |
| IOGP 2022e | IOGP | Safety Performance Indicators | 5, 7 |
| IOGP S-501 | IOGP | Construction / Contractor Management | 2, 6 |
| IOGP RP 70 | IOGP | PSSR / Commissioning | 4, 6 |
| OSPAR Dec. 98/3 | OSPAR | Offshore Decommissioning | 2, 10 |
| NORSOK D-010 | S​tandardNorge | Well Integrity | 2, 9 |
| NORSOK Z-013 | S​tandardNorge | Risk and Emergency Preparedness | 7 |
| ASME B31.8 / B31.8S | ASME | Gas Transmission Pipelines | 2, 12 |
| CSA Z662-15 | CSA | Oil & Gas Pipeline Systems (Canada) | 2, 12 |
| NI 51-101 | CSA / CAPP | Canadian Oil & Gas Disclosure | 5 |
| AER Directive 056 | AER | Energy Development Applications (Alberta) | 2, 8 |
| BSEE 30 CFR 250 | BSEE | Offshore Oil & Gas Operations (USA) | 2, 3, 8 |
| ONRR Reporting | ONRR | Royalty Reporting (USA Federal) | 8 |
| EPA 40 CFR 98 Subpart W | EPA | GHG Reporting (Oil & Gas) | 8 |
| FERC Form 2 / Order 7157 | FERC | Pipeline Reporting / Abandonment (USA) | 8, 12 |
| GDPR Art. 25 | EU | Data Protection by Design | 10 |
| PIPEDA | OPC Canada | Canadian Privacy Legislation | 10 |

---

# Appendix B — RACI Legend and Roles

| Role | Description |
|---|---|
| **Lead Engineer** | Senior engineer responsible for technical correctness |
| **Domain SME** | G&G, Reservoir, Drilling, Production, or Facilities subject matter expert |
| **Data Architect** | PPDM data model + schema governance |
| **Backend Dev** | C# service layer + Beep integration |
| **API Dev** | ASP.NET Core controller + DTO design |
| **UI Dev** | Blazor server page + MudBlazor component development |
| **QA/Test** | Test plan, unit/integration/E2E coverage |
| **Safety Officer** | HSE domain authority, API RP 754 incident classification ownership |
| **Compliance Officer** | Regulatory filings, obligation tracking, royalty report ownership |
| **DevOps / IT Ops** | Infrastructure, deployment, monitoring, DR |
| **Legal / Regulatory** | Permit applications, consent conditions, JOA obligations |

> R = Responsible | A = Accountable | C = Consulted | I = Informed  
> Maximum one **A** per row. One role may hold R+A simultaneously.
