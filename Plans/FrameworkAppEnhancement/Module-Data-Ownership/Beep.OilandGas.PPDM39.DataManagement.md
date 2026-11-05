# Beep.OilandGas.PPDM39.DataManagement — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

14 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

| Original source | Current definition | Namespace |
| --- | --- | --- |
| `TheTechIdea.Data/OilGas/COMPANY_GHG_CATALOG_SELECTION.cs` | `Beep.OilandGas.PPDM39.DataManagement/Models/COMPANY_GHG_CATALOG_SELECTION.cs` | `Beep.OilandGas.PPDM39.DataManagement.Models` |
| `TheTechIdea.Data/OilGas/COMPANY_OILGAS_ENVIRONMENT.cs` | `Beep.OilandGas.PPDM39.DataManagement/Models/COMPANY_OILGAS_ENVIRONMENT.cs` | `Beep.OilandGas.PPDM39.DataManagement.Models` |
| `TheTechIdea.Data/OilGas/CompanyGhgCatalogSelection.cs` | `Beep.OilandGas.PPDM39.DataManagement/Models/CompanyGhgCatalogSelection.cs` | `Beep.OilandGas.PPDM39.DataManagement.Models` |
| `TheTechIdea.Data/OilGas/PPDMFieldEligibility.cs` | `Beep.OilandGas.PPDM39.DataManagement/Models/PPDMFieldEligibility.cs` | `Beep.OilandGas.PPDM39.DataManagement.Models` |
| `TheTechIdea.Data/OilGas/SetupDriverItem.cs` | `Beep.OilandGas.PPDM39.DataManagement/Models/SetupDriverItem.cs` | `Beep.OilandGas.PPDM39.DataManagement.Models` |
| `TheTechIdea.Data/OilGas/SetupRepositoryConnection.cs` | `Beep.OilandGas.PPDM39.DataManagement/Models/SetupRepositoryConnection.cs` | `Beep.OilandGas.PPDM39.DataManagement.Models` |
| `TheTechIdea.Data/OilGas/SetupRepositoryMigration.cs` | `Beep.OilandGas.PPDM39.DataManagement/Models/SetupRepositoryMigration.cs` | `Beep.OilandGas.PPDM39.DataManagement.Models` |
| `TheTechIdea.Data/OilGas/SYSTEM_SETUP_RECORD.cs` | `Beep.OilandGas.PPDM39.DataManagement/Models/SYSTEM_SETUP_RECORD.cs` | `Beep.OilandGas.PPDM39.DataManagement.Models` |
| `TheTechIdea.Data/OilGas/SystemSetupConnectionSelection.cs` | `Beep.OilandGas.PPDM39.DataManagement/Models/SystemSetupConnectionSelection.cs` | `Beep.OilandGas.PPDM39.DataManagement.Models` |
| `TheTechIdea.Data/OilGas/ICompanyCorrectiveActionCommands.cs` | `Beep.OilandGas.PPDM39.DataManagement/Core/Interfaces/ICompanyCorrectiveActionCommands.cs` | `Beep.OilandGas.PPDM39.DataManagement.Core.Interfaces` |
| `TheTechIdea.Data/OilGas/ICompanyIncidentCommands.cs` | `Beep.OilandGas.PPDM39.DataManagement/Core/Interfaces/ICompanyIncidentCommands.cs` | `Beep.OilandGas.PPDM39.DataManagement.Core.Interfaces` |
| `TheTechIdea.Data/OilGas/SystemSetupConfiguration.cs` | `Beep.OilandGas.PPDM39.DataManagement/Models/SystemSetupConfiguration.cs` | `Beep.OilandGas.PPDM39.DataManagement.Models` |
| `TheTechIdea.Data/OilGas/CompanyEnvironmentPreview.cs` | `Beep.OilandGas.PPDM39.DataManagement/Models/CompanyEnvironmentPreview.cs` | `Beep.OilandGas.PPDM39.DataManagement.Models` |
| `TheTechIdea.Data/OilGas/CompanyEnvironmentIdentity.cs` | `Beep.OilandGas.PPDM39.DataManagement/Models/CompanyEnvironmentIdentity.cs` | `Beep.OilandGas.PPDM39.DataManagement.Models` |

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.PPDM39.DataManagement/Beep.OilandGas.PPDM39.DataManagement.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

