# OilGas Default Repository

Installation requirements, verified evidence, and remaining acceptance steps:
[Installation Acceptance](../docs/identity-and-web/INSTALLATION-ACCEPTANCE.md).

## Remaining Identity Consolidation (2026-09-12)

External-account lookup now verifies the stored issuer-provider key and subject
using ordinal comparisons after the database lookup. Bootstrap likewise checks
the raw login keys before accepting an existing registration. This closes a
reproduced LocalDB identity-alias issue: FIRST resolved the Administrator account
registered as first under the default case-insensitive SQL Server collation.
Trailing-space aliases are rejected too, including SQL's padded-string comparison.
The failing reproduction is recorded in TestResults/external-subject-before-fix.log.
After the fix, all 28 repository tests and 836 API tests passed with LocalDB enabled
and zero skipped (external-subject-repository.log and external-subject-api.log).
The HTTP fixture confirms a case alias receives no local account, cannot access
user administration, and cannot bind itself through registration. No existing
login keys, passwords, schemas, or database collations were changed. Supporting
two genuinely distinct subjects that collide under a provider's current unique-key
collation remains unresolved: such a second registration is denied, never mapped
to the existing account. Live OIDC registration remains unverified.

AssetHierarchyController now requires a local authenticated principal. Unfiltered
organization/children/path reads and configuration administration require the
standard Administrator role; user-filtered reads and path validation require the
owner or Administrator. Eight controller cases verify denial before service calls
and permitted owner/admin calls. The LocalDB HTTP fixture verifies that a member's
forged external Administrator claim cannot authorize unfiltered hierarchy reads,
configuration writes, or another user's hierarchy and access validation.

The API registers BoundAssetHierarchyService to resolve a reviewed PPDM_CORE scope
per operation, construct the hierarchy reader for that explicit connection, and
reject results when the scope changes before completion. The factory validates
cached datasource identity against the configured target. The underlying hierarchy
service no longer has a default connection argument. Nine boundary tests cover all
seven operations with missing bindings, per-operation selection, and a mid-read
binding change. All 836 API tests passed with LocalDB enabled and zero skipped
(TestResults/hierarchy-api-boundary.log). These checks do not prove the hierarchy
feature complete: ORGANIZATION_HIERARCHY_CONFIG still uses an invalid untyped
repository, so configuration persistence needs replacement; tree construction and
user filtering still need correction and live PPDM data verification. Configuration
writes also need a canonical local audit actor instead of the organization ID.
Live OIDC registration remains unverified.

Hierarchy path validation now rejects empty/null paths, blank users, null nodes,
and blank asset identifiers/types before any access lookup. Every supplied node
must receive an explicit grant; denied/null results or lookup failures deny the
whole check, while cancellation propagates. Eleven direct service cases cover
these outcomes. `TestResults/hierarchy-path-access-full.log` records 872 passing
API tests, zero failures and zero skips, with all three LocalDB integration flags
enabled. This validates authorization of the supplied nodes, not the accuracy of
their parent-child relationships or the incomplete hierarchy construction above.

The sixth EF migration, AssetAccessExtensions, adds APP_USER_ASSET_ACCESS as an
AspNetUsers extension in the default repository for all three providers. Grants
carry a database-scope digest, exact asset identifiers, optional organization
scope, access level, inheritance, active state, actor/timestamp metadata, and an
optimistic-concurrency stamp. RepositoryAssetAccessStore implements grant/read/
soft-revoke with active Identity Administrator checks; role claims alone cannot
authorize writes after membership revocation. Scope digests must be canonical
SHA-256 hex supplied by server-side binding resolution, never by client input.
The same asset ID in another database does not share a grant. Organization-scoped
rows are not returned as global grants. SQL Server collation cannot widen exact
organization reads or asset revocations. Revocation retains the row; regrant
preserves creation metadata, but this is not a complete append-only audit history.

The API now registers this store through IUserAssetAccessStore and injects it
into UserAssetAccessService. Asset operations require the reviewed PPDM_CORE
binding; there is no global-connection fallback or USER_ASSET_ACCESS table access.
ModuleConnectionResolver supplies a digest of the binding version and physical
target configuration. Rebinding or editing the target invalidates prior grants;
an administrator must grant access for the new scope. Role/permission operations
remain independent of module bindings. Hierarchy reads use the resolved connection,
validate its cached datasource against the configured target, enforce exact parent
identifiers, and reject a binding change before returning results.

The retained LocalDB integration test now exercises canonical grant/read/revoke
through UserAssetAccessService, proving persistence, scope isolation, regrant,
inactive-user rejection, and revoked-admin denial. Additional tests verify missing
bindings, binding changes, stale cached sources, and physical-target fingerprints.
All 819 API tests passed with LocalDB enabled and zero skipped
(TestResults/asset-store-integration-api.log). The preceding schema verification
passed all 28 repository tests (TestResults/asset-extension-repository.log).
Provider model snapshots and idempotent SQL generation passed offline; Oracle and
PostgreSQL were not contacted. The additive migration was applied to development
LocalDB: six applied migrations, zero users, zero asset grants. No module data or
existing databases were deleted, and no bootstrap user was seeded.

Asset checks now match the requested asset ID/type against direct and expanded
access, and require any requested canonical application permission even for
WRITE/DELETE grants. Repository failures deny access. Reads use typed entities;
field inheritance uses WELL.ASSIGNED_FIELD/UWI, POOL.FIELD_ID, and
FACILITY.PRIMARY_FIELD_ID. Type filtering happens after inheritance expansion.
The invalid WELL.POOL_ID inheritance query was removed; pool-to-well inheritance
remains unavailable pending a validated relationship. Seven regression cases
verify child matching and permission enforcement, not live PPDM hierarchy data.
All 813 API tests passed with LocalDB integration enabled and zero skipped
(`TestResults/asset-inheritance-full-localdb.log`). The subsequent integration
described above replaces the old untyped grant/revoke persistence and global
asset connection. Live OIDC registration remains unverified. Local development
and live local database tests use SQL Server LocalDB only.

AccessControlController now requires an authenticated local user, restricts
user-specific reads to the owner or Administrator, and requires Administrator
for asset grants/revocations and role-permission administration. Seven direct
controller cases verify denial before service access and allowed owner/admin paths.
The LocalDB HTTP registration fixture also verifies that an ordinary member's
forged external Administrator claim cannot authorize the four mutation endpoints.
All 805 API tests passed (`TestResults/asset-access-api-guards.log`).
The guarded GetRolePermissions/AssignPermissionToRole/RemovePermissionFromRole
methods now delegate to RepositoryApplicationRolePermissionStore and the existing
RepositoryRoleAssignmentService. They no longer read/write ROLE_PERMISSION in a
domain database. Grants use AspNetRoleClaims with APP_ROLE_PERMISSION metadata;
revocation preserves history, resolves either a permission ID or catalog key, and
rejects ambiguous duplicate grants rather than performing a partial revocation.
Mutations require an authenticated local Administrator actor; organization-scoped
application-role operations remain unsupported instead of widening to global grants.
The retained LocalDB test verifies grant, permission lookup, key-based revocation,
audit history, duplicate rejection, and non-admin denial through the asset service.
Its strict Beep editor confirms no module datasource access. All 806 API tests
passed (`TestResults/asset-role-permission-store.log`). Asset-specific access
records now use the database-scoped repository extension described above.

The registered LifeCycle `UserAssetAccessService` now delegates application-role
and permission queries to `RepositoryApplicationAuthorizationReader`. It reads
active AspNetUsers joined to AspNetUserRoles/AspNetRoles and permission-valued
AspNetRoleClaims. The USER_ROLE, BA_AUTHORITY, and ROLE_PERMISSION fallbacks were
removed from this service. Permission values are compared ordinally after reading;
inactive or missing users receive no roles/permissions, and repository failures
propagate rather than trying a domain database. Organization-scoped application
role requests are explicitly rejected: global grants are not substituted for
organization/asset access checks.

Three tests cover actual LocalDB role/permission lookup, deactivation, membership
revocation, organization-scope rejection, and no legacy fallback on failure.
All 797 API tests passed (`TestResults/asset-identity-authorization-regression.log`).
The test uses a strict module editor to prove role checks never access its datasource.

Profile persistence is now consolidated through `RepositoryUserProfileService`.
The legacy LifeCycle service was removed. APP_USER stores nullable preferences JSON,
preferred layout, last-login UTC, and a primary-role reference to AspNetRoles; the
primary role must already be assigned and is not returned after membership revocation.
Reads do not create metadata, missing/inactive users do not receive active profiles,
and mutations record the local actor. Invalid JSON or unassigned primary-role
selections return controlled validation errors from the profile controller.

The fifth EF migration, UserProfileMetadata, was generated for all three providers.
Provider SQL/model checks and 28 repository tests passed; PostgreSQL/Oracle generation
was offline. All 798 API tests (including a retained LocalDB profile round-trip) and
64 Web tests passed. Evidence: `TestResults/profile-repository-migrations.log`,
`TestResults/profile-repository-api-regression.log`, and
`TestResults/profile-repository-web-regression.log`.
The additive migration was applied through Install-Repository.ps1 to the configured
development LocalDB repository. An independent SQL check found five applied
migrations, all four new APP_USER columns, and zero users. Earlier four-migration
startup observations below describe the state before this addition.

Asset-specific access records still require separate ownership/routing review,
and successful browser/OIDC registration remains unverified.
Old UserManagement role-assignment implementations also remain in source, though
the API registers `RepositoryRoleAssignmentService` for its administration routes.

`UserProfileController` previously accepted arbitrary user IDs from any registered
caller. Every endpoint now requires the authenticated local user to own the target
profile or have Administrator; primary-role changes additionally require Administrator.
Six direct-controller tests cover denied access without service calls, denied owner
primary-role changes, and permitted owner/admin preference updates. All 794 API
tests passed with LocalDB integration (`TestResults/profile-authorization-regression.log`).
These guards do not prove the profile endpoints through an authenticated browser.

## HeatMap Database Ownership (2026-09-12)

Schema verification now rejects native SQL Server/PostgreSQL `date` columns when
the declared CLR field is DateTime; Oracle DATE is not rejected by this check
because it includes time-of-day. The sibling BeepDataSources file
`DataSourcesPlugins/RDBMSDataSource/PartialClasses/RDBSource/RDBSource.Schema.cs`
now preserves native column type names and disposes schema readers even on failure.
The live LocalDB fixture verifies that the HeatMap creation field reports
`datetime2`. All 788 API tests passed (`TestResults/module-timestamp-verification.log`).
This is a targeted temporal mismatch check, not complete type/precision validation.
The BeepDataSources source change must ship alongside the BeepDM changes below.

HeatMap configuration persistence no longer accepts a global connection name.
The API supplies a `HEAT_MAP` module resolver, called once per saved-configuration
read/write. Missing bindings fail before datasource access. `HeatMapModule`
declares `HEAT_MAP_CONFIGURATION` for the existing discovered BeepDM module setup
pipeline; its key is declared explicitly and its duplicate inherited ACTIVE_IND
property was removed. Configuration IDs no longer use an unrelated PPDM catalog
table name. The module seeds no configurations.

Four focused tests verify successive reads after rebinding, exact ID/active filters,
missing-binding read/write rejection, and key/column metadata. All 785 API tests
passed with LocalDB integration enabled (`TestResults/heatmap-full-api-regression.log`).
The LocalDB fixture now installs this HeatMap manifest through the HTTP plan and
approved BeepDM execution path, verifies the schema, saves/reloads a configuration,
and independently reads its name and creation timestamp with SqlClient. The other
named database remains without a HeatMap table. All 785 API tests passed with this
live coverage (`TestResults/heatmap-live-full-regression.log`). Optional metadata
is nullable, the owner is stored, and creation dates come from the persisted row.

The live test found that BeepDM generated SQL Server DateTime columns as `date`,
discarding time-of-day. The sibling BeepDM source was corrected in
`DataManagementEngineStandard/Helpers/DataTypesHelpers/DatabaseTypeMappingRepositories/DatabaseTypeMappingRepository.SqlServer.cs`
to prefer `datetime2`, and in `DataTypeMappingLookup.cs` to honor preferred mappings
for system fields. These dependency changes must be shipped with the app; the
published-package path has not been verified. No existing schemas were altered.
Previously created `date` columns need a reviewed BeepDM upgrade plan, and already
discarded time-of-day cannot be recovered by changing the type.
Other global-connection service registrations still require routing review.

## Local Installer Target Guard (2026-09-12)

`Install-Repository.ps1 -LocalDevelopment` now parses the connection string and
requires one LocalDB server instance and one nonblank database name. A remote
server cannot pass by placing `(localdb)` in a database or application name;
conflicting server/database aliases are rejected before EF runs.
Run `./Beep.OilandGas.Repository.Tests/Test-LocalDevelopmentConnection.ps1` for
13 non-connecting guard cases. Those cases passed, and the actual installer passed
`-LocalDevelopment -Mode Apply -WhatIf` against the development configuration.
All 28 repository tests also passed with `OILGAS_TEST_LOCALDB=1`
(`TestResults/installer-repository-regression.log`), including offline provider
migration checks and real LocalDB installation/first-registration tests.

## Live Registration and Role Administration (2026-09-12)

The Web user screen clears its previous list and selected record before reloading,
so a denied or failed read cannot leave stale editable user data visible. The role
screen clears stale memberships and requires a successful read before enabling
assignment/removal; its reload action retries the selected user's memberships.
All 64 Web authentication/component-state tests passed, including failed user reads
and pre-load role-mutation guards (`TestResults/administration-state-web-regression.log`).
These are component-method tests, not rendered browser interaction coverage.

`RepositoryRegistrationHttpTests` now grants and revokes Reader membership through
the protected user-management HTTP endpoints instead of inserting/deleting EF
membership rows directly. Against a new isolated LocalDB repository, it verifies:

- First registration receives Administrator; a subsequent registration has no roles.
- Anonymous and unregistered callers cannot access protected user administration.
- Request-body identity forgery is ignored; repository identity comes from token claims.
- An ordinary member cannot self-promote or remove another user's Administrator role.
- Administrator grants/revocations immediately affect `/api/auth/repository/me`
  and the filtered workflow inbox on subsequent requests.
- Removing the last Administrator returns 409 and preserves that membership.
- Role changes invalidate an older user concurrency stamp; stale edits return 409,
  while a fresh read permits the intended user deactivation.
- Inactive users cannot obtain repository access or register again to regain access.

The focused HTTP integration test passed; evidence is
`TestResults/role-administration-localdb-http.log`. It applies all four SQL Server
EF repository migrations and retains the isolated test database. Its temporary
Kestrel host is stopped after the test. Authentication uses a test-only external
handler, so real OIDC token validation and browser registration remain unverified.

## Web Startup Configuration (2026-09-12)

Current runtime check: the default LocalDB repository has four applied EF
migrations and zero users, roles, or bootstrap markers. The built API and Web
hosts were started temporarily in Development against that configuration:

- `GET https://localhost:7001/health/repository`: 503, `BootstrapRequired`.
- `GET https://localhost:7066/setup/repository`: 200.
- `GET https://localhost:7001/api/identity/users`: 503 setup-required response;
  the repository readiness middleware gates this request before authentication.
- Starting sign-in redirected to `/login` with an IdentityServer-unavailable
  error, preserving `/setup/repository` as the return URL. No listener was running
  on the configured IdentityServer port 7062.

Both temporary hosts were stopped, and an independent SQL read reconfirmed zero
users, roles, and bootstrap markers. Startup does not silently create an admin.
Logs: `TestResults/current-api-startup.log` and `TestResults/current-web-startup.log`.
This verifies actual pre-registration startup and the unavailable-identity error
path, not successful OIDC registration. The first-admin HTTP integration test
uses a test authentication handler, as documented above.

The API owns the development repository at `(localdb)\MSSQLLocalDB`, database
`BeepOilGasRepository`. Web settings no longer advertise a separate, unused
`BeepOilGasDev` database. Module databases remain API-managed BeepDM bindings.

OIDC and the named IdentityServer HTTP client share one resolved authority:
`services:identityserver:https:0`, then
`Authentication:Schemes:OpenIdConnect:Authority`, then `IdentityServer:Authority`.
Blank values are skipped. Configure an absolute HTTPS URL without credentials,
query, or fragment; invalid or missing configuration fails startup instead of
silently using localhost in a deployment. The development settings explicitly
select `https://localhost:7062/`. The unused `IdentityServer:BaseUrl` fallback was
removed. Configured OIDC scopes no longer include external roles; role claims
continue to come from the app repository through its claims transformation.

All 60 Web authentication tests passed, including LocalDB configuration ownership,
authority precedence and invalid URL rejection
(`TestResults/identity-authority-web-regression.log`). This is build and regression
evidence, not a live IdentityServer registration/browser test. Existing MudBlazor
analyzer warnings remain.

## Workflow Binding Verification (2026-09-12)

### Live Lifecycle Installation and Reads

The LocalDB fixture now also calls CreateProcessDefinitionAsync and StartProcessAsync
through BoundProcessService, reloads the new instance through a fresh fixed-target
service, and verifies its field, creator, current step, two required-role assignments,
pending/blocked statuses and PROCESS_STARTED history. Independent SqlClient queries
confirm one instance, two step rows and one history row for the new workflow.
This found GetByIdAsync's unconditional dependence on PPDM catalog metadata for
extension tables. Declared single-key entities now use their key attribute directly;
tables without declared keys retain catalog formatting, and composite-key lookups
require explicit filters rather than accepting a single ambiguous ID.
The full API suite passed 781 tests with LocalDB integration enabled
(TestResults/workflow-start-full-regression.log). Successful multi-table persistence
is verified; failure rollback and atomic workflow execution remain outstanding.

The retained LocalDB fixture now binds LIFECYCLE, plans over HTTP, approves and
executes BeepDM migration for all 21 lifecycle tables, verifies their live schema,
inserts three PROCESS_INSTANCE rows through the real SQL Server datasource, and
reads matching well/facility rows through BoundProcessService/PPDMProcessService.
An independent SqlClient query confirms all three rows exist; the other named
database has no PROCESS_INSTANCE table. No pre-existing database is changed.

This exposed and fixed three installation/runtime defects:
- ORGANIZATION_HIERARCHY_CONFIG was incorrectly included in the module manifest
  despite being classified as repository-owned security data. It is excluded;
  its canonical repository implementation remains a separate audit item.
- Lifecycle tracking and core workflow entities lacked declared keys, and
  WORKFLOW_VERSION duplicated inherited EFFECTIVE_DATE. Keys now have bounded
  metadata, the reference table declares its composite key, and the version
  uses the inherited date property with its default timestamp preserved.
- PPDMGenericRepository silently discarded BeepDM-generated row objects because
  they were not application entity instances. It now maps matching fields into
  declared entities, supports DataTable/DataRow/dictionary representations, and
  surfaces invalid scalar/null conversions instead of silently dropping rows.

The full API suite passed 779 tests with LocalDB module/driver integration enabled
(TestResults/lifecycle-localdb-full-regression.log). Focused mapping tests cover
case-insensitive fields, numeric conversion, nullability, standard row containers,
and invalid rows. Full workflow creation, step/history writes and transactional
execution are not proved by this field-query test. Oracle/PostgreSQL lifecycle
execution remains unverified; local development continues to use LocalDB only.

Generic workflow instance/detail and history reads now reject instances outside
the selected field. History resolves one fixed-target service for both ownership
verification and history retrieval. Instance-list operations also pin their
connection and filter returned rows by field. Three focused cases verify allowed,
foreign-field and missing-field detail/history reads; denied history reads never
query history. The API suite passed 774 tests with LocalDB integration enabled
(TestResults/workflow-read-boundary-regression.log).
These tests mock workflow storage. The field list now calls the explicit
GetProcessInstancesForFieldAsync contract, which queries PROCESS_INSTANCE.FIELD_ID
and ACTIVE_IND instead of treating the field ID as each entity's ID. It includes
child-entity workflows without enumerating entity types, projects instance headers
without loading every step/history row, and excludes foreign/inactive rows.
A controller-to-PPDMProcessService test verifies the actual filters, well/facility
results, and one bound connection using a strict datasource mock. The expanded
API suite passed 776 tests with LocalDB integration enabled
(TestResults/workflow-field-query-regression.log). Live field-list SQL execution
and workflow history hydration are not claimed by that focused test.

The generic process step PATCH action now checks the authenticated local actor,
selected field, current step, explicit assignment, persisted required role and
definition step roles before execution. Administrator bypasses assignment/role
checks but not field/current-step checks. Unassigned steps without role rules are
denied for non-administrators. The action obtains one fixed-target process service
from BoundProcessService so authorization reads and execution share a connection.
Seven focused controller cases cover wrong role/assignee/field/current step and
permitted user/role assignments. The API suite passed 767 tests with LocalDB
integration enabled (TestResults/workflow-step-authorization-regression.log).
The generic transition action now uses the same current-step assignment/role
guard before checking or executing a transition. Generic close requires the
process creator or Administrator and verifies the selected field. Both resolve
one fixed-target service for authorization reads and mutation. Seven existing
assignment cases also exercise transitions, and four close cases cover creator,
unrelated user, administrator and foreign field. The API suite passed 771 tests
with LocalDB integration enabled
(TestResults/workflow-action-authorization-regression.log).
These cases mock workflow persistence. HTTP coverage of these action guards and
authorization of other workflow/domain action endpoints remain outstanding;
inbox visibility must not be mistaken for action authorization.

The previously missing /api/workflow/tasks inbox, counts and filtered-list routes
now exist. WorkflowTasksController derives the current user's active persona and
roles from the default repository, rejects a different requested persona, and
filters both tasks and counts by the task's assigned role. External role claims
are not used for this filtering. Empty persona input selects the stored profile;
missing/inactive profiles and inactive users are forbidden. This covers pending
cross-persona tasks, not aggregation of all approval/access-review sources.

Inbox contracts now live in TheTechIdea.Data. The Web client no longer converts
API errors into empty results, uses a distinct list endpoint, and encodes query
parameters. The inbox page requires authentication, no longer hardcodes a persona,
and clears stale tasks on refresh failure. Current responses use process/step IDs
in the display-name fields; friendly workflow names remain to be resolved.

Verification: 760 API tests with LocalDB integration and 47 Web auth tests passed
(TestResults/inbox-authorization-localdb-regression.log and
TestResults/inbox-web-regression.log). The LocalDB test invokes controller methods
with mocked task storage and verifies forged-role exclusion, cross-persona denial,
filtered counts, role removal and account deactivation.

HTTP middleware coverage is now included in the retained LocalDB registration
fixture: anonymous inbox requests return 401; unregistered, disabled and foreign
persona requests return 403. Forged external Administrator claims do not reveal
Administrator tasks; inbox/count/list responses contain only the member's Reader
task. Invalid paging/sorting returns 400, and role removal affects the next HTTP
request without restarting the host. The full API suite passed 760 tests with
LocalDB flags enabled (TestResults/inbox-http-localdb-regression.log). The test
uses its isolated external-auth handler and mocked task storage, not production
OIDC or live module task reads. Browser interaction and task-action authorization
remain unverified.

