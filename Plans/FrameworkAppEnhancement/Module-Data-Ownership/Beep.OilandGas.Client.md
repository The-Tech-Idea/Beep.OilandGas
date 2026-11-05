# Beep.OilandGas.Client — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

2 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

| Original source | Current definition | Namespace |
| --- | --- | --- |
| `TheTechIdea.Data/OilGas/ClientIAccessControlService.cs` | `Beep.OilandGas.Client/App/Services/AccessControl/IAccessControlService.cs` | `Beep.OilandGas.Client.App.Services.AccessControl` |
| `TheTechIdea.Data/OilGas/ClientILeaseService.cs` | `Beep.OilandGas.Client/App/Services/Lease/ILeaseService.cs` | `Beep.OilandGas.Client.App.Services.Lease` |

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.Client/Beep.OilandGas.Client.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

3 candidate incoming files; 71 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |
| `TheTechIdea.Data/OilGas/ClientIAccessControlService.cs` | `Beep.OilandGas.Client.App.Services.AccessControl`: IAccessControlService, for | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ClientILeaseService.cs` | `Beep.OilandGas.Client.App.Services.Lease`: ILeaseService | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/IAccessControlService.cs` | `Beep.OilandGas.Models.Core.Interfaces`: IAccessControlService | Beep.OilandGas.Client/App/Services/AccessControl/IAccessControlService.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.Client/App/AppOptions.cs`
- `Beep.OilandGas.Client/App/BeepOilandGasApp.cs`
- `Beep.OilandGas.Client/App/ClientApiTransport.cs`
- `Beep.OilandGas.Client/App/CompanyClientContext.cs`
- `Beep.OilandGas.Client/App/CompanyResponseStream.cs`
- `Beep.OilandGas.Client/App/IBeepOilandGasApp.cs`
- `Beep.OilandGas.Client/App/ServiceAccessMode.cs`
- `Beep.OilandGas.Client/App/Services/AccessControl/AccessControlService.cs`
- `Beep.OilandGas.Client/App/Services/Accounting/AccountingService.cs`
- `Beep.OilandGas.Client/App/Services/Accounting/IAccountingService.cs`
- `Beep.OilandGas.Client/App/Services/Analysis/AnalysisService.GasLift.cs`
- `Beep.OilandGas.Client/App/Services/Analysis/AnalysisService.Pipeline.cs`
- `Beep.OilandGas.Client/App/Services/Analysis/AnalysisService.WellTest.cs`
- `Beep.OilandGas.Client/App/Services/Analysis/AnalysisService.cs`
- `Beep.OilandGas.Client/App/Services/Analysis/IAnalysisService.cs`
- `Beep.OilandGas.Client/App/Services/Calculations/CalculationsService.Economic.cs`
- `Beep.OilandGas.Client/App/Services/Calculations/CalculationsService.Flash.cs`
- `Beep.OilandGas.Client/App/Services/Calculations/CalculationsService.NodalAnalysis.cs`
- `Beep.OilandGas.Client/App/Services/Calculations/CalculationsService.cs`
- `Beep.OilandGas.Client/App/Services/Calculations/ICalculationsService.cs`
- `Beep.OilandGas.Client/App/Services/Connection/ConnectionService.cs`
- `Beep.OilandGas.Client/App/Services/Connection/IConnectionService.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/DataManagementService.Audit.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/DataManagementService.CRUD.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/DataManagementService.Defaults.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/DataManagementService.FieldMapping.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/DataManagementService.Jurisdiction.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/DataManagementService.LOV.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/DataManagementService.Metadata.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/DataManagementService.Repository.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/DataManagementService.Setup.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/DataManagementService.Validation.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/DataManagementService.Versioning.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/DataManagementService.Workflow.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/DataManagementService.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/IDataManagementService.cs`
- `Beep.OilandGas.Client/App/Services/DataManagement/PpdmDataClient.cs`
- `Beep.OilandGas.Client/App/Services/Drilling/DrillingService.cs`
- `Beep.OilandGas.Client/App/Services/Drilling/IDrillingService.cs`
- `Beep.OilandGas.Client/App/Services/Field/FieldService.cs`
- `Beep.OilandGas.Client/App/Services/Field/HseIncidentClient.cs`
- `Beep.OilandGas.Client/App/Services/Field/IFieldService.cs`
- `Beep.OilandGas.Client/App/Services/Lease/LeaseService.cs`
- `Beep.OilandGas.Client/App/Services/LifeCycle/ILifeCycleService.cs`
- `Beep.OilandGas.Client/App/Services/LifeCycle/LifeCycleService.cs`
- `Beep.OilandGas.Client/App/Services/Operations/IOperationsService.cs`
- `Beep.OilandGas.Client/App/Services/Operations/OperationsService.cs`
- `Beep.OilandGas.Client/App/Services/Permits/IPermitsService.cs`
- `Beep.OilandGas.Client/App/Services/Permits/PermitsService.cs`
- `Beep.OilandGas.Client/App/Services/Production/IProductionService.cs`
- `Beep.OilandGas.Client/App/Services/Production/ProductionService.Accounting.cs`
- `Beep.OilandGas.Client/App/Services/Production/ProductionService.cs`
- `Beep.OilandGas.Client/App/Services/Properties/IPropertiesService.cs`
- `Beep.OilandGas.Client/App/Services/Properties/PropertiesService.cs`
- `Beep.OilandGas.Client/App/Services/Pumps/IPumpsService.cs`
- `Beep.OilandGas.Client/App/Services/Pumps/PumpsService.HydraulicPump.cs`
- `Beep.OilandGas.Client/App/Services/Pumps/PumpsService.cs`
- `Beep.OilandGas.Client/App/Services/ServiceBase.cs`
- `Beep.OilandGas.Client/App/Services/Well/IWellService.cs`
- `Beep.OilandGas.Client/App/Services/Well/WellService.cs`
- `Beep.OilandGas.Client/Authentication/IAuthenticationProvider.cs`
- `Beep.OilandGas.Client/Authentication/TokenProvider.cs`
- `Beep.OilandGas.Client/Connection/ConnectionContext.cs`
- `Beep.OilandGas.Client/Connection/ConnectionHelper.cs`
- `Beep.OilandGas.Client/Connection/ConnectionManager.cs`
- `Beep.OilandGas.Client/Connection/DataSourceManager.cs`
- `Beep.OilandGas.Client/DependencyInjection/ClientServiceCollectionExtensions.cs`
- `Beep.OilandGas.Client/Exceptions/ApiClientException.cs`
- `Beep.OilandGas.Client/Exceptions/ApiException.cs`
- `Beep.OilandGas.Client/Exceptions/ApiTimeoutException.cs`
- `Beep.OilandGas.Client/Models/ApiResponse.cs`

## Migration tasks

- [x] Record the confirmed existing owner, destination, namespace and dependency decisions in inventory.json and restoration-progress.json; current destination hashes are in observed evidence.
- [x] Keep the internal project graph acyclic and record shared-contract placements without another global data project. The current static superset audit covers 69 existing projects and 284 literal project-reference edges, with zero cycles; external symbolic references are listed separately in evidence/canonical-dependency-recovery/project-reference-audit.json.
- [ ] Move definitions once, update all listed callers and references, then remove forwarding stubs/obsolete definitions. Preserve source changes from the soft-reset commit.
- [ ] Fix CLR namespaces and ambiguous/duplicate DTO names; retain native table/key names and public JSON member names unless all consumers migrate together. Update explicit JsonPropertyName, EF/Beep mappings, entity registration, migration discovery, serializers and stored assembly-qualified type names where applicable.
- [ ] Update typed HTTP client signatures, controller request/response types, service interfaces, DI registration, Razor bindings, setup/module manifests, tests, source-linked fixtures and build scripts that name the moved source/assembly.
- [ ] Reuse existing log, audit and notification paths; check that changed exception/type names do not hide errors, lose correlation or change UI failure feedback. IdentityServer stays authentication-only and app RBAC remains authoritative.
- [ ] Update relevant docs and project/solution/package references. Remove stale centralized-ownership instructions as part of the authorized implementation.

## Verification and dependencies

Run only affected checks selected against the active master tracker. Compile the target owner and immediate consumers, then verify wire serialization, native entity/key registration, migration discovery and retained app-role/company/field boundaries. Test projects must replace source links and assembly HintPaths to moved definitions, not create test-only substitute models. Record full-host/provider/build failures separately from focused harness results. Current executed checks and unavailable integration gates are recorded in the Current implementation section and evidence/canonical-dependency-recovery/observed-evidence.json.

Depends on the common ownership/naming decision and inventory review; caller migration follows each owner group. Finish with one-definition and zero-obsolete-reference checks. Keep a move manifest so an individual slice can be reversed without resetting unrelated staged work.

## Dependency-group implementation - 2026-11-04

Current-owner files below migrated with namespace/caller/source-link updates. Shared contract folders follow existing feature conventions. Duplicate import result/error definitions consolidated into existing plain transport models; no forwarded or parallel result types retained. Exact build/runtime scope and integration failures are recorded in restoration-progress.json.

- `Beep.OilandGas.Client/App/Services/AccessControl/IAccessControlService.cs` (`Beep.OilandGas.Client.App.Services.AccessControl`)
- `Beep.OilandGas.Client/App/Services/Lease/ILeaseService.cs` (`Beep.OilandGas.Client.App.Services.Lease`)
