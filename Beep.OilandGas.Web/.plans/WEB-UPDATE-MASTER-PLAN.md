# Beep.OilandGas.Web — Update Master Plan

> Created: 2026-04-21  
> Scope: Web architecture cleanup, Pages vs Components consolidation, staged calculation-project adoption, and domain-project integration planning  
> Applies To: `Beep.OilandGas.Web`, `Beep.OilandGas.ApiService`, and the referenced domain projects behind the API  
> Relationship To Existing Plan: This is a follow-on modernization plan. It does not replace the earlier UX rollout notes; it sequences the cleanup and integration work needed after those pages were created.
> Scan basis: corrected against a broad repository scan covering routed Razor files, web services, API controllers, and representative service/model folders across the Beep.OilandGas solution, then grounded in one project document per repo-local `.csproj` under `Projects/`.

---

## Canonical Planning Baseline

Use the per-project knowledge base as the primary planning source:

- Entry point: `Projects/INDEX.md`
- Canonical project-level evidence: `Projects/Presentation/*`, `Projects/Core/*`, `Projects/Operations/*`, `Projects/FinanceAndSupport/*`, `Projects/Engineering/*`
- Supporting evidence only: `RepositoryScanSummary.md`, phase `ScanEvidence.md` files, and the earlier phase matrices

### Planning Rules Derived From The Project Docs

- Treat `Beep.OilandGas.Web`, `Beep.OilandGas.ApiService`, `Beep.OilandGas.Models`, `Beep.OilandGas.PPDM39`, `Beep.OilandGas.PPDM39.DataManagement`, and `Beep.OilandGas.LifeCycle` as architectural anchor projects.
- Treat `ProspectIdentification`, `LeaseAcquisition`, and `Decommissioning` as stronger operational slices than `ProductionOperations`, `EnhancedRecovery`, and `DrillingAndConstruction`.
- Treat `Accounting`, `ProductionAccounting`, and `PermitsAndApplications` as service-rich domains that still need UI/API surfacing decisions in phase 9.
- Treat `DCA`, `PumpPerformance`, `ChokeAnalysis`, `CompressorAnalysis`, and similar modules as technically present but not automatically productized; phase 7 and phase 10 must distinguish deferred surfacing from regressions.

## MudBlazor Implementation Prerequisite

Before implementing any web-plan task that changes `.razor`, layout, theme, dialog, grid, tabs, stepper, navigation, or shared-component files:

- start with `../MudBlazor_Docs/README.md`
- read the matching local component docs before editing markup or parameters
- use the repo UI rules and local docs together; do not guess MudBlazor parameters when a local doc exists
- treat shell/provider changes as high-risk and read `Layouts.txt`, `Theme.txt`, `Services.txt`, `AppBar.txt`, `Drawer.txt`, and `PopOver.txt` first

## Short Answer: Is `Components` + `Pages` Duplication?

Not by default.

- `Pages/` should contain routable entry surfaces with `@page` directives.
- `Components/` should contain reusable UI units: dialogs, cards, charts, selectors, layouts, timelines, and feature widgets.

In the current web project, the structure is not purely duplicated, but it has drifted into overlap and ambiguity.

### Confirmed Duplication / Overlap Risks

| Area | Current Evidence | Why It Is a Problem | Target Decision |
|------|------------------|---------------------|-----------------|
| App shell | `App.razor` and `Components/App.razor` both exist | Two entry-point patterns confuse routing ownership | Keep root `App.razor` + one routed shell (`Components/Routes.razor`) |
| Shared KPI card | `Components/Shared/KpiCard.razor` and `Components/Dashboard/KpiCard.razor` | Same concept, different parameter models, inconsistent reuse | Standardize on one shared KPI card |
| Routed content under `Components/` | `Components/Account/Pages/*` and `Components/Pages/Account/Dashboard.razor` contain `@page` routes | Makes it unclear which folder owns routable surfaces | Allow `Components/Account/Pages` as Identity exception only; move custom routed pages to `Pages/` |
| Data pages | `Pages/Data/*` and `Pages/PPDM39/Data/*` both exist | Legacy and PPDM-specific admin pages are split without a clear boundary | Consolidate under one admin/data route strategy |
| Setup pages | `Pages/PPDM39/CreateDatabaseWizard.razor` and `Pages/PPDM39/Setup/DatabaseSetupWizard.razor` coexist | Competing setup flows create support debt | Keep one canonical setup wizard and retire the other |
| Production forecasting pages | `Pages/PPDM39/Production/Forecasts.razor` and `Pages/PPDM39/Production/ProductionForecasting.razor` both exist | Same business space with unclear responsibility split | Consolidate into one forecasting workbench |
| Integration style | Some pages use typed HTTP clients; some use `IBeepOilandGasApp` directly | Web-to-domain boundaries are inconsistent | Standardize on Web -> typed client -> API -> domain service |

