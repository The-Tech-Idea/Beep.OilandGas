# Beep.OilandGas — Role-Based Enhancement Plan

**Date:** 2026-06-29
**Scope:** Every role from CEO to Admin to Accounting, across all 130+ pages and 75 business processes
**Basis:** Full reading of 52 projects, 75 process definitions, 16 personas, 13 roles, 177 permissions, 51 PPDM categories

---

## Executive Summary

The Beep.OilandGas platform has mature engineering calculation capabilities and a comprehensive PPDM 3.9 data model. However, the role-based access and user experience for different organizational roles (CEO, Manager, Engineer, Accountant, Admin) need enhancement.

### Key Findings

1. **ALLOWED_WORKFLOWS_JSON is NULL for all 16 personas** — persona-based navigation is default-allow, not default-deny ✅ FIXED in DefaultSecuritySeedService
2. **DataManagementController had no authorization** — generic CRUD for all PPDM tables was unauthenticated ✅ FIXED — added [Authorize]
3. **No CEO/Executive persona** — roles cover engineering, management, HSE, but no C-suite dashboard
4. **No Accounting-specific persona** — the `Accounting` role exists but has no matching persona with workflow mapping
5. **Persona-to-role mapping is implicit** — no explicit relationship between what persona a user selects and what permissions they have
6. **75 processes defined but not seeded at startup** — fixed in BeepDM integration Phase 3 (M-26)
7. **Most pages use only soft persona-based gating** — only 6 pages have `[Authorize(Roles)]`; API controllers now protected
8. **No role-specific dashboards** — all users see the same landing page regardless of role

### Approach: End-to-End Per Role

Each role is completed end-to-end before moving to the next:
```
Backend Aggregation Service → API Controller → Persona/Role Setup → Blazor Dashboard
```

### Priority Order

| # | Role | Why |
|---|------|-----|
| 1 | Security Hardening | Foundation for all other work |
| 2 | Accountant | Most underserved role. 32 accounting services, 80+ entity types already built — no persona, no dedicated UI |
| 3 | Executive / CEO | Highest visibility. Portfolio-level KPIs across all assets |
| 4 | Production Manager | Largest operational user base. Daily production, downtime, interventions |
| 5 | Reservoir Engineer | Technical depth. Reserves, material balance, EOR screening |
| 6 | Drilling Engineer | Well construction programs, AFE tracking, daily reports |
| 7 | Production Engineer | Nodal analysis, artificial lift optimization, pipeline hydraulics |
| 8 | HSE Officer | Regulatory requirement. Incident tracking, Tier dashboard, audits |
| 9 | Compliance Officer | Obligations, GHG reporting, permit expiration calendar |
| 10 | Data Analyst | Data quality, profiling, import/export, sync health |
| 11 | System Administrator | User/role management, migration governance, setup wizard |
| 12 | Asset Manager | Multi-field portfolio, reserves, lease calendar |

---

## Phase 1: Security Hardening (Foundation) ✅ COMPLETE

### 1.1 Populate ALLOWED_WORKFLOWS_JSON for All Personas ✅
**File:** `UserManagement/Services/DefaultSecuritySeedService.cs`

All 16 personas now have explicit workflow access codes:

| Persona | Workflows |
|---------|-----------|
| ADMINISTRATOR | exploration, development, production, reservoir, economics, hse, data, processes |
| ASSET_MANAGER, PRODUCTION_MANAGER | production, reservoir, economics, data |
| RESERVOIR_ENGINEER | reservoir, production, data |
| DEVELOPMENT_PLANNER | development, economics, data |
| DRILLING_ENGINEER | development, data |
| PRODUCTION_ENGINEER, FIELD_ENGINEER | production, data |
| FACILITIES_ENGINEER, FACILITY_OPERATOR | development, production, data |
| EXPLORATION_GEOLOGIST | exploration, data |
| HSE_OFFICER, HSE_COORDINATOR | hse, data |
| DATA_ANALYST | data, processes |
| WORKFLOW_ADMINISTRATOR | processes, data |
| DECOMMISSIONING_COORDINATOR | development, hse, data |