Cross-persona routing no longer queries the old PERSONA_ROLE module table. The
API's RepositoryRolePersonaReader resolves the assigned AspNetRole and joins
AspNetUserRoles, active AspNetUsers, APP_USER_PERSONA and active APP_PERSONA.
Recipients are distinct persona codes represented by current active role members,
not static role-to-persona mappings. Unknown roles fail; a role without eligible
recipients produces no tasks. Changing a persona does not grant a role or access.
Task visibility and task actions must independently enforce the assigned role;
this recipient query is not an authorization policy or a revocation mechanism
for already-created tasks.

Task routing pins one LIFECYCLE connection across step reads and all task inserts;
inbox/count reads also require a binding. The lifecycle service registrations no
longer read the global BeepOg:DatabaseConnectionName fallback. The API suite passed
760 tests with LocalDB integration enabled; evidence:
TestResults/role-persona-localdb-regression.log. A retained isolated LocalDB test
verifies inactive-user/persona exclusion, deduplication and role removal. Focused
task-write routing uses strict datasource mocks; live task writes remain unverified.

Escalation updates now resolve LIFECYCLE through the standard workflow callback.
The SLA monitor passes a callback returning its already-selected connection, so
its scan and escalation updates stay on the same target. Tests cover rejection
of unbound reassignment, level escalation and suspension, plus one resolution
across a suspension read/update. The API suite passed 757 tests with LocalDB
integration enabled; evidence:
TestResults/escalation-routing-localdb-regression.log. Focused escalation tests
use strict datasource mocks. Notification delivery, auto-approval behavior and
missing-record handling have not been established by this routing verification.

Business-event trigger registration and lookup now require the current LIFECYCLE
binding, including the lookup performed before event dispatch. Missing/blank
bindings reject operations before datasource access or dispatch. Focused tests
cover this boundary and changes between registration and event lookup. The API
suite passed 724 tests with LocalDB integration flags enabled; evidence:
TestResults/business-event-localdb-regression.log.
Program.cs now registers BoundProcessService, which resolves LIFECYCLE for each
IProcessService operation and constructs a fresh PPDMProcessService with that
fixed connection. Its nested calls and repository caches stay on that target.
Tests cover binding rejection for all 27 interface methods and current-target
forwarding for start/cancel. The API suite passed 752 tests with LocalDB module
and driver integration enabled; evidence:
TestResults/bound-process-localdb-regression.log.
These focused tests use mocked process delegates, not live workflow writes.
Event trigger lookup and process start now share one resolved connection per
event. BusinessEventTriggerService uses an explicit fixed-target process factory
instead of resolving another binding-aware service during dispatch. A regression
test changes the binding during lookup, verifies two process starts retain the
original target, and verifies the next lookup uses the new binding. The API suite
passed 753 tests with LocalDB integration enabled; evidence:
TestResults/event-dispatch-binding-regression.log. This focused test mocks process
starts; live workflow writes, transactional creation and dispatch reliability
remain unverified. Existing per-trigger exception handling can still return a
partial list of started instances and is not an atomic delivery guarantee.

Delegation-of-authority rule and escalation reads now require the current
LIFECYCLE binding instead of the global PPDM39 connection. Focused tests verify
missing/blank bindings, binding changes between operations, and propagation of
database read failures rather than empty approval requirements. The API suite
passed 721 tests with LocalDB module/driver integration enabled; evidence:
TestResults/doa-routing-localdb-regression.log. These tests verify routing and
failure handling, not the completeness of approval threshold business rules.

Handoff validation also requires the LIFECYCLE binding, pins it across process
instance and contract reads, and resolves again for the next operation. Missing
bindings fail before database access. Three focused test cases cover these
boundaries. The API suite passed 714 tests with the LocalDB module and driver
flags enabled; evidence: TestResults/handoff-routing-localdb-regression.log.
This verifies connection routing, not completeness of document/rule validation.

The SoD role lookup now uses RepositoryRolePermissionReader in the API to read
AspNetRoles and permission claims in AspNetRoleClaims from the default repository.
Identity's configured name normalizer resolves role names; unrelated claims are
excluded and permissions are deduplicated. Unknown roles and repository failures
stop evaluation rather than returning an empty permission set. SOD_RULE remains
module data and requires a resolved LIFECYCLE connection; explicit module seeding
retains its selected connection. No legacy role tables are queried by this engine.

The API suite passed 717 tests with the LocalDB module and driver flags enabled.
Evidence: TestResults/sod-identity-localdb-regression.log. The new test creates and
retains an isolated LocalDB repository, applies EF migrations, and evaluates real
Identity permissions against a strict mocked module rule source. It verifies the
Identity boundary, not complete SoD business-rule coverage or role-assignment
enforcement. Remaining runtime services and authorization consumers still require
audit before Identity consolidation can be declared complete.

Workflow versioning and dependency-graph services now require the persisted
LIFECYCLE binding through the API's ModuleConnectionResolver. They no longer
fall back to the global PPDM39 connection. Each operation resolves once and
passes that connection through its nested repository calls; the next operation
resolves the current binding again. Missing bindings fail before database access.

Focused tests verify missing-binding rejection, version history/insert routing,
multi-read prerequisite routing, and binding changes between operations.
The full API suite passed 711 tests with OILGAS_TEST_LOCALDB_MODULE=1 and
OILGAS_TEST_LOCALDB_DRIVER=1. Evidence:
TestResults/workflow-dependency-localdb-regression.log.
These focused routing tests use strict datasource mocks; the suite also runs
the separate real LocalDB repository and module installation/isolation tests.
This does not prove that all other runtime services honor module bindings or
that workflow business rules and multi-table transactions are complete.

## Rewrite Direction

The user has explicitly removed the backward-compatibility requirement. The target
is a clean repository-backed implementation, not permanent adapters around old
APP_* services, DTOs, routes or business-database authentication stores. Existing
user, persona and RBAC APIs now use canonical shared contracts. Old standalone
library implementations are not the application role source of truth.

Use canonical shared contracts/entities in TheTechIdea.Data, Identity-backed API
services, and typed Web clients. Persona profiles and preferences are account
extensions, not role/permission grants. Keep IdentityServer authentication-only.
Keep EF installation for the default repository and BeepDM migration/routing for
selected module databases. No automatic old-data import is required. Database
deletion or destructive cleanup still requires explicit approval.

Canonical persona backend is now implemented: AppPersona, AppUserPersona,
AppPersonaPreference and AppPersonaAudit live in TheTechIdea.Data and map to
APP_PERSONA, APP_USER_PERSONA, APP_PERSONA_PREFERENCE and APP_PERSONA_AUDIT.
Identity user foreign keys enforce account ownership. Profiles/preferences use
optimistic concurrency and audit writes share the same SaveChanges transaction.
Personas never grant roles or permissions. No personas or users are auto-seeded.

The new /api/personas API and PersonaClient use canonical request types with no
caller-supplied actor, physical row ID or effective-access JSON. Catalog updates
require Administrator; profiles/preferences require owner or Administrator.
PersonaExtensions is the fourth migration in each provider set. The SQL Server
migration was applied to development LocalDB on 2026-09-05 without importing old
data. PostgreSQL/Oracle migration SQL and snapshots were tested offline only.

Web Landing, PersonaSelector and PersonaContextService now use PersonaClient and
canonical models. Switching personas preserves other profile defaults and submits
the current concurrency stamp. Missing profiles route to the dashboard instead of
blocking first login. The selector also allows an initial selection when a catalog
exists, without requiring a preexisting profile. Its CSS is in wwwroot/css/app.css.
The old profile controller, API service registration and Web client methods are
removed. Persona-based route/workflow authorization is removed; persona selection
is not an authorization mechanism. Dashboard uses standard Authorize rather than
a browser-local token. Browser/OIDC verification remains outstanding.
Next: persona catalog management, remaining module routing and live verification.

Status: provider migrations, API repository registration, user/role administration,
and the local role-claims bridge are implemented. APP_* stores extend Identity.
Legacy scope/elevation services, module routing coverage, and live end-to-end
verification remain incomplete.

The repository is separate from user-selected BeepDM module databases. It owns
application authorization accounts and standard ASP.NET Identity tables, not
IdentityServer credentials or PPDM domain tables. Shared entities live in
TheTechIdea.Data. No Web project should reference this EF project.

## Providers

One shared model, three concrete contexts and migration sets:

| Database | Context | Folder |
| --- | --- | --- |
| SQL Server | SqlServerRepositoryDbContext | Migrations/SqlServer |
| PostgreSQL | PostgreSqlRepositoryDbContext | Migrations/PostgreSql |
| Oracle 19c or later | OracleRepositoryDbContext | Migrations/Oracle |

Oracle uses 19c SQL compatibility, NUMBER(1) booleans and identifiers capped at
30 characters. These are generated migrations; real-server installation and
upgrade testing is still required for every supported database version.

## Schema

- AspNetUsers, AspNetRoles, AspNetUserRoles
- AspNetUserClaims, AspNetRoleClaims, AspNetUserLogins, AspNetUserTokens
- RepositoryBootstrap: intended singleton marker for transactional first-admin setup
- ModuleDatabaseBindings: module ID to persisted BeepDM connection name, no credentials

An external identity must be linked by validated issuer and subject, not email.
Do not populate PasswordHash or enable a second local login system merely because
the standard Identity schema includes password fields. Role permissions can use
AspNetRoleClaims; application roles must not come from IdentityServer role claims.

## Generate and Apply

Local development uses SQL Server LocalDB, instance MSSQLLocalDB, database
BeepOilGasRepository. ApiService/appsettings.Development.json configures that
repository. Install locally:

```powershell
./Beep.OilandGas.Repository/Install-Repository.ps1 -LocalDevelopment -OutputPath ./repository-install.sql
# Review the generated SQL, then apply pending migrations:
./Beep.OilandGas.Repository/Install-Repository.ps1 -LocalDevelopment -Mode Apply
```

The PowerShell 7 installer requires the .NET 10 SDK and dotnet-ef 10.x. Its default
mode generates idempotent SQL without connecting to the database. It refuses to
overwrite an existing output file. Apply mode updates to the latest migration;
it has no rollback/drop option. Use -WhatIf to inspect the requested operation.
LocalDevelopment reads the API development configuration and rejects a simultaneous
OILGAS_REPOSITORY_CONNECTION setting so the target cannot be ambiguous.

For another installation, supply OILGAS_REPOSITORY_CONNECTION through your secure
environment configuration, then use -Provider SqlServer, PostgreSql or Oracle.
The connection string is not passed as a command-line argument or printed by the
wrapper. The wrapper restores the prior process environment even when EF fails.
Configure the API's Repository:Provider and Repository:ConnectionString separately
to match the installed repository; the installer does not rewrite application config.

```powershell
./Beep.OilandGas.Repository/Install-Repository.ps1 -Provider PostgreSql -OutputPath ./postgres-install.sql
./Beep.OilandGas.Repository/Install-Repository.ps1 -Provider PostgreSql -Mode Apply
```

Installer verification: generated idempotent SQL for all three real EF provider
contexts; no live PostgreSQL or Oracle connection was used. Run the no-database
guardrail checks with `pwsh -File Beep.OilandGas.Repository.Tests/Install-Repository.Tests.ps1`.

This requires SQL Server Express LocalDB on Windows and creates the repository
database if absent. It does not configure or migrate any BeepDM module database.

