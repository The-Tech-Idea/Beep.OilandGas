# Beep Oil & Gas — UX Business Process Implementation: MASTER PLAN

> **Created**: 2026-04-18  
> **Scope**: First-run setup wizard + main app menu redesign + petroleum engineer business process pages  
> **Reference**: `Plans/BusinessProcessesPlan/PetroleumEngineerBusinessProcesses.md`  
> **Guidelines**: `Plans/UI-UX-OilAndGas-Guidelines.md`  
> **PPDM Data Management**: PPDMTreeView is KEPT as-is for data managers (not in scope here)

---

## Strategic Objective

Transform Beep Oil & Gas from a **data management tool** into a **petroleum engineer's work platform**. Engineers should recognize their daily work processes in the UI, not database tables.

---

## Phase Summary

| Phase | Name | Focus | Status | Est. Duration |
|-------|------|-------|--------|---------------|
| [Phase 0](Phase0-DatabaseSetupAndSeeding.md) | Database Setup & Seeding | First-run wizard, reference data seeding, dummy data generator | 🔲 Not Started | 1 week |
| [Phase 1](Phase1-Foundation.md) | Foundation & Navigation | NavMenu, guidelines, SVG icons, base patterns | 🔲 Not Started | 1 week |
| [Phase 2](Phase2-ExplorationUX.md) | Exploration Business Process | Prospect board, well program approval | 🔲 Not Started | 2 weeks |
| [Phase 3](Phase3-DevelopmentUX.md) | Development Business Process | FDP wizard, well design, construction | 🔲 Not Started | 2 weeks |
| [Phase 4](Phase4-ProductionUX.md) | Production Business Process | Daily ops, well optimizer, allocation | 🔲 Not Started | 3 weeks |
| [Phase 5](Phase5-ReservoirAndEconomics.md) | Reservoir & Economics | Reservoir summary, reserves, economics | 🔲 Not Started | 2 weeks |

---

## Cross-Cutting Rules (Apply to ALL Phases)

1. **SVG only** — no new PNG/JPG icons or images
2. **Process-first** — every page simulates a business process, not a CRUD table
3. **Context always visible** — active field name on every page
4. **MudBlazor** — all UI uses MudBlazor components; no inline styles
5. **PPDMTreeView stays separate** — data management is not mixed with engineer UX
6. **One business domain per page** — split if page title contains "and"

---

## Todo Master Tracker

### Phase 0 — Database Setup & Seeding *(must complete before Phase 1)*

#### 0.1 First-Run Database Setup Wizard
| ID | Task | File | Status |
|----|------|------|--------|
| 0.1.a | Create `FirstRunService.cs` (LocalStorage + API status check) | `Services/FirstRunService.cs` | 🔲 |
| 0.1.b | Register `FirstRunService` in Web `Program.cs` | `Program.cs` | 🔲 |
| 0.1.c | Add first-run redirect to `DefaultLayout.razor` | `Components/Layout/DefaultLayout.razor` | 🔲 |
| 0.1.d | Create `DatabaseSetupWizard.razor` — Step 1 DB connection form (all DB types, fields per type) | `Pages/PPDM39/Setup/DatabaseSetupWizard.razor` | 🔲 |
| 0.1.e | Add Step 1B inline SQLite creation (collapsible fallback link, auto-fills form on success) | same | 🔲 |
| 0.1.f | Add Step 2 — Schema check + create missing tables | same | 🔲 |
| 0.1.g | Add Step 3 — Summary + navigate to seeding | same | 🔲 |
| 0.1.h | Add `POST /api/ppdm39/setup/test-connection` endpoint | `Controllers/PPDM39/PPDM39SetupController.cs` | 🔲 |
| 0.1.i | Add `POST /api/ppdm39/setup/create-sqlite` endpoint | same | 🔲 |
| 0.1.j | Add `GET /api/ppdm39/setup/schema-check` endpoint | same | 🔲 |
| 0.1.k | Add `POST /api/ppdm39/setup/create-schema` endpoint | same | 🔲 |
| 0.1.l | Add `GET /api/ppdm39/setup/status` endpoint | same | 🔲 |
| 0.1.m | Add `/ppdm39/setup` link in NavMenu Data Management section | `Shared/NavMenu.razor` | 🔲 |

