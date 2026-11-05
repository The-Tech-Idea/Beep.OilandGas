# Beep.OilandGas.Models — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

285 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

| Original source | Current definition | Namespace |
| --- | --- | --- |
| `TheTechIdea.Data/OilGas/AccessCheckRequest.cs` | `Beep.OilandGas.Models/Data/AccessControl/AccessCheckRequest.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/AccessControlRequests.cs` | `Beep.OilandGas.Models/Data/AccessControl/AccessControlRequests.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/AR_CREDIT_MEMO.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/AR_CREDIT_MEMO.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/GenericEntityRequest.cs` | `Beep.OilandGas.Models/Data/DataManagement/GenericEntityRequest.cs` | `Beep.OilandGas.Models.Data.DataManagement` |
| `TheTechIdea.Data/OilGas/GenericEntityResponse.cs` | `Beep.OilandGas.Models/Data/DataManagement/GenericEntityResponse.cs` | `Beep.OilandGas.Models.Data.DataManagement` |
| `TheTechIdea.Data/OilGas/GetAssetHierarchyRequest.cs` | `Beep.OilandGas.Models/Data/AccessControl/GetAssetHierarchyRequest.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/GetEntitiesRequest.cs` | `Beep.OilandGas.Models/Data/DataManagement/GetEntitiesRequest.cs` | `Beep.OilandGas.Models.Data.DataManagement` |
| `TheTechIdea.Data/OilGas/GetEntitiesResponse.cs` | `Beep.OilandGas.Models/Data/DataManagement/GetEntitiesResponse.cs` | `Beep.OilandGas.Models.Data.DataManagement` |
| `TheTechIdea.Data/OilGas/IAssetHierarchyService.cs` | `Beep.OilandGas.Models/Core/Interfaces/IAssetHierarchyService.cs` | `Beep.OilandGas.Models.Core.Interfaces` |
| `TheTechIdea.Data/OilGas/JournalPostingEvidence.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/JournalPostingEvidence.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/OrganizationHierarchyConfiguration.cs` | `Beep.OilandGas.Models/Data/Security/ORGANIZATION_HIERARCHY_CONFIG.cs` | `Beep.OilandGas.Models.Data.Security` |
| `TheTechIdea.Data/OilGas/ProcessTransitionRequest.cs` | `Beep.OilandGas.Models/Data/Process/ProcessTransitionRequest.cs` | `Beep.OilandGas.Models.Data.Process` |
| `TheTechIdea.Data/OilGas/ProductionDashboardResponse.cs` | `Beep.OilandGas.Models/Data/Production/ProductionDashboardResponse.cs` | `Beep.OilandGas.Models.Data.Production` |
| `TheTechIdea.Data/OilGas/TransitionWorkOrderRequest.cs` | `Beep.OilandGas.Models/Data/WorkOrder/TransitionWorkOrderRequest.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/UpdateLeaseStatusRequest.cs` | `Beep.OilandGas.Models/Data/Operations/UpdateLeaseStatusRequest.cs` | `Beep.OilandGas.Models.Data.Operations` |
| `TheTechIdea.Data/OilGas/UpdatePreferencesRequest.cs` | `Beep.OilandGas.Models/Data/AccessControl/UpdatePreferencesRequest.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/UpdatePreferredLayoutRequest.cs` | `Beep.OilandGas.Models/Data/AccessControl/UpdatePreferredLayoutRequest.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/UpdatePrimaryRoleRequest.cs` | `Beep.OilandGas.Models/Data/AccessControl/UpdatePrimaryRoleRequest.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/ValidateAccessRequest.cs` | `Beep.OilandGas.Models/Data/AccessControl/ValidateAccessRequest.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/ApplyRowFiltersRequest.cs` | `Beep.OilandGas.Models/Data/AccessControl/ApplyRowFiltersRequest.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/AssetAccessLevelChecks.cs` | `Beep.OilandGas.Models/Data/AccessControl/AssetAccessLevelChecks.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/AssetHierarchyNode.cs` | `Beep.OilandGas.Models/Data/AccessControl/AssetHierarchyNode.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/BeepConnectionName.cs` | `Beep.OilandGas.Models/Data/DataManagement/BeepConnectionName.cs` | `Beep.OilandGas.Models.Data.DataManagement` |
| `TheTechIdea.Data/OilGas/CheckDatabaseAccessRequest.cs` | `Beep.OilandGas.Models/Data/AccessControl/CheckDatabaseAccessRequest.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/CheckDataSourceAccessRequest.cs` | `Beep.OilandGas.Models/Data/AccessControl/CheckDataSourceAccessRequest.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/CheckRowAccessRequest.cs` | `Beep.OilandGas.Models/Data/AccessControl/CheckRowAccessRequest.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/CheckSourceAccessRequest.cs` | `Beep.OilandGas.Models/Data/AccessControl/CheckSourceAccessRequest.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/IAccessControlService.cs` | `Beep.OilandGas.Models/Core/Interfaces/IAccessControlService.cs` | `Beep.OilandGas.Models.Core.Interfaces` |
| `TheTechIdea.Data/OilGas/CompanyNativeDecisionHandoffReview.cs` | `Beep.OilandGas.Models/Data/Company/CompanyNativeDecisionHandoffReview.cs` | `Beep.OilandGas.Models.Data.Company` |
| `TheTechIdea.Data/OilGas/CompanyOrganizationReview.cs` | `Beep.OilandGas.Models/Data/DataManagement/CompanyOrganizationReview.cs` | `Beep.OilandGas.Models.Data.DataManagement` |
| `TheTechIdea.Data/OilGas/CompanyRequestHeaders.cs` | `Beep.OilandGas.Models/Data/Company/CompanyRequestHeaders.cs` | `Beep.OilandGas.Models.Data.Company` |
| `TheTechIdea.Data/OilGas/CompanyRequestPolicy.cs` | `Beep.OilandGas.Models/Data/Company/CompanyRequestPolicy.cs` | `Beep.OilandGas.Models.Data.Company` |
| `TheTechIdea.Data/OilGas/CompanyWellQualityReview.cs` | `Beep.OilandGas.Models/Data/Production/CompanyWellQualityReview.cs` | `Beep.OilandGas.Models.Data.Production` |
| `TheTechIdea.Data/OilGas/ExecutivePortfolioReview.cs` | `Beep.OilandGas.Models/Data/Reporting/ExecutivePortfolioReview.cs` | `Beep.OilandGas.Models.Data.Reporting` |
| `TheTechIdea.Data/OilGas/ExplicitUtcInput.cs` | `Beep.OilandGas.Models/Data/Common/ExplicitUtcInput.cs` | `Beep.OilandGas.Models.Data.Common` |
| `TheTechIdea.Data/OilGas/FileImportModels.cs` | `Beep.OilandGas.Models/Data/DataManagement/FileImportResult.cs` | `Beep.OilandGas.Models.Data.DataManagement` |
| `TheTechIdea.Data/OilGas/HierarchyConfigurationRules.cs` | `Beep.OilandGas.Models/Data/AccessControl/HierarchyConfigurationRules.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/HseIncidentPage.cs` | `Beep.OilandGas.Models/Data/HSE/HseIncidentPage.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/IFieldSelectionContext.cs` | `Beep.OilandGas.Models/Core/Interfaces/IFieldSelectionContext.cs` | `Beep.OilandGas.Models.Core.Interfaces` |
| `TheTechIdea.Data/OilGas/IHierarchyConfigurationStore.cs` | `Beep.OilandGas.Models/Core/Interfaces/IHierarchyConfigurationStore.cs` | `Beep.OilandGas.Models.Core.Interfaces` |
| `TheTechIdea.Data/OilGas/ILeaseAcquisitionService.cs` | `Beep.OilandGas.Models/Core/Interfaces/ILeaseAcquisitionService.cs` | `Beep.OilandGas.Models.Core.Interfaces` |
| `TheTechIdea.Data/OilGas/InitialCompanyProvisionRequest.cs` | `Beep.OilandGas.Models/Data/DataManagement/InitialCompanyProvisionRequest.cs` | `Beep.OilandGas.Models.Data.DataManagement` |
| `TheTechIdea.Data/OilGas/RecordedSourceId.cs` | `Beep.OilandGas.Models/Data/Common/RecordedSourceId.cs` | `Beep.OilandGas.Models.Data.Common` |
| `TheTechIdea.Data/OilGas/WellTestReview.cs` | `Beep.OilandGas.Models/Data/Production/WellTestReview.cs` | `Beep.OilandGas.Models.Data.Production` |
| `TheTechIdea.Data/OilGas/AccessCheckResponse.cs` | `Beep.OilandGas.Models/Data/AccessControl/AccessCheckResponse.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/AssetAccess.cs` | `Beep.OilandGas.Models/Data/AccessControl/AssetAccess.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/CompanyAccountingProcessStartCatalog.cs` | `Beep.OilandGas.Models/Data/Process/CompanyAccountingProcessStartCatalog.cs` | `Beep.OilandGas.Models.Data.Process` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceQualifiedReturnParticipant.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceQualifiedReturnParticipant.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceRecordedWork.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceRecordedWork.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderExecutionPermitDecision.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderExecutionPermitDecision.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CreateLeaseAcquisition.cs` | `Beep.OilandGas.Models/Data/Lease/CreateLeaseAcquisition.cs` | `Beep.OilandGas.Models.Data.Lease` |
| `TheTechIdea.Data/OilGas/FieldGhgProductionSources.cs` | `Beep.OilandGas.Models/Data/HSE/FieldGhgProductionSources.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/FieldInterestReview.cs` | `Beep.OilandGas.Models/Data/Lease/FieldInterestReview.cs` | `Beep.OilandGas.Models.Data.Lease` |
| `TheTechIdea.Data/OilGas/FieldObligationSourceReview.cs` | `Beep.OilandGas.Models/Data/Compliance/FieldObligationSourceReview.cs` | `Beep.OilandGas.Models.Data.Compliance` |
| `TheTechIdea.Data/OilGas/HierarchyConfig.cs` | `Beep.OilandGas.Models/Data/AccessControl/HierarchyConfig.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/HSE_INCIDENT_CLOSURE.cs` | `Beep.OilandGas.Models/Data/HSE/HSE_INCIDENT_CLOSURE.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/ProcessInstanceRequest.cs` | `Beep.OilandGas.Models/Data/Process/ProcessInstanceRequest.cs` | `Beep.OilandGas.Models.Data.Process` |
| `TheTechIdea.Data/OilGas/ProductionInterventionNativeDecision.cs` | `Beep.OilandGas.Models/Data/WorkOrder/ProductionInterventionNativeDecision.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/UserProfile.cs` | `Beep.OilandGas.Models/Data/AccessControl/UserProfile.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/WellTestResponse.cs` | `Beep.OilandGas.Models/Data/Production/WellTestResponse.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/WorkOrderSchema.cs` | `Beep.OilandGas.Models/Data/WorkOrder/WorkOrderSchema.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/AR_PAYMENT.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/AR_PAYMENT.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanyAccountingProcessStart.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyAccountingProcessStart.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceAssetReturnStatus.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceAssetReturnStatus.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceAssetReturnCommand.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceAssetReturnCommand.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CorrectiveActionPlanContracts.cs` | `Beep.OilandGas.Models/Data/HSE/CorrectiveActionPlanContracts.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/IncidentReportIntent.cs` | `Beep.OilandGas.Models/Data/HSE/IncidentReportIntent.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/LeaseCreationContext.cs` | `Beep.OilandGas.Models/Data/Lease/LeaseCreationContext.cs` | `Beep.OilandGas.Models.Data.Lease` |
| `TheTechIdea.Data/OilGas/ProductionInterventionActivityChecks.cs` | `Beep.OilandGas.Models/Data/WorkOrder/ProductionInterventionActivityChecks.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/ProductionInterventionWorkOrderReview.cs` | `Beep.OilandGas.Models/Data/WorkOrder/ProductionInterventionWorkOrderReview.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/WorkflowInboxModels.cs` | `Beep.OilandGas.Models/Data/Company/WorkflowInboxModels.cs` | `Beep.OilandGas.Models.Data.Company` |
| `TheTechIdea.Data/OilGas/WorkOrderParentAccess.cs` | `Beep.OilandGas.Models/Data/AccessControl/WorkOrderParentAccess.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/CompanyAccountingStepCommand.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyAccountingStepCommand.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/IncidentClosureResolution.cs` | `Beep.OilandGas.Models/Data/HSE/IncidentClosureResolution.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/CompanyHseClosure.cs` | `Beep.OilandGas.Models/Data/HSE/CompanyHseClosure.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceAssetReturn.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceAssetReturn.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceQualifiedAssetReturn.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceQualifiedAssetReturn.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderLaborCost.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderLaborCost.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderMaterialCostSource.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderMaterialCostSource.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CreateCAPlanRequest.cs` | `Beep.OilandGas.Models/Data/HSE/CreateCAPlanRequest.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/IncidentReportOptions.cs` | `Beep.OilandGas.Models/Data/HSE/IncidentReportOptions.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/INVENTORY_STOCK_INTENT.cs` | `Beep.OilandGas.Models/Data/Accounting/INVENTORY_STOCK_INTENT.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/InvestigatorAssignmentReview.cs` | `Beep.OilandGas.Models/Data/HSE/InvestigatorAssignmentReview.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/DevelopmentPlanning/FACILITY_EDIT_INTENT.cs` | `Beep.OilandGas.Models/Data/DevelopmentPlanning/FACILITY_EDIT_INTENT.cs` | `Beep.OilandGas.Models.Data.DevelopmentPlanning` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderActualCostOwnership.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/CompanyWorkOrderActualCostOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/ACCOUNTING_COST.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/ACCOUNTING_COST.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanyAccountingStepReview.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyAccountingStepReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceClosure.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceClosure.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderBudgetDecisionFacts.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderBudgetDecisionFacts.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderLaborUsageFacts.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderLaborUsageFacts.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/IncidentClosureReview.cs` | `Beep.OilandGas.Models/Data/HSE/IncidentClosureReview.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/PurchaseReceiptStockCommand.cs` | `Beep.OilandGas.Models/Data/Accounting/PurchaseReceiptStockCommand.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/DevelopmentPlanning/DevelopmentFacilityEdit.cs` | `Beep.OilandGas.Models/Data/DevelopmentPlanning/DevelopmentFacilityEdit.cs` | `Beep.OilandGas.Models.Data.DevelopmentPlanning` |
| `TheTechIdea.Data/OilGas/ALLOCATION_DETAIL.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/ALLOCATION_DETAIL.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/RevenueDeliveryAcceptanceCorrection.cs` | `Beep.OilandGas.Models/Data/Accounting/RevenueDeliveryAcceptanceCorrection.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanySalesJournalPosting.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanySalesJournalPosting.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SALES_JOURNAL_POSTING_INTENT.cs` | `Beep.OilandGas.Models/Data/Accounting/SALES_JOURNAL_POSTING_INTENT.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SALES_JOURNAL_REVIEW_INTENT.cs` | `Beep.OilandGas.Models/Data/Accounting/SALES_JOURNAL_REVIEW_INTENT.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanySalesJournalReview.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanySalesJournalReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanySalesJournalPostingOutcome.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanySalesJournalPostingOutcome.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanySalesInvoiceQueueReview.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanySalesInvoiceQueueReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanySalesReceivableWorkspace.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanySalesReceivableWorkspace.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanySalesJournalSource.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanySalesJournalSource.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanySalesJournalPreparation.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanySalesJournalPreparation.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SALES_RECEIVABLE_REVIEW_INTENT.cs` | `Beep.OilandGas.Models/Data/Accounting/SALES_RECEIVABLE_REVIEW_INTENT.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanySalesReceivableDecision.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanySalesReceivableDecision.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SALES_RECEIVABLE_INTENT.cs` | `Beep.OilandGas.Models/Data/Accounting/SALES_RECEIVABLE_INTENT.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanySalesReceivablePreparation.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanySalesReceivablePreparation.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SALES_RECEIVABLE_OUTCOME.cs` | `Beep.OilandGas.Models/Data/Accounting/SALES_RECEIVABLE_OUTCOME.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SalesContractTermApplicabilityReview.cs` | `Beep.OilandGas.Models/Data/Accounting/SalesContractTermApplicabilityReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/RevenueDeliveryBillingBasisReview.cs` | `Beep.OilandGas.Models/Data/Accounting/RevenueDeliveryBillingBasisReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/ALLOCATION_RESULT.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/ALLOCATION_RESULT.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/RevenueDeliverySourceReview.cs` | `Beep.OilandGas.Models/Data/Accounting/RevenueDeliverySourceReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanySalesInvoiceReviewSource.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanySalesInvoiceReviewSource.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SALES_INVOICE_REVIEW_INTENT.cs` | `Beep.OilandGas.Models/Data/Accounting/SALES_INVOICE_REVIEW_INTENT.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanySalesInvoiceDecision.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanySalesInvoiceDecision.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanySalesInvoiceDraft.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanySalesInvoiceDraft.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SALES_INVOICE_INTENT.cs` | `Beep.OilandGas.Models/Data/Accounting/SALES_INVOICE_INTENT.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SALES_INVOICE_OUTCOME.cs` | `Beep.OilandGas.Models/Data/Accounting/SALES_INVOICE_OUTCOME.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanyAccountingSourceTypes.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyAccountingSourceTypes.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SalesContractTermsWorkflow.cs` | `Beep.OilandGas.Models/Data/Accounting/SalesContractTermsWorkflow.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SalesContractTermsAmendment.cs` | `Beep.OilandGas.Models/Data/Accounting/SalesContractTermsAmendment.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SalesContractTermsNativeProof.cs` | `Beep.OilandGas.Models/Data/Accounting/SalesContractTermsNativeProof.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SALES_CONTRACT.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/SALES_CONTRACT.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/RevenueSalesContractReview.cs` | `Beep.OilandGas.Models/Data/Accounting/RevenueSalesContractReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/SalesContractTermsReadReview.cs` | `Beep.OilandGas.Models/Data/Accounting/SalesContractTermsReadReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/RevenueDeliveryAcceptance.cs` | `Beep.OilandGas.Models/Data/Accounting/RevenueDeliveryAcceptance.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/RUN_TICKET.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/RUN_TICKET.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/AllocationRunTicketCompatibility.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/AllocationRunTicketCompatibility.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/APInvoiceCorrectionStepEvidence.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceCorrectionStepEvidence.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderBudgetDecision.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderBudgetDecision.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderLaborUsage.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderLaborUsage.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderReturnAcceptance.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderReturnAcceptance.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CorrectiveActionVerificationDecision.cs` | `Beep.OilandGas.Models/Data/HSE/CorrectiveActionVerificationDecision.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/IncidentRcaIntent.cs` | `Beep.OilandGas.Models/Data/HSE/IncidentRcaIntent.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/PurchaseReceiptStockReview.cs` | `Beep.OilandGas.Models/Data/Accounting/PurchaseReceiptStockReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/PurchaseReceiptInventoryReview.cs` | `Beep.OilandGas.Models/Data/Accounting/PurchaseReceiptInventoryReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/AddCARequest.cs` | `Beep.OilandGas.Models/Data/HSE/AddCARequest.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/AllocationMethod.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/AllocationMethod.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/APInvoiceCorrectionDecisionReview.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceCorrectionDecisionReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/AR_INVOICE.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/AR_INVOICE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanyAccountingStepEvidence.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyAccountingStepEvidence.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanyFxConversion.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyFxConversion.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanyLedgerPeriodSource.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyLedgerPeriodSource.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/GL_ENTRY.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/GL_ENTRY.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanySalesGLEntryOwnership.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/CompanySalesGLEntryOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanySalesInvoiceValidation.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanySalesInvoiceValidation.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/JOURNAL_ENTRY_LINE.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/JOURNAL_ENTRY_LINE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/JOURNAL_ENTRY.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/JOURNAL_ENTRY.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanySalesJournalNativeOwnership.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/CompanySalesJournalNativeOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderBudget.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderBudget.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderCompletionDecisionStatus.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderCompletionDecisionStatus.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/FieldRevenueSourceReview.cs` | `Beep.OilandGas.Models/Data/Accounting/FieldRevenueSourceReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/IncidentRcaSourceReview.cs` | `Beep.OilandGas.Models/Data/HSE/IncidentRcaSourceReview.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/INVOICE.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/INVOICE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanyFxRate.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyFxRate.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanyLedgerPeriodTransition.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyLedgerPeriodTransition.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanyLedgerPeriodReview.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyLedgerPeriodReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderCompletionDecision.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderCompletionDecision.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/HseRestrictionReview.cs` | `Beep.OilandGas.Models/Data/HSE/HseRestrictionReview.cs` | `Beep.OilandGas.Models.Data.HSE` |
| `TheTechIdea.Data/OilGas/REVENUE_TRANSACTION.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/REVENUE_TRANSACTION.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanyGLMappingReview.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyGLMappingReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderPermitPrerequisiteEvidence.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderPermitPrerequisiteEvidence.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderAcceptedPersonnelReview.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderAcceptedPersonnelReview.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderMaterialAllocationRecovery.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderMaterialAllocationRecovery.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderMaterialRelease.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderMaterialRelease.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderMaterialSourceReview.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderMaterialSourceReview.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderMaterialUsage.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderMaterialUsage.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderMaterialAllocation.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderMaterialAllocation.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderAcceptedMaterialReview.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderAcceptedMaterialReview.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderSubmission.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderSubmission.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceQualifiedOperating.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceQualifiedOperating.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceQualifiedReturnPolicy.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceQualifiedReturnPolicy.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceLicenseArea.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceLicenseArea.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceLicenseConditionTerms.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceLicenseConditionTerms.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceQualifiedLicensing.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceQualifiedLicensing.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceQualifiedAssets.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceQualifiedAssets.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderExecutionPrerequisites.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderExecutionPrerequisites.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderSubmissionInspectionReview.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderSubmissionInspectionReview.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderCompletionReview.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderCompletionReview.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceQualifiedWorkScope.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceQualifiedWorkScope.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderRecordedApproval.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderRecordedApproval.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderExecutionRequirements.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderExecutionRequirements.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderExecutionStart.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderExecutionStart.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyPersonnelIdentity.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyPersonnelIdentity.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderAcceptedDocumentReview.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderAcceptedDocumentReview.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderDocumentContributors.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderDocumentContributors.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderPermitDocumentSource.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderPermitDocumentSource.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderDocumentAcceptance.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderDocumentAcceptance.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderPersonnelAssignment.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderPersonnelAssignment.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderPersonnelRelease.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderPersonnelRelease.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyPersonnelCredentialReview.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyPersonnelCredentialReview.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderPersonnelAcceptance.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderPersonnelAcceptance.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/WORK_ORDER_INSPECTION.cs` | `Beep.OilandGas.Models/Data/WorkOrder/WORK_ORDER_INSPECTION.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/WorkOrderDTOs.cs` | `Beep.OilandGas.Models/Data/WorkOrder/WorkOrderDTOs.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/WorkOrderInspectionRules.cs` | `Beep.OilandGas.Models/Data/WorkOrder/WorkOrderInspectionRules.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderExecutionPermitIntent.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderExecutionPermitIntent.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderPersonnelSource.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderPersonnelSource.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderAssignedPersonnelReview.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderAssignedPersonnelReview.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderExecutionPermitApprovalReview.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderExecutionPermitApprovalReview.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderMaterialReadinessReview.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderMaterialReadinessReview.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderMaterialAcceptance.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderMaterialAcceptance.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/WorkOrderWorkflow.cs` | `Beep.OilandGas.Models/Data/WorkOrder/WorkOrderWorkflow.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceInspectionReview.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceInspectionReview.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/COMPANY_LEDGER_PERIOD.cs` | `Beep.OilandGas.Models/Data/Accounting/COMPANY_LEDGER_PERIOD.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/Country.cs` | `Beep.OilandGas.Models/Data/PermitsAndApplications/Country.cs` | `Beep.OilandGas.Models.Data.PermitsAndApplications` |
| `TheTechIdea.Data/OilGas/FX_RATE.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/FX_RATE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/RegulatoryAuthority.cs` | `Beep.OilandGas.Models/Data/PermitsAndApplications/RegulatoryAuthority.cs` | `Beep.OilandGas.Models.Data.PermitsAndApplications` |
| `TheTechIdea.Data/OilGas/StateProvince.cs` | `Beep.OilandGas.Models/Data/PermitsAndApplications/StateProvince.cs` | `Beep.OilandGas.Models.Data.PermitsAndApplications` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceAssignedStageReview.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceAssignedStageReview.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceReturnPolicy.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceReturnPolicy.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyPersonnelPolicy.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyPersonnelPolicy.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/GLAccountMapping.cs` | `Beep.OilandGas.Models/Data/Accounting/GLAccountMapping.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/PermitToWorkType.cs` | `Beep.OilandGas.Models/Enums/Shared/PermitToWorkType.cs` | `Beep.OilandGas.Models.Enums.Shared` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceAcceptance.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceAcceptance.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/WORK_ORDER_EXECUTION_PERMIT.cs` | `Beep.OilandGas.Models/Data/WorkOrder/WORK_ORDER_EXECUTION_PERMIT.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/ModelEntityBase.cs` | `Beep.OilandGas.Models/Data/ModelEntityBase.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/ACCOUNTING_METHOD.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/ACCOUNTING_METHOD.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/DevelopmentPlanning/DevelopmentAfeOwnership.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/DevelopmentAfeOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderBudgetOwnership.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/CompanyWorkOrderBudgetOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/AFE.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/AFE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/PO_RECEIPT.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/PO_RECEIPT.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanyAccessModels.cs` | `Beep.OilandGas.Models/Data/AccessControl/CompanyAccessModels.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/PURCHASE_ORDER.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/PURCHASE_ORDER.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/PO_LINE_ITEM.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/PO_LINE_ITEM.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/PurchaseOrderOwnership.cs` | `Beep.OilandGas.Models/Data/Accounting/PurchaseOrderOwnership.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/INVENTORY_ITEM.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/INVENTORY_ITEM.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/ProcessDefinitionRevision.cs` | `Beep.OilandGas.Models/Data/Process/ProcessDefinitionRevision.cs` | `Beep.OilandGas.Models.Data.Process` |
| `TheTechIdea.Data/OilGas/INVENTORY_TRANSACTION.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/INVENTORY_TRANSACTION.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanyInventorySourceReview.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyInventorySourceReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/PO_INVENTORY_LINK_HISTORY.cs` | `Beep.OilandGas.Models/Data/Accounting/PO_INVENTORY_LINK_HISTORY.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/PurchaseOrderInventoryLinkDecision.cs` | `Beep.OilandGas.Models/Data/Accounting/PurchaseOrderInventoryLinkDecision.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/PurchaseOrderInventoryLinkProposal.cs` | `Beep.OilandGas.Models/Data/Accounting/PurchaseOrderInventoryLinkProposal.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/PurchaseOrderInventoryLinkHistoryReview.cs` | `Beep.OilandGas.Models/Data/Accounting/PurchaseOrderInventoryLinkHistoryReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/PurchaseOrderSourceReview.cs` | `Beep.OilandGas.Models/Data/Accounting/PurchaseOrderSourceReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/PurchaseReceiptDecisionCommand.cs` | `Beep.OilandGas.Models/Data/Accounting/PurchaseReceiptDecisionCommand.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/AP_INVOICE_LINE.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/AP_INVOICE_LINE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/AP_RECEIPT_ALLOCATION.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/AP_RECEIPT_ALLOCATION.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/AP_INVOICE.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/AP_INVOICE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanyMatchingPolicyReview.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyMatchingPolicyReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanyMatchingPolicyCommand.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyMatchingPolicyCommand.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APInvoiceCorrectionReview.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceCorrectionReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APInvoiceFinancialReview.MatchingPolicy.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceFinancialReview.MatchingPolicy.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APInvoiceVarianceDecisionReview.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceVarianceDecisionReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APInvoiceVarianceDecisionCommand.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceVarianceDecisionCommand.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APReceiptAllocationReview.cs` | `Beep.OilandGas.Models/Data/Accounting/APReceiptAllocationReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APReceiptConsumptionReview.cs` | `Beep.OilandGas.Models/Data/Accounting/APReceiptConsumptionReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APInvoiceFinancialReview.OriginalVariance.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceFinancialReview.OriginalVariance.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APInvoiceVarianceCommand.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceVarianceCommand.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APInvoiceSourceReview.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceSourceReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APLinePurchaseSourceReview.cs` | `Beep.OilandGas.Models/Data/Accounting/APLinePurchaseSourceReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APReceiptAllocationProposal.cs` | `Beep.OilandGas.Models/Data/Accounting/APReceiptAllocationProposal.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APReceiptAllocationComparisons.cs` | `Beep.OilandGas.Models/Data/Accounting/APReceiptAllocationComparisons.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APInvoiceVarianceProposalReview.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceVarianceProposalReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APInvoiceSourceReview.CompanyWorkflow.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceSourceReview.CompanyWorkflow.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APInvoiceFinancialReview.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceFinancialReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APInvoiceFinancialReview.ReceiptConsumption.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceFinancialReview.ReceiptConsumption.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/APInvoiceCorrectionCommand.cs` | `Beep.OilandGas.Models/Data/Accounting/APInvoiceCorrectionCommand.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/DevelopmentPlanning/DevelopmentInvestmentOwnership.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/DevelopmentInvestmentOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanyWorkOrderBudgetDecisionOwnership.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/CompanyWorkOrderBudgetDecisionOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/APPROVAL_WORKFLOW.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/APPROVAL_WORKFLOW.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/AppUserAssetAccess.cs` | `Beep.OilandGas.Models/Data/AccessControl/AppUserAssetAccess.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/AssetGrantReview.cs` | `Beep.OilandGas.Models/Data/AccessControl/AssetGrantReview.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/CompanyBillingPolicy.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyBillingPolicy.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/GL_ACCOUNT.cs` | `Beep.OilandGas.Models/Data/ProductionAccounting/GL_ACCOUNT.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanyGLAccountReview.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyGLAccountReview.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/COMPANY_LEDGER_BOOK.cs` | `Beep.OilandGas.Models/Data/Accounting/COMPANY_LEDGER_BOOK.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/CompanyLedgerBookConfiguration.cs` | `Beep.OilandGas.Models/Data/Accounting/CompanyLedgerBookConfiguration.cs` | `Beep.OilandGas.Models.Data.Accounting` |
| `TheTechIdea.Data/OilGas/IUserAssetAccessStore.cs` | `Beep.OilandGas.Models/Data/AccessControl/IUserAssetAccessStore.cs` | `Beep.OilandGas.Models.Data.AccessControl` |
| `TheTechIdea.Data/OilGas/FieldListItem.cs` | `Beep.OilandGas.Models/Data/FieldOrchestrator/FieldListItem.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/SetActiveFieldResponse.cs` | `Beep.OilandGas.Models/Data/FieldOrchestrator/SetActiveFieldResponse.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/SetActiveFieldRequest.cs` | `Beep.OilandGas.Models/Data/FieldOrchestrator/SetActiveFieldRequest.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/FieldResponse.cs` | `Beep.OilandGas.Models/Data/Production/FieldResponse.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/FieldSelectionReview.cs` | `Beep.OilandGas.Models/Data/Field/FieldSelectionReview.cs` | `Beep.OilandGas.Models.Data.Field` |
| `TheTechIdea.Data/OilGas/LeaseSummary.cs` | `Beep.OilandGas.Models/Data/Lease/LeaseSummary.cs` | `Beep.OilandGas.Models.Data.Lease` |
| `TheTechIdea.Data/OilGas/LeaseSummaryValidator.cs` | `Beep.OilandGas.Models/Data/Lease/LeaseSummaryValidator.cs` | `Beep.OilandGas.Models.Data.Lease` |
| `TheTechIdea.Data/OilGas/FieldSelectionReviewValidator.cs` | `Beep.OilandGas.Models/Data/Field/FieldSelectionReviewValidator.cs` | `Beep.OilandGas.Models.Data.Field` |
| `TheTechIdea.Data/OilGas/ProductionInterventionMarkers.cs` | `Beep.OilandGas.Models/Data/WorkOrder/ProductionInterventionMarkers.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/ProductionInterventionCandidateDto.cs` | `Beep.OilandGas.Models/Data/WorkOrder/ProductionInterventionCandidateDto.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/ProductionInterventionDecision.cs` | `Beep.OilandGas.Models/Data/WorkOrder/ProductionInterventionDecision.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/ProductionInterventionMaintenanceLink.cs` | `Beep.OilandGas.Models/Data/WorkOrder/ProductionInterventionMaintenanceLink.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/CompanyMaintenanceProcessStart.cs` | `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceProcessStart.cs` | `Beep.OilandGas.Models.Data.WorkOrder` |
| `TheTechIdea.Data/OilGas/DevelopmentPlanning/CreateDevelopmentPlan.cs` | `Beep.OilandGas.Models/Data/CreateDevelopmentPlan.cs` | `Beep.OilandGas.Models.Data` |
| `TheTechIdea.Data/OilGas/DevelopmentPlanning/FIELD_DEVELOPMENT_PLAN.cs` | `Beep.OilandGas.Models/Data/DevelopmentPlanning/FIELD_DEVELOPMENT_PLAN.cs` | `Beep.OilandGas.Models.Data.DevelopmentPlanning` |
| `TheTechIdea.Data/OilGas/DevelopmentPlanning/DevelopmentPlanBaselineOwnership.cs` | `Beep.OilandGas.Models/Data/DevelopmentPlanning/DevelopmentPlanBaselineOwnership.cs` | `Beep.OilandGas.Models.Data.DevelopmentPlanning` |

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.Models/Beep.OilandGas.Models.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

