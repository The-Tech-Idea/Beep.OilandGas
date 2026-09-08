# Oil and Gas Application Review and Delivery Plan

Date: 2026-09-05
Status: Proposed delivery baseline following repository review. No application code changed.

Update following local BeepDM source review: complete the [BeepDM modernization prerequisite](BEEPDM_MODERNIZATION_REVIEW_AND_PLAN.md) before broad feature work. The app mixes core package versions and partially adopts newer engine capabilities. The local engine also has registration lifetime behavior that must be validated or corrected before adoption. This prerequisite extends Phases 0-2; it does not defer the urgent authorization fixes.

## 1. Review conclusion

Beep.OilandGas has extensive implementation across petroleum engineering, PPDM data management, asset lifecycle, production, accounting, and administration. The immediate priority is to establish a reproducible, securely authorized application and prove complete operational workflows. Project and page counts do not establish production readiness.

Recommended first release: an authenticated field operations experience covering field/well/facility selection, daily production capture and validation, intervention/work-order handling, approval, and traceable reporting. Extend into allocation and period close only after persistence, reconciliation, and authorization are demonstrated.

This is a risk-focused architecture and sampled code review, not an exhaustive audit of every controller, scientific formula, or accounting method. No live database or browser session was used. Numerical correctness, regulatory compliance, deployment readiness, and end-to-end behavior remain unverified.

## 2. Evidence and current state

- Inventory: 62 project files, 160 Razor files with route directives, and 19 test projects.
- Actual implementation: .NET 10 Blazor Web, ASP.NET Core ApiService, reusable Client, Beep/PPDM repositories, domain modules, and separate model assemblies.
- Existing assets worth retaining: typed service clients, API authentication requirement, field/asset access attributes and tests, numerical edge-case tests, module seed catalogs, workflow services, and domain-specific libraries.
- API startup is approximately 2,800 lines; service registration and application composition are concentrated in one file.
- Existing planning is distributed across Plans/Architecture, Plans/Phases, module trackers, Docs, and the root tracker. Their completion statements need verification against executable acceptance tests.
- AGENTS.md describes TheTechIdeaWeb, TheTechIdea.Data, BeepDiA, and Aspire. Those application projects are not present as local project files in this checkout. The Web project references external shared identity and branding projects. Treat the supplied architecture rules as requirements; record an explicit mapping before implementation.

### Build verification

Environment SDK: 10.0.400.

Attempted: `dotnet build Beep.OilandGas.sln --no-restore -v quiet -clp:ErrorsOnly`.

The build reported a missing external Beep.Foundation.IdentityServer.Shared project, missing NuGet packages, and missing assets files. The command ended with a failed summary of 37 errors and 624 warnings after a cancellation request. This was a local no-restore baseline, not a clean restored compilation. Do not classify every diagnostic as a source defect or reuse the old tracker claim that Permits is the sole blocker. Tests were not executed because the dependency/build baseline was unavailable.

## 3. Findings, ordered by priority

### R1 - P1: Role administration permits ordinary authenticated callers

Evidence: `Beep.OilandGas.ApiService/Controllers/Identity/RoleAssignmentController.cs:10`, `:74`, and `:133`; `Beep.OilandGas.UserManagement/Services/RoleAssignmentService.cs:50` and `:121`.

The controller requires authentication but its assignment, revocation, and permission-grant actions do not require an administrator role. The reviewed service writes approved assignments and grants without validating the acting user's administrative authority. The optional segregation-of-duties check evaluates assignment conflicts, not the caller's authority. The reviewed global asset middleware only populates context and does not provide this administrator gate.

Impact: a valid non-admin account can reach operations that alter authorization, including targeting its own user ID, subject to database configuration and available role IDs.

Required result: authoritative app-owned role resolution; standard ASP.NET role guards on all administrative mutations; anonymous=401, ordinary user=403, authorized administrator=success. Cover direct HTTP calls, self-assignment, revocation, and permission changes.

### R2 - P1: Generic data writes trust caller-supplied audit identity

Evidence: `Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39DataController.cs:108` and `:152`.

Insert and update actions accept a query-string userId defaulting to SYSTEM and pass it to the data service. The actor is not derived from the authenticated principal in these actions.

Impact: the caller can select another audit identity or SYSTEM at the API boundary. Downstream persistence behavior must also be traced, but the public contract already exposes an untrusted actor field.

