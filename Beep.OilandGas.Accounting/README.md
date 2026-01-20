# Beep.OilandGas.Accounting

**Traditional Accounting Foundation (GL, AP, AR, Inventory, Reporting, Period Close)**

This project provides the core accounting foundation used by Production Accounting and other modules. It follows standard accounting controls: double-entry posting, balanced journal entries, audit trails, period close, and reconciliation support.

## Scope

### General Ledger
- Chart of Accounts (GL_ACCOUNT)
- Journal entries and posting (JOURNAL_ENTRY, JOURNAL_ENTRY_LINE, GL_ENTRY)
- Account balances and trial balance

### Accounts Payable
- Vendor bills and payments (AP_INVOICE, AP_PAYMENT)
- GL posting for expense recognition and settlements

### Accounts Receivable
- Customer invoices and payments (AR_INVOICE, AR_PAYMENT)
- GL posting for revenue recognition and collections

### Inventory
- Items and transactions (INVENTORY_ITEM, INVENTORY_TRANSACTION)
- Valuation and adjustments (weighted average, usage)

### Reporting & Close
- Trial balance generation
- Cash flow statement (IAS 7)
- Period close routines
- Close checklist and readiness checks
- Adjustments and corrections with reversals (IAS 8)

## Invoice Lifecycle Consolidation

- `InvoiceService` is the canonical invoice flow using `INVOICE` tables.
- `ARInvoiceService` has been removed; use `InvoiceService` for all billing flows.
- Status values are standardized via `InvoiceStatuses`.

## Architecture

### Primary Services
- `GLAccountService` for account master data and balances
- `JournalEntryService` for entry creation, validation, and posting
- `TrialBalanceService` for balance validation and reporting
- `PeriodClosingService` for period close workflows
- `APInvoiceService`, `APPaymentService` for AP lifecycle
- `ARService` for AR lifecycle
- `InventoryService` for inventory movements and valuation
- `InventoryLcmService` for NRV/LCM write-downs
- `PurchaseOrderService` for PO lifecycle and receipts
- `InvoiceService` for general invoicing workflows
- `AccountMappingService` for configurable GL account mappings
- `ReconciliationService` for subledger vs GL control checks
- `CashFlowService` for IAS 7 cash flow statements
- `AccountingPolicyService` for IAS 8 policy changes (effective dating)
- `ErrorCorrectionService` for IAS 8 corrections (reversal + repost)
- `FixedAssetLifecycleService` for IAS 16 asset lifecycle (capitalize, depreciate, dispose)
- `ProvisionService` for IAS 37 provisions and ARO tracking
- `PerformanceObligationService` for IFRS 15 obligations and revenue recognition
- `LeaseAccountingService` for IFRS 16 lease accounting (ROU asset and lease liability)
- `PresentationService` for IAS 1 presentation package and disclosure checklist
- `EventsAfterReportingService` for IAS 10 subsequent events tracking and adjusting entries
- `TaxProvisionService` for IAS 12 current/deferred tax provisioning
- `CurrencyTranslationService` for IAS 21 FX translation and gain/loss postings
- `ImpairmentService` for IAS 36 impairment tracking and allowance postings
- `IntangibleAssetService` for IAS 38 capitalization and amortization
- `RelatedPartyDisclosureService` for IAS 24 related party summaries
- `EmployeeBenefitsService` for IAS 19 benefit accruals and settlements
- `GovernmentGrantService` for IAS 20 grant recognition and income release
- `BorrowingCostService` for IAS 23 interest expense/capitalization
- `RetirementBenefitPlanService` for IAS 26 retirement plan contributions and benefits
- `SeparateFinancialStatementService` for IAS 27 separate statement packaging
- `EquityMethodInvestmentService` for IAS 28 associate/joint venture equity method
- `HyperinflationRestatementService` for IAS 29 restatement adjustments
- `FinancialInstrumentService` for IAS 32/39 financial instruments
- `EarningsPerShareService` for IAS 33 EPS calculations and disclosures
- `InterimReportingService` for IAS 34 interim reporting packages
- `InvestmentPropertyService` for IAS 40 investment property recognition
- `AgricultureService` for IAS 41 biological assets
- `InsuranceContractsService` for IFRS 17 insurance contracts and CSM tracking
- `ExpectedCreditLossService` for IFRS 9 expected credit loss provisioning
- `FairValueMeasurementService` for IFRS 13 fair value hierarchy and measurement
- `FirstTimeAdoptionService` for IFRS 1 transition adjustments
- `ShareBasedPaymentService` for IFRS 2 share-based payments
- `BusinessCombinationService` for IFRS 3 business combinations and goodwill
- `AssetsHeldForSaleService` for IFRS 5 held-for-sale classification
- `ExplorationEvaluationService` for IFRS 6 exploration/evaluation costs
- `OperatingSegmentService` for IFRS 8 segment disclosures
- `ConsolidationService` for IFRS 10 eliminations
- `JointArrangementsService` for IFRS 11 joint arrangements
- `InvestmentDisclosureService` for IFRS 12 entity disclosures
- `RegulatoryDeferralService` for IFRS 14 deferral balances
- `FinancialInstrumentDisclosureService` for IFRS 7 risk and sensitivity disclosures
- `PresentationEnhancementService` for IFRS 18 statement presentation guidance
- `SubsidiaryDisclosureService` for IFRS 19 reduced disclosures
- `CeclService` for ASC 326 current expected credit losses
- `GaapRevenueRecognitionService` for ASC 606 contract assets/liabilities
- `GaapLeaseAccountingService` for ASC 842 lease accounting