### 1.2 Add [Authorize] to Unprotected API Controllers ✅

| Controller | Route | Authorization |
|------------|-------|--------------|
| DataManagementController | /api/datamanagement | [Authorize] |
| WellController | /api/well | [Authorize] |
| CalculationsController | /api/calculations | [Authorize] |
| ConnectionController | /api/connections | [Authorize(Roles = "Admin,Administrator")] |
| DemoDatabaseController | /api/demo | [Authorize(Roles = "Admin,Administrator")] |

### 1.3 Upcoming: Persona-to-Role Mapping Entity
**New entity:** `PERSONA_ROLE` — maps persona code to required role
This enables: "When user switches to X persona, they need role Y"

---

## Phase 2: Accounting & Financial Role (Weeks 1-3)

### Backend: Accounting Aggregation Service
**File:** `ApiService/Services/AccountingAggregationService.cs` (NEW)

Aggregates PPDM accounting data for dashboard display:
```
GetRevenueSummaryAsync(fieldId, start, end)     → RevenueSummary { TotalRevenue, ByProduct, ByField, Mtd, Ytd, Trend }
GetCostSummaryAsync(fieldId, start, end)         → CostSummary { TotalLOE, TotalCAPEX, ByCostCenter, ByAFE, PerBOE }
GetRoyaltySummaryAsync(fieldId, start, end)      → RoyaltySummary { TotalRoyalties, ByOwner, PendingPayments, DueDates }
GetPeriodCloseStatusAsync(fieldId)               → PeriodCloseStatus { OpenPeriods, ReconciliationGaps, DaysSinceClose }
GetAFESummaryAsync(fieldId)                      → List<AfeSummary> { AFE_ID, Description, Budget, Spent, Remaining, Status }
GetRunTicketSummaryAsync(fieldId, start, end)    → RunTicketSummary { TotalVolume, TotalValue, AveragePrice, ByProduct }
GetJIBSummaryAsync(fieldId, period)              → JIBSummary { TotalCharges, ByPartner, NetPosition }
```

**Data sources:** RUN_TICKET, REVENUE_TRANSACTION, REVENUE_ALLOCATION, ROYALTY_CALCULATION, ROYALTY_PAYMENT, COST_TRANSACTION, COST_ALLOCATION, COST_CENTER, AFE, AFE_LINE_ITEM, JIB_CHARGE, JOIB_LINE_ITEM, JOINT_INTEREST_BILL, GL_ENTRY, JOURNAL_ENTRY

### API: Accounting Aggregation Controller
**File:** `ApiService/Controllers/AccountingAggregationController.cs` (NEW)
**Route:** `/api/accounting/aggregation` with `[Authorize]`

```
GET  /api/accounting/aggregation/revenue?fieldId=&start=&end=       → RevenueSummary
GET  /api/accounting/aggregation/costs?fieldId=&start=&end=         → CostSummary
GET  /api/accounting/aggregation/royalties?fieldId=&start=&end=     → RoyaltySummary
GET  /api/accounting/aggregation/period-close?fieldId=               → PeriodCloseStatus
GET  /api/accounting/aggregation/afe?fieldId=                        → List<AfeSummary>
GET  /api/accounting/aggregation/run-tickets?fieldId=&start=&end=   → RunTicketSummary
GET  /api/accounting/aggregation/jib?fieldId=&period=                → JIBSummary
```

### Persona & Security
**New persona:** `ACCOUNTANT` — Category: Finance, DefaultRoute: `/accounting/dashboard`, Workflows: ["economics", "data"], SortOrder: 25

**Role permission mapping:**
```
Accountant → Accounting (View, PostJournal, ApproveJournal, ViewReports, ManagePeriods)
              Tax (ViewProvision, Calculate)
              Production (View)
              Reporting (View, Export)
```

### UI: Accountant Dashboard
**File:** `Web/Pages/PPDM39/Accounting/AccountantDashboard.razor` (NEW)
**Route:** `/accounting/dashboard`