Required result: derive audit identity from validated claims, reject absent identity, prevent body/query audit-column overrides, and distinguish service accounts from interactive users. Verify persisted audit fields with a real database test. In the same pass, audit table/connection allowlists and field/company isolation on generic CRUD, import, and export endpoints.

### R3 - P1: Web role mapping contradicts the required authentication boundary

Evidence: `Beep.OilandGas.Web/Program.cs:183` and `:190`.

Web requests the roles scope and maps role claims from the identity provider. No local IClaimsTransformation implementation was found in the repository search. This does not establish whether an external shared assembly registers one; that dependency needs inspection once restored.

Impact: the visible configuration depends on identity-provider roles rather than the required app-owned RBAC bridge. Authentication-only tokens may leave role guards ineffective for intended users or cause incorrect role sourcing.

Required result: implement the mandated read-only current-user role API and Web IClaimsTransformation with a marker claim, standard ClaimTypes.Role, and fail-closed behavior. Independently resolve/enforce the API's endpoint roles. Verify role revocation and stale-session behavior.

### R4 - P1: Build depends on external checkout layout

Evidence: `Beep.OilandGas.Web/Beep.OilandGas.Web.csproj:29`; `.github/workflows/ci.yml:18`; current build diagnostics.

Web references sibling repositories by relative filesystem paths. CI checks out this repository only before restoring the full solution. At least the shared identity project was absent in the current environment.

Impact: a fresh clone or standard CI runner cannot reliably reproduce the application build without explicit external dependency provisioning.

Required result: versioned shared packages or documented, pinned dependency checkouts. Establish restore/build/test commands that pass on a clean agent; fix source compilation errors only after dependency resolution produces a trustworthy baseline.

### R5 - P2: Advertised workflow integration tests can pass without production code

Evidence: `Beep.OilandGas.ApiService.Tests/CrossModuleWorkflowTests.cs:24`, particularly the act block at `:44`.

The intervention-to-work-order-to-AFE test invokes configured mock objects directly and asserts their configured results. It does not exercise the actual orchestration, controllers, HTTP middleware, or persistence.

Impact: broken workflow wiring or missing persisted relationships can remain undetected while the test passes.

Required result: invoke real orchestration with repository boundaries, then add hosted HTTP/database tests for the complete chain. A deliberate break in the production handoff must fail the relevant test.

### R6 - P2: Web can select local data access through client auto-detection

Evidence: `Beep.OilandGas.Web/Program.cs:375`; `Beep.OilandGas.Client/DependencyInjection/ClientServiceCollectionExtensions.cs:92` and `:118`; `Beep.OilandGas.Client/Connection/DataSourceManager.cs:28`.

Web uses auto-mode registration. That registration can choose local services when configured, and the Client assembly includes datasource connection code. This is a permitted execution path, not proof that the current configuration opens a database from Web.

Required result: explicitly register remote-only clients in Web; isolate local access for authorized non-Web hosts; add dependency/registration checks preventing Web from resolving domain repositories or database drivers. Remove BuildServiceProvider during registration to avoid a second DI container.

### R7 - P2: Field-map controls do not operate a map

Evidence: `Beep.OilandGas.Web/Components/Visualization/FieldMap.razor:45`, `:86`, and `:111`.

The component displays implementation-needed text instead of a map. Layer/fullscreen actions toggle fields without rendering the corresponding map behavior. It also contains page-local CSS contrary to AGENTS.md; similar style blocks exist in several shared components.

Required result: implement an actual geographic view with coordinates, selection, layers, and fullscreen behavior, or remove unavailable controls from the release scope. Consolidate custom styles/scripts into the required files and retain MudBlazor palette tokens.

## 4. Architecture decisions to settle first

| Decision | Required direction | Concrete deliverable |
|---|---|---|
| API ownership | ApiService remains the sole business-data gateway | Map Beep.OilandGas.ApiService to the canonical platform API boundary; document host/integration arrangement |
| Domain ownership | Follow TheTechIdea.Data ownership rule | Inventory current Models, PPDM.Models, PPDM39, and module entities; define migration/compatibility mapping to the canonical owner before moving types |
| Persistence | Preserve working Beep/PPDM behavior during alignment | Document physical PPDM tables, extensions, keys, migrations, and repositories; do not assume a wholesale EF rewrite is required |
| Administration | Core operational administration belongs to BeepDiA | Define API resources and migration/integration of current Web admin screens; explicitly document any retained Web admin scope |
| Identity | Authentication-only IdentityServer | Current-user RBAC API, claims transformation, independent API role enforcement |
| Runtime composition | .NET 10 with reproducible dependencies | SDK policy, pinned external libraries, documented Web/API/identity startup and Aspire integration location |
| Branding | Shared BrandingConfig and mapper | Restore shared branding dependency, one experience-root theme, token-based CSS |

