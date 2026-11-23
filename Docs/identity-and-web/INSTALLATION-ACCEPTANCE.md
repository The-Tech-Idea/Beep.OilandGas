# OilGas Installation Acceptance

Source dependencies for this installation slice: BeepDM
`c031539409872017237517f01c23f571bfe4759e` adds read-only native target inspection;
BeepWeb `5362768bbba3e30159d3592e280eb37c9f6143a7` removes the retired connector
project reference. These are source-checkout dependencies, not claims of new
published NuGet package versions. Local Beep connection catalogs, ownership
receipts and runtime locks stay outside version control.

## Driver prerequisite inside the connection wizard - 2026-11-23

ConnectionSetupDialog now includes Target → Driver → Connection → Review → Save.
The existing SetupDrivers page is reused in place as the filtered Driver step;
there is no second installer, download endpoint or package-source configuration.
Its existing standalone route remains available. The selected database type is
converted by ConnectionConfig to the canonical Beep datasource type, so SQL Server,
PostgreSQL and SQLite match SqlServer, Postgre and SqlLite catalog entries.

The step reports connector and provider readiness separately. Missing or
unregistered drivers, unavailable catalogs and unconfigured package versions
keep Continue disabled. Creation, connection testing and saving also require
readiness. The download action uses the administrator-protected existing API and
its reviewed exact catalog package identities. Mismatched or lost installation
responses clear readiness; the user must refresh the catalog before continuing
or retrying. There is no automatic installation retry. Changing database type
invalidates readiness and target/connection reviews, and never borrows a loaded
driver for another database. Busy operations disable wizard actions.

The generated missing/ready/recovery visual target was shown before implementation:
`docs/design/module-database-creation-20261123/driver-prerequisite-mockup.png`, SHA256
`27F37828A5A5F7C3F4D76900BC2681C42D9C658B89FB988F3E526048BF6DA5F3`.
The prompt, primary comparable-product references and scope are retained in
the existing PROMPT.md. Existing theme tokens, MudBlazor controls and shared
failure reporter are reused; no database, credentials or package-feed changes
are part of this UI extension.

Initial focused evidence: `wizard-driver-step-web-build-20261123.log` records
zero errors (77 existing warnings). `wizard-driver-step-tests-20261123.log` records
10 passing rendered dialog tests, including the four prior catalog-creation/test
guards and missing-driver installation, lost/mismatched acknowledgement, catalog
failure, type changes and absent install configuration. These use the real API
client with a recording HTTP transport, not a live NuGet download.
`wizard-driver-guard-red-tests-20261123.log` records seven expected failures when
the Driver-step navigation guard was deliberately removed. The source was restored;
`wizard-driver-step-restored-build-20261123.log` has zero errors and
`wizard-driver-step-restored-tests-20261123.log` records 11 passed, zero skipped.
The additional case rejects an absent matching registration even when another
database's driver is loaded.

Live Web/API observations: SQL Server's bundled connector and
System.Data.SqlClient 4.9.1 were loaded, and Continue opened Connection details.
Postgre/Npgsql were also already loaded; inspecting catalog readiness did not
open a PostgreSQL connection. MySQL's connector was loaded but MySql.Data 9.5.0
was not. The wizard displayed the missing-provider warning and enabled Download
and load while keeping Connection/Review/Save and Continue disabled. Tab moved
focus from Refresh drivers to the download action. No package was installed and
no database connection or write was attempted during this UI inspection.
Desktop captures: `driver-step-ready.jpg` and `driver-step-missing.jpg` in the
existing design folder. Compared with the generated target: same five steps,
separate connector/provider status, compact download/refresh actions and readiness
gate; actual catalog names/versions replace the mockup's illustrative labels.

A 390px check exposed long package identifiers overflowing the dialog. The fix
uses `driver-package-details` in the existing app.css to allow wrapping, and
removes redundant dialog-stack padding. `wizard-driver-responsive-build-20261123.log`
records zero errors; `wizard-driver-final-tests-20261123.log` records 11 passed,
zero skipped on that build. The actual 390px dialog, content and package panel
had equal client/scroll widths (326/326, 316/316, 218/218 respectively), confirming
the outer-content overflow is gone. The Stepper retains its native horizontal
header scroll. Final captures: `driver-step-mobile.jpg` and
`driver-step-missing-final.jpg`. The host runs from
`web-wizard-driver-responsive-startup-20261123.log`; the browser viewport was
reset and the dialog closed without writes. Real package download/loading remains covered by the
existing installer and recording-transport checks, not claimed as a new native
provider qualification. Local development database use remains SQL LocalDB only.

## New LocalDB module database workflow - 2026-11-23

The real administrator can open module setup without a company membership. The
page requires an active tenant and saved Beep connection before binding/planning.
The driver page reports SQL Server's bundled connector/provider loaded. The
connection dialog offers SQL Server, PostgreSQL, MySQL, Oracle and SQLite; this
inspection made no connection, tenant or database writes.

