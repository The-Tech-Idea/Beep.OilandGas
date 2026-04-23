# Phase 10 Final Route and Integration Matrix

> Published for W10-05
> Date: 2026-04-23
> Scope: canonical web routes, typed client ownership, workflow integration seams, compatibility redirects, and final direct web dependency set.

## Canonical Runtime Route Matrix

| Area | Canonical route prefix or entry | Primary page anchor | Web seam owner | Notes |
|------|----------------------------------|---------------------|----------------|-------|
| Landing | `/` and `/landing` | `Pages/Landing.razor` | `IIdentityServiceClient`, `IPersonaContextService`, `INavigationPolicyService` | `/` now has a single owner; persona-aware landing redirects to default route or `/dashboard`. |
| Welcome | `/welcome` | `Pages/Index.razor` | none | Legacy marketing-style landing retained as non-canonical informational entry. |
| Dashboard | `/dashboard` | `Pages/Dashboard.razor` | `IPersonaContextService` | Persona-aware workflow entry surface. |
| Exploration | `/exploration*` | `Pages/PPDM39/Exploration/ExplorationDashboard.razor` plus prospect, seismic, and well-program pages | `IExplorationServiceClient` | Canonical engineer workflow area. |
| Development | `/development*` | `Pages/PPDM39/Development/DevDashboard.razor` plus FDP, well design, and construction pages | `IDevelopmentServiceClient`, `IDrillingServiceClient` | Development routes own well design and construction handoff. |
| Production | `/production*` | `Pages/PPDM39/Production/ProductionDashboard.razor` plus optimizer, intervention, allocation, and forecasting pages | `IProductionServiceClient`, `ICalculationServiceClient` | Calculations remain routed through focused calculation seam where applicable. |
| Reservoir | `/reservoir*` | `Pages/PPDM39/Reservoir/ReservoirOverview.razor` plus characterization, EOR, and reserves pages | `IPropertiesServiceClient`, `IEnhancedRecoveryServiceClient`, `ICalculationServiceClient` | Reservoir stays separate from raw PPDM admin pages. |
| Economics | `/economics/*` | `Pages/PPDM39/Economics/AFEManagement.razor`, `Pages/PPDM39/Economics/EconomicEvaluation.razor` | `IAfeServiceClient`, `IAccountingServiceClient` | Finance-facing workflow entry points. |
| Business processes | `/ppdm39/process*` | `Pages/PPDM39/BusinessProcess/ProcessDashboard.razor` and related process/gate/instance pages | `IBusinessProcessServiceClient` | Lifecycle/process orchestration stays under PPDM39 process route tree. |
| Compliance | `/ppdm39/compliance*` | `Pages/PPDM39/Compliance/ComplianceDashboard.razor` and obligations/report pages | `IComplianceServiceClient` | Canonical compliance dashboard and obligations tree. |
| HSE operations | `/hse/*` | `Pages/PPDM39/HSE/IncidentManagement.razor`, `Pages/PPDM39/HSE/ComplianceCalendar.razor` | `IHSEServiceClient` | HSE operational entry routes remain short-form and distinct from PPDM compliance dashboard routes. |
| Decommissioning | `/decommissioning/*` | `Pages/PPDM39/Decommissioning/WellPAWorkflow.razor`, `Pages/PPDM39/Decommissioning/FacilityDecommissioning.razor` | `IDecommissioningServiceClient`, `IComplianceServiceClient`, `IAfeServiceClient` | Preserved as explicit lifecycle closeout workflow. |
| Work orders | `/ppdm39/workorder*` | `Pages/PPDM39/WorkOrder/WorkOrderDashboard.razor` and related detail/calendar pages | `IWorkOrderServiceClient` | Canonical work-order route tree. |
| Data management | `/ppdm39/data-management*` | `Pages/PPDM39/DataManagementHub.razor` and table/schema/business-associate subpages | `IDataManagementService`, `IConnectionService` | Raw PPDM data/admin surface remains isolated from engineer workflow navigation. |
| Setup | `/ppdm39/setup` | `Pages/PPDM39/Setup/DatabaseSetupWizard.razor` | `IDataManagementService`, `IFirstRunService` | Canonical first-run and database setup entry route. |
| Account/auth pages | `/Account/*`, `/login`, `/register`, `/access-denied` | `Components/Account/Pages/*` | ASP.NET Core Identity area + account layout | Separate account shell retained. |