### Default Accounts
Defaults are centralized in:
- `Beep.OilandGas.Accounting/Constants/DefaultGlAccounts.cs`

Use these defaults in service logic rather than hard-coded account numbers.

## Core Accounting Controls

- **Double-entry**: debits must equal credits.
- **Posting**: only posted entries affect GL balances.
- **Auditability**: all changes tracked with standard PPDM audit fields.
- **Period close**: revenue and expense accounts closed to retained earnings.

## Typical Workflows

### Journal Entry
1. Create entry with line items.
2. Validate balance.
3. Post entry to create GL entries.

### AP Bill
1. Create bill (DRAFT).
2. Receive bill (RECEIVED) and post expense/AP.
3. Pay bill and post AP/cash.

### Invoice
1. Create invoice (DRAFT).
2. Issue invoice (ISSUED) and post AR/revenue.
3. Record payment and post cash/AR.

### Inventory
1. Receive stock and post inventory/AP.
2. Use stock and post COGS/inventory.
3. Reconcile on-hand counts and adjust as needed.

### Period Close
1. Validate trial balance.
2. Post closing entry (revenue/expense to retained earnings).
3. Generate post-close trial balance.

### Adjustments and Corrections
1. Record policy changes with effective dates and reason codes.
2. Reverse original entry and post corrected entry.
3. Preserve audit trail with remarks and references.

### Fixed Assets (IAS 16)
1. Capitalize acquisitions to fixed asset accounts.
2. Post periodic depreciation to expense and accumulated depreciation.
3. Track componentization for significant parts.
4. Dispose assets with gain/loss recognition and audit trail.

### Provisions and ARO (IAS 37)
1. Record ARO with discounted present value.
2. Update estimates with reason codes and audit trail.
3. Accrete liability over time with periodic expense posting.

### Performance Obligations (IFRS 15)
1. Record performance obligations tied to sales contracts.
2. Allocate transaction price across obligations.
3. Recognize revenue on satisfaction (AR or contract asset).
4. Track contract liabilities for prepayments and release to revenue.

### Leases (IFRS 16)
1. Create lease contracts with discount rates and terms.
2. Record payment schedules and measure initial ROU asset/liability.
3. Post lease payments with interest/principal split.
4. Post periodic ROU amortization.

### Presentation (IAS 1)
1. Generate statement package (P&L, balance sheet, cash flow).
2. Attach disclosure checklist for consistent reporting.

### Events After Reporting Period (IAS 10)
1. Record adjusting and non-adjusting events with dates and descriptions.
2. Post adjusting entries where required and retain disclosure trail.

### Income Taxes (IAS 12)
1. Record current tax provisions and liabilities.
2. Track deferred tax asset/liability balances and movements.
3. Generate tax return summaries for periods.

### Foreign Exchange (IAS 21)
1. Translate balances using FX rates.
2. Record FX gain/loss revaluations.
3. Store translation results for disclosures.

### Impairment (IAS 36)
1. Record impairment triggers and amounts.
2. Post impairment loss and allowance entries.

### Intangibles (IAS 38)
1. Capitalize intangible costs with audit trail.
2. Record amortization and accumulated amortization.

### Related Parties (IAS 24)
1. Summarize related-party AR/AP/contract activity.
2. Produce disclosure-ready summaries by BA ID.

### Employee Benefits (IAS 19)
1. Accrue benefits to expense and liability.
2. Record benefit payments and settle liabilities.

### Government Grants (IAS 20)
1. Record grant awards as receivable/cash with deferred income.
2. Recognize grant income over the related periods.

### Borrowing Costs (IAS 23)
1. Record interest expense or capitalize to qualifying assets.
2. Track borrowing costs for disclosures and audit.

