# Beep.OilandGas.ProductionAccounting

**Comprehensive Oil and Gas Production Accounting System**

A unified, enterprise-grade production accounting system that integrates Financial Accounting (FASB compliant), Traditional Accounting (GL, AP, AR, Inventory), and Operational Accounting (Production, Allocation, Royalty, Pricing) following industry best practices and PPDM standards.

## Overview

This library provides complete production accounting functionality covering:

### Financial Accounting (FASB Compliant)
- ✅ **Successful Efforts Accounting** (FASB Statement No. 19)
- ✅ **Full Cost Accounting** (Alternative method)
- ✅ **Amortization Calculations** (Units-of-production method)
- ✅ **Interest Capitalization**
- ✅ **Ceiling Test Calculations**
- ✅ **Impairment Tracking**

### Traditional Accounting
- ✅ **General Ledger** (Chart of Accounts, Journal Entries)
- ✅ **Invoice Management** (Customer Invoices, Payments)
- ✅ **Purchase Order Management** (PO Creation, Receipts)
- ✅ **Accounts Payable** (Vendor Invoices, Payments, Credit Memos)
- ✅ **Accounts Receivable** (Customer Invoices, Payments, Credit Memos)
- ✅ **Inventory Management** (Items, Transactions, Adjustments, Valuation)

### Operational Accounting
- ✅ **Production Management** (Run Tickets, Tank Inventories)
- ✅ **Production Allocation** (Advanced allocation methods)
- ✅ **Revenue Accounting** (Revenue Transactions, Allocations)
- ✅ **Cost Accounting** (Cost Transactions, Allocations, AFE Management)
- ✅ **Royalty Management** (Calculations, Payments, Statements)
- ✅ **Pricing Management** (Price Indices, Valuation)
- ✅ **Trading & Exchanges** (Exchange Trading, Reconciliation)
- ✅ **Storage Facilities** (Tank Batteries, LACT Units)
- ✅ **Ownership Management** (Working Interest, Division Orders)
- ✅ **Unitization** (Unit Operations)

### Reporting & Analytics
- ✅ **Financial Reports** (Income Statement, Balance Sheet)
- ✅ **Operational Reports** (Production, Revenue, Costs)
- ✅ **Royalty Statements**
- ✅ **Tax Reporting**
- ✅ **Production Analytics**

## Architecture

### Unified Entry Points

**Financial Accounting** (Static Methods):
```csharp
using Beep.OilandGas.ProductionAccounting;

// Successful Efforts
var seAccounting = AccountingManager.CreateSuccessfulEffortsAccounting();
seAccounting.RecordAcquisition(unprovedProperty);
seAccounting.RecordExplorationCosts(explorationCosts);

// Full Cost
var fcAccounting = AccountingManager.CreateFullCostAccounting();
fcAccounting.RecordExplorationCosts(propertyId, costCenterId, explorationCosts);

// Amortization
var amortization = AccountingManager.CalculateAmortization(
    netCapitalizedCosts, totalProvedReservesBOE, productionBOE);
```

**Traditional Accounting** (Instance-Based):
```csharp
var traditionalAccounting = new TraditionalAccountingManager();

// General Ledger
var glAccount = traditionalAccounting.GeneralLedger.CreateAccount(request, userId);
var journalEntry = traditionalAccounting.GeneralLedger.CreateJournalEntry(request, userId);

// Invoice, PO, AP, AR, Inventory
var invoice = traditionalAccounting.Invoice.CreateInvoice(request, userId);
var po = traditionalAccounting.PurchaseOrder.CreatePurchaseOrder(request, userId);
var apInvoice = traditionalAccounting.AccountsPayable.CreateInvoice(request, userId);
var arInvoice = traditionalAccounting.AccountsReceivable.CreateInvoice(request, userId);
var inventoryItem = traditionalAccounting.Inventory.CreateItem(request, userId);
```

**Operational Accounting** (Instance-Based):
```csharp
// Production
var productionManager = new ProductionManager();
var runTicket = productionManager.CreateRunTicket(leaseId, wellId, tankBatteryId, 
    measurement, dispositionType, purchaser);

// Allocation
var allocationEngine = new AllocationEngine();
var allocations = allocationEngine.AllocateProduction(runTicket, ownershipInterests);

// Royalty
var royaltyManager = new RoyaltyManager();
var royaltyPayment = royaltyManager.CalculateAndCreatePayment(
    salesTransaction, royaltyOwnerId, royaltyInterest, paymentDate);

// Pricing
var pricingManager = new PricingManager();
var price = pricingManager.GetPrice(priceIndex, date, differential);
```

