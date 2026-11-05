# Implementation 48 — Stored grant review and retirement

Date: 2026-10-05. Owners: CA-03/05/06; FG-01/10/11/12/20. Status: current-company/current-database inventory, retirement evidence and administration consumers implemented with focused verification. Historical ownership resolution and integrated provider/release acceptance remain open.

## Problem and resulting behavior

Effective access lists intentionally exclude inactive assets, accounts without current membership and unqualified historical facility grants. They cannot serve as a complete administration inventory. The existing store could revoke qualified keys but could not review and retire an exact historical record with expected-revision checks and durable command evidence.

The existing [store contract](../../TheTechIdea.Data/OilGas/IUserAssetAccessStore.cs) and [RepositoryAssetAccessStore](../../Beep.OilandGas.ApiService/Services/RepositoryAssetAccessStore.Review.cs) now support stored inventory, retirement and retirement-history reads. [Shared review requests/pages/evidence](../../TheTechIdea.Data/OilGas/AssetGrantReview.cs) belong to TheTechIdea.Data and reuse AppUserAssetAccess for stored grant snapshots. There is one grant store, one server facade and one administration state; no alternate grant authority or client database access is introduced.

Inventory requires a current selected-company Administrator and the current reviewed PPDM database fingerprint. It includes active/inactive records for the exact requested application account, without requiring subject membership, supported asset metadata, a facility qualifier or live PPDM visibility. An unknown subject returns an explicit empty page. No business datasource is opened or queried for review, retirement or history.

Ownership remains mandatory. OrganizationId must equal the selected company and DatabaseScope must equal its current tenant-sensitive reviewed fingerprint. Unowned rows, foreign records and old database-binding fingerprints are excluded; these records cannot be guessed into the selected company. Their ownership/provenance review remains a separate outstanding task. Headers are compared ordinally before counts and paging, including under case-insensitive provider collations. Full page rows receive the same postchecks. Payloads are bounded to 1–100 rows, default 50; header scans are proportional to the selected subject's stored grant count and need large-inventory provider acceptance.

## Retirement transaction and audit

Retirement addresses the exact stored grant ID and requires its reviewed ConcurrencyStamp plus a nonempty reason of at most 1000 characters without surrounding whitespace/control characters. The server reads current ownership, compares the revision and refuses missing/foreign, stale or already-inactive records. Inactive rows remain reviewable; a new retirement event is not fabricated for them.

The command requires a clean repository context and its own serializable transaction. It sets only IsActive=false, ChangedBy, ChangedUtc and a new ConcurrencyStamp on the grant. A new AssetGrantRetirement persists the original complete grant JSON, previous/new revisions, grant ID, tenant/company/database, reason and server-derived application actor/UTC timestamp. It preserves creation metadata and unknown historical keys, including null facility qualifiers. Grant and receipt save/commit together; current actor/company/binding checks run again before saving. EF compare-and-swap failure becomes a refresh/review conflict, and failed commands detach both queued writes so an unrelated later save cannot apply them.

[RepositoryDbContext](../../Beep.OilandGas.Repository/RepositoryDbContext.cs) refuses modified/deleted retirement evidence through both synchronous and asynchronous SaveChanges. There is no retirement-evidence update/delete endpoint. This is application persistence enforcement, not proof against privileged database tampering or provider-specific concurrency. Later qualified regrant uses normal admission and preserves the earlier retirement snapshot. Retirement history represents these new retirement commands; it does not invent an earlier grant/revoke event ledger.

[AssetAccessMapping](../../Beep.OilandGas.Repository/AssetAccessMapping.cs) preserves the UTC Kind when materializing existing CreatedUtc/ChangedUtc fields and retirement ChangedUtc. Stored ticks are retained; no timezone conversion or historical timestamp rewrite occurs.

## API and SDK surface

The existing [AccessControlController](../../Beep.OilandGas.ApiService/Controllers/AccessControl/AccessControlController.cs) uses standard Administrator route guards and the local authenticated-account resolver. The existing [server facade](../../Beep.OilandGas.LifeCycle/Services/AccessControl/UserAssetAccessService.cs) resolves the reviewed module fingerprint before calling the store; current app-owned company authority is independently enforced there.

| Route | Request | Success |
|---|---|---|
| `GET /api/AccessControl/user/{targetUserId}/stored-grants?offset=0&limit=50` | Exact application subject; nonnegative offset; limit 1–100 | AssetGrantReviewPage: Company, DatabaseScope, UserId, Offset, Limit, Total, Items of AppUserAssetAccess |
| `POST /api/AccessControl/stored-grants/{grantId}/retire` | RetireAssetGrantRequest: ExpectedRevision, Reason | AssetGrantRetirement: Revision, GrantId, TenantId, OrganizationId, DatabaseScope, PreviousRevision, SnapshotJson, Reason, ActorUserId, ChangedUtc |
| `GET /api/AccessControl/stored-grants/{grantId}/retirements?offset=0&limit=50` | Exact grant ID; nonnegative offset; limit 1–100 | AssetGrantRetirementPage: Company, DatabaseScope, GrantId, Offset, Limit, Total, Items of AssetGrantRetirement |

Company selectors use the existing captured-company headers; the client cannot supply a replacement database scope or actor. IDs are encoded route segments. Existing refusal handling supplies 400 for invalid input, 401 for unauthenticated HTTP requests, 403 for missing authority/unavailable owned records and 409 for revision/inactive/binding conflicts. Persistence failures use the existing referenced failure path. Direct-controller checks do not prove middleware/OIDC/error-handler execution.

