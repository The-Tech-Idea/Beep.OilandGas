# ProductionAccounting Gap Matrix (Docs vs Services)

Status: Implemented, Partial, Missing, External

## Financial & Accounting Plan Alignment

| Feature | Planned Service/Module | Evidence | Status | Notes |
|---|---|---|---|---|
| Successful Efforts | SuccessfulEffortsService | Beep.OilandGas.ProductionAccounting/Services/SuccessfulEffortsService.cs | Implemented | Core logic present. |
| Full Cost | FullCostService | Beep.OilandGas.ProductionAccounting/Services/FullCostService.cs | Implemented | Ceiling test + depletion present. |
| Amortization/Depletion | AmortizationService | Beep.OilandGas.ProductionAccounting/Services/AmortizationService.cs | Implemented | Unit-of-production logic present. |
| Revenue Recognition | RevenueService | Beep.OilandGas.ProductionAccounting/Services/RevenueService.cs | Implemented | Allocation validation + recognition present. |
| Journal Entries & GL | JournalEntryService | Beep.OilandGas.Accounting/Services/JournalEntryService.cs | External | Implemented in Accounting. |
| Period Close | PeriodClosingService | Beep.OilandGas.Accounting/Services/PeriodClosingService.cs | External | Implemented in Accounting. |
| Authorization Workflow | AuthorizationWorkflowService | Beep.OilandGas.ProductionAccounting/Services/AuthorizationWorkflowService.cs | Implemented | Approval checks present. |
| Lease & Economic Interest | LeaseEconomicInterestService | Beep.OilandGas.ProductionAccounting/Services/LeaseEconomicInterestService.cs | Implemented | Ownership validation present. |
| Reserve Accounting | ReserveAccountingService | Beep.OilandGas.ProductionAccounting/Services/ReserveAccountingService.cs | Implemented | Reserve input storage present. |

## IFRS & Reporting Extensions

| Feature | Planned Service/Module | Evidence | Status | Notes |
|---|---|---|---|---|
| Reserve Disclosures | ReserveDisclosureService | Beep.OilandGas.ProductionAccounting/Services/ReserveDisclosureService.cs | Implemented | Service present. |
| Exploration & Evaluation | ExplorationEvaluationService | Beep.OilandGas.ProductionAccounting/Services/ExplorationEvaluationService.cs | Implemented | Service present. |
| Borrowing Costs | BorrowingCostCapitalizationService | Beep.OilandGas.ProductionAccounting/Services/BorrowingCostCapitalizationService.cs | Implemented | Service present. |
| Asset Swaps / Farm-in/out | AssetSwapService | Beep.OilandGas.ProductionAccounting/Services/AssetSwapService.cs | Implemented | Service present. |
| PSAs | ProductionSharingService | Beep.OilandGas.ProductionAccounting/Services/ProductionSharingService.cs | Implemented | Entitlement calc present. |
| Decommissioning / ARO | DecommissioningService | Beep.OilandGas.ProductionAccounting/Services/DecommissioningService.cs | Partial | Null-return paths. |
| Impairment Testing | ImpairmentTestingService | Beep.OilandGas.ProductionAccounting/Services/ImpairmentTestingService.cs | Implemented | Service present. |
| Functional Currency & FX | FunctionalCurrencyService | Beep.OilandGas.ProductionAccounting/Services/FunctionalCurrencyService.cs | Implemented | Service present. |
| Leasing | LeasingService | Beep.OilandGas.ProductionAccounting/Services/LeasingService.cs | Partial | Null-return paths. |
| Financial Instruments | FinancialInstrumentsService | Beep.OilandGas.ProductionAccounting/Services/FinancialInstrumentsService.cs | Implemented | Service present. |
| Emissions Trading | EmissionsTradingService | Beep.OilandGas.ProductionAccounting/Services/EmissionsTradingService.cs | Implemented | Service present. |

## Best Practices Coverage

| Feature | Planned Service/Module | Evidence | Status | Notes |
|---|---|---|---|---|
| Measurement & Run Tickets | MeasurementService | Beep.OilandGas.ProductionAccounting/Services/MeasurementService.cs | Implemented | Date filter TODOs in TODO.md. |
| Allocation | AllocationEngine, AllocationService | Beep.OilandGas.ProductionAccounting/Services/AllocationEngine.cs | Implemented | Validation logic present. |
| Royalty Management | RoyaltyService | Beep.OilandGas.ProductionAccounting/Services/RoyaltyService.cs | Implemented | Some null-return paths. |
| JIB / COPAS Overhead | JointInterestBillingService, CopasOverheadService | Beep.OilandGas.ProductionAccounting/Services/JointInterestBillingService.cs | Partial | CopasOverheadService has null-return paths. |
| Take-or-Pay | TakeOrPayService | Beep.OilandGas.ProductionAccounting/Services/TakeOrPayService.cs | Partial | Multiple null-return paths. |
| Production Taxes | ProductionTaxService | Beep.OilandGas.ProductionAccounting/Services/ProductionTaxService.cs | Partial | Multiple null-return paths. |
| Inventory Valuation & LCM | InventoryService, InventoryLcmService | Beep.OilandGas.ProductionAccounting/Services/InventoryLcmService.cs | Partial | Null-return paths. |
| Unproved Property | UnprovedPropertyService | Beep.OilandGas.ProductionAccounting/Services/UnprovedPropertyService.cs | Implemented | Service present. |
| Drilling Scenario | DrillingScenarioAccountingService | Beep.OilandGas.ProductionAccounting/Services/DrillingScenarioAccountingService.cs | Implemented | Service present. |
| Internal Controls | InternalControlService | Beep.OilandGas.ProductionAccounting/Services/InternalControlService.cs | Implemented | Service present. |
| Reporting | ReportingService | Beep.OilandGas.ProductionAccounting/Services/ReportingService.cs | Implemented | Service present. |
| Monitoring & Exception Mgmt | - | - | Missing | No dedicated service found. |
| Partner/JV Cash Calls | - | - | Missing | No dedicated service found. |
