# Beep.OilandGas.LifeCycle — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

4 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

| Original source | Current definition | Namespace |
| --- | --- | --- |
| `TheTechIdea.Data/OilGas/ProcessStartSourceReview.cs` | `Beep.OilandGas.LifeCycle/Models/ProcessStartSourceReview.cs` | `Beep.OilandGas.LifeCycle.Models` |
| `TheTechIdea.Data/OilGas/ProcessTransitionReview.cs` | `Beep.OilandGas.LifeCycle/Models/ProcessTransitionReview.cs` | `Beep.OilandGas.LifeCycle.Models` |
| `TheTechIdea.Data/OilGas/WorkflowInboxReview.cs` | `Beep.OilandGas.LifeCycle/Models/WorkflowInboxReview.cs` | `Beep.OilandGas.LifeCycle.Models` |
| `TheTechIdea.Data/OilGas/WorkflowTaskRoute.cs` | `Beep.OilandGas.LifeCycle/Models/WorkflowTaskRoute.cs` | `Beep.OilandGas.LifeCycle.Models` |

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.LifeCycle/Beep.OilandGas.LifeCycle.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

0 candidate incoming files; 172 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.LifeCycle/Core/Interfaces/IApprovalWorkflowEngine.cs`
- `Beep.OilandGas.LifeCycle/Core/Interfaces/IAuditChainService.cs`
- `Beep.OilandGas.LifeCycle/Core/Interfaces/IFieldOrchestrator.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/BUSINESS_EVENT_TRIGGER.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/COMPENSATING_CONTROL.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/CROSS_PERSONA_TASK.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/DELEGATION_OF_AUTHORITY.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/PROCESS_APPROVAL.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/PROCESS_DEFINITION.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/PROCESS_HISTORY.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/PROCESS_INSTANCE.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/PROCESS_STEP_INSTANCE.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/ROLE_HANDOFF_CONTRACT.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/R_LIFECYCLE_STATE_REFERENCE.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/SOD_CONFLICT.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/SOD_RULE.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/WORKFLOW_DEPENDENCY.cs`
- `Beep.OilandGas.LifeCycle/Data/Tables/WORKFLOW_VERSION.cs`
- `Beep.OilandGas.LifeCycle/Definitions/AccountingStandardsWorkflowSeed.PhaseBCD.cs`
- `Beep.OilandGas.LifeCycle/Definitions/AccountingStandardsWorkflowSeed.cs`
- `Beep.OilandGas.LifeCycle/Definitions/CrossRoleWorkflowSeed.AssetLifecycle.cs`
- `Beep.OilandGas.LifeCycle/Definitions/CrossRoleWorkflowSeed.Finance.cs`
- `Beep.OilandGas.LifeCycle/Definitions/CrossRoleWorkflowSeed.HSECompliance.cs`
- `Beep.OilandGas.LifeCycle/Definitions/CrossRoleWorkflowSeed.Operations.cs`
- `Beep.OilandGas.LifeCycle/Definitions/OperationalAccountingWorkflowSeed.cs`
- `Beep.OilandGas.LifeCycle/DependencyInjection/LifeCycleServiceCollectionExtensions.cs`
- `Beep.OilandGas.LifeCycle/Modules/AccountingModuleSetup.cs`
- `Beep.OilandGas.LifeCycle/Modules/LifeCycleModule.cs`
- `Beep.OilandGas.LifeCycle/Services/AccessControl/AssetHierarchyService.cs`
- `Beep.OilandGas.LifeCycle/Services/AccessControl/UserAssetAccessService.cs`
- `Beep.OilandGas.LifeCycle/Services/Accounting/PPDMAccountingService.CostAllocation.cs`
- `Beep.OilandGas.LifeCycle/Services/Accounting/PPDMAccountingService.RoyaltyReporting.cs`
- `Beep.OilandGas.LifeCycle/Services/Accounting/PPDMAccountingService.cs`
- `Beep.OilandGas.LifeCycle/Services/Accounting/RunTicketVolumeReconciler.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.ArtificialLift.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.CRUD.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.Common.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.DCA.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.DataRetrieval.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.Economics.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.Facilities.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.Flash.Units.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.Flash.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.Nodal.Helpers.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.Nodal.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.PropertyHelpers.Forecasting.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.PropertyHelpers.Pool.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.PropertyHelpers.Production.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.PropertyHelpers.WellTest.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.Results.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.WellTest.cs`
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/CompressorAnalysisMapper.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/DCAMapper.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/FlashCalculationsMapper.EquationOfState.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/FlashCalculationsMapper.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/GasLiftMapper.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/HydraulicPumpsMapper.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/IPPDM39Mapper.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/NodalAnalysisMapper.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/PipelineAnalysisMapper.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/PlungerLiftMapper.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/ProductionForecastingMapper.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/PumpPerformanceMapper.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/SuckerRodPumpingMapper.cs`
- `Beep.OilandGas.LifeCycle/Services/DataMapping/WellTestAnalysisMapper.cs`
- `Beep.OilandGas.LifeCycle/Services/Decommissioning/PPDMDecommissioningService.cs`
- `Beep.OilandGas.LifeCycle/Services/Decommissioning/Processes/DecommissioningProcessService.cs`
- `Beep.OilandGas.LifeCycle/Services/Development/PPDMDevelopmentService.cs`
- `Beep.OilandGas.LifeCycle/Services/Development/Processes/DevelopmentProcessService.cs`
- `Beep.OilandGas.LifeCycle/Services/Exploration/LeadExplorationService.cs`
- `Beep.OilandGas.LifeCycle/Services/Exploration/Processes/ExplorationProcessService.WorkflowStepPrerequisites.cs`
- `Beep.OilandGas.LifeCycle/Services/Exploration/Processes/ExplorationProcessService.cs`
- `Beep.OilandGas.LifeCycle/Services/Exploration/Processes/ExplorationWorkflowPrerequisiteException.cs`
- `Beep.OilandGas.LifeCycle/Services/FacilityManagement/FacilityManagementService.cs`
- `Beep.OilandGas.LifeCycle/Services/FacilityManagement/Processes/FacilityManagementProcessService.cs`
- `Beep.OilandGas.LifeCycle/Services/FieldLifecycle/FieldLifecycleService.cs`
- `Beep.OilandGas.LifeCycle/Services/FieldManagement/FieldManagementService.cs`
- `Beep.OilandGas.LifeCycle/Services/FieldOrchestrator.cs`
- `Beep.OilandGas.LifeCycle/Services/HSE/PPDMHSEService.cs`
- `Beep.OilandGas.LifeCycle/Services/Inspection/InspectionManagementService.cs`
- `Beep.OilandGas.LifeCycle/Services/Inspection/Processes/InspectionProcessService.cs`
- `Beep.OilandGas.LifeCycle/Services/Integration/AnalysisResultStorage.cs`
- `Beep.OilandGas.LifeCycle/Services/Integration/DataFlowService.cs`
- `Beep.OilandGas.LifeCycle/Services/Maintenance/MaintenanceManagementService.cs`
- `Beep.OilandGas.LifeCycle/Services/Maintenance/Processes/MaintenanceProcessService.cs`
- `Beep.OilandGas.LifeCycle/Services/Operations/OperationsManagementService.cs`
- `Beep.OilandGas.LifeCycle/Services/Operations/Processes/OperationsProcessService.cs`
- `Beep.OilandGas.LifeCycle/Services/PipelineManagement/PipelineManagementService.cs`
- `Beep.OilandGas.LifeCycle/Services/PipelineManagement/Processes/PipelineManagementProcessService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/ApprovalWorkflowEngine.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/BoundProcessService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/BusinessEventTriggerService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/ComplianceReportService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/ConditionalTransition.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/CrossPersonaTaskRouter.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/DevelopmentReviewDefinition.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/DoAEvaluationService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/DomainEventPublisher.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/DynamicRoutingService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/EscalationActionService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/HandoffValidationService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/ICrossPersonaTaskRouter.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/IRolePersonaReader.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/IncidentReportingDefinition.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/MultiEntityWorkflowOrchestrator.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.CompanyStart.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.CompanyStep.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.CompletionDecision.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.CompletionReview.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.CompletionReviewDecision.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.CompletionReviewProgress.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.DevelopmentDecision.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.DevelopmentRead.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.DevelopmentStart.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.ExecutionAssignment.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.ExecutionAssignmentConflict.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.HseClosure.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.HseCorrection.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.HseStart.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.InvestmentHandoff.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.InvestmentProgress.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.InvestmentStage.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceAcceptance.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceAssetReturnSource.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceAssignedStage.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceClosure.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceExecutionHandoff.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceGraphDelta.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceOwnedAudit.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceReturnHandoff.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceReturnReview.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceStart.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.ProgramDecision.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.ProgramProgress.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.ProgramReview.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.ReportingClosure.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.ReportingReassignment.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.ReportingTransfer.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.ReportingTransferPersistence.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.ReturnAcceptance.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.ReviewStagePersistence.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.StartPersistence.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/ProcessDefinitionInitializer.AssetLifecycles.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/ProcessDefinitionInitializer.HSECompliance.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/ProcessDefinitionInitializer.WorkOrders.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/ProcessDefinitionInitializer.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/ProcessIntegrationHelper.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/ProcessServiceBase.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/ProcessStateMachine.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/ProcessValidator.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/ReportTemplateService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/RoleBasedAccessService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/SlaMonitorService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/SlaTrackingService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/SodEvaluationEngine.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/WorkflowDependencyGraphService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/WorkflowTemplateService.cs`
- `Beep.OilandGas.LifeCycle/Services/Processes/WorkflowVersioningService.cs`
- `Beep.OilandGas.LifeCycle/Services/Production/PPDMProductionService.InterventionDecisions.cs`
- `Beep.OilandGas.LifeCycle/Services/Production/PPDMProductionService.cs`
- `Beep.OilandGas.LifeCycle/Services/Production/Processes/ProductionProcessService.cs`
- `Beep.OilandGas.LifeCycle/Services/ReportedFailure.cs`
- `Beep.OilandGas.LifeCycle/Services/ReservoirLifecycle/ReservoirLifecycleService.cs`
- `Beep.OilandGas.LifeCycle/Services/Seeding/AccountingReferenceSeedService.cs`
- `Beep.OilandGas.LifeCycle/Services/Seeding/LifeCycleSeedService.cs`
- `Beep.OilandGas.LifeCycle/Services/WellComparisonService.cs`
- `Beep.OilandGas.LifeCycle/Services/WellLifecycle/WellLifecycleService.cs`
- `Beep.OilandGas.LifeCycle/Services/WellManagement/Processes/WellManagementProcessService.cs`
- `Beep.OilandGas.LifeCycle/Services/WellManagement/WellManagementService.cs`
- `Beep.OilandGas.LifeCycle/Services/WorkOrder/WorkOrderManagementService.cs`

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

- `Beep.OilandGas.LifeCycle/Models/ProcessStartSourceReview.cs` (`Beep.OilandGas.LifeCycle.Models`)
- `Beep.OilandGas.LifeCycle/Models/ProcessTransitionReview.cs` (`Beep.OilandGas.LifeCycle.Models`)
- `Beep.OilandGas.LifeCycle/Models/WorkflowInboxReview.cs` (`Beep.OilandGas.LifeCycle.Models`)

## Compiler-bound migration checkpoint - 2026-11-04

Files below returned with callers and source links. Incident command interfaces use the consuming data-management Core.Interfaces layer; shared work-order/field facts stay in the existing Models contract layer. Owner builds pass, using existing dependency outputs. Full consumer/provider gates remain open. The retired Data validation bridge is removed. Current internal evidence-validation access is restricted to Accounting, ProductionAccounting and the existing Repository test assembly; it does not cache authority or datasource reads.

- `Beep.OilandGas.LifeCycle/Models/WorkflowTaskRoute.cs` (`Beep.OilandGas.LifeCycle.Models`)