5 candidate incoming files; 121 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |
| `TheTechIdea.Data/OilGas/CompanySalesGLEntryOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: GL_ENTRY | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/CompanySalesJournalNativeOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: JOURNAL_ENTRY, JOURNAL_ENTRY_LINE | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderActualCostOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: ACCOUNTING_COST | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderBudgetDecisionOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: APPROVAL_WORKFLOW | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderBudgetOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: AFE | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.PPDM39.DataManagement/Core/DemoDatabaseRepository.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/HseIncidentNativeMetadata.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/PPDM39Metadata.Generated.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/PPDMMetadataService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/PPDMSqlMetadataLoader.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/PermitApplicationNativeMetadata.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/ProductionInterventionNativeMetadata.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/WorkOrderDocumentSourceMetadata.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/WorkOrderFinancialMetadata.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/WorkOrderInspectionMetadata.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/WorkOrderMaterialMetadata.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/Models/DatabaseCreation/ScriptExecutionResult.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/Models/DatabaseCreation/ScriptType.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/MigrationConnectionTarget.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleMigrationScope.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSchemaBoundary.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSchemaVerification.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSetupAbortException.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSetupBase.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSetupDiscovery.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSetupOrchestrator.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/QueryBuilder.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/ReportedFailure.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Core/Tree/PPDMTreeBuilder.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Modules/CorePpdmModule.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Modules/DemoDataModule.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Modules/SharedReferenceModule.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Modules/SystemSetupModule.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Modules/WellReferenceModule.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Modules/WellStatusFacetModule.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Repositories/IFieldMappingService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Repositories/IJurisdictionService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Repositories/IWellStatusService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/DummyData/PPDM39DummyDataGenerator.Activities.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/DummyData/PPDM39DummyDataGenerator.Facilities.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/DummyData/PPDM39DummyDataGenerator.Fields.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/DummyData/PPDM39DummyDataGenerator.Production.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/DummyData/PPDM39DummyDataGenerator.Tests.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/DummyData/PPDM39DummyDataGenerator.Wells.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/DummyData/PPDM39DummyDataGenerator.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/EnumReferenceDataSeeder.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/IPPDMSeeder.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/PPDMCSVSeeder.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/PPDMDemoDataSeeder.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/PPDMReferenceDataSeeder.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/SeedTemplateJson.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/Services/CSVLOVImporter.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/Services/IHSStandardValueImporter.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/Services/PPDMStandardValueImporter.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/Services/StandardValueMapper.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/WellStatusFacetSeeder.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Compliance/ComplianceFieldObligationMapper.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Compliance/ComplianceService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Compliance/GHGReportingService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Compliance/GhgProductionSourceMapper.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Compliance/RoyaltyCalculationService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/ConnectionProbe.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/ConnectionService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/DemoDatabaseCleanupService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/DemoDatabaseService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/FieldMappingService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/HSE/BarrierManagementService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/HSE/CorrectiveActionService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/HSE/HAZOPService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/HSE/HSEIncidentService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/HSE/HSEKPIService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/HSE/RCAService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Integrations/IntegrationHealthService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Integrations/ProdmlAdapterService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Integrations/WitsmlAdapterService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/JurisdictionService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/LOVManagementService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/PPDM39SetupService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/PPDM39WorkflowService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/PPDMDataAccessAuditService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/PPDMDataQualityDashboardService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/PPDMDataQualityService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/PPDMDataValidationService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/PPDMDataVersioningService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/PPDMDefaultValueService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/PPDMMappingService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/SchemaInstallationTracker.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Well/WellServices.FacetCatalog.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Well/WellServices.Helpers.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Well/WellServices.MaintenanceAssetReturn.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Well/WellServices.MaintenanceReturnEvidence.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Well/WellServices.Models.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Well/WellServices.WellStatus.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Well/WellServices.WellStructures.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/Well/WellServices.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.ExecutorIdentity.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelAcceptanceCore.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelAssignment.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelCredentials.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelIdentity.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelPolicy.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelRelease.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelSource.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.DocumentAcceptance.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.DocumentContributors.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.DocumentSource.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.MaterialAcceptance.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.PersonnelAcceptance.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/RecordedWorkOrderCreation.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/RecordedWorkOrderRows.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/SchedulingService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderDatabaseBinding.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.Decision.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.ExecutionPrerequisites.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.Labor.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.RecordedApproval.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.RecordedWork.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.Start.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.Submission.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.SubmissionInspections.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderRowValues.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/globalusings.cs`

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

- `Beep.OilandGas.PPDM39.DataManagement/Models/COMPANY_GHG_CATALOG_SELECTION.cs` (`Beep.OilandGas.PPDM39.DataManagement.Models`)
- `Beep.OilandGas.PPDM39.DataManagement/Models/COMPANY_OILGAS_ENVIRONMENT.cs` (`Beep.OilandGas.PPDM39.DataManagement.Models`)
- `Beep.OilandGas.PPDM39.DataManagement/Models/CompanyGhgCatalogSelection.cs` (`Beep.OilandGas.PPDM39.DataManagement.Models`)
- `Beep.OilandGas.PPDM39.DataManagement/Models/PPDMFieldEligibility.cs` (`Beep.OilandGas.PPDM39.DataManagement.Models`)
- `Beep.OilandGas.PPDM39.DataManagement/Models/SetupDriverItem.cs` (`Beep.OilandGas.PPDM39.DataManagement.Models`)
- `Beep.OilandGas.PPDM39.DataManagement/Models/SetupRepositoryConnection.cs` (`Beep.OilandGas.PPDM39.DataManagement.Models`)
- `Beep.OilandGas.PPDM39.DataManagement/Models/SetupRepositoryMigration.cs` (`Beep.OilandGas.PPDM39.DataManagement.Models`)
- `Beep.OilandGas.PPDM39.DataManagement/Models/SYSTEM_SETUP_RECORD.cs` (`Beep.OilandGas.PPDM39.DataManagement.Models`)
- `Beep.OilandGas.PPDM39.DataManagement/Models/SystemSetupConnectionSelection.cs` (`Beep.OilandGas.PPDM39.DataManagement.Models`)

## Compiler-bound migration checkpoint - 2026-11-04

Files below returned with callers and source links. Incident command interfaces use the consuming data-management Core.Interfaces layer; shared work-order/field facts stay in the existing Models contract layer. Owner builds pass, using existing dependency outputs. Full consumer/provider gates remain open. The retired Data validation bridge is removed. Current internal evidence-validation access is restricted to Accounting, ProductionAccounting and the existing Repository test assembly; it does not cache authority or datasource reads.

- `Beep.OilandGas.PPDM39.DataManagement/Core/Interfaces/ICompanyCorrectiveActionCommands.cs` (`Beep.OilandGas.PPDM39.DataManagement.Core.Interfaces`)
- `Beep.OilandGas.PPDM39.DataManagement/Core/Interfaces/ICompanyIncidentCommands.cs` (`Beep.OilandGas.PPDM39.DataManagement.Core.Interfaces`)
- `Beep.OilandGas.PPDM39.DataManagement/Models/SystemSetupConfiguration.cs` (`Beep.OilandGas.PPDM39.DataManagement.Models`)
