# Implementation 44 — Company hierarchy experience

Date: 2026-10-05. Owners: CA-03/05/06; FG-01/10/11/12. Status: implemented with focused state/service/transport and Razor compilation evidence; configuration persistence and integrated runtime/release gates remain open.

## Problem and resulting behavior

The existing asset tree loaded once, derived its account from a direct claim helper, retained previously loaded nodes after errors, and had no facility qualifier or company-change handling. The hierarchy configuration page accepted arbitrary organization IDs, hid errors behind generic snackbars and offered a save operation over an uneditable grid backed by an unavailable store. The asset tree had no page consumer.

[AssetHierarchyState](../../Beep.OilandGas.Web/Services/AssetHierarchyState.cs) captures the existing CompanyClientContext selection and composes the existing subject hierarchy route from its API-resolved account and company. Root selectors preserve FACILITY_ID and FACILITY_TYPE together. Supplied organization IDs must match the current company. Missing selection and successful empty containers have explicit states; null/malformed/mismatched results and failed calls remain errors, rather than empty hierarchies.

Each load clears prior data, cancels previous work and captures its generation/company selection. Company, account, parameter and disposal changes prevent late results from replacing current state. Node selection requires membership in the confirmed tree, its original complete identity/parent tuple and the same company snapshot; copied or mutated nodes and presentation containers cannot become selections. These checks protect the display and its callbacks; API-owned current authorization remains authoritative.

## Active UI integration

- [AssetAccessTree](../../Beep.OilandGas.Web/Components/AccessControl/AssetAccessTree.razor) reloads for parameter and company changes, supports RootFacilityType, disposes subscriptions and renders errors before data. Native lists and MudButton controls replace clickable list-item divs. Company containers are presentation text; facility labels retain their type. Loading, empty, missing-company, error and refresh states are explicit. Selection invalidation clears the consuming page's previous selection on reload.
- [HierarchyConfigurationState](../../Beep.OilandGas.Web/Services/HierarchyConfigurationState.cs) resolves the selected company and composes its encoded config route. It clears old rows during reload/switch/disposal, requires an Administrator selection for the UI request and validates response company/level identities. Error wording delegates to the existing OilGasCallFailures path.
- The existing [hierarchy configuration page](../../Beep.OilandGas.Web/Pages/Admin/AccessControl/HierarchyConfig.razor), already linked from the admin workspace, uses the standard Administrator route guard and shows the selected company/tenant. It hosts the asset tree for the signed-in account and displays complete selected asset identities. The free-text organization selector, unsupported save button and misleading creation suggestion are removed. Config API refusals appear in the page. Configuration editing is explicitly unavailable until the canonical store/commands exist; no substitute persistence or successful save is introduced.
- Web registration constructs transient, per-component states with the existing ApiClient and OilGasCallFailures. The existing company transport/authentication/failure owners remain in use. New styling lives only in app.css, uses logical indentation for RTL, and introduces no hardcoded colors, Razor style/script blocks or new JavaScript.

The tree validates reviewed FIELD/WELL/POOL/FACILITY nodes, complete keys, known relationships, unique identities and bounded graph size/depth. The current limit is 4,096 nodes and three levels; larger trees require narrower root selection or future paging rather than an unbounded render. This does not certify localization or complete international UX.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger 'console;verbosity=minimal'
./Scripts/Test-HierarchyWebComponents.ps1
```

**760 passed, 0 failed, 0 skipped**, adding 56 [hierarchy UI-state cases](../../Beep.OilandGas.Repository.Tests/CompanyHierarchyUiTests.cs) to the previous 704 checks. Final log: `%TEMP%/oilgas-hierarchy-ui-final-tests.log`.

Cases exercise missing company, encoded account/company/root selectors, complete facility selection, invalid scope/key assertions, invalid/duplicate/recursive/mismatched response graphs, company/account/root/disposal changes, cancellation and late responses, failure after a previous successful load, empty results, config roles/identity/error handling and post-load identity mutation. One direct Web-state round trip uses the actual hierarchy service, company gate, SQLite access registry, row policy and recording PPDM SDK; the real unavailable config-store refusal reaches the configuration state. One HTTP/JSON case uses the actual CompanyClientContext header and response guard. These are focused production-state/service/transport cases, not the full Web ApiClient/HTTP authentication pipeline.

[Test-HierarchyWebComponents.ps1](../../Scripts/Test-HierarchyWebComponents.ps1) compiles exact copies of both edited Razor components, preserving their namespaces/folder layout, against the app's actual MudBlazor 9.7 package and focused production-state/Data/Beep assemblies. It checks source-copy hashes after compilation and creates its evidence project under the temporary directory. It does not replace or fabricate missing shared libraries. **Both components compiled with 0 warnings and 0 errors.** Log: `%TEMP%/oilgas-hierarchy-razor-final-check.log`. Run the focused tests first so the referenced production-state binaries are current.

Full Web Program DI/startup, the complete Web ApiClient/diagnostics/auth-owner assembly graph, live ASP.NET HTTP/OIDC/browser, keyboard/screen-reader/responsive/manual RTL behavior and live PPDM provider execution remain unverified. Successful component compilation is narrower than rendered browser acceptance. No missing project was recreated, and no broad host build was attempted.

## Next work

Implement the canonical repository-owned hierarchy configuration store, shared entity/contracts, provisioning and audited atomic commands, then replace the temporary read-only UI with verified editing. The active configuration API still explicitly refuses that unavailable store. Continue asset administration/client/domain/local-mode and shared connection/reference/cache audits; the asset-grant administration screen is not covered by this slice.

Direct facility grant persistence, FIELD/legal/operator/JV entitlement, historical-grant review, remaining domain policies, physical identity/provisioning, authoritative period close and the finance/engineering/operations/regional/release plans retain their owners. Business schemas remain Beep MigrationManager; the approved Identity/access registry remains EF. No handwritten SQL, schema migration, live provisioning or phase closure occurred.
