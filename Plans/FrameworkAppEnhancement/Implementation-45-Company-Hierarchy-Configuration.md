# Implementation 45 — Company hierarchy configuration

Date: 2026-10-05. Owners: CA-03/05/06; FG-01/10/11/12/20. Status: implemented with focused persistence, command, hierarchy, editing-state and Razor compilation evidence. Live repository upgrades, host integration and release acceptance remain open.

## Problem and resulting behavior

The hierarchy configuration API refused an unavailable store, and the existing administration page could only display configuration. There was an unused security model in Models but no canonical provisioning or audited write path. This slice implements that path in the existing repository, with shared contracts and entities owned by TheTechIdea.Data.

[RepositoryHierarchyConfigurationStore](../../Beep.OilandGas.ApiService/Services/RepositoryHierarchyConfigurationStore.cs) resolves current active selected-company membership through CompanyModuleConnection. Configuration reads require membership; the existing configuration administration API additionally requires the current company Administrator. Writes independently refresh that authority, rather than trusting incoming role claims or client state. Organization assertions must exactly match the selected company. Tenant and company jointly own the aggregate, including when two tenants use the same company identifier. Configuration administration does not require a PPDM module binding.

The reviewed structure is FIELD at level zero and WELL, POOL and FACILITY as its level-one children. Commands accept exactly those four categories, valid display names and coherent visibility. A hidden FIELD category requires hidden children. Commands do not invent asset relationships, assign permissions or accept arbitrary structures. Unicode category names are supported; full app localization remains planned.

Each read carries one server revision. Initial creation requires a missing expected revision; updates require the exact persisted revision. Stale or inconsistent revisions are refused. A serializable EF repository transaction writes the current aggregate and one new immutable revision record together. The server derives actor, UTC time, company, key and new revision. Incoming inherited audit fields are excluded from the stored configuration. Creation metadata is preserved across updates; historical revision rows are retained. A command requires a clean repository context and its own transaction. Failure rolls back both writes and detaches its pending aggregate/audit, preventing a subsequent unrelated SaveChanges from persisting a failed command. Provider-specific concurrent creation and transaction behavior still require live acceptance; no automatic mutation retry is introduced.

## Canonical data and schema ownership

- [Shared configuration entities](../../TheTechIdea.Data/OilGas/OrganizationHierarchyConfiguration.cs), [store contract](../../TheTechIdea.Data/OilGas/IHierarchyConfigurationStore.cs) and [configuration DTO](../../TheTechIdea.Data/OilGas/HierarchyConfig.cs) live in TheTechIdea.Data. The unused Models entity is removed; its security namespace remains consistent with the module-schema exclusion.
- [HierarchyConfigurationMapping](../../Beep.OilandGas.Repository/HierarchyConfigurationMapping.cs) maps ORGANIZATION_HIERARCHY_CONFIG and APP_HIERARCHY_REVISION into the existing Identity/access repository. Composite company ownership, unique company configuration, restrictive company/account foreign keys and an EF revision concurrency token are explicit.
- Actual EF tooling generated CompanyHierarchyConfiguration migrations and snapshots for [SQL Server](../../Beep.OilandGas.Repository/Migrations/SqlServer/20261005122617_CompanyHierarchyConfiguration.cs), [PostgreSQL](../../Beep.OilandGas.Repository/Migrations/PostgreSql/20261005122643_CompanyHierarchyConfiguration.cs) and [Oracle](../../Beep.OilandGas.Repository/Migrations/Oracle/20261005122647_CompanyHierarchyConfiguration.cs). The migrations add only these two tables and their constraints/indexes; they contain no handwritten SQL or production seed data. No live database migration was applied.

This is the existing repository-owned security configuration boundary, already excluded from Beep module provisioning by ModuleSchemaBoundary. Business/module schemas remain Beep MigrationManager. There is no second PPDM configuration table, client database connection or alternate configuration store. The approved existing Identity/access repository remains EF-owned.

## Active hierarchy and UI integration