An earlier LocalDB inspection on 2026-09-05, before PersonaExtensions, confirmed:
InitialRepository, IdentityExtensions, UserExtension. All seven AspNet* tables,
five APP_* extension tables, RepositoryBootstrap and ModuleDatabaseBindings exist.
All seven extension foreign keys are enabled and trusted, linking extensions to
AspNetUsers, AspNetRoles, AspNetRoleClaims and APP_PERMISSION as appropriate.
At that inspection the database had zero users, bootstrap markers and module bindings.
No accounts were created by this inspection; first registration is still pending.
The current repository suite includes offline SQL generation/model checks for
all three providers. These are not evidence of live Oracle/PostgreSQL installation.

The opt-in LocalDbInstallationTests integration test creates a uniquely named
BeepOilGas_Integration_* database and deliberately retains it for inspection.
Enable it with OILGAS_TEST_LOCALDB=1 when running Repository.Tests on Windows.
It applies migrations twice, checks passwordless first-admin and ordinary second
registration, verifies replay does not duplicate accounts, and checks readiness.
The live run on 2026-09-05 passed using
BeepOilGas_Integration_779aeb3e8dee414ea5725b4fcb10003b. This test does not exercise
HTTP/OIDC or simultaneous first registrations. It does not modify the development
repository or drop databases; cleanup requires separate explicit authorization.

A separate live LocalDB race test uses a test-only query barrier to make two
registrations observe the empty user table before either proceeds. The run passed:
one Created result, one conflicting transaction rolled back with no orphan user or
login, then an ordinary registration on retry. Exactly one bootstrap marker and
admin membership remained. Retained database:
BeepOilGas_Integration_3378d765efb74f96b97b33e5b64defcd. This verifies SQL Server
service/store concurrency, not multi-provider or HTTP/OIDC concurrency.

Latest live verification (2026-09-05): all 27 repository tests passed with
OILGAS_TEST_LOCALDB=1. Fresh installation applied all four current migrations,
including PersonaExtensions, and replay left no pending migrations. First/second
registration and readiness passed against SQL Server. The forced registration
race also passed with one administrator and no orphan rows after rollback.
Retained fresh-install database: BeepOilGas_Integration_5ba927d237694bcb8fe02892ec2fbe0e.
Retained race database: BeepOilGas_Integration_e05f2f34ff9446c19cf07799fc787e3c.
Detailed evidence: TestResults/localdb-current-repository.log. These tests neither
modify the default application database nor exercise OIDC or BeepDM module DDL.

The API requires Repository:Provider and Repository:ConnectionString. Production
has no implicit LocalDB or PPDM39 fallback. AddOilGasRepository registers scoped
EF contexts and UserManager/RoleManager stores without replacing JWT authentication.
It does not run migrations or seed an administrator. GET /health/repository returns
Ready (200) or Unavailable, MigrationRequired, BootstrapRequired, RecoveryRequired (503); response
bodies do not expose connection strings or database exception details.

## First Registration

POST /api/setup/repository/register with an API-valid IdentityServer bearer token.
The endpoint derives the external issuer and subject from that token, never from
submitted JSON. The first OilGas registration becomes Administrator; subsequent
registrations create ordinary passwordless accounts without automatic roles.
The account, login link, role membership and bootstrap marker commit together.
Initial registration also copies optional name/email metadata from the validated
principal: name goes to APP_USER.FullName, email to AspNetUsers.Email, and
EmailConfirmed is true only for a valid email with an explicit email_verified=true
claim. Names are limited to 1000 characters, emails to 256, and malformed/control-
character values are ignored without preventing registration. Request-body profile
fields are ignored. Accounts are never matched by email: two different subjects may
share an email and remain separate accounts, with only the first receiving Admin.
Repeated registration does not overwrite locally maintained profile metadata.
The shared ExternalRegistrationProfile contract lives in TheTechIdea.Data.
LocalDB tests and the HTTP registration fixture verify these rules; all 29 repository
tests and 836 API tests passed (TestResults/registration-profile-repository.log and
registration-profile-api.log). No schema changes or development users were created
by this profile update. Display metadata remains optional when the provider omits it.
If an empty-user installation already has a normalized match for the reserved
Administrator role with different casing, first registration reuses its ID and
canonicalizes its display name for standard role guards. Last-admin membership
removal uses Identity normalization rather than a case-sensitive display-name
comparison. Regression tests cover lowercase and uppercase role names, independent
removal/deactivation checks, and canonical role claims after first registration.
Repeated calls are nonduplicating. Concurrent first registrations may require a
retry after a transaction conflict; the singleton marker prevents a second admin.
Existing users without a marker require operator reconciliation, not automatic
promotion of the next registrant. Keep new deployments private until registration.
The Web OIDC callback calls RepositorySignInService using the access token. The
shared RepositoryRegistrationResponse must confirm Created, Registered, or
AlreadyCompleted before TokenProvider receives the token. HTTP errors, malformed
payloads, missing status, and unexpected outcomes fail sign-in without replacing
the cached token. Only then is the request token made available to the role bridge.
The claims transformation calls the me endpoint and replaces external roles with
application roles; failures deny access. All 81 Web tests and 836 API tests passed
(TestResults/registration-signin-web.log and registration-signin-api.log), including
registration outcome validation and token-cache ordering. Live API database tests
used LocalDB only. Interactive sign-in against the configured IdentityServer remains
unverified; port 7062 was still not listening at the latest check.
The subsequent isolated IdentityServer probe successfully served HTTPS discovery
and its registration page, applied 49 authentication-server migrations to a new
LocalDB database, and confirmed zero seeded users/clients. The temporary process
was stopped and its database retained. Full test-account registration awaits consent
approval; OAuth client provisioning and encrypted-token compatibility also need
verification. See docs/identity-and-web/LOCALDB-IDENTITY-VERIFICATION.md for the
recorded evidence and remaining end-to-end steps. This did not modify the OilGas
development repository or the IdentityServer's personal configuration.

The unused /register-callback and /first-login pages were removed. They offered
obsolete demo/global-connection setup independent of repository readiness and module
bindings. No remaining Web navigation references those routes; route-reflection
tests enforce their removal. Initial installation uses /setup/repository, and
Administrator module configuration uses /admin/module-databases. No compatibility
redirects or automatic demo database creation were added.

## API Role Resolution

The API now resolves roles and permission claims from AspNetUserRoles and
AspNetRoleClaims through RepositoryClaimsTransformation. External role/permission
claims are discarded. Disabled users and lookup failures fail closed; unknown
authenticated users have no app roles and can call the registration endpoint.
GET /api/auth/repository/me is a read-only authoritative access endpoint for the
Web claims bridge. It does not provision users. Legacy role-management endpoints
are Administrator-only and now update Identity membership/claims and APP_* history
in the repository together.

## APP_* Extensions

Administrator-only POST /api/identity/roles creates a standard Identity role and
APP_ROLE metadata in one transaction. The existing role-assignment page includes
name/description inputs for this operation. Names are trimmed, limited to 256
characters, and cannot contain commas; Identity enforces normalized name uniqueness.
Creating a role does not assign it to any user or grant permissions automatically.
Role deletion/rename UI and live browser verification are not yet implemented.

Role-assignment reads now start from AspNetUserRoles, including memberships created
outside the extension adapter. Missing APP_USER_ROLE metadata does not hide or
prevent removal of a membership. Read responses use an opaque identity-prefixed
membership reference without inserting metadata; callers should round-trip it
unchanged. Revocation creates observation history with unknown original grant
details when necessary. Last-administrator protection applies in both cases.

Permission reads likewise start from AspNetRoleClaims with ClaimType=permission.
Matching extension metadata is attached where present; otherwise an opaque claim
reference is returned for revocation. Removing such a claim detaches any linked
history and logs the local actor without inventing historical approval details.
Non-permission claims cannot be removed through this path. Reads create no metadata.

- APP_USER: full name, tenant/business-associate references and audit metadata for
  AspNetUsers. Account activation remains in AspNetUsers; credentials stay external.
- APP_ROLE: one-to-one metadata for AspNetRoles, including field scope and sensitivity.
- APP_PERMISSION: permission metadata; PermissionKey is the permission claim value.
- APP_USER_ROLE: assignment reasons, approvals and effective-date history linked to
  AspNetUsers/AspNetRoles. Active access comes from AspNetUserRoles.
- APP_ROLE_PERMISSION: grant history linked to AspNetRoleClaims while active. On
  revocation the claim link is cleared and the history is retained.

IdentityExtensions migrations exist for all three providers and are applied to
development LocalDB. Old APP_* data in module databases is not automatically
imported, renamed or removed. Legacy IDs require explicit mapping to Identity IDs
before import. Scope and SoD enforcement services still need migration; preserving
metadata alone does not establish field-scoped authorization.

Set OILGAS_REPOSITORY_CONNECTION in the process environment using a secret source.
Use the context matching that connection. Commands run from the solution root:

```powershell
dotnet ef migrations script --project Beep.OilandGas.Repository --context SqlServerRepositoryDbContext --idempotent --output repository.sql
# Review the script and back up an existing repository before applying.
dotnet ef database update --project Beep.OilandGas.Repository --context SqlServerRepositoryDbContext
```

Use PostgreSqlRepositoryDbContext or OracleRepositoryDbContext for the other
providers. Oracle installations must provision the database/service and schema
account separately; migrations create objects under that account. These commands
must never target a module connection just because a user selected it in a wizard.

For a model change, scaffold a migration for EACH context into its own folder.
Do not use EnsureCreated with this migrations-managed schema. Production startup
must not silently migrate the database; use an explicit installation command.

## Verification

User management now reads/writes the repository. Deactivation is soft and cannot
disable the last administrator. Local password creation and retired credential
routes are removed; users register through IdentityServer and then OilGas sign-in.
The administrator user screen is `/admin/access-control/users`, reachable from
the user-role screen. It lists/searches accounts and edits the name and active
status through the typed API client. Deactivation requires confirmation and saves
carry the displayed Identity concurrency stamp. Username and verified email are
not editable. There is no local-password creation form. Live browser verification
of this screen is still outstanding.

Module setup API (Administrator only): GET /api/setup/modules lists bindings;
PUT /api/setup/modules/{moduleId}/connection accepts ConnectionName and the last
ConcurrencyStamp (null for a new binding). POST /api/setup/modules/{moduleId}/plan
creates a BeepDM plan using that persisted connection. Review and approve the
returned plan using the existing schema-migration endpoints before execution.
This does not apply migrations automatically or redirect the EF repository.
The older all-module wizard and domain service routing are not yet converted.

The Administrator-only /admin/module-databases page is available under Data >
Module Databases. It lists discovered modules and BeepDM connection names, saves
bindings with concurrency stamps, and generates read-only migration plans with
environment/backup/restore evidence. The connections endpoint returns names only,
not credentials. Binding edits clear the displayed plan. Approval and execution
require explicit review confirmation; high-risk operations require acknowledgement.
The client verifies approval hashes and sends expected plan/manifest hashes when
applying. It rechecks the saved binding before either action and disables repeat
execution after an attempt. Unknown outcomes require status investigation rather
than an automatic retry. Browser and live migration validation remain pending.

PPDM setup has no anonymous action overrides, including planning, SQLite creation,
status, CI validation, and artifact retrieval. These routes require Administrator.
Approval, synchronous execution, and background start overwrite caller-supplied
actor names with the local NameIdentifier and reject requests without that identity.
Migration environment input is validated before datasource access; unknown, blank,
and numeric values no longer silently downgrade to Development. Protected remains
an alias for Production. These changes require old setup clients to authenticate.

