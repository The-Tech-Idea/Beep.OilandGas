# Implementation 46 — Company asset-grant administration

Date: 2026-10-05. Owners: CA-03/05/06; FG-01/10/11/12. Status: implemented with focused production-state/service/transport and exact-source Razor evidence. Complete facility grant commands, historical grant review and integrated runtime/release acceptance remain open.

## Problem and resulting behavior

The existing asset-access administration page lacked a standard Administrator route guard, offered unsupported AREA and incomplete direct FACILITY assignments, omitted the selected-company assertion from grants and read inherited access as if it were directly revocable. It did not load when the target user changed, retained old rows after failures or company changes and reported generic snackbar messages. A positive mutation response was treated as success without confirming the resulting grant state.

The existing [AssetAccess page](../../Beep.OilandGas.Web/Pages/Admin/AccessControl/AssetAccess.razor) now uses the standard Administrator route guard and current CompanyClientContext. It shows the selected tenant/company, accepts an application-resolved account ID and explicitly loads that subject's direct grants. It manages the currently supported FIELD, WELL and POOL identities. AREA is removed. Facilities continue through reviewed FIELD inheritance; direct facility assignments remain explicitly unavailable until the canonical grant contract/store/policy carry the complete facility identity. This is an incremental implementation of the approved plan, not completion of its facility requirement.

[AssetGrantAdministrationState](../../Beep.OilandGas.Web/Services/AssetGrantAdministrationState.cs) belongs to one component instance. It captures the company/account selection and target subject for each operation, cancels superseded requests and refuses late results after reload, subject/company/account changes or disposal. Company/account changes also clear the target subject and draft. The UI requires current selected-company Administrator status before requests; current authoritative membership, roles, subject and binding are independently enforced by the existing API/service/store. Client state is never authorization authority.

Reads use the existing `/api/AccessControl/user/{subject}/assets?includeInherited=false&organizationId={company}` route. User and company identifiers are URI-encoded. The response must be non-null, bounded to 4,096 rows, active, unique by reviewed asset key and consistent with the exact subject/company. Unsupported types, inappropriate facility qualifiers, malformed identifiers and levels are refused. Only successful empty responses become an empty view. Returned legacy WELL/POOL inheritance flags can be inspected/revoked; new draft inheritance is restricted to FIELD, whose child relationship policy is actually implemented. Larger lists need future paging or narrower API queries, rather than unbounded rendering.

## Mutations and confirmation

Grant/revoke operations use the existing shared Data requests and API routes; no parallel DTO, HTTP client, database store or local access mode is introduced. Grant bodies carry the captured target subject, selected company, exact asset type/identifier, level and inheritance flag. Actor metadata is not synthesized by Web. Invalid drafts show actionable errors while keeping the previously confirmed list; no write is sent.

Revocation requires the original object from the confirmed current list and its original subject/company/type/ID/level/inheritance tuple. Copied or mutated rows and stale company/subject selections cannot become revoke targets. The page presents a textual confirmation naming the subject and exact asset before sending the command; confirmation becomes unavailable when the list or selection is invalidated.

Inputs/actions are disabled while a request is active, and state guards prevent repeated writes. Mutation requests capture their draft independently of later edits. A positive API response triggers a fresh direct-grant read. Grant success requires the requested asset/level/inheritance in that read; revoke success requires the target key to be absent. False responses, malformed or mismatched readback, provider/request errors and unconfirmed changes clear the editable list and require reload before another write. Error wording uses the existing OilGasCallFailures owner, including its failure reference, rather than swallowed generic snackbars. This proves current returned state, not a durable mutation receipt or concurrent-provider acceptance.

## Active application integration

Web Program registers the state transiently using the existing ApiClient GET/POST paths, CompanyClientContext transport and OilGasCallFailures. The full Program/ApiClient/auth-owner graph is source integration only until the missing shared projects are available. API-first access and shared Data ownership are preserved.

The page uses MudBlazor fields, selects, switches, buttons, progress, alerts and a data grid. It has explicit missing-company, missing-role, loading, empty, validation/request error, confirmed success and disabled states. Revoke actions have text and an accessible label; custom clickable divs, style/script blocks and new hardcoded colors are not introduced. Responsive field placement uses the existing MudGrid breakpoints. Actual keyboard, screen-reader, responsive, localization/RTL and browser interaction remain acceptance work.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger 'console;verbosity=minimal'
./Scripts/Test-HierarchyWebComponents.ps1
```

**881 passed, 0 failed, 0 skipped**, adding 74 [asset-grant UI/state/transport cases](../../Beep.OilandGas.Repository.Tests/CompanyAssetGrantUiTests.cs) to the previous 807. Final log: `%TEMP%/oilgas-asset-grant-ui-tests.log`.

Cases cover authority/identifier requirements, encoded direct-only routing, malformed/mismatched/duplicate/oversized results, supported and unsupported drafts, exact grant/revoke requests, copied/mutated revoke targets, false/error/mismatched readback, cleared stale rows, single in-flight writes and cancellation/late responses during read/write/readback. Six late-failure cases also prove that disposed or superseded work does not resolve obsolete circuit failure services or publish errors. Three direct editor round trips use the actual production state, facade, target verifier, SQLite company/access repository, PPDM row policy and recording Beep SDK to grant, update and revoke FIELD/WELL/POOL access. A separate actual-store case proves revoked membership prevents mutation despite a stale Administrator UI selection. One real CompanyClientContext HTTP/JSON case verifies selected-company headers and shared request/readback routing independently of the target subject.

The existing [component verification script](../../Scripts/Test-HierarchyWebComponents.ps1) now includes the asset-access page alongside the two hierarchy components. It compiles exact source copies against the actual MudBlazor 9.7.0 package and focused production-state/Data assemblies, verifies source-copy hashes and leaves evidence under the temporary directory. **All three components compile with 0 warnings and 0 errors.** Log: `%TEMP%/oilgas-asset-grant-razor-check.log`.

These are focused source-linked state/service/registry/SDK/transport checks and component compilation. Full Web/API host DI/startup, complete ApiClient/diagnostics/auth-owner graph, ASP.NET HTTP/OIDC/browser, live PPDM providers and concurrent authorization/provider behavior remain unverified. Builds retain the missing shared analyzer warning. Missing projects were not fabricated. No schema change, handwritten SQL or live migration was introduced.

## Next work

Complete FACILITY_ID/FACILITY_TYPE throughout canonical direct grant/revoke contracts, persistence, target verification, row policy, facade and consumers, with safe handling of historical unqualified grants. Review historical grants and inaccessible/inactive targets: this page reads active PPDM-visible direct grants, not an administrative inventory of all stored grants. Continue FIELD/legal/operator/JV entitlement, remaining domain/local-mode/API-gateway wrappers and shared connection/cache/reference policies before authoritative provisioning and physical identity acceptance. CA-04 period close, finance, engineering, operations, regional and release phases remain open with their existing owners.
