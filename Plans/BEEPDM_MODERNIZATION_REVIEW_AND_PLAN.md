# BeepDM Modernization Review and Plan

Date: 2026-09-05. Scope: source comparison, phased plan, and implementation progress.

## Implementation progress - 2026-09-05

Implementation has started following approval. The original review below describes the pre-change state.

- Centralized direct Engine/Models package references on BeepDMVersion=3.1.1.
- Added opt-in source integration in Directory.Build.targets. Use `-p:UseBeepDMSource=true`; the default source root is the sibling BeepDM directory. Override with `-p:BeepDMSourceRoot=...` where needed. Ordinary builds remain package-based. Source-mode builds use the working tree, including existing edits; they are not a claim of an immutable release artifact.
- Verified the source-mode test assets resolve Engine and Models as projects, not package copies. The cached Engine 3.1.1 package records commit e5cc317d66d2a8b55c55b5e70bcf90bb25830605, which differs from the reviewed local HEAD.
- Corrected PPDM insert/update/batch-write/delete result handling and checked batch delete/upsert Commit results. Non-OK flags and missing results cannot be treated as success.
- Made Web use explicit remote registration even when configuration requests local mode. Auto registration for other hosts now inspects service descriptors without constructing an extra container or editor.
- Added real repository-method regression coverage and client-registration tests in Beep.OilandGas.PPDM39.Tests, registered in the solution and CI.
- PPDM and Client package-mode builds passed. All 15 repository/registration tests passed against the local BeepDM source. These tests do not exercise a real database or validate transaction isolation.

Verification command:

```powershell
dotnet test Beep.OilandGas.PPDM39.Tests/Beep.OilandGas.PPDM39.Tests.csproj -p:UseBeepDMSource=true -p:GeneratePackageOnBuild=false
```

Remaining work: request/editor isolation (B2), connection readiness and transaction ownership (B4), remaining background workflows and durable job recovery (B5), and full hosted/database verification. No claim is made that M0-M4 are complete.

LifeCycle now compiles in source mode after resolving duplicate seed methods, missing workflow definition properties, contract ownership cycles, stale accounting/choke references, and seed counter/type mismatches. Existing shared contracts were moved without changing their namespaces; no replacement copies were introduced. Workflow definition persistence/roundtrip tests remain required.

The user selected calculation of new field cost allocations. The facade now validates configured field cost centers, reads period costs, and invokes the existing cost allocation engine separately for capital and operating costs. The API forwards the selected connection and no longer overwrites calculated totals with request values. See FIELD_COST_ALLOCATION.md for configuration, semantics, and rollout limitations.

All nine new LifeCycle allocation tests pass against local BeepDM source. They exercise the real engine with a mocked datasource, including a single-support fixture for all four methods, source cost validation, field filter construction, and no writes during computation. Run `dotnet test Beep.OilandGas.LifeCycle.Tests/Beep.OilandGas.LifeCycle.Tests.csproj -p:UseBeepDMSource=true -p:GeneratePackageOnBuild=false`.

The UserAssetAccess mismatch is resolved using ASSET_ID only for FIELD assets. FieldAccessService now checks active/current-user grants, asset expiry, scope effective dates, and deny precedence. Lookup exceptions discard partial grants. Username SYSTEM no longer grants access without authoritative database assignments. Unsupported asset types return no field grants until an authoritative asset-to-field mapping is implemented. Commas are rejected in field IDs because the existing JWT field_scope serialization is comma-delimited.

Compatibility note: the existing field_scope claim cannot encode wildcard exclusions. With any effective field denial, global grants are conservatively reduced to explicit allowed fields; a wildcard denial returns no fields. Review assignments before rollout, including service accounts previously relying on the SYSTEM bypass. Existing JWTs retain their claims until expiration/revocation; live database re-evaluation and the broader standard-role bridge are not implemented by this resolver fix.

Nine UserManagement field-access regression tests pass in both local-source and package modes. The test project is included in the solution. UserManagement and LifeCycle tests have independent CI jobs so they can run while the API build is blocked. These use the real resolver/repository with a mocked datasource, not a hosted authentication pipeline.