POST /api/setup/modules/{moduleId}/seed runs only the selected module against its
saved BeepDM connection. Supply the current binding ConcurrencyStamp in the request;
missing or changed bindings return 409. It uses the local authenticated user ID for
audit, requires Administrator, and does not accept a connection override or audit
user from the caller. Missing connections never fall back to PPDM39. Seed failures
and partial errors are not reported as success. Apply the module schema before
seeding. Six mocked endpoint tests cover routing and rejection paths; they do not
verify a live module database.

PPDM_CORE selection now includes the canonical PPDM model catalog even though its
seeding module has no declared extension types. Combined selections deduplicate
shared references. Feature-only selections do not automatically install all PPDM.
GAS_LIFT now declares GAS_LIFT_DESIGN and GAS_LIFT_PERFORMANCE alongside its
reference-code table, using existing shared types rather than duplicate entities.

Runtime routing prerequisite: other manifests still need comparison with service
persistence. Reconcile schema ownership, required keys and cross-module dependencies
before routing services to isolated databases. Live generated-DDL validation remains
necessary; manifest inclusion alone does not establish a working module installation.

GasLiftService runtime persistence now resolves GAS_LIFT through the API's scoped
ModuleConnectionResolver for each operation. All three design/performance data paths
use that saved BeepDM connection; unbound modules and deleted connections fail rather
than falling back to PPDM39. Resolution is deferred so calculation-only methods do
not require a database. Other domain service registrations and shared defaults are
still pending routing audits. Tests cover resolver lookup and failure before legacy
datasource access; successful live writes to a separate module DB remain unverified.

EconomicAnalysisService result reads/writes now resolve ECONOMICS on demand using
the same API resolver. Its manifest includes ECONOMIC_ANALYSIS_RESULT and its three
economic reference tables. The economics seeder no longer invokes broad accounting
reference seeding against the economics connection. Tests cover manifest inclusion
and unbound-operation rejection; live separate-database persistence remains pending.

NodalAnalysisService and FlashCalculationService now resolve NODAL_ANALYSIS and
FLASH_CALCULATIONS respectively in API persistence paths. Nodal resolves once per
save and passes the selected connection into curve snapshot persistence, preventing
a binding change from splitting one save across databases. Flash's manifest now
includes FLASH_CALCULATION_RESULT. Existing standalone constructors retain explicit
connection compatibility; the API always supplies the repository-backed resolver.
Tests verify unbound reads/writes fail before legacy datasource access. They do not
establish transactional atomicity or successful live provider-specific persistence.

OIL_PROPERTIES and GAS_PROPERTIES are now discoverable schema modules for the
existing composition/result entities in Models.Data.Common. They do not insert
sample composition data. API oil/gas property services resolve those bindings only
on persistence calls; gas header/component operations share one resolved target.
Tests cover declared tables and rejection of unbound reads/writes before defaults
or legacy datasource access. Their schemas and writes still need live validation.

The global setup gate and /health now use repository readiness, not the presence
of a hardcoded PPDM39 datasource. This permits administration before module setup.
Module connections are validated independently by the module planning path.

ProductionForecastingService now routes forecast headers/points through the
PRODUCTION_FORECASTING binding and production-history fitting through PPDM_CORE.
Each forecast read/save uses one resolved target for its header and points.
Explicit decline parameters can bypass history fitting. Tests verify the distinct
binding requests and unbound-operation rejection, not live cross-database fitting.

PipelineAnalysisService routes its four configuration/result persistence paths
through PIPELINE_ANALYSIS. The new discoverable module declares PIPELINE and
PIPELINE_ANALYSIS_RESULT from Models.Data.PipelineAnalysis without inserting sample
data. Result saves now map to the configured entity type instead of passing the
service DTO into the entity repository, and use the result table's ID formatter.
Tests cover module tables and rejection before legacy access when unbound. Live
DDL and successful read/write verification remain outstanding. This routing does
not migrate the separate LifeCycle PipelineManagementService or its workflows.
FacilityManagementService now resolves storage by entity ownership at repository
creation: PPDM39 facility, work-order and production tables use PPDM_CORE, while
FACILITY_MEASUREMENT and FACILITY_EQUIPMENT_ACTIVITY use FACILITY. API registration
supplies the saved-module resolver; an empty or failed binding never falls back to
the global connection. Tests exercise factory ownership and a public read failure
before datasource access. They do not establish successful cross-database workflows
or atomicity across separate repositories. Runtime connection lookup also rejects
duplicate configured names, matching migration-fingerprint ambiguity checks.
Module connection selection, binding, planning and seeding now use the same
case-insensitive uniqueness rule. Ambiguous names are omitted from the selector;
binding rejects them with 400, while planning/seeding reject them with 409 before
opening a datasource or invoking migration/seeding. A regression test uses two
case-variant names with different targets to exercise all four entry points.
ProductionManagementService now resolves PPDM_CORE before creating repositories
for all seven PDEN/facility read and create paths, including facility declarations.
The API supplies the saved binding and lookup receives the operation cancellation
token. Tests cover every public storage path failing before datasource access when
the binding is unavailable.
ProductionOperationsService also resolves saved bindings: PPDM volume, well and
equipment-maintenance records use PPDM_CORE; operation cost records use PRODUCTION.
PRODUCTION_COSTS is now explicitly declared by ProductionAccountingModuleSetup so
BeepDM installation includes that persisted extension table. Facility workflows
delegate to the independently routed FacilityManagementService. Four new read-path
tests verify ownership lookup before datasource access. Successful live reads,
writes and installation of the cost table remain unverified; several unrelated
advanced methods still contain placeholder business behavior.
SeismicAnalysisService resolves PPDM_CORE for SEIS_ACQTN_SURVEY and EXPLORATION
for its PROSPECT validation repository, using the exploration registry constant.
API registration supplies saved binding lookup for both. Tests cover list/read
and prospect-validation failures before datasource access. Other prospect services
are not covered by this change, and live cross-database survey creation remains
unverified.
ProspectEvaluationService now follows the same ownership split: its prospect
repository resolves EXPLORATION and its seismic repository resolves PPDM_CORE.
The API supplies persisted module bindings. Tests cover public prospect list/read/
evaluation entry points and the seismic repository factory rejecting unavailable
bindings before datasource access. These tests do not verify a complete evaluation
across two live databases.
ProspectIdentificationService's shared PROSPECT factory now resolves EXPLORATION
as well. Four entry-point tests cover list/create/evaluate/rank rejection before
datasource access. The 26-test ProspectIdentification domain suite passes after
the factory changes; this is not live persistence verification.
DrillingOperationService now resolves PPDM_CORE for its WELL and WELL_DRILL_REPORT
repositories. Repository wrappers are no longer cached on the service, and helper
calls forward their cancellation token to binding resolution. Factory tests verify
failure before datasource access and token forwarding. The DRILLING_EXECUTION
extension schema is not used by these existing core-table workflows; migrating
those business workflows to the extension model is a separate change.
LeaseAcquisitionService's persisted core workflows resolve PPDM_CORE once per
operation and pass that connection to LAND_RIGHT, LAND_AGREEMENT and LAND_STATUS
repositories. Missing/empty bindings fail before repository access. These paths
do not write the LEASE_ACQUISITION extension tables. Tests cover list/evaluation
binding failures; multi-table atomicity and live lease persistence remain unverified.
EnhancedRecoveryService now resolves PPDM_CORE before constructing its PDEN, FIELD,
WELL and PDEN_FLOW_MEASUREMENT units of work. API registration provides the saved
binding. Four factory tests verify failure before datasource access, and all 13
EnhancedRecovery domain tests pass. This does not relocate the separate enhanced-
recovery reference table or prove live module persistence.
PlungerLiftService now resolves PPDM_CORE before all five WELL_ACTIVITY storage
paths (design save/read/update and performance save/read). Five public entry-point
tests verify an unavailable binding fails before datasource access. This change
does not make the existing WELL_ACTIVITY projection a complete design/performance
round-trip; its business-model persistence limitations remain.
SuckerRodPumpingService's WELL_ACTIVITY save now resolves PPDM_CORE before repository
creation. Its binding-failure test verifies no datasource access on an empty target.
The saved activity remains an incomplete projection of the full pump design.
Accounting services still contain hardcoded PPDM39 targets and require a separate
ownership/routing pass; calculation-only registrations do not imply persistence.
GLAccountService and JournalEntryService now resolve PRODUCTION in the API. Saved
binding lookup takes precedence over journal read methods' connection argument.
GL_ACCOUNT, GL_ENTRY, JOURNAL_ENTRY and JOURNAL_ENTRY_LINE are now declared by the
PRODUCTION module for BeepDM installation. The unused pre-repository metadata read
was removed from both factories. Tests verify missing bindings prevent metadata/
datasource access, including a supplied connection override. No standalone
Accounting.Tests project exists in this checkout; live ledger posting is unverified.
Other accounting services still require routing changes.
APInvoiceService and APPaymentService now use the same PRODUCTION binding as GL
and journal posting in the API. AP_INVOICE and AP_PAYMENT are explicitly declared
for module migration. Tests verify missing bindings fail before metadata or
datasource access. These changes do not make invoice/payment/GL writes atomic,
and successful live posting remains unverified.
Invoice/payment ID lookups now propagate storage failures instead of converting
them into null/not-found results; the missing-binding tests cover this behavior.
PurchaseOrderService now uses PRODUCTION and propagates storage failures from
GetPOByIdAsync. PURCHASE_ORDER, PO_LINE_ITEM and PO_RECEIPT are declared by the
module for installation. Its regression test verifies missing bindings fail before
metadata/datasource access. Live order and receipt write verification remains open.
Accounting InventoryService now resolves the PRODUCTION binding for item creation
and its shared repository factory. Item, list and transaction reads propagate storage
failures instead of returning misleading empty results. INVENTORY_ITEM and
INVENTORY_TRANSACTION are declared for module installation. Four regression cases
verify missing bindings prevent metadata/datasource access. Live inventory writes
and atomic inventory/ledger posting remain unverified.
InventoryLcmService also uses the API's PRODUCTION resolver, overriding caller
connection names. INVENTORY_ADJUSTMENT, INVENTORY_VALUATION and PRICE_INDEX are
included in module installation. Two regression cases cover missing-binding
failures before datasource/metadata access for valuation reads and adjustments.
Successful live write-down posting and transaction atomicity remain unverified.
ARService uses the API's PRODUCTION resolver for all repository construction,
overriding caller connection names. AR_INVOICE, AR_PAYMENT and AR_CREDIT_MEMO are
declared for module installation. Invoice/payment read regression tests verify
missing bindings stop access before metadata or datasource calls. Live payment
posting and multi-step payment transaction atomicity remain unverified.
Accounting PeriodClosingService now resolves PRODUCTION for its direct closing-entry
lookup, matching the routed journal service used for reversals. The reopen regression
test verifies a missing binding prevents database/metadata access. Trial balance
delegates to the already routed GL service. Live close/reopen remains unverified.
Subledger ReconciliationService now has an explicit API registration resolving
PRODUCTION, matching its GL comparison service. Receivables, payables and inventory
tests verify missing bindings stop before datasource/metadata access. These tables
are already declared in the module. Live reconciliation remains unverified.
BankReconciliationService now uses PRODUCTION for its AP_PAYMENT and
JOURNAL_ENTRY_LINE reads, matching its routed GL dependency. Two regression cases
verify missing bindings block check-clearing and aged-item queries before access.
Successful live bank reconciliation queries remain unverified.
Registration audit found duplicate, unrouted constructions behind IJournalEntryService
and IARService. Both now resolve the configured scoped concrete services, so interface
consumers use the same PRODUCTION resolver. The 596-test API suite passes after this
change; full host resolution and live interface-consumer workflows remain unverified.
IAccountingServices is still not registered in the API. Production accounting's
optional aggregate consumers therefore retain fallback paths requiring further
rewrite/routing; concrete-service tests do not prove those paths are configured.
RoyaltyService no longer depends on the optional accounting aggregate. It uses the
injected journal interface and a required PRODUCTION resolver for all twelve former
repository-construction paths. Typed repositories replace metadata-based type lookup.
ROYALTY_INTEREST, OWNERSHIP_INTEREST and ACCOUNTING_COST are now declared for module
installation. Two read tests cover missing bindings despite caller connection names.
Live calculation/payment workflows and transaction atomicity remain unverified.
Production period closing now requires the configured JournalEntryService and a
PRODUCTION resolver instead of the optional accounting aggregate. All remaining
direct repositories use the resolver and typed entities. Readiness, close and
unreconciled-item entry points reject missing bindings before checks begin.
Direct fallback journal writes were removed; closing journals use create/post.
The repeat-close lookup uses field/period reference plus source module and rejects
unfinished journals. This is not a concurrent idempotency guarantee. Module setup
now declares ASSET_RETIREMENT_OBLIGATION and LEASE_CONTRACT. Three binding tests
pass; successful close posting, optional adjustment-service wiring and live
transaction/recovery behavior remain unverified. On 2026-09-12, the build also
reported NU1903 for Microsoft.OpenApi 3.5.2 and assembly-version warnings.
Production InventoryService now requires the PRODUCTION resolver for tank reads,
updates, valuation, transactions and reconciliation reports. Its typed repository
factory replaces both direct tank constructors and metadata-based type resolution.
All accessed inventory tables were already declared in the module. Two regression
tests verify missing bindings block tank reads/updates despite caller overrides;
the validation-only test retains its no-database-access behavior. Live inventory
reads/writes, valuation and reconciliation reports remain unverified.
Production AllocationService now requires the PRODUCTION resolver for result/detail
reads, history and reversal repositories, using typed entities. AllocateAsync forwards
the resolved connection to its engine instead of the caller override. Three read
failure tests and one engine-forwarding test pass. Allocation tables were already
declared. Successful live allocation persistence and reversal atomicity remain
unverified.
AllocationEngine now requires its own PRODUCTION resolver. Each database entry
point resolves once and passes that connection through private typed repositories,
including ownership and division-order reads. Direct engine calls cannot use a
caller override. Three missing-binding tests pass for allocation, result and detail
entry points. All engine tables were already declared. Live persistence, failure
recovery and allocation-method correctness remain unverified.
RevenueService now requires PRODUCTION resolution before revenue recognition and
passes the resolved connection through its private typed repositories and the
lease-interest service call. Two tests cover missing and failing resolvers without
metadata/datasource access. Validation remains database-independent. All six table
types were already declared. Live revenue persistence and multi-write atomicity
are unverified.
LeaseEconomicInterestService now requires the PRODUCTION resolver for ownership,
royalty-interest and division-order repositories. Caller connection overrides do
not select storage. Three missing-binding tests cover public reads and economic
validation entry. All three tables were already declared. Successful live reads
and consistent multi-query validation during concurrent rebinding remain unverified.
PricingService now requires the PRODUCTION resolver for current and historical
PRICE_INDEX queries, replacing metadata-based type resolution. Revenue calculation
and average-price methods delegate to those routed queries. Two missing-binding
tests pass. PRICE_INDEX was already declared. Live queries and the business policy
for missing-price fallback remain unverified.
MeasurementService now requires the PRODUCTION resolver for recording, ID lookup,
well history and lease history. One typed MEASUREMENT_RECORD repository factory
replaces four metadata-based constructors. Four missing-binding tests pass, and
the table was already declared. Successful live recording and query behavior
remain unverified.