## Final Workflow and Client Integration Matrix

| Workflow slice | Primary routes | Primary web client or scoped service | Primary API/domain seam | Current state |
|----------------|---------------|--------------------------------------|-------------------------|---------------|
| Identity and persona | `/`, `/landing`, account routes | `IIdentityServiceClient`, `IPersonaContextService`, `INavigationPolicyService` | identity/profile and role assignment APIs | Canonical and active. |
| Exploration | `/exploration`, `/exploration/prospect-board`, `/exploration/prospect/*`, `/exploration/seismic`, `/exploration/well-program*` | `IExplorationServiceClient` | field exploration controller and exploration domain services | Canonical and active. |
| Development | `/development`, `/development/fdp*`, `/development/well-design*`, `/development/construction` | `IDevelopmentServiceClient` | field development controller and development planning domain | Canonical and active. |
| Drilling/construction handoff | development-driven routes plus drilling operations support | `IDrillingServiceClient` | field drilling controller and drilling/construction domain | Active through development workflow handoff. |
| Production operations | `/production`, `/production/well-performance`, `/production/intervention`, `/production/allocation` | `IProductionServiceClient` | field production controller and production operations domain | Canonical and active. |
| Calculation workbenches | `/analysis/*`, selected `/production/forecasting`, reservoir properties routes | `ICalculationServiceClient`, `IPumpServiceClient`, `IPropertiesServiceClient` | calculation and engineering support domains | Active; route ownership remains in web while service ownership is focused. |
| Reservoir and EOR | `/reservoir`, `/reservoir/characterization`, `/reservoir/eor`, `/reservoir/reserves` | `IEnhancedRecoveryServiceClient`, `IPropertiesServiceClient` | reservoir/EOR services plus supporting calculation seams | Canonical with some maturity gaps preserved as domain-level follow-up. |
| Economics and AFE | `/economics/afe`, `/economics/evaluation`, `/accounting/production` | `IAfeServiceClient`, `IAccountingServiceClient` | accounting and production accounting services | Canonical and active. |
| Business processes | `/ppdm39/process*` | `IBusinessProcessServiceClient` | lifecycle/process services | Canonical and active. |
| Compliance and HSE | `/ppdm39/compliance*`, `/hse/*` | `IComplianceServiceClient`, `IHSEServiceClient` | compliance and HSE controllers/services | Canonical split preserved intentionally. |
| Lease acquisition | linked from workflow handoffs | `ILeaseServiceClient` | lease acquisition domain services | Active as support seam, not top-level nav owner. |
| Work orders | `/ppdm39/workorder*` | `IWorkOrderServiceClient` | work-order controller/services | Canonical and active. |
| Data administration | `/ppdm39/data-management*`, `/ppdm39/setup*` | `IDataManagementService`, `IConnectionService` | PPDM39 data-management services | Canonical and active. |

## Compatibility and Redirect Matrix

| Transitional route | Current behavior | Canonical replacement | Reason |
|--------------------|------------------|-----------------------|--------|
| `/data/database-setup` | Redirects with `replace: true` | `/ppdm39/setup` | Retained for backward compatibility after setup-route consolidation. |
| `/ppdm39/database-setup` | Redirects with `replace: true` | `/ppdm39/setup` | Retained for backward compatibility after setup-route consolidation. |
| `/counter` | Redirects with `replace: true` | `/dashboard` | Old template route retained as harmless compatibility stub. |
| `/fetchdata` | Redirects with `replace: true` | `/dashboard` | Old template route retained as harmless compatibility stub. |
| `/welcome` | Standalone page | `/` or `/landing` for runtime entry | Non-canonical informational entry after root-route ownership cleanup. |

## Route Governance Results Relevant to Phase 10

| Check | Result |
|------|--------|
| Router default layout | `MainLayout` via `Components/Routes.razor` |
| Runtime layout layers | Canonical runtime shell is `MainLayout`; account routes use `AccountLayout` |
| Duplicate root owner | Resolved; `/` is owned only by `Pages/Landing.razor` |
| Nav links without a backing page | `0` after legacy hidden nav retirement and HAZOP route completion |
| Deprecated setup callers | Repointed to `/ppdm39/setup` |

## Final Direct Web Dependency Matrix

| Direct project reference retained | Reason retained |
|----------------------------------|-----------------|
| `Beep.Foundation.IdentityServer.Shared` | Shared identity UI/integration dependency outside this repo. |
| `Beep.OilandGas.Drawing` | Still required by drawing sample host pages; build remains blocked only by pre-existing drawing-sample issues. |
| `Beep.OilandGas.HeatMap` | Direct namespace usage from retained heat map page. |
| `Beep.OilandGas.LifeCycle` | Direct process/workflow model usage in retained business-process components and clients. |
| `Beep.OilandGas.Models` | Primary shared contract dependency across web pages and clients. |
| `Beep.OilandGas.UserManagement` | Explicitly required for persona and identity models; replaces accidental transitive dependency through ApiService. |
| `Beep.OilandGas.PPDM39.DataManagement` | Direct data-management, metadata, and well-status types used in web. |
| `Beep.OilandGas.Client` | Shared client abstraction layer still used by the web project. |
| `Beep.OilandGas.PPDM39` | Core PPDM entities and contracts used directly in web. |
| `Beep.OilandGas.Branchs` | Still required by PPDM tree/navigation components. |

## Removed Direct Web References in Phase 10

The following direct web references were removed because no `.cs` or `.razor` usage remained in the web project after client-boundary consolidation:

- `Beep.OilandGas.ApiService`
- `Beep.OilandGas.ChokeAnalysis`
- `Beep.OilandGas.CompressorAnalysis`
- `Beep.OilandGas.DCA`
- `Beep.OilandGas.Decommissioning`
- `Beep.OilandGas.DevelopmentPlanning`
- `Beep.OilandGas.DrillingAndConstruction`
- `Beep.OilandGas.EconomicAnalysis`
- `Beep.OilandGas.EnhancedRecovery`
- `Beep.OilandGas.FlashCalculations`
- `Beep.OilandGas.GasLift`
- `Beep.OilandGas.GasProperties`
- `Beep.OilandGas.HydraulicPumps`
- `Beep.OilandGas.LeaseAcquisition`
- `Beep.OilandGas.NodalAnalysis`
- `Beep.OilandGas.OilProperties`
- `Beep.OilandGas.PermitsAndApplications`
- `Beep.OilandGas.PipelineAnalysis`
- `Beep.OilandGas.PlungerLift`
- `Beep.OilandGas.ProductionAccounting`
- `Beep.OilandGas.ProductionForecasting`
- `Beep.OilandGas.ProductionOperations`
- `Beep.OilandGas.ProspectIdentification`
- `Beep.OilandGas.PumpPerformance`
- `Beep.OilandGas.SuckerRodPumping`
- `Beep.OilandGas.WellTestAnalysis`

## Known Explicit Exception

| Exception | Status | Note |
|-----------|--------|------|
| `Pages/PPDM39/DataManagement/DrawingSampleHost.razor.cs` drawing types | Deferred blocker | Web build still fails only on pre-existing drawing sample namespace/type issues under `Beep.OilandGas.Drawing`; this remains explicit technical debt, not a Phase 10 regression. |