ApiService now builds with zero errors in both local-source and package modes. Warnings remain, and this does not establish runtime readiness. Fixed aggregation alias collisions/nonnullable volume access, defaults/setup namespace drift, actual audit/telemetry APIs, seeding failure reporting, and the trial-balance constructor dependency. Calculation/drilling controllers and registrations now use the existing concrete services because their former interfaces are absent from this checkout.

Removed unused registrations for a nonexistent ModuleSetupWizardAdapter and an ambiguous IGLAccountService implemented by neither referenced concrete accounting service. The existing wizard/seeding path and concrete GLAccountService remain. EntityDefaultsProfileRegistry also does not exist: the manager can use configured defaults, and PPDM39DefaultsRepository retains its existing database/constant fallback, but PPDM-specific profile registration is not implemented. DefaultsManager still has static editor state; request isolation remains an unresolved runtime gate.

BeepSyncService now maps Id/EntityName/DestinationEntityName/MappedFields and calls SyncDataAsync with a DataSyncSchema. It checks the returned error flag, child errors, and schema status; bulk sync collects actual per-schema results rather than unconditional success. Cancellation propagates; stale reconciliation is cleared. Unsupported bidirectional/composite-key configurations are rejected. Initial schemas remain templates until key/sync mappings are configured; incremental mappings require an explicit watermark. No actual sync was executed against a database.

Focused PPDM tests compile the actual sync adapter source independently of the API test suite (no fake framework contracts). Both package and local-source modes pass 21 tests, including six sync-adapter tests.

API test modernization: restored module-owned INodalAnalysisService, IChokeAnalysisService, and the drilling-create IDrillingOperationService boundary from actual implementation signatures. Their concrete services implement these interfaces; DI forwards interface resolutions to the existing scoped concrete registrations. The nodal controller and choke orchestration now consume interfaces, preserving meaningful strict-mock tests without making implementation methods virtual. Updated test imports for those contracts and ModuleSetupOrchestrator.

The API suite initially ran 355 tests: 347 passed and eight exposed a real exploration step-data mapping gap. RunExplorationWorkflowStepAsync populated Data but not DataJson/StepType/Status. It now fills those consistently, preserving the complete payload and serializing empty input as an empty JSON object. All existing tests pass, plus two JSON-deserialization cases: 357 passed, zero failed/skipped in both package and local-source modes. TRX results are in TestResults/api-modernization.trx and TestResults/api-modernization-source.trx. These are unit/controller tests, not proof of a running hosted application; the existing mock-only cross-module chain tests remain illustrative, not end-to-end workflow evidence. Hosted API/database verification, editor ownership, and background migration work remain incomplete.

Removing global NU1605 suppression exposed existing Microsoft.Extensions 10.0.9/10.0.10 and SkiaSharp 3.119.4/4.150.1 conflicts. The original suppression is retained until those non-core dependencies are reconciled and tested. Core references are aligned, but the entire package graph is not yet clean.

### Background migration ownership

StartSchemaMigrationExecutionAsync no longer launches a detached Task.Run that captures the request's PPDM39SetupService. It submits immutable job arguments with a static handler to a host-owned ScopedBackgroundOperationQueue. The worker resolves PPDM39SetupService within its own async DI scope and disposes that scope only after execution returns. It rechecks approval, plan/manifest identity, and protected-tier policy before running. A failed engine result becomes a failed job, rather than an unobserved success.

The queue has one worker, capacity for 64 pending jobs, duplicate-active-key rejection, and bounded retention of 256 terminal status entries. Admission fails when the queue is full or stopping. Migration progress exposes queued, failed, and cancelled job states even when the worker cannot open the datasource. Engine checkpoint creation and execution tokens are retained. Queue availability is mandatory for the asynchronous migration endpoint; other hosts without it receive an explicit failure instead of detached execution.

Shutdown stops admission, cancels pending jobs, and waits for active work before scope disposal. BeepDM's synchronous migration calls cannot be interrupted through a cancellation token: a blocked driver can therefore delay graceful shutdown indefinitely. Forced process termination still requires checkpoint review/recovery; it is not a rollback guarantee.

This fixes service-scope ownership, not editor isolation: BeepDM still supplies a shared editor and static DefaultsManager state. The queue serializes only jobs submitted through it; direct execution endpoints, requests, and imports are not covered by that serialization. Setup wizard jobs now use this queue as described below. Job status and plan-session lookup remain in memory, so host restart is not yet a complete durable resume workflow even though engine checkpoints are persisted. Duplicate admission races may leave an unused newly-created checkpoint; they do not schedule duplicate active jobs for the same key.

Five queue tests cover fresh scope ownership after request disposal, duplicate rejection and failure reporting, shutdown waiting/disposal and queued cancellation, bounded admission, and recovery after service-resolution failure. API suite in both package and local-source modes: 362 passed, zero failed/skipped. TestResults/background-ownership.trx and TestResults/background-ownership-source.trx record those runs. No live migration, database write, or hosted shutdown against an actual provider was exercised. Next: migrate the setup wizard/import tasks, establish isolated editor/datasource ownership, and persist/recover job and approval state before production rollout.

### Setup wizard ownership

SetupWizardController now submits an immutable connection/actor job to the same host-owned queue. A scoped SetupWizardJobRunner resolves the executor; the singleton coordinator holds only job metadata, cancellation, and immutable status snapshots. Duplicate queued/running starts are rejected. Cancellation flows into module seeding, terminal status ignores late progress, and scope-disposal failures override an apparent worker success. Provider operations that ignore cancellation still delay worker shutdown.

The old detached controller Task.Run, static adapter fields, and invalid final-step dependency on an unregistered schema-setup step were removed. The endpoint runs existing module seeders on the explicitly selected configured connection. It does not automatically provision drivers or approve/run a schema migration. Module-specific effects still require provider validation. The old global setup-wizard-state.json skip list is not reused across connections; migration checkpoints remain separate.

Start requires an authenticated NameIdentifier/sub actor, with no SYSTEM fallback. Preflight and status now inherit the existing Admin/Administrator role guard instead of allowing anonymous access. Routes and status field names remain; the step list reports the actual single PPDM Modules stage. A module's partial row errors now fail the wizard even if its Success flag is true.

The Blazor wizard now recognizes Queued/Cancelling/Cancelled states, resumes polling active jobs, waits for server-confirmed cancellation, and has the matching standard admin route guard. Removed its misleading promise of automatic non-destructive schema creation. Fixed three existing MudChip type-inference errors on this page. The Web build still fails with 640 errors elsewhere, including malformed Razor in GenericCrudPage. No wizard-page errors were reported in the final build log (TestResults/wizard-web.log); this is not a successful client build or browser validation. The app was not started.

Verification covers actor/connection forwarding, duplicate admission, queued and running cancellation, retry after disposal, sanitized failures, late progress, scope-disposal failure, rejected admission, and real executor result handling with mocked modules/datasources. All 371 API tests pass in both package and local-source modes, with zero failures/skips (Beep.OilandGas.ApiService.Tests/TestResults/wizard-package.trx and wizard-source.trx). Wizard state remains in memory and cannot resume after restart. Import jobs, editor isolation, durable recovery, and hosted/provider integration tests remain pending. No live database operations were performed.

### CSV import ownership

The CSV upload endpoint no longer captures its controller/repository dependencies in detached Task.Run work. It validates the PPDM entity and copies the upload into owned bytes before admitting a CsvImportJob. The shared queue resolves a scoped CsvImportJobRunner and ICsvImportExecutor. Only the worker creates the randomly named temporary CSV; it awaits repository completion and deletes the input in finally. User filenames never become server paths. Failure checks include both ErrorCount and the Errors collection, including errors returned without an incremented count.

Compatibility changes: uploads are limited to 2 MiB, including an actual-stream check plus HTTP/form limits. Larger import support needs a bounded durable spool, not a higher unbounded memory limit. Up to 64 pending jobs can retain approximately 128 MiB of CSV bytes, plus the active job, request buffering, and parsed repository data. Caller-supplied operation IDs now receive 400; the server assigns IDs. The legacy userId query parameter remains accepted but is ignored for audit identity, which comes from NameIdentifier/sub. Missing actor claims return 401. Queue saturation/stopping returns 503 without retaining a temporary file. The existing Web caller does not supply operation IDs.