Panels:
1. **Revenue Summary** — Total revenue MTD/YTD, by product (oil/gas/NGL), by field, average price/BOE, trend indicator
2. **Cost Summary** — LOE vs CAPEX breakdown, by cost center, $/BOE metric, AFE variance
3. **Royalty Summary** — Total royalties owed/paid, by owner, pending payment count, next due date
4. **Period Close Status** — Open accounting periods, days since last close, unreconciled items count
5. **AFE Status** — Approved/Pending/Overspent AFEs, budget vs actual by AFE
6. **Run Ticket Quick View** — Recent run tickets, total volume/value, average price
7. **Quick Actions** — New Journal Entry, Process Run Ticket, Close Period, View JIB Statement

### UI: Revenue Accountant Workbench
**File:** `Web/Pages/PPDM39/Accounting/RevenueWorkbench.razor` (NEW)
**Route:** `/accounting/revenue-workbench`

| Feature | PPDM Source |
|---------|------------|
| Run ticket processing | RUN_TICKET |
| Revenue recognition (ASC 606) | REVENUE_TRANSACTION, REVENUE_ALLOCATION |
| Royalty calculation | ROYALTY_CALCULATION |
| Joint interest billing | JIB_CHARGE, JOIB_LINE_ITEM |
| Volume reconciliation | PDEN_VOL_SUMMARY vs RUN_TICKET |
| Period close dashboard | PeriodClosingService |

### UI: Cost Accountant Workbench
**File:** `Web/Pages/PPDM39/Accounting/CostWorkbench.razor` (NEW)
**Route:** `/accounting/cost-workbench`

| Feature | PPDM Source |
|---------|------------|
| AFE vs actual tracking | AFE, AFE_LINE_ITEM, COST_TRANSACTION |
| LOE / CAPEX classification | FullCostService, SuccessfulEffortsService |
| DD&A calculation | AmortizationService |
| Impairment testing | ImpairmentTestingService |
| Cost allocation | COST_ALLOCATION |
| COPAS overhead | COPAS_OVERHEAD_SCHEDULE |

### UI: Tax Accountant Page
**File:** `Web/Pages/PPDM39/Accounting/TaxWorkbench.razor` (NEW)
**Route:** `/accounting/tax-workbench`

| Feature | PPDM Source |
|---------|------------|
| Severance tax calculations | ProductionTaxService |
| Ad valorem tax tracking | TAX_TRANSACTION |
| 1099 / royalty owner reporting | ROYALTY_OWNER, ROYALTY_PAYMENT |
| State/federal filing calendar | OBLIGATION with tax type filters |

---

## Phase 3: Executive & Management Dashboards (Weeks 4-6)

### Backend: Executive Aggregation Service
**File:** `ApiService/Services/ExecutiveAggregationService.cs` (NEW)

```
GetExecutiveKpiAsync()          → ExecutiveKpi { Production, Revenue, Reserves, HSE, CAPEX, OpCost }
GetAssetPerformanceAsync()      → List<AssetPerformance> { FieldName, Production, OpEx, Reserves, Wells, Phase }
GetUpcomingDeadlinesAsync()     → List<DeadlineItem> { Description, Date, DaysUntil, Type }
GetPortfolioNpvAsync()          → PortfolioNpv { TotalNPV, ByField, DiscountRate }
GetProductionVsBudgetAsync()    → ProductionVsBudget { Actual, Budget, Variance, ByField }
```

**Data sources:** PDEN_VOL_SUMMARY, REVENUE_TRANSACTION, RESERVE, HSE_INCIDENT, AFE, OBLIGATION, LAND_RIGHT, ECONOMIC_ANALYSIS_RESULT

### API: Executive Dashboard Controller
**File:** `ApiService/Controllers/ExecutiveController.cs` (NEW)
**Route:** `/api/executive` with `[Authorize]`

```
GET /api/executive/kpi              → ExecutiveKpi
GET /api/executive/assets           → List<AssetPerformance>
GET /api/executive/deadlines        → List<DeadlineItem>
GET /api/executive/portfolio-npv    → PortfolioNpv
GET /api/executive/production-vs-budget → ProductionVsBudget
```

