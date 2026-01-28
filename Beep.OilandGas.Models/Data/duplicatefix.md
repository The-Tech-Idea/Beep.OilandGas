# Duplicate Class Resolution Plan

**Total Duplicates Found**: 63

## Strategy
1. **Identify Master**: Choose the most relevant namespace/folder (e.g., `Data` vs `ProductionAccounting`).
2. **Merge Properties**: Consolidate unique properties to the master file.
3. **Update References**: Fix usages in the code.
4. **Delete Duplicate**: Remove the redundant file.

## Duplicate Candidates
### Address
- `Data\General\Address.cs`
- `Data\ProductionAccounting\ADDRESS.cs`
- **Action**: [ ] Select Master / Merge / Delete

### AgingAnalysis
- `Data\Accounting\AgingAnalysis.cs`
- `Data\General\AgingAnalysis.cs`
- **Action**: [ ] Select Master / Merge / Delete

### AgingBucket
- `Data\Accounting\AgingBucket.cs`
- `Data\General\AgingBucket.cs`
- **Action**: [ ] Select Master / Merge / Delete

### BankReconciliation
- `Data\Accounting\BankReconciliation.cs`
- `Data\General\BankReconciliation.cs`
- **Action**: [ ] Select Master / Merge / Delete

### CalculateRoyaltyRequest
- `Data\Accounting\Royalty\CalculateRoyaltyRequest.cs`
- `Data\ProductionAccounting\CalculateRoyaltyRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### CeilingTestRequest
- `Data\Accounting\Financial\CeilingTestRequest.cs`
- `Data\ProductionAccounting\CeilingTestRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### CostAllocationRequest
- `Data\Accounting\Cost\CostAllocationRequest.cs`
- `Data\ProductionAccounting\CostAllocationRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### CreateCostTransactionRequest
- `Data\Accounting\CreateCostTransactionRequest.cs`
- `Data\Accounting\Cost\CreateCostTransactionRequest.cs`
- `Data\ProductionAccounting\CreateCostTransactionRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### CreateExchangeContractRequest
- `Data\Accounting\Trading\CreateExchangeContractRequest.cs`
- `Data\Trading\CreateExchangeContractRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### CreateInventoryItemRequest
- `Data\Accounting\CreateInventoryItemRequest.cs`
- `Data\Inventory\CreateInventoryItemRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### CreateInventoryTransactionRequest
- `Data\Accounting\Traditional\CreateInventoryTransactionRequest.cs`
- `Data\Inventory\CreateInventoryTransactionRequest.cs`
- `Data\ProductionAccounting\CreateInventoryTransactionRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### CreateJournalEntryRequest
- `Data\Accounting\CreateJournalEntryRequest.cs`
- `Data\ProductionAccounting\CreateJournalEntryRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### CreateRevenueTransactionRequest
- `Data\Accounting\CreateRevenueTransactionRequest.cs`
- `Data\Accounting\Revenue\CreateRevenueTransactionRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### CreateRoyaltyPaymentRequest
- `Data\Accounting\Royalty\CreateRoyaltyPaymentRequest.cs`
- `Data\ProductionAccounting\CreateRoyaltyPaymentRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### CreateStorageFacilityRequest
- `Data\Accounting\Storage\CreateStorageFacilityRequest.cs`
- `Data\Storage\CreateStorageFacilityRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### CreateUnitAgreementRequest
- `Data\Accounting\Unitization\CreateUnitAgreementRequest.cs`
- `Data\Unitization\CreateUnitAgreementRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### FieldResponse
- `Data\LifeCycle\FieldResponse.cs`
- `Data\Production\FieldResponse.cs`
- **Action**: [ ] Select Master / Merge / Delete

### FlashCalculationPropertyResult
- `Data\Calculations\FlashCalculationPropertyResult.cs`
- `Data\Properties\FlashCalculationPropertyResult.cs`
- **Action**: [ ] Select Master / Merge / Delete