## Key Features

### 1. PPDM Integration
- Leverages existing PPDM tables (`BUSINESS_ASSOCIATE`, `CONTRACT`, `FINANCE`, `OBLIGATION`)
- Creates new tables only when necessary (GL, Invoice, Revenue, Cost, etc.)
- Follows PPDM naming conventions and audit column standards
- See `PPDM_INTEGRATION_GUIDE.md` for details

### 2. FASB Compliance
- **Successful Efforts Method**: Per FASB Statement No. 19
  - Acquisition costs capitalized
  - G&G costs expensed, exploratory drilling capitalized if successful
  - Development costs capitalized
  - Production costs expensed
  - Units-of-production amortization

- **Full Cost Method**: Alternative method
  - All exploration and development costs capitalized
  - Units-of-production amortization by cost center
  - Annual ceiling test required

### 3. Complete Integration Workflows

**Production-to-Revenue Flow**:
```
Measurement → RunTicket → Allocation → Pricing → Revenue Transaction → 
Revenue Allocation → Royalty Payment → GL Entry
```

**Cost-to-Capitalization Flow**:
```
Cost Transaction → Cost Allocation → AFE → Financial Accounting Decision → 
GL Entry (Capitalized or Expensed) → Amortization (Periodic)
```

### 4. Industry Best Practices
- ✅ Joint Interest Billing (JIB) automation
- ✅ Revenue recognition at point of sale
- ✅ Accurate production allocation
- ✅ Royalty calculation and payment
- ✅ Cost center management
- ✅ AFE tracking and approval
- ✅ Internal controls and audit trails
- ✅ Regulatory compliance (SEC, FASB)

## Project Structure

```
Beep.OilandGas.ProductionAccounting/
├── Financial/                    # Financial Accounting (FASB)
│   ├── SuccessfulEfforts/
│   ├── FullCost/
│   └── Amortization/
├── GeneralLedger/                # Chart of Accounts
├── Invoice/                      # Customer Invoices
├── PurchaseOrder/                # Purchase Orders
├── AccountsPayable/               # Vendor Invoices
├── AccountsReceivable/            # Customer Invoices
├── Inventory/                     # Inventory Management
├── Production/                    # Production Data
├── Allocation/                    # Production Allocation
├── Royalty/                       # Royalty Management
├── Pricing/                       # Price Management
├── Trading/                       # Exchange Trading
├── Storage/                       # Storage Facilities
├── Ownership/                     # Working Interest
├── Unitization/                   # Unit Operations
├── Reporting/                     # Reports
├── PPDMIntegration/               # PPDM Mappings
├── Models/                        # Data Models
├── Calculations/                  # Calculation Engines
├── Validation/                    # Data Validation
├── Constants/                     # Constants
├── Exceptions/                    # Exception Classes
└── AccountingManager.cs           # Unified Entry Point
```

## Data Models

### Entity Classes (Beep.OilandGas.Models/Data/)
- **Financial**: `AMORTIZATION_RECORD`, `IMPAIRMENT_RECORD`, `CEILING_TEST_CALCULATION`
- **Traditional**: `GL_ACCOUNT`, `INVOICE`, `PURCHASE_ORDER`, `AP_INVOICE`, `AR_INVOICE`, `INVENTORY_ITEM`
- **Revenue**: `REVENUE_TRANSACTION`, `SALES_CONTRACT`, `PRICE_INDEX`, `REVENUE_ALLOCATION`
- **Cost**: `COST_TRANSACTION`, `COST_ALLOCATION`, `AFE`, `AFE_LINE_ITEM`, `COST_CENTER`
- **Royalty**: `ROYALTY_INTEREST`, `ROYALTY_OWNER`, `ROYALTY_PAYMENT`, `ROYALTY_PAYMENT_DETAIL`
- **Tax**: `TAX_TRANSACTION`, `TAX_RETURN`
- **Joint Venture**: `JOINT_OPERATING_AGREEMENT`, `JOA_INTEREST`, `JOIB_LINE_ITEM`, `JOIB_ALLOCATION`

### DTOs (Beep.OilandGas.Models/DTOs/Accounting/)
- Request/Response DTOs for all major operations
- Follows standard DTO patterns for API integration

## Usage Examples