#### 0.2 Reference Data Seeding Pages
| ID | Task | File | Status |
|----|------|------|--------|
| 0.2.a | Create `SeedSetupPage.razor` at `/ppdm39/setup/seed` | `Pages/PPDM39/Setup/SeedSetupPage.razor` | 🔲 |
| 0.2.b | Add 3 seeding cards (WSC v3 Facets, Enum Data, Field/Facility Data) | same | 🔲 |
| 0.2.c | Add Seed All button + per-card progress tracking | same | 🔲 |
| 0.2.d | Add Continue to Dummy Data / Skip navigation | same | 🔲 |
| 0.2.e | Wire `SeedSetupPage` into wizard Step 3 Summary navigation | `Pages/PPDM39/Setup/DatabaseSetupWizard.razor` | 🔲 |

#### 0.3 Dummy Data Generator
| ID | Task | File | Status |
|----|------|------|--------|
| 0.3.a | Create `DummyDataGeneratorOptions.cs` (options + result DTOs) | `SeedData/DummyData/DummyDataGeneratorOptions.cs` | 🔲 |
| 0.3.b | Create `PPDM39DummyDataGenerator.cs` — constructor + `GenerateAllAsync` + ref-data loader | `SeedData/DummyData/PPDM39DummyDataGenerator.cs` | 🔲 |
| 0.3.c | Create `.Fields.cs` partial — `GenerateFieldsAsync` + `GeneratePoolsAsync` | `SeedData/DummyData/PPDM39DummyDataGenerator.Fields.cs` | 🔲 |
| 0.3.d | Create `.Wells.cs` partial — `GenerateWellsAsync` (WellServices + initializeDefaultStatuses) | `SeedData/DummyData/PPDM39DummyDataGenerator.Wells.cs` | 🔲 |
| 0.3.e | Create `.Tests.cs` partial — `GenerateWellTestsAsync` | `SeedData/DummyData/PPDM39DummyDataGenerator.Tests.cs` | 🔲 |
| 0.3.f | Create `.Facilities.cs` partial — `GenerateFacilitiesAsync` | `SeedData/DummyData/PPDM39DummyDataGenerator.Facilities.cs` | 🔲 |
| 0.3.g | Create `.Production.cs` partial — `GenerateProductionAsync` (PDEN_VOL_SUMMARY) | `SeedData/DummyData/PPDM39DummyDataGenerator.Production.cs` | 🔲 |
| 0.3.h | Create `.Activities.cs` partial — `GenerateActivitiesAsync` (WELL_ACTIVITY) | `SeedData/DummyData/PPDM39DummyDataGenerator.Activities.cs` | 🔲 |
| 0.3.i | Register `PPDM39DummyDataGenerator` in ApiService `Program.cs` | `Beep.OilandGas.ApiService/Program.cs` | 🔲 |
| 0.3.j | Add `POST /api/ppdm39/setup/generate-dummy-data` endpoint | `Controllers/PPDM39/PPDM39SetupController.cs` | 🔲 |
| 0.3.k | Add `GET /api/ppdm39/setup/dummy-data/status` endpoint | same | 🔲 |
| 0.3.l | Add `DELETE /api/ppdm39/setup/dummy-data` endpoint | same | 🔲 |
| 0.3.m | Create `DummyDataPage.razor` at `/ppdm39/setup/dummy-data` | `Pages/PPDM39/Setup/DummyDataPage.razor` | 🔲 |
| 0.3.n | Wire `DummyDataPage` into `SeedSetupPage` Continue navigation | `Pages/PPDM39/Setup/SeedSetupPage.razor` | 🔲 |

### Phase 1 — Foundation & Navigation
| ID | Task | File | Status |
|----|------|------|--------|
| 1.1 | Redesign NavMenu to process-centric structure | `Shared/NavMenu.razor` | 🔲 |
| 1.2 | Create SVG icons for O&G concepts (6 core) | `wwwroot/imgs/icons/process/` | 🔲 |
| 1.3 | Add UI/UX rules to copilot-instructions.md | `.github/copilot-instructions.md` | 🔲 |
| 1.4 | Create KpiCard shared component | `Components/Shared/KpiCard.razor` | 🔲 |
| 1.5 | Create StatusBadge shared component | `Components/Shared/StatusBadge.razor` | 🔲 |
| 1.6 | Create ProcessTimeline shared component | `Components/Shared/ProcessTimeline.razor` | 🔲 |
| 1.7 | Update Field Dashboard with new KPI layout | `Pages/PPDM39/FieldDashboard.razor` | 🔲 |

