# Beep.OilandGas.ProductionAccounting — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

13 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

| Original source | Current definition | Namespace |
| --- | --- | --- |
| `TheTechIdea.Data/OilGas/AFE_LINE_ITEM.cs` | `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AFE_LINE_ITEM.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting` |
| `TheTechIdea.Data/OilGas/CompanyCommercialAmendmentPage.cs` | `Beep.OilandGas.ProductionAccounting/Models/CompanyCommercialAmendmentPage.cs` | `Beep.OilandGas.ProductionAccounting.Models` |
| `TheTechIdea.Data/OilGas/CompanyCommercialHandoffReview.cs` | `Beep.OilandGas.ProductionAccounting/Models/CompanyCommercialHandoffReview.cs` | `Beep.OilandGas.ProductionAccounting.Models` |
| `TheTechIdea.Data/OilGas/CompanyDeliveryProcessSourceReview.cs` | `Beep.OilandGas.ProductionAccounting/Models/CompanyDeliveryProcessSourceReview.cs` | `Beep.OilandGas.ProductionAccounting.Models` |
| `TheTechIdea.Data/OilGas/CompanyDeliverySavedWork.cs` | `Beep.OilandGas.ProductionAccounting/Models/CompanyDeliverySavedWork.cs` | `Beep.OilandGas.ProductionAccounting.Models` |
| `TheTechIdea.Data/OilGas/RevenueDeliveryAcceptanceSavedReview.cs` | `Beep.OilandGas.ProductionAccounting/Models/RevenueDeliveryAcceptanceSavedReview.cs` | `Beep.OilandGas.ProductionAccounting.Models` |
| `TheTechIdea.Data/OilGas/CompanyCommercialAmendmentQueue.cs` | `Beep.OilandGas.ProductionAccounting/Models/CompanyCommercialAmendmentQueue.cs` | `Beep.OilandGas.ProductionAccounting.Models` |
| `TheTechIdea.Data/OilGas/DELIVERY_ACCEPTANCE_INTENT.cs` | `Beep.OilandGas.ProductionAccounting/Models/DELIVERY_ACCEPTANCE_INTENT.cs` | `Beep.OilandGas.ProductionAccounting.Models` |
| `TheTechIdea.Data/OilGas/COMMERCIAL_TERMS_INTENT.cs` | `Beep.OilandGas.ProductionAccounting/Models/COMMERCIAL_TERMS_INTENT.cs` | `Beep.OilandGas.ProductionAccounting.Models` |
| `TheTechIdea.Data/OilGas/CompanyCommercialProcessSourceReview.cs` | `Beep.OilandGas.ProductionAccounting/Models/CompanyCommercialProcessSourceReview.cs` | `Beep.OilandGas.ProductionAccounting.Models` |
| `TheTechIdea.Data/OilGas/RevenueDeliveryAcceptanceIntent.cs` | `Beep.OilandGas.ProductionAccounting/Models/RevenueDeliveryAcceptanceIntent.cs` | `Beep.OilandGas.ProductionAccounting.Models` |
| `TheTechIdea.Data/OilGas/CompanyCommercialAmendmentReview.cs` | `Beep.OilandGas.ProductionAccounting/Models/CompanyCommercialAmendmentReview.cs` | `Beep.OilandGas.ProductionAccounting.Models` |
| `TheTechIdea.Data/OilGas/CompanyDeliveryCorrectionReview.cs` | `Beep.OilandGas.ProductionAccounting/Models/CompanyDeliveryCorrectionReview.cs` | `Beep.OilandGas.ProductionAccounting.Models` |

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.ProductionAccounting/Beep.OilandGas.ProductionAccounting.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