### Persona
**New persona:** `EXECUTIVE` — Category: Management, DefaultRoute: `/executive/dashboard`, Workflows: ["production", "reservoir", "economics", "data"], SortOrder: 5

### UI: CEO / Executive Dashboard
**File:** `Web/Pages/PPDM39/ExecutiveDashboard.razor` (DRAFT CREATED)
**Route:** `/executive/dashboard`

| Metric | PPDM Source |
|--------|------------|
| Total Production (BOE/d) | PDEN_VOL_SUMMARY aggregation |
| Revenue (MTD/YTD) | REVENUE_TRANSACTION aggregation |
| Operating Cost ($/BOE) | COST_TRANSACTION aggregation |
| Reserves (1P/2P/3P, MMBOE) | RESERVE aggregation |
| Production vs Budget (%) | PRODUCTION_FORECAST comparison |
| HSE LTIF / Tier 1-2 count | HSE_INCIDENT API RP 754 tiers |
| Active Wells / Producing Wells | WELL + WELL_STATUS aggregation |
| Pending Regulatory Items | OBLIGATION status count |
| CAPEX vs AFE ($MM) | AFE + AFE_LINE_ITEM summary |
| Portfolio NPV | ECONOMIC_ANALYSIS_RESULT |

### UI: Asset Manager Portfolio Dashboard
**File:** `Web/Pages/PPDM39/AssetPortfolio.razor` (NEW)
**Route:** `/asset/portfolio`
**Persona:** ASSET_MANAGER (existing, enhanced)

| Feature | PPDM Source |
|---------|------------|
| Multi-field comparison | FIELD comparison |
| Reserves by asset (SPE PRMS) | RESERVE by FIELD |
| Production by asset | PDEN_VOL_SUMMARY by FIELD |
| Economic summary by field | ECONOMIC_CASH_FLOW |
| Lease expiration calendar | LAND_RIGHT expiry dates |
| Capital allocation overview | AFE by FIELD |

### UI: Production Manager Operations Dashboard
**File:** `Web/Pages/PPDM39/Production/ProductionManagerDashboard.razor` (NEW)
**Route:** `/production/operations`
**Persona:** PRODUCTION_MANAGER (existing, enhanced)

| Feature | PPDM Source |
|---------|------------|
| Daily production by field/well | PDEN_VOL_SUMMARY |
| Downtime tracking | EQUIPMENT_MAINTAIN, WELL_ACTIVITY |
| Well test results | WELL_TEST analysis |
| Intervention candidates | ProductionOperationsService |
| Gas lift optimization | GAS_LIFT_PERFORMANCE |
| Nodal analysis summary | NODAL_ANALYSIS_RESULT |

---

## Phase 4: Engineering Roles Deep-Dive (Weeks 7-10)

### 4.1 Reservoir Engineer Workspace

**Backend Service:** `ApiService/Services/ReservoirAggregationService.cs` (NEW)
```
GetPoolSummaryAsync(fieldId)       → List<PoolSummary>
GetReservesByCategoryAsync(fieldId) → ReservesBreakdown { P1, P2, P3, ByField, ByPool }
GetEorScreeningAsync(poolId)       → EorScreeningResult
GetMaterialBalanceAsync(poolId)    → MaterialBalanceResult
```

**API:**
```
GET /api/reservoir/aggregation/pools?fieldId=          → List<PoolSummary>
GET /api/reservoir/aggregation/reserves?fieldId=        → ReservesBreakdown
GET /api/reservoir/aggregation/eor-screening?poolId=    → EorScreeningResult
GET /api/reservoir/aggregation/material-balance?poolId= → MaterialBalanceResult
```

**Persona:** RESERVOIR_ENGINEER (existing, workflow mapping enhanced)

**UI:** `/reservoir/workspace` — Material balance dashboard, decline curve analysis, EOR screening matrix, reserves workbook (PRMS), well test interpretation, PVT characterization

### 4.2 Drilling Engineer Workspace