The physical database gap is now implemented in the existing setup service,
controller and ConnectionSetupDialog. The four steps are Target, Connection,
Review and Save. A new LocalDB target is inspected without creating a claim or
catalog. Creation consumes an exact, administrator-bound review token once;
BeepDM's MigrationManager.BindDatabaseTarget and native SQL Server provider own
the durable receipt and catalog lifecycle. Unowned existing catalogs cannot be
adopted; uncertain creation requires reconciliation. The administration,
diagnostics and permanent IdentityServer databases are protected targets.

Observed on the actual running Web/API: the registered Administrator reviewed
the missing `OilGasModulesDev` database on `(localdb)\MSSQLLocalDB`, explicitly
created it, received ownership acknowledgement, tested the connection, and saved
it. Closing the dialog refreshed the list with that exact named connection,
no TCP port, and Inactive status (it was not made a global current connection).
Reopening the wizard confirmed durable ownership without another create.
No tenant/company, module binding or module table was created by this walkthrough.

The four Image Gen target states are retained with implemented desktop captures
and a 390x844 connection capture in the design folder below. The implementation
preserves the app's existing charcoal/purple theme, standard Mud components and
four-step hierarchy. Fields stack at phone width; MudStepper headers scroll
horizontally and the dialog body scrolls vertically. Tab moved focus from
Connection name to Server. This is focused responsive/keyboard evidence, not a
complete accessibility audit. The existing global navigation is outside this change.

Focused evidence: `localdb-target-api-tests-20261123.log` records 12 passed,
including a real retained LocalDB catalog through the production binding factory.
`localdb-target-dialog-tests-20261123.log` records four rendered Mud dialog tests:
successful creation, lost-response recovery, mismatched target refusal, and stale
connection-test rejection. The recording HTTP transport is not a live-server test.
`native-target-inspection-red-20261123.log` records four failures with inspection
deliberately broken; `native-target-inspection-restored-20261123.log` records all
five passing after restoration. `localdb-review-actor-target-red-20261123.log`
records the expected failure when actor/target binding was removed.
`localdb-wizard-and-module-restored-20261123.log` records 13 passed, zero skipped
after restoration: all review guards and real LocalDB catalog creation plus the
existing full module migration/read-write test on two separate named targets.
`localdb-dialog-ownership-red-20261123.log` records two expected failures when the
create-before-test guard was removed. The restored Web build has zero errors;
`localdb-dialog-restored-tests-20261123.log` records four passed, zero skipped.
The restored host is running from the final build (`web-localdb-target-final-startup-20261123.log`).
The browser reloaded after that restart and still listed `OilGasModulesDev` at
the exact LocalDB target (`implemented-persisted.jpg`); no repeat creation or
global-current selection was needed.

The existing native target provider is explicitly restricted to integrated-
security LocalDB. Its capability must be presented truthfully; do not generate
application SQL or claim PostgreSQL/Oracle catalog creation. The detailed design,
research, four-state Image Gen visual target and implementation/verification
sequence are retained at
[module database creation design](../design/module-database-creation-20261123/PROMPT.md).
The mockup was shown before implementation. Live catalog creation, connection
testing, persistence and ownership recovery are verified; the live tenant-bound
module planning/approval/execution journey remains a separate acceptance item.

## Current API build gate - 2026-11-23

Actual API startup and local installation are now verified. The concrete
CompanyPPDMRowAccessPolicy and its interface resolve the same scoped instance;
the two maintenance evidence owners no longer fail DI validation. Startup calls
the registered IBeepService directly for assembly loading, configuration summary
and validation, replacing assembly-wide type searches/reflection and obsolete
process-wide helpers. `TestResults/api-startup-runtime-wiring-build-20261123.log`
and `TestResults/api-diagnostics-migrations-build-20261123.log` record successful
builds; `TestResults/api-startup-installed-localdb-20261123.log` records the actual
API listening on https://localhost:7001 after installation.

The diagnostics context lacked its back-channel logout migration. The generated
AddBackchannelLogoutRevocations migrations and snapshots now cover all three
existing provider contexts. Their Up operations add only that table and expiry
index. PostgreSQL/Oracle generation was offline, with no server connection or
database creation; local runtime checks use SQL Server LocalDB exclusively.
`TestResults/diagnostics-migrations-localdb-current-20261123.log` records all 16
diagnostics tests passed, zero skipped, including three-provider model/snapshot
checks and a retained fresh LocalDB failure-store installation/read-back.