Do not silently replace the supplied rules with the repository's older module-owned model convention. Resolve the CI module-ownership guard's scope alongside the domain ownership mapping. During planning, preserve existing code and database contracts.

## 5. Release scope and users

Initial planning assumption: an upstream operator managing multiple fields, with production operations as the first usable release. This assumption is reversible and does not imply that downstream/midstream features are absent or unwanted.

| User | Primary release workflow |
|---|---|
| Production engineer | Select field/well, review production and losses, investigate variance, propose intervention |
| Operations supervisor | Review submissions, approve work, schedule work orders, record completion |
| Data steward | Validate well/facility master data, stage imports, resolve quality failures |
| Production accountant | Review approved measurements and reconciliation; allocation/close follows operational pilot |
| Asset manager | Review field totals, exceptions, work backlog, and approved reports |
| Administrator | Manage app-owned roles and asset access through authorized admin APIs |

First-release scope: master asset context, production capture/import, validation, review/approval, work orders, basic operational reporting, traceable exports, role/asset enforcement.

Subsequent releases: production allocation and close; forecasting/economics; drilling/development/exploration workflow completion; advanced lift/PVT/nodal tools; expanded HSE, permits, lease, and decommissioning workflows. Preserve existing capabilities but do not label them release-ready without evidence.

## 6. Phased execution backlog

Estimates below are planning ranges in engineer-weeks, not calendar commitments. They assume one main deployment database, existing services are reusable, and access to a domain reviewer. Re-estimate after Phase 0; do not sum these into a promised delivery date.

### Phase 0 - Reproducible baseline and ownership decisions

Priority: P1. Effort: 1-2 engineer-weeks. Owners: technical lead and build engineer.

1. Inventory missing external references; choose pinned packages or provisioned dependency checkouts.
2. Establish a clean restore and build; categorize environment, package, source, and generated-code failures separately.
3. Run the 19 test projects and retain results, including skipped tests and failure causes.
4. Build a page -> client -> route -> service -> table -> test matrix for the proposed release workflows.
5. Record the architecture mappings in Section 4 and identify available required skill files before implementation.
6. Reconcile existing trackers against verified behavior; keep one release backlog with links to module plans.

Exit: clean-agent build, reproducible test results, documented run prerequisites, agreed ownership mapping, and a bounded operational pilot backlog. No fabricated progress percentage.

### Phase 1 - Authorization, isolation, and audit integrity

Priority: P1. Effort: 2-4 engineer-weeks. Dependency: Phase 0 build baseline. Owners: API and identity engineers.

1. Address R1-R3, including authoritative current-user roles and standard role guards.
2. Enumerate administrative endpoints: role/access changes, connections, setup, seeding, schema operations, bulk CRUD, import/export, and period-close actions.
3. Enforce company/field/well/facility access server-side on list, detail, mutation, export, and background-job paths. Context populated by middleware is insufficient unless downstream queries enforce it.
4. Derive the actor from authenticated claims; normalize subject handling; remove caller-controlled SYSTEM attribution.
5. Validate table and connection selection against server-managed permissions. Keep secrets out of returned connection metadata.
6. Test role removal, API failures during role resolution, guessed foreign IDs, empty access sets, concurrent users, and privileged job access.

Exit: hosted HTTP tests demonstrate 401/403/success behavior and denial of cross-field access; real persistence verifies audit identity; no known P1 authorization issue remains in release routes.

### Phase 2 - API contracts and data foundation

Priority: P1/P2. Effort: 2-4 engineer-weeks. Dependencies: Phase 0 ownership decisions and Phase 1 access contract. Owners: API/data engineers.

