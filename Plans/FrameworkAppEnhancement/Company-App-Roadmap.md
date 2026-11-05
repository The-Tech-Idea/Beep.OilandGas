# Company-wide oil and gas application roadmap

Persona-first delivery: [company persona data workflows](Company-Persona-Data-Workflows.md) and 13 detailed persona plans now define the execution view: company/role landing -> daily queue -> source/detail -> permitted action -> next-role handoff -> recorded outcome -> KPI drill-down. Existing CA/FG owners remain; prioritize missing backend behavior needed by these visible journeys.

Detailed implementation documents: [company plan index](Company-Plan-Index.md). Covers all reviewed recommendations with feature-level plans; existing phases and CA/FG task owners remain authoritative.

Reviewed 2026-10-05 against `6e32d266`. This extends the existing framework enhancement package. The root `MASTER-TODO-TRACKER.md` remains the only cross-project execution tracker. Scope is the company application, from accountants and engineers to operators, managers, HSE, commercial teams and administrators.

## Product outcome

The product must support independent customer companies and multinational corporate groups. [Multinational requirements](Company-Multinational-Requirements.md) define tenant isolation, legal entities, currencies, jurisdiction policies, regional units/timezones, languages and data residency as cross-phase requirements. CA-05/06 now precede production mutations; CA-15/16, CA-24 and CA-45/46 extend the respective finance, engineering and release gates.

One company workspace follows an asset from prospect and investment approval through drilling, production, sale, accounting and retirement. Each role sees its assigned work and permitted assets. A measurement, engineering decision, maintenance intervention and financial entry remain traceable to their source and approvals. A feature is delivered only when a user can complete the workflow, including correction and recovery, through the authenticated UI and API.

## Current source review

| Area | Evidence inspected | Assessment |
|---|---|---|
| Foundation | Web project targets net10.0; Program.cs registers AddBeepOilandGasAppRemote; shared theme/auth references | Existing HTTP path to extend. Web still references lifecycle, data-management and datasource dependencies; dependency presence alone does not prove direct database access. Audit reachable registrations before removal. |
| Accounting | Traditional AP/AR/GL/Inventory/Invoice/PurchaseOrder controllers; Accounting services; production accounting pages | Broad backend surface. Dedicated transactional UI coverage is not established by the inspected page filenames. Inventory every route and process form before adding screens. |
| Period close | Pages/PPDM39/Accounting/PeriodClose.razor | Historical closes are hardcoded, checklist completion is local, and successful close inserts a local record with actor current_user. Replace with authoritative API history and readiness evidence. Backend enforcement requires separate review. |
| Engineering | ProductionEngineerWorkspace, ReservoirEngineerWorkspace, DrillingEngineerWorkspace; calculation controllers and engineering projects | Existing workspaces and calculators. Numerical validation, persisted decisions, approval handoffs and browser journeys remain unverified in this review. |
| Operations | ProductionOperationsController, facility controllers, WorkOrder pages | Existing owners. ProductionOperationsController still exposes named compatibility methods alongside active service paths; consolidate consumers rather than add another operations model. |
| Governance | HSE, permits, compliance, BusinessProcess and Workflow page/controller inventories | Candidate integrated journeys already exist. Their durable state, attachments, restricted visibility and completion require verification. |
| Navigation | AccountantNavMenu custom activators inspected; root tracker records menu defect | Activator buttons lack an explicit menu-context invocation in the inspected excerpt. Reproduce in browser before recording a current runtime defect or applying the established fix. |
| Delivery | .github/workflows/ci.yml | CI triggers main/develop/release branches while checkout uses master. Align triggers and inspect all dependency acquisition and test jobs. Historical passing counts are not current verification. |

Coverage: existing framework review/alignment/phase plans, root tracker, source/project/route inventory and selected production, accounting, navigation and CI implementations. No full method audit, build, database migration, live identity/browser session, numerical benchmark or financial certification was performed. Page names and controller presence establish source surface only. This roadmap does not assert absent capabilities from keyword searches.

## Role and workflow coverage