The protected bootstrap endpoint confirmed BeepOilGasRepository did not exist
(`TestResults/default-repository-bootstrap-plan-20261123.json`, CreatesDatabase
true). Its 16 pending EF migrations were applied through the existing setup
service and exact reviewed target token; result HTTP 200/BootstrapRequired is in
`TestResults/default-repository-bootstrap-result-20261123.json`. No account was
seeded. The temporary bootstrap key was disabled by restarting the API with its
normal Development settings and its task-generated key file was removed.
`TestResults/installed-localdb-readiness-20261123.json` records BootstrapRequired,
401 for an anonymous account read and 401 for bootstrap access without a key.
The intended user subsequently completed real IdentityServer registration and the
OilGas callback. The browser rendered Repository ready and admitted the account
to the Administrator-only role screen. This establishes the first-account rule
in the live local installation; the role data read initially exposed the separate
asset middleware defect described below.

Website startup remains incomplete. The shared Beep.Razor.Components project
referenced a missing, unused retired connector checkout; that reference was
removed at its owner and restore succeeded in
`TestResults/web-current-dependencies-restore-20261123.log`. The Web global-using
order, current model import and typed MudTextField callback were corrected.
`TestResults/web-startup-contracts-build-20261123.log` now has no ordinary C#
errors, but initially failed with 437 analyzer errors: 422 BEEP0003, ten BEEP0001
and five BEEP0004. The enum and public-error paths are corrected. Razor catch
paths report through the existing diagnostics service; state owners now receive
the existing OilGasCallFailures owner directly instead of an opaque formatting
delegate. `TestResults/web-state-reporting-owners-build-20261123.log` reduced the
remaining failures to 28 BEEP0003 errors, with no ordinary C# errors. The final
cancellation and evidence-review handlers are being corrected; their current
build completed successfully: `TestResults/web-startup-final-owners-build-20261123.log`
and `TestResults/web-navigation-lifecycle-build-20261123.log` record zero errors.
No analyzer was suppressed. The real setup page initially failed because the
company authentication provider subscribed to navigation before endpoint
rendering initialized it. Subscription now starts when Blazor requests the
authentication state, and disposal unsubscribes only when subscribed. The browser
then rendered First-time setup with "Repository installed" and the first-person
administrator instruction. This does not prove authenticated registration.

The real sign-in redirect exposed a stale Development client ID. LocalDevWireClients
was rebuilt because its existing binary referenced the absent local-dev worktree.
It found the existing OilGas registration, whose plaintext secret was absent from
the local output registry. The helper now requires an explicit application filter
for its opt-in `--rotate-missing-secrets` action and calls the existing audited
IDeveloperService credential-rotation owner for confidential clients only. The
OilGas client ID was retained. The current credentials were synchronized to local
Development settings and User Secrets; a stale User Secrets override had otherwise
continued selecting the missing registration. Credential values are excluded from
this evidence. Logs: `TestResults/oilgas-identity-secret-recovery-helper-build-20261123.log`,
`TestResults/oilgas-local-client-credential-recovery-20261123.log` and
`TestResults/web-current-usersecrets-startup-20261123.log`.

The browser reached the branded OilGas Web (local) IdentityServer sign-in page,
then the intended user completed registration and the callback. No secondary
IdentityServer database, synthetic OilGas administrator, or authentication bypass
was used. The first registered OilGas account is automatically Administrator;
later registrations receive no automatic role. After the asset middleware fix,
the real role screen loaded the intended registered account and showed
"Current User Roles" → "Administrator" without any manual assignment or company
selection. /health/repository returned HTTP 200/Ready. Current hosts are recorded
in `TestResults/api-central-admin-installed-startup-20261123.log` and
`TestResults/web-central-admin-installed-startup-20261123.log`.

The live role page exposed an HTTP 403 from /api/identity/users: AssetAccessMiddleware
still asked central administration endpoints for company/module assets. It now
uses CompanyRequestPolicy's existing control-plane boundary, preserving asset
checks for business routes and identity row security. Global persona catalog and
role workspace configuration are classified centrally; company persona profiles
and current company workspaces still require company selection. No endpoint role
or permission guard was relaxed.

`TestResults/repository-admin-no-company-localdb-20261123.log` records 17 selected
checks passed, zero skipped. Its new retained LocalDB HTTP case uses actual
Identity storage, API identity provisioning, controllers and asset middleware:
the first subject receives Administrator, central user/role/persona reads work
without module bindings, the second subject receives no role and cannot list
users, and exactly one Identity membership and bootstrap marker remain. The
strict asset service receives no calls. Restoring the previous asset middleware
behavior failed eight of those checks, including the new HTTP case
(`TestResults/admin-asset-boundary-red-20261123.log`). The corrected source was
restored and rebuilt; all 17 checks passed again, zero skipped
(`TestResults/admin-asset-boundary-restored-localdb-20261123.log`). All 29 selected
client company/role boundary checks passed, zero skipped
(`TestResults/company-persona-control-plane-tests-20261123.log`). The test host
does not certify the complete production service graph; the real role-screen read
provides separate evidence for the running development host.

