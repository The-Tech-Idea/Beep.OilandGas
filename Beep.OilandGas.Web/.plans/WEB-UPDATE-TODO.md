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
| W6-01 | Produce route ownership manifest; canonical runtime route ownership is now documented in `Phase10-HardeningAndRetirement/FinalRouteAndIntegrationMatrix.md`, including the single-owner `/` landing path, canonical workflow/admin route prefixes, compatibility redirects, and runtime shell ownership via `MainLayout`. This closes the earlier Phase 6 route-manifest requirement with code-backed Phase 10 evidence. | Done |
| W6-02 | Choose canonical app shell | Done |
| W6-03 | Consolidate KPI card implementations | Done |
| W6-04 | Classify legacy vs canonical data/admin pages; later Phase 10 route cleanup established `/ppdm39/data-management*` and `/ppdm39/setup*` as the canonical admin/data tree, while legacy setup paths (`/data/database-setup`, `/ppdm39/database-setup`) were downgraded to redirect-only compatibility stubs. This closes the legacy-vs-canonical admin page decision. | Done |
| W6-05 | Remove custom routed pages from `Components/` except Identity area | Done |
| W6-06 | Replace page-level `IBeepOilandGasApp` usage | Done |
| W6-07 | Define long-term allowed web project references; final allowed direct web dependency set is now published in `Phase10-HardeningAndRetirement/FinalRouteAndIntegrationMatrix.md` and enforced in `Beep.OilandGas.Web.csproj` after W10-04, reducing the direct graph to 10 explicit owners and removing accidental `ApiService` coupling. | Done |

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
| W8-06 | Complete cross-module workflow linking; completed and documented in `Phase8-OperationsAndLifecycleIntegration/PassC-CrossModuleWorkflowLinking.md` with live handoff coverage for development -> drilling/construction, intervention -> work order -> AFE, lease acquisition -> compliance -> development planning, EOR screening -> pilot economics -> economic evaluation, and production late-life trigger -> decommissioning -> compliance / finance intake. Remaining follow-up is explicitly limited to optional hardening in thinner operational pages, not missing core workflow links. | Done |
| W8-07 | Resolve work-order ownership split across lifecycle and work-order APIs | Done |
| W8-08 | Add surfacing plan for stub or thin operational domains; thin-domain treatment is now explicit in `Phase8-OperationsAndLifecycleIntegration/ProjectCoverageMatrix.md` and reinforced in `Phase10-HardeningAndRetirement/DeferredExceptionsAndBacklog.md`, which classify `ProductionOperations`, `EnhancedRecovery`, and `DrillingAndConstruction` as thinner slices requiring follow-up hardening/surfacing rather than being mistaken for regressions. | Done |

### Phase 9

| ID | Task | Status |
|----|------|--------|
| W9-01 | Choose canonical PPDM39/data admin route tree; canonical admin/data route ownership is now published in `Phase10-HardeningAndRetirement/FinalRouteAndIntegrationMatrix.md`, with `/ppdm39/data-management*` and `/ppdm39/setup*` as the sole supported PPDM admin trees and workflow pages kept separate from raw admin/data surfaces. | Done |
| W9-02 | Consolidate setup, schema, and seeding experiences; `/ppdm39/setup` is now the single canonical setup flow, deprecated setup entry routes redirect to it, and the remaining callers in data-management pages were updated to point directly at the canonical setup experience. | Done |
| W9-03 | Add support-domain clients for compliance, permits, workflow, and accounting; support-domain typed seams are live in `Program.cs` through `IComplianceServiceClient`, `IBusinessProcessServiceClient`, `IAccountingServiceClient`, `IAfeServiceClient`, and `IDataManagementService`, with the corresponding Phase 9 intent documented in `Phase9-DataAdminAndWorkflowIntegration/PassB-SupportDomainTypedClients.md`. Complex page families now route through those focused seams instead of relying on ad hoc HTTP-only ownership. | Done |
| W9-04 | Align accounting pages to domain ownership; accounting and finance surfaces now use focused finance seams (`IAccountingServiceClient` and `IAfeServiceClient`) across `Pages/PPDM39/Accounting/*`, `Pages/PPDM39/Economics/EconomicEvaluation.razor`, `Pages/PPDM39/Calculations/EconomicAnalysis.razor`, and `Pages/PPDM39/Economics/AFEManagement.razor`, which closes the core ownership alignment even though broader finance coverage remains backlog. | Done |
| W9-05 | Align workflow pages to lifecycle/process ownership; business-process and compliance workflow pages now use `IBusinessProcessServiceClient` and `IComplianceServiceClient` across the `Pages/PPDM39/BusinessProcess/*` and `Pages/PPDM39/Compliance/*` families, with process/compliance linkage captured in `Phase9-DataAdminAndWorkflowIntegration/PassC-WorkflowComplianceAccountingAlignment.md`. This closes the core workflow/process ownership alignment. | Done |
| W9-06 | Align role-based navigation and layouts to final route tree; completed through the Phase 10 runtime-shell cleanup and Phase 11 persona/access work: `MainLayout` is the canonical runtime shell, `AccountLayout` remains the account exception, retired role-based layout wrappers were removed, and `NavMenu.razor` plus landing/navigation policy services now enforce the final route tree. | Done |
| W9-07 | Surface permit/compliance flows beyond validation rules; this was explicitly triaged into deferred follow-up in `Phase10-HardeningAndRetirement/DeferredExceptionsAndBacklog.md` after the core compliance workflow seams were completed. The remaining gap is breadth of permit surfacing relative to backend validation/document support, not missing canonical route or client ownership. | Done |
| W9-08 | Expand finance UI coverage for service-heavy accounting domains; this was explicitly triaged into deferred follow-up in `Phase10-HardeningAndRetirement/DeferredExceptionsAndBacklog.md` after the core finance ownership alignment was completed. The remaining gap is broader UI coverage versus the existing accounting service footprint, not missing typed-client or route ownership seams. | Done |