RunTicketController list, lookup and create now use an async RunTicketStore with a
required PRODUCTION binding instead of ProductionManager's in-memory cache. Create
uses the authenticated local NameIdentifier for audit/optional GL posting; a missing
local actor is forbidden. Responses now include RUN_TICKET_ID. Three store tests
verify missing bindings prevent access. RUN_TICKET was already declared. Other
service endpoints on this controller still require routing review. Live persistence and atomic ticket/GL posting are
unverified.
Pricing valuation and lease reporting now await RunTicketStore too. The store's
date-range query accepts a lease filter. No API controller calls ProductionManager's
run-ticket methods after this change. Pricing index/calculation and report/lease
facades remain in use and need replacement separately. The 624-test API suite
passes, but live HTTP pricing/reporting behavior is unverified.
The four unused ProductionManager run-ticket facade methods and their static
RunTickets dictionary have now been removed after a source-wide caller check.
API tests passed at that step (624). Other compatibility managers remain and must
not be mistaken for completed database-backed workflows.
Tank inventory endpoints now use async TankInventoryStore against the PRODUCTION
binding and TANK_INVENTORY instead of a static dictionary. Creation requires a
local authenticated actor and rejects negative volumes. The unused ProductionManager
facade and its last inventory cache were removed. Two missing-binding tests pass;
the final API suite passes 626 tests. Live persistence/restart behavior is unverified.
Tank and run-ticket create actions explicitly require an authenticated principal
with local NameIdentifier, in addition to the API authentication pipeline. Four
tank-controller tests cover external-subject-only and unauthenticated-ID rejection,
negative-volume validation without storage access, and missing-binding errors not
being presented as missing inventory. These are direct controller tests, not a
live OIDC/HTTP authorization verification.
Run-ticket production-cycle processing also requires authenticated local identity;
the sub/system audit fallback was removed. Four direct controller cases cover
creation and cycle rejection for external-subject-only or unauthenticated-ID
principals, including a forged userId query value.
GLIntegrationService.PostProductionToGL now creates balanced dated lines through
IJournalEntryService and requires successful posting before returning the persisted
journal ID. It uses the existing default cash/receivable and revenue account IDs;
the journal service validates those accounts. Three mock-based tests verify date,
reference, line balance, account selection and failed-post propagation. Other GL
integration methods still return placeholders. Live production posting, configured
account mapping, retry idempotency and atomic ticket/journal writes remain unverified.

API module discovery excludes the legacy SecurityModule. Reference-data seeding
and schema planning also enforce the repository boundary: newly planned module
schemas reject entities in legacy Security, UserManagement Identity, canonical
OilGas repository and ASP.NET Identity namespaces before datasource access. This
applies to explicit assembly/namespace requests as well as module manifests.
Executable plan sessions are currently process-local, not rehydrated from exported
artifacts after restart. Regenerate and approve a new plan after restart. Regeneration
always resets approval and prior execution-token state, including when a plan ID is
reused. Direct and background execution require both reviewed plan/manifest hashes;
omitting them is rejected before datasource access. The older PlanHash/ManifestHash
request fields remain accepted as aliases for the Expected* fields.

Reference-data seeding
does not seed legacy users or roles, and the legacy security seeder is no longer
registered by the API. Database creation defaults SeedDefaultSecurityData to false
and rejects explicit true before opening a connection. PPDM setup endpoints require
the local Administrator role. Repository registration remains the account creation
path. Existing legacy security data is left untouched for explicit reconciliation.

```powershell
dotnet test Beep.OilandGas.Repository.Tests
```

Authorization regression coverage also verifies that API and Web claims bridges
discard external `permissions` and `elevated_permissions` list claims before adding
local Identity grants. The shared permission handler checks every individual
`permission` claim and rejects unauthenticated principals. Its legacy list support
remains for other consumers; those lists are not accepted from OilGas external
tokens. The API's current named admin policies require the local Administrator role.
The API default and fallback policies now require an authenticated local repository
user ID, which the claims bridge strips from external tokens and supplies only for
active local accounts. Registration and repository account lookup explicitly use
an external-account policy so first registration remains possible. The controller
mapping no longer adds an unconditional default policy on top of that exception.
Health endpoints remain explicitly anonymous. Policy-combination tests cover plain
Authorize, unannotated endpoints, registration and lookup, including disabled and
unregistered accounts and a forged external local-user ID.
Permission revocation targets the exact AspNetRoleClaims row, including when called
through an APP_ROLE_PERMISSION ID. Stale metadata whose role/key no longer matches
the claim cannot revoke it. Linked history is detached and ended without deleting
history records. Duplicate Identity claims remain individually visible and
revocable; revoking one does not silently remove the others. Granting tolerates
existing duplicate claims without adding another, and replaces stale extension
history when a new grant is needed after an external claim change.
Module plan creation requires the ConcurrencyStamp returned by the module binding
list/save response. Missing or stale stamps return 409 before invoking BeepDM;
the Web planning request includes its displayed binding version. External callers
must reload the binding before retrying. API-hosted module-scoped plans capture a
server-side fingerprint of the selected module IDs, connection names and binding
versions. Approval, execution, queue submission and the queued worker revalidate
it and fail closed on changed/missing bindings or repository errors. Every selected
module must be bound to the plan's connection. Tests cover version changes even
when the connection name stays the same, missing/misdirected bindings, failed
approval/execution/queue gates and successful approval with a matching fingerprint.
This is not a distributed transaction or a lock held across BeepDM DDL execution.
The fingerprint also includes configured provider/driver, host/database/schema,
file/URL, connection string, credentials, parameters and SQL transport settings.
Only its SHA-256 digest is retained in the process-local plan; credentials are not
added to plan responses or logs. Duplicate configured connection names are rejected.
Display preferences and parameter dictionary ordering do not invalidate a plan.
This observes configuration, not a live database identity: cached datasource
reconfiguration and concurrent edits after the final check still need validation.
Legacy assembly/namespace-scoped plans have no module fingerprint.
The permission catalog includes custom AspNetRoleClaims permission values as well
as APP_PERMISSION metadata and built-in codes, without creating rows during reads.
Non-permission claims are excluded. Explicit grants accept known permission keys
or metadata IDs and reuse existing metadata by key; arbitrary unknown codes remain
rejected. Tests verify custom grants and metadata-ID/key reuse without duplicates.
Persona profile/preference endpoints now require the route user to be the local
actor or the actor to hold the local Administrator role. Missing local IDs fail
closed, including on administrator-shaped principals. Route user IDs override
submitted profile IDs, and effective-access JSON cannot be submitted as a user
preference. Tests verify rejection before storage and owner/admin write behavior.
The old PersonaProfileService is no longer registered or called by the application.
The canonical PersonasController uses RepositoryPersonaService directly; tests
cover owner/admin writes, stale version conflicts and catalog role requirements.
Canonical profile and preference contracts contain no submitted physical row IDs,
actor IDs or effective-access JSON. Storage keys are derived from the authorized
route user and persona/view keys; profile and preference changes are audited.
Revenue GL posting now shares the production journal creation/posting workflow,
preserving the transaction date, reference and local actor with a distinct REVENUE
source. It returns only the journal service's persisted ID after successful posting.
Tests cover cash/receivable balance, failed posting, invalid inputs and missing IDs.
Default account IDs remain in use; other GL integration methods remain placeholders.
Live posting, configured account mapping and retry atomicity remain unverified.

API startup no longer invokes ProcessDefinitionInitializer as SYSTEM against the
global business connection. That initializer's unused API registration is removed.
Lifecycle reference seeding now writes every reference set to its declared
R_LIFECYCLE_STATE_REFERENCE table, not three undeclared table aliases. Tests execute
the real seeder against two named mock datasources and verify target separation,
actor attribution, repeat-run skips and cancellation before datasource access.
The full workflow-template catalog still needs an explicit selected-binding
installation path; removing startup writes does not complete that work. Live
module DDL and seeding on selected provider databases remain unverified.

