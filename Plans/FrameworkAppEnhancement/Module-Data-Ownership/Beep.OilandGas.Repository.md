# Beep.OilandGas.Repository — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

19 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

| Original source | Current definition | Namespace |
| --- | --- | --- |
| `TheTechIdea.Data/OilGas/BootstrapOutcome.cs` | `Beep.OilandGas.Repository/Models/BootstrapOutcome.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/ExternalRegistrationProfile.cs` | `Beep.OilandGas.Repository/Models/ExternalRegistrationProfile.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/IdentityExtensions.cs` | `Beep.OilandGas.Repository/Models/IdentityExtensions.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/ModuleBindingRequest.cs` | `Beep.OilandGas.Repository/Models/ModuleBindingRequest.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/PersonaModels.cs` | `Beep.OilandGas.Repository/Models/PersonaModels.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/RepositoryAccountDeletion.cs` | `Beep.OilandGas.Repository/Models/RepositoryAccountDeletion.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/RepositoryAccountRefusals.cs` | `Beep.OilandGas.Repository/Models/RepositoryAccountRefusals.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/RepositoryModels.cs` | `Beep.OilandGas.Repository/Models/RepositoryModels.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/RepositoryRbacModels.cs` | `Beep.OilandGas.Repository/Models/RepositoryRbacModels.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/RepositoryRegistrationResponse.cs` | `Beep.OilandGas.Repository/Models/RepositoryRegistrationResponse.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/RepositoryRoleRequest.cs` | `Beep.OilandGas.Repository/Models/RepositoryRoleRequest.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/RepositoryUserAccess.cs` | `Beep.OilandGas.Repository/Models/RepositoryUserAccess.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/RepositoryUserModels.cs` | `Beep.OilandGas.Repository/Models/RepositoryUserModels.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/UserRoleChangeRequest.cs` | `Beep.OilandGas.Repository/Models/UserRoleChangeRequest.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/CompanyMemberAccessReview.cs` | `Beep.OilandGas.Repository/Models/CompanyMemberAccessReview.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/CompanyMembershipRequest.cs` | `Beep.OilandGas.Repository/Models/CompanyMembershipRequest.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/CompanyMembershipRetirement.cs` | `Beep.OilandGas.Repository/Models/CompanyMembershipRetirement.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/FinancialOperationClaim.cs` | `Beep.OilandGas.Repository/Models/FinancialOperationClaim.cs` | `Beep.OilandGas.Repository.Models` |
| `TheTechIdea.Data/OilGas/RepositoryReadiness.cs` | `Beep.OilandGas.Repository/Models/RepositoryReadiness.cs` | `Beep.OilandGas.Repository.Models` |

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.Repository/Beep.OilandGas.Repository.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

0 candidate incoming files; 96 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.Repository/AssetAccessMapping.cs`
- `Beep.OilandGas.Repository/CompanyAccessMapping.cs`
- `Beep.OilandGas.Repository/FinancialOperationClaimMapping.cs`
- `Beep.OilandGas.Repository/FinancialOperationClaimStore.cs`
- `Beep.OilandGas.Repository/HierarchyConfigurationMapping.cs`
- `Beep.OilandGas.Repository/IdentityExtensionMapping.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20260905143749_InitialRepository.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20260905143749_InitialRepository.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20260905150500_IdentityExtensions.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20260905155046_UserExtension.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20260905175050_PersonaExtensions.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20260912184711_UserProfileMetadata.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20260912192333_AssetAccessExtensions.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20260921080819_FinancialOperationClaims.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20261005045234_CompanyAccess.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20261005051826_CompanyRoleRevocation.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20261005054856_TenantModuleBindings.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20261005072638_PhysicalModuleTargetOwnership.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20261005122647_CompanyHierarchyConfiguration.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20261005122647_CompanyHierarchyConfiguration.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20261005132125_CompositeFacilityGrants.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20261005135359_AssetGrantRetirement.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20261005135359_AssetGrantRetirement.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20261028124523_CompanyMembershipRetirement.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20261028124523_CompanyMembershipRetirement.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20261028223214_CompanyPersonaScope.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/20261028223214_CompanyPersonaScope.cs`
- `Beep.OilandGas.Repository/Migrations/Oracle/OracleRepositoryDbContextModelSnapshot.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260905143745_InitialRepository.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260905143745_InitialRepository.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260905150455_IdentityExtensions.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260905150455_IdentityExtensions.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260905155042_UserExtension.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260905155042_UserExtension.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260905175045_PersonaExtensions.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260905175045_PersonaExtensions.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260912184652_UserProfileMetadata.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260912184652_UserProfileMetadata.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260912192316_AssetAccessExtensions.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260912192316_AssetAccessExtensions.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260921080755_FinancialOperationClaims.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20260921080755_FinancialOperationClaims.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261005045231_CompanyAccess.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261005045231_CompanyAccess.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261005051822_CompanyRoleRevocation.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261005054852_TenantModuleBindings.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261005054852_TenantModuleBindings.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261005072634_PhysicalModuleTargetOwnership.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261005072634_PhysicalModuleTargetOwnership.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261005122643_CompanyHierarchyConfiguration.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261005122643_CompanyHierarchyConfiguration.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261005132121_CompositeFacilityGrants.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261005135357_AssetGrantRetirement.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261005135357_AssetGrantRetirement.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261028124514_CompanyMembershipRetirement.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261028124514_CompanyMembershipRetirement.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261028223210_CompanyPersonaScope.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/20261028223210_CompanyPersonaScope.cs`
- `Beep.OilandGas.Repository/Migrations/PostgreSql/PostgreSqlRepositoryDbContextModelSnapshot.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20260905143742_InitialRepository.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20260905143742_InitialRepository.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20260905150451_IdentityExtensions.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20260905155038_UserExtension.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20260905175015_PersonaExtensions.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20260912184634_UserProfileMetadata.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20260912192147_AssetAccessExtensions.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20260921080731_FinancialOperationClaims.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20261005045154_CompanyAccess.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20261005051805_CompanyRoleRevocation.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20261005054834_TenantModuleBindings.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20261005072608_PhysicalModuleTargetOwnership.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20261005122617_CompanyHierarchyConfiguration.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20261005122617_CompanyHierarchyConfiguration.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20261005132117_CompositeFacilityGrants.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20261005135355_AssetGrantRetirement.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20261005135355_AssetGrantRetirement.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20261028124456_CompanyMembershipRetirement.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20261028124456_CompanyMembershipRetirement.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20261028223152_CompanyPersonaScope.Designer.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/20261028223152_CompanyPersonaScope.cs`
- `Beep.OilandGas.Repository/Migrations/SqlServer/SqlServerRepositoryDbContextModelSnapshot.cs`
- `Beep.OilandGas.Repository/PersonaMapping.cs`
- `Beep.OilandGas.Repository/PersonaScopeMigrationPreflight.cs`
- `Beep.OilandGas.Repository/RepositoryAccessService.cs`
- `Beep.OilandGas.Repository/RepositoryBootstrapService.cs`
- `Beep.OilandGas.Repository/RepositoryCompanyContext.cs`
- `Beep.OilandGas.Repository/RepositoryConnectionSource.cs`
- `Beep.OilandGas.Repository/RepositoryDbContext.cs`
- `Beep.OilandGas.Repository/RepositoryDesignTimeFactories.cs`
- `Beep.OilandGas.Repository/RepositoryPersonaService.Workspaces.cs`
- `Beep.OilandGas.Repository/RepositoryPersonaService.cs`
- `Beep.OilandGas.Repository/RepositoryReadinessService.cs`
- `Beep.OilandGas.Repository/RepositoryRoleCatalogService.cs`
- `Beep.OilandGas.Repository/RepositoryRoleScope.cs`
- `Beep.OilandGas.Repository/RepositoryServiceRegistration.cs`
- `Beep.OilandGas.Repository/RepositoryTenantContext.cs`

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

## Applied module restoration - 2026-11-04

Moved files now use Beep.OilandGas.Repository.Models. All recorded caller imports and exact qualified names were updated. Focused test source links follow owner sources. Data copies removed. Shared validators reused across assembly boundaries. Actual owner-build outcomes are tracked in restoration-progress.json; no runtime/EF/provider acceptance claimed.

- `TheTechIdea.Data/OilGas/BootstrapOutcome.cs` -> `Beep.OilandGas.Repository/Models/BootstrapOutcome.cs`
- `TheTechIdea.Data/OilGas/ExternalRegistrationProfile.cs` -> `Beep.OilandGas.Repository/Models/ExternalRegistrationProfile.cs`
- `TheTechIdea.Data/OilGas/IdentityExtensions.cs` -> `Beep.OilandGas.Repository/Models/IdentityExtensions.cs`
- `TheTechIdea.Data/OilGas/ModuleBindingRequest.cs` -> `Beep.OilandGas.Repository/Models/ModuleBindingRequest.cs`
- `TheTechIdea.Data/OilGas/PersonaModels.cs` -> `Beep.OilandGas.Repository/Models/PersonaModels.cs`
- `TheTechIdea.Data/OilGas/RepositoryAccountDeletion.cs` -> `Beep.OilandGas.Repository/Models/RepositoryAccountDeletion.cs`
- `TheTechIdea.Data/OilGas/RepositoryAccountRefusals.cs` -> `Beep.OilandGas.Repository/Models/RepositoryAccountRefusals.cs`
- `TheTechIdea.Data/OilGas/RepositoryModels.cs` -> `Beep.OilandGas.Repository/Models/RepositoryModels.cs`
- `TheTechIdea.Data/OilGas/RepositoryRbacModels.cs` -> `Beep.OilandGas.Repository/Models/RepositoryRbacModels.cs`
- `TheTechIdea.Data/OilGas/RepositoryRegistrationResponse.cs` -> `Beep.OilandGas.Repository/Models/RepositoryRegistrationResponse.cs`
- `TheTechIdea.Data/OilGas/RepositoryRoleRequest.cs` -> `Beep.OilandGas.Repository/Models/RepositoryRoleRequest.cs`
- `TheTechIdea.Data/OilGas/RepositoryUserAccess.cs` -> `Beep.OilandGas.Repository/Models/RepositoryUserAccess.cs`
- `TheTechIdea.Data/OilGas/RepositoryUserModels.cs` -> `Beep.OilandGas.Repository/Models/RepositoryUserModels.cs`
- `TheTechIdea.Data/OilGas/UserRoleChangeRequest.cs` -> `Beep.OilandGas.Repository/Models/UserRoleChangeRequest.cs`

## Domain migration checkpoint - 2026-11-04

Applied owner namespace moves below; copies removed from Data, callers and focused source links updated. Owner builds are recorded in restoration-progress.json. Direct owner builds using existing dependency outputs do not prove clean integrated builds, API/Web or provider behavior.

- `Beep.OilandGas.Repository/Models/BootstrapOutcome.cs`: `Beep.OilandGas.Repository.Models`
- `Beep.OilandGas.Repository/Models/ExternalRegistrationProfile.cs`: `Beep.OilandGas.Repository.Models`
- `Beep.OilandGas.Repository/Models/IdentityExtensions.cs`: `Beep.OilandGas.Repository.Models`
- `Beep.OilandGas.Repository/Models/ModuleBindingRequest.cs`: `Beep.OilandGas.Repository.Models`
- `Beep.OilandGas.Repository/Models/PersonaModels.cs`: `Beep.OilandGas.Repository.Models`
- `Beep.OilandGas.Repository/Models/RepositoryAccountDeletion.cs`: `Beep.OilandGas.Repository.Models`
- `Beep.OilandGas.Repository/Models/RepositoryAccountRefusals.cs`: `Beep.OilandGas.Repository.Models`
- `Beep.OilandGas.Repository/Models/RepositoryModels.cs`: `Beep.OilandGas.Repository.Models`
- `Beep.OilandGas.Repository/Models/RepositoryRbacModels.cs`: `Beep.OilandGas.Repository.Models`
- `Beep.OilandGas.Repository/Models/RepositoryRegistrationResponse.cs`: `Beep.OilandGas.Repository.Models`
- `Beep.OilandGas.Repository/Models/RepositoryRoleRequest.cs`: `Beep.OilandGas.Repository.Models`
- `Beep.OilandGas.Repository/Models/RepositoryUserAccess.cs`: `Beep.OilandGas.Repository.Models`
- `Beep.OilandGas.Repository/Models/RepositoryUserModels.cs`: `Beep.OilandGas.Repository.Models`
- `Beep.OilandGas.Repository/Models/UserRoleChangeRequest.cs`: `Beep.OilandGas.Repository.Models`

## Dependency-group implementation - 2026-11-04

Current-owner files below migrated with namespace/caller/source-link updates. Shared contract folders follow existing feature conventions. Duplicate import result/error definitions consolidated into existing plain transport models; no forwarded or parallel result types retained. Exact build/runtime scope and integration failures are recorded in restoration-progress.json.

- `Beep.OilandGas.Repository/Models/CompanyMemberAccessReview.cs` (`Beep.OilandGas.Repository.Models`)
- `Beep.OilandGas.Repository/Models/CompanyMembershipRequest.cs` (`Beep.OilandGas.Repository.Models`)
- `Beep.OilandGas.Repository/Models/CompanyMembershipRetirement.cs` (`Beep.OilandGas.Repository.Models`)
- `Beep.OilandGas.Repository/Models/FinancialOperationClaim.cs` (`Beep.OilandGas.Repository.Models`)

## Compiler-bound migration checkpoint - 2026-11-04

Files below returned with callers and source links. Incident command interfaces use the consuming data-management Core.Interfaces layer; shared work-order/field facts stay in the existing Models contract layer. Owner builds pass, using existing dependency outputs. Full consumer/provider gates remain open. The retired Data validation bridge is removed. Current internal evidence-validation access is restricted to Accounting, ProductionAccounting and the existing Repository test assembly; it does not cache authority or datasource reads.

- `Beep.OilandGas.Repository/Models/RepositoryReadiness.cs` (`Beep.OilandGas.Repository.Models`)
