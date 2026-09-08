# Startup, Admin, and Default Repository Review

Reviewed 2026-09-05. Source review, not a live deployment audit.

## Findings in the Current API and Web

1. **P1: Role mutations require authentication but not administration.**
   `ApiService/Controllers/Identity/RoleAssignmentController.cs:10,75` permits
   any authenticated caller to assign/revoke roles and grant permissions.
   The service treats the actor as audit data, not an authorization decision.
   Program.cs:1183 also constructs the service without its optional SoD detector.
2. **P1: Two disconnected RBAC stores.** DefaultSecuritySeedService and
   UserManagementService use USER, ROLE, USER_ROLE, PERMISSION, ROLE_PERMISSION.
   RoleAssignmentService.cs:76,124,170 uses APP_USER_ROLE, APP_ROLE_PERMISSION,
   APP_ROLE, APP_PERMISSION. Changes through one path need not affect the other.
3. **P1: No OilGas first-human-admin flow.** DefaultSecuritySeedService.cs:313
   creates SYSTEM with PASSWORD_HASH=seeded-no-login and assigns Administrator.
   The initiating actor is not granted that role. SYSTEM is not an OIDC account.
4. **P1: Incomplete user-management composition.** AddUserManagement and
   AddPermissionPolicy exist in UserManagementServiceCollectionExtensions but
   have no callers in this repository. IUserService/IAuthService dependencies and
   named policies such as Admin.ManageUsers are not wired by the API composition.
5. **P1: External sign-in is not bridged to local roles.** Web Program.cs:183,190
   requests roles and trusts the external role claim; no application
   IClaimsTransformation bridge is registered. This conflicts with AGENTS.md.
6. **P1: Security seeding ignores the selected database.** SecurityModule.cs:75
   accepts connectionName but calls a seeder bound to the global connection in
   API Program.cs:1195. In the new design security must deliberately stay in the
   repository, and must no longer be offered as a module seed on arbitrary DBs.
7. **P2: Setup gating uses the wrong readiness boundary.** SetupGateMiddleware
   tests only the existence of the literal PPDM39 connection and does not exempt
   /api/ppdm39/setup. It can block setup itself, and cannot represent readiness of
   a default repository plus independently installed modules.
8. **P2: Legacy token issuance does not match API validation.** AuthService.cs:487
   issues locally signed HMAC tokens with OilGas defaults. API Program.cs:78
   validates external authority tokens for beep-api. Do not enable this second
   login path as part of the repository transition.

## How Startup Works Today

1. API configures external JWT validation and Beep services, captures the configured
   PPDM39 connection name, and registers domain services and setup modules.
2. It initializes Beep configuration, assemblies, and drivers. Initialization
   failures are fatal. Process definitions are seeded as SYSTEM in a nonfatal
   startup block at Program.cs:2744. Registering setup modules does not seed them.
3. SetupGate runs before authentication. Authentication, asset middleware,
   authorization, and authenticated controller mapping follow.
4. Security seeding is invoked explicitly through database/setup flows, not a
   first-admin startup transaction. It creates roles, permissions, SYSTEM,
   personas, organization scope and SYSTEM access records.
5. Web uses cookies and OIDC code/PKCE with shared identity services. There is no
   local source-proven flow that provisions a human OilGas user and authoritative
   app roles on external sign-in.

No Aspire AppHost or DistributedApplication entry point was found in this repo;
service-discovery configuration comments alone do not establish orchestration.

## IdentityServer Is Separate

The sibling Beep.IdentityServer contains its own EF migrations. Its
Services/Security/AdminBootstrap.cs:88 assigns the first registering account the
server OperatorRoles.Administrator role using a singleton claim row. Register.razor
and external provisioning call it. Startup SealAsync marks servers with existing
users as already claimed. No configured default password is seeded there.

That grants administration of the identity server, NOT OilGas business access.
A publicly exposed unclaimed identity server can be claimed by the first arrival;
provision it under operator-controlled access. A failed role grant leaves its claim
closed and requires operator recovery. No live user or claim rows were inspected.

## Accepted Target Architecture

- One default repository selected at installation: SQL Server, PostgreSQL, or Oracle.
- EF owns only that repository's installation/versioned schema, standard AspNet*
  users and roles, app permissions, bootstrap state, and module connection bindings.
- IdentityServer owns credentials and authentication. OilGas links validated issuer
  and subject and owns its application roles. Email is not an identity key.
- ApiService is the only application data gateway. Web calls typed API clients.
- BeepDM ConfigEditor continues to own persisted named connection definitions.
- ModuleDatabaseBindings refers to those names, never passwords/connection strings.
- Module setup uses BeepDM MigrationManager with explicit module entities, selected
  datasource, plan/preflight/approval and execution checks. EF never creates PPDM tables.