**Backend Service:** `ApiService/Services/DrillingAggregationService.cs` (NEW)
```
GetDrillingProgramsAsync(fieldId)    → List<DrillingProgram>
GetDailyReportSummaryAsync(wellUwi)  → DailyReportSummary
GetAfeTrackingAsync(afeId)           → AfeTracking
GetNptSummaryAsync(fieldId, start, end) → NptSummary
```

**Persona:** DRILLING_ENGINEER (existing, workflow mapping enhanced)

**UI:** `/drilling/workspace` — Drilling program designer, AFE cost estimator, daily report tracker, bit/fluid/casing tracker, offset well analysis, NPT tracking

### 4.3 Production Engineer Workspace

**Backend Service:** `ApiService/Services/ProductionEngineerAggregationService.cs` (NEW)
```
GetNodalAnalysisSummaryAsync(wellUwi)      → NodalSummary
GetArtificialLiftStatusAsync(fieldId)       → List<LiftStatus>
GetPipelineHydraulicsAsync(pipelineId)       → HydraulicsResult
GetCompressorStatusAsync(facilityId)         → CompressorStatus
```

**Persona:** PRODUCTION_ENGINEER (existing, workflow mapping enhanced)

**UI:** `/production/engineer-workspace` — Nodal analysis IPR/VLP, artificial lift optimization (gas lift, SRP, plunger, hydraulic, ESP), pipeline hydraulics, compressor analysis, choke performance, production forecasting

---

## Phase 5: HSE & Compliance Roles (Weeks 11-13)

### 5.1 HSE Officer Dashboard Upgrade

**Backend Service:** `ApiService/Services/HseAggregationService.cs` (NEW)
```
GetIncidentSummaryAsync(fieldId, start, end) → IncidentSummary { ByTier, ByType, ByLocation, TotalCount, LTIF }
GetCorrectiveActionsAsync(fieldId)            → List<CorrectiveAction> { Description, DueDate, Status, Owner }
GetPermitStatusAsync(fieldId)                 → List<PermitStatus> { Type, IssuedTo, ExpiryDate, Status }
GetBarrierHealthAsync(facilityId)             → BarrierHealth { Effective, Degraded, Failed, NotApplicable }
```

**Persona:** HSE_OFFICER (existing, workflow enhanced)

**UI:** `/hse/dashboard` upgrade — Incident heat map, API RP 754 Tier dashboard, corrective action tracker, barrier management, safety observations, PTW status

### 5.2 Compliance Officer Workspace

**Backend Service:** `ApiService/Services/ComplianceAggregationService.cs` (NEW)
```
GetObligationsAsync(fieldId)        → List<Obligation> { Type, Description, DueDate, Status }
GetGhgReportAsync(fieldId, year)     → GhgReport { TotalEmissions, BySource, ByGas }
GetPermitExpirationsAsync(fieldId)   → List<PermitExpiration> { PermitType, ExpiryDate, DaysUntil }
GetAuditTrailAsync(entityType, id)   → List<AuditEvent>
```

**New persona:** `COMPLIANCE_OFFICER` — Category: Compliance, DefaultRoute: `/compliance/dashboard`, Workflows: ["hse", "data"]

**UI:** `/compliance/dashboard` — Obligation tracker, GHG reporting dashboard, permit expiration calendar, regulatory filing status, audit trail viewer

---

## Phase 6: Data Management & Admin Roles (Weeks 14-16)

### 6.1 Data Management Workspace

**Backend Service:** `ApiService/Services/DataQualityAggregationService.cs` (NEW)
```
GetQualityDashboardAsync()     → List<TableQuality> { TableName, Score, Trend, IssueCount }
GetImportHistoryAsync()        → List<ImportRun> { ContextKey, RecordsRead, RecordsFailed, Duration, Date }
GetSyncHealthAsync()           → SyncHealth { SchemaCount, LastSyncTime, SloTier, ReconciliationReport }
GetProfilingSummaryAsync()     → List<TableProfileSummary> { TableName, RowCount, NullRatio, DistinctCount }
```

**Persona:** DATA_ANALYST (existing, workflow enhanced)

**UI:** `/analytics/dashboard` upgrade — Data quality dashboard with trends, import/export hub, data profiling, sync health dashboard