| Role | Daily work and required outcome | Reuse |
|---|---|---|
| Accountant/controller | AP, AR, journals, bank reconciliation, fixed assets, cost allocation, royalty/JIB, close and financial statements | Accounting services, Traditional controllers, accounting workbenches and process definitions |
| Production engineer/operator | Capture production and tests, validate units, investigate loss, approve allocation, intervene and hand over | ProductionOperations, ProductionAccounting, field dashboard, well tests and work orders |
| Reservoir/petroleum engineer | Diagnose performance, run validated scenarios, forecast, compare alternatives and approve a recommendation | Existing reservoir, forecasting/DCA, nodal, lift and property modules |
| Drilling/development engineer | Approve development and AFE, track construction and costs, accept well handover | DevelopmentPlanning, DrillingAndConstruction, AFE and lifecycle |
| Maintenance/facility team | Schedule work, control permits, reserve materials, capture labor/cost and return equipment to service | WorkOrder, facility and existing inventory/services |
| HSE/compliance team | Incident investigation, corrective actions, permits, compliance deadlines and evidence | HSE, permits, compliance and workflow engine |
| Commercial/land team | Ownership/leases, contract terms, sales, pricing and settlement | LeaseAcquisition, Ownership, Trading, Revenue and Royalty |
| Manager/executive | Approve investment, monitor production/cost/cash/risk, drill into evidence | Executive aggregation, economics, portfolio and shared dashboards |
| Data steward/admin/auditor | Configure governed data, resolve quality issues, provision app roles, inspect history | Setup, repository RBAC, data management and external BeepDiA |

HR/payroll, enterprise document control, external banking/ERP, telemetry and mobile/offline needs must be captured in the coverage audit. Classify each as native workflow, integration or explicitly deferred with rationale; do not silently omit company functions or create a second ERP inside domain libraries.

## Phases and dependencies

1. [A: trustworthy foundation](Company-Phase-A-Foundation.md), CA-01..04; extends FG-00/01/10/11/20/50/60.
2. [B: accounting and commercial work](Company-Phase-B-Finance.md), CA-10..14; extends FG-41/52.
3. [C: engineering and asset delivery](Company-Phase-C-Engineering.md), CA-20..23; extends FG-30/31/42/52.
4. [D: operations, safety and maintenance](Company-Phase-D-Operations.md), CA-30..33; extends FG-22/40/42/52.
5. [E: company integration and release](Company-Phase-E-Company-Release.md), CA-40..44; extends FG-51/60..62.

Start A with role/route coverage and removal of fabricated close history. B and C follow the relevant A gates; D connects their approved results; E gates every promoted journey. Roles and resource checks precede writes. Do not wait for all calculators to finish before delivering one complete production-to-close pilot.

## Architecture and ownership

Apply the existing architecture alignment document and AGENTS.md. The canonical ApiService is the business-data gateway; TheTechIdea.Data owns shared business contracts. Resolve the local OilGas API-to-platform gateway deployment mapping under FG-01 rather than inventing a substitute host. Preserve generated PPDM schema and migrate shared contracts with all consumers, without copying models. BeepDiA owns operational master-data administration; domain workbenches consume its governed records through APIs. IdentityServer authenticates; authoritative app RBAC feeds the standard ASP.NET roles bridge, and API endpoints independently enforce roles and asset access. Persona selection cannot grant authority.

For each slice record the existing entity, service, API, client, route and test owner. Consolidate overlapping routes/forms before adding capabilities. Preserve one workflow engine, setup orchestrator, calculation path and posting path. Use MudBlazor/shared branding; CSS in app.css and JS in site.js. No fabricated values, silent default business inputs, or success-shaped failure responses.

## Delivery evidence

Every CA item needs: current owner/coverage inventory, concrete API/UI behavior, relevant automated checks, persisted audit and failure behavior, browser evidence for the role journey, and explicit remaining provider/integration gaps. Engineering requires independent expected results and units/provenance. Finance requires reconciliation, concurrency, idempotency and recovery evidence. Use real supported providers for promoted persistent workflows. Record SHA, commands, outcomes and skipped gates in the root tracker. No calendar promises or staffing assignments are inferred.
