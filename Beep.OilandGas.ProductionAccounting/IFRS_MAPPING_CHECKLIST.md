# IFRS Mapping Checklist

Status legend: Implemented, Partial, Not scoped, TBD

## Core IFRS Standards (High-Level)

| Standard | Topic | Current Coverage | Evidence/Notes |
|---|---|---|---|
| IAS 1 | Presentation of Financial Statements | Partial | Financial statements exist via `FinancialStatementService`; full IAS 1 disclosure set not mapped. |
| IAS 2 | Inventories | Partial | Inventory valuation via `InventoryService`; LCM/NRV adjustments are partial in `InventoryLcmService`. |
| IAS 7 | Statement of Cash Flows | Partial | Cash flow reporting not explicitly implemented in Accounting docs. |
| IAS 8 | Accounting Policies/Estimates/Errors | TBD | Policies/changes framework not explicitly mapped. |
| IAS 10 | Events After Reporting Period | TBD | No explicit post-period event handling. |
| IAS 12 | Income Taxes | TBD | Tax calculation service exists, but IAS 12 compliance not mapped. |
| IAS 16 | Property, Plant and Equipment | Partial | Depreciation service exists; full IAS 16 asset lifecycle not mapped. |
| IAS 21 | The Effects of Changes in FX Rates | Implemented | `FunctionalCurrencyService` in ProductionAccounting. |
| IAS 23 | Borrowing Costs | Implemented | `BorrowingCostCapitalizationService`. |
| IAS 24 | Related Party Disclosures | TBD | No explicit related-party disclosure mapping. |
| IAS 36 | Impairment of Assets | Implemented | `ImpairmentTestingService`. |
| IAS 37 | Provisions/Contingent Liabilities | Partial | `DecommissioningService` present; implementation incomplete. |
| IAS 38 | Intangible Assets | TBD | No explicit service. |
| IAS 40 | Investment Property | Not scoped | Not typical for upstream production accounting. |
| IAS 41 | Agriculture | Not scoped | Not applicable to oil and gas. |
| IFRS 1 | First-time Adoption | TBD | No explicit adoption workflow. |
| IFRS 6 | Exploration and Evaluation | Implemented | `ExplorationEvaluationService`. |
| IFRS 9 | Financial Instruments | Implemented | `FinancialInstrumentsService`. |
| IFRS 15 | Revenue from Contracts with Customers | Partial | Revenue recognition in ProductionAccounting; full IFRS 15 disclosures not mapped. |
| IFRS 16 | Leases | Partial | `LeasingService` present; implementation incomplete. |
| IFRS 17 | Insurance Contracts | Not scoped | Not applicable to oil and gas. |

## Oil and Gas Specific Extensions

| Area | Standard/Guidance | Current Coverage | Evidence/Notes |
|---|---|---|---|
| Reserves disclosures | IFRS best practice | Implemented | `ReserveDisclosureService`. |
| Depletion/Amortization | IAS 16/IFRS 6 | Implemented | `AmortizationService` (unit of production). |
| Asset swaps/farm-in/farm-out | IFRS guidance | Implemented | `AssetSwapService`. |
| Production sharing agreements | IFRS guidance | Implemented | `ProductionSharingService`. |
| Emissions trading | IFRS guidance | Implemented | `EmissionsTradingService`. |

## Checklist Actions

- [ ] Confirm IFRS 15 recognition and disclosure requirements against `RevenueService`.
- [ ] Define IAS 1 reporting package and map disclosures to `FinancialStatementService`.
- [ ] Complete IAS 2 LCM/NRV logic in `InventoryLcmService`.
- [ ] Define IAS 16 asset lifecycle and link to depreciation/impairment services.
- [ ] Finalize IFRS 16 leasing flows and disclosures in `LeasingService`.
- [ ] Complete IAS 37 ARO/decommissioning calculations and disclosure support.
- [ ] Add governance for IAS 8 policy changes and error corrections.

