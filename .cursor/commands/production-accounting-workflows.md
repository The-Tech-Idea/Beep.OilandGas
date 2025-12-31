# Production Accounting Workflows

## Overview

This document outlines workflows and patterns for the Beep.OilandGas.ProductionAccounting system, covering Financial Accounting (FASB compliant), Traditional Accounting, and Operational Accounting.

## Architecture

### Unified Entry Points

**Financial Accounting** (Static Methods via AccountingManager):
- Successful Efforts Accounting (FASB Statement No. 19)
- Full Cost Accounting (Alternative method)
- Amortization Calculations

**Traditional Accounting** (Instance-Based via TraditionalAccountingManager):
- General Ledger (Chart of Accounts, Journal Entries)
- Invoice Management
- Purchase Order Management
- Accounts Payable
- Accounts Receivable
- Inventory Management

**Operational Accounting** (Instance-Based):
- Production Management
- Allocation Engine
- Royalty Management
- Pricing Management
- Trading & Exchanges

## Financial Accounting Workflows

### Successful Efforts Method

```csharp
using Beep.OilandGas.ProductionAccounting;

// Create Successful Efforts Accounting instance
var seAccounting = AccountingManager.CreateSuccessfulEffortsAccounting();

// Record Property Acquisition (Capitalized)
var unprovedProperty = new UnprovedProperty
{
    PropertyId = "PROP-001",
    PropertyName = "Smith Ranch",
    AcquisitionCost = 1000000m,
    AcquisitionDate = new DateTime(2023, 1, 1)
};
seAccounting.RecordAcquisition(unprovedProperty);

// Record Exploration Costs
var explorationCosts = new ExplorationCosts
{
    PropertyId = "PROP-001",
    WellId = "WELL-001",
    GeologicalCosts = 50000m,      // Expensed
    GeophysicalCosts = 75000m,      // Expensed
    DrillingCosts = 2000000m,       // Capitalized if successful
    FoundProvedReserves = true      // Determines capitalization
};
seAccounting.RecordExplorationCosts(explorationCosts);

// Record Development Costs (All Capitalized)
var developmentCosts = new DevelopmentCosts
{
    PropertyId = "PROP-001",
    DevelopmentWellDrillingCosts = 5000000m,  // Capitalized
    DevelopmentWellEquipment = 2000000m,      // Capitalized
    SupportEquipmentAndFacilities = 3000000m  // Capitalized
};
seAccounting.RecordDevelopmentCosts(developmentCosts);

// Calculate Amortization (Units-of-Production Method)
var amortization = AccountingManager.CalculateAmortization(
    netCapitalizedCosts: 10000000m,
    totalProvedReservesBOE: 1000000m,  // barrels
    productionBOE: 10000m               // barrels for period
);
// Result: 100,000 (10,000 * 10,000,000 / 1,000,000)
```

### Full Cost Method

```csharp
// Create Full Cost Accounting instance
var fcAccounting = AccountingManager.CreateFullCostAccounting();

// Record Exploration Costs (All Capitalized)
fcAccounting.RecordExplorationCosts(
    propertyId: "PROP-001",
    costCenterId: "COST-CENTER-001",
    costs: explorationCosts
);

// Record Development Costs (All Capitalized)
fcAccounting.RecordDevelopmentCosts(
    propertyId: "PROP-001",
    costCenterId: "COST-CENTER-001",
    costs: developmentCosts
);

// Ceiling Test (Required Annually)
var ceilingTest = new CEILING_TEST_CALCULATION
{
    PROPERTY_ID = "PROP-001",
    NET_CAPITALIZED_COST = 10000000m,
    DISCOUNTED_FUTURE_NET_CASH_FLOWS = 12000000m,
    DISCOUNT_RATE = 0.10m
};
// If NET_CAPITALIZED_COST > DISCOUNTED_FUTURE_NET_CASH_FLOWS, record impairment
```

## Traditional Accounting Workflows

### General Ledger

```csharp
var traditionalAccounting = new TraditionalAccountingManager();

// Create GL Account
var glAccount = traditionalAccounting.GeneralLedger.CreateAccount(
    new CreateAccountRequest
    {
        AccountNumber = "4000",
        AccountName = "Oil Revenue",
        AccountType = "Revenue",
        ParentAccountId = null
    }, 
    userId);

// Create Journal Entry
var journalEntry = traditionalAccounting.GeneralLedger.CreateJournalEntry(
    new JournalEntryRequest
    {
        EntryDate = DateTime.UtcNow,
        Description = "Production Revenue",
        Lines = new List<JournalEntryLineRequest>
        {
            new JournalEntryLineRequest
            {
                GLAccountId = "4000", // Revenue
                DebitAmount = null,
                CreditAmount = 100000m
            },
            new JournalEntryLineRequest
            {
                GLAccountId = "1200", // Accounts Receivable
                DebitAmount = 100000m,
                CreditAmount = null
            }
        }
    }, 
    userId);
```

### Accounts Payable

