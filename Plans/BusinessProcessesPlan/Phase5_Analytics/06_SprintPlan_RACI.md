# Phase 5 — Sprint Plan & RACI
## Analytics Delivery Schedule

> Total story points: ~55 SP  
> Sprint velocity: 22–25 SP/sprint  
> Target: 3 sprints (~6 weeks)

---

## Sprint Breakdown

### Sprint 5.1 — Service Layer (Weeks 1–2)

| Story | Points | Owner | Deliverable |
|---|---|---|---|
| Define `IAnalyticsService` interface + KPI result records | 3 | DA | Interface compiles |
| Implement `GetWorkOrderKPIsAsync` (WO-01 through WO-05) | 5 | BD | Returns real WO data |
| Implement `GetHSEKPIsAsync` (HSE-01 through HSE-05) | 5 | BD | Tier 1/2 PSE, TRIR |
| Implement `GetDashboardSummaryAsync` | 3 | BD | All 5 KPI sets in one call |
| Register `AnalyticsQueryService` in `Program.cs` | 1 | BD | Injectable |
| `AnalyticsController` — `GET /dashboard` endpoint | 2 | BD | Returns JSON |
| Unit tests: WO KPI calculations (5 test cases) | 3 | QA | Pass |
| **Sprint total** | **22 SP** | | |

---

### Sprint 5.2 — Dashboard UI (Weeks 3–4)

| Story | Points | Owner | Deliverable |
|---|---|---|---|
| `KPICard` component for WO and HSE cards | 3 | FE | Cards show correct values |
| `KPITrendChart` component (MudChart line) | 4 | FE | Production trend renders |
| `AnalyticsDashboard.razor` page wired to API | 5 | FE | Page loads with real data |
| Date range and exposure hours filters work | 3 | FE | Changing range refreshes cards |
| `AnalyticsController` — production trend endpoint | 2 | BD | Returns monthly data |
| bUnit tests: KPICard renders correct values | 4 | FE/QA | 6 tests pass |
| **Sprint total** | **21 SP** | | |

---

### Sprint 5.3 — Export & Polish (Weeks 5–6)

| Story | Points | Owner | Deliverable |
|---|---|---|---|
| `IReportExportService` + `GenerateWorkOrderSummaryXlsxAsync` | 5 | BD | XLSX downloads properly |
| `GenerateHSEKPIReportPdfAsync` via QuestPDF | 5 | BD | PDF has header, KPI table, footer |
| Export API endpoints (WO PDF, WO XLSX, HSE PDF) | 3 | BD | Files download from Swagger |
| Export download buttons in `AnalyticsDashboard.razor` | 2 | FE | Click → browser downloads file |
| Smoke test checklist | 2 | QA | All 8 checklist items green |
| **Sprint total** | **17 SP** | | |

---

## RACI Matrix

| Task | BD | DA | FE | QA | PM |
|---|---|---|---|---|---|
| KPI formula definitions | C | **R/A** | — | C | I |
| `IAnalyticsService` interface | **R** | **A** | — | — | I |
| Service implementation | **R/A** | C | — | C | I |
| Controller endpoints | **R/A** | — | — | C | I |
| Dashboard page + charts | — | — | **R/A** | C | I |
| Export (PDF/XLSX) | **R/A** | — | C | C | I |
| Integration tests | R | — | — | **R/A** | I |
| Acceptance sign-off | C | C | C | C | **R/A** |

---

## Risk Register

| Risk ID | Description | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R5-01 | `PPDM_AUDIT_HISTORY` table empty (no history seeded) | Medium | High | Seed realistic audit history in test environment during Sprint 5.1 |
| R5-02 | QuestPDF license check on build server | Low | Medium | Confirm QuestPDF Community license; add license acceptance in startup |
| R5-03 | MudChart API differs from expected | Low | Low | Verify chart component API at Sprint 5.2 start |

---

## Definition of Done (Phase 5)

- [ ] `GET /api/field/current/analytics/dashboard` returns correct values for test field
- [ ] Dashboard page renders 4 KPI cards with real data
- [ ] Date range filter changes data on dashboard
- [ ] WO XLSX report downloads with correct columns
- [ ] HSE PDF report has correct KPI table with regulatory references
- [ ] 11 analytics unit tests pass
- [ ] `dotnet build Beep.OilandGas.sln` exits 0 errors