The shared [SDK interface](../../TheTechIdea.Data/OilGas/ClientIAccessControlService.cs) and [actual SDK service](../../Beep.OilandGas.Client/App/Services/AccessControl/AccessControlService.cs) expose all three operations through the captured-company HTTP transport. They validate response ownership, paging, canonical fingerprints and retirement snapshots. Missing/malformed evidence remains an error. Retirement POST is a single attempt when the connection or response is lost; it is not automatically replayed. Existing requests and consumers move together without a compatibility path.

## Administration experience

The existing [AssetAccess page](../../Beep.OilandGas.Web/Pages/Admin/AccessControl/AssetAccess.razor) offers Review Stored Grants alongside effective direct-grant loading. The existing [state](../../Beep.OilandGas.Web/Services/AssetGrantAdministrationState.Review.cs) retains one selected company/subject, one busy operation and confirmed row identities. Stored review does not enable the effective grant editor. Historical asset/qualifier/level values are displayed as evidence without inferring authority; unqualified facilities are identified explicitly.

Paged inventory shows stored level, active state, last actor and UTC change time. Active confirmed rows offer an explicit retirement confirmation and required reason; inactive rows still offer retirement history. History shows server actor, reason, UTC timestamp and previous/new revisions, with page navigation. Copied or changed rows cannot become command targets; the complete original snapshot and current revision are captured before the write.

Success requires the returned receipt to match the reviewed snapshot, requested reason, selected account/company/database and previous revision. A fresh history read must confirm that exact persistent receipt, followed by fresh inventory showing the inactive grant with the receipt's revision. Any failed/unconfirmed step clears actions until reload/review. Account/company/subject/reload/disposal changes cancel pending work and discard late results before resolving circuit failure services. Web DI uses the existing remote app SDK and standard app roles; full host DI remains unverified.

## Repository upgrade

Actual EF 10.0.9 tooling generated AssetGrantRetirement migrations/snapshots for [SQL Server](../../Beep.OilandGas.Repository/Migrations/SqlServer/20261005135355_AssetGrantRetirement.cs), [PostgreSQL](../../Beep.OilandGas.Repository/Migrations/PostgreSql/20261005135357_AssetGrantRetirement.cs) and [Oracle](../../Beep.OilandGas.Repository/Migrations/Oracle/20261005135359_AssetGrantRetirement.cs). They add only APP_ASSET_GRANT_RETIREMENT and its generated indexes/restrict-delete foreign keys to the existing grant, application actor and tenant/company owners. There is no handwritten SQL, seed/backfill operation, grant rewrite or live migration. Provider snapshots match the current model. Business/module schemas remain Beep MigrationManager; the approved existing Identity/access repository remains EF-owned.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger 'console;verbosity=minimal'
./Scripts/Test-HierarchyWebComponents.ps1
```

**1031 passed, 0 failed, 0 skipped**, adding 98 cases to the previous 933:

- 35 [stored-review/retirement cases](../../Beep.OilandGas.Repository.Tests/StoredAssetGrantReviewTests.cs), using actual SQLite persistence, current company authority and the existing store. They cover former/inactive accounts, historical unknown qualifiers, inactive inventory, scoped paging, exact/unproven ownership, current authority/binding refusal, stale/already-inactive conflicts, required revisions/reasons, rollback after SaveChanges, EF compare-and-swap failure, unrelated pending changes, preserved creation/evidence, append-only SaveChanges, regrant and case-insensitive aliases before counts/out-of-range pages.
- 43 [administration-state cases](../../Beep.OilandGas.Repository.Tests/StoredAssetGrantReviewUiTests.cs), covering unknown stored fields, authoritative response checks, original row identity, retirement evidence/inactive readback, invalid reasons, unconfirmed writes/history and late results after company/account/subject/disposal changes.
- 17 additional [SDK transport cases](../../Beep.OilandGas.Repository.Tests/AccessControlClientTests.cs), covering all three operations under missing selection, caller cancellation, token/response company changes, HTTP refusal and single-attempt POST failure.
- Three [provider schema cases](../../Beep.OilandGas.Repository.Tests/RepositoryMigrationTests.cs) inspect generated operations/constraints and prove no pending model changes without connecting to a provider.

Existing direct-controller refusal cases now cover all three administrative routes. One new actual controller/facade/store/SDK/shared-JSON/Web-state round trip reviews and retires a historical unqualified grant, confirms durable evidence/inactive inventory and performs no PPDM reads/writes. It uses in-process dispatch and proves no live HTTP/auth/DI behavior.

Focused log: `%TEMP%/oilgas-grant-review-tests.log`. All three exact-source access-control components compile against actual MudBlazor 9.7.0 with **0 warnings and 0 errors** through the existing [component check](../../Scripts/Test-HierarchyWebComponents.ps1). Log: `%TEMP%/oilgas-grant-review-razor-check.log`. Focused builds retain known missing-analyzer warnings; missing shared projects are not recreated or stubbed.

## Remaining work and next target

Continue explicit FIELD entitlement and legal/operator/JV eligibility using canonical PPDM interest owners. Unowned/old-binding grant provenance, historical authority retirement outside the current reviewed scope, ordinary grant/revoke event history, large-inventory/provider concurrency and upgrade acceptance remain open. No ownership/type/timezone is guessed from historical records. Complete remaining domain/local-mode/API-gateway/connection/cache/reference boundaries before authoritative provisioning and physical identity acceptance.

Full API/Web startup/DI, auth-owner assemblies, HTTP/OIDC/browser/accessibility/RTL, real PPDM/repository providers, concurrent authority changes and release pilots remain unverified. CA-04 period close and the finance, engineering, operations, regional and release phases retain their existing owners. No full phase, company isolation or plan completion is claimed.
