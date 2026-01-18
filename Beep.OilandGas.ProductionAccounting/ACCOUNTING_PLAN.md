# Production Accounting - Financial & Accounting Services Plan

## Overview

Implements FASB ASC 932 (Oil & Gas) accounting standards, including:
- **COPAS Compliance**: Council of Petroleum Accountants Societies standards
- **ASC 606**: Revenue recognition standards
- **Cost Capitalization**: Successful Efforts vs. Full Cost methods
- **Depletion/Depreciation**: Unit of production method
- **Joint Interest Accounting**: Multi-owner cost and revenue allocation

---

## Domain Architecture

### Cost Accounting Methods

#### 1. Successful Efforts Method Service
**File**: `Accounting/SuccessfulEffortsService.cs`

**Principle**: Capitalize costs only for successful wells; immediately expense unsuccessful exploratory wells.

**Entities**:
- WELL_ALLOCATION_DATA (well interest tracking)
- ACCOUNTING_COST (capitalized costs)
- JOURNAL_ENTRY (GL posting)
- ACCOUNTING_METHOD (method definition)

**Interface**:
```csharp
public interface ISuccessfulEffortsService
{
    // Cost Capitalization
    Task<ACCOUNTING_COST> CapitalizeWellCostAsync(
        WELL_ALLOCATION_DATA well,
        decimal drillingCost,
        decimal completionCost,
        decimal equipmentCost,
        string costCenter,
        string userId,
        string? connectionName = null);
    
    Task<JOURNAL_ENTRY> ExpenseUnsuccessfulWellAsync(
        WELL_ALLOCATION_DATA well,
        decimal totalCost,
        string reason,
        string userId,
        string? connectionName = null);
    
    // Cost Center Management
    Task<List<COST_CENTER>> GetCostCentersAsync(string? connectionName = null);
    Task<COST_CENTER> CreateCostCenterAsync(
        COST_CENTER costCenter,
        string userId,
        string? connectionName = null);
    
    // Successful Well Tracking
    Task<bool> MarkWellSuccessfulAsync(
        string wellId,
        DateTime successDate,
        string userId,
        string? connectionName = null);
    
    Task<bool> MarkWellUnsuccessfulAsync(
        string wellId,
        DateTime decisionDate,
        string userId,
        string? connectionName = null);
    
    // Cost Reports
    Task<decimal> GetTotalCapitalizedCostAsync(
        string costCenter,
        DateTime? asOfDate = null,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Track well status changes (exploratory → successful/unsuccessful)
- Maintain cost center definitions (field, country, development program)
- Create dry hole expense journal entries immediately upon determination
- Capitalize development well costs
- Match cost to well life for depletion

---

#### 2. Full Cost Method Service
**File**: `Accounting/FullCostService.cs`

**Principle**: Capitalize all exploration/development costs within cost center; amortize over production.

**Entities**:
- COST_CENTER (cost pool definition)
- ACCOUNTING_COST (capitalized pool)
- AMORTIZATION_RECORD (depletion tracking)

**Interface**:
```csharp
public interface IFullCostService
{
    // Cost Pool Management
    Task<COST_CENTER> CreateCostPoolAsync(
        string countryCode,
        decimal capitalizedCosts,
        string userId,
        string? connectionName = null);
    
    Task<(decimal capitalized, decimal amortized)> GetCostPoolStatusAsync(
        string costCenter,
        string? connectionName = null);
    
    // Cost Capitalization
    Task<ACCOUNTING_COST> AddCostToCostPoolAsync(
        string costCenter,
        decimal amount,
        string costType,  // "Exploration", "Development", "Acquisition"
        string userId,
        string? connectionName = null);
    
    // Ceiling Test (SEC Requirement)
    Task<(bool passes, decimal bookValue, decimal fairValue)> PerformCeilingTestAsync(
        string costCenter,
        DateTime testDate,
        string? connectionName = null);
    
    Task<IMPAIRMENT_RECORD> RecordCeilingTestImpairmentAsync(
        string costCenter,
        decimal impairmentAmount,
        string userId,
        string? connectionName = null);
    