### Folder Rule Going Forward

| Folder | Owns | Does Not Own |
|--------|------|--------------|
| `Pages/` | Routes, page composition, page-level state, page authorization | Dialog-only fragments, shared widgets, cards |
| `Components/Shared/` | Cross-domain reusable components | Domain-specific screens |
| `Components/<Feature>/` | Domain-specific reusable widgets used by multiple pages | Top-level routable screens |
| `Services/` | Typed API clients, view-specific orchestration, state adapters | Domain calculations or persistence logic |

---

## Architecture Direction

The target architecture remains the same as the repository rules already describe:

```
Web (Blazor) -> ApiService (HTTP boundary) -> Domain Projects / PPDM39 / LifeCycle Services
```

### Web-Layer Rules

1. `Beep.OilandGas.Web` should prefer typed HTTP clients over direct use of domain libraries.
2. Pages should consume focused typed clients such as `ICalculationServiceClient`, `IOperationsServiceClient`, `IPropertiesServiceClient`, `IPumpServiceClient`, `ILifeCycleService`, `IBusinessProcessServiceClient`, `IComplianceServiceClient`, `IHSEServiceClient`, or additional typed clients introduced by this plan. When an accounting page already has a stable shared contract and direct `ApiClient` seam, do not keep a duplicate web-only accounting client.
3. Direct `IBeepOilandGasApp` usage is treated as transitional compatibility, not the long-term page pattern.
4. Direct `ApiService` project references from the web project should be retired over time.
5. Domain project references in the web project should be reduced to shared contracts/models only when practical.

### Current-State Findings That Drive This Plan

- The web project currently has a healthy typed-client layer in `Services/`, but not all pages use it.
- `EconomicAnalysis.razor` still calls `BeepApp.Calculations.*` directly instead of going through `ICalculationServiceClient`.
- `Program.cs` already registers focused typed web clients for calculations, operations, properties, compliance, business process, HSE, lifecycle, pumps, and the other active page route families.
- `Beep.OilandGas.Web.csproj` still references `Beep.OilandGas.ApiService` plus most domain projects directly.
- Several engineering libraries have API controllers but no dedicated typed client methods or pages.
- Several libraries exist in the solution with no first-class web vertical slice yet.

### Scan-Based Corrections

- The duplicate data/admin route problem is real: both `Pages/Data/*` and `Pages/PPDM39/Data/*` contain live routes.
- Work-order ownership is genuinely split between web pages, `Controllers/WorkOrder/WorkOrderController.cs`, `Controllers/LifeCycle/WorkOrderController.cs`, and `Beep.OilandGas.LifeCycle/Services/WorkOrder/*`.
- `PermitsAndApplications` is presently validation-heavy and PDF-support oriented, with no comparable first-class permit controller family or web page set.
- `ProductionOperations`, `EnhancedRecovery`, and `DrillingAndConstruction` are materially thinner than `ProspectIdentification`, `LeaseAcquisition`, `Decommissioning`, `Accounting`, `ProductionAccounting`, and `PPDM39.DataManagement`.
- Finance domains are service-rich but UI-thin, so later phases must include surfacing work, not only cleanup.

---

## Calculation Project Placement Matrix

The web project should expose calculation projects as workbenches or embedded engineering panels, never as raw library calls from pages.

| Project | Current Surface | Recommended Web Surface | Integration Rule | Planned Phase |
|---------|-----------------|-------------------------|------------------|---------------|
| `Beep.OilandGas.EconomicAnalysis` | API controller + page, but page still uses `BeepApp` | `/economics/evaluation`, AFE justification, intervention NPV side panels | Migrate page to `ICalculationServiceClient`; API stays orchestration layer | Phase 7 |
| `Beep.OilandGas.ProductionForecasting` | API controller + page/client | `/production/forecasting`, feed economics scenarios | Keep via typed client and version forecast results | Phase 7 |
| `Beep.OilandGas.NodalAnalysis` | API controller + page/client | `/production/well-performance`, `/ppdm39/calculations/nodalanalysis` | Keep via typed client; add shared well selector and result cards | Phase 7 |
| `Beep.OilandGas.GasLift` | API controller + page/client | Gas lift optimisation and artificial-lift recommendations | Keep via typed client; connect to well performance workflow | Phase 7 |
| `Beep.OilandGas.PipelineAnalysis` | API controller + page/client | Pipeline / flow assurance workbench | Keep via typed client; connect to operations constraints pages | Phase 7 |
| `Beep.OilandGas.FlashCalculations` | API controller only | PVT / separator flash page and facility simulation panel | Add typed client + dedicated page before direct use | Phase 7 |
| `Beep.OilandGas.DCA` | No dedicated web/API vertical slice | DCA tab inside well performance and production forecasting | Add API surface, typed client methods, shared charting components | Phase 7 |
| `Beep.OilandGas.GasProperties` | API controller only | Gas properties page + embedded calculator in gas lift/pipeline/flash | Extend `IPropertiesServiceClient`; avoid direct library calls in pages | Phase 7 |
| `Beep.OilandGas.OilProperties` | API controller only | Oil properties page + embedded calculator in reservoir/economics | Extend `IPropertiesServiceClient` | Phase 7 |
| `Beep.OilandGas.HeatMap` | API controller + page/client | Reservoir, production, and spatial surveillance heat maps | Keep via properties client; expose shared map/overlay components | Phase 7 |
| `Beep.OilandGas.ChokeAnalysis` | No web/API surface | Choke sizing page from well performance and production tuning flows | Add API controller + typed client + page | Phase 7 |
| `Beep.OilandGas.CompressorAnalysis` | No web/API surface | Gas compression optimisation and facility operations page | Add API controller + typed client + page | Phase 7 |
| `Beep.OilandGas.WellTestAnalysis` | Legacy page space only, no dedicated controller | `/production/well-tests` and diagnostics tab in well performance | Add dedicated API or wrap through production endpoints | Phase 7 |
| `Beep.OilandGas.PumpPerformance` | No web/API surface | Pump selection / equipment sizing workbench | Add pump API vertical slice | Phase 7 |
| `Beep.OilandGas.SuckerRodPumping` | API controller + page | Pump design and optimisation | Keep via pump client | Phase 7 |
| `Beep.OilandGas.HydraulicPumps` | API controller + page | Hydraulic pump design page | Keep via pump client | Phase 7 |
| `Beep.OilandGas.PlungerLift` | API controller + page | Gas well plunger-lift optimisation | Keep via pump client | Phase 7 |

---

## Integration Map For The Rest Of The Domain Projects

| Project | Primary Web Use | Preferred Integration Seam | Planned Phase |
|---------|------------------|----------------------------|---------------|
| `Beep.OilandGas.LifeCycle` | field dashboard, work order, process dashboards, active-field context | `ILifeCycleService` + new typed workflow clients where needed | Phase 8 |
| `Beep.OilandGas.ProspectIdentification` | exploration dashboard, prospect board, screening, ranking | split or extend `IOperationsServiceClient` into exploration-focused client | Phase 8 |
| `Beep.OilandGas.DevelopmentPlanning` | FDP, well design, facilities, construction progress | typed development client + field-scoped API endpoints | Phase 8 |
| `Beep.OilandGas.ProductionOperations` | daily ops, well status, interventions, deferments | typed production client + field-scoped controllers | Phase 8 |
| `Beep.OilandGas.Decommissioning` | abandonment, P&A workflow, facility decommissioning, cost estimation | field-scoped typed decommissioning client | Phase 8 |
| `Beep.OilandGas.LeaseAcquisition` | lease acquisition, due diligence, land / rights pages | operations client or dedicated lease client | Phase 8 |
| `Beep.OilandGas.EnhancedRecovery` | EOR screening, pilot proposal, injection workflows | operations client + reservoir integration | Phase 8 |
| `Beep.OilandGas.ProductionAccounting` | production accounting dashboard, royalties, reconciliation | dedicated accounting client, linked to field/work-order context | Phase 9 |
| `Beep.OilandGas.Accounting` | general accounting, allocation, AFE, reporting, financial controls | dedicated accounting and finance page set behind typed clients | Phase 9 |
| `Beep.OilandGas.PermitsAndApplications` | compliance calendar, permit packets, regulatory submissions | dedicated permits/compliance client | Phase 9 |
| `Beep.OilandGas.PPDM39.DataManagement` and `Beep.OilandGas.PPDM39` | setup, schema, metadata, table management, LOV/reference data | admin/data clients only, kept separate from engineer workflows | Phase 9 |
| `Beep.OilandGas.Drawing` | diagrams and visualisation components | reusable shared visual components, not page-level business logic | Phase 9 |