### 6.2 System Administrator Workspace

**Backend Service:** `ApiService/Services/AdminAggregationService.cs` (NEW)
```
GetUserRoleAssignmentsAsync()   → List<UserRoleAssignment>
GetPermissionAuditAsync()       → List<RolePermission> { Role, Permission, GrantedBy, Date }
GetConnectionPoolStatusAsync()  → ConnectionPoolStatus { ActiveConnections, IdleConnections, Errors }
GetModuleSeedingStatusAsync()   → List<ModuleSeedStatus> { ModuleId, Seeded, RecordsInserted, LastRun }
GetMigrationPlansAsync()        → List<MigrationPlan> { PlanId, Status, CreatedDate, ApprovedBy }
```

**Persona:** ADMINISTRATOR (existing, workflow enhanced)

**UI Pages:**
- `/admin/user-roles` — User role assignment (uncomment API calls in existing UserRoles.razor)
- `/admin/permission-audit` — All role-permission mappings
- `/admin/connections` — Connection pool monitor
- `/admin/module-seeding` — Module seeding status with resume capability
- `/admin/migration-governance` — Plan approval, wave promotion, rollback

---

## Phase 7: Workflow & Approval Enhancements (Weeks 17-19)

### 7.1 Gate Review Dashboard by Persona

**Backend Service:** Uses existing ProcessService + ProcessDefinitionInitializer

| Persona | Pending Gates | Entity |
|---------|--------------|--------|
| EXPLORATION_GEOLOGIST | Gate: Exploration Review | PROSPECT |
| DEVELOPMENT_PLANNER | Gate: FDP Review, FID, Well Design Approval | PROJECT, WELL |
| ASSET_MANAGER | Gate: AFE Approval, FID | FINANCE, PROJECT |
| HSE_COORDINATOR | Gate: HAZOP Review, Safety Case | FACILITY |
| DECOMMISSIONING_COORDINATOR | Gate: Abandonment Approval | WELL |

### 7.2 Approval Chain Configuration UI
**New page:** `/admin/approval-chains`
- Configure multi-level approval chains per process type
- Delegate approvals during absence
- Set SLA deadlines per approval step

### 7.3 Process Instance Tracking by User
**New page:** `/ppdm39/process/my-tasks`
- Shows all process steps assigned to current user
- Pending approvals requiring action
- Overdue process steps with SLA breach highlighting

---

## Master TODO Tracker

### Phase 1: Security (Foundation) — COMPLETED

| # | Task | Status |
|---|------|--------|
| 1.1 | Populate ALLOWED_WORKFLOWS_JSON for all 16 personas | ✅ DONE |
| 1.2 | Add [Authorize] to DataManagementController | ✅ DONE |
| 1.3 | Add [Authorize] to WellController | ✅ DONE |
| 1.4 | Add [Authorize] to CalculationsController | ✅ DONE |
| 1.5 | Add [Authorize(Roles)] to ConnectionController | ✅ DONE |
| 1.6 | Add [Authorize(Roles)] to DemoDatabaseController | ✅ DONE |
| 1.7 | Create PERSONA_ROLE entity + seeding | ⬜ |

### Phase 2: Accounting (Current)

| # | Task | Status |
|---|------|--------|
| 2.1 | Create AccountingAggregationService | ⬜ |
| 2.2 | Create AccountingAggregationController | ⬜ |
| 2.3 | Seed ACCOUNTANT persona | ⬜ |
| 2.4 | Create AccountantDashboard.razor | ⬜ |
| 2.5 | Create RevenueWorkbench.razor | ⬜ |
| 2.6 | Create CostWorkbench.razor | ⬜ |
| 2.7 | Create TaxWorkbench.razor | ⬜ |
| 2.8 | Register AccountingAggregationService in DI | ⬜ |

### Phase 3: Executive & Management

| # | Task | Status |
|---|------|--------|
| 3.1 | Create ExecutiveAggregationService | ⬜ |
| 3.2 | Create ExecutiveController | ⬜ |
| 3.3 | Seed EXECUTIVE persona | ⬜ |
| 3.4 | Complete ExecutiveDashboard.razor (draft exists) | 🟡 |
| 3.5 | Create AssetPortfolio.razor | ⬜ |
| 3.6 | Create ProductionManagerDashboard.razor | ⬜ |

