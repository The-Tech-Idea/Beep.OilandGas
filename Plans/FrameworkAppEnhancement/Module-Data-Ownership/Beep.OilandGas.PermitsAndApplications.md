# Beep.OilandGas.PermitsAndApplications — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

35 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

| Original source | Current definition | Namespace |
| --- | --- | --- |
| `TheTechIdea.Data/OilGas/PermitApplicationCatalog.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationCatalog.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitApplicationCommandRecovery.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationCommandRecovery.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/CreateFieldObligation.cs` | `Beep.OilandGas.PermitsAndApplications/Models/CreateFieldObligation.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitApplicationSchema.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationSchema.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/JURISDICTION_REQUIREMENTS.cs` | `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/JURISDICTION_REQUIREMENTS.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables` |
| `TheTechIdea.Data/OilGas/MIT_RESULT.cs` | `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/MIT_RESULT.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables` |
| `TheTechIdea.Data/OilGas/PERMIT_APPLICATION_COMMAND.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PERMIT_APPLICATION_COMMAND.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PERMIT_DOCUMENT_STAGE.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PERMIT_DOCUMENT_STAGE.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/REQUIRED_FORM.cs` | `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/REQUIRED_FORM.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables` |
| `TheTechIdea.Data/OilGas/R_PERMITS_REFERENCE_CODE.cs` | `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/R_PERMITS_REFERENCE_CODE.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables` |
| `TheTechIdea.Data/OilGas/PermitApplicationInboxReview.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationInboxReview.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitEnvironmentalDraftChanges.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitEnvironmentalDraftChanges.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitSupportingDraftRules.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitSupportingDraftRules.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitInjectionDraftChanges.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitInjectionDraftChanges.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitApplicationSourceReview.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationSourceReview.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitApplicationCreationReview.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationCreationReview.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitApplicationDataAccess.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationDataAccess.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitDocumentStaging.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitDocumentStaging.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitAttachmentDocument.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitAttachmentDocument.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitApplicationSupportingSnapshot.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationSupportingSnapshot.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitDrillingDraftChanges.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitDrillingDraftChanges.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitSupportingAmendments.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitSupportingAmendments.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitApplicationCommandOutcome.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationCommandOutcome.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/APPLICATION_ATTACHMENT.cs` | `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/APPLICATION_ATTACHMENT.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables` |
| `TheTechIdea.Data/OilGas/PermitApplicationDraftChanges.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationDraftChanges.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitApplicationStatusCodes.cs` | `Beep.OilandGas.PermitsAndApplications/Constants/PermitApplicationStatusCodes.cs` | `Beep.OilandGas.PermitsAndApplications.Constants` |
| `TheTechIdea.Data/OilGas/PermitApplicationSupportingRecords.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationSupportingRecords.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PermitApplicationWorkflow.cs` | `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationWorkflow.cs` | `Beep.OilandGas.PermitsAndApplications.Models` |
| `TheTechIdea.Data/OilGas/PERMIT_STATUS_HISTORY.cs` | `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/PERMIT_STATUS_HISTORY.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables` |
| `TheTechIdea.Data/OilGas/DRILLING_PERMIT_APPLICATION.cs` | `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/DRILLING_PERMIT_APPLICATION.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables` |
| `TheTechIdea.Data/OilGas/ENVIRONMENTAL_PERMIT_APPLICATION.cs` | `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/ENVIRONMENTAL_PERMIT_APPLICATION.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables` |
| `TheTechIdea.Data/OilGas/INJECTION_PERMIT_APPLICATION.cs` | `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/INJECTION_PERMIT_APPLICATION.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables` |
| `TheTechIdea.Data/OilGas/PERMIT_APPLICATION.cs` | `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/PERMIT_APPLICATION.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables` |
| `TheTechIdea.Data/OilGas/PermitApplicationStatus.cs` | `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/PermitApplicationStatus.cs` | `Beep.OilandGas.Models.Data.PermitsAndApplications` |
| `TheTechIdea.Data/OilGas/PermitApplicationType.cs` | `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/PermitApplicationType.cs` | `Beep.OilandGas.Models.Data.PermitsAndApplications` |

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.PermitsAndApplications/Beep.OilandGas.PermitsAndApplications.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