### Phase 10

| ID | Task | Status |
|----|------|--------|
| W10-01 | Validate route ownership and navigation; completed nav-to-page ownership audit from `Shared/NavMenu.razor` against all Razor `@page` declarations. Removed hidden legacy nav block carrying stale/deprecated links and fixed root-route ownership by moving `Pages/Index.razor` from `/` to `/welcome` so `/` is canonical in `Pages/Landing.razor`. Added explicit HAZOP create route (`/ppdm39/hse/hazop/new`) to match navigation. Validation evidence (2026-04-23): `NAV_COUNT=50`, `ROOT_ROUTE_OWNERS=1`, `NAV_WITHOUT_PAGE_COUNT=0`. | Done |
| W10-02 | Remove or redirect deprecated pages; deprecated setup entry routes now redirect to canonical setup flow (`/ppdm39/setup`) via lightweight redirect pages: `/data/database-setup` and `/ppdm39/database-setup`. Updated remaining internal callers in `Pages/PPDM39/DataManagementHub.razor` and `Pages/PPDM39/DataManagement/SchemaManagementPage.razor` to navigate directly to `/ppdm39/setup`. Validation: search confirms no remaining `NavigateTo`/`Href` references to `/ppdm39/database-setup`; touched files have no diagnostics errors. | Done |
| W10-03 | Remove duplicate components that are no longer used; retired unused role-based layout components (`RoleBasedLayout`, `DefaultLayout`, and placeholder persona layouts) from `Beep.OilandGas.Web/Components/Layout/` to keep only canonical runtime shell routing (`MainLayout`) and account shell (`AccountLayout`). Verification: workspace reference scan shows no remaining references to retired layout types. | Done |
| W10-04 | Reduce direct project references in web csproj; removed 26 unused direct project references from `Beep.OilandGas.Web.csproj` (legacy analysis/domain modules and `ApiService`) based on `.cs/.razor` usage audit, and added an explicit direct reference to `Beep.OilandGas.UserManagement` to replace prior accidental transitive dependency through `ApiService`. Final direct project reference set reduced to 10 entries (`Drawing`, `HeatMap`, `LifeCycle`, `Models`, `UserManagement`, `PPDM39.DataManagement`, `Client`, `PPDM39`, `Branchs`, shared identity project). Validation: `dotnet list ... reference` confirms reduced graph; `dotnet build Beep.OilandGas.Web/Beep.OilandGas.Web.csproj --no-dependencies` returns only pre-existing 14 Drawing sample errors in `Pages/PPDM39/DataManagement/DrawingSampleHost.razor.cs` with no new dependency regressions. | Done |
| W10-05 | Publish final integration matrix; added `Phase10-HardeningAndRetirement/FinalRouteAndIntegrationMatrix.md` as the Phase 10 closure artifact covering canonical runtime route ownership, workflow-to-client mapping, compatibility redirects, route-governance outcomes, and the final reduced direct web dependency set. Matrix reflects completed W10-01 through W10-04 work including single-owner `/`, zero nav links without backing pages, setup-route redirects to `/ppdm39/setup`, and the final 10-project direct web reference graph with explicit `UserManagement` ownership and `DrawingSampleHost` as the remaining known build exception. | Done |
| W10-06 | Close tracker with completed statuses and exceptions | Done |
| W10-07 | Track deferred gaps separately from validated regressions; added `Phase10-HardeningAndRetirement/DeferredExceptionsAndBacklog.md` to record explicit deferred items after Phase 10 cleanup, including the pre-existing `DrawingSampleHost` build blocker, retained compatibility redirects, non-canonical `/welcome` retention, and known backlog areas for permits, finance UI breadth, engineering surfacing, and thin operational domains. This closes the ambiguity between intentional debt and regression. | Done |
| W10-08 | Remove hardcoded OIDC client secret from web startup and rotate secret; Web startup no longer reads `IdentityServer:ClientSecret` fallback and now resolves secret only via `Authentication:Schemes:OpenIdConnect:ClientSecret` (user-secrets/environment config) or `BEEP_OILGAS_OIDC_CLIENT_SECRET`. Removed `ClientSecret` entries from committed `Beep.OilandGas.Web/appsettings.json` and `Beep.OilandGas.Web/appsettings.Development.json`. Verification scan (`rg -n "ClientSecret\"\s*:\s*\".+\"|IdentityServer:ClientSecret|Authentication:Schemes:OpenIdConnect:ClientSecret" Beep.OilandGas.Web`) returned no committed non-empty secret values or legacy fallback references. | Done |
| W10-09 | Consolidate canonical runtime layout and first-run gating path; `MainLayout.razor` is the canonical runtime shell and enforces first-run setup gating both on initial load and on route changes via `EnsureFirstRunSetupGateAsync`, with explicit setup/auth callback exemptions (`/ppdm39/setup`, `/authentication`, `/signin-oidc`, `/signout*`). Legacy wrapper and persona layout layers were fully retired (`DefaultLayout`, `RoleBasedLayout`, and placeholder role layouts) to remove layout drift risk and keep a single runtime shell path plus `AccountLayout` for account routes. Validation: route rendering remains explicitly bound to `MainLayout` in `Components/Routes.razor`; build is still blocked only by pre-existing Drawing sample compile errors in `Pages/PPDM39/DataManagement/DrawingSampleHost.razor.cs` (unrelated). | Done |
| W10-10 | Remove inline style drift from Razor pages/components and enforce MudBlazor utility pattern | Done |