The all-module /api/ppdm/modules and /api/setup/wizard controllers, worker,
coordinator and global seeding service are removed. Their former Web setup routes
now render the canonical Module Databases page. That page exposes explicit seeding
for one saved binding, requires confirmation, rechecks its version, and calls the
typed client without an actor or connection override. The API derives the actor
from the authenticated local account and returns a shared ModuleSeedSummary.
Tests verify retired routes are absent, canonical seeding rejects unauthenticated
IDs, and the client sends only the selected module and binding version. Other
older PPDM setup/import endpoints still require review for binding enforcement.

The shared ModuleSetupOrchestrator rejects empty/blank selections, unknown IDs
(including mixed valid/unknown requests), ambiguous registrations, SECURITY, and
repository entity namespaces before running any selected module. Both aggregate
paths treat non-empty errors as failure while retaining partial insertion counts.
PPDM39SetupService no longer falls back to all-reference-data when its selected
module orchestrator is unavailable. Regression tests verify no preceding valid
module runs on invalid selections. The older seed/selected-modules endpoint and
its string-parsed response helpers are removed. The former database-wizard module
screen now renders Module Databases, which requires a saved binding rather than
accepting a target/audit-user override. The canonical seed endpoint also checks
the entity ownership boundary before opening a datasource, so renamed modules
cannot seed Identity or APP_* repository entities. Route and negative-write tests
pass; the replacement Web page builds and its client regression suite passes.

Schema planning now requires explicit module IDs and a configured binding validator
that returns a non-empty target fingerprint. Assembly/namespace scans cannot bypass
these requirements. Approval, direct execution, queueing and the queued worker all
reject cached plans missing module IDs, fingerprints or the reviewed entity list.
Execution registers assemblies from that retained entity list rather than scanning
the default core namespace. Tests cover incomplete cached plans and unbound planning;
the full API migration workflow is still unverified live. Older assembly-scoped planning screens must use
Module Databases instead; they cannot generate an unbound executable plan.

Migration manager creation now compares the selected ConfigEditor connection with
the cached datasource's ConnectionProp before calling Openconnection. Missing,
ambiguous or mismatched targets fail without closing/replacing the datasource.
The same target fingerprint is used by the API binding digest. Tests cover changes
to host, database, schema, connection string, credentials and driver, plus missing
active settings and harmless display preferences. This verifies configuration
comparison, not the physical target of an already-open driver connection. In-place
mutation of shared connection objects and non-migration datasource consumers still
need lifecycle/target verification with live drivers.

An opt-in LocalDbDriverTests probe now constructs the API's shipped SQLServerDataSource
with its BeepDM connection driver configuration and a Microsoft.Data.SqlClient
connection factory. It opens (localdb)\MSSQLLocalDB, validates the configured target,
and reads DB_NAME() as master through the driver's underlying connection. This
read-only probe passed without creating or changing schema/data. Editor/config
dependencies are mocked, so this is driver connectivity evidence, not full host
initialization, module DDL/seeding, or two-database isolation evidence. Enable with
OILGAS_TEST_LOCALDB_DRIVER=1 on Windows; otherwise the probe is skipped.

The opt-in `OILGAS_TEST_LOCALDB_MODULE=1` test now runs BeepDM's explicit-type
schema creation against a fresh LocalDB database and independently queries
`sys.tables`. It passed with exactly OIL_COMPOSITION and OIL_PROPERTY_RESULT,
and no Identity tables. Both models now declare required, bounded primary keys;
an offline regression verifies BeepDM recognizes that metadata. The fixture uses
real class/type helpers and built-in SQL Server mappings with mocked editor/config
registration. It does not verify full API approval/execution, host registration,
column fidelity, seeding, replay, or two-database isolation. Diagnostic attempts
and the successful test retain their `BeepOilGas_Module_*` databases; none were
deleted. Incomplete helper configuration exposed a driver success report despite
SQL rejection, so installation must not be considered verified by flags alone.
Logs: `TestResults/localdb-module-installation.log` and
`TestResults/oil-module-api-regression.log`.

Migration execution now verifies fresh BeepDM table metadata against the reviewed
entity manifest before reporting success. Direct execution, queued execution and
completed-checkpoint reads use the same check. It creates a new metadata request
per table instead of reusing cached pre-execution fields, and rejects missing
tables/columns or missing/nullable declared primary keys. Seven regression cases
and the earlier table-only live LocalDB module test passed. This checks presence and declared keys,
not full type/size/index/foreign-key fidelity. The complete API approval-to-apply
workflow still needs live verification. Evidence:
`TestResults/module-schema-verification.log` and
`TestResults/schema-verification-api-regression.log`.

Canonical module seeding now resolves the selected datasource, compares its cached
connection properties to the configured target, and verifies the module schema
before invoking its seed implementation. Target mismatch and missing/unreadable
schema return conflict without seed writes; unavailable/closed datasources return
503. The successful path retains the saved binding and authenticated local actor.
Four additional negative cases pass; all 30 focused seeding tests pass. This does
not prove live seed persistence or protect against every concurrent/in-place
connection mutation. Evidence: `TestResults/module-seed-target-schema.log` and
`TestResults/module-seed-api-regression.log`.

The live module test now also inserts a composition through BeepDM and independently
reads its ID/name. This expanded test passes with the combined local engine and
relational-driver sources. Package mode fails runtime entity
compilation with missing Key/Required/MaxLength attributes. Local BeepDM source
fixes compilation, but the shipped RDBDataSource clears the caller's GUID after a
successful insert; the stored row retains the original GUID. The supplied driver
source has an auto-increment guard for this defect. Its relative BeepDM project
paths resolve correctly in the sibling checkout layout (the earlier path diagnosis
was incorrect). Directory.Build.targets now uses Engine, Models, RDBDataSource and
SqlServerDataSourceCore source projects by default and suppresses dependency package
generation. Both sibling checkouts are required; there is no silent package fallback.
`UseBeepDMSource=false` is only a package-release evaluation path until its runtime
defects are fixed. A reproducible, pinned release dependency set remains outstanding.
ModelEntityBase REMARK/SOURCE are now nullable, matching their optional meaning;
otherwise source-mode migrations made them required and ordinary inserts failed.
The metadata regression verifies their nullability. No existing database columns
were altered; all diagnostic module databases remain retained.
Evidence: `TestResults/localdb-module-persistence.log`,
`TestResults/localdb-module-persistence-source.log`, and
`TestResults/source-persistence-api-regression.log`. Combined-source persistence
evidence: `TestResults/localdb-source-driver-persistence.log`.

Live API startup was exercised on loopback with Development configuration and the
real LocalDB repository. `/health/repository` returned 503/BootstrapRequired and
module setup was blocked by the readiness gate. No registration was performed.
The first run exposed SlaMonitorService querying hardcoded PPDM39 before setup.
It now waits for repository Ready plus a valid installed LIFECYCLE binding, uses
the resolved connection for reads, escalation and history, and scopes its in-memory
breach keys by connection. A missing background resolver cannot fall back to PPDM39.
The restart no longer queried PROCESS_STEP_INSTANCE during bootstrap. The test
host was stopped and loopback port 5087 was verified free. Six new startup-gate
tests pass; live post-bootstrap SLA execution and other workflow services still
need validation. Evidence: `TestResults/api-startup-localdb.log`,
`TestResults/workflow-background-startup.log`, and
`TestResults/startup-gate-api-regression.log`.

Verification: all 690 API tests pass with the default combined-source build and both
LocalDB probes enabled, including module creation/persistence, with no skips.
Evidence: `TestResults/default-source-live-api-regression.log`.
(Retired wizard-worker tests removed; canonical user, role and routing
tests added), 27 repository tests (including both live LocalDB tests), and the
current 44-test Web suite passes with the default source dependency stack. Legacy scope/elevation
services and existing APP_* data still require explicit reconciliation; this does
not claim that all legacy authorization workflows have been migrated.
AuthenticationController and its compatibility responses are removed. The Web app
uses its OIDC /authentication routes; /api/setup/repository/register only registers
an externally authenticated account. Live browser sign-in/sign-out is unverified.
The Web role bridge's request cache is keyed by issuer, subject, authentication
type and access token. Changed identity/token values cannot reuse another resolved
principal; missing subject/token values deny access even after an earlier success
in the same request. Five new regression cases pass alongside the existing
same-request reuse and next-request refresh tests. Evidence:
`TestResults/web-identity-cache.log`. This does not verify live OIDC callbacks.

OilGas now registers its own RevalidatingServerAuthenticationStateProvider. Every
minute it resolves the captured subject's token and queries repository access via
the API, without reading a later HttpContext. Disabled accounts, mismatched local
IDs, changed role/permission sets, missing tokens and lookup failures invalidate
the circuit's authentication. The framework publishes anonymous state; existing
UserCircuitHandler subscribers update their captured user. This does not revoke
the OIDC cookie or token globally. A reload obtains fresh request-level claims;
API endpoint authorization remains independently enforced throughout the interval.
Nine new tests include the actual framework revalidation loop (shortened interval
in the test), verifying anonymous state after revocation. All 44 Web tests pass.
Evidence: `TestResults/circuit-access-revalidation.log`. Live browser/circuit
verification with the real IdentityServer is still outstanding.

UserManagementController and RepositoryUserService now use shared
RepositoryUserSummary/RepositoryUserUpdate contracts, not USER or IUserService.
No password, lockout or email-change inputs are exposed. Profile updates require
the current Identity concurrency stamp and retain tenant/business-associate data.
Only administrators can change active status; owners may update their own name.
Last-administrator protection remains transactional. Tests cover stale/missing
versions, unchanged verified email, metadata audit actor and denied owner/status
updates. GET /api/identity/roles supplies shared RepositoryRoleSummary values from
Identity, with optional APP_ROLE descriptions and no writes during catalog reads.
The user-role screen consumes both canonical catalogs through its typed client.
Role-assignment and permission endpoints now use RepositoryUserRole,
RepositoryRolePermission, RepositoryRoleDetails and RepositoryPermission contracts
from TheTechIdea.Data. The API no longer registers IRoleAssignmentService or maps
to the old UserManagement AppRole models. The unused IdentityServiceClient and
its interface were removed. Grant/revoke requests cannot supply their audit actor;
all four controller mutations reject a missing local actor before accessing storage.
Validation and grant conflicts return 400/409. Membership and permission history
behavior, including Identity rows without extensions and exact-claim revocation,
remains covered by the repository RBAC regression suite.

Web startup now reads the anonymous `/health/repository` endpoint through the
typed RepositoryAccountClient. `/setup/repository` distinguishes unavailable,
migration-required, administrator-registration-required and ready states. Login
and authentication callbacks remain accessible during bootstrap. Unknown payloads,
malformed JSON, transport failures and inconsistent HTTP/status combinations fail
closed as unavailable. Tests cover these response contracts.
Readiness reports RecoveryRequired when users exist without the bootstrap marker,
or when a marker exists but there is no active Administrator membership. Startup
does not offer first-administrator registration in these states and never repairs
them by granting access automatically. Operator investigation is required. Tests
verify both conditions and that readiness leaves membership/bootstrap rows alone.
The old FirstRunService, SetupGate and ConnectionCheck components were removed:
neither browser-local PPDM completion flags nor a global business connection decide
whether repository administration is available. Module connections are configured
separately. The router uses AuthorizeRouteView to enforce page-level authorization;
unauthenticated visitors go to login and authenticated unauthorized visitors see
access denied. The Web build passes, but these routing and setup flows still need
live browser/OIDC verification.

Tests generate ordinary and idempotent SQL for all three providers and verify
snapshot consistency without connecting to a database. They do not establish
that a live database has been installed or that API authorization is wired up.

