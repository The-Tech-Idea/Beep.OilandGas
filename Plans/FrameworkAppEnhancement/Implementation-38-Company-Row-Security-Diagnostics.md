# Implementation 38: company row-security diagnostics

Date: 2026-10-05. Owners: CA-03/05/06 and FG-01/11/12/20. Extends Implementations 35–37. Status: focused service/persistence/SDK/direct-controller/middleware/client verification; full HTTP/auth-owner/Program DI and live providers remain open.

## Source finding and resulting behavior

The active API did not register AddUserManagement or its JWT field-scope handler. It did expose RowLevelSecurityController, whose former service depended on separate PPDM USER/USER_SCOPE_ASSIGNMENT tables and generated SQL filter expressions. That service was not registered by the active Program. The shared routing classifier treated every identity route as repository control-plane traffic, so the security routes also bypassed company selection. These findings do not establish that the dormant JWT handler had been reachable in the active host.

The existing routes now consume CompanyRowLevelSecurityService, registered by ApiService. It uses the existing resolved active-account gate, CompanyModuleConnection, RepositoryAssetAccessStore, CompanyPPDMRowAccessPolicy and PPDM39GenericDataService. No new scope table, grant store, claim parser or datasource reader is introduced. Both the controller and service independently require the current resolved actor; the service restricts these diagnostics to self and requires that actor to match the validated selected company.

The shared CompanyRequestPolicy now classifies the exact api/identity/security subtree as company business traffic. Other identity/control-plane routes retain their existing classification, with segment boundaries preventing similarly named routes from changing category. Actual company middleware refuses missing selectors, validates membership and installs scoped standard roles. It also removes obsolete field_scope claims. The existing client capture uses the same classifier and requires/captures company headers for these routes.

## Canonical row and filter path

Check-row-access accepts a canonical table and JSON containing every primary-key column. It validates the reviewed ownership policy and queries the actual stored row through the existing scoped CRUD service. Caller-supplied field ownership is not evidence: a foreign well cannot become visible by supplying an allowed ASSIGNED_FIELD. Complete composite FACILITY keys are supported through SDK equality filters. Missing, malformed, duplicate-property, numeric, incomplete and oversized keys are refused. Unsupported table ownership remains refused by the canonical policy. No stored visible row returns a false read-access result; it does not disclose whether an inaccessible row exists.

Actual reads retain the repository's current grant revalidation, exact equality postchecks, row-policy validation and primary-key deduplication. Diagnostics do not bypass those checks and do not establish write/delete permission, legal ownership or asset-grant assignment eligibility.

Apply-row-filters returns current Beep read predicates and narrowing AppFilter values. The repository's existing CopyAndValidateFilters method is exposed and reused, so columns/operators and copying follow the same implementation as actual CRUD. Read predicates form a bounded OR; each supplied filter narrows each predicate through AND. Empty predicates explicitly set DenyAll. No SQL expression is generated, consumed or passed to a datasource. The actual CRUD path regenerates current authority; a returned diagnostic plan is not a permission token or an unrestricted empty query.

Scope queries return exact current explicit asset/field grants in the selected company and the selected organization. They do not enumerate inherited assets through an alternate lookup. Inherited row visibility remains on the reviewed PPDM policy. Source/datasource checks compare the selected reviewed core connection; database checks compare its exact canonical fingerprint. A matching binding is reported separately from row access and never grants all rows. Stale account/company authority and binding changes are refused.

## Contract ownership and retirement

IRowLevelSecurityService, its result contracts and five request DTOs move into TheTechIdea.Data, retaining namespaces and removing old definitions. Actor IDs are separate trusted service arguments; request DTOs no longer expose USER_ID. ApplyRowFiltersRequest accepts typed Filters, replacing serialized filter input. RowFilterResult returns ReadPredicates, Filters and DenyAll instead of SQL FilterExpressions. CheckDatabaseAccessRequest uses DatabaseFingerprint instead of a guessed/raw database name. Routes remain the existing security routes; no legacy parsing, forwarding classes or fallback schema reader is retained. No checked-in Web/facade consumer of these request contracts was found; external consumer compatibility remains unverified and is not claimed.

The old PPDM RowLevelSecurityService, FieldAccessService and JWT FieldScopeAuthorizationHandler are removed. Their unused registrations and JWT package references are removed from the historical library. Its obsolete field-scope tests are replaced by focused canonical authority/read tests. The old principal-only row-scope test is replaced by a real gate/store/controller query-selector test. The library README now documents active ownership rather than claiming local JWT/password/MFA functionality that the active app does not implement.

Remaining historical PPDM user/role/persona/hierarchy/elevation/bootstrap services are not registered as active API authority, and SecurityModule remains excluded from setup discovery. Their dependency/domain reuse audit and retirement or canonical integration remain open; this slice does not claim the entire library has been reconciled.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false
```

**413 passed, zero failed/skipped**: previous 377 plus 36 new cases. Tests execute the actual row-security service/controller, current account/company/grant persistence, PPDM policy/generic repository and shared client/server routing, using actual Identity managers/EF stores, SQLite and the checked-in PPDM metadata. A recording SDK datasource verifies query conjunctions and stored-row behavior. The fixture installs the real HttpContextAccessor request in the calling execution context after async creation; it does not replace production account resolution with an auth-owner implementation stub.

Cases cover empty grants despite wildcard/admin claims; exact IDs and company switching; all eight service operations refusing foreign subjects; inactive accounts and lost membership/roles; captured binding changes; forged row ownership versus stored records; composite keys and malformed keys; ignored provider equality filters; SDK predicate copying/column/operator validation; unknown table ownership; reviewed target checks; controller query-selector refusal and current actor checks; route boundaries; actual middleware refusing missing selection and installing current roles while stripping field claims; and actual client request capture requiring company headers. Previous company/PPDM/account/client/hub/queue/migration cases remain green. Actual Data, Models and PPDM projects build in this graph. `git diff --check` passes; missing analyzer warnings remain outside the focused gate.

Program registration, historical-library DI/package changes and wider host tests are source evidence. The focused run does not validate complete HTTP serialization/model-binding/token handling/Program startup, OIDC, browser clients, supported database providers or concurrency with live datasource writes. No full regression or deployment claim is made.

## Remaining work

Continue authoritative asset grant assignment/existence and legal/operator ownership, remaining table/reference/domain policies, domain connection/cache scope and historical library authority retirement. Current read-policy scope remains FIELD/WELL/POOL and field-inherited FACILITY. Physical database identity/provisioning, broader field/domain administration and all company release gates remain open. Verify actual HTTP/DI/provider/client behavior and authority changes racing diagnostics/data access.

Authoritative period close and finance/engineering/operations/regional phases retain their existing owners and gates. Business/module schemas remain on Beep Migration; existing Identity/access persistence uses EF. This slice adds no schema migration, handwritten SQL or live database mutation. No company phase or release is complete.