- A module database can be shared by modules or separate. A request's connection
  selection cannot change the default repository or authorize access by itself.

## Delivery and Acceptance Gates

### 1. Repository Schema Foundation (Implemented, LocalDB Validated)

Added TheTechIdea.Data for the new shared entities and Beep.OilandGas.Repository
for server-side IdentityDbContext and three provider-specific migration sets.
Standard seven AspNet tables, RepositoryBootstrap, and ModuleDatabaseBindings are
included. No API cutover or existing data migration is performed by this addition.

Acceptance: clean build; provider SQL generation; snapshots match; fresh install,
second apply and upgrade on real SQL Server/PostgreSQL/Oracle instances. Offline
SQL tests do not replace the last three live checks.

### 2. Installation and First Admin

Add validated Repository provider/connection configuration independent of PPDM39.
Provide explicit installation/migration execution; do not auto-upgrade production
on every API start. Readiness must distinguish missing repository, unavailable
repository, pending schema, and admin bootstrap required.

User clarification: the first user registering in OilGas is always Administrator.
Keep a fresh deployment private until the intended user registers. Create the local
account, external link, Administrator role, membership and singleton completion
record atomically. A duplicate request cannot reopen setup; failures roll back.
Keep an audited operator recovery path. Never inherit IdentityServer roles.

Implemented POST /api/setup/repository/register (bootstrap alias retained): validated
JWT issuer/subject registers a passwordless local user; the first local registration
gets Administrator and a singleton completion marker in one transaction. Subsequent
registrations get no automatic roles. Replays do not duplicate rows; existing users
without a marker fail closed pending reconciliation. This API still needs a Web
registration caller and the local-role claims bridge before end-to-end use.

Acceptance: empty repository, existing repository, two concurrent bootstrap claims,
failed transaction, disabled account, wrong external issuer, replay and restart.

### 3. User Management and Role Enforcement

Latest user-management progress: IUserService now resolves to RepositoryUserService.
Lists, lookups, profile metadata, role operations and soft deactivation use the
repository. APP_USER extends AspNetUsers for full name, tenant/BA references and
change metadata. UserExtension migrations exist for all three providers and were
applied to development LocalDB. Legacy local-password creation returns 410;
credentials remain in IdentityServer. Activation changes are admin-only; reads of
other users require admin. The last administrator cannot be disabled. Four focused
repository user/role tests and nineteen migration/bootstrap/access tests pass.
UI audit correction: no concrete create-user page was found in the current Web
source. The existing UserRoles page was instead a placeholder: it displayed success
without calling an assignment/removal API. It now has an Administrator guard,
repository-backed user/role selectors and real mutation requests through a typed
UserAdministrationClient, with busy/error handling. Seven Web authentication/client
tests pass against the actual Web project (linked-source workaround removed).

Progress: API RepositoryClaimsTransformation strips external role/permission
claims, resolves local repository membership, and denies disabled users and lookup
failures. It uses per-scope object caching, not a caller-supplied marker to bypass
lookups. JWT inbound mapping is disabled to preserve validated issuer and subject.
GET /api/auth/repository/me exposes current authoritative local access read-only.
RoleAssignmentController is now Administrator-only; its legacy storage remains.
Five focused API claims tests and nineteen repository tests pass. API builds with
zero errors. Web transformation, registration caller and CRUD cutover remain open.

Web bridge progress: OnTokenValidated now calls the repository registration API
with the explicit bearer token and fails sign-in if registration fails. The Web
IClaimsTransformation calls the read-only me endpoint, strips external/stale roles,
and adds standard role claims. It caches only within the current HTTP request;
cookie validation restores the saved API token into request state. The OIDC roles
scope is removed. Five linked-production-source Web auth tests pass (API role
replacement, per-request lookup, fail-closed responses, registration POST).
Full Web compilation still fails on the same five pre-existing Razor/model errors;
these isolated tests do not establish successful interactive sign-in or revocation
of roles inside an already-connected Blazor circuit. Those runtime checks remain.

Move active user/role CRUD to UserManager/RoleManager over the repository. Use
AspNetRoleClaims for application permission codes rather than a third role store.
Expose a read-only current-user roles API with independent API authorization.
Implement Web IClaimsTransformation, marker per request and fail-closed resolution;
pages retain standard Authorize/Roles and AuthorizeView. API independently resolves
local roles for validated tokens. Protect role mutations and ownership boundaries.