Progress polling reconciles queue failure/cancellation even when scope resolution fails or shutdown discards a pending job before the runner starts. Existing SignalR broadcasts remain best-effort and are not a durable event stream; clients should poll after reconnect. The repository API cannot interrupt an active CSV import. Shutdown waits for it; partial writes can survive failures and retries are not idempotent. Pending payloads/status are lost on restart, and forced termination can leave an active temporary file. No database or provider operation was executed during this change.

Verification: 384 API tests pass in both package and local-source modes, with zero failures/skips (Beep.OilandGas.ApiService.Tests/TestResults/import-package.trx and import-source.trx). Thirteen added cases cover file ownership/cleanup, error-list failures, pre-execution cancellation, actor/connection forwarding, admission rejection, invalid and oversized uploads, and progress reconciliation for jobs that never reach the runner. These are unit tests with mocked import execution, not live CSV/database verification.

Scope remains limited to CSV imports. Script/setup and workflow background operations, detached progress broadcasts, shared editor isolation, import authorization granularity, durable recovery, and hosted/database integration tests remain outstanding. Export execution is unchanged.

### Unimplemented execution boundaries

Review of the remaining detached operations found that setup CopyDatabaseAsync is a failure stub and all four PPDM39 workflow handlers (importcsv, validate, qualitycheck, version) merely delayed and returned success. The old workflow dependency handling could also skip unmet dependencies without recording failure. Moving these implementations onto a queue would preserve misleading behavior rather than deliver real execution.

The setup copy-database and workflow execute endpoints now return HTTP 501 for valid requests, without creating an operation/progress record or starting background work. Invalid requests retain 400 responses. Direct PPDM39WorkflowService.ExecuteWorkflowAsync callers receive an explicit failed result with no step results, including for an empty workflow. Removed the placeholder handlers, ineffective private dependency scheduler, and detached execution-history write from this execution path. Definition management and explicit history APIs remain unchanged. This is an intentional compatibility change: clients must not interpret these endpoints as available execution features.

Re-enablement plan:

1. Define supported copy scope and mapping from explicit source/target entities; reuse the current BeepDM ETL/sync boundary only after validating keys, types, write policy, target tier, and authorization. Do not silently substitute a broad sync for database copy.
2. Implement workflow handlers against actual import, validation, quality, and version services with typed inputs, explicit actor/connection/field context, and observable failure results. Uploaded data must use owned bounded storage, not arbitrary client-supplied server paths.
3. Validate the whole dependency graph before admission: unique step IDs, existing dependencies, cycle rejection, topological ordering, and explicit skipped/failed outcomes. Use one operation identity consistently across progress, execution, and history.
4. Queue immutable job arguments in an owned scope; await history persistence before scope disposal. Define cancellation, partial-write, idempotency, and persistence-failure semantics before returning success.
5. Add hosted authorization, real-provider, restart/retry, and mixed-success workflow tests before restoring successful admission responses. Unit tests alone are not an activation gate.

Verification: 395 API tests pass in both package and local-source modes, with zero failures/skips (Beep.OilandGas.ApiService.Tests/TestResults/unavailable-package.trx and unavailable-source.trx). Eleven new cases cover each placeholder operation, unknown/empty workflows, endpoint validation, and rejection without progress creation, field-context mutation, or datasource access. No database operations were executed. Remaining Task.Run occurrences in progress tracking are best-effort broadcasts/retention, not covered by this change. Shared editor isolation and durable job recovery remain open.

### Progress worker ownership

ProgressTrackingService is now a singleton BackgroundService, shared by its IProgressTrackingService and hosted-service registrations. Replaced its eleven detached Task.Run broadcast/retention blocks with a separate notification channel and awaited broadcast/cleanup loops. This worker does not use or block the database-operation queue. A failed SignalR send is logged and processing continues; shutdown cancels the send token and abandons pending notifications rather than extending shutdown to flush them.

