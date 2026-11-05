# Implementation 55 — FIELD-bound lease portfolio

Follow-up: [Implementation 56](Implementation-56-Company-FIELD-Selection.md) repairs the existing shared selector profile persistence, authenticated company/account/current-list-set boundary, shared DTO ownership and company-aware selector state. Its focused core/UI/MVC/static-component evidence is distinct from source-only Web transport/host wiring and remaining lifecycle request restoration. Historical counts and selector findings below describe this checkpoint; Implementation 56 and the master tracker own current selector status. Durable status/creation/provider/circuit and wider plan gates remain open.

Date: 2026-10-05. Execution owners: CA-03/05/06, FG-01/10/11/12/20/50; [master tracker](../../MASTER-TODO-TRACKER.md) owns status. This extends the existing lease service, API resource, SDK, Web adapters and operations page from [Implementation 54](Implementation-54-Lease-Creation-Intent-and-Reconciliation.md). The company plan remains active.

## Problem and resulting behavior

The old lease operations endpoint accepted reads by lease ID alone, while its current-field controller mirror did not pass the orchestrator's FIELD into the canonical service. A company actor holding access to two FIELDs could therefore request a lease from B through a screen presenting A. The page swallowed selection/read failures into an empty collection and rendered zero counts, retained old data across FIELD changes, discarded the returned record evaluation, and did not bound delayed status confirmations to the original context.

One canonical controller now uses `api/fields/{fieldId}/lease-acquisition`. All six operations carry an explicit FIELD: available, evaluate, creation context, creation, read-only creation reconciliation and status. The duplicate implicit-current-field lease controller is removed. Query/body assertions that disagree with the route are rejected; the existing canonical core requires FIELD for reads and status writes and includes it in the transactional reread. No second selector, persistence store, authority mechanism or compatibility endpoint is introduced.

SDK/core evaluation and status signatures now require FIELD. Web adapters delegate the exact identity through the existing SDK/company-captured transport; SDK filters are copied before routing and use MVC dictionary keys (`filters[LeaseName]`, for example) so they actually reach the controller. Known repository callers were updated. This is a route/SDK contract change: external consumers must move with the release; do not promote independently or assert compatibility without their acceptance.

## Ownership and authority

The existing company lease gate resolves the request's application account/company and checks current Administrator plus actual FIELD WRITE before mutation. The canonical service independently checks live company, database binding, resource, association, reference code and transaction authority. Controller `[Authorize]` and standard Administrator role attributes remain in place. IdentityServer remains authentication-only. Controller tests use actual source plus explicit gate fixtures; they do not certify middleware/OIDC claims transformation.

Shared contracts and `LeaseSummaryValidator` live in TheTechIdea.Data. The validator preserves recorded status and raw calculated interest, validates full subtype/ID/FIELD/date-zone identities and rejects duplicate complete keys, missing/malformed data and inferred financial working interest. Snapshotting copies the same DTO rather than creating a parallel model. No business database/schema change, SQL, migration or live write occurs. Business/module schema ownership remains Beep MigrationManager; the existing identity/access registry's EF ownership is unchanged.

## Portfolio behavior

`LeasePortfolioState` owns evidence for the current selected FIELD, rather than selection or authority. It captures one company generation, cancels superseded portfolio reads, immediately clears prior records on reload/selection change and suppresses late selection/read/evaluation/command results. It copies transport records and retains original row identity so unrelated, old or identity-mutated rows cannot become action targets. The page renders counts only for a confirmed current response; genuine empty, unavailable, loading and failure states are distinct.

The existing `IDataManagementService` implements a narrow `IFieldSelectionContext` facet; DI resolves it to the same existing scoped instance. The page listens to its current FIELD event, invalidates synchronously and loads that explicitly selected FIELD. It displays complete LAND identity, date-zone provenance, raw calculated interest and the returned record details. Standard role controls remain; selected-company roles guard UI commands independently of API authorization.

Status confirmation waits for the actual SDK call. A confirmed same-context write clears old facts and asks for a fresh read rather than changing a row optimistically. A status target is reserved before the await, so switching away and back cannot submit a second in-flight write to it. Failed or uncertain commands stay visibly blocked for that target in this component session even after reload. A delayed confirmed write releases its reservation but does not create a confirmation under another FIELD/company generation. No automatic write retry, read-as-command-reconciliation, circuit-loss recovery or durable status ledger is claimed.

The creation dialog continues using its original frozen intent from Implementation 54. A changed selected FIELD leaves the original reviewed FIELD visible and all submit/reconcile/edit actions disabled; a prompt identifies the FIELD to return to. Its pending command identity is not replaced by a new FIELD selection.

## Focused verification

The initial source-linked controller/client/routing/dialog checks passed **55, with zero failures/skips**. The portfolio state checks passed **28, with zero failures/skips**. Expanded core/controller/client/routing/dialog/portfolio checks passed **222, with zero failures/skips**, in 4 minutes 49 seconds. Final combined company gate: **1463 passed, 0 failed, 0 skipped**, adding 56 to the previous 1407, in 6 minutes 44 seconds. After the final MVC filter serialization and company-selection prompt changes, all **74 affected SDK/state/HTTP checks passed**, with zero failures/skips, followed by that final combined gate. Git diff --check passed; all 212 relative links resolve across the seven updated documents.