10 candidate incoming files; 307 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |
| `TheTechIdea.Data/OilGas/AFE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: AFE | Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AFE.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/AFE_LINE_ITEM.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: AFE_LINE_ITEM | Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AFE_LINE_ITEM.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ALLOCATION_DETAIL.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: ALLOCATION_DETAIL | Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ALLOCATION_DETAIL.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ALLOCATION_RESULT.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: ALLOCATION_RESULT | Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ALLOCATION_RESULT.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/AllocationMethod.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: AllocationMethod | Beep.OilandGas.Models/Enums/AllocationMethod.cs; Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AllocationMethod.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/AllocationRunTicketCompatibility.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: ALLOCATION_DETAIL, ALLOCATION_RESULT, RUN_TICKET | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/APPROVAL_WORKFLOW.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: APPROVAL_WORKFLOW | Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/APPROVAL_WORKFLOW.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/REVENUE_TRANSACTION.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: REVENUE_TRANSACTION | Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/REVENUE_TRANSACTION.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/RUN_TICKET.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: RUN_TICKET | Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/RUN_TICKET.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/SALES_CONTRACT.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: SALES_CONTRACT | Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/SALES_CONTRACT.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.ProductionAccounting/Constants/AccountingSourceModuleCodes.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/AfeStatusCodes.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/AmortizationMethods.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/ApprovalWorkflowStatusCodes.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/BoeConversionFactors.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/CostCategories.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/CostTypes.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/DocumentWorkflowStatusCodes.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/DrillingScenarioPhraseCodes.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/FullCostAccountingConstants.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/InventoryReferenceCodes.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/JournalEntryStatusCodes.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/JournalEntryTypeCodes.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/LeaseEconomicInterestConstants.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/MeasurementServiceConstants.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/ProductionAccountingDescriptionPhrases.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/ProductionAccountingReferenceCodeSeed.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/ProductionSharingAgreementDefaults.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/ReportingConstants.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/ReserveAccountingConstants.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/RevenueRecognitionStatusCodes.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/RoyaltyDeductionCostTypeCodes.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/RoyaltyPaymentStatusCodes.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/RoyaltyStatementStatusCodes.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/RoyaltyStatus.cs`
- `Beep.OilandGas.ProductionAccounting/Constants/WorkflowReferenceCodes.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IAPService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IAccountReconciliationService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IAccountingService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IAfeService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IAllocationEngine.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IAllocationService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IAmortizationService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IAssetSwapService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IAuthorizationWorkflowService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IBorrowingCostCapitalizationService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IContractPerformanceService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/ICopasOverheadService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IDrillingScenarioAccountingService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IEmissionsTradingService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IExplorationEvaluationService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IFinancialInstrumentsService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IFullCostService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IFunctionalCurrencyService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IGLAccountService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IImbalanceService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IImpairmentTestingService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IInternalControlService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IInventoryService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IJointInterestBillingService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/ILeasingService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IMeasurementService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IOwnershipService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IPeriodClosingService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IPricingService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IProductionAccountingService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IProductionAccountingValidator.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IProductionService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IProductionSharingService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IProductionTaxService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IPurchaseOrderService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IReserveAccountingService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IReserveDisclosureService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IRevenueService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IRoyaltyService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/ISuccessfulEffortsService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/ITakeOrPayService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/ITradingService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IUnitizationService.cs`
- `Beep.OilandGas.ProductionAccounting/Core/Interfaces/IUnprovedPropertyService.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ACCOUNTING_AMORTIZATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ACTUAL_DELIVERY.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AFE.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AFE_LINE_ITEM.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ALLOCATION_REPORT_SUMMARY.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AMORTIZATION_RECORD.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AMORTIZATION_SPLIT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/APPROVAL_WORKFLOW.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AP_CREDIT_MEMO.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ASSET_SWAP_TRANSACTION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AVAILS_ALLOCATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AccountingStatusData.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AllocationDetail.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AllocationEntityRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AllocationRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AllocationResult.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AmortizationCalculationRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AmortizationSchedule.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AmortizationSchedulePeriod.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AmortizationSummary.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CEILING_TEST_CALCULATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CONTACT_INFORMATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/COPAS_OVERHEAD_AUDIT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/COPAS_OVERHEAD_SCHEDULE.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/COST_REPORT_SUMMARY.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/COST_SHARING.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/COST_TRANSACTION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/COST_VARIANCE_REPORT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CRUDE_OIL_INVENTORY.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CRUDE_OIL_INVENTORY_TRANSACTION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CRUDE_OIL_PROPERTIES.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CRUDE_OIL_SPECIFICATIONS.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CalculateAmortizationRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CeilingTestResult.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CostAllocationTarget.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CostCenterRollup.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CreateAutomaticMeasurementRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CreateCostCenterRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CreateLeaseRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CreateManualMeasurementRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CreateRunTicketRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CreateSalesTransactionRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CreateTankInventoryRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/CrudeOilClassification.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/DELAY_RENTAL.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/DEPLETION_CALCULATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/DEPLETION_ROLLFORWARD.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/DIVISION_ORDER.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/DeductionsSummary.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/DeliveryInformation.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/DeliveryTerms.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/DivisionOrderStatus.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/EMISSIONS_ALLOWANCE.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/EMISSIONS_OBLIGATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/EMISSIONS_SETTLEMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/EXCHANGE_COMMITMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/EXCHANGE_CONTRACT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/FINANCIAL_INSTRUMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/FINANCIAL_REPORT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/Form1099Info.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/GOVERNMENTAL_PRODUCTION_DATA.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/GOVERNMENTAL_REPORT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/GOVERNMENTAL_ROYALTY_DATA.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/GOVERNMENTAL_TAX_DATA.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/GenerateGovernmentalReportRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/GenerateScheduleRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/GovernmentLease.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/HEDGE_MEASUREMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/HEDGE_RELATIONSHIP.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/IMBALANCE_ADJUSTMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/IMBALANCE_RECONCILIATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/IMBALANCE_STATEMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/IMBALANCE_SUMMARY.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/INTERNAL_CONTROL_RULE.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/INVENTORY_REPORT_SUMMARY.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/Imbalance.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ImpairmentResult.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/Inventory.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/JIB_CHARGE.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/JIB_COST_ALLOCATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/JIB_CREDIT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/JIB_PARTICIPANT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/JOA_INTEREST.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/JOIB_ALLOCATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/JOIB_LINE_ITEM.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/JOINT_INTEREST_BILL.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/JOINT_INTEREST_STATEMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/JOINT_OPERATING_AGREEMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/JointInterestOwner.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/JointInterestParticipant.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/JournalEntryLineRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/LACTTransferRecord.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/LACTUnit.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/LEASEHOLD_CARRYING_GROUP.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/LEASE_ALLOCATION_DATA.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/LEASE_EXPIRY_EVENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/LEASE_OPTION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/LEASE_REPORT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/LOCATION_ADJUSTMENTS.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/LeaseLocation.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/LeaseProvisions.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/MEASUREMENT_ACCURACY.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/MEASUREMENT_CORRECTIONS.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/MEASUREMENT_RECORD.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/MEASUREMENT_REPORT_SUMMARY.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/Measurement.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/MeasurementStandard.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/MeterConfiguration.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/NOMINATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/NetProfitCostRecovery.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/OIL_IMBALANCE.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/OPERATIONAL_REPORT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/OWNER_INFORMATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/OilSalesAgreement.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/PARTICIPATING_AREA.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/PRODUCTION_ALLOCATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/PRODUCTION_AVAILS.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/PRODUCTION_REPORT_SUMMARY.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/PRODUCTION_SHARING_AGREEMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/PRODUCTION_SHARING_ENTITLEMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/PROVED_RESERVES.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/PricingTerms.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ProcessingAgreement.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ProductionAvail.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ProductionMarketingCosts.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ProductionSummary.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ProductionTax.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/QUALITY_ADJUSTMENTS.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/QualityMeasurementSystem.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/REGULATED_PRICE.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/RESERVE_CASHFLOW.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/RESERVE_DISCLOSURE_PACKAGE.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/REVENUE_ALLOCATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/REVENUE_DEDUCTION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/REVENUE_DISTRIBUTION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/REVENUE_SHARING.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ROYALTY_CALCULATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ROYALTY_DEDUCTIONS.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ROYALTY_OWNER.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ROYALTY_PAYMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ROYALTY_PAYMENT_DETAIL.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/RUN_TICKET_VALUATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/R_PRODUCTION_ACCOUNTING_REFERENCE_CODE.Table.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ReconcileImbalanceRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/Report.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/RevenueSummary.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/RoyaltyStatement.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/RunTicketValuationModel.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ServiceUnit.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/ServiceUnitParticipant.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/StorageAgreement.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/StorageFacility.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TAKE_OR_PAY_BALANCE.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TAKE_OR_PAY_SCHEDULE.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TANK_INVENTORY.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TAX_ADJUSTMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TAX_WITHHOLDING.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TIME_ADJUSTMENTS.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TRACT_ALLOCATION_DATA.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TRACT_PARTICIPATION.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TRANSFER_ORDER.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/Tank.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TankBattery.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TankInventoryResponse.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TaxWithholdingType.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TestResult.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TestSeparator.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/TransportationAgreement.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/UNIT_AGREEMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/UNIT_OPERATING_AGREEMENT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/UNIT_PARTICIPANT.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/VOTING_RIGHTS.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/VolumeReconciliationRequest.cs`
- `Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/WELL_ALLOCATION_DATA.cs`
- `Beep.OilandGas.ProductionAccounting/Exceptions/AccountingException.cs`
- `Beep.OilandGas.ProductionAccounting/Exceptions/AllocationException.cs`
- `Beep.OilandGas.ProductionAccounting/Exceptions/ProductionAccountingException.cs`
- `Beep.OilandGas.ProductionAccounting/Exceptions/RoyaltyException.cs`
- `Beep.OilandGas.ProductionAccounting/Exceptions/ValidationException.cs`
- `Beep.OilandGas.ProductionAccounting/Modules/ProductionAccountingFinancialManifest.cs`
- `Beep.OilandGas.ProductionAccounting/Modules/ProductionAccountingModuleSetup.cs`
- `Beep.OilandGas.ProductionAccounting/Services/AfeService.Development.cs`
- `Beep.OilandGas.ProductionAccounting/Services/AfeService.DevelopmentIndex.cs`
- `Beep.OilandGas.ProductionAccounting/Services/AfeService.WorkOrderBudget.cs`
- `Beep.OilandGas.ProductionAccounting/Services/AfeService.WorkOrderLaborCost.cs`
- `Beep.OilandGas.ProductionAccounting/Services/AfeService.WorkOrderMaterialCost.cs`
- `Beep.OilandGas.ProductionAccounting/Services/AfeService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/AllocationEngine.cs`
- `Beep.OilandGas.ProductionAccounting/Services/AllocationService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/AmortizationService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/AssetSwapService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/AuthorizationWorkflowService.DevelopmentInvestment.cs`
- `Beep.OilandGas.ProductionAccounting/Services/AuthorizationWorkflowService.WorkOrderBudget.cs`
- `Beep.OilandGas.ProductionAccounting/Services/AuthorizationWorkflowService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/BorrowingCostCapitalizationService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ContractPerformanceService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/CopasOverheadService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/DecommissioningService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/DrillingScenarioAccountingService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/EmissionsTradingService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ExplorationEvaluationService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/FinancialInstrumentsService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/FullCostService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/FunctionalCurrencyService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ImbalanceService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ImpairmentTestingService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/InternalControlService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/InventoryLcmService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/InventoryService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/JointInterestBillingService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/LeaseEconomicInterestService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/LeasingService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/MeasurementService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/PeriodCloseJournalRecords.cs`
- `Beep.OilandGas.ProductionAccounting/Services/PeriodClosingService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/PricingService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ProductionAccountingService.AfeQueries.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ProductionAccountingService.CommercialIntent.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ProductionAccountingService.CommercialTerms.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ProductionAccountingService.Compatibility.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ProductionAccountingService.ControllerFacade.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ProductionAccountingService.DeliveryAcceptance.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ProductionAccountingService.DeliveryIntent.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ProductionAccountingService.LegacyLedgerFacade.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ProductionAccountingService.RevenueSources.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ProductionAccountingService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ProductionSharingService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ProductionTaxService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ReportingService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ReserveAccountingService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/ReserveDisclosureService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/RevenueService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/RoyaltyService.PostingReview.cs`
- `Beep.OilandGas.ProductionAccounting/Services/RoyaltyService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/SuccessfulEffortsService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/TakeOrPayService.cs`
- `Beep.OilandGas.ProductionAccounting/Services/UnprovedPropertyService.cs`

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