### FullCostAcquisitionRequest
- `Data\Accounting\Financial\FullCostAcquisitionRequest.cs`
- `Data\ProductionAccounting\FullCostAcquisitionRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### FullCostDevelopmentRequest
- `Data\Accounting\Financial\FullCostDevelopmentRequest.cs`
- `Data\ProductionAccounting\FullCostDevelopmentRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### FullCostExplorationRequest
- `Data\Accounting\Financial\FullCostExplorationRequest.cs`
- `Data\ProductionAccounting\FullCostExplorationRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### GasLiftPerformancePoint
- `Data\Calculations\GasLiftPerformancePoint.cs`
- `Data\GasLift\GasLiftPerformancePoint.cs`
- **Action**: [ ] Select Master / Merge / Delete

### GenerateJIBStatementRequest
- `Data\ProductionAccounting\GenerateJIBStatementRequest.cs`
- `Data\Reporting\GenerateJIBStatementRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### GenerateLeaseReportRequest
- `Data\Accounting\Reporting\GenerateLeaseReportRequest.cs`
- `Data\ProductionAccounting\GenerateLeaseReportRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### GenerateOperationalReportRequest
- `Data\Accounting\Reporting\GenerateOperationalReportRequest.cs`
- `Data\ProductionAccounting\GenerateOperationalReportRequest.cs`
- `Data\Reporting\GenerateOperationalReportRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### ImbalanceSummary
- `Data\Imbalance\ImbalanceSummary.cs`
- `Data\ProductionAccounting\ImbalanceSummary.cs`
- **Action**: [ ] Select Master / Merge / Delete

### ImpairmentRequest
- `Data\Accounting\Financial\ImpairmentRequest.cs`
- `Data\ProductionAccounting\ImpairmentRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### IntercompanyReconciliation
- `Data\Accounting\IntercompanyReconciliation.cs`
- `Data\General\IntercompanyReconciliation.cs`
- **Action**: [ ] Select Master / Merge / Delete

### InterestCapitalizationData
- `Data\Calculations\InterestCapitalizationData.cs`
- `Data\ProductionAccounting\InterestCapitalizationData.cs`
- **Action**: [ ] Select Master / Merge / Delete

### Lease
- `Data\Lease\Lease.cs`
- `Data\ProductionAccounting\Lease.cs`
- **Action**: [ ] Select Master / Merge / Delete

### LeaseAgreement
- `Data\LeaseManagement\LeaseAgreement.cs`
- `Data\ProductionAccounting\LeaseAgreement.cs`
- **Action**: [ ] Select Master / Merge / Delete

### MaintenanceRequest
- `Data\HydraulicPumps\MaintenanceRequest.cs`
- `Data\LifeCycle\MaintenanceRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### MeasurementAccuracy
- `Data\Measurement\MeasurementAccuracy.cs`
- `Data\ProductionAccounting\MeasurementAccuracy.cs`
- **Action**: [ ] Select Master / Merge / Delete

### OutstandingItem
- `Data\Accounting\OutstandingItem.cs`
- `Data\General\OutstandingItem.cs`
- **Action**: [ ] Select Master / Merge / Delete

### OwnershipTreeNode
- `Data\Ownership\OwnershipTreeNode.cs`
- `Data\ProductionAccounting\OwnershipTreeNode.cs`
- **Action**: [ ] Select Master / Merge / Delete

### PeriodClosingResult
- `Data\Accounting\PeriodClosingResult.cs`
- `Data\General\PeriodClosingResult.cs`
- **Action**: [ ] Select Master / Merge / Delete

### PeriodClosingStatus
- `Data\Accounting\PeriodClosingStatus.cs`
- `Data\General\PeriodClosingStatus.cs`
- **Action**: [ ] Select Master / Merge / Delete

### PeriodClosingValidation
- `Data\Accounting\PeriodClosingValidation.cs`
- `Data\General\PeriodClosingValidation.cs`
- **Action**: [ ] Select Master / Merge / Delete

### PipelineResponse
- `Data\Development\PipelineResponse.cs`
- `Data\LifeCycle\PipelineResponse.cs`
- **Action**: [ ] Select Master / Merge / Delete

### PlungerLiftWellProperties
- `Data\PlungerLift\PlungerLiftWellProperties.cs`
- `Data\Pumps\PlungerLiftWellProperties.cs`
- **Action**: [ ] Select Master / Merge / Delete

### PostingResult
- `Data\Accounting\PostingResult.cs`
- `Data\General\PostingResult.cs`
- **Action**: [ ] Select Master / Merge / Delete

### PressureDropResult
- `Data\Calculations\PressureDropResult.cs`
- `Data\PipelineAnalysis\PressureDropResult.cs`
- **Action**: [ ] Select Master / Merge / Delete