The channel retains at most 256 serialized snapshots and drops the oldest notification on overflow. JSON snapshots preserve the app's current SignalR camel-case wire shape and cannot change when a later update mutates the stored status. State methods are serialized under one lock, incoming progress is copied, and polling returns detached snapshots. Late operation updates/cancellation cannot reopen or overwrite terminal operation state. Notifications remain best-effort: even a terminal broadcast may be dropped, so polling remains necessary.

Retention is independent of broadcast success: a supervised 30-second cleanup loop removes terminal operations after five minutes and completed workflows after ten minutes; polling also applies expiry. There are no per-completion delayed tasks. Active records and operation groups still require a broader retention policy, and the count-bounded channel is not a byte-size budget. CancelOperation still describes progress state; it is not a general cancellation mechanism for database work. Host shutdown relies on SignalR respecting its cancellation token and the normal hosted-service timeout.

Six added worker tests cover ordered snapshots, overflow with retained polling state, send-failure recovery, cancellation during shutdown, input/output isolation with terminal protection, and retention without delivered broadcasts. Existing CSV progress tests now explicitly start/stop the worker. All 401 API tests pass in package and local-source modes, with zero failures/skips (Beep.OilandGas.ApiService.Tests/TestResults/progress-package.trx and progress-source.trx). No live SignalR client, database, or hosted application was exercised. Shared editor isolation, durable recovery, and the previously documented Web build failures remain unresolved.

### Web build foundation

The Web package-mode build is now down from the previously recorded 640 errors to 69. Fixed a missing catch brace in GenericCrudPage, missing try-block closing braces in ProgressDisplay and PPDM39DatabaseWizard, an unescaped display-text @ in AssetPortfolio, and graph-model declarations outside an @code block. Added the existing ThemeBranding namespace where BrandingConfig was unresolved. These changes restore parsing/type visibility; they do not verify the affected UI workflows.

Replaced obsolete developer-specific IdentityServer.Shared/ThemeBranding project paths with the actual sibling Beep.IdentityServer and Beep.Web repositories. The Web project exposes IdentitySharedProject and ThemeBrandingProject MSBuild properties for alternate layouts. These are shared library references, not references to the IdentityServer host. No identity authorization logic was moved, and no sibling source files were edited. Builds still require these external repositories and BeepWeb's Razor components; packaging/pinning those shared dependencies remains a reproducibility task.

Verification command: `dotnet build Beep.OilandGas.Web -p:UseBeepDMSource=false --no-restore --verbosity quiet`. The final build reports 69 errors and 22 warnings; details are in TestResults/web-foundation.log. Intermediate logs record 614 errors after the first brace fix, 99 after reference repair, and 84 after the event-handler brace fixes. Error totals are diagnostic counts, not independent defect counts.

Next Web steps: migrate MudChip/MudList/MudListItem/MudSwitch generic arguments based on their actual bound values; align stale UserProfile references with the current shared contract; resolve the remaining SlaStats and Razor markup issues; then rebuild to expose any later compilation errors. Do not assume this is the complete remaining list because Razor failures can suppress downstream diagnostics. The Web app was not started, no browser tests were run, and no databases were changed.

### MudBlazor compatibility

Used the supplied local reference, located on this checkout at the sibling Beep.Web/MudBlazor_Docs directory (Chips.txt, CheckBox.txt, and switch examples), plus installed MudBlazor 9.5.0 XML API documentation. Resolved all 64 reported generic-inference diagnostics: 25 display chips, 32 navigation list items, six navigation lists, and one boolean switch. Display-only chips and navigation lists now explicitly use string; the boolean switch uses bool.

Migrated eight boolean controls from Checked/CheckedChanged or @bind-Checked to Value/ValueChanged or @bind-Value, including the DataImport switch, setup execution/connection switches and script checkbox, CSV import options, and asset-access inheritance. Explicit bool type arguments previously let several obsolete bindings avoid inference errors; changing only the type would not have corrected those bindings.