```csharp
// Create AP Invoice
var apInvoice = traditionalAccounting.AccountsPayable.CreateInvoice(
    new CreateAPInvoiceRequest
    {
        VendorId = "VENDOR-001",
        InvoiceNumber = "INV-001",
        InvoiceDate = DateTime.UtcNow,
        DueDate = DateTime.UtcNow.AddDays(30),
        LineItems = new List<APInvoiceLineItemRequest>
        {
            new APInvoiceLineItemRequest
            {
                Description = "Drilling Services",
                Amount = 50000m,
                GLAccountId = "6000" // Operating Expense
            }
        }
    }, 
    userId);

// Create Payment
var payment = traditionalAccounting.AccountsPayable.CreatePayment(
    new CreateAPPaymentRequest
    {
        InvoiceId = apInvoice.InvoiceId,
        PaymentDate = DateTime.UtcNow,
        PaymentAmount = 50000m,
        PaymentMethod = "Check"
    }, 
    userId);
```

### Accounts Receivable

```csharp
// Create AR Invoice
var arInvoice = traditionalAccounting.AccountsReceivable.CreateInvoice(
    new CreateARInvoiceRequest
    {
        CustomerId = "CUSTOMER-001",
        InvoiceNumber = "INV-001",
        InvoiceDate = DateTime.UtcNow,
        DueDate = DateTime.UtcNow.AddDays(30),
        LineItems = new List<ARInvoiceLineItemRequest>
        {
            new ARInvoiceLineItemRequest
            {
                Description = "Oil Sales",
                Amount = 100000m,
                GLAccountId = "4000" // Revenue
            }
        }
    }, 
    userId);
```

## Operational Accounting Workflows

### Production-to-Revenue Workflow

```csharp
// 1. Create Run Ticket from Measurement
var productionManager = new ProductionManager();
var runTicket = productionManager.CreateRunTicket(
    leaseId: "LEASE-001",
    wellId: "WELL-001",
    tankBatteryId: "TANK-001",
    measurement: measurementRecord,
    dispositionType: DispositionType.Sale,
    purchaser: "PURCHASER-001"
);

// 2. Allocate Production to Working Interests
var allocationEngine = new AllocationEngine();
var ownershipInterests = ownershipManager.GetOwnershipInterests(leaseId);
var allocations = allocationEngine.AllocateProduction(
    runTicket: runTicket,
    ownershipInterests: ownershipInterests,
    allocationMethod: AllocationMethod.WorkingInterest
);

// 3. Apply Pricing
var pricingManager = new PricingManager();
var price = pricingManager.GetPrice(
    priceIndex: "WTI",
    date: runTicket.TicketDateTime,
    differential: 2.50m
);

// 4. Create Revenue Transaction
var revenueTransaction = new REVENUE_TRANSACTION
{
    REVENUE_TRANSACTION_ID = Guid.NewGuid().ToString(),
    PROPERTY_ID = propertyId,
    WELL_ID = "WELL-001",
    TRANSACTION_DATE = runTicket.TicketDateTime,
    OIL_VOLUME = allocations.Sum(a => a.AllocatedVolume),
    OIL_PRICE = price,
    GROSS_REVENUE = allocations.Sum(a => a.AllocatedVolume) * price,
    ACTIVE_IND = "Y",
    ROW_CREATED_BY = userId,
    ROW_CREATED_DATE = DateTime.UtcNow
};

// 5. Calculate and Pay Royalty
var royaltyManager = new RoyaltyManager();
var royaltyPayment = royaltyManager.CalculateAndCreatePayment(
    salesTransaction: salesTransaction,
    royaltyOwnerId: "ROYALTY-OWNER-001",
    royaltyInterest: 0.125m, // 12.5%
    paymentDate: DateTime.UtcNow
);

// 6. Create GL Entry
var journalEntry = traditionalAccounting.GeneralLedger.CreateJournalEntry(
    new JournalEntryRequest
    {
        EntryDate = revenueTransaction.TRANSACTION_DATE,
        Description = "Production Revenue",
        Lines = new List<JournalEntryLineRequest>
        {
            new JournalEntryLineRequest
            {
                GLAccountId = "4000", // Revenue
                DebitAmount = null,
                CreditAmount = revenueTransaction.GROSS_REVENUE
            },
            new JournalEntryLineRequest
            {
                GLAccountId = "1200", // Accounts Receivable
                DebitAmount = revenueTransaction.GROSS_REVENUE,
                CreditAmount = null
            }
        }
    },
    userId
);
```

### Cost-to-Capitalization Workflow

