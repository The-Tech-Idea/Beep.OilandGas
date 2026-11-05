# Beep.OilandGas.ProductionOperations — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

11 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

| Original source | Current definition | Namespace |
| --- | --- | --- |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceExecutionHandoff.cs` | `Beep.OilandGas.ProductionOperations/Models/CompanyMaintenanceExecutionHandoff.cs` | `Beep.OilandGas.ProductionOperations.Models` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceReturnHandoff.cs` | `Beep.OilandGas.ProductionOperations/Models/CompanyMaintenanceReturnHandoff.cs` | `Beep.OilandGas.ProductionOperations.Models` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderFinancialSources.cs` | `Beep.OilandGas.ProductionOperations/Models/CompanyWorkOrderFinancialSources.cs` | `Beep.OilandGas.ProductionOperations.Models` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderMaterialCatalog.cs` | `Beep.OilandGas.ProductionOperations/Models/CompanyWorkOrderMaterialCatalog.cs` | `Beep.OilandGas.ProductionOperations.Models` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderMaterialReleaseRecovery.cs` | `Beep.OilandGas.ProductionOperations/Models/CompanyWorkOrderMaterialReleaseRecovery.cs` | `Beep.OilandGas.ProductionOperations.Models` |
| `TheTechIdea.Data/OilGas/WorkOrderProcessReview.cs` | `Beep.OilandGas.ProductionOperations/Models/WorkOrderProcessReview.cs` | `Beep.OilandGas.ProductionOperations.Models` |
| `TheTechIdea.Data/OilGas/CompanyMaterialReservationQueue.cs` | `Beep.OilandGas.ProductionOperations/Models/CompanyMaterialReservationQueue.cs` | `Beep.OilandGas.ProductionOperations.Models` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderActualCostProof.cs` | `Beep.OilandGas.ProductionOperations/Models/CompanyWorkOrderActualCostProof.cs` | `Beep.OilandGas.ProductionOperations.Models` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderActualCostReview.cs` | `Beep.OilandGas.ProductionOperations/Models/CompanyWorkOrderActualCostReview.cs` | `Beep.OilandGas.ProductionOperations.Models` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderMaterialCost.cs` | `Beep.OilandGas.ProductionOperations/Models/CompanyWorkOrderMaterialCost.cs` | `Beep.OilandGas.ProductionOperations.Models` |
| `TheTechIdea.Data/OilGas/WorkOrderSourceReview.cs` | `Beep.OilandGas.ProductionOperations/Models/WorkOrderSourceReview.cs` | `Beep.OilandGas.ProductionOperations.Models` |

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.ProductionOperations/Beep.OilandGas.ProductionOperations.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