### Retirement Benefit Plans (IAS 26)
1. Record plan contributions and benefit payments.
2. Track plan assets and obligations with audit history.

### Separate Financial Statements (IAS 27)
1. Generate entity-level statement packages.
2. Track separate reporting events with audit trail.

### Equity Method Investments (IAS 28)
1. Record associate/joint venture investments.
2. Post equity method earnings and dividends.

### Hyperinflation (IAS 29)
1. Restate non-monetary balances using price indices.
2. Record monetary gains/losses to reserves.

### Financial Instruments (IAS 32/39)
1. Record instrument recognition and classifications.
2. Post fair value gains/losses and liability changes.

### Earnings Per Share (IAS 33)
1. Calculate basic/diluted EPS.
2. Store EPS disclosures with period metadata.

### Interim Reporting (IAS 34)
1. Generate interim statement packages.
2. Record interim reporting events.

### Investment Property (IAS 40)
1. Record acquisitions as investment property.
2. Track fair value changes through P&L.

### Agriculture (IAS 41)
1. Record biological assets.
2. Track fair value changes and disclosures.

### Insurance Contracts (IFRS 17)
1. Record contract inception and premium receipts.
2. Recognize insurance service revenue and claim expense.
3. Track CSM adjustments and insurance finance expense.

### Expected Credit Loss (IFRS 9)
1. Record ECL adjustments and allowance movements.
2. Maintain audit trail for staging and model changes.

### Fair Value Measurement (IFRS 13)
1. Record fair value gains/losses with Level 1/2/3 tags.
2. Store valuation methodology notes for disclosures.

### First-time Adoption (IFRS 1)
1. Record transition adjustments to equity reserves.
2. Keep reconciliation notes for opening balances.

### Share-based Payments (IFRS 2)
1. Record equity-settled compensation expense.
2. Track awards and valuation assumptions.

### Business Combinations (IFRS 3)
1. Record consideration and net assets at fair value.
2. Recognize goodwill or bargain purchase gain.

### Held for Sale (IFRS 5)
1. Reclassify assets to held-for-sale.
2. Record impairment to fair value less costs to sell.

### Exploration and Evaluation (IFRS 6)
1. Capitalize or expense exploration costs.
2. Maintain audit trail by project.

### Operating Segments (IFRS 8)
1. Record segment metrics for disclosures.
2. Store segment level assets, revenue, and margins.

### Consolidation (IFRS 10)
1. Record intercompany eliminations.
2. Track consolidation adjustments by period.

### Joint Arrangements (IFRS 11)
1. Record joint operation cost sharing.
2. Maintain arrangement notes and audit trail.

### Interests in Other Entities (IFRS 12)
1. Store disclosure notes for subsidiaries/associates/JVs.
2. Tag disclosure type for reporting outputs.

### Regulatory Deferral (IFRS 14)
1. Record regulatory deferral assets or liabilities.
2. Amortize deferrals to income or expense.

### Financial Instrument Disclosures (IFRS 7)
1. Record risk, sensitivity, and liquidity disclosures.
2. Tag disclosures by instrument group and category.

### Presentation Enhancements (IFRS 18)
1. Generate presentation package with operating/investing/financing categories.
2. Record management performance measure disclosures.

### Subsidiary Disclosures (IFRS 19)
1. Record reduced disclosure notes for subsidiaries.
2. Tag topics for audit and reporting.

### CECL (ASC 326)
1. Record allowance changes and CECL expense.
2. Maintain audit trail of CECL adjustments.

### Revenue (ASC 606)
1. Record contract billing to contract assets/liabilities.
2. Recognize revenue from contract assets or liabilities.

### Leases (ASC 842)
1. Record ROU asset and lease liability at commencement.
2. Record periodic lease payments.

## Integration Notes

- Production Accounting uses this project for GL, AP/AR, inventory, and reporting services.
- PPDM data model conventions and audit columns are enforced across entities.

## Dual-Basis Posting (IFRS/GAAP)

Use `AccountingBasisPostingService` with `AccountMappingService` (IFRS) and `GaapAccountMappingService` (GAAP)
to post to one book or both in a single call. Book IDs are stored in `SOURCE` and filtered in reports.

```csharp
var ifrsMap = new AccountMappingService();
var gaapMap = new GaapAccountMappingService();
var poster = new AccountingBasisPostingService(journalEntryService, ifrsMap, gaapMap);

await poster.PostBalancedEntryAsync(
    AccountMappingKeys.Cash,
    AccountMappingKeys.Revenue,
    1000m,
    "Sale",
    userId,
    AccountingBasis.Both);
```

## Status

Operational with standard workflows implemented.
