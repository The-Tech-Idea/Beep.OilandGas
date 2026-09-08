# OilGas Default Repository

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
The Web OIDC callback now calls registration using the access token. Its claims
transformation calls the me endpoint and replaces external roles with application
roles; failures deny access. Five isolated Web auth tests pass. Full Web build and
interactive sign-in verification remain blocked by existing Razor/model errors.

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
Verification: 598 API tests (retired-route tests removed; canonical user, role and routing
tests added), 27 repository tests (including both live LocalDB tests), and the
current 26-test Web suite passes. Legacy scope/elevation
services and existing APP_* data still require explicit reconciliation; this does
not claim that all legacy authorization workflows have been migrated.
AuthenticationController and its compatibility responses are removed. The Web app
uses its OIDC /authentication routes; /api/setup/repository/register only registers
an externally authenticated account. Live browser sign-in/sign-out is unverified.
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

Reference: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers
