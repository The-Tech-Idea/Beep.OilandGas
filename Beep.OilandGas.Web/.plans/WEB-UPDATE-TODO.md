# Beep.OilandGas.Web — Update Todo Tracker

> Tracking scope: web architecture cleanup + staged domain integration  
> Status legend: `Planned`, `In Progress`, `Blocked`, `Done`

---

## Planning Artifacts

| Item | Status | Notes |
|------|--------|-------|
| Repository scan summary | Done | `RepositoryScanSummary.md` captures the file-level findings used to correct phases 8-10 |
| Project knowledge base | Done | `Projects/INDEX.md` + one document per repo-local project now form the canonical planning baseline |
| MudBlazor local-doc baseline | Done | `../MudBlazor_Docs/README.md` is now referenced by agent instructions and the active web-plan docs before UI implementation |
| Phase 6 folder plan | Done | `Phase6-WebArchitectureConsolidation/README.md` + pass docs |
| Phase 7 folder plan | Done | `Phase7-CalculationSurfaceIntegration/README.md` + pass docs |
| Phase 8 folder plan | Done | `Phase8-OperationsAndLifecycleIntegration/README.md` + pass docs + whole-solution matrix |
| Phase 9 folder plan | Done | `Phase9-DataAdminAndWorkflowIntegration/README.md` + pass docs + whole-solution matrix |
| Phase 10 folder plan | Done | `Phase10-HardeningAndRetirement/README.md` + pass docs + whole-solution matrix |

---

## Stream Overview

| Phase | Name | Status | Notes |
|------|------|--------|-------|
| 6 | Web Architecture Consolidation | In Progress | Canonical shell path confirmed (`App.razor` + `Components/Routes.razor` + `Shared/MainLayout.razor`), duplicate dashboard KPI removed, custom account dashboard route moved into `Pages/`, and web-layer `IBeepOilandGasApp` usage has been retired in favor of focused typed/scoped services |
| 7 | Calculation Surface Integration | Done | Existing calculation pages now use the typed client path; DCA, Flash Calculations, Gas/Oil Properties, Well Test Analysis, Pump Performance, Choke Analysis, and Compressor Analysis are live vertical slices, reusable result-display components now back the shared KPI/summary surface, reusable comparison-grid shells now back the scenario comparison pages plus Compare Wells, reusable optimization-summary components now back Pump Performance baseline-vs-optimized deltas, and shared lift-recommendation cards now connect Pump Performance, Gas Lift, Well Performance Optimizer, Sucker Rod Pumping, Hydraulic Pump, and Plunger Lift through query-based well handoff. The optimizer opens prefilled intervention, work-order, and AFE intake surfaces, recommendation-driven work-order creation can link AFEs through the lifecycle API with linkage visible on work-order detail, those operational pages can return to the originating optimizer context, and shared JSON export packages now cover the optimizer, production forecasting, economic evaluation, and compare-wells comparison surfaces |
| 8 | Operations and Lifecycle Integration | In Progress | Exploration, development, drilling, production, lease, enhanced recovery, decommissioning, and work-order pages now have explicit web client boundaries. `IExplorationServiceClient`, `IDevelopmentServiceClient`, `IDrillingServiceClient`, `IProductionServiceClient`, `ILeaseServiceClient`, `IEnhancedRecoveryServiceClient`, `IDecommissioningServiceClient`, and `IWorkOrderServiceClient` now own their routed pages, while `ProductionForecasting.razor` remains calculations-owned through `ICalculationServiceClient` with development and production clients feeding field and well context. `Controllers/Field/ExplorationController.cs`, `Controllers/Field/DevelopmentController.cs`, `Controllers/Field/DrillingController.cs`, `Controllers/Field/ProductionController.cs`, `Controllers/Field/DecommissioningController.cs`, and `Controllers/WorkOrder/WorkOrderController.cs` now back the active workflow pages, the work-order controller now owns field-scoped AFE link actions instead of forcing the work-order UI through the lifecycle API for that seam, approved production interventions now create planned corrective work orders plus linked AFEs through the field production and accounting workflow seam, lease acquisition now hands off into prefilled compliance-obligation intake plus FDP regulatory planning instead of stopping at a dead permits route, enhanced recovery now exposes a real pilot-economics workflow seam that deep-links into prefilled economic evaluation with return navigation, late-life production decisions can now launch decommissioning records plus lifecycle workflow context directly into the P&A execution surface with return navigation, the decommissioning P&A / abandonment pages can now launch prefilled closure-compliance intake plus closeout AFE creation with preserved return navigation back into the decommissioning workflow, and development well design now launches prefilled construction-progress and drilling-operation workflows with nested return navigation while drilling actions stay on the dedicated drilling client and field-scoped drilling controller instead of the shared operations client; the drilling slice now also round-trips real well status/depth/completion plus rig/contractor metadata and no longer collects unsupported create-time daily cost input. Remaining Phase 8 work is now limited to optional polish in thinner operational domains. |
| 9 | Data, Admin, and Workflow Integration | Planned | Rationalize PPDM39 admin pages, accounting, workflow, compliance, and permits using the support/core project docs as the ownership baseline |
| 10 | Hardening and Retirement | In Progress | Architecture and oil-and-gas fit assessment completed with concrete security, layout-governance, and UI-rule findings documented under `Documentation/WebApp-Architecture-And-OilGas-Fit-Assessment-2026-04-23.md`; retirement and dependency trimming remain |
| 11 | Identity, Persona, and Access Governance | Planned | Build role-and-persona based profile system with least-privilege authorization and auditable access for Reservoir Engineer, Petroleum Engineer, Accountant, and Geologist views across Web, ApiService, LifeCycle, and UserManagement seams |

---

## Priority Tracker

### Phase 6

| ID | Task | Status |
|----|------|--------|
| W6-01 | Produce route ownership manifest | Planned |
| W6-02 | Choose canonical app shell | Done |
| W6-03 | Consolidate KPI card implementations | Done |
| W6-04 | Classify legacy vs canonical data/admin pages | Planned |
| W6-05 | Remove custom routed pages from `Components/` except Identity area | Done |
| W6-06 | Replace page-level `IBeepOilandGasApp` usage | Done |
| W6-07 | Define long-term allowed web project references | Planned |

### Phase 7

| ID | Task | Status |
|----|------|--------|
| W7-01 | Normalize existing calculation pages to typed-client pattern | Done |
| W7-02 | Add DCA web/API/client slice | Done |
| W7-03 | Add Flash Calculations web/API/client slice | Done |
| W7-04 | Add Gas and Oil Properties pages and client methods | Done |
| W7-05 | Add Well Test Analysis slice | Done |
| W7-06 | Add Pump Performance slice | Done |
| W7-07 | Add Choke Analysis and Compressor Analysis slices | Done |
| W7-08 | Create reusable calculation result components | Done |
| W7-09 | Create reusable compare and optimization summary components | Done |
| W7-10 | Start Pass C lift composition | Done |
| W7-11 | Extend lift recommendations across remaining pump workbenches | Done |
| W7-12 | Connect optimizer recommendations into intervention, work-order, and AFE intake | Done |
| W7-13 | Deepen work-order and AFE lifecycle linkage | Done |
| W7-14 | Add optimizer return-flow polish | Done |
| W7-15 | Add comparison and analysis export support | Done |

### Phase 8

| ID | Task | Status |
|----|------|--------|
| W8-01 | Split or refine domain-specific operational web clients | Done |
| W8-02 | Align exploration pages with prospect-identification domain services | Done |
| W8-03 | Align development pages with development-planning domain services | Done |
| W8-04 | Align production pages with production-operations domain services | Done |
| W8-05 | Align lease, EOR, and decommissioning pages with dedicated clients | Done |
| W8-06 | Complete cross-module workflow linking | In Progress |
| W8-07 | Resolve work-order ownership split across lifecycle and work-order APIs | Done |
| W8-08 | Add surfacing plan for stub or thin operational domains | Planned |

### Phase 9

| ID | Task | Status |
|----|------|--------|
| W9-01 | Choose canonical PPDM39/data admin route tree | Planned |
| W9-02 | Consolidate setup, schema, and seeding experiences | Planned |
| W9-03 | Add support-domain clients for compliance, permits, workflow, and accounting | Planned |
| W9-04 | Align accounting pages to domain ownership | Planned |
| W9-05 | Align workflow pages to lifecycle/process ownership | Planned |
| W9-06 | Align role-based navigation and layouts to final route tree | Planned |
| W9-07 | Surface permit/compliance flows beyond validation rules | Planned |
| W9-08 | Expand finance UI coverage for service-heavy accounting domains | Planned |

### Phase 10

| ID | Task | Status |
|----|------|--------|
| W10-01 | Validate route ownership and navigation | In Progress |
| W10-02 | Remove or redirect deprecated pages | Planned |
| W10-03 | Remove duplicate components that are no longer used | Planned |
| W10-04 | Reduce direct project references in web csproj | Planned |
| W10-05 | Publish final integration matrix | Planned |
| W10-06 | Close tracker with completed statuses and exceptions | Done |
| W10-07 | Track deferred gaps separately from validated regressions | In Progress |
| W10-08 | Remove hardcoded OIDC client secret from web startup and rotate secret | In Progress |
| W10-09 | Consolidate canonical runtime layout and first-run gating path | In Progress |
| W10-10 | Remove inline style drift from Razor pages/components and enforce MudBlazor utility pattern | Done |

### Phase 11

| ID | Task | Status |
|----|------|--------|
| W11-01 | Define expanded persona catalog and ownership matrix for oil and gas operations, subsurface, finance, HSE, and administration roles | Planned |
| W11-02 | Publish canonical identity/access data model and migration baseline document before implementation | Planned |
| W11-03 | Migrate security/user models from `Beep.OilandGas.Models` to `Beep.OilandGas.UserManagement` with compatibility strategy (`Schema-Contract-UserManagement.md` + `MigrationManager-Evidence-Checklist.md`); source preflight evidence complete; final runtime Step 6 rerun now targets `Beep.OilandGas.UserManagement` with `targetModelNamespace=Beep.OilandGas.UserManagement.Models` successfully and saves plan `248a8ad9bfdd4c61a4a9b896162bcb39` plus artifacts under `logs/w11-runtime-*-usermgmt-fixed.json`; scope mismatch is closed (`EntityTypeCount=24`, contract entity coverage `24/24`) and runtime key inference is resolved (`primary-key-missing=0` after explicit `[Key]` annotations on canonical `_ID` fields), but remaining blockers are schema-plan preflight `preflight-connectivity=Block` (`canApply=false`) and missing runtime-emitted table/column metadata for final table/JSON closure | In Progress |
| W11-04 | Enhance migrated models for persona profile, lifecycle state, scope context, and audit metadata | In Progress |
| W11-05 | Add API endpoints for profile retrieval/update and role/permission assignment with audit events | Planned |
| W11-06 | Add typed web client seams for access-control and profile management (no direct domain calls from pages) | Planned |
| W11-07 | Build role-aware navigation and default landing views per persona | Planned |
| W11-08 | Build persona-specific dashboards and page-access gating for core workflows | Planned |
| W11-09 | Add ABAC-style field/asset scoping checks in critical workflow endpoints | Planned |
| W11-10 | Add end-to-end authorization tests, negative-access tests, and impersonation-safe validation | Planned |
| W11-11 | Add identity and authorization observability (audit trail, denied-access telemetry, policy evaluation traces) | Planned |
| W11-12 | Run hardening pass for password/hash/token/session controls and privilege escalation regression checks | Planned |

---

## Guardrails

- Keep the three-layer architecture: Web -> API -> domain services
- Do not add new page-level direct domain-library calls
- Prefer one business function per page
- Keep engineer workflow pages separate from PPDM raw data-management pages
- Use shared components only when they truly serve more than one page or feature
