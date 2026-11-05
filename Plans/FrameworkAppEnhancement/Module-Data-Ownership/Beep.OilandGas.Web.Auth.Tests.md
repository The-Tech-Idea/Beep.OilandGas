# Beep.OilandGas.Web.Auth.Tests — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

0 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

This project consumes changed contracts; no original Data definitions are assigned here. Caller checks remain in scope.

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.Web.Auth.Tests/Beep.OilandGas.Web.Auth.Tests.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

0 candidate incoming files; 249 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.Web.Auth.Tests/APInvoiceSourceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/APLinePurchaseSourceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/AccountingPeriodCheckTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/AssetAccessLevelChecksTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CommercialLandPortfolioTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyAccountingHandoffTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyAccountingProcessStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyAssetReviewNavigationTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyEffectiveMappingSelectionTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyEmploymentEligibilityTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyEnvironmentIndicatorTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyEnvironmentSetupTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyEnvironmentSourceReviewValidatorTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyHseWorkflowClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyHseWorkflowIntentStoreTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyMaintenanceReturnPolicyClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyMaintenanceReturnPolicyPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyMembershipSetupTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyPersonaContextTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyPersonnelIdentityClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyPersonnelIdentityPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyPersonnelPolicyClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyPersonnelPolicyPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanySelectorIdentityTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CompanyWellQualityTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ComplianceCalendarTimeTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ConnectionEditTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ConstructionEvidenceScreenTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CorrectivePlanWorkspaceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/CreateFieldObligationPageTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DataAnalystWorkspaceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentAfeStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentAssignedReviewStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentDraftIntentStoreTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentDraftWorkspaceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentFacilityEditContractTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentFacilityEditJournalTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentFacilityEvidenceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentInvestmentAssignedStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentInvestmentHandoffStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentInvestmentProgressScreenTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentInvestmentRoutingStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentInvestmentStageScreenTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentInvestmentStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentLaunchConfigurationTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentPlanReleaseStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentReviewDecisionStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentReviewProgressStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentReviewWorkspaceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DevelopmentWellEvidenceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingAssignedCompletionStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingCompletionAcceptanceStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingCompletionAssignedStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingCompletionDecisionStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingCompletionReportStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingCompletionRoutingStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingCompletionSubmissionStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingCompletionTestInspectionStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingDailyReportConflictStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingDailyReportCorrectionStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingDailyReportPageStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingDailyReportStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingExecutionAssignedStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingExecutionAssignmentConflictStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingExecutionAssignmentStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingExecutionConflictStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingExecutionStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingProgramAssignedReviewStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingProgramDraftStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingProgramReviewDecisionStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingProgramReviewProgressStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingProgramReviewScreenTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingProgramReviewStartStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingReportingClosureStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingReportingReassignmentStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/DrillingWorkflowBrowserFixture.cs`
- `Beep.OilandGas.Web.Auth.Tests/EngineeringWorkspaceLandingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ExecutivePortfolioLandingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ExecutivePortfolioTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ExplorationPersonaLandingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/FacilityCreationIntentTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/FacilityEditIntentTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/FieldDashboardStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/FieldEngineerWorkspaceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/FieldSelectionTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/FinancialCatalogRenderingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/FinancialProcessCatalogTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/FirstRunNavigationGateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/FirstRunSetupGuideTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/Fixtures/HierarchyConfigurationRenderHost.razor`
- `Beep.OilandGas.Web.Auth.Tests/Fixtures/MaintenanceLandingRouteHost.razor`
- `Beep.OilandGas.Web.Auth.Tests/Fixtures/MaintenanceProcessRouteHost.razor`
- `Beep.OilandGas.Web.Auth.Tests/Fixtures/WorkOrderDetailRouteHost.razor`
- `Beep.OilandGas.Web.Auth.Tests/Fixtures/WorkOrderInspectionRenderHost.razor`
- `Beep.OilandGas.Web.Auth.Tests/GHGReportingAvailabilityTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/GhgFactorSetupTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/HseAssignedActionsTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/HseClosureAuthorityTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/HseClosureCleanupTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/HseClosureProgressTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/HseClosureProtectedCleanupTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/HseClosureReasonValidationTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/HseIncidentScreenTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/HseLandingAuthorityTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/HsePersonaLandingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/HseWorkflowStartTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/InitialCompanySetupTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/InventoryLinkCycleScreenTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/InventoryLinkCycleWorkspaceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/InventoryLinkDecisionRenderingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/InventoryLinkDecisionScreenTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/InventoryLinkDecisionWorkspaceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/InventoryLinkPreparationScreenTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/InventoryLinkPreparationWorkspaceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/InventoryWorkspaceStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/InvestigatorAssignmentScreenTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/JournalSourceReviewPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/LandingWorkspaceRefreshTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaintenanceAssetEvidenceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaintenanceAssetReturnStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaintenanceAssetReturnStatusTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaintenanceAssignedStageClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaintenanceClosureStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaintenanceClosureStatusTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaintenanceHandoffClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaintenanceHandoffPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaintenanceInspectionClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaintenanceReceivingPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaintenanceRecordedWorkLandingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaintenanceRecordedWorkTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaterialReservationLocalTimeTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/MaterialReservationQueueTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ModuleDatabaseClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/PermitApplicationReadWorkflowTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/PersonaClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/PersonaInboxClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/PersonaTaskInboxStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/PersonaWorkspaceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ProcessAnalyticsUnavailableTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ProcessDefinitionDetailStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ProcessDefinitionRenderingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ProcessInstanceDetailStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ProcessTransitionResponseTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ProcurementStockCoordinationTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ProductionDashboardStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ProductionInterventionDecisionStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ProductionInterventionEvidenceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ProductionInterventionPageTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ProductionInterventionSourceReviewTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ProductionPersonaRouteTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/PurchaseOrderSourceRenderingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/PurchaseOrderWorkspaceStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ReceiptInventoryScreenTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ReceiptInventoryWorkspaceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ReceiptStockScreenTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/ReceiptStockWorkspaceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/RecordedProcessRenderingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/RecordedWorkOrderCreationTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/RecordedWorkOrderOverviewTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/RecordedWorkOrderRowsTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/RecordingFailureReporter.cs`
- `Beep.OilandGas.Web.Auth.Tests/RepositoryAccountTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/RepositorySetupPageTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/RepositoryStartupTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/RoyaltyDetailsTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/SetupDriversTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/SignInSurfaceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/SystemSetupRepositoryTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/TestCompany.cs`
- `Beep.OilandGas.Web.Auth.Tests/TestFailures.cs`
- `Beep.OilandGas.Web.Auth.Tests/UserAdministrationStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/UserAssetAccessAdmissionTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WarehouseScreenTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WellComparisonStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WellTestMappingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WellTestRecordedDetailTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WellTestReviewStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderAcceptedMaterialReviewTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderAccessClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderAssignedPersonnelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderBudgetClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderBudgetDecisionClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderBudgetDecisionPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderBudgetDecisionStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderBudgetDecisionStatusPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderBudgetDecisionStatusTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderBudgetPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderBudgetStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderCompletionDecisionStatusTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderCompletionDecisionTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderCompletionReviewTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderCreateStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderDatabaseBindingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderDetailStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderExecutionHandoffStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderExecutionPermitApprovalClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderExecutionPermitApprovalPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderExecutionPermitClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderExecutionPermitDecisionClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderExecutionPermitIssuerTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderExecutionPermitStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderExecutionStartStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderFinancialContractOwnershipTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderFinancialWorkspaceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderGrantAdministrationTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderInspectionCommandTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderInspectionRenderingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderInspectionSchemaTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderInterventionDetailTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderLaborCostClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderLaborCostPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderLaborCostStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderLaborUsageStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderListStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaintenanceReturnReviewTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialAcceptanceClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialAcceptancePanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialAcceptanceStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialAllocationClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialAllocationPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialAllocationStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialCatalogTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialCostClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialCostPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialCostStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialReadinessTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialSourcePanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialSourceTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderMaterialUsageStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderParentAccessTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderPermitDocumentSourceClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderPermitDocumentSourcePanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderPermitMaterialReadinessPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderPersonnelAcceptanceClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderPersonnelAcceptanceStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderPersonnelReleaseClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderPersonnelSourceClientTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderPersonnelSourcePanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderProcessReviewTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderQueueRenderingTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderReturnAcceptanceFixture.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderReturnAcceptanceStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderReturnAcceptanceStatusTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderReturnHandoffStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderSourceChecksTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderSourceReviewPanelTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderSubmissionStateTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkOrderWorkflowTests.cs`
- `Beep.OilandGas.Web.Auth.Tests/WorkflowAdminWorkspaceTests.cs`

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