Six additional Kestrel loopback checks passed using the actual controller/SDK and ASP.NET model binding/standard role middleware: both supported SDK filters reach the controller; anonymous creation-context requests receive 401, readers 403 and fixture Administrators 200; route FIELD and JSON status reach the configured actor gate/core. Authentication, company gate and domain service are explicitly mocked boundaries in this fixture. It discovers only the actual lease controller and does not substitute missing host projects or claim production OIDC/company-middleware/provider conformance.

`Scripts/Test-LeaseWebMarkup.ps1 -RenderPortfolio` now compiles the complete exact operations page plus its actual KpiCard and StatusBadge against MudBlazor 9.7.0 and the focused production-state assembly, with **zero warnings/errors**. It checks source hashes after compilation. No callback/behavior stand-ins replace page code.

Eight exact-page static HtmlRenderer checks passed: English and French recorded numbers/identity/status, confirmed empty, read failure, missing FIELD, missing company, loading and uncertain status after reload. The render fixture supplies documented mock boundaries and standard role claims solely for static rendering; it does not execute live auth/HTTP, registered Web host DI, browser interaction, JS or a Beep provider.

Expanded tests cover an actor holding both A/B FIELD grants, rejection of B lease read/status through A, the actual controller using the real canonical service/current SQLite application-authority gate, actor-role revocation, missing FIELD despite a lease ID, escaped route identities/filter copying, single-attempt PUTs, late results, cancelled reads, recorded DTO isolation, full-key malformed/duplicate/foreign evidence, reader restrictions and status reservation across selection/reload changes.

Evidence logs: `%TEMP%/oilgas-lease-field-routing-focused.log`, `%TEMP%/oilgas-lease-field-portfolio-focused.log`, `%TEMP%/oilgas-lease-portfolio-state.log`, `%TEMP%/oilgas-lease-http-binding-focused.log`, `%TEMP%/oilgas-lease-field-all-focused.log` (earlier 1457 checkpoint), `%TEMP%/oilgas-lease-field-final-ui-http.log`, `%TEMP%/oilgas-lease-field-final-all-focused.log`, `%TEMP%/oilgas-lease-page-final-focused.log`. Page artifacts: `%TEMP%/oilgas-lease-page-edf34a7478284f6fa95f5b9dd1ff760c` and its `-render` companion.

## Remaining acceptance and next owners

Reproduce the final focused gate and page check from this checkout (the separate temporary output avoids overlapping Windows DLL locks; the second command reads the actual completed focused assembly):

```powershell
$leaseFocusedOutput = Join-Path $env:TEMP 'oilgas-lease-http-focused-output'
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false "-p:OutputPath=$leaseFocusedOutput/" --no-restore
& Scripts/Test-LeaseWebMarkup.ps1 -RenderPortfolio -FocusedOutputDirectory $leaseFocusedOutput
```

No missing project or production authentication helper is fabricated. The focused build retains known missing-analyzer/member-hiding warnings; the isolated exact-page compilation has zero warnings/errors.

- Repair the existing shared selector's authenticated company/account/FIELD persistence and current/list/set endpoints. `IFieldOrchestrator` is API-request scoped, with a private current FIELD initialized null; set-active currently changes that instance. No durable owner was established by this source review. Its current list endpoint still has global PPDM39 routing; orchestrator access checks can be skipped when its optional access service or current actor is absent. `FieldSelectionState` does not subscribe to company changes, so its already-loaded FIELD list also needs immediate invalidation/cancellation. Existing profile preferences are global account JSON, not demonstrated company/database-bound selection authority; do not silently promote them into a legal/resource grant. A missing current selection remains explicit; no global default, cached fallback or second selection store is introduced here. FIELD list/database routing and the selector's own HTTP/browser behavior remain open.
- Bind status commands to a reviewed full interpretation and database/configuration revision, expected record/history version and durable command identity. FIELD route binding prevents the reproduced A/B substitution; it does not provide optimistic concurrency, durable uncertain-write reconciliation, subtype/configuration-change fencing or lifetime retention. The session reservation is a UI fence, not a durable ledger.
- Accept the real Beep driver transaction ownership surface and canonical LAND area/parent/FK requirements from Implementations 52/53 before live mutation promotion. The inspected installed binary still lacks the ownership property. No provider shortcut or source-less version substitution is added.
- Complete real host DI, current account/company claims bridge, production endpoint discovery/HTTP authorization, browser dialogs/FIELD switching, accessibility, regional date-zone/localization/RTL, real-provider/multi-process history concurrency and circuit/navigation recovery evidence. Focused MVC/filter/role evidence above is bounded to its documented fixture. The full host's missing shared projects and PartyIdClaims drift are not fabricated or repaired for this focused gate.
- Repair the remaining advanced lease/title/operator/JV/financial/domain paths and wider foundation/release plan. Static status badges and recorded ACTIVE counts do not establish legal, operational or financial approval. The retained legacy/advanced lease service owners are not certified by these tests.

No phase, full-plan, host, provider, runtime, release or production-readiness closure is claimed.
