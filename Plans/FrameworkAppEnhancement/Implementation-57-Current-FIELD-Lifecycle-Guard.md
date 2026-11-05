# Implementation 57 — current FIELD lifecycle request guard

Date: 2026-10-05. Execution owners: CA-03/05/06, FG-10/11/12/20. [Master tracker](../../MASTER-TODO-TRACKER.md) owns execution status. This continues [Implementation 56](Implementation-56-Company-FIELD-Selection.md) through the existing lifecycle filter/controller/orchestrator owners. The wider plan remains active.

## Problem and resulting behavior

The existing `RequireCurrentFieldAccessAttribute` trusted an ephemeral orchestrator FIELD and a principal claim without resolving the active app account, selected company, physical database binding or persistent preference. A fresh request could lack a FIELD, while a preloaded instance could supply one without the reviewed selection. The five lifecycle GET actions had no current-FIELD restoration guard.

The existing filter now resolves the actual `RepositoryRequestAccount`, `CompanyModuleConnection` and `ICompanyFieldSelectionService`. It validates preference evidence against the resolved account/company/database, requires current availability, and refuses conflicting route/query FIELD assertions. It checks independent application asset access and requested permission before restoring the exact FIELD through the existing orchestrator. The restored ID, reviewed preference/revision, active account, company grants and binding are checked again; application access/permission is reread after restoration. Failure or cancellation clears restored request state. An allowed observation is recorded only after these checks.

`api/field/current/summary`, `wells`, `statistics`, `timeline` and `dashboard` use this filter. The selector's catalog/current/set actions retain their existing independent authority checks; they do not require an already selected FIELD. First selection remains possible. The filter adds no alternate selection store, authentication claim bridge or authorization engine.

## Concrete orchestrator source and remaining domain boundary

The existing `FieldOrchestrator` constructor now requires the selection owner, row-access policy and application access service. Its actor comes from the reviewed application account; optional authorization and external subject/display-name fallbacks are removed. Its connection comes from the same reviewed database binding. `SetActiveFieldAsync` restores an existing preference, independently reads the exact active FIELD through the real policy-bound repository, and cannot choose or persist another FIELD. A request captures one review and refuses subsequent account/company/database/FIELD/revision changes. Public aggregate reads check that review before and after their work. Direct repository constructions pass the required row policy; WELL access uses the canonical UWI identity.

These **concrete orchestrator and Program registration edits are source-only**. The focused assembly compiles the actual filter, full controller, interfaces and required real module contracts, but explicitly mocks the orchestrator implementation. No mock result establishes concrete aggregate or phase-service behavior.

Development, production, decommissioning, HSE, exploration and process owners still need an explicit connection/resource/row-policy audit. Some phase helpers receive the reviewed connection from the orchestrator but retain repository paths without mandatory row policy; other registrations still use global/default connections. Existing dashboard estimates/status heuristics and numerical/domain interpretation also remain unverified. This slice cannot establish complete lifecycle/company isolation or numerical accuracy. Extend those existing owners before promotion.

## Focused evidence and test ownership

The final affected gate passed **166 tests, 0 failed, 0 skipped, in 58 seconds**: 38 lifecycle guard/HTTP cases, 31 existing PPDM row-policy cases, 29 company/profile selector cases, 25 UI/shared-validator cases, 6 selector MVC/auth/binding cases and 37 existing account/profile cases. No new all-inventory run is claimed; Implementation 56 retains its separate historical combined checkpoint. The concrete orchestrator, production authentication/middleware, full API/Web DI and real installed database drivers remain outside that gate.

The 38 lifecycle cases include 18 direct guard cases and 20 Kestrel loopback cases. Direct checks use the actual profile, active account, company/module registry, SQLite persistence, Beep generic gateway, row policy and entitlement interpreter. Application permission and orchestrator behavior are explicit mocks. Cases cover first selection, anonymous/missing account, conflicting FIELD assertions, provider failure, preference/grant/account/binding changes during restoration, failed/wrong/cancelled restoration and permission revocation. They verify exact resolved actor/resource/permission, cleared context and absence of allowed observations after refusal.

The 20 loopback cases discover the actual complete FIELD controller and exercise all five lifecycle routes under anonymous, missing-selection, denied and allowed authority. They run actual MVC authorization filters, route dispatch and the real profile/company/account/generic gateway. Authentication uses an explicit fixture principal resolver; the production IdentityServer account mapping/standard role bridge is not simulated as certified. Domain aggregate methods are mocked and must be called only after successful restoration. No provider mutation occurs.

The existing recording PPDM/SQLite test fixture is extracted unchanged to `CompanyPPDMTestFixture.cs`, shared by the existing repository tests and linked into the normal API test target with `CurrentFieldLifecycleGateTests.cs`. These cases replace the obsolete API filter tests that supplied only cached FIELD/claim mocks. The normal API host target remains unverified because of missing external projects; no dependency project or production helper is fabricated. The focused build retains the known missing-analyzer warnings.

Logs: `%TEMP%/oilgas-lifecycle-field-gate-focused.log` (initial 13 cases), `%TEMP%/oilgas-lifecycle-field-http-focused.log` (74 passed before the final permission reread) and `%TEMP%/oilgas-lifecycle-field-final-affected.log` (final affected gate). The initial compile probes were repaired by linking actual module interfaces/contracts and using the installed xUnit data API. They are not passing evidence.

Reproduce the affected gate:

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --filter 'FullyQualifiedName~CurrentFieldLifecycleGateTests|FullyQualifiedName~CompanyPPDMRowAccessTests|FullyQualifiedName~CompanyFieldSelectionTests|FullyQualifiedName~FieldSelectionTests|FullyQualifiedName~FieldSelectionEndpointTests|FullyQualifiedName~RepositoryAccountProfileTests'
```

## Acceptance still required

- Compile and exercise the concrete orchestrator/registered host using actual dependencies; propagate reviewed connection, FIELD/resource authority and mandatory row policy through existing phase/helper owners, including direct phase endpoints and caches.
- Verify the production account resolver, standard role bridge, company middleware, API/Web registrations and remaining current-FIELD consumers. Accept genuine provider errors, unsupported table ownership and unselected FIELD states without fabricated success or automatic defaults.
- Complete Web transport/subscriber and browser/circuit behavior, retained-preference capacity/retirement, real provider and multi-process/concurrent revocation acceptance. Checks before/after a read do not form a distributed transaction or guarantee revocation atomically with aggregate construction.
- Continue durable lease status/full-interpretation revisions and creation reconciliation/fencing, canonical area/FK/driver acceptance, legal/operator/JV/financial owners and the wider CA/FG release gates.

No business schema change, handwritten SQL, EF migration, live migration or deployment is introduced. Business/module schemas remain Beep MigrationManager-owned; the existing approved identity/profile registry remains the persistence owner. No phase, full-plan, provider, browser, release or production-readiness closure is claimed.

Follow-up: [Implementation 58](Implementation-58-Company-Retirement-Request-Boundary.md) compiles and exercises the actual retirement owner with mandatory reviewed connection/FIELD/actor/revision and row policy. It removes alternate status/activity portfolios and false empty responses for missing metadata. Retirement extension schema/resource/mapping/domain/mutation acceptance and other phase owners remain open; the complete orchestrator/Program paths above are still source-only.