```csharp
// 1. Create Cost Transaction
var costTransaction = new COST_TRANSACTION
{
    COST_TRANSACTION_ID = Guid.NewGuid().ToString(),
    PROPERTY_ID = propertyId,
    WELL_ID = "WELL-001",
    COST_TYPE = "DRILLING",
    AMOUNT = 2000000m,
    IS_CAPITALIZED = true,
    ACTIVE_IND = "Y",
    ROW_CREATED_BY = userId,
    ROW_CREATED_DATE = DateTime.UtcNow
};

// 2. Allocate Cost to Cost Centers
var costAllocation = new COST_ALLOCATION
{
    COST_ALLOCATION_ID = Guid.NewGuid().ToString(),
    COST_TRANSACTION_ID = costTransaction.COST_TRANSACTION_ID,
    COST_CENTER_ID = "COST-CENTER-001",
    ALLOCATED_AMOUNT = 2000000m,
    ALLOCATION_PERCENTAGE = 1.0m,
    ALLOCATION_METHOD = "VOLUME_BASED"
};

// 3. Link to AFE
var afe = new AFE
{
    AFE_ID = Guid.NewGuid().ToString(),
    PROPERTY_ID = propertyId,
    AFE_NUMBER = "AFE-2024-001",
    BUDGET_AMOUNT = 5000000m,
    ACTUAL_AMOUNT = 2000000m,
    STATUS = "APPROVED",
    START_DATE = new DateTime(2024, 1, 1),
    END_DATE = new DateTime(2024, 12, 31)
};

// 4. Financial Accounting Decision (Successful Efforts)
var seAccounting = AccountingManager.CreateSuccessfulEffortsAccounting();
seAccounting.RecordDevelopmentCosts(developmentCosts); // Capitalizes costs

// 5. Create GL Entry for Capitalized Costs
var journalEntry = traditionalAccounting.GeneralLedger.CreateJournalEntry(
    new JournalEntryRequest
    {
        EntryDate = DateTime.UtcNow,
        Description = "Drilling Costs - Capitalized",
        Lines = new List<JournalEntryLineRequest>
        {
            new JournalEntryLineRequest
            {
                GLAccountId = "1500", // Oil and Gas Properties
                DebitAmount = costTransaction.AMOUNT,
                CreditAmount = null
            },
            new JournalEntryLineRequest
            {
                GLAccountId = "2000", // Accounts Payable
                DebitAmount = null,
                CreditAmount = costTransaction.AMOUNT
            }
        }
    },
    userId
);
```

## Royalty Accounting Workflow

```csharp
// 1. Register Royalty Interest
var royaltyInterest = new ROYALTY_INTEREST
{
    ROYALTY_INTEREST_ID = Guid.NewGuid().ToString(),
    PROPERTY_ID = propertyId,
    ROYALTY_OWNER_BA_ID = "ROYALTY-OWNER-001",
    INTEREST_TYPE = "MINERAL_ROYALTY",
    INTEREST_PERCENTAGE = 0.125m, // 12.5%
    EFFECTIVE_DATE = new DateTime(2023, 1, 1),
    ACTIVE_IND = "Y"
};

// 2. Calculate and Create Payment
var royaltyManager = new RoyaltyManager();
var royaltyPayment = royaltyManager.CalculateAndCreatePayment(
    salesTransaction: salesTransaction,
    royaltyOwnerId: "ROYALTY-OWNER-001",
    royaltyInterest: 0.125m,
    paymentDate: DateTime.UtcNow
);

// 3. Create Royalty Payment Detail
var royaltyPaymentDetail = new ROYALTY_PAYMENT_DETAIL
{
    ROYALTY_PAYMENT_DETAIL_ID = Guid.NewGuid().ToString(),
    ROYALTY_PAYMENT_ID = royaltyPayment.PaymentId,
    REVENUE_TRANSACTION_ID = revenueTransaction.REVENUE_TRANSACTION_ID,
    ROYALTY_AMOUNT = royaltyPayment.RoyaltyAmount
};

// 4. Create GL Entry for Royalty Expense
var journalEntry = traditionalAccounting.GeneralLedger.CreateJournalEntry(
    new JournalEntryRequest
    {
        EntryDate = DateTime.UtcNow,
        Description = "Royalty Payment",
        Lines = new List<JournalEntryLineRequest>
        {
            new JournalEntryLineRequest
            {
                GLAccountId = "5000", // Royalty Expense
                DebitAmount = royaltyPayment.RoyaltyAmount,
                CreditAmount = null
            },
            new JournalEntryLineRequest
            {
                GLAccountId = "1000", // Cash
                DebitAmount = null,
                CreditAmount = royaltyPayment.RoyaltyAmount
            }
        }
    },
    userId
);
```

## Key Principles

1. **Financial Accounting**: Use static methods via `AccountingManager` for FASB-compliant accounting
2. **Traditional Accounting**: Use instance-based `TraditionalAccountingManager` for GL, AP, AR, Inventory
3. **Operational Accounting**: Use instance-based managers (ProductionManager, AllocationEngine, etc.)
4. **Integration**: All workflows integrate through GL entries and PPDM tables
5. **Audit Trail**: All entities include standard PPDM audit columns (ROW_CREATED_BY, ROW_CREATED_DATE, etc.)
6. **PPDM Integration**: Leverage existing PPDM tables (BUSINESS_ASSOCIATE, CONTRACT, FINANCE, OBLIGATION) where possible

## References

- See `Beep.OilandGas.ProductionAccounting/ARCHITECTURE_AND_INTEGRATION.md` for complete architecture
- See `Beep.OilandGas.ProductionAccounting/BEST_PRACTICES_OIL_GAS_ACCOUNTING.md` for best practices
- See `Beep.OilandGas.ProductionAccounting/README.md` for module overview