---

## Phase Sequence

| Phase | Name | Outcome |
|------|------|---------|
| [Phase 6](Phase6-WebArchitectureConsolidation/README.md) | Web Architecture Consolidation | Clear ownership for Pages vs Components, canonical shells, reduced duplication, integration standards |
| [Phase 7](Phase7-CalculationSurfaceIntegration/README.md) | Calculation Surface Integration | All engineering/calculation libraries surface through consistent API clients and workbenches |
| [Phase 8](Phase8-OperationsAndLifecycleIntegration/README.md) | Operations and Lifecycle Integration | Exploration, development, production, lease, EOR, and decommissioning flows align to domain services through whole-solution passes |
| [Phase 9](Phase9-DataAdminAndWorkflowIntegration/README.md) | Data, Admin, and Workflow Integration | PPDM39 data tools, compliance, permits, access control, accounting, and business-process pages are rationalized through whole-solution passes |
| [Phase 10](Phase10-HardeningAndRetirement/README.md) | Hardening and Retirement | Validation, route cleanup, dependency reduction, and removal of legacy pages/components across the full solution |
| [Phase 11](Phase11-IdentityPersonaAndAccessControl/README.md) | Identity, Persona, and Access Governance | Role-and-persona driven user experience, least-privilege access controls, profile lifecycle governance, and auditable authorization boundaries across web/API/data seams |

Phase 11 implementation starts with a required data model and migration baseline:

- `Phase11-IdentityPersonaAndAccessControl/DataModel-And-Migration.md`

Each phase is executed in multiple passes:

1. Pass A: inventory, classify, and freeze current seams
2. Pass B: add or migrate the canonical slice
3. Pass C: validate, route-swap, and retire the old path

---

## Detailed Phase Folders

Each phase now has its own subfolder with a phase README and one pass document per execution pass.

| Phase | Overview | Detailed Folder |
|------|----------|-----------------|
| 6 | `Phase6-WebArchitectureConsolidation.md` | `Phase6-WebArchitectureConsolidation/` |
| 7 | `Phase7-CalculationSurfaceIntegration.md` | `Phase7-CalculationSurfaceIntegration/` |
| 8 | `Phase8-OperationsAndLifecycleIntegration.md` | `Phase8-OperationsAndLifecycleIntegration/` |
| 9 | `Phase9-DataAdminAndWorkflowIntegration.md` | `Phase9-DataAdminAndWorkflowIntegration/` |
| 10 | `Phase10-HardeningAndRetirement.md` | `Phase10-HardeningAndRetirement/` |
| 11 | `Phase11-IdentityPersonaAndAccessControl.md` | `Phase11-IdentityPersonaAndAccessControl/` |

## Document Index

- `RepositoryScanSummary.md`
- `Projects/INDEX.md`
- `WEB-UPDATE-TODO.md`
- `Phase6-WebArchitectureConsolidation.md`
- `Phase6-WebArchitectureConsolidation/README.md`
- `Phase7-CalculationSurfaceIntegration.md`
- `Phase7-CalculationSurfaceIntegration/README.md`
- `Phase8-OperationsAndLifecycleIntegration.md`
- `Phase8-OperationsAndLifecycleIntegration/README.md`
- `Phase9-DataAdminAndWorkflowIntegration.md`
- `Phase9-DataAdminAndWorkflowIntegration/README.md`
- `Phase10-HardeningAndRetirement.md`
- `Phase10-HardeningAndRetirement/README.md`
- `Phase11-IdentityPersonaAndAccessControl.md`
- `Phase11-IdentityPersonaAndAccessControl/README.md`
- `Phase11-IdentityPersonaAndAccessControl/DataModel-And-Migration.md`

---

## Definition Of Done For This Update Stream

- Pages are clearly separated from reusable components
- One canonical app shell and one canonical KPI card remain
- All calculation pages call typed web clients, not direct app/domain abstractions
- Every integrated domain project has a documented API seam and web owner
- Legacy overlapping pages are either retired or explicitly marked as compatibility paths
- The web project can build without relying on `Beep.OilandGas.ApiService` as a direct runtime dependency