Final package-mode Web build: five errors, 22 warnings (TestResults/mudblazor-final.log), with no RZ10001 diagnostics. A project-wide Razor search found no remaining Checked/CheckedChanged bindings on switches/checkboxes. Build remains blocked by three removed UserProfile references, SlaStats declared outside Razor code, and a literal less-than sign parsed as markup in AssetPortfolio. No browser interaction tests or app startup were possible, and later compilation stages may reveal additional issues. API/backend code and databases were not changed in this phase.

## Baseline

Application: `C:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas`.

Reference source: `C:\Users\f_ald\source\repos\The-Tech-Idea\BeepDM`.

Reference HEAD: `251d751f`. The BeepDM worktree has existing modifications in assembly-loading sources and generated documentation. Those modifications were not changed. A released 3.1.1 package is not assumed to contain every feature or fix in this checkout merely because the project version is 3.1.1.

The application is partially modernized, not uniformly legacy. It already uses UnitOfWorkFactory, DefaultsManager with PPDM profiles, and MigrationManager with planning/checkpoints. Retain those integrations where compatible. Modernization should repair the boundaries around them and consolidate duplicated behavior without replacing domain logic indiscriminately.

This comparison did not build BeepDM, replace application packages, execute migrations, or change a database. The previous application build remains blocked by external dependencies. Findings below distinguish verified source behavior from migration risks that require runtime verification.

## Findings

### B1 - P1: Mixed core dependencies and suppressed downgrade diagnostics

- ApiService, Web, Client, and LifeCycle request Engine 3.0.1 and Models 3.0.0.
- PPDM39, PPDM39.DataManagement, and Branchs request Engine/Models 3.1.1; Models and PPDM.Models request Models 3.1.1.
- Local BeepDM Engine and Models project versions are both 3.1.1, targeting net8.0/net9.0/net10.0.
- `Directory.Build.props:3` suppresses NU1605 across the application solution.

Evidence: `Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj:21`, `Beep.OilandGas.PPDM39/Beep.OilandGas.PPDM39.csproj:33`; local `DataManagementEngineStandard/DataManagementEngine.csproj` and `DataManagementModelsStandard/DataManagementModels.csproj`.

Impact: dependency declarations disagree, while downgrade diagnostics are hidden. Actual resolved assets and plugin assembly identities must be inspected after restore; this review does not assert that every runtime loads two engine versions.

Action: pin one tested Engine/Models baseline, align direct and transitive references, validate all required datasource plugins, and remove broad downgrade suppression once the graph is corrected. Use a versioned build from a known source revision for changes not already published. Avoid mixing local projects and package copies of the same core assembly in one host.

### B2 - P1: Request isolation cannot be inferred from registration labels

Application evidence: `Beep.OilandGas.ApiService/Program.cs:175` calls AddBeepServices without setting lifetime and comments that scoped is the default.

Local engine evidence: `DataManagementEngineStandard/Services/RegisterBeepinServiceCollection.cs:762` defaults ServiceLifetime to Singleton. At `:700`, scoped registration uses `services.AddScoped<IBeepService>(_ => beepService)`, returning the pre-created instance; registration also retains a static cached service.

Impact: against this source, selecting Scoped does not create distinct BeepService instances per request. The application must not assume independent mutable editor/connection state based on the DI descriptor. The exact lifetime behavior of the currently resolved older package remains unverified.

Action: test reference identity and state isolation across scopes and hosts before adoption. Determine and document which metadata/driver catalogs may be shared and which editor/datasource/transaction state must be owned by an operation. Resolve any upstream registration defect or provide a narrowly scoped application integration with tested ownership. Do not treat a one-line lifetime change as the fix.

### B3 - P1: Repository can report a failed write as successful

Evidence: `Beep.OilandGas.PPDM39/Core/PPDMGenericRepository.cs:942` and `:1017`, plus the batch loops. The methods check only whether result.Errors contains entries. They do not require result.Flag to indicate success and also accept a null result.

Local contract: `DataManagementModelsStandard/ConfigUtil/IErrorsInfo.cs:11` exposes Flag independently from its Errors list.

Impact: a datasource returning Flag=Failed with an empty error list reaches the success return path. This is a source-level contract handling defect; reproduce it in a targeted test before changing behavior.