    // Amortization/Depletion
    Task<AMORTIZATION_RECORD> CalculateDepletionAsync(
        string costCenter,
        PROVED_RESERVES reserves,
        decimal productionVolume,
        string userId,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Track costs by cost center (country for full cost)
- Implement ceiling test (SEC requirement) quarterly
- Record impairments if ceiling exceeded
- Calculate depletion on unit of production method
- Support cost pool rollforward reporting

---

### 3. Amortization & Depletion Service
**File**: `Accounting/AmortizationService.cs`

**Principle**: Allocate capitalized costs over reserve life using unit of production method.

**Entities**:
- PROVED_RESERVES (reserve quantities)
- ACCOUNTING_COST (capitalized base)
- AMORTIZATION_RECORD (period charges)
- DEPLETION_CALCULATION (calc details)

**Interface**:
```csharp
public interface IAmortizationService
{
    // Depletion Calculation (Unit of Production)
    Task<AMORTIZATION_RECORD> CalculateMonthlyDepletionAsync(
        string costCenter,
        PROVED_RESERVES reserves,
        RUN_TICKET production,
        string userId,
        string? connectionName = null);
    
    Task<decimal> GetDepletionRateAsync(
        string costCenter,
        string? connectionName = null);
    
    // Depreciation for Equipment
    Task<AMORTIZATION_RECORD> CalculateDepreciationAsync(
        string wellId,
        decimal equipmentCost,
        int estimatedLife,
        string depreciationMethod, // "Straight-line", "Unit of production"
        string userId,
        string? connectionName = null);
    
    // Ceiling Test Support
    Task<decimal> CalculateNetBookValueAsync(
        string costCenter,
        DateTime asOfDate,
        string? connectionName = null);
    
    // Reports
    Task<AMORTIZATION_RECORD> GetCumulativeDepletionAsync(
        string costCenter,
        DateTime fromDate,
        DateTime toDate,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Unit of production: Depletion = (Production × Capitalized Cost) / Total Reserves
- Adjust for reserve estimate changes
- Track equipment depreciation separately
- Monthly/quarterly depletion calculation
- Annual depreciation for financial reporting

---

### 4. Journal Entry Service
**File**: `Accounting/JournalEntryService.cs`

**Principle**: Post all transactions to general ledger; ensure double-entry accounting.

**Entities**:
- JOURNAL_ENTRY (GL entries)
- GL_ACCOUNT (chart of accounts)
- GL_ENTRY (line items)
- JOURNAL_ENTRY_LINE (entry details)

**Interface**:
```csharp
public interface IJournalEntryService
{
    // Journal Entry Creation
    Task<JOURNAL_ENTRY> CreateJournalEntryAsync(
        DateTime entryDate,
        string description,
        string userId,
        string? connectionName = null);
    
    Task<JOURNAL_ENTRY_LINE> AddLineItemAsync(
        string journalEntryId,
        string accountCode,
        decimal debitAmount,
        decimal creditAmount,
        string description,
        string userId,
        string? connectionName = null);
    
    Task<bool> PostJournalEntryAsync(
        string journalEntryId,
        string userId,
        string? connectionName = null);
    
    // GL Account Management
    Task<List<GL_ACCOUNT>> GetChartOfAccountsAsync(string? connectionName = null);
    Task<GL_ACCOUNT> GetAccountAsync(string accountCode, string? connectionName = null);
    
    // GL Balance Queries
    Task<decimal> GetAccountBalanceAsync(
        string accountCode,
        DateTime asOfDate,
        string? connectionName = null);
    
    Task<(decimal debits, decimal credits)> GetAccountActivityAsync(
        string accountCode,
        DateTime fromDate,
        DateTime toDate,
        string? connectionName = null);
    
    // Validation
    Task<(bool isBalanced, decimal variance)> ValidateJournalEntryAsync(
        string journalEntryId,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Enforce debit = credit for all entries
- Track entry source (Sales, Allocation, Royalty, etc.)
- Support multiple currencies with conversion
- Maintain audit trail (who, when, why)
- Batch entry support for month-end close

---

### 5. Revenue Recognition Service
**File**: `Accounting/RevenueService.cs`

**Principle**: Recognize revenue per ASC 606; match to performance obligations.

**Entities**:
- SALES_TRANSACTION (sales events)
- INVOICE (billing documents)
- REVENUE_TRANSACTION (recognized revenue)
- JOURNAL_ENTRY (GL posting)

**Interface**:
```csharp
public interface IRevenueService
{
    // Revenue Recognition (ASC 606)
    Task<REVENUE_TRANSACTION> RecognizeRevenueAsync(
        SALES_TRANSACTION transaction,
        string performanceObligation,
        string userId,
        string? connectionName = null);
    
    // Performance Obligation Tracking
    Task<(decimal totalAmount, decimal recognized, decimal deferred)> GetPerformanceObligationAsync(
        string customerId,
        string? contractId = null,
        string? connectionName = null);
    
    // Revenue Adjustments
    Task<REVENUE_TRANSACTION> AdjustRevenueAsync(
        string salesTransactionId,
        decimal adjustmentAmount,
        string reason,
        string userId,
        string? connectionName = null);
    
    // Invoicing
    Task<INVOICE> CreateInvoiceAsync(
        List<SALES_TRANSACTION> transactions,
        string customerId,
        DateTime invoiceDate,
        string userId,
        string? connectionName = null);
    
    Task<bool> ReconcileInvoiceAsync(
        string invoiceId,
        decimal paidAmount,
        DateTime paymentDate,
        string userId,
        string? connectionName = null);
    
    // Revenue Reports
    Task<decimal> GetTotalRevenueAsync(
        DateTime fromDate,
        DateTime toDate,
        string? wellId = null,
        string? connectionName = null);
    
    Task<List<REVENUE_TRANSACTION>> GetRevenueHistoryAsync(
        DateTime fromDate,
        DateTime toDate,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Recognize revenue when control of product transfers
- For gas: Calculate MMBtu and recognize per contract terms
- For oil: Recognize at delivery point
- Handle sales returns and price adjustments
- Track deferred revenue for advance payments
- Match with cash receipts

---

### 6. Period Closing Service
**File**: `Accounting/PeriodClosingService.cs`

**Principle**: Month/quarter/year-end close procedures; generate financial statements.

**Entities**:
- JOURNAL_ENTRY (adjusting entries)
- GL_ACCOUNT (final balances)
- OPERATIONAL_REPORT (production summary)
- COST_REPORT_SUMMARY (cost summary)

**Interface**:
```csharp
public interface IPeriodClosingService
{
    // Month-End Close
    Task<List<string>> GetMonthEndChecklistAsync(
        DateTime monthEnd,
        string? connectionName = null);
    
    Task<(bool passes, List<string> issues)> ValidateMonthEndAsync(
        DateTime monthEnd,
        string? connectionName = null);
    
    Task<OPERATIONAL_REPORT> GenerateMonthEndReportAsync(
        DateTime monthEnd,
        string? connectionName = null);
    
    // Accruals & Reversals
    Task<JOURNAL_ENTRY> CreateAccrualAsync(
        string description,
        decimal amount,
        string costCenter,
        DateTime periodEnd,
        string userId,
        string? connectionName = null);
    
    Task<JOURNAL_ENTRY> ReverseAccrualAsync(
        string accrualEntryId,
        DateTime reverseDate,
        string userId,
        string? connectionName = null);
    
    // Trial Balance
    Task<List<(string accountCode, string accountName, decimal debit, decimal credit)>> 
        GetTrialBalanceAsync(
            DateTime asOfDate,
            string? connectionName = null);
    
    // Reconciliations
    Task<bool> ReconcileProductionToRevenueAsync(
        DateTime monthEnd,
        string? connectionName = null);
    
    Task<bool> ReconcileAllocationToRoyaltyAsync(
        DateTime monthEnd,
        string? connectionName = null);
}
```

**Implementation Notes**:
- Daily: Post transactions, validate balances
- Weekly: GL reconciliation, cash position
- Month-end: Accruals, cutoff, depletion, close
- Quarter-end: Ceiling test (full cost), impairment testing
- Year-end: Audit adjustments, financial statement prep

---

## Cost Center Definition

**Cost Center Structure** (per COPAS standards):

```
Cost Center
├── Geography (Country, State)
├── Capitalized Costs Pool
│   ├── Acquisition Costs
│   ├── Exploration Costs
│   └── Development Costs
├── Reserves (Proved 1P, 2P, 3P)
├── Depletion Rate ($/BBL or $/MCF)
└── Ceiling Test Status
```

**Cost Center Tracking**:
- Successful Efforts: Field-level cost centers
- Full Cost: Country-level cost centers
- Track both for hybrid methods

---

## Joint Interest Accounting (COPAS)

**File**: `Accounting/JointInterestService.cs`

**Interface**:
```csharp
public interface IJointInterestService
{
    // Interest Tracking
    Task<OWNERSHIP_INTEREST> RecordOwnershipInterestAsync(
        string wellId,
        string partyId,
        decimal interestPercent,
        string userId,
        string? connectionName = null);
    
    // Cost Allocation
    Task<List<COST_ALLOCATION>> AllocateCostsToOwnersAsync(
        string wellId,
        List<ACCOUNTING_COST> costs,
        string userId,
        string? connectionName = null);
    
    // Revenue Allocation
    Task<List<REVENUE_SHARING>> AllocateRevenueToOwnersAsync(
        string wellId,
        SALES_TRANSACTION sales,
        List<ROYALTY_INTEREST> interests,
        string userId,
        string? connectionName = null);
    
    // JIB Statement (Joint Interest Billing)
    Task<JOINT_INTEREST_STATEMENT> GenerateJIBStatementAsync(
        string wellId,
        DateTime statementDate,
        string? connectionName = null);
    
    // Overhead Allocation (COPAS AG)
    Task<decimal> CalculateOverheadAsync(
        string costCenter,
        DateTime month,
        string? connectionName = null);
}
```

---

## Accounting Methods Selection

**Decision Tree**:
```
Is company large (>$10B annual revenue)?
├─ Yes → Successful Efforts (SE)
└─ No → Full Cost (FC) or Hybrid
    ├─ Exploration-focused? → SE
    └─ Development-focused? → FC
```

---

## Chart of Accounts Structure

```
10000-19999: Assets
  11000-11999: Property, Plant & Equipment (Capitalized Costs)
  12000-12999: Accumulated Depletion/Depreciation
  13000-13999: Inventory

20000-29999: Liabilities
  21000-21999: Accounts Payable
  22000-22999: Accrued Royalties
  23000-23999: Deferred Revenue

40000-49999: Revenue
  41000: Oil & Gas Sales (Gross)
  41100: Sales Adjustments
  
50000-59999: Operating Costs
  51000-51999: Direct Operating Costs
  52000-52999: Exploration Costs (expensed)
  53000-53999: Depreciation/Depletion
  54000-54999: Severance Taxes

60000-69999: Royalties & Interests
  61000: Royalty Payments
  62000: Working Interest Costs
  
70000-79999: Administrative
  71000: General & Administrative
```

---

## Period Close Checklist

**Daily**:
- [ ] Post production tickets
- [ ] Post sales transactions
- [ ] Validate GL balances (debit = credit)

**Weekly**:
- [ ] GL reconciliation
- [ ] Bank reconciliation
- [ ] Cash position review

**Month-End (T+3 days)**:
- [ ] Production data complete and validated
- [ ] Measurement corrections applied
- [ ] Allocation completed
- [ ] Royalty calculations complete
- [ ] Revenue recognized per ASC 606
- [ ] All invoices posted
- [ ] Accruals recorded
- [ ] Depletion calculated
- [ ] Trial balance balanced
- [ ] Reconciliations complete:
  - Production to Revenue
  - Allocation to Royalty
  - Cash to Bank
- [ ] Adjusting entries posted
- [ ] Financial reports generated

**Quarter-End (Full Cost only)**:
- [ ] Ceiling test performed
- [ ] Impairment testing complete
- [ ] Reserve estimates reviewed
- [ ] Depletion rate recalculated

**Year-End**:
- [ ] Audit adjustments
- [ ] 10-K preparation
- [ ] Reserve certification
- [ ] Regulatory filings

---

## Standards & References

- **FASB ASC 932**: Accounting for Oil & Gas
- **FASB ASC 606**: Revenue Recognition
- **COPAS Standards**: Joint interest accounting
- **SEC Guidance**: Reserve estimation, full cost ceiling
- **API Guide**: Industry best practices

---

**Document Version**: 1.0  
**Status**: Ready for Implementation