53 candidate incoming files; 1153 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |
| `TheTechIdea.Data/OilGas/AccessCheckRequest.cs` | `Beep.OilandGas.Models.Data`: AccessCheckRequest | Beep.OilandGas.Models/Data/AccessControl/AccessCheckRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/AccessCheckResponse.cs` | `Beep.OilandGas.Models.Data`: AccessCheckResponse | Beep.OilandGas.Models/Data/AccessControl/AccessCheckResponse.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/AccessControlRequests.cs` | `Beep.OilandGas.Models.Data.AccessControl`: GrantAccessRequest, RevokeAccessRequest, RoleDefinitions | Beep.OilandGas.Models/Data/AccessControl/AccessControlRequests.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ACCOUNTING_COST.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: ACCOUNTING_COST | Beep.OilandGas.Models/Data/ProductionAccounting/ACCOUNTING_COST.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ACCOUNTING_METHOD.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: ACCOUNTING_METHOD | Beep.OilandGas.Models/Data/ProductionAccounting/ACCOUNTING_METHOD.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/AP_INVOICE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: AP_INVOICE | Beep.OilandGas.Models/Data/ProductionAccounting/AP_INVOICE.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/AR_CREDIT_MEMO.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: AR_CREDIT_MEMO | Beep.OilandGas.Models/Data/ProductionAccounting/AR_CREDIT_MEMO.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/AR_INVOICE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: AR_INVOICE | Beep.OilandGas.Models/Data/ProductionAccounting/AR_INVOICE.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/AR_PAYMENT.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: AR_PAYMENT | Beep.OilandGas.Models/Data/ProductionAccounting/AR_PAYMENT.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/AssetAccess.cs` | `Beep.OilandGas.Models.Data`: AssetAccess | Beep.OilandGas.Models/Data/AccessControl/AssetAccess.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/AssetHierarchyNode.cs` | `Beep.OilandGas.Models.Data`: AssetHierarchyNode | Beep.OilandGas.Models/Data/AccessControl/AssetHierarchyNode.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/DevelopmentPlanning/DevelopmentAfeOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: AFE | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/DevelopmentPlanning/DevelopmentInvestmentOwnership.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: APPROVAL_WORKFLOW | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/DevelopmentPlanning/DevelopmentPlanBaselineOwnership.cs` | `Beep.OilandGas.Models.Data.DevelopmentPlanning`: FIELD_DEVELOPMENT_PLAN | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/DrillingAndConstruction/DRILLING_OPERATION.cs` | `TheTechIdea.Data.OilGas`: DRILLING_OPERATION | Beep.OilandGas.Models/Data/Drilling/DRILLING_OPERATION.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/FieldListItem.cs` | `Beep.OilandGas.Models.Data`: FieldListItem | Beep.OilandGas.Models/Data/FieldOrchestrator/FieldListItem.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/FieldResponse.cs` | `Beep.OilandGas.Models.Data`: FieldResponse | Beep.OilandGas.Models/Data/Production/FieldResponse.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/FX_RATE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: FX_RATE | Beep.OilandGas.Models/Data/ProductionAccounting/FX_RATE.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/GenericEntityRequest.cs` | `Beep.OilandGas.Models.Data.DataManagement`: GenericEntityRequest | Beep.OilandGas.Models/Data/DataManagement/GenericEntityRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/GenericEntityResponse.cs` | `Beep.OilandGas.Models.Data.DataManagement`: GenericEntityResponse | Beep.OilandGas.Models/Data/DataManagement/GenericEntityResponse.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/GetAssetHierarchyRequest.cs` | `Beep.OilandGas.Models.Data`: GetAssetHierarchyRequest | Beep.OilandGas.Models/Data/AccessControl/GetAssetHierarchyRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/GetEntitiesRequest.cs` | `Beep.OilandGas.Models.Data.DataManagement`: GetEntitiesRequest | Beep.OilandGas.Models/Data/DataManagement/GetEntitiesRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/GetEntitiesResponse.cs` | `Beep.OilandGas.Models.Data.DataManagement`: GetEntitiesResponse | Beep.OilandGas.Models/Data/DataManagement/GetEntitiesResponse.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/GL_ACCOUNT.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: GL_ACCOUNT | Beep.OilandGas.Models/Data/ProductionAccounting/GL_ACCOUNT.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/GL_ENTRY.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: GL_ENTRY | Beep.OilandGas.Models/Data/ProductionAccounting/GL_ENTRY.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/HierarchyConfig.cs` | `Beep.OilandGas.Models.Data`: HierarchyConfig | Beep.OilandGas.Models/Data/AccessControl/HierarchyConfig.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/IAssetHierarchyService.cs` | `Beep.OilandGas.Models.Core.Interfaces`: IAssetHierarchyService | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/INVENTORY_ITEM.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: INVENTORY_ITEM | Beep.OilandGas.Models/Data/ProductionAccounting/INVENTORY_ITEM.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/INVENTORY_TRANSACTION.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: INVENTORY_TRANSACTION | Beep.OilandGas.Models/Data/ProductionAccounting/INVENTORY_TRANSACTION.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/INVOICE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: INVOICE | Beep.OilandGas.Models/Data/ProductionAccounting/INVOICE.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/JOURNAL_ENTRY.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: JOURNAL_ENTRY | Beep.OilandGas.Models/Data/ProductionAccounting/JOURNAL_ENTRY.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/JOURNAL_ENTRY_LINE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: JOURNAL_ENTRY_LINE | Beep.OilandGas.Models/Data/ProductionAccounting/JOURNAL_ENTRY_LINE.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/JournalPostingEvidence.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: JournalPostingEvidence | Beep.OilandGas.Models/Data/ProductionAccounting/JournalPostingEvidence.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ModelEntityBase.cs` | `Beep.OilandGas.Models.Data`: ModelEntityBase | Beep.OilandGas.Models/Data/ModelEntityBase.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/OrganizationHierarchyConfiguration.cs` | `Beep.OilandGas.Models.Data.Security`: ORGANIZATION_HIERARCHY_CONFIG, OrganizationHierarchyConfigRevision | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/PermitToWorkType.cs` | `Beep.OilandGas.Models.Enums.Shared`: PermitToWorkType | Beep.OilandGas.Models/Enums/Shared/PermitToWorkType.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/PO_LINE_ITEM.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: PO_LINE_ITEM | Beep.OilandGas.Models/Data/ProductionAccounting/PO_LINE_ITEM.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/PO_RECEIPT.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: PO_RECEIPT | Beep.OilandGas.Models/Data/ProductionAccounting/PO_RECEIPT.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ProcessInstanceRequest.cs` | `Beep.OilandGas.Models.Data.Process`: ProcessInstanceRequest | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ProcessTransitionRequest.cs` | `Beep.OilandGas.Models.Data.Process`: ProcessTransitionRequest | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ProductionDashboardResponse.cs` | `Beep.OilandGas.Models.Data.Production`: ProductionDashboardResponse, ProductionDashboardSummary, ProductionWellStatusDto | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/PURCHASE_ORDER.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: PURCHASE_ORDER | Beep.OilandGas.Models/Data/ProductionAccounting/PURCHASE_ORDER.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/SetActiveFieldRequest.cs` | `Beep.OilandGas.Models.Data`: SetActiveFieldRequest | Beep.OilandGas.Models/Data/FieldOrchestrator/SetActiveFieldRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/SetActiveFieldResponse.cs` | `Beep.OilandGas.Models.Data`: SetActiveFieldResponse | Beep.OilandGas.Models/Data/FieldOrchestrator/SetActiveFieldResponse.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/TransitionWorkOrderRequest.cs` | `Beep.OilandGas.Models.Data.WorkOrder`: TransitionWorkOrderRequest | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/UpdateLeaseStatusRequest.cs` | `Beep.OilandGas.Models.Data.Operations`: UpdateLeaseStatusRequest | Beep.OilandGas.Models/Data/Operations/UpdateLeaseStatusRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/UpdatePreferencesRequest.cs` | `Beep.OilandGas.Models.Data.AccessControl`: UpdatePreferencesRequest | Beep.OilandGas.Models/Data/AccessControl/UpdatePreferencesRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/UpdatePreferredLayoutRequest.cs` | `Beep.OilandGas.Models.Data.AccessControl`: UpdatePreferredLayoutRequest | Beep.OilandGas.Models/Data/AccessControl/UpdatePreferredLayoutRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/UpdatePrimaryRoleRequest.cs` | `Beep.OilandGas.Models.Data.AccessControl`: UpdatePrimaryRoleRequest | Beep.OilandGas.Models/Data/AccessControl/UpdatePrimaryRoleRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/UserProfile.cs` | `Beep.OilandGas.Models.Data`: UserProfile | Beep.OilandGas.Models/Data/AccessControl/UserProfile.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ValidateAccessRequest.cs` | `Beep.OilandGas.Models.Data.AccessControl`: ValidateAccessRequest | Beep.OilandGas.Models/Data/AccessControl/ValidateAccessRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/WellTestResponse.cs` | `Beep.OilandGas.Models.Data`: WellTestResponse | Beep.OilandGas.Models/Data/Production/WellTestResponse.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/WorkOrderDTOs.cs` | `Beep.OilandGas.Models.Data.WorkOrder`: AssignContractorRequest, CalendarSlot, ContractorAssignment, ContractorQualificationResult, CostCode, CreateWorkOrderRequest, RecordInspectionResultRequest, RecordedWorkOrderComponentChecks, ScheduleConflict, ScheduleResult, ScheduleWorkOrderRequest, WorkOrderComponentReference, WorkOrderDetailModel, WorkOrderStep, WorkOrderSummary | Beep.OilandGas.Models/Data/WorkOrder/WorkOrderDTOs.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.Models/Constants/AccountingReferenceCodes.cs`
- `Beep.OilandGas.Models/Constants/ComponentDatabase.cs`
- `Beep.OilandGas.Models/Constants/ConnectionNames.cs`
- `Beep.OilandGas.Models/Constants/DrillingReferenceCodes.cs`
- `Beep.OilandGas.Models/Constants/FacilityReferenceCodes.cs`
- `Beep.OilandGas.Models/Constants/HseReferenceCodes.cs`
- `Beep.OilandGas.Models/Constants/ProductionReferenceCodes.cs`
- `Beep.OilandGas.Models/Constants/WellReferenceCodes.cs`
- `Beep.OilandGas.Models/Core/DTOs/ModuleSeedingRequest.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IAccessControlServices.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IAnalyticsService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IBackgroundOperationQueue.cs`
- `Beep.OilandGas.Models/Core/Interfaces/ICalculationService_PPDM.cs`
- `Beep.OilandGas.Models/Core/Interfaces/ICommonColumnHandler.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IConnectionService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IContractorManagementService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/ICorrectiveActionService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IDataManagementServices.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IDecommissioningService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IDemoDatabaseRepository.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IDemoDatabaseService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IEnhancedRecoveryService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IExportService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IFieldHSEService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IFieldProductionService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IGLAccountService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IGasPropertiesService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IHSEService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IHydraulicPumpService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IInspectionService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IIntegrationHealthService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/ILeaseEconomicInterestService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IOilPropertiesService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IPPDM39DataService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IPPDM39DemoDatabaseService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IPPDM39ReferenceDataSetupService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IPPDM39SchemaMigrationService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IPPDM39SetupExecutionService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IPPDM39SetupService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IPPDM39WorkflowService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IPPDMDatabaseCreatorService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IPPDMProductionService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IPPDMRepository.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IPipelineAnalysisService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IPlungerLiftService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IProcessService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IProductionForecastingService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IProductionOperationsService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IProgressTrackingService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IRenderingService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IReportingService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IRoyaltyDisputeService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/ISchedulingService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IStorageService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/ISuckerRodPumpingService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/ITrialBalanceService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IValidationService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IWellComparisonService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IWellTestAnalysisService.cs`
- `Beep.OilandGas.Models/Core/Interfaces/IWorkOrderService.cs`
- `Beep.OilandGas.Models/Core/Refusals/RefusalException.cs`
- `Beep.OilandGas.Models/Core/Refusals/RefusalKind.cs`
- `Beep.OilandGas.Models/Data/AccessControl/GetAccessibleAssetsRequest.cs`
- `Beep.OilandGas.Models/Data/AccessControl/RolePermission.cs`
- `Beep.OilandGas.Models/Data/Accounting/AccountingCompatibilityResults.cs`
- `Beep.OilandGas.Models/Data/Accounting/CloseAccountingPeriodRequest.cs`
- `Beep.OilandGas.Models/Data/Accounting/Financial/CeilingTestRequest.cs`
- `Beep.OilandGas.Models/Data/Accounting/Financial/FullCostAcquisitionRequest.cs`
- `Beep.OilandGas.Models/Data/Accounting/Financial/FullCostDevelopmentRequest.cs`
- `Beep.OilandGas.Models/Data/Accounting/Financial/FullCostExplorationRequest.cs`
- `Beep.OilandGas.Models/Data/Accounting/Financial/ImpairmentRequest.cs`
- `Beep.OilandGas.Models/Data/Accounting/PeriodCloseResult.cs`
- `Beep.OilandGas.Models/Data/Accounting/ReconciliationResult.cs`
- `Beep.OilandGas.Models/Data/Accounting/UnreconciledItem.cs`
- `Beep.OilandGas.Models/Data/Agreement/CreateSalesAgreementRequest.cs`
- `Beep.OilandGas.Models/Data/Agreement/CreateTransportationAgreementRequest.cs`
- `Beep.OilandGas.Models/Data/Agreement/PROCESSING_AGREEMENT.cs`
- `Beep.OilandGas.Models/Data/Agreement/SALES_AGREEMENT.cs`
- `Beep.OilandGas.Models/Data/Agreement/STORAGE_AGREEMENT.cs`
- `Beep.OilandGas.Models/Data/Agreement/TRANSPORTATION_AGREEMENT.cs`
- `Beep.OilandGas.Models/Data/Allocation/WellAllocationData.cs`
- `Beep.OilandGas.Models/Data/Analytics/ANALYTICS_RESULT.cs`
- `Beep.OilandGas.Models/Data/Analytics/AnalyticsDTOs.cs`
- `Beep.OilandGas.Models/Data/Analytics/AnalyticsInsight.cs`
- `Beep.OilandGas.Models/Data/Analytics/CostTrendsRequest.cs`
- `Beep.OilandGas.Models/Data/Analytics/DashboardData.cs`
- `Beep.OilandGas.Models/Data/Analytics/DashboardRequest.cs`
- `Beep.OilandGas.Models/Data/Analytics/ProductionTrendData.cs`
- `Beep.OilandGas.Models/Data/Analytics/ProductionTrendsRequest.cs`
- `Beep.OilandGas.Models/Data/Analytics/RevenueTrendData.cs`
- `Beep.OilandGas.Models/Data/Analytics/RevenueTrendsRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/AdvancedChokeAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/AnalyzeGasLiftPotentialRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/AnalyzePipelineFlowRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/BeanChokeDesign.cs`
- `Beep.OilandGas.Models/Data/Calculations/BubblePointAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/CALCULATION_RESULT.cs`
- `Beep.OilandGas.Models/Data/Calculations/CalculateApiGravityRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/CalculateDeclineRateRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/CalculatePressureDropRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/CalculateVolumeRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/CalculationInputParameter.cs`
- `Beep.OilandGas.Models/Data/Calculations/CalculationResultResponse.cs`
- `Beep.OilandGas.Models/Data/Calculations/CalculationRunStatus.cs`
- `Beep.OilandGas.Models/Data/Calculations/CapitalScheduleItem.cs`
- `Beep.OilandGas.Models/Data/Calculations/CapitalizedCostData.cs`
- `Beep.OilandGas.Models/Data/Calculations/CashFlowPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/CentrifugalCompressorAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChemicalEORAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChemicalParameters.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeAnalysisAdditionalResults.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeAnalysisOptions.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeBackPressureOptimization.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeErosionPrediction.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeNodalAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeOpeningAdjustment.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeOpeningPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokePerformanceDiagnostics.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeProductionForecast.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeProductionPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeSandCutRiskAssessment.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeSystemComprehensiveReport.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeTemperatureEffectAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeTransientAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/ChokeTrimSelection.cs`
- `Beep.OilandGas.Models/Data/Calculations/ComponentData.cs`
- `Beep.OilandGas.Models/Data/Calculations/CompressorAnalysisAdditionalResults.cs`
- `Beep.OilandGas.Models/Data/Calculations/CompressorAnalysisOptions.cs`
- `Beep.OilandGas.Models/Data/Calculations/CompressorAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/CompressorAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/CompressorAnalysisWellKnown.cs`
- `Beep.OilandGas.Models/Data/Calculations/CompressorDesign.cs`
- `Beep.OilandGas.Models/Data/Calculations/CompressorEfficiencyAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/CompressorMaintenancePrediction.cs`
- `Beep.OilandGas.Models/Data/Calculations/CompressorPerformance.cs`
- `Beep.OilandGas.Models/Data/Calculations/CompressorPowerAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/CompressorPressureFlowAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/CompressorStage.cs`
- `Beep.OilandGas.Models/Data/Calculations/CostBreakdownResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/CostComparisonResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DCAForecastPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/DCARequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/DCAResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaAdditionalResults.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaAnalysisOptions.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaDeclineModel.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaEndOfLifePredictionResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaForecastReliabilityResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaInflectionPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaLongTermForecastResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaModelComparisonMetricsResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaModelMetric.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaMonteCarloForecastResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaMonteCarloMonthlyStats.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaMultipleModelsComparisonResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaOptimizedParametersResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaParameterSensitivity.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaPhaseTransition.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaPortfolioAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaPortfolioWellAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaProductionTrendAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DcaSensitivityAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DeclineCurveAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/DeclineCurveAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/DesignValvesRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/DevelopmentCostAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DevelopmentCostEscalationFactorEntry.cs`
- `Beep.OilandGas.Models/Data/Calculations/DevelopmentPhase.cs`
- `Beep.OilandGas.Models/Data/Calculations/DevelopmentPhaseScheduleResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/DevelopmentStrategy.cs`
- `Beep.OilandGas.Models/Data/Calculations/DevelopmentWellTypeDistributionEntry.cs`
- `Beep.OilandGas.Models/Data/Calculations/DewPointAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/DrillingProgramResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/EOREconomicAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/EORMethodComparison.cs`
- `Beep.OilandGas.Models/Data/Calculations/EORMethodScore.cs`
- `Beep.OilandGas.Models/Data/Calculations/EconomicAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/EconomicAnalysisOptions.cs`
- `Beep.OilandGas.Models/Data/Calculations/EconomicAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/EconomicComparisonResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/EconomicProductionPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/EnvelopePoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/EnvironmentalComparisonResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/EnvironmentalImpactAssessmentResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/EquilibriumConstantAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/FacilityDesignResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/FieldDevelopmentStrategyResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/FlashCalculationAdditionalResults.cs`
- `Beep.OilandGas.Models/Data/Calculations/FlashCalculationOptions.cs`
- `Beep.OilandGas.Models/Data/Calculations/FlashCalculationPropertyResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/FlashCalculationRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/FlashCalculationResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/ForecastProductionDataPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/ForecastRiskAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/GasInjectionAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/GasLiftAnalysisAdditionalResults.cs`
- `Beep.OilandGas.Models/Data/Calculations/GasLiftAnalysisOptions.cs`
- `Beep.OilandGas.Models/Data/Calculations/GasLiftAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/GasLiftAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/GasLiftPerformancePoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/GasLiftValveData.cs`
- `Beep.OilandGas.Models/Data/Calculations/GasTypeCharacteristics.cs`
- `Beep.OilandGas.Models/Data/Calculations/ImpairmentAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/InfrastructurePlanningResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/InjectionWellOptimization.cs`
- `Beep.OilandGas.Models/Data/Calculations/InterestCapitalizationData.cs`
- `Beep.OilandGas.Models/Data/Calculations/InvestmentEvaluationResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/LiftSystemChokeInteraction.cs`
- `Beep.OilandGas.Models/Data/Calculations/MultiphaseChokeAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/NodalAnalysisAdditionalResults.cs`
- `Beep.OilandGas.Models/Data/Calculations/NodalAnalysisApiRequests.cs`
- `Beep.OilandGas.Models/Data/Calculations/NodalAnalysisHttpRoutes.cs`
- `Beep.OilandGas.Models/Data/Calculations/NodalAnalysisOptions.cs`
- `Beep.OilandGas.Models/Data/Calculations/NodalAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/NodalAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/NodalCurvePoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/NodalOperatingPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/NodalPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/OptimizeSystemRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/PVTEnvelopeAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/PerformNodalAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/PhaseDiagram.cs`
- `Beep.OilandGas.Models/Data/Calculations/PhasePropertiesData.cs`
- `Beep.OilandGas.Models/Data/Calculations/PhaseRegion.cs`
- `Beep.OilandGas.Models/Data/Calculations/PipelineAnalysisAdditionalResults.cs`
- `Beep.OilandGas.Models/Data/Calculations/PipelineAnalysisOptions.cs`
- `Beep.OilandGas.Models/Data/Calculations/PipelineAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/Calculations/PipelineAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/PipelineSpecification.cs`
- `Beep.OilandGas.Models/Data/Calculations/PressureDropResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/PressureFlowPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/PressurePerformanceAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/ProbabilisticForecast.cs`
- `Beep.OilandGas.Models/Data/Calculations/ProcessingEquipment.cs`
- `Beep.OilandGas.Models/Data/Calculations/ProcessingFacilitySpecification.cs`
- `Beep.OilandGas.Models/Data/Calculations/ProductionDataPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/ProductionForecastPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/ProductionForecastResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/ProductionPhase.cs`
- `Beep.OilandGas.Models/Data/Calculations/ProductionScheduleOptimizationResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/ProvedReserves.cs`
- `Beep.OilandGas.Models/Data/Calculations/RankedEORMethod.cs`
- `Beep.OilandGas.Models/Data/Calculations/ReciprocationCompressorAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/RecoverableReservesEstimate.cs`
- `Beep.OilandGas.Models/Data/Calculations/RiskAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/RiskComparisonResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/RiskItem.cs`
- `Beep.OilandGas.Models/Data/Calculations/ScenarioAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/ScheduleComparisonResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/SensitivityParameter.cs`
- `Beep.OilandGas.Models/Data/Calculations/SeparatorSimulation.cs`
- `Beep.OilandGas.Models/Data/Calculations/SeparatorStage.cs`
- `Beep.OilandGas.Models/Data/Calculations/StabilityAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/StorageRequirementsResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/StrategiesComparisonResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/StrategyRanking.cs`
- `Beep.OilandGas.Models/Data/Calculations/TemperatureFlowPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/ThermalRecoveryAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/TradeOffAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Calculations/TransientPoint.cs`
- `Beep.OilandGas.Models/Data/Calculations/TrimOption.cs`
- `Beep.OilandGas.Models/Data/Calculations/UncertaintyRange.cs`
- `Beep.OilandGas.Models/Data/Calculations/UnsuccessfulWellData.cs`
- `Beep.OilandGas.Models/Data/Calculations/VenturiChokeAnalysis.cs`
- `Beep.OilandGas.Models/Data/Calculations/WaterCutTrend.cs`
- `Beep.OilandGas.Models/Data/Calculations/WaterfloodPerformanceAnalysis.cs`
- `Beep.OilandGas.Models/Data/ChokeAnalysis/CHOKE_FLOW_RESULT.cs`
- `Beep.OilandGas.Models/Data/ChokeAnalysis/CHOKE_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/ChokeAnalysis/ChokePerformanceCurve.cs`
- `Beep.OilandGas.Models/Data/ChokeAnalysis/ChokeValidationResult.cs`
- `Beep.OilandGas.Models/Data/ChokeAnalysis/GAS_CHOKE_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/ChokeAnalysis/R_CHOKE_ANALYSIS_REFERENCE_CODE.cs`
- `Beep.OilandGas.Models/Data/Common/GAS_COMPOSITION.cs`
- `Beep.OilandGas.Models/Data/Common/GAS_COMPOSITION_COMPONENT.cs`
- `Beep.OilandGas.Models/Data/Common/LIST_OF_VALUE.cs`
- `Beep.OilandGas.Models/Data/Common/OIL_COMPOSITION.cs`
- `Beep.OilandGas.Models/Data/Common/OIL_PROPERTY_RESULT.cs`
- `Beep.OilandGas.Models/Data/Common/PPDM_DEFAULT_VALUE.cs`
- `Beep.OilandGas.Models/Data/Common/PROVED_PROPERTY.cs`
- `Beep.OilandGas.Models/Data/Common/RESERVOIR_STATUS.cs`
- `Beep.OilandGas.Models/Data/Common/R_ANOMALY_TYPE.cs`
- `Beep.OilandGas.Models/Data/Common/R_DISCOVERY_TYPE.cs`
- `Beep.OilandGas.Models/Data/Common/R_ECONOMIC_METRIC.cs`
- `Beep.OilandGas.Models/Data/Common/R_INSPECTION_TYPE.cs`
- `Beep.OilandGas.Models/Data/Common/R_PROSPECT_STATUS.cs`
- `Beep.OilandGas.Models/Data/Common/R_PROSPECT_TYPE.cs`
- `Beep.OilandGas.Models/Data/Common/R_RISK_LEVEL.cs`
- `Beep.OilandGas.Models/Data/Common/R_TRAP_TYPE.cs`
- `Beep.OilandGas.Models/Data/Common/R_WORKFLOW_STAGE.cs`
- `Beep.OilandGas.Models/Data/Common/UNPROVED_PROPERTY.cs`
- `Beep.OilandGas.Models/Data/Compliance/ComplianceDTOs.cs`
- `Beep.OilandGas.Models/Data/DCA/DCAFitResultEnhanced.cs`
- `Beep.OilandGas.Models/Data/DCA/DCAProductionDataPoint.cs`
- `Beep.OilandGas.Models/Data/DCA/DCA_FIT_RESULT.cs`
- `Beep.OilandGas.Models/Data/DataManagement/AccessStatistics.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ArchiveCriteria.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ArchiveResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/BatchValidationRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/BulkOperationError.cs`
- `Beep.OilandGas.Models/Data/DataManagement/BulkOperationResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/CleanupDemoDatabasesResponse.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ConflictResolutionResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ConnectionConfig.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ConnectionInfo.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ConnectionTestResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/CreateDatabaseRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/CreateDemoDatabaseRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/CreateDemoDatabaseResponse.cs`
- `Beep.OilandGas.Models/Data/DataManagement/CurrentConnectionResponse.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DATA_ACCESS_AUDIT.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DATA_QUALITY_ALERT.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DATA_QUALITY_DASHBOARD.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DATA_QUALITY_ISSUE.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DATA_QUALITY_METRICS.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DATA_QUALITY_SCORE.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DATA_QUALITY_TREND.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DATA_VALIDATION_RULE.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DATA_VERSION_COMPARISON.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DATA_VERSION_SNAPSHOT.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DataAccessEvent.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DataLineageInfo.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DataLineageNode.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DataQualityDashboardResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DataQualityRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DataQualityResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DatabaseCreationOptions.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DatabaseCreationResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DeleteDemoDatabaseResponse.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DemoDatabaseConfig.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DemoDatabaseMetadata.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DemoDatabaseStatusResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DemoSeedStageResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DuplicateGroup.cs`
- `Beep.OilandGas.Models/Data/DataManagement/DuplicateRecord.cs`
- `Beep.OilandGas.Models/Data/DataManagement/EntityListResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/EntityResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ExportRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/FieldDifference.cs`
- `Beep.OilandGas.Models/Data/DataManagement/FieldQualityMetrics.cs`
- `Beep.OilandGas.Models/Data/DataManagement/FileImportError.cs`
- `Beep.OilandGas.Models/Data/DataManagement/FileImportResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/GetDefaultsRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/GetVersionHistoryRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/GetWellStatusFacetsRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ImportError.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ImportRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ImportResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ListDemoDatabasesResponse.cs`
- `Beep.OilandGas.Models/Data/DataManagement/MaskingRule.cs`
- `Beep.OilandGas.Models/Data/DataManagement/MaskingStrategy.cs`
- `Beep.OilandGas.Models/Data/DataManagement/MaskingType.cs`
- `Beep.OilandGas.Models/Data/DataManagement/MergeResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/OperationProgress.cs`
- `Beep.OilandGas.Models/Data/DataManagement/OperationResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ProgressUpdate.cs`
- `Beep.OilandGas.Models/Data/DataManagement/QualityAlert.cs`
- `Beep.OilandGas.Models/Data/DataManagement/QualityDashboardData.cs`
- `Beep.OilandGas.Models/Data/DataManagement/QualityTrend.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ReconciliationDifference.cs`
- `Beep.OilandGas.Models/Data/DataManagement/RestoreResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/RestoreVersionRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/RestoreVersionResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ScriptExecutionProgressInfo.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ScriptExecutionResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ScriptInfo.cs`
- `Beep.OilandGas.Models/Data/DataManagement/SetCurrentConnectionRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/SetCurrentConnectionResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/SetupRequests.cs`
- `Beep.OilandGas.Models/Data/DataManagement/SyncConflict.cs`
- `Beep.OilandGas.Models/Data/DataManagement/SyncOptions.cs`
- `Beep.OilandGas.Models/Data/DataManagement/SyncResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/SyncStatus.cs`
- `Beep.OilandGas.Models/Data/DataManagement/TestConnectionRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/TransformationMapping.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ValidationError.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ValidationRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ValidationResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ValidationRule.cs`
- `Beep.OilandGas.Models/Data/DataManagement/ValidationWarning.cs`
- `Beep.OilandGas.Models/Data/DataManagement/VersionComparison.cs`
- `Beep.OilandGas.Models/Data/DataManagement/VersionInfo.cs`
- `Beep.OilandGas.Models/Data/DataManagement/VersionSnapshot.cs`
- `Beep.OilandGas.Models/Data/DataManagement/VersioningRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/VersioningResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/WorkflowDefinition.cs`
- `Beep.OilandGas.Models/Data/DataManagement/WorkflowExecutionRequest.cs`
- `Beep.OilandGas.Models/Data/DataManagement/WorkflowExecutionResult.cs`
- `Beep.OilandGas.Models/Data/DataManagement/WorkflowStep.cs`
- `Beep.OilandGas.Models/Data/Development/DevelopmentWellRequest.cs`
- `Beep.OilandGas.Models/Data/Development/DevelopmentWellResponse.cs`
- `Beep.OilandGas.Models/Data/Development/FacilityRequest.cs`
- `Beep.OilandGas.Models/Data/Development/FeasibilityCashFlowPoint.cs`
- `Beep.OilandGas.Models/Data/Development/FeasibilityProductionPoint.cs`
- `Beep.OilandGas.Models/Data/Development/FeasibilityStudyRequest.cs`
- `Beep.OilandGas.Models/Data/Development/FeasibilityStudyResponse.cs`
- `Beep.OilandGas.Models/Data/Development/PipelineRequest.cs`
- `Beep.OilandGas.Models/Data/Development/PoolRequest.cs`
- `Beep.OilandGas.Models/Data/Development/PoolResponse.cs`
- `Beep.OilandGas.Models/Data/Development/WellboreRequest.cs`
- `Beep.OilandGas.Models/Data/Development/WellboreResponse.cs`
- `Beep.OilandGas.Models/Data/Drilling/CASING_STRING.cs`
- `Beep.OilandGas.Models/Data/Drilling/COMPLETION_STRING.cs`
- `Beep.OilandGas.Models/Data/Drilling/CreateDrillingReport.cs`
- `Beep.OilandGas.Models/Data/Drilling/DRILLING_REPORT.cs`
- `Beep.OilandGas.Models/Data/Drilling/DrillingReport.cs`
- `Beep.OilandGas.Models/Data/Drilling/FacilityConstruction.cs`
- `Beep.OilandGas.Models/Data/Drilling/PERFORATION.cs`
- `Beep.OilandGas.Models/Data/Drilling/WELL_CONSTRUCTION.cs`
- `Beep.OilandGas.Models/Data/EconomicAnalysis/CashFlow.cs`
- `Beep.OilandGas.Models/Data/EconomicAnalysis/CashFlowAnalysis.cs`
- `Beep.OilandGas.Models/Data/EconomicAnalysis/ECONOMIC_ANALYSIS_RESULT.cs`
- `Beep.OilandGas.Models/Data/EconomicAnalysis/ECONOMIC_CASH_FLOW.cs`
- `Beep.OilandGas.Models/Data/EconomicAnalysis/EconomicAnalysisRequests.cs`
- `Beep.OilandGas.Models/Data/EconomicAnalysis/EconomicCashFlowPoint.cs`
- `Beep.OilandGas.Models/Data/EconomicAnalysis/EconomicResult.cs`
- `Beep.OilandGas.Models/Data/EconomicAnalysis/EconomicSensitivityResult.cs`
- `Beep.OilandGas.Models/Data/EconomicAnalysis/NPV_PROFILE_POINT.cs`
- `Beep.OilandGas.Models/Data/EnhancedRecovery/CreateEnhancedRecoveryOperation.cs`
- `Beep.OilandGas.Models/Data/EnhancedRecovery/EnhancedRecoveryOperation.cs`
- `Beep.OilandGas.Models/Data/EnhancedRecovery/GasInjection.cs`
- `Beep.OilandGas.Models/Data/EnhancedRecovery/InjectionOperation.cs`
- `Beep.OilandGas.Models/Data/EnhancedRecovery/InjectionWell.cs`
- `Beep.OilandGas.Models/Data/EnhancedRecovery/WaterFlooding.cs`
- `Beep.OilandGas.Models/Data/Export/EXPORT_HISTORY.cs`
- `Beep.OilandGas.Models/Data/Export/ExportHistoryResponse.cs`
- `Beep.OilandGas.Models/Data/Export/ExportResult.cs`
- `Beep.OilandGas.Models/Data/Export/ExportToCsvRequest.cs`
- `Beep.OilandGas.Models/Data/Export/ExportToExcelRequest.cs`
- `Beep.OilandGas.Models/Data/Export/ExportToJsonRequest.cs`
- `Beep.OilandGas.Models/Data/Export/ScheduleExportRequest.cs`
- `Beep.OilandGas.Models/Data/FieldOrchestrator/Field.cs`
- `Beep.OilandGas.Models/Data/FieldOrchestrator/FieldDashboard.cs`
- `Beep.OilandGas.Models/Data/FieldOrchestrator/FieldDashboardAlert.cs`
- `Beep.OilandGas.Models/Data/FieldOrchestrator/FieldDashboardPhaseSummary.cs`
- `Beep.OilandGas.Models/Data/FieldOrchestrator/FieldLifecycleSummary.cs`
- `Beep.OilandGas.Models/Data/FieldOrchestrator/FieldOrchestratorResponse.cs`
- `Beep.OilandGas.Models/Data/FieldOrchestrator/FieldPerformanceMetric.cs`
- `Beep.OilandGas.Models/Data/FieldOrchestrator/FieldProductionSummary.cs`
- `Beep.OilandGas.Models/Data/FieldOrchestrator/FieldStatistics.cs`
- `Beep.OilandGas.Models/Data/FieldOrchestrator/FieldTimeline.cs`
- `Beep.OilandGas.Models/Data/FieldOrchestrator/FieldTimelineEvent.cs`
- `Beep.OilandGas.Models/Data/FlashCalculations/FLASH_CALCULATION_RESULT.cs`
- `Beep.OilandGas.Models/Data/FlashCalculations/FLASH_COMPONENT.cs`
- `Beep.OilandGas.Models/Data/FlashCalculations/FLASH_CONDITIONS.cs`
- `Beep.OilandGas.Models/Data/FlashCalculations/FlashComponentFraction.cs`
- `Beep.OilandGas.Models/Data/FlashCalculations/FlashComponentKValue.cs`
- `Beep.OilandGas.Models/Data/FlashCalculations/FlashResult.cs`
- `Beep.OilandGas.Models/Data/FlashCalculations/MultiStageFlashRequest.cs`
- `Beep.OilandGas.Models/Data/GasLift/GAS_LIFT_DESIGN.cs`
- `Beep.OilandGas.Models/Data/GasLift/GAS_LIFT_PERFORMANCE.cs`
- `Beep.OilandGas.Models/Data/GasLift/GAS_LIFT_PERFORMANCE_POINT.cs`
- `Beep.OilandGas.Models/Data/GasLift/GAS_LIFT_POTENTIAL_RESULT.cs`
- `Beep.OilandGas.Models/Data/GasLift/GAS_LIFT_VALVE.cs`
- `Beep.OilandGas.Models/Data/GasLift/GAS_LIFT_VALVE_DESIGN_RESULT.cs`
- `Beep.OilandGas.Models/Data/GasLift/GAS_LIFT_WELL_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/GasLift/GasLiftConstraintAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/GasLift/GasLiftEconomicAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/GasLift/GasLiftEconomicPoint.cs`
- `Beep.OilandGas.Models/Data/GasLift/GasLiftPerformanceDiagnosisResult.cs`
- `Beep.OilandGas.Models/Data/GasLift/GasLiftRiskAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/GasLift/GasLiftValveOptimizationResult.cs`
- `Beep.OilandGas.Models/Data/GasLift/GasLiftValveSpacingResult.cs`
- `Beep.OilandGas.Models/Data/GasProperties/AVERAGE_GAS_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/GasProperties/CalculateGasDensityRequest.cs`
- `Beep.OilandGas.Models/Data/GasProperties/CalculateGasFVFRequest.cs`
- `Beep.OilandGas.Models/Data/GasProperties/CalculateZFactorRequest.cs`
- `Beep.OilandGas.Models/Data/GasProperties/ComponentContribution.cs`
- `Beep.OilandGas.Models/Data/GasProperties/GAS_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/GasProperties/GasComponent.cs`
- `Beep.OilandGas.Models/Data/GasProperties/GasComposition.cs`
- `Beep.OilandGas.Models/Data/GasProperties/GasCompressibilityAnalysis.cs`
- `Beep.OilandGas.Models/Data/GasProperties/GasMixtureAnalysis.cs`
- `Beep.OilandGas.Models/Data/GasProperties/GasPropertyMatrix.cs`
- `Beep.OilandGas.Models/Data/GasProperties/GasPropertyResult.cs`
- `Beep.OilandGas.Models/Data/GasProperties/GasSolubilityAnalysis.cs`
- `Beep.OilandGas.Models/Data/GasProperties/GasViscosityAnalysis.cs`
- `Beep.OilandGas.Models/Data/GasProperties/MixtureComponentAnalysis.cs`
- `Beep.OilandGas.Models/Data/GasProperties/PSEUDO_PRESSURE_RESULT.cs`
- `Beep.OilandGas.Models/Data/GasProperties/PropertyValue.cs`
- `Beep.OilandGas.Models/Data/GasProperties/PseudocriticalPropertyAnalysis.cs`
- `Beep.OilandGas.Models/Data/GasProperties/ThermalConductivityAnalysis.cs`
- `Beep.OilandGas.Models/Data/GasProperties/VirialCoefficientAnalysis.cs`
- `Beep.OilandGas.Models/Data/General/Address.cs`
- `Beep.OilandGas.Models/Data/HSE/HSEDTOs.cs`
- `Beep.OilandGas.Models/Data/HeatMap/GradientPoint.cs`
- `Beep.OilandGas.Models/Data/HeatMap/HEAT_MAP_CONFIGURATION.cs`
- `Beep.OilandGas.Models/Data/HeatMap/HEAT_MAP_DATA_POINT.cs`
- `Beep.OilandGas.Models/Data/HeatMap/HeatMapConfigurationRecord.cs`
- `Beep.OilandGas.Models/Data/HeatMap/HeatMapRequests.cs`
- `Beep.OilandGas.Models/Data/HeatMap/HeatMapResult.cs`
- `Beep.OilandGas.Models/Data/HeatMap/TemperatureGradientAnalysis.cs`
- `Beep.OilandGas.Models/Data/HeatMap/TemperatureZone.cs`
- `Beep.OilandGas.Models/Data/HeatMap/ThermalAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/HeatMap/ThermalAnomaly.cs`
- `Beep.OilandGas.Models/Data/HeatMap/ThermalComparisonResult.cs`
- `Beep.OilandGas.Models/Data/HeatMap/ThermalImageQuality.cs`
- `Beep.OilandGas.Models/Data/HeatMap/ThermalTrendAnalysis.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/AnomalyDetection.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/BalanceFactor.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/BalanceRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/CavitationAnalysis.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/CavitationRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/ComparisonCriteria.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/ConditionAssessment.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/ContaminationLevel.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/ContaminationRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/ControlParam.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/ControlParameters.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/ControlRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/CostAnalysisReport.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/CostEstimateRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/CostReportRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/DiagnosticsRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/DiagnosticsResult.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/EfficiencyImprovement.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/EfficiencyRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/FailureModeAnalysis.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/FailureRiskAssessment.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/FiltrationRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/FiltrationSystem.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/FlowCharacteristics.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/FlowRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/FluidAnalysis.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/FluidAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/FluidChangeRecommendation.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/HYDRAULIC_JET_PUMP_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/HYDRAULIC_JET_PUMP_RESULT.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/HYDRAULIC_PISTON_PUMP_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/HYDRAULIC_PISTON_PUMP_RESULT.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/HYDRAULIC_PUMP_WELL_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/HydraulicBalance.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/HydraulicPumpAnalysisAdditionalResults.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/HydraulicPumpAnalysisOptions.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/HydraulicPumpAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/HydraulicPumpAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/HydraulicPumpDesign.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/IntegrationParameter.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/InteractionFactor.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/InteractionRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/MTBFCalculation.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/MaintenanceActivity.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/MaintenanceCostEstimate.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/MaintenanceSchedule.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/MonitoringRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/OptimizationRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/ParameterAdjustment.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PartsInventory.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PartsRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PerformanceAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PerformanceSummaryReport.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PerformanceTrends.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PowerCalculationRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PowerRequirements.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PredictiveMaintenance.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PressureDynamics.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PressureRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpComparison.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpDesignRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpDesignResult.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpEfficiency.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpHistory.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpMonitoringData.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpPerformanceAnalysis.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpPerformanceHistory.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpReport.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpSelectionCriteria.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpSizingRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpSizingResult.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpTypeOption.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpTypeSelection.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/PumpUpgradeRecommendation.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/RebuildAnalysis.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/RebuildRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/ReliabilityAssessment.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/ReliabilityRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/ReportRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/RodStringDesign.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/RodStringRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/SCADAConfig.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/SCODAIntegration.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/ScheduleRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/SummaryReportRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/UpgradeRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/VibrationAnalysis.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/VibrationRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/WearAnalysis.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/WearRequest.cs`
- `Beep.OilandGas.Models/Data/HydraulicPumps/WellboreInteraction.cs`
- `Beep.OilandGas.Models/Data/Imbalance/CreateActualDeliveryRequest.cs`
- `Beep.OilandGas.Models/Data/Imbalance/CreateNominationRequest.cs`
- `Beep.OilandGas.Models/Data/Imbalance/CreateProductionAvailRequest.cs`
- `Beep.OilandGas.Models/Data/Imbalance/ImbalanceReconciliationResult.cs`
- `Beep.OilandGas.Models/Data/Imbalance/ImbalanceSettlementResult.cs`
- `Beep.OilandGas.Models/Data/Imbalance/ImbalanceSummary.cs`
- `Beep.OilandGas.Models/Data/Imbalance/PRODUCTION_AVAIL.cs`
- `Beep.OilandGas.Models/Data/Integrations/IntegrationDTOs.cs`
- `Beep.OilandGas.Models/Data/Inventory/CreateInventoryAdjustmentRequest.cs`
- `Beep.OilandGas.Models/Data/Inventory/CreateInventoryItemRequest.cs`
- `Beep.OilandGas.Models/Data/Inventory/CreateInventoryTransactionRequest.cs`
- `Beep.OilandGas.Models/Data/Inventory/CreateTankInventoryRequest.cs`
- `Beep.OilandGas.Models/Data/Inventory/InventoryReconciliationResult.cs`
- `Beep.OilandGas.Models/Data/Inventory/InventorySummary.cs`
- `Beep.OilandGas.Models/Data/Inventory/UpdateInventoryItemRequest.cs`
- `Beep.OilandGas.Models/Data/Inventory/ValuationMethod.cs`
- `Beep.OilandGas.Models/Data/LeaseManagement/LeaseAgreement.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/ActivatePoolRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/ApproveLeadRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/ApprovePoolRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/AssignReservesRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/CleanoutRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/CleanupSiteRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/CoiledTubingWorkRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/CompleteAbandonmentRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/CompleteDecommissioningRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/ComplianceRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/DECOMMISSIONING_COST.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/DailyOperationsRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/DelineatePoolRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/ENVIRONMENTAL_RESTORATION.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/EvaluateLeadRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/ExplorationWorkflowStepRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FIELD_PHASE.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FacilityCreationRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FacilityEquipmentRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FacilityInspectionRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FacilityIntegrityRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FacilityMaintenanceRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FacilityOperationsRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FacilityPerformanceResponse.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FacilityResponse.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FacilityWorkOrderRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FacilityWorkOrderResponse.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FieldConfigurationRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FieldCreationRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FieldOperationsRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FieldPerformanceRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FieldPerformanceResponse.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/FieldPlanningRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/IncidentRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/InspectionExecutionRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/InspectionFindingRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/InspectionResponse.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/InspectionScheduleRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/MaintenanceExecutionRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/MaintenanceHistoryRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/MaintenanceRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/MaintenanceResponse.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/MaintenanceScheduleRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/ObtainRegulatoryApprovalRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/ObtainRegulatoryClosureRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/OperationsResponse.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/PipelineCapacityResponse.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/PipelineCreationRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/PipelineFlowRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/PipelineInspectionRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/PipelineIntegrityRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/PipelineMaintenanceRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/PipelineOperationsRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/PipelineResponse.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/PlanAbandonmentRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/PlanDecommissioningRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/PlugWellRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/PromoteLeadToProspectRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/RejectLeadRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/RemoveEquipmentRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/RestoreSiteRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/RigWorkoverRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/RiglessWorkoverRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/SafetyAssessmentRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/ShiftHandoverRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/StartDiscoveryToDevelopmentRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/StartFacilityDecommissioningRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/StartFacilityDevelopmentRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/StartLeadToProspectRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/StartPipelineDevelopmentRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/StartPoolDefinitionRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/StartProspectToDiscoveryRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/StartWellAbandonmentRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/StartWellDevelopmentRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/StimulationRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WellCreationRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WellEquipmentRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WellInspectionRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WellMaintenanceRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WellOperationsRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WellPerformanceResponse.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WellPlanningRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WellProperty.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WellResponse.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WellWorkoverResponse.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WirelineWorkRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WorkOrderCreationRequest.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WorkOrderResponse.cs`
- `Beep.OilandGas.Models/Data/LifeCycle/WorkOrderUpdateRequest.cs`
- `Beep.OilandGas.Models/Data/Measurement/MeasurementAccuracy.cs`
- `Beep.OilandGas.Models/Data/Measurement/MeasurementHistory.cs`
- `Beep.OilandGas.Models/Data/Measurement/MeasurementSummary.cs`
- `Beep.OilandGas.Models/Data/Measurement/MeasurementValidationResult.cs`
- `Beep.OilandGas.Models/Data/Measurement/RecordAutomaticMeasurementRequest.cs`
- `Beep.OilandGas.Models/Data/Measurement/RecordManualMeasurementRequest.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/ArtificialLiftRecommendation.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/IPRPoint.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/LateralBranch.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/MonteCarloResults.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/MultilateralDeliverabilityResult.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/MultilateralWellProperties.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/NODAL_ANALYSIS_RESULT.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/NODAL_ANALYSIS_RUN_METADATA.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/NODAL_IPR_POINT.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/NODAL_OPERATING_POINT.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/NODAL_RESERVOIR_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/NODAL_VLP_POINT.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/NODAL_WELLBORE_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/NodalAnalysisParameters.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/NodalAnalysisRunResult.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/OperatingPoint.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/OptimizationGoals.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/OptimizationResult.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/ParameterRanking.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/PerformanceMatchingAnalysis.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/PressureMaintenanceStrategy.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/ProductionScenario.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/R_NODAL_ANALYSIS_REFERENCE_CODE.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/ReservoirProperties.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/SensitivityAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/SensitivityCurvePoint.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/SensitivityScenarioResult.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/TimeStepVLPForecast.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/TornadoAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/VLPForecastTimeStep.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/VLPPoint.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/WellDiagnosticsResult.cs`
- `Beep.OilandGas.Models/Data/NodalAnalysis/WellboreProperties.cs`
- `Beep.OilandGas.Models/Data/OilProperties/BUBBLE_POINT_RESULT.cs`
- `Beep.OilandGas.Models/Data/OilProperties/CalculateDensityRequest.cs`
- `Beep.OilandGas.Models/Data/OilProperties/CalculateFVFRequest.cs`
- `Beep.OilandGas.Models/Data/OilProperties/CalculateOilPropertiesRequest.cs`
- `Beep.OilandGas.Models/Data/OilProperties/CalculateViscosityRequest.cs`
- `Beep.OilandGas.Models/Data/OilProperties/CompressibilityFactorAnalysis.cs`
- `Beep.OilandGas.Models/Data/OilProperties/FluidBehaviorAnalysis.cs`
- `Beep.OilandGas.Models/Data/OilProperties/InterfacialTensionAnalysis.cs`
- `Beep.OilandGas.Models/Data/OilProperties/OIL_PROPERTY_CONDITIONS.cs`
- `Beep.OilandGas.Models/Data/OilProperties/OilComposition.cs`
- `Beep.OilandGas.Models/Data/OilProperties/OilPropertyResult.cs`
- `Beep.OilandGas.Models/Data/OilProperties/PVTSurfaceProperty.cs`
- `Beep.OilandGas.Models/Data/OilProperties/PhaseDiagramAnalysis.cs`
- `Beep.OilandGas.Models/Data/OilProperties/PhasePoint.cs`
- `Beep.OilandGas.Models/Data/OilProperties/PressureRangeProperty.cs`
- `Beep.OilandGas.Models/Data/OilProperties/PropertyCorrelationMatrix.cs`
- `Beep.OilandGas.Models/Data/OilProperties/PropertyTrendAnalysis.cs`
- `Beep.OilandGas.Models/Data/OilProperties/SOLUTION_GOR_RESULT.cs`
- `Beep.OilandGas.Models/Data/OilProperties/TemperatureRangeProperty.cs`
- `Beep.OilandGas.Models/Data/Operations/AnalyzeEOREconomicsRequest.cs`
- `Beep.OilandGas.Models/Data/Operations/AnalyzeEORRequest.cs`
- `Beep.OilandGas.Models/Data/Operations/CalculateRecoveryFactorRequest.cs`
- `Beep.OilandGas.Models/Data/Operations/ManageInjectionRequest.cs`
- `Beep.OilandGas.Models/Data/Operations/RankProspectsRequest.cs`
- `Beep.OilandGas.Models/Data/Ownership/CreateDivisionOrderRequest.cs`
- `Beep.OilandGas.Models/Data/Ownership/CreateOwnershipInterestRequest.cs`
- `Beep.OilandGas.Models/Data/Ownership/CreateTransferOrderRequest.cs`
- `Beep.OilandGas.Models/Data/Ownership/OwnershipApprovalResult.cs`
- `Beep.OilandGas.Models/Data/Ownership/OwnershipChangeHistory.cs`
- `Beep.OilandGas.Models/Data/Ownership/OwnershipChangeRequest.cs`
- `Beep.OilandGas.Models/Data/Ownership/OwnershipChangeResult.cs`
- `Beep.OilandGas.Models/Data/Ownership/OwnershipTree.cs`
- `Beep.OilandGas.Models/Data/Ownership/OwnershipTreeNode.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/AllScriptsExecPayload.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/AllScriptsExecutionResult.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/CopyDatabaseRequest.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/CopyDatabaseResult.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/CreateSchemaResult.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/CreateSqliteRequest.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/CreateSqliteResult.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/DatabaseConnectionListItem.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/DeleteConnectionResult.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/DropDatabaseRequest.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/DropDatabaseResult.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/DummyDataDeleteResponse.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/DummyDataStatusResponse.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/ExecuteScriptRequest.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/GenerateDummyDataRequest.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/GenerateDummyDataResponse.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/LOVImportRequest.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/LovManagementContracts.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/RaTableContracts.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/RecreateDatabaseRequest.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/RecreateDatabaseResult.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/SaveConnectionRequest.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/SaveConnectionResult.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/SchemaMigrationContracts.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/SchemaPrivilegeCheckResult.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/ScriptExecPayload.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/SetCurrentDatabaseRequest.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/SetCurrentDatabaseResult.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/SetupProgressContracts.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/SetupStatusResult.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/GAS_PIPELINE_FLOW_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/LIQUID_PIPELINE_FLOW_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_ANALYSIS_RESULT.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_ANOMALY.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_CAPACITY_RESULT.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_COMPONENT.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_CONSTRUCTION.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_DESIGN.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_FACILITY.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_FIELD.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_FLOW_ANALYSIS_RESULT.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_INCIDENT.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_INSPECTION.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_MAINTENANCE.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_MAINTENANCE_SCHEDULE.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_MATERIAL.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_OPERATION.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_REPAIR.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_RISK_ASSESSMENT.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_ROUTE.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_SEGMENT.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_STATION.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_TEST.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/PIPELINE_WELL.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/R_PIPELINE_STATUS.cs`
- `Beep.OilandGas.Models/Data/PipelineAnalysis/R_PIPELINE_TYPE.cs`
- `Beep.OilandGas.Models/Data/PlungerLift/PLUNGER_LIFT_CYCLE_POINT.cs`
- `Beep.OilandGas.Models/Data/PlungerLift/PLUNGER_LIFT_CYCLE_RESULT.cs`
- `Beep.OilandGas.Models/Data/PlungerLift/PLUNGER_LIFT_GAS_REQUIREMENTS.cs`
- `Beep.OilandGas.Models/Data/PlungerLift/PLUNGER_LIFT_WELL_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/PlungerLift/PlungerLiftAnalysisAdditionalResults.cs`
- `Beep.OilandGas.Models/Data/PlungerLift/PlungerLiftAnalysisOptions.cs`
- `Beep.OilandGas.Models/Data/PlungerLift/PlungerLiftAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/PlungerLift/PlungerLiftAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/PlungerLift/PlungerLiftDesign.cs`
- `Beep.OilandGas.Models/Data/PlungerLift/PlungerLiftOptimization.cs`
- `Beep.OilandGas.Models/Data/PlungerLift/PlungerLiftOptimizationResult.cs`
- `Beep.OilandGas.Models/Data/PlungerLift/PlungerLiftPerformance.cs`
- `Beep.OilandGas.Models/Data/PlungerLift/PlungerLiftPerformanceResult.cs`
- `Beep.OilandGas.Models/Data/Pricing/CreatePriceIndexRequest.cs`
- `Beep.OilandGas.Models/Data/Pricing/PriceIndex.cs`
- `Beep.OilandGas.Models/Data/Pricing/PricingApproval.cs`
- `Beep.OilandGas.Models/Data/Pricing/PricingApprovalResult.cs`
- `Beep.OilandGas.Models/Data/Pricing/PricingReconciliationRequest.cs`
- `Beep.OilandGas.Models/Data/Pricing/PricingReconciliationResult.cs`
- `Beep.OilandGas.Models/Data/Pricing/RegulatedPrice.cs`
- `Beep.OilandGas.Models/Data/Pricing/ValueRunTicketRequest.cs`
- `Beep.OilandGas.Models/Data/Process/PROCESS_DATA.cs`
- `Beep.OilandGas.Models/Data/Process/PROCESS_STEP_DATA.cs`
- `Beep.OilandGas.Models/Data/Production/FieldRequest.cs`
- `Beep.OilandGas.Models/Data/Production/ProductionAllocationRequest.cs`
- `Beep.OilandGas.Models/Data/Production/ProductionForecastRequest.cs`
- `Beep.OilandGas.Models/Data/Production/ProductionForecastResponse.cs`
- `Beep.OilandGas.Models/Data/Production/ProductionRequest.cs`
- `Beep.OilandGas.Models/Data/Production/ProductionResponse.cs`
- `Beep.OilandGas.Models/Data/Production/ReservesRequest.cs`
- `Beep.OilandGas.Models/Data/Production/ReservesResponse.cs`
- `Beep.OilandGas.Models/Data/Production/WellPerformanceAnalysis.cs`
- `Beep.OilandGas.Models/Data/Production/WellTestAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/Production/WellTestAnalysisResponse.cs`
- `Beep.OilandGas.Models/Data/Production/WellTestFlowRequest.cs`
- `Beep.OilandGas.Models/Data/Production/WellTestFlowResponse.cs`
- `Beep.OilandGas.Models/Data/Production/WellTestPressureRequest.cs`
- `Beep.OilandGas.Models/Data/Production/WellTestPressureResponse.cs`
- `Beep.OilandGas.Models/Data/Production/WellTestRequest.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/ACCOUNTING_COST.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/AP_PAYMENT.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/ASSET_RETIREMENT_OBLIGATION.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/CONTRACT_PERFORMANCE_OBLIGATION.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/COST_ALLOCATION.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/COST_CENTER.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/CURRENCY_TRANSLATION_RESULT.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/DEFERRED_TAX_BALANCE.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/DevelopmentCosts.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/EXCHANGE_DELIVERY_POINT.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/EXCHANGE_PARTY.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/EXCHANGE_PRICING_TERMS.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/EXCHANGE_TERMS.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/ExchangeContractType.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/ExplorationCosts.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/IMPAIRMENT_RECORD.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/INVENTORY_ADJUSTMENT.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/INVENTORY_VALUATION.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/INVOICE_LINE_ITEM.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/INVOICE_PAYMENT.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/LEASE_ACCOUNTING_ENTRY.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/LEASE_CONTRACT.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/LEASE_PAYMENT.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/OWNERSHIP_INTEREST.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/PRICE_INDEX.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/ProductionCosts.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/PropertyType.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/ProvedProperty.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/RECEIVABLE.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/ROYALTY_INTEREST.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/TAX_RETURN.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/TAX_TRANSACTION.cs`
- `Beep.OilandGas.Models/Data/ProductionAccounting/UnprovedProperty.cs`
- `Beep.OilandGas.Models/Data/ProductionForecasting/FORECAST_POINT.cs`
- `Beep.OilandGas.Models/Data/ProductionForecasting/GenerateForecastRequest.cs`
- `Beep.OilandGas.Models/Data/ProductionForecasting/PRODUCTION_FORECAST.cs`
- `Beep.OilandGas.Models/Data/ProductionForecasting/PRODUCTION_FORECAST_POINT.cs`
- `Beep.OilandGas.Models/Data/ProductionForecasting/RESERVOIR_FORECAST_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/ProductionForecasting/R_PRODUCTION_FORECASTING_REFERENCE_CODE.cs`
- `Beep.OilandGas.Models/Data/ProductionOperations/PRODUCTION_COSTS.cs`
- `Beep.OilandGas.Models/Data/ProductionOperations/ProductionData.cs`
- `Beep.OilandGas.Models/Data/ProductionOperations/ProductionOperation.cs`
- `Beep.OilandGas.Models/Data/ProductionOperations/ProductionOptimizationRecommendation.cs`
- `Beep.OilandGas.Models/Data/Properties/AsphalteneAnalysis.cs`
- `Beep.OilandGas.Models/Data/Properties/AsphaltenePoint.cs`
- `Beep.OilandGas.Models/Data/Properties/CustodyTransfer.cs`
- `Beep.OilandGas.Models/Data/Properties/EOSComponent.cs`
- `Beep.OilandGas.Models/Data/Properties/EOSPhase.cs`
- `Beep.OilandGas.Models/Data/Properties/EOSResult.cs`
- `Beep.OilandGas.Models/Data/Properties/FluidComponent.cs`
- `Beep.OilandGas.Models/Data/Properties/FluidComposition.cs`
- `Beep.OilandGas.Models/Data/Properties/FluidPropertyAnalysis.cs`
- `Beep.OilandGas.Models/Data/Properties/LabTestResult.cs`
- `Beep.OilandGas.Models/Data/Properties/PVTAnaysisResult.cs`
- `Beep.OilandGas.Models/Data/Properties/PVTDataPoint.cs`
- `Beep.OilandGas.Models/Data/Properties/PVTParameters.cs`
- `Beep.OilandGas.Models/Data/Properties/PhaseEnvelope.cs`
- `Beep.OilandGas.Models/Data/Properties/PhaseEnvelopePoint.cs`
- `Beep.OilandGas.Models/Data/Properties/PropertyResult.cs`
- `Beep.OilandGas.Models/Data/Properties/SampleChainOfCustody.cs`
- `Beep.OilandGas.Models/Data/Properties/SampleInfo.cs`
- `Beep.OilandGas.Models/Data/Properties/SaturationPoint.cs`
- `Beep.OilandGas.Models/Data/Properties/SaturationTestResult.cs`
- `Beep.OilandGas.Models/Data/Properties/TestMeasurement.cs`
- `Beep.OilandGas.Models/Data/Properties/ViscosityCorrelation.cs`
- `Beep.OilandGas.Models/Data/Properties/WaxAnalysis.cs`
- `Beep.OilandGas.Models/Data/Properties/WaxFraction.cs`
- `Beep.OilandGas.Models/Data/ProspectIdentification/Domain/RiskFactor.cs`
- `Beep.OilandGas.Models/Data/PumpPerformance/ESP_CABLE_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/PumpPerformance/ESP_DESIGN_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/PumpPerformance/ESP_DESIGN_RESULT.cs`
- `Beep.OilandGas.Models/Data/PumpPerformance/ESP_MOTOR_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/PumpPerformance/ESP_PUMP_POINT.cs`
- `Beep.OilandGas.Models/Data/Pumps/AnalyzePerformanceRequest.cs`
- `Beep.OilandGas.Models/Data/Pumps/DesignPlungerLiftSystemRequest.cs`
- `Beep.OilandGas.Models/Data/Pumps/DesignPumpSystemRequest.cs`
- `Beep.OilandGas.Models/Data/Pumps/DesignSuckerRodPumpSystemRequest.cs`
- `Beep.OilandGas.Models/Data/Pumps/PumpAnalysisAdditionalResults.cs`
- `Beep.OilandGas.Models/Data/Pumps/PumpAnalysisOptions.cs`
- `Beep.OilandGas.Models/Data/Pumps/PumpAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/Pumps/PumpAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Pumps/PumpPerformancePoint.cs`
- `Beep.OilandGas.Models/Data/Pumps/SRPDiagnosticsResult.cs`
- `Beep.OilandGas.Models/Data/Pumps/SRPOptimizationResult.cs`
- `Beep.OilandGas.Models/Data/Pumps/SuckerRodPumpDesign.cs`
- `Beep.OilandGas.Models/Data/Pumps/SuckerRodPumpPerformance.cs`
- `Beep.OilandGas.Models/Data/Pumps/SuckerRodPumpWellProperties.cs`
- `Beep.OilandGas.Models/Data/Rendering/CHART_CONFIGURATION.cs`
- `Beep.OilandGas.Models/Data/Rendering/ChartConfigurationResponse.cs`
- `Beep.OilandGas.Models/Data/Rendering/ExportChartRequest.cs`
- `Beep.OilandGas.Models/Data/Rendering/ExportChartResult.cs`
- `Beep.OilandGas.Models/Data/Rendering/RenderChartRequest.cs`
- `Beep.OilandGas.Models/Data/Rendering/RenderChartResult.cs`
- `Beep.OilandGas.Models/Data/Rendering/SaveChartConfigurationRequest.cs`
- `Beep.OilandGas.Models/Data/Reporting/GenerateFinancialReportRequest.cs`
- `Beep.OilandGas.Models/Data/Reporting/GenerateJIBStatementRequest.cs`
- `Beep.OilandGas.Models/Data/Reporting/GenerateOperationalReportRequest.cs`
- `Beep.OilandGas.Models/Data/Reporting/GenerateRoyaltyStatementRequest.cs`
- `Beep.OilandGas.Models/Data/Reporting/ReportDistributionRequest.cs`
- `Beep.OilandGas.Models/Data/Reporting/ReportDistributionResult.cs`
- `Beep.OilandGas.Models/Data/Reporting/ReportHistory.cs`
- `Beep.OilandGas.Models/Data/Reporting/ReportResult.cs`
- `Beep.OilandGas.Models/Data/Reporting/ReportSchedule.cs`
- `Beep.OilandGas.Models/Data/Reporting/ScheduleReportRequest.cs`
- `Beep.OilandGas.Models/Data/Royalty/ROYALTY_DISPUTE.cs`
- `Beep.OilandGas.Models/Data/Royalty/ROYALTY_STATEMENT.cs`
- `Beep.OilandGas.Models/Data/Security/PERMISSION.cs`
- `Beep.OilandGas.Models/Data/Security/PERSONA_ROLE.cs`
- `Beep.OilandGas.Models/Data/Security/PermissionConstants.cs`
- `Beep.OilandGas.Models/Data/Security/ROLE.cs`
- `Beep.OilandGas.Models/Data/Security/ROLE_PERMISSION.cs`
- `Beep.OilandGas.Models/Data/Security/SETUP_WIZARD_LOG.cs`
- `Beep.OilandGas.Models/Data/Security/USER.cs`
- `Beep.OilandGas.Models/Data/Security/USER_ASSET_ACCESS.cs`
- `Beep.OilandGas.Models/Data/Security/USER_PROFILE.cs`
- `Beep.OilandGas.Models/Data/Security/USER_ROLE.cs`
- `Beep.OilandGas.Models/Data/SeedData/CategoryInfo.cs`
- `Beep.OilandGas.Models/Data/SeedData/LOVRequest.cs`
- `Beep.OilandGas.Models/Data/SeedData/LOVResponse.cs`
- `Beep.OilandGas.Models/Data/SeedData/ListOfValue.cs`
- `Beep.OilandGas.Models/Data/SeedData/RATablesExportRequest.cs`
- `Beep.OilandGas.Models/Data/SeedData/RATablesExtractResponse.cs`
- `Beep.OilandGas.Models/Data/SeedData/SeedDataCategory.cs`
- `Beep.OilandGas.Models/Data/SeedData/SeedDataRequest.cs`
- `Beep.OilandGas.Models/Data/SeedData/SeedDataResponse.cs`
- `Beep.OilandGas.Models/Data/SeedData/SeedDataValidationRequest.cs`
- `Beep.OilandGas.Models/Data/SeedData/TableSeedResult.cs`
- `Beep.OilandGas.Models/Data/SeedData/WorkflowSeedDataRequirement.cs`
- `Beep.OilandGas.Models/Data/Seismic/AVOAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/Seismic/AVOAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Seismic/AVOCrossplotRequest.cs`
- `Beep.OilandGas.Models/Data/Seismic/AVOCrossplotResult.cs`
- `Beep.OilandGas.Models/Data/Seismic/CoherenceAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/Seismic/CoherenceAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Seismic/CreateSeismicSurvey.cs`
- `Beep.OilandGas.Models/Data/Seismic/DrillingTarget.cs`
- `Beep.OilandGas.Models/Data/Seismic/FluidSubstitutionRequest.cs`
- `Beep.OilandGas.Models/Data/Seismic/FluidSubstitutionResult.cs`
- `Beep.OilandGas.Models/Data/Seismic/ProspectRiskAssessment.cs`
- `Beep.OilandGas.Models/Data/Seismic/SeismicAnomaly.cs`
- `Beep.OilandGas.Models/Data/Seismic/SeismicAttributesRequest.cs`
- `Beep.OilandGas.Models/Data/Seismic/SeismicAttributesResult.cs`
- `Beep.OilandGas.Models/Data/Seismic/SeismicDataQuality.cs`
- `Beep.OilandGas.Models/Data/Seismic/SeismicInterpretationRequest.cs`
- `Beep.OilandGas.Models/Data/Seismic/SeismicInterpretationResult.cs`
- `Beep.OilandGas.Models/Data/Seismic/SeismicInversionRequest.cs`
- `Beep.OilandGas.Models/Data/Seismic/SeismicInversionResult.cs`
- `Beep.OilandGas.Models/Data/Seismic/SeismicReport.cs`
- `Beep.OilandGas.Models/Data/Seismic/SeismicReportRequest.cs`
- `Beep.OilandGas.Models/Data/Seismic/SeismicWellTieRequest.cs`
- `Beep.OilandGas.Models/Data/Seismic/SeismicWellTieResult.cs`
- `Beep.OilandGas.Models/Data/Seismic/SpectralDecompositionRequest.cs`
- `Beep.OilandGas.Models/Data/Seismic/SpectralDecompositionResult.cs`
- `Beep.OilandGas.Models/Data/Seismic/StratigraphicInterpretation.cs`
- `Beep.OilandGas.Models/Data/Seismic/StratigraphicUnit.cs`
- `Beep.OilandGas.Models/Data/Seismic/StructuralFeature.cs`
- `Beep.OilandGas.Models/Data/Seismic/TargetIdentificationRequest.cs`
- `Beep.OilandGas.Models/Data/Seismic/UpdateSeismicSurvey.cs`
- `Beep.OilandGas.Models/Data/Seismic/VolumetricAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/Setup/AreaConfiguration.cs`
- `Beep.OilandGas.Models/Data/Setup/AssetAccessSetupCollection.cs`
- `Beep.OilandGas.Models/Data/Setup/AssetAccessSetupData.cs`
- `Beep.OilandGas.Models/Data/Setup/HierarchyLevelConfig.cs`
- `Beep.OilandGas.Models/Data/Setup/HierarchySetupData.cs`
- `Beep.OilandGas.Models/Data/Setup/OrganizationSetupData.cs`
- `Beep.OilandGas.Models/Data/Setup/RoleAssignmentData.cs`
- `Beep.OilandGas.Models/Data/Setup/ScriptExecutionSetupCollection.cs`
- `Beep.OilandGas.Models/Data/Setup/ScriptExecutionSetupData.cs`
- `Beep.OilandGas.Models/Data/Setup/UserRoleSetupData.cs`
- `Beep.OilandGas.Models/Data/Storage/CreateLACTUnitRequest.cs`
- `Beep.OilandGas.Models/Data/Storage/CreateServiceUnitRequest.cs`
- `Beep.OilandGas.Models/Data/Storage/CreateStorageFacilityRequest.cs`
- `Beep.OilandGas.Models/Data/Storage/CreateTankBatteryRequest.cs`
- `Beep.OilandGas.Models/Data/Storage/LACT_UNIT.cs`
- `Beep.OilandGas.Models/Data/Storage/SERVICE_UNIT.cs`
- `Beep.OilandGas.Models/Data/Storage/STORAGE_FACILITY.cs`
- `Beep.OilandGas.Models/Data/Storage/StorageCapacitySummary.cs`
- `Beep.OilandGas.Models/Data/Storage/StorageUtilizationDetail.cs`
- `Beep.OilandGas.Models/Data/Storage/StorageUtilizationReport.cs`
- `Beep.OilandGas.Models/Data/Storage/TANK_BATTERY.cs`
- `Beep.OilandGas.Models/Data/Stratigraphy/StratColumn.cs`
- `Beep.OilandGas.Models/Data/Stratigraphy/StratColumnUnit.cs`
- `Beep.OilandGas.Models/Data/Stratigraphy/StratHierarchy.cs`
- `Beep.OilandGas.Models/Data/Stratigraphy/StratHierarchyTree.cs`
- `Beep.OilandGas.Models/Data/Stratigraphy/StratHierarchyTreeNode.cs`
- `Beep.OilandGas.Models/Data/Stratigraphy/StratUnit.cs`
- `Beep.OilandGas.Models/Data/Stratigraphy/StratWellSection.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/PumpCard.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/PumpCardAnalysis.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/PumpCardPoint.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/ROD_SECTION.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/RodFatigueAnalysis.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/SUCKER_ROD_FLOW_RATE_POWER_RESULT.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/SUCKER_ROD_LOAD_RESULT.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/SUCKER_ROD_STRING.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/SUCKER_ROD_SYSTEM_PROPERTIES.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/SuckerRodAnalysisAdditionalResults.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/SuckerRodAnalysisOptions.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/SuckerRodAnalysisRequest.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/SuckerRodAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/SuckerRodPumping/SuckerRodAnalyzeRequest.cs`
- `Beep.OilandGas.Models/Data/Trading/CreateExchangeCommitmentRequest.cs`
- `Beep.OilandGas.Models/Data/Trading/CreateExchangeContractRequest.cs`
- `Beep.OilandGas.Models/Data/Trading/CreateExchangeTransactionRequest.cs`
- `Beep.OilandGas.Models/Data/Trading/EXCHANGE_TRANSACTION.cs`
- `Beep.OilandGas.Models/Data/Trading/ExchangeNetPosition.cs`
- `Beep.OilandGas.Models/Data/Trading/ExchangeReconciliation.cs`
- `Beep.OilandGas.Models/Data/Trading/ExchangeReconciliationResult.cs`
- `Beep.OilandGas.Models/Data/Trading/ExchangeSettlementResult.cs`
- `Beep.OilandGas.Models/Data/Trading/ExchangeStatement.cs`
- `Beep.OilandGas.Models/Data/Trading/ExchangeStatementGenerator.cs`
- `Beep.OilandGas.Models/Data/Trading/ExchangeSummary.cs`
- `Beep.OilandGas.Models/Data/Trading/ReconcileExchangeRequest.cs`
- `Beep.OilandGas.Models/Data/Unitization/CreateParticipatingAreaRequest.cs`
- `Beep.OilandGas.Models/Data/Unitization/CreateTractParticipationRequest.cs`
- `Beep.OilandGas.Models/Data/Unitization/CreateUnitAgreementRequest.cs`
- `Beep.OilandGas.Models/Data/Unitization/UnitApprovalResult.cs`
- `Beep.OilandGas.Models/Data/Unitization/UnitOperationsSummary.cs`
- `Beep.OilandGas.Models/Data/UserManagement/ApplyRowFiltersRequest.cs`
- `Beep.OilandGas.Models/Data/UserManagement/CheckDataSourceAccessRequest.cs`
- `Beep.OilandGas.Models/Data/UserManagement/CheckDatabaseAccessRequest.cs`
- `Beep.OilandGas.Models/Data/UserManagement/CheckPermissionAllRequest.cs`
- `Beep.OilandGas.Models/Data/UserManagement/CheckPermissionAnyRequest.cs`
- `Beep.OilandGas.Models/Data/UserManagement/CheckPermissionRequest.cs`
- `Beep.OilandGas.Models/Data/UserManagement/CheckRoleRequest.cs`
- `Beep.OilandGas.Models/Data/UserManagement/CheckRowAccessRequest.cs`
- `Beep.OilandGas.Models/Data/UserManagement/CheckSourceAccessRequest.cs`
- `Beep.OilandGas.Models/Data/Validation/CreateValidationRuleRequest.cs`
- `Beep.OilandGas.Models/Data/Validation/VALIDATION_RESULT.cs`
- `Beep.OilandGas.Models/Data/Validation/ValidationIssue.cs`
- `Beep.OilandGas.Models/Data/Validation/ValidationResultResponse.cs`
- `Beep.OilandGas.Models/Data/Validation/ValidationRuleResponse.cs`
- `Beep.OilandGas.Models/Data/Validation/ValidationSummary.cs`
- `Beep.OilandGas.Models/Data/WellComparison/CompareWellsMultiSourceRequest.cs`
- `Beep.OilandGas.Models/Data/WellComparison/CompareWellsRequest.cs`
- `Beep.OilandGas.Models/Data/WellComparison/ComparisonField.cs`
- `Beep.OilandGas.Models/Data/WellComparison/ComparisonMetadata.cs`
- `Beep.OilandGas.Models/Data/WellComparison/ComparisonValue.cs`
- `Beep.OilandGas.Models/Data/WellComparison/WellComparison.cs`
- `Beep.OilandGas.Models/Data/WellComparison/WellComparisonData.cs`
- `Beep.OilandGas.Models/Data/WellComparison/WellComparisonItem.cs`
- `Beep.OilandGas.Models/Data/WellComparison/WellComparisonResult.cs`
- `Beep.OilandGas.Models/Data/WellSourceMapping/WellSourceMapping.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/AnalysisComparisonResult.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/ComparisonEntry.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/DrawdownAnalysisResult.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/FlowRegimeIndicator.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/MultiRateTestData.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/PRESSURE_TIME_POINT.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/ProductionHistory.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/RateChange.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/ReservoirBoundary.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/TestDataValidationResult.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/TypeCurveMatch.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/TypeCurveMatchResult.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/VariableRateData.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/WELL_TEST_ANALYSIS_RESULT.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/WELL_TEST_DATA.CalculationWire.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/WELL_TEST_DATA.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/WellTestAnalysisAdditionalResults.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/WellTestAnalysisCalculationRequest.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/WellTestAnalysisOptions.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/WellTestAnalysisReport.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/WellTestAnalysisWellKnown.cs`
- `Beep.OilandGas.Models/Data/WellTestAnalysis/WellTestDataPoint.cs`
- `Beep.OilandGas.Models/Enums/AllocationMethod.cs`
- `Beep.OilandGas.Models/Enums/JIBStatus.cs`
- `Beep.OilandGas.Models/Enums/ReceivableStatus.cs`
- `Beep.OilandGas.Models/Enums/Shared/AreaType.cs`
- `Beep.OilandGas.Models/Enums/Shared/CompletionStatus.cs`
- `Beep.OilandGas.Models/Enums/Shared/CurrencyCode.cs`
- `Beep.OilandGas.Models/Enums/Shared/PermitToWorkType.cs`
- `Beep.OilandGas.Models/Enums/Shared/RateUnit.cs`
- `Beep.OilandGas.Models/Enums/Shared/WellIntent.cs`
- `Beep.OilandGas.Models/Enums/Shared/WellStatus.cs`
- `Beep.OilandGas.Models/GlobalUsings.cs`
- `Beep.OilandGas.Models/Processes/ProcessDefinition.cs`
- `Beep.OilandGas.Models/Processes/ProcessHistory.cs`
- `Beep.OilandGas.Models/Processes/ProcessInstance.cs`
- `Beep.OilandGas.Models/Processes/ProcessState.cs`
- `Beep.OilandGas.Models/Processes/ProcessStepDefinition.cs`
- `Beep.OilandGas.Models/Processes/ProcessStepInstance.cs`
- `Beep.OilandGas.Models/Processes/ProcessTransition.cs`
- `Beep.OilandGas.Models/Reservoir/ReservoirLayer.cs`
- `Beep.OilandGas.Models/Schematics/IWellSchematicsLoader.cs`
- `Beep.OilandGas.Models/WellData/WellData_Borehole.cs`
- `Beep.OilandGas.Models/WellData/WellData_Casing.cs`
- `Beep.OilandGas.Models/WellData/WellData_Equip.cs`

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

## Actual restoration checkpoint - 2026-11-04

Source moves applied; unchanged CLR/wire/native names, no retained source copies in Data. Caller project references and stale imports updated where needed. Models build passes; other owner/consumer gates remain pending until recorded in restoration-progress.json.

- `TheTechIdea.Data/OilGas/AccessCheckRequest.cs` -> `Beep.OilandGas.Models/Data/AccessControl/AccessCheckRequest.cs`
- `TheTechIdea.Data/OilGas/AccessControlRequests.cs` -> `Beep.OilandGas.Models/Data/AccessControl/AccessControlRequests.cs`
- `TheTechIdea.Data/OilGas/AR_CREDIT_MEMO.cs` -> `Beep.OilandGas.Models/Data/ProductionAccounting/AR_CREDIT_MEMO.cs`
- `TheTechIdea.Data/OilGas/GenericEntityRequest.cs` -> `Beep.OilandGas.Models/Data/DataManagement/GenericEntityRequest.cs`
- `TheTechIdea.Data/OilGas/GenericEntityResponse.cs` -> `Beep.OilandGas.Models/Data/DataManagement/GenericEntityResponse.cs`
- `TheTechIdea.Data/OilGas/GetAssetHierarchyRequest.cs` -> `Beep.OilandGas.Models/Data/AccessControl/GetAssetHierarchyRequest.cs`
- `TheTechIdea.Data/OilGas/GetEntitiesRequest.cs` -> `Beep.OilandGas.Models/Data/DataManagement/GetEntitiesRequest.cs`
- `TheTechIdea.Data/OilGas/GetEntitiesResponse.cs` -> `Beep.OilandGas.Models/Data/DataManagement/GetEntitiesResponse.cs`
- `TheTechIdea.Data/OilGas/IAssetHierarchyService.cs` -> `Beep.OilandGas.Models/Core/Interfaces/IAssetHierarchyService.cs`
- `TheTechIdea.Data/OilGas/JournalPostingEvidence.cs` -> `Beep.OilandGas.Models/Data/ProductionAccounting/JournalPostingEvidence.cs`
- `TheTechIdea.Data/OilGas/OrganizationHierarchyConfiguration.cs` -> `Beep.OilandGas.Models/Data/Security/ORGANIZATION_HIERARCHY_CONFIG.cs`
- `TheTechIdea.Data/OilGas/ProcessTransitionRequest.cs` -> `Beep.OilandGas.Models/Data/Process/ProcessTransitionRequest.cs`
- `TheTechIdea.Data/OilGas/ProductionDashboardResponse.cs` -> `Beep.OilandGas.Models/Data/Production/ProductionDashboardResponse.cs`
- `TheTechIdea.Data/OilGas/TransitionWorkOrderRequest.cs` -> `Beep.OilandGas.Models/Data/WorkOrder/TransitionWorkOrderRequest.cs`
- `TheTechIdea.Data/OilGas/UpdateLeaseStatusRequest.cs` -> `Beep.OilandGas.Models/Data/Operations/UpdateLeaseStatusRequest.cs`
- `TheTechIdea.Data/OilGas/UpdatePreferencesRequest.cs` -> `Beep.OilandGas.Models/Data/AccessControl/UpdatePreferencesRequest.cs`
- `TheTechIdea.Data/OilGas/UpdatePreferredLayoutRequest.cs` -> `Beep.OilandGas.Models/Data/AccessControl/UpdatePreferredLayoutRequest.cs`
- `TheTechIdea.Data/OilGas/UpdatePrimaryRoleRequest.cs` -> `Beep.OilandGas.Models/Data/AccessControl/UpdatePrimaryRoleRequest.cs`
- `TheTechIdea.Data/OilGas/ValidateAccessRequest.cs` -> `Beep.OilandGas.Models/Data/AccessControl/ValidateAccessRequest.cs`

## Dependency-group implementation - 2026-11-04

Current-owner files below migrated with namespace/caller/source-link updates. Shared contract folders follow existing feature conventions. Duplicate import result/error definitions consolidated into existing plain transport models; no forwarded or parallel result types retained. Exact build/runtime scope and integration failures are recorded in restoration-progress.json.

- `Beep.OilandGas.Models/Data/AccessControl/ApplyRowFiltersRequest.cs` (`Beep.OilandGas.Models.Data.AccessControl`)
- `Beep.OilandGas.Models/Data/AccessControl/AssetAccessLevelChecks.cs` (`Beep.OilandGas.Models.Data.AccessControl`)
- `Beep.OilandGas.Models/Data/AccessControl/AssetHierarchyNode.cs` (`Beep.OilandGas.Models.Data`)
- `Beep.OilandGas.Models/Data/DataManagement/BeepConnectionName.cs` (`Beep.OilandGas.Models.Data.DataManagement`)
- `Beep.OilandGas.Models/Data/AccessControl/CheckDatabaseAccessRequest.cs` (`Beep.OilandGas.Models.Data.AccessControl`)
- `Beep.OilandGas.Models/Data/AccessControl/CheckDataSourceAccessRequest.cs` (`Beep.OilandGas.Models.Data.AccessControl`)
- `Beep.OilandGas.Models/Data/AccessControl/CheckRowAccessRequest.cs` (`Beep.OilandGas.Models.Data.AccessControl`)
- `Beep.OilandGas.Models/Data/AccessControl/CheckSourceAccessRequest.cs` (`Beep.OilandGas.Models.Data.AccessControl`)
- `Beep.OilandGas.Models/Core/Interfaces/IAccessControlService.cs` (`Beep.OilandGas.Models.Core.Interfaces`)
- `Beep.OilandGas.Models/Data/Company/CompanyNativeDecisionHandoffReview.cs` (`Beep.OilandGas.Models.Data.Company`)
- `Beep.OilandGas.Models/Data/DataManagement/CompanyOrganizationReview.cs` (`Beep.OilandGas.Models.Data.DataManagement`)
- `Beep.OilandGas.Models/Data/Company/CompanyRequestHeaders.cs` (`Beep.OilandGas.Models.Data.Company`)
- `Beep.OilandGas.Models/Data/Company/CompanyRequestPolicy.cs` (`Beep.OilandGas.Models.Data.Company`)
- `Beep.OilandGas.Models/Data/Production/CompanyWellQualityReview.cs` (`Beep.OilandGas.Models.Data.Production`)
- `Beep.OilandGas.Models/Data/Reporting/ExecutivePortfolioReview.cs` (`Beep.OilandGas.Models.Data.Reporting`)
- `Beep.OilandGas.Models/Data/Common/ExplicitUtcInput.cs` (`Beep.OilandGas.Models.Data.Common`)
- `Beep.OilandGas.Models/Data/DataManagement/FileImportResult.cs` (`Beep.OilandGas.Models.Data.DataManagement`)
- `Beep.OilandGas.Models/Data/AccessControl/HierarchyConfigurationRules.cs` (`Beep.OilandGas.Models.Data.AccessControl`)
- `Beep.OilandGas.Models/Data/HSE/HseIncidentPage.cs` (`Beep.OilandGas.Models.Data.HSE`)
- `Beep.OilandGas.Models/Core/Interfaces/IFieldSelectionContext.cs` (`Beep.OilandGas.Models.Core.Interfaces`)
- `Beep.OilandGas.Models/Core/Interfaces/IHierarchyConfigurationStore.cs` (`Beep.OilandGas.Models.Core.Interfaces`)
- `Beep.OilandGas.Models/Core/Interfaces/ILeaseAcquisitionService.cs` (`Beep.OilandGas.Models.Core.Interfaces`)
- `Beep.OilandGas.Models/Data/DataManagement/InitialCompanyProvisionRequest.cs` (`Beep.OilandGas.Models.Data.DataManagement`)
- `Beep.OilandGas.Models/Data/Common/RecordedSourceId.cs` (`Beep.OilandGas.Models.Data.Common`)
- `Beep.OilandGas.Models/Data/Production/WellTestReview.cs` (`Beep.OilandGas.Models.Data.Production`)

## Compiler-bound migration checkpoint - 2026-11-04

Files below returned with callers and source links. Incident command interfaces use the consuming data-management Core.Interfaces layer; shared work-order/field facts stay in the existing Models contract layer. Owner builds pass, using existing dependency outputs. Full consumer/provider gates remain open. The retired Data validation bridge is removed. Current internal evidence-validation access is restricted to Accounting, ProductionAccounting and the existing Repository test assembly; it does not cache authority or datasource reads.

- `Beep.OilandGas.Models/Data/AccessControl/AccessCheckResponse.cs` (`Beep.OilandGas.Models.Data`)
- `Beep.OilandGas.Models/Data/AccessControl/AssetAccess.cs` (`Beep.OilandGas.Models.Data`)
- `Beep.OilandGas.Models/Data/Process/CompanyAccountingProcessStartCatalog.cs` (`Beep.OilandGas.Models.Data.Process`)
- `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceQualifiedReturnParticipant.cs` (`Beep.OilandGas.Models.Data.WorkOrder`)
- `Beep.OilandGas.Models/Data/WorkOrder/CompanyMaintenanceRecordedWork.cs` (`Beep.OilandGas.Models.Data.WorkOrder`)
- `Beep.OilandGas.Models/Data/WorkOrder/CompanyWorkOrderExecutionPermitDecision.cs` (`Beep.OilandGas.Models.Data.WorkOrder`)
- `Beep.OilandGas.Models/Data/Lease/CreateLeaseAcquisition.cs` (`Beep.OilandGas.Models.Data.Lease`)
- `Beep.OilandGas.Models/Data/HSE/FieldGhgProductionSources.cs` (`Beep.OilandGas.Models.Data.HSE`)
- `Beep.OilandGas.Models/Data/Lease/FieldInterestReview.cs` (`Beep.OilandGas.Models.Data.Lease`)
- `Beep.OilandGas.Models/Data/Compliance/FieldObligationSourceReview.cs` (`Beep.OilandGas.Models.Data.Compliance`)
- `Beep.OilandGas.Models/Data/AccessControl/HierarchyConfig.cs` (`Beep.OilandGas.Models.Data`)
- `Beep.OilandGas.Models/Data/HSE/HSE_INCIDENT_CLOSURE.cs` (`Beep.OilandGas.Models.Data.HSE`)
- `Beep.OilandGas.Models/Data/Process/ProcessInstanceRequest.cs` (`Beep.OilandGas.Models.Data.Process`)
- `Beep.OilandGas.Models/Data/WorkOrder/ProductionInterventionNativeDecision.cs` (`Beep.OilandGas.Models.Data.WorkOrder`)
- `Beep.OilandGas.Models/Data/AccessControl/UserProfile.cs` (`Beep.OilandGas.Models.Data`)
- `Beep.OilandGas.Models/Data/Production/WellTestResponse.cs` (`Beep.OilandGas.Models.Data`)
- `Beep.OilandGas.Models/Data/WorkOrder/WorkOrderSchema.cs` (`Beep.OilandGas.Models.Data.WorkOrder`)