### PriceIndex
- `Data\Pricing\PriceIndex.cs`
- `Data\ProductionAccounting\PriceIndex.cs`
- **Action**: [ ] Select Master / Merge / Delete

### PriceIndexRequest
- `Data\Accounting\Pricing\PriceIndexRequest.cs`
- `Data\ProductionAccounting\PriceIndexRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### ProductionData
- `Data\Calculations\ProductionData.cs`
- `Data\ProductionAccounting\ProductionData.cs`
- `Data\ProductionOperations\ProductionData.cs`
- **Action**: [ ] Select Master / Merge / Delete

### ProductionForecast
- `Data\NodalAnalysis\ProductionForecast.cs`
- `Data\ProductionForecasting\ProductionForecast.cs`
- **Action**: [ ] Select Master / Merge / Delete

### ProvedReserves
- `Data\Calculations\ProvedReserves.cs`
- `Data\ProductionAccounting\ProvedReserves.cs`
- **Action**: [ ] Select Master / Merge / Delete

### ReconciliationResult
- `Data\Accounting\ReconciliationResult.cs`
- `Data\DataManagement\ReconciliationResult.cs`
- **Action**: [ ] Select Master / Merge / Delete

### ReconciliationSummary
- `Data\Accounting\ReconciliationSummary.cs`
- `Data\General\ReconciliationSummary.cs`
- **Action**: [ ] Select Master / Merge / Delete

### ReconciliationVariance
- `Data\Accounting\ReconciliationVariance.cs`
- `Data\General\ReconciliationVariance.cs`
- **Action**: [ ] Select Master / Merge / Delete

### RegisterOwnershipInterestRequest
- `Data\Accounting\Ownership\RegisterOwnershipInterestRequest.cs`
- `Data\ProductionAccounting\RegisterOwnershipInterestRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### RegulatedPrice
- `Data\Pricing\RegulatedPrice.cs`
- `Data\ProductionAccounting\RegulatedPrice.cs`
- **Action**: [ ] Select Master / Merge / Delete

### SalesTransaction
- `Data\Accounting\SalesTransaction.cs`
- `Data\ProductionAccounting\SalesTransaction.cs`
- **Action**: [ ] Select Master / Merge / Delete

### SeedDataRequest
- `Data\DataManagement\SeedDataRequest.cs`
- `Data\SeedData\SeedDataRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### SeedDataResponse
- `Data\DataManagement\SeedDataResponse.cs`
- `Data\SeedData\SeedDataResponse.cs`
- **Action**: [ ] Select Master / Merge / Delete

### SensitivityAnalysis
- `Data\Calculations\SensitivityAnalysis.cs`
- `Data\Evaluation\SensitivityAnalysis.cs`
- **Action**: [ ] Select Master / Merge / Delete

### SensitivityAnalysisResult
- `Data\Calculations\SensitivityAnalysisResult.cs`
- `Data\NodalAnalysis\SensitivityAnalysisResult.cs`
- **Action**: [ ] Select Master / Merge / Delete

### UnmatchedTransaction
- `Data\Accounting\UnmatchedTransaction.cs`
- `Data\General\UnmatchedTransaction.cs`
- **Action**: [ ] Select Master / Merge / Delete

### UnreconciledItem
- `Data\Accounting\UnreconciledItem.cs`
- `Data\General\UnreconciledItem.cs`
- **Action**: [ ] Select Master / Merge / Delete

### ValidationResult
- `Data\DataManagement\ValidationResult.cs`
- `Data\General\ValidationResult.cs`
- **Action**: [ ] Select Master / Merge / Delete

### ValueRunTicketRequest
- `Data\Accounting\Pricing\ValueRunTicketRequest.cs`
- `Data\Pricing\ValueRunTicketRequest.cs`
- `Data\ProductionAccounting\ValueRunTicketRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

### WellAllocationData
- `Data\Allocation\WellAllocationData.cs`
- `Data\ProductionAccounting\WellAllocationData.cs`
- **Action**: [ ] Select Master / Merge / Delete

### WellTestRequest
- `Data\LifeCycle\WellTestRequest.cs`
- `Data\Production\WellTestRequest.cs`
- **Action**: [ ] Select Master / Merge / Delete