### HTTP Module Plan Serialization (2026-09-12)

The first planning request in the LocalDB module integration test now passes through
the real ModuleRepositoryController route and ASP.NET MVC JSON formatter over
loopback HTTP. The deserialized response must contain reviewed plan/manifest hashes
and dry-run operations; that response then drives the existing approval, execution,
rebinding and SQL isolation assertions. The second target still uses direct service
planning. The temporary HTTP host is stopped and disposed after the plan response.

This test supplies a test-only local administrator principal to isolate MVC request
binding and response formatting. Authentication and repository role transformation
are covered separately; this is not a live OIDC test or full production-host test.
All 704 API tests pass, including LocalDB integration checks. Evidence:
`TestResults/http-module-plan-regression.log`.

### LocalDB HTTP Registration and Validation (2026-09-12)

A real loopback HTTP test now uses the production registration, account, user,
role and persona controllers with EF-migrated SQL LocalDB Identity stores. It
verifies anonymous rejection, first-user Administrator assignment, ordinary second
registration, administrator-only user listing, local actor resolution, deactivation,
last-administrator protection, replay and rejection of disabled accounts. Forged
registration fields do not create a different external login. Test authentication
is isolated to the test host; JWT validation and browser OIDC are not simulated as
production successes. The test database is retained and the host is stopped.

This exposed an actual HTTP 500: positional request records attached DataAnnotations
to generated properties rather than constructor parameters, which ASP.NET MVC
rejects during model validation. RepositoryUserUpdate, RepositoryRoleRequest and
the three persona update records now put validation metadata on their parameters.
Persona service validation reads those same parameter attributes for non-MVC
callers. No entity mapping or migration changed.

HTTP tests cover successful role/persona writes and 400 responses for empty or
oversized values. Direct persona-service tests preserve the same validation rules.
All 704 API tests and 28 repository tests pass with LocalDB checks enabled.
Evidence: `TestResults/http-registration-api-regression.log` and
`TestResults/record-validation-repository-tests.log`.

### Default Development Launch (2026-09-12)

Both projects now list the HTTPS launch profile first. Previously plain dotnet run
selected HTTP-only while the Web development configuration called the HTTPS API.
Default launches now align with the configured endpoints:

```powershell
dotnet run --project Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj
dotnet run --project Beep.OilandGas.Web/Beep.OilandGas.Web.csproj
```

Run these in separate terminals. The API listens at https://localhost:7001 and
the Web app at https://localhost:7066. Explicit --launch-profile https is also
supported. The HTTP-only profiles remain available for deliberate diagnostics.
Local development continues to use (localdb)\MSSQLLocalDB / BeepOilGasRepository.

Actual default-profile launches were verified without URL overrides or certificate
validation bypasses: API readiness returned 503/BootstrapRequired, and the Web
setup endpoint returned 200 with administrator registration offered. Test hosts
were stopped. IdentityServer on port 7062 remained unavailable, so OIDC registration
was not attempted. Logs: `TestResults/default-profile-api-startup.log` and
`TestResults/default-profile-web-startup.log`. All 47 Web tests pass, including
default-profile/configuration alignment (`TestResults/development-launch-web-tests.log`).

### Inactive User Role Management (2026-09-12)

The user-management role listing now returns stored AspNetUserRoles assignments
for inactive accounts as well as active ones. Previously the management endpoint
returned an empty list for an inactive user, hiding memberships that would become
effective again on reactivation and preventing removal from the role screen.
RepositoryAccessService remains the authorization source and still returns no
roles or permissions for inactive users. The owner/Administrator endpoint guards
are unchanged.

A regression test deactivates an assigned user, verifies its role remains visible
for administration but absent from authorization, and removes that membership.
All 703 API tests pass with LocalDB integration checks enabled. Evidence:
`TestResults/inactive-user-rbac-regression.log`.

### Migration HTTP Authorization (2026-09-12)

SchemaMigrationHttpAuthorizationTests starts isolated Kestrel hosts on ephemeral
loopback ports and sends HTTP approval/execution requests to the real setup
controller. It uses RepositoryClaimsTransformation and RepositoryAuthorization
with a test-only external authentication scheme and mocked repository lookup and
migration service. No test authentication code is registered in production.

Anonymous requests return 401. Members, unregistered users, disabled accounts and
repository lookup failures return 403 without reaching migration code, even when
the external identity contains a forged Administrator role and local identifier.
An authoritative repository administrator reaches both actions; client-supplied
audit actors are replaced with the repository user ID. Revoking the repository
role between requests causes the next execution request to return 403 without an
additional migration call. Each test host is stopped and disposed.

All 702 API tests pass, including these six HTTP cases and the LocalDB probes.
Evidence: `TestResults/schema-http-api-regression.log`. This verifies the endpoint
authorization pipeline, not JWT signature validation, a live OIDC callback, the
entire production host configuration, or migration execution over HTTP.

### Canonical LocalDB Migration Flow (2026-09-12)

The live two-target test now saves bindings through ModuleRepositoryController,
resolves them with ModuleConnectionResolver, generates plans through the controller
and PPDM39SetupService, approves them, and executes using the reviewed plan and
manifest hashes. Both LocalDB targets use this path, replacing the earlier direct
MigrationManager calls in this test.

Assertions verify that planning and unapproved execution leave the first database
empty, incorrect plan hashes are rejected, rebinding invalidates the old approved
plan, the resolver returns the new target, and both installed databases contain
exactly the selected module tables with isolated composition rows.

The binding store is now a third, isolated SQL LocalDB database created with all
four repository EF migrations. Reopening that repository verifies the persisted
binding, the Identity tables, and the absence of module tables. Module installation
does not create users. Both module targets also use real SQL Server LocalDB drivers.
This is an in-process controller/service test,
not a test of HTTP authorization middleware, OIDC, or an out-of-process worker.
All 696 API tests pass with both LocalDB probes enabled. Evidence:
`TestResults/localdb-canonical-migration.log` (initial canonical path) and
`TestResults/canonical-migration-api-regression.log` (rebinding assertions).
The all-LocalDB repository/module run passes all 702 API tests; current evidence
is `TestResults/localdb-repository-module-regression.log`.
All generated LocalDB test databases are retained.

Local development and local integration verification use SQL Server LocalDB.
Oracle/PostgreSQL migration previews remain available for deployment review; those
providers are not local development prerequisites.

### Unbound Schema API Removed (2026-09-12)

Review found a remaining PPDM39SchemaController exposing install, install-all,
reference seeding and tracking-reset operations under only authenticated-user
authorization. It accepted arbitrary named connections without saved module
bindings or approval and used broad assembly/prefix discovery; its seeder accepted
a caller-supplied audit user. The controller and its private response models were
removed, not retained as a compatibility API.

The associated SchemaModuleManager, ReferenceValuesManager and old schema page
were removed. `/ppdm39/data-management/schema` now resolves to the existing
Administrator-only ModuleDatabases page, using saved bindings, explicit manifests,
reviewed plans and the canonical seed action. Existing tracking records and all
database tables/data were left intact.

Regression tests verify that the removed controller route is no longer exposed,
the canonical module controller requires Administrator, and schema management
resolves to the guarded canonical page. All 696 API tests (including LocalDB)
and 46 Web tests pass. Evidence: `TestResults/schema-boundary-api-regression.log`
and `TestResults/schema-boundary-web-regression.log`. Full live HTTP
binding/plan/approval/execution verification is still pending.

### Reopened Repository Verification (2026-09-12)

All 27 repository tests were rerun with OILGAS_TEST_LOCALDB=1. The live installation
test now builds a second service provider against the migrated test database and
independently reads all seven standard AspNet tables. It verifies persisted first
administrator identity, an initially unassigned second user, a new Identity role
and permission claim, membership revocation across scopes, and an inactive user's
empty access result even when membership remains stored. Bootstrap replay for that
inactive user is rejected. The separate race test still proves exactly one first
administrator. Evidence: `TestResults/repository-reopened-rbac.log`.

These tests use fresh retained databases, not the configured development repository.
They exercise real SQL Server stores but do not exercise the OIDC browser callback.

Install-Repository.ps1 was also executed in Script mode for each provider. It
successfully built and generated idempotent previews, checked for the seven Identity
tables and ModuleDatabaseBindings:

- `TestResults/repository-install-SqlServer-b956b105a425404791f498d219c9e043.sql`
- `TestResults/repository-install-PostgreSql-b956b105a425404791f498d219c9e043.sql`
- `TestResults/repository-install-Oracle-b956b105a425404791f498d219c9e043.sql`

Preview generation does not connect to those databases. Live Oracle/PostgreSQL
installation and provider-specific execution remain unverified.

### Compliance Identity Reporting (2026-09-12)

ComplianceReportService no longer queries legacy USER, ROLE, PERMISSION or
ROLE_PERMISSION tables. Its API-owned ComplianceIdentityReader reads AspNetUsers,
AspNetRoles, AspNetUserRoles and permission-type AspNetRoleClaims from the default
EF repository. It includes unassigned users, deduplicates permissions and does not
require APP_* extension rows. User listings no longer silently truncate at 500.
This reports stored assignments; it is not an authorization decision or effective
access evaluation for inactive users.

Workflow portions of reports resolve the saved LIFECYCLE binding once per report
operation. There is no global PPDM39 connection fallback. Matrix generation needs
only the Identity repository, not an installed workflow database. Report exceptions
now propagate instead of being converted into empty successful reports.

Four new tests verify Identity-backed reporting, matrix delegation, failure
propagation and missing-binding rejection. All 694 API tests pass with both
LocalDB probes enabled. Evidence: `TestResults/compliance-identity-tests.log` and
`TestResults/compliance-api-regression.log`. Full live compliance report execution,
unimplemented report metrics and other workflow services' global routing still
need review; this is not a certification of the broader compliance feature.

### Named Database Isolation (2026-09-12)

The live OilProperties migration test now uses two fresh, retained LocalDB
databases under one BeepDM editor, with both datasources open simultaneously.
The unselected connection is first in configuration. Installing into the selected
target leaves the other database empty. Each target is then installed separately;
independent SQL queries check that each contains exactly the module's two tables.
Distinct composition records inserted through the two named datasources remain
isolated, including rechecking the original target after writing to the second.

This proves the tested SQL Server driver/explicit-manifest migration path, not
the full HTTP binding/plan/approval execution flow, repository installation in
those test databases, concurrent rebinding, or Oracle/PostgreSQL behavior.
Evidence: `TestResults/localdb-named-target-isolation.log` and
`TestResults/named-target-api-regression.log`. No test databases are deleted.

### Live Web Setup Verification (2026-09-12)

The Development API and Web app were run on loopback against the default LocalDB
repository. The browser rendered BootstrapRequired as first-administrator
registration pending, and Refresh returned the same status. No user was created.
The configured IdentityServer was not running, so OIDC registration, callback and
authenticated administration remain unverified.

The initial Web launch failed because its direct OpenIdConnect 10.0.9 reference
was older than the shared authentication library's 10.0.10 requirement. The Web
reference now matches 10.0.10, with a runtime assembly-load regression test.
All 45 Web authentication tests pass; evidence is in
`TestResults/setup-layout-tests.log`. Existing build warnings remain.

The browser check also found an unbounded header logo covering setup controls.
Layout sizing now lives in the loaded `wwwroot/css/app.css`, and App.razor uses
ASP.NET static-asset URLs with MapStaticAssets so updated assets are versioned.
The corrected desktop layout and Refresh action were checked in the browser.
The browser viewport override did not take effect, so mobile layout is not yet
verified. Test hosts were stopped after verification; test databases were retained.

Reference: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers
