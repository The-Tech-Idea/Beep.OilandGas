# ProductionAccounting Architecture and Integration Guide

## Executive Summary

This document provides a comprehensive overview of the `Beep.OilandGas.ProductionAccounting` system architecture, module integration patterns, and adherence to oil and gas accounting best practices per FASB, SEC, and industry standards.

## System Architecture

### Core Architecture Principles

1. **Separation of Concerns**: Each module handles a specific domain (Financial, Operational, Traditional Accounting)
2. **PPDM Integration**: Leverage existing PPDM tables where possible, create new tables only when necessary
3. **Unified Entry Point**: `AccountingManager` provides static methods for Financial Accounting
4. **Instance-Based Traditional Accounting**: `TraditionalAccountingManager` provides instance access to GL, Invoice, PO, AP, AR, Inventory
5. **Data Persistence**: All entities follow PPDM pattern with standard audit columns

### Module Structure

```
Beep.OilandGas.ProductionAccounting/
├── Financial/                    # Financial Accounting (FASB Compliant)
│   ├── SuccessfulEfforts/        # FASB Statement No. 19
│   ├── FullCost/                 # Alternative method
│   └── Amortization/             # Units-of-production, Interest capitalization
├── GeneralLedger/                # Chart of Accounts, Journal Entries
├── Invoice/                      # Customer Invoices
├── PurchaseOrder/                # Purchase Orders
├── AccountsPayable/              # Vendor Invoices, Payments
├── AccountsReceivable/           # Customer Invoices, Payments
├── Inventory/                    # Inventory Management
├── Production/                   # Production Data, Run Tickets
├── Allocation/                   # Production Allocation Engine
├── Royalty/                      # Royalty Calculations, Payments
├── Pricing/                      # Price Index Management, Valuation
├── Trading/                      # Exchange Trading, Reconciliation
├── Storage/                      # Storage Facilities, Tank Batteries
├── Ownership/                    # Working Interest, Division Orders
├── Unitization/                  # Unit Operations
├── Reporting/                    # Financial and Operational Reports
├── PPDMIntegration/              # PPDM Table Mappings
└── AccountingManager.cs          # Unified Entry Point
```

## Module Integration Patterns

### 1. Financial Accounting Integration

**Entry Point**: `AccountingManager` (Static Methods)

```csharp
// Successful Efforts Accounting
var seAccounting = AccountingManager.CreateSuccessfulEffortsAccounting();
seAccounting.RecordAcquisition(unprovedProperty);
seAccounting.RecordExplorationCosts(explorationCosts);
seAccounting.RecordDevelopmentCosts(developmentCosts);

// Full Cost Accounting
var fcAccounting = AccountingManager.CreateFullCostAccounting();
fcAccounting.RecordExplorationCosts(propertyId, costCenterId, explorationCosts);
fcAccounting.RecordDevelopmentCosts(propertyId, costCenterId, developmentCosts);

// Amortization
var amortization = AccountingManager.CalculateAmortization(
    netCapitalizedCosts,
    totalProvedReservesBOE,
    productionBOE);
```

**Integration Points**:
- Uses `PropertyModels` (UnprovedProperty, ProvedProperty, ProvedReserves)
- Uses `CostModels` (ExplorationCosts, DevelopmentCosts, ProductionCosts)
- Persists to PPDM tables: `ACCOUNTING_COST`, `ACCOUNTING_AMORTIZATION`, `ACCOUNTING_METHOD`
- Creates new tables: `AMORTIZATION_RECORD`, `IMPAIRMENT_RECORD`, `CEILING_TEST_CALCULATION`

### 2. Traditional Accounting Integration

**Entry Point**: `TraditionalAccountingManager` (Instance-Based)

```csharp
var traditionalAccounting = new TraditionalAccountingManager();

// General Ledger
var glAccount = traditionalAccounting.GeneralLedger.CreateAccount(createRequest, userId);
var journalEntry = traditionalAccounting.GeneralLedger.CreateJournalEntry(entryRequest, userId);

// Invoice Management
var invoice = traditionalAccounting.Invoice.CreateInvoice(createRequest, userId);

// Purchase Orders
var po = traditionalAccounting.PurchaseOrder.CreatePurchaseOrder(createRequest, userId);

// Accounts Payable
var apInvoice = traditionalAccounting.AccountsPayable.CreateInvoice(createRequest, userId);

// Accounts Receivable
var arInvoice = traditionalAccounting.AccountsReceivable.CreateInvoice(createRequest, userId);

// Inventory
var inventoryItem = traditionalAccounting.Inventory.CreateItem(createRequest, userId);
```

**Integration Points**:
- Uses `BUSINESS_ASSOCIATE` for vendors/customers
- Uses `OBLIGATION` and `OBLIG_PAYMENT` for payment tracking
- Creates new tables: `GL_ACCOUNT`, `INVOICE`, `PURCHASE_ORDER`, `AP_INVOICE`, `AR_INVOICE`, `INVENTORY_ITEM`

### 3. Production Accounting Integration

**Entry Point**: `ProductionManager`

```csharp
var productionManager = new ProductionManager();

// Create Run Ticket from Measurement
var runTicket = productionManager.CreateRunTicket(
    leaseId, wellId, tankBatteryId,
    measurement, dispositionType, purchaser);

// Create Tank Inventory
var inventory = productionManager.CreateTankInventory(
    tankBatteryId, inventoryDate,
    openingInventory, receipts, deliveries);
```

**Integration Points**:
- Uses `Measurement` module for measurement records
- Uses `Storage` module for tank batteries
- Creates `RunTicket` records
- Links to `REVENUE_TRANSACTION` for sales

### 4. Revenue Accounting Integration

**Flow**: Production → Allocation → Pricing → Revenue Transaction → GL Entry

```csharp
// 1. Production recorded
var runTicket = productionManager.CreateRunTicket(...);

// 2. Allocation to working interests
var allocationEngine = new AllocationEngine();
var allocations = allocationEngine.AllocateProduction(
    runTicket, ownershipInterests);

// 3. Pricing applied
var pricingManager = new PricingManager();
var price = pricingManager.GetPrice(priceIndex, date, differential);

// 4. Revenue transaction created
var revenueTransaction = new REVENUE_TRANSACTION
{
    PROPERTY_ID = propertyId,
    TRANSACTION_DATE = runTicket.TicketDateTime,
    OIL_VOLUME = allocatedVolume,
    OIL_PRICE = price,
    GROSS_REVENUE = allocatedVolume * price
};

// 5. GL Entry created
var glEntry = traditionalAccounting.GeneralLedger.CreateJournalEntry(
    new JournalEntryRequest
    {
        Description = "Production Revenue",
        DebitAccount = "Revenue",
        CreditAccount = "Accounts Receivable",
        Amount = revenueTransaction.GROSS_REVENUE
    }, userId);
```

**Integration Points**:
- `ProductionManager` → `AllocationEngine` → `PricingManager` → `REVENUE_TRANSACTION` → `GL_ENTRY`
- Uses `OwnershipManager` for interest calculations
- Uses `RoyaltyManager` for royalty deductions

### 5. Cost Accounting Integration

**Flow**: Cost Transaction → Cost Allocation → AFE → GL Entry

```csharp
// 1. Cost transaction recorded
var costTransaction = new COST_TRANSACTION
{
    PROPERTY_ID = propertyId,
    WELL_ID = wellId,
    COST_TYPE = "DRILLING",
    AMOUNT = drillingCost,
    IS_CAPITALIZED = true
};

// 2. Cost allocated to cost centers
var costAllocation = new COST_ALLOCATION
{
    COST_TRANSACTION_ID = costTransaction.COST_TRANSACTION_ID,
    COST_CENTER_ID = costCenterId,
    ALLOCATED_AMOUNT = allocatedAmount,
    ALLOCATION_PERCENTAGE = allocationPercentage
};

// 3. Linked to AFE
var afe = new AFE
{
    PROPERTY_ID = propertyId,
    AFE_NUMBER = "AFE-001",
    BUDGET_AMOUNT = budgetAmount,
    ACTUAL_AMOUNT = actualAmount
};

// 4. GL Entry for capitalized costs
var glEntry = traditionalAccounting.GeneralLedger.CreateJournalEntry(
    new JournalEntryRequest
    {
        Description = "Drilling Costs - Capitalized",
        DebitAccount = "Oil and Gas Properties",
        CreditAccount = "Accounts Payable",
        Amount = costTransaction.AMOUNT
    }, userId);
```

**Integration Points**:
- `COST_TRANSACTION` → `COST_ALLOCATION` → `AFE` → `GL_ENTRY`
- Links to Financial Accounting for capitalization decisions
- Uses `COST_CENTER` for organizational tracking

### 6. Royalty Accounting Integration

**Flow**: Revenue Transaction → Royalty Calculation → Royalty Payment → GL Entry

```csharp
// 1. Revenue transaction exists
var revenueTransaction = ...;

// 2. Royalty calculated
var royaltyManager = new RoyaltyManager();
var royaltyPayment = royaltyManager.CalculateAndCreatePayment(
    salesTransaction,
    royaltyOwnerId,
    royaltyInterest,
    paymentDate);

// 3. Royalty payment recorded
var royaltyPaymentDetail = new ROYALTY_PAYMENT_DETAIL
{
    ROYALTY_PAYMENT_ID = royaltyPayment.PaymentId,
    REVENUE_TRANSACTION_ID = revenueTransaction.REVENUE_TRANSACTION_ID,
    ROYALTY_AMOUNT = royaltyPayment.RoyaltyAmount
};

// 4. GL Entry for royalty expense
var glEntry = traditionalAccounting.GeneralLedger.CreateJournalEntry(
    new JournalEntryRequest
    {
        Description = "Royalty Payment",
        DebitAccount = "Royalty Expense",
        CreditAccount = "Cash",
        Amount = royaltyPayment.RoyaltyAmount
    }, userId);
```

**Integration Points**:
- `REVENUE_TRANSACTION` → `RoyaltyManager` → `ROYALTY_PAYMENT` → `GL_ENTRY`
- Uses `ROYALTY_INTEREST` for interest definitions
- Uses `BUSINESS_ASSOCIATE` for royalty owners

## Best Practices for Oil and Gas Accounting

### 1. FASB Compliance

#### Successful Efforts Method (FASB Statement No. 19)
- **Acquisition Costs**: Capitalize as unproved property
- **Exploration Costs**: 
  - G&G costs: Expense as incurred
  - Exploratory drilling: Capitalize if finds proved reserves, expense if dry hole
- **Development Costs**: Capitalize all development costs
- **Production Costs**: Expense as incurred (lifting costs)
- **Amortization**: Units-of-production method based on proved reserves

#### Full Cost Method (Alternative)
- **All Costs**: Capitalize all exploration and development costs
- **Amortization**: Units-of-production method based on total reserves in cost center
- **Ceiling Test**: Required annually to ensure capitalized costs don't exceed discounted future net cash flows

### 2. SEC Reporting Requirements

- **Proved Reserves**: Must be reported annually (SEC Rule 4-10)
- **Ceiling Test**: Required for Full Cost method
- **Disclosure**: Material changes in reserves, costs, and production
- **MD&A**: Management Discussion and Analysis of financial condition

### 3. Joint Interest Billing (JIB) Best Practices

- **Automated Allocation**: Use `AllocationEngine` for cost allocation
- **Timely Billing**: Bill within 60-90 days of cost incurrence
- **Audit Trail**: Maintain complete audit trail of all allocations
- **Approval Workflow**: Require approval for non-operated costs
- **Reconciliation**: Regular reconciliation with operators

### 4. Revenue Recognition

- **Point of Sale**: Recognize revenue when title transfers (typically at wellhead or delivery point)
- **Price Determination**: Use index-based pricing with differentials
- **Volume Measurement**: Use certified measurement (run tickets, LACT units)
- **Allocation**: Allocate production to working interests accurately
- **Royalty Deduction**: Calculate and deduct royalties before revenue distribution

### 5. Cost Management

- **AFE Management**: Track all costs against Authorization for Expenditure
- **Cost Centers**: Organize costs by property, well, or field
- **Capitalization Rules**: Follow FASB rules for capitalization vs. expensing
- **Cost Allocation**: Allocate shared costs (facilities, overhead) equitably
- **Budget vs. Actual**: Regular comparison and variance analysis

### 6. Internal Controls

- **Segregation of Duties**: Separate production, accounting, and approval functions
- **Authorization**: Require approval for capital expenditures (AFEs)
- **Reconciliation**: Regular reconciliation of:
  - Production volumes (run tickets vs. sales)
  - Revenue (sales vs. cash receipts)
  - Costs (invoices vs. payments)
- **Audit Trail**: Maintain complete audit trail of all transactions
- **Documentation**: Retain all supporting documentation (run tickets, invoices, contracts)

### 7. Data Integrity

- **PPDM Compliance**: Use PPDM standard tables and naming conventions
- **Standard Audit Columns**: All tables include ROW_CREATED_BY, ROW_CREATED_DATE, etc.
- **Referential Integrity**: Maintain foreign key relationships
- **Data Validation**: Validate all inputs (volumes, prices, costs)
- **Version Control**: Track changes to critical data (ownership, contracts)

## Integration Workflows

### Complete Production-to-Revenue Workflow

```
1. Production Measurement
   └─> MeasurementManager.CreateMeasurement()
       └─> ProductionManager.CreateRunTicket()

2. Production Allocation
   └─> AllocationEngine.AllocateProduction()
       └─> Uses OwnershipManager for interest calculations

3. Pricing
   └─> PricingManager.GetPrice()
       └─> Uses PriceIndexManager for index prices

4. Revenue Transaction
   └─> Create REVENUE_TRANSACTION
       └─> Links to run ticket, allocation, price

5. Revenue Allocation
   └─> Create REVENUE_ALLOCATION
       └─> Allocates revenue to working interests

6. Royalty Calculation
   └─> RoyaltyManager.CalculateAndCreatePayment()
       └─> Creates ROYALTY_PAYMENT

7. GL Entry
   └─> TraditionalAccountingManager.GeneralLedger.CreateJournalEntry()
       └─> Debit: Revenue, Credit: Accounts Receivable
       └─> Debit: Royalty Expense, Credit: Cash
```

### Complete Cost-to-Capitalization Workflow

```
1. Cost Incurred
   └─> Create COST_TRANSACTION
       └─> Links to property, well, AFE

2. Cost Allocation
   └─> Create COST_ALLOCATION
       └─> Allocates to cost centers

3. Financial Accounting Decision
   └─> SuccessfulEffortsAccounting.RecordExplorationCosts()
       └─> OR FullCostAccounting.RecordExplorationCosts()
       └─> Determines capitalization vs. expensing

4. GL Entry
   └─> TraditionalAccountingManager.GeneralLedger.CreateJournalEntry()
       └─> If capitalized: Debit Oil and Gas Properties
       └─> If expensed: Debit Exploration Expense
       └─> Credit: Accounts Payable

5. Amortization (Periodic)
   └─> AccountingManager.CalculateAmortization()
       └─> Creates AMORTIZATION_RECORD
       └─> GL Entry: Debit Amortization Expense, Credit Accumulated Amortization
```

## Module Dependencies

```
AccountingManager (Static)
├── Financial.SuccessfulEfforts
├── Financial.FullCost
└── Financial.Amortization

TraditionalAccountingManager (Instance)
├── GeneralLedger
├── Invoice
├── PurchaseOrder
├── AccountsPayable
├── AccountsReceivable
└── Inventory

ProductionManager
├── Measurement
├── Storage
└── AllocationEngine

RoyaltyManager
├── OwnershipManager
├── PricingManager
└── ProductionManager

AllocationEngine
├── OwnershipManager
└── ProductionManager

PricingManager
└── PriceIndexManager
```

## Data Flow Architecture

### Production Data Flow
```
Measurement → RunTicket → Allocation → Revenue Transaction → GL Entry
```

### Cost Data Flow
```
Cost Transaction → Cost Allocation → AFE → Financial Accounting → GL Entry
```

### Revenue Data Flow
```
Revenue Transaction → Revenue Allocation → Royalty Payment → GL Entry
```

### Financial Accounting Data Flow
```
Property Acquisition → Exploration Costs → Development Costs → Amortization → GL Entry
```

## PPDM Integration Strategy

### Use Existing PPDM Tables
- `BUSINESS_ASSOCIATE` - All parties (vendors, customers, royalty owners)
- `CONTRACT` - All contracts (sales, purchase, service)
- `FINANCE` - Financial transactions
- `OBLIGATION` - Payment obligations
- `OBLIG_PAYMENT` - Payment records
- `LAND_RIGHT` - Property rights
- `EQUIPMENT` - Equipment inventory
- `PDEN` - Production entities
- `PDEN_VOL_SUMMARY` - Production volumes

### Create New Tables (Not in PPDM)
- General Ledger tables (GL_ACCOUNT, GL_ENTRY, JOURNAL_ENTRY)
- Invoice tables (INVOICE, INVOICE_LINE_ITEM)
- Purchase Order tables (PURCHASE_ORDER, PO_LINE_ITEM)
- AP/AR tables (AP_INVOICE, AR_INVOICE, AP_PAYMENT, AR_PAYMENT)
- Inventory tables (INVENTORY_ITEM, INVENTORY_TRANSACTION)
- Revenue tables (REVENUE_TRANSACTION, REVENUE_ALLOCATION)
- Cost tables (COST_TRANSACTION, COST_ALLOCATION, AFE)
- Financial Accounting tables (AMORTIZATION_RECORD, IMPAIRMENT_RECORD)
- Royalty tables (ROYALTY_INTEREST, ROYALTY_PAYMENT)

## Compliance and Standards

### FASB Standards
- ✅ FASB Statement No. 19 - Successful Efforts Method
- ✅ FASB Statement No. 25 - Full Cost Method (Alternative)
- ✅ FASB Statement No. 69 - Disclosures about Oil and Gas Producing Activities

### SEC Requirements
- ✅ SEC Rule 4-10 - Definition of Proved Reserves
- ✅ SEC Regulation S-X - Financial Statement Requirements
- ✅ SEC Regulation S-K - Disclosure Requirements

### Industry Standards
- ✅ PPDM Data Model - Standard data model for oil and gas
- ✅ API Standards - Measurement and allocation standards
- ✅ AGA Standards - Gas measurement standards

## Recommendations

### 1. Enhance Integration Points
- Add event-driven architecture for real-time updates
- Implement workflow engine for approval processes
- Add notification system for critical transactions

### 2. Improve Data Validation
- Add comprehensive validation rules
- Implement business rule engine
- Add data quality checks

### 3. Enhance Reporting
- Add standard financial reports (Income Statement, Balance Sheet)
- Add operational reports (Production, Revenue, Costs)
- Add compliance reports (SEC, FASB)

### 4. Add Audit Capabilities
- Implement audit log for all transactions
- Add change tracking for critical data
- Implement approval workflows

### 5. Improve Performance
- Add caching for frequently accessed data
- Optimize database queries
- Implement batch processing for large operations

## Conclusion

The `Beep.OilandGas.ProductionAccounting` system provides a comprehensive, integrated solution for oil and gas accounting that adheres to FASB, SEC, and industry standards. The modular architecture allows for flexible integration while maintaining separation of concerns and data integrity.