### Complete Production-to-Revenue Workflow

```csharp
using Beep.OilandGas.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.ProductionAccounting.Allocation;
using Beep.OilandGas.ProductionAccounting.Pricing;
using Beep.OilandGas.ProductionAccounting.Royalty;
using Beep.OilandGas.Models.Data;

// 1. Create Run Ticket
var productionManager = new ProductionManager();
var runTicket = productionManager.CreateRunTicket(
    leaseId: "LEASE-001",
    wellId: "WELL-001",
    tankBatteryId: "TANK-001",
    measurement: measurementRecord,
    dispositionType: DispositionType.Sale,
    purchaser: "PURCHASER-001"
);

// 2. Allocate Production
var allocationEngine = new AllocationEngine();
var ownershipInterests = ownershipManager.GetOwnershipInterests(leaseId);
var allocations = allocationEngine.AllocateProduction(runTicket, ownershipInterests);

// 3. Get Price
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
var traditionalAccounting = new TraditionalAccountingManager();
var journalEntry = traditionalAccounting.GeneralLedger.CreateJournalEntry(
    new JournalEntryRequest
    {
        EntryDate = revenueTransaction.TRANSACTION_DATE,
        Description = "Production Revenue",
        Lines = new List<JournalEntryLineRequest>
        {
            new JournalEntryLineRequest
            {
                GLAccountId = "REVENUE-ACCOUNT",
                DebitAmount = null,
                CreditAmount = revenueTransaction.GROSS_REVENUE
            },
            new JournalEntryLineRequest
            {
                GLAccountId = "AR-ACCOUNT",
                DebitAmount = revenueTransaction.GROSS_REVENUE,
                CreditAmount = null
            }
        }
    },
    userId
);
```

### Financial Accounting Example

```csharp
// Successful Efforts Accounting
var seAccounting = AccountingManager.CreateSuccessfulEffortsAccounting();

// Record Property Acquisition
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
    GeologicalCosts = 50000m,
    GeophysicalCosts = 75000m,
    DrillingCosts = 2000000m,
    FoundProvedReserves = true
};
seAccounting.RecordExplorationCosts(explorationCosts);

// Calculate Amortization
var provedReserves = new ProvedReserves
{
    ProvedDevelopedOilReserves = 1000000m, // barrels
    ProvedDevelopedGasReserves = 5000000m  // MCF
};
var totalReservesBOE = AccountingManager.ConvertReservesToBOE(provedReserves);
var productionBOE = 10000m; // barrels
var netCapitalizedCosts = 3000000m; // acquisition + exploration

var amortization = AccountingManager.CalculateAmortization(
    netCapitalizedCosts,
    totalReservesBOE,
    productionBOE);
```

## Dependencies

- `Beep.OilandGas.Models` - Entity classes and DTOs
- `Beep.OilandGas.PPDM39.DataManagement` - PPDM data management
- `Beep.OilandGas.PPDM39` - PPDM core functionality
- `TheTechIdea.Beep.Editor` - Data management engine

## Documentation

- **`ARCHITECTURE_AND_INTEGRATION.md`** - Complete architecture and integration guide
- **`PPDM_INTEGRATION_GUIDE.md`** - PPDM table usage and integration
- **`DATABASE_SCRIPTS_GENERATION_GUIDE.md`** - Database script generation patterns
- **`ACCOUNTING_PROJECT_REMOVAL_FINAL.md`** - Migration from Accounting project
- **`COMPLETE_IMPLEMENTATION_SUMMARY.md`** - Implementation status and summary

## Compliance & Standards

### FASB Standards
- ✅ FASB Statement No. 19 - Successful Efforts Method
- ✅ FASB Statement No. 25 - Full Cost Method
- ✅ FASB Statement No. 69 - Disclosures

### SEC Requirements
- ✅ SEC Rule 4-10 - Proved Reserves Definition
- ✅ SEC Regulation S-X - Financial Statements
- ✅ SEC Regulation S-K - Disclosures

### Industry Standards
- ✅ PPDM Data Model - Standard data model
- ✅ API Standards - Measurement standards
- ✅ AGA Standards - Gas measurement

## Status

**✅ Production Ready**

All core modules implemented and integrated:
- Financial Accounting ✅
- Traditional Accounting ✅
- Operational Accounting ✅
- PPDM Integration ✅
- Database Scripts ✅
- Documentation ✅

## License

[Your License Here]

---

**Status: Production Ready** 🚀