- `Beep.OilandGas.ProductionAccounting/Models/CompanyCommercialAmendmentPage.cs` (`Beep.OilandGas.ProductionAccounting.Models`)
- `Beep.OilandGas.ProductionAccounting/Models/CompanyCommercialHandoffReview.cs` (`Beep.OilandGas.ProductionAccounting.Models`)
- `Beep.OilandGas.ProductionAccounting/Models/CompanyDeliveryProcessSourceReview.cs` (`Beep.OilandGas.ProductionAccounting.Models`)
- `Beep.OilandGas.ProductionAccounting/Models/CompanyDeliverySavedWork.cs` (`Beep.OilandGas.ProductionAccounting.Models`)
- `Beep.OilandGas.ProductionAccounting/Models/RevenueDeliveryAcceptanceSavedReview.cs` (`Beep.OilandGas.ProductionAccounting.Models`)

## Compiler-bound migration checkpoint - 2026-11-04

Files below returned with callers and source links. Incident command interfaces use the consuming data-management Core.Interfaces layer; shared work-order/field facts stay in the existing Models contract layer. Owner builds pass, using existing dependency outputs. Full consumer/provider gates remain open. The retired Data validation bridge is removed. Current internal evidence-validation access is restricted to Accounting, ProductionAccounting and the existing Repository test assembly; it does not cache authority or datasource reads.

- `Beep.OilandGas.ProductionAccounting/Models/CompanyCommercialAmendmentQueue.cs` (`Beep.OilandGas.ProductionAccounting.Models`)
- `Beep.OilandGas.ProductionAccounting/Models/DELIVERY_ACCEPTANCE_INTENT.cs` (`Beep.OilandGas.ProductionAccounting.Models`)