`TestResults/company-ui-reporting-owners-focused-20261123.log` records 376 selected
company/field/asset-grant/hierarchy/lease state tests passed, zero skipped. The new
deferred-navigation guard was shown to fail when the old constructor subscription
was restored (`TestResults/navigation-lifecycle-guard-red-20261123.log`), then the
corrected source was restored and all 23 CompanyClientScopeTests passed
(`TestResults/navigation-lifecycle-guard-restored-20261123.log`). The mutation did
not change the running website binary. Web.Auth.Tests now compiles against actual
API/repository/failure owners and builds its Razor fixtures. Its selected startup,
draft and persona checks initially passed 208/221; the 13 failures were missing
failure-service registrations in the persona render fixtures. After correcting
those fixtures, all 221 selected tests passed, zero skipped
(`TestResults/web-startup-rbac-reporting-focused-pass-20261123.log`). The API test
project also compiles against the current workspace/failure owners, and all 79
selected invoice financial review/HTTP/service evidence tests passed, zero skipped
(`TestResults/api-invoice-state-owner-focused-20261123.log`). These checks do not
certify all screens or remove the outstanding MudBlazor/dependency warnings.

Current production build evidence: `TestResults/api-startup-admission-reporting-20261123.log`
exited 0 with zero errors and 52 warnings. The remaining handled-exception paths
now use IFailureReporter, including connection selection, repository resolution,
company environment status, process admission, background work and SignalR
delivery. Publishers and queues take the reporter directly instead of delegates;
the lifetime manager owns failure reporting for both notification variants.
Denials, recovery states and fixed public messages are preserved. This proves
compilation, not live installation acceptance.