16 candidate incoming files; 60 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |
| `TheTechIdea.Data/OilGas/APPLICATION_ATTACHMENT.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables`: APPLICATION_ATTACHMENT | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/APPLICATION_ATTACHMENT.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/Country.cs` | `Beep.OilandGas.Models.Data.PermitsAndApplications`: Country | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/Country.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/DRILLING_PERMIT_APPLICATION.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables`: DRILLING_PERMIT_APPLICATION | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/DRILLING_PERMIT_APPLICATION.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ENVIRONMENTAL_PERMIT_APPLICATION.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables`: ENVIRONMENTAL_PERMIT_APPLICATION | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/ENVIRONMENTAL_PERMIT_APPLICATION.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/INJECTION_PERMIT_APPLICATION.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables`: INJECTION_PERMIT_APPLICATION | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/INJECTION_PERMIT_APPLICATION.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/JURISDICTION_REQUIREMENTS.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables`: JURISDICTION_REQUIREMENTS | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/JURISDICTION_REQUIREMENTS.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/MIT_RESULT.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables`: MIT_RESULT | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/MIT_RESULT.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/PERMIT_APPLICATION.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables`: PERMIT_APPLICATION | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/PERMIT_APPLICATION.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/PERMIT_STATUS_HISTORY.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables`: PERMIT_STATUS_HISTORY | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/PERMIT_STATUS_HISTORY.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/PermitApplicationStatus.cs` | `Beep.OilandGas.Models.Data.PermitsAndApplications`: PermitApplicationStatus | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/PermitApplicationStatus.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/PermitApplicationStatusCodes.cs` | `Beep.OilandGas.PermitsAndApplications.Constants`: PermitApplicationStatusCodes | Beep.OilandGas.PermitsAndApplications/Constants/PermitApplicationStatusCodes.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/PermitApplicationType.cs` | `Beep.OilandGas.Models.Data.PermitsAndApplications`: PermitApplicationType | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/PermitApplicationType.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/R_PERMITS_REFERENCE_CODE.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables`: R_PERMITS_REFERENCE_CODE | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/R_PERMITS_REFERENCE_CODE.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/RegulatoryAuthority.cs` | `Beep.OilandGas.Models.Data.PermitsAndApplications`: RegulatoryAuthority | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/RegulatoryAuthority.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/REQUIRED_FORM.cs` | `Beep.OilandGas.PermitsAndApplications.Data.PermitTables`: REQUIRED_FORM | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Tables/REQUIRED_FORM.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/StateProvince.cs` | `Beep.OilandGas.Models.Data.PermitsAndApplications`: StateProvince | Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/StateProvince.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.PermitsAndApplications/Constants/PermitConstants.cs`
- `Beep.OilandGas.PermitsAndApplications/Core/Interfaces/IPermitApplicationLifecycleService.cs`
- `Beep.OilandGas.PermitsAndApplications/Core/Interfaces/IPermitApplicationWorkflowService.cs`
- `Beep.OilandGas.PermitsAndApplications/Core/Interfaces/IPermitComplianceCheckService.cs`
- `Beep.OilandGas.PermitsAndApplications/Core/Interfaces/IPermitComplianceReportService.cs`
- `Beep.OilandGas.PermitsAndApplications/Core/Interfaces/IPermitStatusHistoryService.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/ApplicationBusinessAssociate.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/ApplicationDescription.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/ApplicationRemark.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/BusinessAssociatePermit.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/ComplianceReport.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/ComplianceStatusCount.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/ExpiringPermitRecord.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/FacilityLicenseInfo.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/PermitApplication.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/PermitComplianceResult.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/PermitRequirements.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/PermitValidationResult.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/WellLicense.cs`
- `Beep.OilandGas.PermitsAndApplications/Data/PermitsAndApplications/Projections/WellPermitTypeInfo.cs`
- `Beep.OilandGas.PermitsAndApplications/DataMapping/JurisdictionHelper.cs`
- `Beep.OilandGas.PermitsAndApplications/DataMapping/WellLicenseMapper.cs`
- `Beep.OilandGas.PermitsAndApplications/Exceptions/PermitException.cs`
- `Beep.OilandGas.PermitsAndApplications/Forms/JurisdictionConfig.cs`
- `Beep.OilandGas.PermitsAndApplications/Forms/JurisdictionConfigLoader.cs`
- `Beep.OilandGas.PermitsAndApplications/Forms/PermitFormPacketWriter.cs`
- `Beep.OilandGas.PermitsAndApplications/Forms/PermitFormPayloadBuilder.cs`
- `Beep.OilandGas.PermitsAndApplications/Forms/PermitFormRenderContext.cs`
- `Beep.OilandGas.PermitsAndApplications/Forms/PermitFormRenderer.cs`
- `Beep.OilandGas.PermitsAndApplications/Forms/PermitFormTemplateRegistry.cs`
- `Beep.OilandGas.PermitsAndApplications/Modules/PermitsModule.cs`
- `Beep.OilandGas.PermitsAndApplications/Services/PermitApplicationLifecycleService.AttachmentContent.cs`
- `Beep.OilandGas.PermitsAndApplications/Services/PermitApplicationLifecycleService.Commands.cs`
- `Beep.OilandGas.PermitsAndApplications/Services/PermitApplicationLifecycleService.DocumentVocabulary.cs`
- `Beep.OilandGas.PermitsAndApplications/Services/PermitApplicationLifecycleService.Documents.cs`
- `Beep.OilandGas.PermitsAndApplications/Services/PermitApplicationLifecycleService.cs`
- `Beep.OilandGas.PermitsAndApplications/Services/PermitApplicationWorkflowService.cs`
- `Beep.OilandGas.PermitsAndApplications/Services/PermitComplianceCheckService.cs`
- `Beep.OilandGas.PermitsAndApplications/Services/PermitComplianceReportService.cs`
- `Beep.OilandGas.PermitsAndApplications/Services/PermitStatusHistoryService.cs`
- `Beep.OilandGas.PermitsAndApplications/Services/PermitStatusTransitionRules.cs`
- `Beep.OilandGas.PermitsAndApplications/Services/PermitsServiceBase.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/PermitFieldValueResolver.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/PermitValidationEngine.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/PermitValidationRequest.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/PermitValidationRulesFactory.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/AerDrillingRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/AerEnvironmentalRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/BcerDrillingRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/BcerEnvironmentalRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/BoemDrillingRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/BseeEnvironmentalRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/RequiredFieldRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/RequiredFormsRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/RrcDrillingRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/RrcEnvironmentalRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/RrcInjectionRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/StatusDateRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/TceqEnvironmentalRule.cs`
- `Beep.OilandGas.PermitsAndApplications/Validation/Rules/TemplateRequiredFieldsRule.cs`

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

## Domain migration checkpoint - 2026-11-04

Applied owner namespace moves below; copies removed from Data, callers and focused source links updated. Owner builds are recorded in restoration-progress.json. Direct owner builds using existing dependency outputs do not prove clean integrated builds, API/Web or provider behavior.

- `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationCatalog.cs`: `Beep.OilandGas.PermitsAndApplications.Models`
- `Beep.OilandGas.PermitsAndApplications/Models/PermitApplicationCommandRecovery.cs`: `Beep.OilandGas.PermitsAndApplications.Models`

## Dependency-group implementation - 2026-11-04

Current-owner files below migrated with namespace/caller/source-link updates. Shared contract folders follow existing feature conventions. Duplicate import result/error definitions consolidated into existing plain transport models; no forwarded or parallel result types retained. Exact build/runtime scope and integration failures are recorded in restoration-progress.json.

- `Beep.OilandGas.PermitsAndApplications/Models/CreateFieldObligation.cs` (`Beep.OilandGas.PermitsAndApplications.Models`)