### Phase 2 — Exploration UX
| ID | Task | File | Status |
|----|------|------|--------|
| 2.1 | Exploration Dashboard page | `Pages/PPDM39/Exploration/Dashboard.razor` | 🔲 |
| 2.2 | Prospect Maturation Board (kanban) | `Pages/PPDM39/Exploration/ProspectBoard.razor` | 🔲 |
| 2.3 | Prospect Detail / Screening page | `Pages/PPDM39/Exploration/ProspectDetail.razor` | 🔲 |
| 2.4 | Well Program Approval wizard | `Pages/PPDM39/Exploration/WellProgramApproval.razor` | 🔲 |
| 2.5 | Seismic Survey Tracker | `Pages/PPDM39/Exploration/SeismicTracker.razor` | 🔲 |
| 2.6 | ProspectCard kanban card component | `Components/Exploration/ProspectCard.razor` | 🔲 |
| 2.7 | Update NavMenu Exploration sub-items | `Shared/NavMenu.razor` | 🔲 |

### Phase 3 — Development UX
| ID | Task | File | Status |
|----|------|------|--------|
| 3.1 | Development Dashboard page | `Pages/PPDM39/Development/Dashboard.razor` | 🔲 |
| 3.2 | FDP Wizard (5-step) | `Pages/PPDM39/Development/FDPWizard.razor` | 🔲 |
| 3.3 | Development Well Design workflow | `Pages/PPDM39/Development/WellDesign.razor` | 🔲 |
| 3.4 | Construction Progress Tracker | `Pages/PPDM39/Development/ConstructionProgress.razor` | 🔲 |
| 3.5 | Project Milestone component | `Components/Development/MilestoneTracker.razor` | 🔲 |
| 3.6 | Update NavMenu Development sub-items | `Shared/NavMenu.razor` | 🔲 |

### Phase 4 — Production UX
| ID | Task | File | Status |
|----|------|------|--------|
| 4.1 | Production Dashboard / Daily Ops Center | `Pages/PPDM39/Production/DailyOpsCenter.razor` | 🔲 |
| 4.2 | Well Performance Optimizer | `Pages/PPDM39/Production/WellPerformance.razor` | 🔲 |
| 4.3 | Well Intervention Decision Tool | `Pages/PPDM39/Production/InterventionDecision.razor` | 🔲 |
| 4.4 | Production Allocation Workbench | `Pages/PPDM39/Production/AllocationWorkbench.razor` | 🔲 |
| 4.5 | Production Forecasting Workbench | `Pages/PPDM39/Production/ForecastingWorkbench.razor` | 🔲 |
| 4.6 | WellStatusCard component | `Components/Production/WellStatusCard.razor` | 🔲 |
| 4.7 | DefermentLogger component | `Components/Production/DefermentLogger.razor` | 🔲 |
| 4.8 | Update NavMenu Production sub-items | `Shared/NavMenu.razor` | 🔲 |

### Phase 5 — Reservoir & Economics
| ID | Task | File | Status |
|----|------|------|--------|
| 5.1 | Reservoir Characterization Summary | `Pages/PPDM39/Reservoir/CharacterizationSummary.razor` | 🔲 |
| 5.2 | EOR Screening Tool | `Pages/PPDM39/Reservoir/EORScreening.razor` | 🔲 |
| 5.3 | Reserves Classification page | `Pages/PPDM39/Reservoir/ReservesClassification.razor` | 🔲 |
| 5.4 | AFE Management page | `Pages/PPDM39/Economics/AFEManagement.razor` | 🔲 |
| 5.5 | Economic Evaluation Workbench | `Pages/PPDM39/Economics/EconomicEvaluation.razor` | 🔲 |
| 5.6 | ReservesChart component | `Components/Reservoir/ReservesChart.razor` | 🔲 |
| 5.7 | Update NavMenu Reservoir & Economics sub-items | `Shared/NavMenu.razor` | 🔲 |

---

## Dependency Graph

```
Phase 0 (Database Setup & Seeding)
    ↓
Phase 1 (Foundation)
    ↓
Phase 2 (Exploration)  ──┐
Phase 3 (Development)  ──┤  can run in parallel after Phase 1
Phase 4 (Production)   ──┤
    ↓                    │
Phase 5 (Reservoir)    ──┘  requires Phase 4 patterns
```

---

## Definition of Done (Each Phase)

- [ ] All pages build without errors (`dotnet build`)
- [ ] No inline `style=""` attributes introduced
- [ ] No PNG images introduced (SVG only for new assets)
- [ ] Every page has a visible active-field context indicator
- [ ] Every page has at least one "next action" button
- [ ] NavMenu updated to reflect new routes
- [ ] Phase plan file updated with ✅ on completed tasks