`TestResults/progress-module-target-reporting-fixed-20261123.log` records 60
focused progress and target identity tests passed, zero skipped. Missing file
components use the documented FileSystemInfo.Attributes sentinel instead of
exception-driven detection; permission/I/O errors still propagate. Reference:
[Microsoft FileSystemInfo.Attributes](https://learn.microsoft.com/en-us/dotnet/api/system.io.filesysteminfo.attributes?view=net-10.0).
Channel parsing checks decoded components before generating the canonical name.
The company environment suite also passed 66 tests, zero skipped in
`TestResults/company-environment-runtime-reporting-20261123.log` after its runtime
and GHG status reporter dependencies changed.

The normal API test project now compiles against the existing owners:
source links include the current HTTP collection and invoice/purchase/inventory
workspace owners, with missing domain imports corrected. The retired
FdpDraftStorageTests targeted removed SubmitFdpDraftRequest/SubmitFdpDraftAsync
APIs and were removed; the active native draft contracts and storage tests remain
in DevelopmentPlanDraftContractTests and DevelopmentPlanNativeDraftTests. Their
existence is not a claim that either suite has run in this acceptance pass.
`TestResults/api-tests-installation-build-final-20261123.log` records build exit
0, zero errors and 78 warnings. Current shared native/HTTP fixtures are included;
their role bridge remains in the existing CurrentFieldLifecycleGateTests owner.

Current live LocalDB evidence: `TestResults/localdb-module-seeding-20261123.log`
records both LocalDbDriverTests passed, zero skipped, exit 0. The fixture now uses
the real ConfigEditor and DMEEditor owners for captured migration storage,
execution ownership and scoped read acknowledgements. Its only editor overrides
select the two explicit native test sources. No storage or read capability gate
was disabled. Reviewed hashes are supplied at approval; a stale plan after
rebinding is asserted to throw the current Conflict refusal before any target DDL.

The passing run covers HTTP plan binding/serialization, refusal before approval,
wrong-hash refusal, OilProperties table creation in two separately selected
LocalDB catalogs, independent UnitOfWork writes/read-back, persisted module
bindings, standard ASP.NET tables in the separate EF repository, lifecycle
migration and persisted workflow graph, and HeatMap configuration/date round-trip.
Lifecycle reference seeding through ModuleRepositoryController inserts records;
the second seed inserts zero duplicates, and its table remains absent from the
unselected database. HeatMap has no default configurations to seed; the passing
HeatMap check is application persistence, not a seeded-template claim.

The isolated catalogs and ownership/history receipts are retained. The latest
receipt root observed after the passing run is
`C:\Users\f_ald\AppData\Local\Temp\OilGasModuleMigration\b93672d0d58440069a8db1589e82da6c`.
No existing database was dropped or recreated. This proves the specified native
integration path, not an authenticated website journey or live Oracle/PostgreSQL
qualification. Actual website/API startup and sign-in acceptance remain open.

`TestResults/api-installation-boundary-tests-20261123.log` records 82 passed and
two failed, zero skipped: progress/queue, process checks, task admission, bootstrap
and accounting scope passed; two HSE KPI cases lacked primary-key metadata in
their test fixture. That fixture now supplies the current selection/row-policy
dependencies, explicit primary keys and active native rows. After fixing the
fixture regressions, `TestResults/api-installation-boundary-tests-current-pass-20261123.log`
records all 84 tests passed, zero skipped, exit 0. The latest test build also
passed with zero errors in `TestResults/api-tests-installation-fixtures-build-20261123.log`.

Earlier live logs (`localdb-module-installation-current`,
`localdb-module-target-identity`, `installation-localdb-boundaries-current`,
`localdb-module-real-config`, `localdb-module-real-editor`, and
`localdb-module-stale-binding`, all dated 20261123) remain diagnostic history.
Their stale fixture failures are superseded by the passing seeding run above.
The latest source build passed with zero errors in
`TestResults/api-tests-localdb-seed-build-20261123.log`.

The company environment suite passed 66 tests, zero skipped, exit 0 in
`TestResults/company-environment-current-boundaries-20261123.log`, including the
effective-controller reporter dependency change. No PostgreSQL, Oracle or physical
SQLite qualification was run. Local live module verification must use SQL LocalDB.

SystemSetupRepositoryController, CompanyGLAccountController,
CompanySalesInvoiceController and CompanySalesReceivableController now report
handled failures through IFailureReporter. Setup status still distinguishes
MigrationRequired/Unavailable, and accounting public messages and authority
checks are preserved. Direct setup controller test callers supply the recording
reporter. `TestResults/api-startup-system-ledger-reporting-20261123.log` records
API build exit 1 on remaining errors elsewhere, with no ordinary C# or diagnostics
in these four controllers. The remaining errors are BEEP0003/BEEP0004; this is
compile/analyzer evidence only, not runtime installation acceptance.

Production intervention, incident workflow, maintenance-return policy and inbox
catch paths now use their owning controller's failure reporter. Production and
return-policy responses no longer expose exception messages; intent IDs and
recoveryRequired fields remain intact. Inbox access refusals retain their
omit/unavailable behavior and now report the reason. Incident test callers supply
the existing recording reporter. `TestResults/api-startup-workflow-reporting-20261123.log`
records build exit 1 on remaining analyzer errors elsewhere, with no ordinary C#
or diagnostics in those four controllers. The focused incident workflow suite
passed 56 tests, zero skipped, exit 0 in
`TestResults/incident-workflow-reporting-current-20261123.log`. This does not prove
all intervention/policy/inbox runtime paths or full startup/module installation.

Repository bootstrap and system setup record controllers now inject the shared
failure reporter. Caught setup-key/access denials, validation refusals and persona
transition requirements are recorded without changing the existing HTTP replies.
The API build in `TestResults/api-startup-repository-setup-reporting-20261123.log`
exits 1 on remaining analyzer failures elsewhere, with no ordinary C# errors or
diagnostics in SetupBootstrapRepositoryController/SystemSetupRecordController.
The first-run HTTP fixture already registers the required reporter. Its runtime
tests remain unexecuted through the failing full API target.

CompanyPersonnelIdentityController and CompanyPersonnelPoliciesController now
report caught refusals and return safe validation/conflict text with diagnostic
references instead of exception messages. FieldRevenueSourceController reports
its caught failures while preserving existing public messages. Administrator and
company guards, status codes and recovery contracts are unchanged. The API build
in `TestResults/api-startup-personnel-revenue-reporting-20261123.log` exits 1 on
remaining analyzer failures elsewhere, with no errors in these three controllers
or ordinary C# errors. The corrected focused HTTP case
ActualCompanyPersonnelIdentityHttpRequiresCurrentCompanyAdministratorAndPreservesNativeBindingRecovery
passed (one test, zero skipped, exit 0), recorded in
`TestResults/personnel-identity-reporting-http-20261123.log`. This is scoped
personnel authorization/recovery evidence, not full host or installation acceptance.

APController (including its correction partial) and PurchaseOrderController now
receive IFailureReporter and report their caught route/argument failures,
authorization refusals and unconfirmed evidence/transaction outcomes. Existing
fixed public messages, status codes and company authority checks remain intact.
`TestResults/api-startup-ap-purchase-reporting-20261123.log` records build exit 1
on BEEP0003/BEEP0004 errors elsewhere, with no ordinary C# errors and no diagnostics
in those controllers. Their 79 prior reporting errors are cleared; this does not
prove endpoint runtime acceptance or full host startup.

InventoryController now receives IFailureReporter; its company-source partial
reports route validation, authorization refusal, provider refusal, overflow and
uncertain transaction/evidence outcomes. Existing response codes and safe public
messages remain unchanged. `TestResults/api-startup-inventory-reporting-20261123.log`
records API build exit 1 on remaining BEEP0003/BEEP0004 failures in other owners,
with no ordinary C# errors or InventoryController diagnostics. This clears the
67 previously reported inventory catch diagnostics without disabling rules.
Inventory endpoint runtime behavior and full startup are not verified by it.

The latest API attempt, `TestResults/api-startup-accounting-reporting-20261123.log`,
exited 1 on remaining BEEP0003/BEEP0004 failures elsewhere. It reports no ordinary
C# errors and no errors in CompanyMaintenanceHandoffController or
CompanyAccountingProcessController. The latter now injects IFailureReporter;
route/argument/authorization catches report handled refusals and evidence failures
report before returning their existing fixed conflict responses. Routes, HTTP
codes, public messages and process/source checks remain unchanged. This build
does not establish whole-host startup or accounting endpoint runtime acceptance.

CompanyMaintenanceHandoffController now injects IFailureReporter. Its repeated
authorization, argument and invalid-operation catches report through that owner;
400/409 responses carry fixed public text and a diagnostic reference instead of
the exception message. HTTP status codes and no-store headers are preserved.
The focused source target compiles. An initial filename-based test filter matched
no tests (not a pass); the corrected method filter executed
ActualMaintenanceReviewRejectsMissingOriginalReferencesBeforeProductionReads:
one passed, zero skipped, exit 0 in
`TestResults/maintenance-reporting-http-20261123.log`. This checks the 400/no-write
boundary before production reads, not every maintenance operation or the full
API build. The test host registers the existing recording failure reporter.

CompanyEnvironmentController now receives the shared IFailureReporter directly.
Every caught validation, authorization and evidence failure reports through that
owner while retaining the existing 400/403/409 responses and safe public messages.
The existing controller callers use the recording test reporter. A regression
case verifies invalid preview JSON is recorded and returns the fixed validation
message. `TestResults/company-environment-reporting-20261123.log` records 66 tests
passed, zero skipped, exit 0. This covers environment/controller behavior, not
the remaining API-wide analyzer failures or live module installation.

`TestResults/api-startup-model-current-20261123.log` records an API build exiting
1. Its remaining errors are BEEP0003 (exception reporting) and BEEP0004 (exception
text escaping catches); no ordinary C# or BEEP0001 errors remain in that attempt.
These analyzers were not disabled. DrillingEngineerAggregationService now uses
the owned daily report's PERIOD_END_UTC for monthly NPT. AFE budget remains sourced
from programmes; spend is null because the current observed-period report has
no cost ledger. Removed references to REPORT_DATE, DAILY_COST and CUMULATIVE_COST
were corrected at the consumer, and the existing KPI test fixture/expectation
was updated to this data contract. That test has not executed through the still
failing API test target. Full startup and live module acceptance remain unverified.

## Company progress startup compilation - 2026-11-23

SetupDriverService's bundled connector selection now uses an explicit SqlServer /
SqlLite lookup. Other providers retain the existing catalog/package route; no
provider was added or removed. The focused source target compiled successfully
(exit 0), recorded in `TestResults/setup-driver-catalog-build-20261123.log`.
No provider runtime test was run for this edit. The initially selected physical
SQLite qualification test was canceled before execution to preserve LocalDB-only
live testing. Full API compilation remains subject to the failures below.

CompanyHubGroups, WorkflowNotificationLifetimeManager and CompanyProgressState
now name all CompanyChannelKind members in their switches. Invalid enum values
retain explicit rejection/denial; future named members require an implementation
decision instead of inheriting a discard branch. The existing focused target
passed 51 CompanyProgressTests/CompanyHubScopeTests with zero skips, exit 0:
`TestResults/company-progress-startup-20261123.log`. This is scoped progress and
notification authorization evidence. Broader API exception-reporting failures,
SetupDriverService's provider switch and stale drilling KPI field references
still prevent claiming full host compilation or startup.

## Current default repository verification - 2026-11-23

The subsequent full normal repository target passed all 72 tests, exit 0, with
OILGAS_TEST_LOCALDB=1 and FocusedCompanyTests=false. Command: `dotnet test
Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj --no-restore
-m:1 -p:UseSharedCompilation=false -p:FocusedCompanyTests=false --logger
"console;verbosity=normal"`. Output:
`TestResults/repository-rbac-persona-current-20261123.log`.
Coverage includes independent company persona profiles/preferences, workspace
selection constrained by company roles, inactive personas, revocation rollback,
audit creation and optimistic concurrency. Persona changes do not grant roles.
Registration tests verify unavailable readiness and refused database access for
missing/unsupported configuration. The two isolated live LocalDB installation
cases executed again. These are repository/service checks; they do not prove
browser administration or live BeepDM module table creation.

`TestResults/repository-installation-current-20261123.log` records exit 0 and
36 passing tests using FocusedCompanyTests=false, OILGAS_TEST_LOCALDB=1 and the
filter RepositoryMigrationTests, RepositoryBootstrapTests, LocalDbInstallationTests.
Both live LocalDB cases executed: fresh EF installation with repeat application,
and competing registrations leaving exactly one Administrator. Bootstrap cases
also verified ordinary later users, replay, inactive-user denial, access revocation
and rollback on completion failure. Isolated LocalDB databases are retained by
the existing test owner; the permanent IdentityServer database was not involved.
Provider cases verified aligned migration names, model snapshots, normal and
idempotent SQL generation for SQL Server, PostgreSQL and Oracle, including
Identity, extension, tenant/company and module-connection tables. Oracle and
PostgreSQL verification remains offline. This result does not cover the focused
tenant module tests, whose subsequent result is recorded below.

## Current module boundary check - 2026-11-23

`CompanyEnvironmentBindingReader` now asks ModuleConnectionResolver.FindAsync
for an optional binding and then verifies its fingerprint. Missing bindings
return null through the resolver's existing optional-read contract; fingerprint
verification errors propagate instead of being swallowed as an unavailable source.
`TestResults/module-environment-binding-20261123.log` records exit 0, 55 passing
tests and zero skips for RepositoryTenantBindingTests and ModuleDatabaseTargetTests
with FocusedCompanyTests=true. The added case verifies a missing source and an
owned binding's current fingerprint without opening a module datasource.

The existing live `LocalDbDriverTests.BeepMigrationAndWritesStayOnTheSelectedNamedDatabase`
fixture now provisions its two isolated catalogs through
MigrationManager.BindDatabaseTarget and SQLServerDatabaseTargetMigrationProvider,
with retained ownership receipts, replacing its hand-written CREATE DATABASE.
The main module's before-approval, selected-target and unselected-target table
checks now use Beep datasource metadata. Composition insert/read checks on both
targets use the existing UnitofWork owner, retaining the single-row and original
database isolation assertions. Lifecycle inserts and row-count checks and the
heat-map persisted name/timestamp check also use UnitofWork; unselected-table
checks use Beep metadata. The EF repository connection's structured GetSchemaAsync
checks its actual tables, and the driver probe reads its native Database property.
Source inspection confirms LocalDbDriverTests contains no CommandText,
ExecuteScalar, ExecuteReader or CreateCommand. That establishes removal of the
ad-hoc command path, not behavioral equivalence. These edits have not yet compiled
or run against LocalDB. A build attempt exited 1 before test compilation;
`TestResults/module-live-test-build-20261123.log` records missing REPORT_DATE,
CUMULATIVE_COST and DAILY_COST members in DrillingEngineerAggregationService,
plus error-reporting analyzer failures in PurchaseOrderController. No live
catalog or module table was created by this build attempt.

The current bindings are tenant-scoped and verify physical database ownership.
`ModuleBindingService.NormalizeModule` now rejects both SECURITY and SYSTEM_SETUP;
connection lookup and migration fingerprint generation use that same validation.
Previously the controller excluded both but the service/resolver excluded only
SECURITY. Four regression cases cover upper/lowercase IDs, rejected binding,
lookup and planning, and absence of binding/ownership writes.

Verification now passes: `TestResults/module-repository-boundary-20261123-final.log`
records 54 passed, zero skipped, exit 0, for RepositoryTenantBindingTests and
ModuleDatabaseTargetTests with FocusedCompanyTests=true. These EF in-memory
fixtures verify binding and physical ownership rules, not live table creation.
The test target's Roslyn Workspaces references now match EF Design at 5.9.0.
Its source-linked HSE KPI service also includes its existing internal rules
source, without expanding production visibility. Earlier failed build/restore
attempts remain in the dated logs for diagnosis.
The dependency has now been migrated to its existing Wellbore.Data and
Wellbore.Loaders project owners. Deeper inspection corrected the initial search:
JsonCountryCatalog, MappingProfile and DataSourceDefinition remain present in
those projects; OilGasEnvironment and OilGasServices were renamed to
WellboreEnvironment and WellboreServices. API consumers and environment tests
now use those names. Focused repository tests use project references instead of
stale prebuilt DLL HintPaths. The removed StyleColors helper was replaced with
SkiaSharp's color parser for the existing WellPath color check.
`TestResults/environment-owner-migration-20261123.log` records 65 environment
tests passed, zero skipped, exit 0. This verifies the environment test scope,
not full API startup or live module schema creation.
The earlier verification results below are historical, not current acceptance.

This audit follows the requested installation architecture, not every feature in
the oil-and-gas solution. It is not a declaration of production readiness.

## Required Architecture and Evidence

| Requirement | Current implementation and evidence | Remaining verification |
| --- | --- | --- |
| Default repository separate from module databases | `RepositoryServiceRegistration` selects one explicit repository provider/connection. `ModuleDatabaseBindings` stores module-to-BeepDM connection selection. Real LocalDB administration repository installation and separate module database creation are observed above. | Live tenant-bound module table walkthrough remains separate from the passing two-target integration test. |
| Standard ASP.NET tables | `RepositoryDbContext` uses Identity stores. Provider-specific migrations create the seven standard `AspNet*` tables plus repository metadata and `APP_*` extensions. Current provider tests check aligned migration names, table presence and snapshot/model consistency. | SQL generation is not a live Oracle/PostgreSQL execution test. |
| Explicit basic installation | `Install-Repository.ps1` defaults to idempotent SQL preview; Apply executes pending EF migrations. It does not seed users or execute module migrations. The actual default repository was installed through protected EF setup and now reports Ready. | Live Oracle/PostgreSQL installation is unverified; it is not part of LocalDB development verification. |
| SQL Server LocalDB development | API development settings select `(localdb)\MSSQLLocalDB`, database `BeepOilGasRepository`, integrated authentication. Installer rejects non-LocalDB targets in LocalDevelopment mode. | No PostgreSQL or Oracle credentials are needed for local work. |
| First registered OilGas user is Administrator | `RepositoryBootstrapService` transaction creates the user, external login, canonical role/membership, extension audit, and singleton bootstrap marker. The intended first user registered in the browser and the local role screen displayed Administrator. LocalDB HTTP tests prove a second account receives no automatic role. | Existing users without a marker require recovery, not promotion of the next arrival. |
| Canonical roles and extension tables | Local ASP.NET roles and memberships are authoritative; extension metadata does not independently grant roles. API and Web transformations discard external authorization claims and resolve local roles. Role/user API tests enforce local Administrator checks. | Browser role assignment, revocation, and inactive-user denial remain to be exercised together. |
| Authentication-only IdentityServer | OilGas validates external identity and registers its own local user. The actual Web callback and authenticated API administration calls succeeded. Introspection tests verify that external Administrator claims do not grant local roles. | Other authentication modes and deployed clients require their own runtime qualification. |
| User-selected module databases and BeepDM migrations | `ModuleConnectionResolver` requires named bindings and refuses SECURITY. Migration binding/target tests cover missing or changed bindings and scoped plans. Module setup uses the BeepDM migration workflow independently of repository EF installation. | Complete the UI walkthrough for selection, review, approval, execution, and subsequent module read. Existing tests are not proof that every domain feature is complete. |

## Current Verification

- `TestResults/installation-audit-repository.log`: 29 passed, zero skipped.
  `OILGAS_TEST_LOCALDB=1`; fresh installation and competing bootstrap use LocalDB.
  Provider SQL and idempotent scripts are generated offline for all three providers.
- `TestResults/installation-audit-web.log`: 81 passed, zero skipped.
  Tests cover the client registration and role bridge, not browser rendering.
- `TestResults/hierarchy-path-access-full.log`: latest full API run, 872 passed,
  zero skipped, with all three LocalDB integration flags enabled.
- No live Oracle/PostgreSQL test was performed. No existing database was reset.

Subsequent module client verification: `TestResults/module-response-web.log`
records 91 passing Web tests and zero skips. Successful approval must identify
the requested plan; successful execution must match its reviewed plan ID, plan
hash, and manifest hash. Mismatched/empty identities are not shown as success,
and an uncertain execution is not automatically retried. Failure responses retain
their diagnostic message. This is protocol-level evidence, not a browser walkthrough.

## Final Installation Walkthrough

1. Reuse the permanent `(localdb)\MSSQLLocalDB` / `identityserver` instance at
   https://localhost:7062. Do not create a second identity database, reset it, or
   follow the superseded isolated-identity procedure.
2. Reconcile the OilGas Web code-flow/PKCE registration through IdentityServer's
   supported owner and the project's LocalDevWireClients helper. Keep local
   credentials out of source control and evidence output. Verify real access-token
   acceptance; discovery availability alone does not prove client trust.
3. The default BeepOilGasRepository is installed through its protected EF setup
   owner and reports BootstrapRequired. Keep its first Administrator for the real
   intended user; isolated integration catalogs are retained separately.
4. Register the first subject through Web, verify standard Administrator role and
   repository readiness, then register a second subject with no automatic role.
5. Exercise user administration: grant/revoke a role and disable the second user.
   Verify both UI role gates and protected API responses change accordingly.
6. Select a distinct named BeepDM module database; review and approve a migration
   plan, execute it, and verify the module reads that target. Confirm repository
   Identity tables remain in the default repository and no global fallback occurs.
7. Stop only the test processes; retain isolated databases and logs. Record actual
   browser/API evidence before declaring installation acceptance complete.

## Known Separate Defects

Hierarchy configuration still uses an invalid untyped repository. Its persistence,
audit actor, traversal, and recursive filtering need correction. These are genuine
feature defects, not evidence that repository EF installation itself failed.

Distinct external subjects that collide under a database's unique-key collation
are denied rather than merged. This prevents account alias escalation but leaves
registration availability for such subjects unresolved. It must remain visible
when assessing supported external identity providers.