Action: centralize Beep operation-result handling; require explicit success for writes, preserve useful diagnostics, and propagate failure to API responses. Test failed flag with no child errors, null result, nested errors, and normal success. Keep programming/configuration exceptions distinguishable from routine datasource failures.

### B4 - P2: Connection and batch semantics vary across repository paths

Evidence: direct write paths at `PPDMGenericRepository.cs:936` obtain a datasource and check null but not open state. InsertBatchAsync at `:957` and UpdateBatchAsync at `:1032` execute individual writes without opening an explicit transaction. Other repository paths use UnitOfWorkFactory and Commit.

Local source: `DataManagementEngineStandard/Editor/DM/DMEEditor.cs:425` provides OpenDataSource, while GetDataSource at `:468` delegates lookup. Lookup is not itself a guarantee of an open connection.

Impact: connection readiness depends on driver/caller behavior. Batch writes have no locally established atomicity; later failure may leave earlier writes persisted unless an external transaction exists.

Action: establish one tested datasource acquisition path and explicit operation ownership. Define whether each batch is atomic or reports partial progress. Use verified datasource/UOW transaction capabilities; do not assume Commit alone guarantees rollback or that every provider supports transactions.

### B5 - P1 migration risk: Background schema work captures request-owned services

Evidence: `Beep.OilandGas.PPDM39.DataManagement/Services/PPDM39SetupService.cs:653` starts an unawaited Task.Run and invokes instance methods using the service's editor. It already uses engine execution checkpoints.

Impact: if modernization introduces genuinely scoped/disposable editor ownership, migration work can outlive its request scope. The untracked task also offers no host-managed restart/retry guarantee. Checkpoints alone do not schedule recovery.

Action: preserve engine planning/checkpoints, move execution into an owned background worker/job with a fresh DI scope, record initiating actor and connection, persist job status, and test interruption/resume. Keep schema execution in ApiService.

### B6 - P2: BeepDM guidance and source APIs disagree

The registration skill and Docs/ServiceRegistration.md advertise AddBeepForWeb and UseBeepForWeb. Repository-wide C# search found only commented examples of these names, not implementations. The actual web extension file defines logging/audit helpers. The supported generic registration method is present at `RegisterBeepinServiceCollection.cs:347`.

The actual fluent interface also uses WithDirectory and WithAppRepo, rather than some names in the skill examples.

Action: compile-check every proposed API against the pinned source. Do not mechanically convert startup to helpers that exist only in documentation. Update guidance separately after the integration behavior is established.

## Modernization boundary

- Web stays an API client. Do not add BeepDM database services to Blazor because a framework example shows them.
- ApiService owns database connections, setup, migrations, imports, and business persistence.
- Retain PPDM-specific identifiers, defaults, audit columns, soft-delete conventions, and module ordering.
- Reuse current engine migration/defaults/UOW facilities where verified; do not rewrite existing adapters solely to change style.
- Respect the main plan's canonical data-ownership decisions. Updating BeepDM does not settle which application assembly owns domain contracts.
- Treat shared Studio/setup services as candidates, not automatic replacements: the sampled registration uses singleton services resolving an editor. Validate lifetimes and API boundaries before adopting them in a server host.

## Execution sequence

### M0 - Establish a reproducible engine baseline

1. Record the BeepDM commit and relevant local changes; select an immutable baseline for the integration build.
2. Inventory Engine, Models, Container, AssemblyLoader, logger, shared UI, and datasource dependency identities across the application and external shared projects.
3. Restore/build the selected framework target and run applicable upstream tests in a controlled output location. Account for local BeepDM post-build/pack targets that copy artifacts outside the repo.
4. Choose either consistently managed source references for a development harness or uniquely versioned packages from the pinned source for CI. Do not overwrite an existing package version with different contents.
5. Align dependencies, restore with visible NU1605 diagnostics, and retain resolved asset reports.

Exit: one identifiable core baseline, no unresolved downgrade/duplicate-identity issue, and a minimal consumer that compiles. Estimate: 1-3 engineer-days, excluding upstream dependency blockers.

### M1 - Prove server lifecycle and connection ownership