[AssetHierarchyService](../../Beep.OilandGas.LifeCycle/Services/AccessControl/AssetHierarchyService.cs) requires the canonical configuration store. Its existing configuration methods delegate to this store with administrator checks. Presentation reads apply category labels and visibility to the already-authorized stored PPDM graph; actual asset names, keys and facility qualifiers remain intact. Existing row/grant/company/binding checks remain mandatory, and a configuration revision change during an asset read refuses the stale result. Stored path validation continues to enforce actual relationships and current grants independently of presentation visibility. The redundant BoundAssetHierarchyService wrapper is removed; mandatory gates now live in the active service.

[HierarchyConfigurationState](../../Beep.OilandGas.Web/Services/HierarchyConfigurationState.cs) validates confirmed configuration identities, categories, structure and revision. A successful empty read permits explicit draft creation; defaults are editable category labels, never invented domain records. Saves capture the confirmed company and revision rather than mutable DTO identity/audit fields. Invalid names remain editable without a write. Inputs and repeated saves are guarded while saving. Company/account changes, reload and disposal cancel pending work and discard late responses.

The existing [hierarchy configuration page](../../Beep.OilandGas.Web/Pages/Admin/AccessControl/HierarchyConfig.razor) retains its standard Administrator guard and selected-company display. MudBlazor controls edit display names and visibility; structural relationships remain fixed. Hiding FIELD hides child categories. A positive write response triggers a fresh read; only a valid new revision produces a save confirmation. A failed or unconfirmed write/readback clears editable state and requires refresh before another save. Confirmed saves refresh the asset tree and clear its previous selection. New edits clear the prior save confirmation. The existing ApiClient/company transport and OilGasCallFailures remain the registered owners; no new HTTP or database path is introduced.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger 'console;verbosity=minimal'
./Scripts/Test-HierarchyWebComponents.ps1
```

**807 passed, 0 failed, 0 skipped.** The focused gate contains the previous 760, minus nine obsolete binding-wrapper cases and two unavailable-store refusal cases, plus 32 store/authority/persistence cases, three provider-schema cases and 23 editing/readback cases. Existing direct controller, binding, row-policy and configuration UI cases continue against the current canonical implementation.

[Configuration tests](../../Beep.OilandGas.Repository.Tests/CompanyHierarchyConfigurationTests.cs) exercise real SQLite repository transactions, immutable history, server-derived metadata, malformed structure, expected revisions, tenant isolation, current account/membership/role refusal, independence from PPDM bindings and failures before/after SaveChanges. Actual hierarchy/policy/recording-Beep cases prove Unicode labels, category hiding without permission revocation and refusal of a configuration change during a PPDM read.

[Editing tests](../../Beep.OilandGas.Repository.Tests/CompanyHierarchyUiTests.cs) cover explicit drafts, revision-bound writes, readback confirmation, identity mutation, invalid labels, coherent visibility, failed writes/refreshes, selection changes and single in-flight saves. One editor/store/hierarchy round trip uses the actual production state and service, canonical SQLite repository and recording PPDM SDK. [Provider migration checks](../../Beep.OilandGas.Repository.Tests/RepositoryMigrationTests.cs) verify EF-only operations, owner/audit mappings and no pending model changes for all three providers; generated scripts are examined without connecting to a live provider.

Final focused log: `%TEMP%/oilgas-hierarchy-configuration-tests.log`. Exact-source component compilation uses the existing [reproducible script](../../Scripts/Test-HierarchyWebComponents.ps1), real MudBlazor 9.7.0 and actual focused production-state/Data assemblies. **Both components compile with zero warnings and errors.** Razor log: `%TEMP%/oilgas-hierarchy-razor-final-check.log`.

Full Web/API Program DI/startup, complete ApiClient/diagnostics/auth-owner assembly integration, live ASP.NET HTTP/OIDC/browser, manual accessibility/RTL, live PPDM providers, repository upgrades and concurrent create/update/revocation behavior remain unverified. Focused builds retain the missing shared analyzer warning. Missing shared projects were not fabricated, and no broad host build was attempted.

## Next work

Audit the existing asset-grant administration UI/client for supported asset types, complete facilities, selected-company state, current subjects and canonical errors. Continue remaining domain wrappers, local-mode/API-gateway paths, shared connection/cache/reference policies, FIELD/legal/operator/JV entitlement, complete direct facility grant persistence and historical-grant review before authoritative provisioning and physical identity acceptance. CA-04 period close, finance, engineering, operations, regional and release phases retain their owners. No phase or full-plan completion is claimed.