Acceptance: anonymous 401, ordinary user 403 on admin operations, admin permitted,
external admin claim alone rejected, revoked/disabled user denied, role resolution
outage denied, no DB dependency in Web, no local password/JWT fallback.

### 4. Module Routing and BeepDM Installation

Startup gate updated: SetupGateMiddleware now checks repository readiness rather
than requiring a connection named PPDM39. Authentication, repository registration,
health and discovery remain reachable for bootstrap. Once the repository is ready,
account administration and module setup can run without a PPDM39 datasource.
Both /health and /health/repository report repository state with 503 for incomplete
installation. Ten focused gate tests pass and the API builds. Module connectivity
and installation state must be checked by the module workflow, not this global gate.

Progress: administrator-only /api/setup/modules exposes module bindings. PUT
/{moduleId}/connection stores an existing BeepDM connection name with optimistic
concurrency in ModuleDatabaseBindings. POST /{moduleId}/plan resolves that binding
and sends only that module's explicit entity types through the existing BeepDM
plan pipeline. SchemaMigrationPlanRequest.ModuleIds supports explicit selection;
unknown, empty, SECURITY, and mixed assembly/module scopes fail rather than
falling back to all entities. Five scope tests pass and the API builds. This does
not yet migrate the old all-module wizard, domain-service connection resolution,
or the binding UI; connection routing across those existing paths remains open.

Replace fixed global PPDM39 assumptions with authorized module binding resolution.
Keep a default module binding only as an explicit setting, not an implicit fallback
that silently writes another database. Remove repository security types from
arbitrary module schema/seed lists. Update wizard readiness and job execution to
carry the selected connection throughout, including seeds and audit results.

Acceptance: two named DBs, concurrent requests, correct table/seed destination,
invalid connection, denied connection, missing provider capability, cancellation,
failed migration, and no repository schema in a module database.

### 5. Existing Installation Transition

User clarification: retain APP_* as extensions of standard Identity, not a second
role system. Implemented APP_ROLE metadata linked to AspNetRoles; APP_PERMISSION
metadata; APP_USER_ROLE assignment history; APP_ROLE_PERMISSION claim-linked grant
history. The repository adapter now updates Identity and extension records in one
transaction, preserves history on revocation, and protects the last administrator.
Catalog responses preserve extension metadata. Two focused adapter tests pass,
nineteen repository tests pass, and IdentityExtensions was applied to LocalDB.
Three-provider migration sets are updated. Legacy data import and scope/SoD
enforcement cutover remain outstanding; no old database records were deleted.

Inventory USER/APP_* records and compare role memberships before cutover. Produce
an explicit mapping/report for external subjects and duplicate users; never merge
on email alone. Back up source data, import under review, reconcile counts and
effective permissions, then disable legacy writers. Preserve audit records and
rollback instructions. Do not delete old tables in the initial installation migration.

## Current Verification Limits

Latest regression run: all 415 API tests passed
(TestResults/repository-api-regression.log); all 7 Web auth/client tests passed
against the Web project (TestResults/user-admin-web-tests.log). Hosted sign-in,
module setup on multiple live providers and production cutover remain unverified.

Latest Web verification: full Web build now succeeds with zero errors and 325
warnings (TestResults/web-startup-build.log). Fixed obsolete account model/API use,
verified email/phone change flows, Razor text/type placement, chip/select literals,
menu origin enums, callback types and Studio extension-method resolution. Earlier
five-error references above are historical. A background development-host launch
was rejected by the execution policy; no listening host or interactive sign-in is
claimed. Web authentication source tests remain separate from hosted verification.

The migration was applied to (localdb)\MSSQLLocalDB / BeepOilGasRepository and
the ten resulting tables (including EF history) were read from sys.tables. A second
apply reported no pending migrations. Six offline SQL/snapshot tests pass across
all three providers. Oracle and PostgreSQL live installation tests remain pending.
Development settings declare SqlServer and the LocalDB repository connection.
API now consumes these through AddOilGasRepository, registering scoped provider
contexts and Identity user/role stores without changing bearer authentication.
GET /health/repository reports database/migration/bootstrap readiness; registration
does not automatically apply schema or bootstrap users. Twelve repository tests
pass, including provider selection and rejection of missing/unsupported settings.
API builds with zero errors (56 existing warnings). The role-resolution and
bootstrap workflows still need implementation; store registration alone does
not switch the legacy endpoints to AspNet tables.
Web currently has five compile errors
outside the last MudBlazor fix, so no successful full-app startup is claimed.
The API's legacy authorization behavior has not yet been replaced. Remaining
delivery phases above are required before the full objective is complete.