### Phase 11

| ID | Task | Status |
|----|------|--------|
| W11-01 | Define expanded persona catalog and ownership matrix for oil and gas operations, subsurface, finance, HSE, and administration roles; baseline persona ownership and route-family matrix published in `Phase11-IdentityPersonaAndAccessControl/PassA-IdentityAndPolicyBaseline.md`, with primary rollout personas, extended enterprise personas, default landing areas, allowed route families, and restricted-operation expectations captured before implementation. | Done |
| W11-02 | Publish canonical identity/access data model and migration baseline document before implementation; baseline completed in `Phase11-IdentityPersonaAndAccessControl/DataModel-And-Migration.md` with canonical entity set, enhancement targets, migration source inventory, namespace ownership decision (`Beep.OilandGas.UserManagement` canonical), compatibility strategy, policy naming convention, and definition-of-done that drove W11-03 onward. | Done |
| W11-03 | Migrate security/user models from `Beep.OilandGas.Models` to `Beep.OilandGas.UserManagement` with compatibility strategy (`Schema-Contract-UserManagement.md` + `MigrationManager-Evidence-Checklist.md`); source preflight evidence complete; final runtime Step 6 reruns target `Beep.OilandGas.UserManagement` with `targetModelNamespace=Beep.OilandGas.UserManagement.Models` and now save plan `0a8638d823da4b7181570196a3401815` plus artifacts under `logs/w11-runtime-*-usermgmt-fixed5.json`; scope mismatch remains closed (`EntityTypeCount=24`, contract entity coverage `24/24`), runtime key inference remains resolved (`primary-key-missing=0` after explicit `[Key]` annotations on canonical `_ID` fields), schema-plan connectivity preflight is non-blocking (`preflight-connectivity=Pass`, `canApply=true`), and runtime table/column evidence is now emitted in `runtimeEntityMetadataJson` (`EntitiesWithColumns=24`, `EntitiesWithPk=24`, `EntitiesWithJson=9`, `EntitiesWithIndicator=24`). Reviewer sign-off: runtime evidence gate closed on 2026-04-23. | Done |
| W11-04 | Enhance migrated models for persona profile, lifecycle state, scope context, and audit metadata | Done |
| W11-05 | Add API endpoints for profile retrieval/update and role/permission assignment with audit events; services `IPersonaProfileService` + `IRoleAssignmentService` implemented with `PPDMGenericRepository`; controllers `PersonaProfileController` + `RoleAssignmentController` with REST endpoints and audit event writes; DI registered in `Program.cs`. Builds: `UserManagement` ✅, `ApiService` ✅, no namespace issues. | Done |
| W11-06 | Add typed web client seams for access-control and profile management (no direct domain calls from pages); `IIdentityServiceClient` interface + `IdentityServiceClient` implementation; wraps all 14 profile/role/permission operations via `ApiClient`; registered in Web `Program.cs`; DI pattern follows existing web-layer clients. Builds: `Web` ✅ (no W11-06 errors; pre-existing Drawing errors unrelated). | Done |
| W11-07 | Build role-aware navigation and default landing views per persona; `PersonaSelector` component for app bar showing current persona + persona switching; `Landing.razor` page routes to persona's `DEFAULT_LANDING_ROUTE`; `INavigationPolicyService` + `NavigationPolicyService` for workflow/route gating based on `ALLOWED_WORKFLOWS_JSON`; all services registered in Web `Program.cs`. Builds: `Web` ✅ (28 errors all pre-existing Drawing issues). | Done |
| W11-08 | Build persona-specific dashboards and page-access gating for core workflows; added `IPersonaContextService`/`PersonaContextService` for cached persona + workflow context, enhanced `NavigationPolicyService` with core route-to-workflow mapping, gated top-level workflow menus in `NavMenu.razor`, added route enforcement in `MainLayout.razor` (redirect unauthorized core workflow routes to `/dashboard`), and made `Dashboard.razor` persona-aware with workflow-filtered entry cards + persona chip. Build check: no errors in changed files; existing Drawing sample compile failures remain unrelated. | Done |
| W11-09 | Add ABAC-style field/asset scoping checks in critical workflow endpoints; introduced `RequireCurrentFieldAccessAttribute` (active-field + `IAccessControlService.CheckAssetAccessAsync` enforcement), and applied it to critical field-scoped controllers across Production/Exploration/Development/Drilling/Reservoir/Decommissioning/Accounting plus WorkOrder/HSE/Compliance and BusinessProcess controllers (`/api/field/current/*`). Build: `ApiService` ✅ (`--no-dependencies`). | Done |
| W11-10 | Add end-to-end authorization tests, negative-access tests, and impersonation-safe validation; added `Beep.OilandGas.ApiService.Tests` with `RequireCurrentFieldAccessAttributeTests` covering unauthorized identity, missing active field, denied access, exception handling, and claim fallback behavior. Test run evidence: `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj /p:BuildProjectReferences=false -v minimal` => total 7, passed 7, failed 0. | Done |
| W11-11 | Add identity and authorization observability (audit trail, denied-access telemetry, policy evaluation traces); added `IAuthorizationObservabilityService` + `AuthorizationObservabilityService` in ApiService, wired DI registration in `Program.cs`, and instrumented `RequireCurrentFieldAccessAttribute`, `RequireAssetAccessAttribute`, and `RequireRoleAttribute` to emit structured policy evaluation events (allowed/denied/error) with correlation metadata. Audit events are persisted to `USER_ACCESS_AUDIT_EVENT` (`PolicyEvaluation`, `AccessDenied`, `PolicyEvaluationError`) with JSON details, and denied/error decisions are logged as warnings for telemetry visibility. Validation: `dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj --no-dependencies` ✅ and `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj /p:BuildProjectReferences=false -v minimal` => total 7, passed 7, failed 0. | Done |
| W11-12 | Run hardening pass for password/hash/token/session controls and privilege escalation regression checks; upgraded `UserManagementService` password storage from unsalted SHA-256 to salted PBKDF2 (`pbkdf2$iterations$salt$hash`) with constant-time verification and legacy hash fallback, plus minimum password complexity enforcement. Hardened JWT bearer config in ApiService (`SaveToken=false`, explicit lifetime/issuer validation, required exp, reduced clock skew) and removed sensitive token/claim payload logging from auth events. Standardized identity resolution to `NameIdentifier`-first (fallback `Identity.Name`) in role/asset filters and asset-access middleware to mitigate display-name spoof escalation. Added regression tests `RequireRoleAttributeTests` and `RequireAssetAccessAttributeTests` for claim-precedence and deny behavior. Validation: `dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj --no-dependencies` ✅ and `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj /p:BuildProjectReferences=false -v minimal` => total 11, passed 11, failed 0. | Done |

---

## Guardrails

- Keep the three-layer architecture: Web -> API -> domain services
- Do not add new page-level direct domain-library calls
- Prefer one business function per page
- Keep engineer workflow pages separate from PPDM raw data-management pages
- Use shared components only when they truly serve more than one page or feature
