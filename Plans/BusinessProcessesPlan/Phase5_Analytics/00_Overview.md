# Phase 5 — Analytics Overview
## KPI Dashboard, Service Design, and Reporting

> **Status**: Not started  
> **Depends on**: Phase 2 (process instance data in PPDM), Phase 3 (API), Phase 4 (UI shell)  
> **Blocks**: Phase 10 (hardening report requires analytics data)  
> **Owner**: Data Architect + Backend Dev  
> **Effort**: ~6 weeks (3 sprints)

---

## Document Index

| # | Document | Purpose |
|---|---|---|
| This file | `00_Overview.md` | Goals, key files, milestones, dependency diagram |
| [01](01_KPIDefinitions.md) | `01_KPIDefinitions.md` | All KPIs with formulas and PPDM data sources |
| [02](02_ServiceDesign.md) | `02_ServiceDesign.md` | `IAnalyticsService` interface and implementation skeleton |
| [03](03_QueryDesign.md) | `03_QueryDesign.md` | `AppFilter`-based query patterns per KPI |
| [04](04_DashboardDesign.md) | `04_DashboardDesign.md` | Blazor dashboard layout and MudBlazor chart components |
| [05](05_ReportTemplates.md) | `05_ReportTemplates.md` | PDF/Excel export templates and column definitions |
| [06](06_SprintPlan_RACI.md) | `06_SprintPlan_RACI.md` | Sprint stories, RACI, risks, Definition of Done |

---

## Goals

1. Six KPI categories: Work Order, Gate Review, HSE, Compliance, Production, Reservoir
2. Field-scoped: all queries filter by `FIELD_ID`
3. Date-range filtering: rolling 30/90/365 days or custom range
4. Blazor dashboard: 4–6 MudCard panels with MudChart visualizations
5. PDF and Excel export for weekly operations reports
6. No raw SQL — all queries via `AppFilter` + `PPDMGenericRepository`

---

## Out of Scope (Phase 5)

- Real-time SCADA data (Phase 9)
- Regulatory filing KPIs (Phase 8)
- Production forecasting charts (separate module)

---

## Key Files

| File Path | Action |
|---|---|
| `Beep.OilandGas.Models/Core/Interfaces/IAnalyticsService.cs` | CREATE |
| `Beep.OilandGas.PPDM39.DataManagement/Services/Analytics/AnalyticsQueryService.cs` | CREATE |
| `Beep.OilandGas.ApiService/Controllers/Analytics/AnalyticsController.cs` | CREATE |
| `Beep.OilandGas.Web/Pages/PPDM39/Analytics/AnalyticsDashboard.razor` | CREATE |
| `Beep.OilandGas.Web/Pages/PPDM39/Analytics/KPIDetail.razor` | CREATE |
| `Beep.OilandGas.Web/Components/Analytics/KPICard.razor` | CREATE |
| `Beep.OilandGas.Web/Components/Analytics/KPITrendChart.razor` | CREATE |

---

## Milestones

| ID | Target | Acceptance Criteria |
|---|---|---|
| M5-1 | Sprint 5.1 | `IAnalyticsService` defined; WO and HSE KPI queries return data |
| M5-2 | Sprint 5.2 | Dashboard page shows 4 KPI cards with real data |
| M5-3 | Sprint 5.3 | PDF export for WO summary report works |

---

## Dependency Diagram

```
Phase 2 (PPDMProcessService)
   │
   ▼  (produces PPDM_AUDIT_HISTORY + PROJECT rows)
Phase 5 Analytics
   │
   ├─► AnalyticsQueryService (reads PPDM_AUDIT_HISTORY, HSE_INCIDENT, OBLIGATION, PROJECT)
   │
   └─► AnalyticsDashboard.razor (via AnalyticsController API)
```