1. Test the local registration's cached-instance behavior across two DI scopes and two service providers.
2. Define safe shared configuration/catalog state versus isolated operation state.
3. Implement the necessary lifecycle correction before enabling concurrent application requests.
4. Normalize connection creation/persistence through ConfigEditor and validate open state before operations.
5. Test distinct users/connections, failure and reconnect, scope disposal, cancellation, and application shutdown.
6. Remove Web local auto-mode and secondary BuildServiceProvider construction from application registration.

Exit: concurrent requests cannot change each other's selected datasource or transaction state; resources have a verified owner and cleanup path. Estimate: 3-5 engineer-days, re-estimate if framework changes are required.

### M2 - Harden the PPDM repository adapter

1. Fix B3 with focused contract tests, then trace all write/delete/commit result handling.
2. Establish explicit connection readiness consistently for direct and UOW operations.
3. Define transaction boundaries for batches, workflow writes, allocation/posting, and period close.
4. Validate PPDM keys, type mapping, defaults, audit fields, and soft delete against the new engine.
5. Verify wrapper capabilities actually used by the app and dispose cached UOW objects according to ownership.
6. Test failed rows, repeated requests, concurrent updates, and retry behavior against the deployment provider.

Exit: failure cannot become success; transaction behavior is explicit; representative existing PPDM operations retain their data semantics. Estimate: 3-6 engineer-days.

### M3 - Modernize setup, migration, and import orchestration

1. Inventory which new engine migration APIs the existing setup service already uses; retain working plan/checkpoint integration.
2. Address B5 through background work with an independent scope and persisted status.
3. Verify provider capability checks, generated DDL, plan validation, dry-run results, checkpoint resumption, and seed idempotency on a database copy.
4. Use verified driver registration helpers; account for providers whose extension methods live in separate datasource packages.
5. Assess import/mapping adapters separately: move reusable mechanics to current engine facilities while retaining PPDM validation and field authorization.
6. Compare custom setup orchestration against current engine wizard facilities; adopt only parts that remove duplication and satisfy server lifecycle requirements.

Exit: setup/import progress survives the HTTP request; restart/resume is demonstrated; a repeated migration/seed does not damage or duplicate existing data. Estimate: 4-7 engineer-days.

### M4 - Integrate and verify one complete workflow

1. Use field/well lookup -> production insert/update -> reload -> approval as the first compatibility slice.
2. Exercise it through Web -> API -> new BeepDM baseline -> representative database.
3. Add the real work-order/AFE handoff and failure rollback case.
4. Run domain suites and remaining provider smoke tests; identify unsupported providers explicitly.
5. Document package/source versions, rollback steps, configuration changes, and operational limitations.

Exit: the selected workflow passes hosted HTTP and database tests; old data remains readable; the build is reproducible without developer-specific paths. Estimate: 3-5 engineer-days.

These estimates overlap the main plan's foundation phases and are not additional fixed commitments. Re-estimate after M0 and M1.

## Required regression cases

| Case | Expected evidence |
|---|---|
| Driver returns Failed with no child errors | Repository/API reports failure, never success |
| Closed or unavailable datasource | Controlled failure before attempting a write |
| Second row fails in atomic batch | First row is rolled back |
| Provider lacks atomic batch capability | Operation rejects the guarantee or exposes documented partial results |
| Two simultaneous requests choose different connections | No datasource, field, or transaction leakage |
| Request ends during schema execution | Job continues in its owned scope or stops with recoverable recorded status |
| Host restarts with an incomplete migration | Persisted checkpoint can be resumed without repeating completed changes |
| Defaults and soft deletion | Required PPDM audit/default columns and ACTIVE_IND remain correct |
| App-owned role denies a write | No engine operation is invoked |
| Plugin loads against aligned core | No type identity, missing method, or driver resolution failure |

## Relationship to the main plan

Begin M0 immediately as part of Phase 0. Carry M1-M3 through the architecture/data foundation, while fixing the existing role and audit vulnerabilities. Use M4 as the readiness gate for broader operational feature work. The modernization is an implementation-pattern and runtime-ownership migration, not just a NuGet version bump.