### Phase 4: Engineering

| # | Task | Status |
|---|------|--------|
| 4.1 | Create ReservoirAggregationService + Controller | ⬜ |
| 4.2 | Create ReservoirEngineerWorkspace.razor | ⬜ |
| 4.3 | Create DrillingAggregationService + Controller | ⬜ |
| 4.4 | Create DrillingEngineerWorkspace.razor | ⬜ |
| 4.5 | Create ProductionEngineerAggregationService + Controller | ⬜ |
| 4.6 | Create ProductionEngineerWorkspace.razor | ⬜ |

### Phase 5: HSE & Compliance

| # | Task | Status |
|---|------|--------|
| 5.1 | Create HseAggregationService + Controller | ⬜ |
| 5.2 | Upgrade HSE dashboard | ⬜ |
| 5.3 | Create ComplianceAggregationService + Controller | ⬜ |
| 5.4 | Seed COMPLIANCE_OFFICER persona | ⬜ |
| 5.5 | Create Compliance dashboard | ⬜ |

### Phase 6: Data Management & Admin

| # | Task | Status |
|---|------|--------|
| 6.1 | Create DataQualityAggregationService + Controller | ⬜ |
| 6.2 | Upgrade Data Analyst dashboard | ⬜ |
| 6.3 | Create AdminAggregationService + Controller | ⬜ |
| 6.4 | Fix UserRoles.razor (uncomment API calls) | ⬜ |
| 6.5 | Create Admin workspace pages | ⬜ |

### Phase 7: Workflows & Approvals

| # | Task | Status |
|---|------|--------|
| 7.1 | Create Gate Review dashboard by persona | ⬜ |
| 7.2 | Create Approval Chain Configuration UI | ⬜ |
| 7.3 | Create My Tasks / Pending Approvals page | ⬜ |

---

## Per-Role Summary

| Role | New Persona? | New API? | New UI? | Existing Backend |
|------|-------------|---------|---------|-----------------|
| Accountant | ✅ NEW | ✅ | ✅ 4 pages | 32 services, 80+ entities |
| Executive / CEO | ✅ NEW | ✅ | ✅ 1 page | FieldOrchestrator + aggregation |
| Asset Manager | ❌ Enhanced | — | ✅ 1 page | FieldOrchestrator |
| Production Manager | ❌ Enhanced | ✅ | ✅ 1 page | ProductionOperations + PDEN |
| Reservoir Engineer | ❌ Enhanced | ✅ | ✅ 1 page | ReservoirLifecycle + data |
| Drilling Engineer | ❌ Enhanced | ✅ | ✅ 1 page | DrillingAndConstruction |
| Production Engineer | ❌ Enhanced | ✅ | ✅ 1 page | 15 engineering calc projects |
| HSE Officer | ❌ Enhanced | ✅ | ✅ 1 page | HSE services (6 files) |
| Compliance Officer | ✅ NEW | ✅ | ✅ 1 page | Compliance + obligation services |
| Data Analyst | ❌ Enhanced | ✅ | ✅ 1 page | DataImport + Sync (new) |
| Administrator | ❌ Enhanced | ✅ | ✅ 5 pages | ModuleSetup + Migration + Security |
| Workflow Admin | ❌ Existing | — | ✅ 3 pages | ProcessService + 75 definitions |
| Field Engineer | ❌ Existing | — | — | Existing Well pages |
| HSE Coordinator | ❌ Existing | — | — | Existing HSE pages |
| Facilities Engineer | ❌ Existing | — | — | Existing Facility pages |
| Facility Operator | ❌ Existing | — | — | Existing ops pages |
| Exploration Geologist | ❌ Existing | — | — | Existing exploration pages |
| Decommissioning Coord. | ❌ Existing | — | — | Existing decommissioning pages |
| Development Planner | ❌ Existing | — | — | Existing development pages |