5 candidate incoming files; 25 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |
| `TheTechIdea.Data/OilGas/WORK_ORDER_EXECUTION_PERMIT.cs` | `Beep.OilandGas.Models.Data.WorkOrder`: WORK_ORDER_EXECUTION_PERMIT | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/WORK_ORDER_INSPECTION.cs` | `Beep.OilandGas.Models.Data.WorkOrder`: WORK_ORDER_INSPECTION | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/WorkOrderInspectionRules.cs` | `Beep.OilandGas.Models.Data.WorkOrder`: AddWorkOrderInspectionRequirementRequest, WorkOrderInspectionApplicability, WorkOrderInspectionRules, WorkOrderPermitPrerequisiteKinds | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/WorkOrderSchema.cs` | `Beep.OilandGas.Models.Data.WorkOrder`: WorkOrderSchema | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/WorkOrderWorkflow.cs` | `Beep.OilandGas.Models.Data.WorkOrder`: ConditionType, InspectionCondition, WorkOrderState, WorkOrderSubType, WorkOrderWorkflow | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.ProductionOperations/Data/Tables/FACILITY_EQUIPMENT_ACTIVITY.Table.cs`
- `Beep.OilandGas.ProductionOperations/Data/Tables/FACILITY_MEASUREMENT.Table.cs`
- `Beep.OilandGas.ProductionOperations/Data/Tables/R_FACILITY_MONITORING_CODE.Table.cs`
- `Beep.OilandGas.ProductionOperations/Modules/FacilityManagementModuleSetup.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.DevelopmentCreation.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.DevelopmentCreationWrite.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.DevelopmentEdit.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.DevelopmentEditWrite.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.MaintenanceAssetReturn.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.MaintenanceAssetReturnWrite.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.MaintenanceIdentity.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.MaintenanceLicensing.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.MaintenanceOperating.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.MaintenancePolicySources.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.MaintenanceReturnParticipant.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.MaintenanceReturnPolicy.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.Operations.cs`
- `Beep.OilandGas.ProductionOperations/Services/FacilityManagementService.cs`
- `Beep.OilandGas.ProductionOperations/Services/IProductionManagementService.cs`
- `Beep.OilandGas.ProductionOperations/Services/IProductionOperationsAdvancedService.cs`
- `Beep.OilandGas.ProductionOperations/Services/IProductionOperationsService.cs`
- `Beep.OilandGas.ProductionOperations/Services/ProductionManagementService.PdenFacilityQueries.cs`
- `Beep.OilandGas.ProductionOperations/Services/ProductionManagementService.cs`
- `Beep.OilandGas.ProductionOperations/Services/ProductionOperationsService.ModelsCoreImpl.cs`
- `Beep.OilandGas.ProductionOperations/Services/ProductionOperationsService.cs`

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

- `Beep.OilandGas.ProductionOperations/Models/CompanyMaintenanceExecutionHandoff.cs`: `Beep.OilandGas.ProductionOperations.Models`
- `Beep.OilandGas.ProductionOperations/Models/CompanyMaintenanceReturnHandoff.cs`: `Beep.OilandGas.ProductionOperations.Models`
- `Beep.OilandGas.ProductionOperations/Models/CompanyWorkOrderFinancialSources.cs`: `Beep.OilandGas.ProductionOperations.Models`
- `Beep.OilandGas.ProductionOperations/Models/CompanyWorkOrderMaterialCatalog.cs`: `Beep.OilandGas.ProductionOperations.Models`
- `Beep.OilandGas.ProductionOperations/Models/CompanyWorkOrderMaterialReleaseRecovery.cs`: `Beep.OilandGas.ProductionOperations.Models`
- `Beep.OilandGas.ProductionOperations/Models/WorkOrderProcessReview.cs`: `Beep.OilandGas.ProductionOperations.Models`

## Dependency-group implementation - 2026-11-04

Current-owner files below migrated with namespace/caller/source-link updates. Shared contract folders follow existing feature conventions. Duplicate import result/error definitions consolidated into existing plain transport models; no forwarded or parallel result types retained. Exact build/runtime scope and integration failures are recorded in restoration-progress.json.

- `Beep.OilandGas.ProductionOperations/Models/CompanyMaterialReservationQueue.cs` (`Beep.OilandGas.ProductionOperations.Models`)
- `Beep.OilandGas.ProductionOperations/Models/CompanyWorkOrderActualCostProof.cs` (`Beep.OilandGas.ProductionOperations.Models`)
- `Beep.OilandGas.ProductionOperations/Models/CompanyWorkOrderActualCostReview.cs` (`Beep.OilandGas.ProductionOperations.Models`)

## Compiler-bound migration checkpoint - 2026-11-04

Files below returned with callers and source links. Incident command interfaces use the consuming data-management Core.Interfaces layer; shared work-order/field facts stay in the existing Models contract layer. Owner builds pass, using existing dependency outputs. Full consumer/provider gates remain open. The retired Data validation bridge is removed. Current internal evidence-validation access is restricted to Accounting, ProductionAccounting and the existing Repository test assembly; it does not cache authority or datasource reads.

- `Beep.OilandGas.ProductionOperations/Models/CompanyWorkOrderMaterialCost.cs` (`Beep.OilandGas.ProductionOperations.Models`)
- `Beep.OilandGas.ProductionOperations/Models/WorkOrderSourceReview.cs` (`Beep.OilandGas.ProductionOperations.Models`)
