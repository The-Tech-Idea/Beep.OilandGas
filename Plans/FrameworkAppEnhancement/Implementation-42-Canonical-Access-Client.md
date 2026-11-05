# Implementation 42 — Canonical access client

Date: 2026-10-05. Owners: CA-02/03/05/06; FG-01/10/11/12. Status: implemented with focused client/transport and service round-trip evidence; full host/client/release gates remain open.

## Problem and resulting behavior

The active SDK access client used GET for an access-check POST endpoint and expected a boolean instead of AccessCheckResponse. Its asset-list and permission URLs did not match the controllers. Profile results were untyped, local access branches bypassed the mandatory API gateway, and facility checks could not supply FACILITY_TYPE.

The existing AccessControlService now uses the canonical ClientApiTransport directly for every operation. The existing application facade obtains that transport when constructing the service; local access is explicitly unavailable through the remote transport guard. No alternate access client or local authorization implementation is introduced.

## Actual controller contracts

| SDK operation | Existing controller route | Request/result behavior |
|---|---|---|
| CheckAssetAccessAsync | POST /api/AccessControl/check-access | Shared AccessCheckRequest; shared AccessCheckResponse.HasAccess decision. FACILITY requires AssetId and FacilityType; optional RequiredPermission is retained. |
| GetUserAccessibleAssetsAsync | GET /api/AccessControl/user/{targetUserId}/assets | Typed AssetAccess list. assetType, includeInherited and optional organizationId assertion use query parameters. |
| GetUserRolesAsync | GET /api/AccessControl/user/{targetUserId}/roles | Typed string list from current API authority. No roles are manufactured or granted locally. |
| HasPermissionAsync | GET /api/AccessControl/user/{targetUserId}/permission/{permissionId} | Explicit boolean result. false is a valid decision; missing/null results are errors. |
| GetUserProfileAsync | GET /api/UserProfile/{targetUserId} | Existing shared UserProfile, with exact subject validation. |

The authoritative controller owners remain [AccessControlController](../../Beep.OilandGas.ApiService/Controllers/AccessControl/AccessControlController.cs) and [UserProfileController](../../Beep.OilandGas.ApiService/Controllers/AccessControl/UserProfileController.cs). This slice aligns the SDK with those routes, not new server endpoints.

## Implementation and ownership

- [AccessControlService](../../Beep.OilandGas.Client/App/Services/AccessControl/AccessControlService.cs) no longer inherits ServiceBase or branches on AccessMode. The unused IAccessControlLocalService contract is removed. All methods use the existing [ClientApiTransport](../../Beep.OilandGas.Client/App/ClientApiTransport.cs), with authentication, per-message captured company scope, cancellation, current-response checks and fresh-message GET retries. Access-check POSTs remain single attempts.
- [BeepOilandGasApp](../../Beep.OilandGas.Client/App/BeepOilandGasApp.cs) constructs the active access service from GetApiTransport. That existing guard refuses local mode. Other facade modules still require their own API-only audit; this slice does not claim all local business access has been retired.
- [SDK access contract](../../TheTechIdea.Data/OilGas/ClientIAccessControlService.cs) moves into TheTechIdea.Data without changing its namespace. It uses shared AssetAccess/UserProfile/request/result owners. The former Client declaration is removed. The server orchestration and SDK transport interfaces retain their distinct existing namespaces and responsibilities; no business DTO copy is added.
- Optional facilityType/requiredPermission and includeInherited/organizationId arguments are appended after existing cancellation parameters. Profile results become typed UserProfile. There are no repository-local external profile callers requiring adaptation; published consumers still require coordinated rebuilding/release evidence.
- [AccessCheckResponse](../../TheTechIdea.Data/OilGas/AccessCheckResponse.cs) marks HasAccess required for System.Text.Json deserialization. Missing decisions cannot silently default to false. A granted decision must carry a reviewed READ/WRITE/DELETE access level.
- Empty/null/malformed results and HTTP refusals remain errors. Accessible assets must have the requested subject, active status, a reviewed type/level, complete facility identity and the captured selected company's identifier. An explicit organization assertion cannot be silently discarded. Invalid profiles and role lists are refused. Legitimate false decisions and empty lists remain valid.
- URI segments/query values are encoded. ConnectionName/global datasource defaults are not appended. The company identifier is captured before reading asset results and compared with returned records; client assertions do not authorize access. The API's current account/company/store/policy gates remain independent authorities.

No IdentityServer role claims, role assignments, database access, schema migration or SQL are added. Client role reads do not replace the existing standard ASP.NET role bridge.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger 'console;verbosity=minimal'
```

Result: **614 passed, 0 failed, 0 skipped**, including **76 new cases** in [AccessControlClientTests](../../Beep.OilandGas.Repository.Tests/AccessControlClientTests.cs). Actual access client and captured-company HTTP transport execute against recording HttpMessageHandlers. Evidence covers:

- All five controller verbs/routes, encoded path/query values, bearer propagation, selected tenant/company headers and complete facility request/response contracts.
- Explicit false versus missing decisions; empty/null/malformed/inconsistent results, incorrect subjects/company/type/levels, missing facility keys and organization assertions.
- HTTP 401/403/404/409/500 errors; invalid inputs; missing selection and caller cancellation before authentication/HTTP.
- Company switches during token lookup and delayed responses for every operation, including handlers ignoring cancellation.
- Single-attempt access-check POST failures/timeouts, fresh-message GET retries preserving company scope, and canonical contract assembly ownership.
- Four round trips connecting the actual SDK client to the actual UserAssetAccessService/policy/registry/PPDM repository for FIELD/WELL/POOL/FACILITY. Shared requests/results serialize across the handler boundary, active records grant access, and subsequent inactive records produce valid false/empty results.

Those four service round trips use a test dispatcher and recording Beep datasource with real SQLite company/Identity/access persistence and canonical PPDM metadata. They do not execute ASP.NET HTTP routing, token authentication, middleware, DI startup or a live PPDM provider. URI encoding evidence proves client construction, not host acceptance of arbitrary encoded identifiers. Application-facade construction is source-only evidence because the complete client/host dependency baseline remains unavailable.

Shared Data, Models and PPDM projects build in the focused invocation. Existing missing-analyzer warnings remain. The 538 previous focused checks continue to pass. No complete client assembly/host/browser/OpenAPI-runtime/provider/full-regression/release claim is made.

## Next work

Integrate remaining hierarchy/domain wrappers and hierarchy clients with canonical row policy and complete facility identities. Direct facility grant persistence, FIELD entitlement, explicit legal/operator/JV interests and historical-grant review remain open. Continue the broader local-mode/gateway and connection/reference/cache audit, then physical identity/provisioning and authoritative period close in the approved order.

Full HTTP/auth-owner/DI/OIDC/browser and provider evidence, coordinated public-package contract adoption, all finance/engineering/operations/regional workflows and release gates retain their existing owners. Business/module schemas remain Beep Migration; the existing Identity/access repository remains EF. No handwritten SQL, live migration or phase closure occurred.