1. Address R6 and explicitly bind Web to remote clients.
2. Consolidate business contracts according to the canonical ownership decision, preserving serialization and compatibility for existing consumers.
3. Establish explicit company, field, well/UWI, wellbore, facility, and production-period identifiers and relationship validation.
4. Define units, standard/reference conditions, timestamps/time zones, effective dates, precision, and null/missing-data semantics for release measurements.
5. Add bounded pagination, validated sorting/filtering, cancellation, and consistent error responses to release queries.
6. Define idempotency and concurrency behavior for import, approval, posting, and close operations.
7. Version schema changes; verify seed reruns are safe; test backup/restore before migrations against a representative copy.
8. Extract cohesive module DI registrations from Program.cs incrementally, with startup resolution tests.

Exit: a fixture dataset can be initialized repeatedly; Web uses only API data access; release contracts and asset relationships are tested; duplicate/concurrent writes behave predictably.

### Phase 3 - Complete field operations pilot

Priority: P2. Effort: 3-5 engineer-weeks. Dependencies: Phases 1-2. Owners: full-stack engineers, QA, production domain reviewer.

1. Provide stable field/well/facility context with server-authorized selection and consistent navigation.
2. Implement daily oil/gas/water capture and file import with preview, validation, row-level errors, provenance, and duplicate handling.
3. Distinguish measured, estimated, missing, rejected, and approved values. Never silently substitute zero for unavailable production.
4. Complete submit -> review -> approve/reject -> correction behavior with history and concurrency protection.
5. Complete intervention -> work order -> AFE link -> approval -> completion using real persisted relationships.
6. Provide operational totals, variance/loss views, well detail, backlog, and export from the same authoritative data.
7. Handle loading, empty, unavailable, forbidden, and validation states consistently; preserve unsaved edits appropriately.
8. Implement R7 only if maps are required for the pilot; otherwise exclude that control from the release surface.

Exit: a steward loads assets, an engineer submits production, a supervisor approves work, and a manager sees matching approved totals after restart. Unauthorized accounts cannot execute the same actions through direct API calls.

### Phase 4 - Production accounting and reconciliation

Priority: P2. Effort: 3-5 engineer-weeks. Dependency: approved operational data from Phase 3. Owners: accounting/API engineers and production accountant.

1. Trace measurements through run tickets, inventory movement, allocation, ownership/effective dates, pricing, and revenue outputs.
2. Define balancing tolerances, rounding, negative adjustments, missing measurements, and allocation versioning with the domain owner.
3. Require validated inputs before posting; make retries idempotent and transaction boundaries explicit.
4. Complete period status transitions, close prerequisites, restricted reopening, reversals, and immutable historical evidence.
5. Test correction of a previously approved measurement without silently changing a closed period.
6. Produce reconciled operational/accounting reports with input lineage and approval history.

Exit: a representative month reconciles within agreed tolerances; repeated posting cannot duplicate entries; an injected failure cannot leave a partial close; reversal/reopen history is retained. Accounting acceptance is by the domain reviewer, not inferred from method names.

### Phase 5 - Engineering calculation reliability

Priority: P2. Effort: 2-4 engineer-weeks for the selected first toolset. Dependencies: Phase 2 measurement conventions. Owners: engineering-library developer and petroleum engineer.

1. Select the first tools based on the pilot: forecasting plus the relevant production-analysis method; keep remaining engines behind separate readiness gates.
2. Review each selected correlation/algorithm's inputs, units, applicability bounds, convergence rules, and error behavior.
3. Assemble independently checked reference cases with explicit provenance and tolerances; verify external technical sources during implementation.
4. Test unit equivalence, limiting cases, invalid input, no-solution conditions, sensitivity, and reproducibility.
5. Persist run inputs, algorithm/version, units, assumptions, warnings, and results; associate runs with assets and approvals.
6. Verify plots against returned numerical data and distinguish failed/non-converged results from usable output.

Exit: domain-approved benchmark cases pass with documented tolerances and each result is reproducible. No claim of scientific validation is made by this repository review.

### Phase 6 - Lifecycle expansion

Priority: P3 after the operational pilot. Effort: estimate separately per workflow. Owners: module engineers and relevant domain reviewers.

| Workstream | Complete workflow | Acceptance evidence |
|---|---|---|
| Exploration/development | Prospect assessment -> gate approval -> development plan | Persisted lineage, versioned decisions, authorized transitions |
| Drilling/construction | Approved program -> daily operations -> completion -> production handover | Well/wellbore identity continuity, costs, approvals, handover record |
| Facilities | Equipment -> maintenance -> work order -> downtime impact | Traceable event and production-loss linkage |
| HSE/permits | Incident/application -> actions -> review -> closure | Attachments, due dates, approved closure and audit trail |
| Leases | Agreement -> obligation -> payment/compliance linkage | Effective dates, asset ownership and obligation traceability |
| Decommissioning | Candidate -> estimate -> approval -> abandonment -> restoration | Financial/operational linkage and closure evidence |

For every stream, reuse the phase checklist: canonical contracts, authorized API, typed client, usable UI, real persistence tests, failure/retry handling, documentation. Confirm jurisdiction-specific rules with the responsible domain owner before implementing them.

### Phase 7 - Release hardening

Priority: release gate. Effort: 2-3 engineer-weeks plus discovered fixes. Runs incrementally during Phases 3-6. Owners: QA, platform engineer, product owner.

1. Run all relevant test projects in CI, not just ApiService.Tests; add a real HTTP/database suite and browser tests for pilot workflows.
2. Review all release pages for permission guards, keyboard access, readable validation, stable layouts, and supported desktop/tablet/mobile sizes.
3. Apply required theme/CSS/JS organization using shared branding; verify Arabic/RTL if it is a release requirement.
4. Measure representative dataset performance; set and record API latency, import throughput, and concurrent-user targets before load tests.
5. Verify request correlation, structured logs, readiness checks, durable background work, authorized progress notifications, and failure recovery.
6. Test deployment, schema migration, backup restoration, rollback compatibility, restart behavior, and persistent authentication keys.
7. Prepare runbooks, environment configuration documentation, release notes, known limitations, and role-specific UAT scripts.

Exit: agreed UAT passes; no unresolved P1 issue; restore/recovery exercise succeeds; measured performance meets agreed targets; production configuration and runbooks are reviewable.

## 7. Verification strategy

| Layer | What to prove |
|---|---|
| Architecture checks | Web cannot register local domain data access; canonical contracts have one owner |
| Unit tests | Real workflow decisions, validation, numerical boundaries; avoid tests that only exercise mocks |
| HTTP tests | Authentication, standard role guards, asset isolation, routing/model binding, error contracts |
| Database tests | Keys, audit fields, transactions, concurrency, idempotency, migration/seed behavior |
| Browser tests | Field selection, production entry, approval, work-order completion, reporting, forbidden routes |
| Domain review | Measurement semantics, calculation tolerances, allocation/reconciliation, period-close rules |
| Operational tests | Load, restart, backup restoration, deployment and recoverable failures |

Use an isolated fixture with two organizations, two fields with different access grants, several wells/facilities, two accounting periods, measured and missing production, and distinct administrator/engineer/supervisor accounts. Use a representative deployment provider for persistence tests; do not assume SQLite proves another provider's behavior.

## 8. First actionable backlog

| Order | Task | Completion evidence |
|---|---|---|
| 1 | Restore external shared dependency availability and clean build | Clean-agent log and documented dependency versions |
| 2 | Record canonical project/data/admin mappings | Architecture decision record linked to AGENTS.md |
| 3 | Protect role/permission mutations | Direct HTTP non-admin-denial regression tests |
| 4 | Replace caller audit IDs with authenticated actor | Database assertion of persisted actor |
| 5 | Implement app-owned role bridge | Auth-only token reaches intended role; resolution failure denies access |
| 6 | Enforce remote-only Web registration | Registration/dependency test |
| 7 | Prove intervention -> work order -> AFE | Test executes real production orchestration and verifies persisted links |
| 8 | Trace daily production page to database | Completed route/service/table matrix and pilot UAT script |
| 9 | Complete production approval and reporting | Restart-safe workflow and matching totals |
| 10 | Extend CI to selected engineering suites and real integration tests | Published test results with no silent omission |

## 9. Decisions needed before committing the release schedule

- Confirm the first operational audience and whether production operations is the correct first release.
- Select the initial database provider and representative data volume; list any mandatory secondary providers.
- Confirm organization/asset isolation, intended deployment topology, and concurrent user scale.
- Confirm external platform repository/package locations and how BeepDiA/TheTechIdea.Data are integrated.
- Identify domain reviewers and the applicable operating/accounting jurisdictions.
- Confirm offline use, GIS, Arabic/RTL, reporting/export, and external system integrations required for the first release.

These decisions refine scope and estimates. They do not prevent starting the reproducible-build and verified security backlog.
