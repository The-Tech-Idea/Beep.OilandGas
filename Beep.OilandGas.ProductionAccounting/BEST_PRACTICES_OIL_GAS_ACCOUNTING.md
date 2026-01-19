# Oil and Gas Accounting Best Practices

## Overview

This document outlines best practices for oil and gas accounting following FASB, SEC, and industry standards. These practices are implemented throughout the `Beep.OilandGas.ProductionAccounting` system and align to the accounting foundation in `Beep.OilandGas.Accounting` (GL, JE, period close, and reporting services).

## Table of Contents

1. [Financial Accounting Standards](#financial-accounting-standards)
2. [Reserve Accounting and Depletion Inputs](#reserve-accounting-and-depletion-inputs)
3. [DD&A Allocation and Split](#dd-a-allocation-and-split)
4. [Revenue Recognition](#revenue-recognition)
5. [Cost Management](#cost-management)
6. [Unproved Property and Impairment](#unproved-property-and-impairment)
7. [Drilling Scenario Accounting](#drilling-scenario-accounting)
8. [Inventory Valuation and LCM](#inventory-valuation-and-lcm)
9. [Take-or-Pay Contracts](#take-or-pay-contracts)
10. [Production Taxes](#production-taxes)
11. [Authorization and Approvals](#authorization-and-approvals)
12. [Production Accounting](#production-accounting)
13. [Royalty Management](#royalty-management)
14. [Lease and Economic Interests](#lease-and-economic-interests)
15. [Joint Interest Billing](#joint-interest-billing)
16. [Internal Controls](#internal-controls)
17. [Data Integrity](#data-integrity)
18. [Reporting and Compliance](#reporting-and-compliance)
19. [System Integration](#system-integration)
20. [Accounting Foundation (GL)](#accounting-foundation-gl)
21. [Period Close and Accruals](#period-close-and-accruals)
22. [Adjustments and Corrections](#adjustments-and-corrections)
23. [Partner and Joint Venture Accounting](#partner-and-joint-venture-accounting)
24. [Tax and Regulatory Reporting](#tax-and-regulatory-reporting)
25. [Data Governance and Master Data](#data-governance-and-master-data)
26. [Monitoring and Exception Management](#monitoring-and-exception-management)

## Financial Accounting Standards

### Successful Efforts Method (FASB Statement No. 19)

#### Property Acquisition
- **Capitalize** all acquisition costs as unproved property
- Track by property ID with acquisition date and cost
- Reclassify to proved property when reserves are discovered
- Test for impairment annually on unproved properties

**Implementation**:
```csharp
var seAccounting = AccountingManager.CreateSuccessfulEffortsAccounting();
seAccounting.RecordAcquisition(unprovedProperty);
```

#### Exploration Costs
- **Expense** geological and geophysical (G&G) costs as incurred
- **Capitalize** exploratory drilling costs if well finds proved reserves
- **Expense** exploratory drilling costs if well is dry hole
- Maintain detailed records of exploration activities

**Implementation**:
```csharp
var explorationCosts = new ExplorationCosts
{
    PropertyId = propertyId,
    GeologicalCosts = 50000m,      // Expensed
    GeophysicalCosts = 75000m,      // Expensed
    DrillingCosts = 2000000m,       // Capitalized if successful
    FoundProvedReserves = true      // Determines capitalization
};
seAccounting.RecordExplorationCosts(explorationCosts);
```

#### Development Costs
- **Capitalize** all development costs (drilling, completion, facilities)
- Track by property and well
- Link to AFE (Authorization for Expenditure) for approval tracking

**Implementation**:
```csharp
var developmentCosts = new DevelopmentCosts
{
    PropertyId = propertyId,
    DevelopmentWellDrillingCosts = 5000000m,  // Capitalized
    DevelopmentWellEquipment = 2000000m,      // Capitalized
    SupportEquipmentAndFacilities = 3000000m  // Capitalized
};
seAccounting.RecordDevelopmentCosts(developmentCosts);
```

#### Production Costs
- **Expense** all production costs (lifting costs) as incurred
- Include operating expenses, maintenance, workovers
- Track by property and well for cost allocation

**Implementation**:
```csharp
var productionCosts = new ProductionCosts
{
    PropertyId = propertyId,
    WellId = wellId,
    OperatingExpenses = 50000m,     // Expensed
    MaintenanceCosts = 25000m,      // Expensed
    WorkoverCosts = 100000m         // Expensed
};
seAccounting.RecordProductionCosts(productionCosts);
```

#### Amortization
- Use **units-of-production method** based on proved reserves
- Calculate amortization rate: Net Capitalized Costs / Total Proved Reserves BOE
- Apply rate to production for period
- Update reserves annually

**Implementation**:
```csharp
var amortization = AccountingManager.CalculateAmortization(
    netCapitalizedCosts: 10000000m,
    totalProvedReservesBOE: 1000000m,  // barrels
    productionBOE: 10000m               // barrels for period
);
// Result: 100,000 (10,000 * 10,000,000 / 1,000,000)
```

### Full Cost Method (Alternative)

#### Cost Capitalization
- **Capitalize** all exploration and development costs
- Organize by cost center (country, region, or field)
- No distinction between successful and unsuccessful exploration

**Implementation**:
```csharp
var fcAccounting = AccountingManager.CreateFullCostAccounting();
fcAccounting.RecordExplorationCosts(
    propertyId: propertyId,
    costCenterId: costCenterId,
    costs: explorationCosts
);
```

#### Ceiling Test
- Perform **annually** to ensure capitalized costs don't exceed discounted future net cash flows
- Calculate: Net Capitalized Costs vs. Discounted Future Net Cash Flows
- Record impairment if ceiling exceeded
- Use appropriate discount rate (typically 10%)

**Implementation**:
```csharp
var ceilingTest = new CEILING_TEST_CALCULATION
{
    PROPERTY_ID = propertyId,
    NET_CAPITALIZED_COST = 10000000m,
    DISCOUNTED_FUTURE_NET_CASH_FLOWS = 12000000m,
    DISCOUNT_RATE = 0.10m
};
// If NET_CAPITALIZED_COST > DISCOUNTED_FUTURE_NET_CASH_FLOWS, record impairment
```

## Reserve Accounting and Depletion Inputs

- Maintain **proved reserves** by property/lease with effective dates
- Update reserves at least **annually** and upon material revisions
- Feed reserve data into **unit-of-production** depletion and ceiling tests
- Track pricing assumptions used in reserve reporting

**Service Alignment**:
- `ReserveAccountingService` for reserve storage and validation
- `AmortizationService` for depletion calculation inputs

## DD&A Allocation and Split

- Calculate DD&amp;A using **unit-of-production** and reserve-based rates
- Split DD&amp;A between **oil and gas** based on production or reserve mix
- Separate **working vs non-working** interest depletion for reporting
- Support **fieldwide** DD&amp;A when assets are pooled

**Service Alignment**:
- `AmortizationService` for fieldwide and split calculations

**Best Practice**:
```csharp
var split = await amortizationService.CalculateSplitAsync(
    propertyId,
    periodEnd,
    userId);
```

## Revenue Recognition

### Point of Sale
- Recognize revenue when **title transfers** (typically at wellhead or delivery point)
- Use run tickets or LACT unit measurements for volume
- Apply pricing at time of sale (index-based with differentials)

**Best Practice**:
```csharp
// 1. Create run ticket from measurement
var runTicket = productionManager.CreateRunTicket(...);

// 2. Allocate production to working interests
var allocations = allocationEngine.AllocateProduction(runTicket, ownershipInterests);

// 3. Apply pricing
var price = pricingManager.GetPrice(priceIndex, date, differential);

// 4. Create revenue transaction
var revenueTransaction = new REVENUE_TRANSACTION
{
    TRANSACTION_DATE = runTicket.TicketDateTime,
    OIL_VOLUME = allocatedVolume,
    OIL_PRICE = price,
    GROSS_REVENUE = allocatedVolume * price
};
```

### Price Determination
- Use **index-based pricing** (WTI, Brent, Henry Hub)
- Apply **differentials** for quality, location, transportation
- Document pricing methodology in contracts
- Maintain price index history

**Best Practice**:
```csharp
var priceIndex = new PRICE_INDEX
{
    INDEX_NAME = "WTI",
    COMMODITY_TYPE = "CRUDE_OIL",
    PRICE_DATE = transactionDate,
    PRICE_VALUE = 75.50m,
    CURRENCY_CODE = "USD"
};

var price = basePrice + differential; // e.g., WTI + $2.50/bbl
```

### Volume Measurement
- Use **certified measurement** (run tickets, LACT units, meters)
- Apply **BSW (Basic Sediment and Water)** adjustments
- Convert to **standard conditions** (temperature, pressure)
- Maintain measurement audit trail

**Best Practice**:
```csharp
var measurement = new MeasurementRecord
{
    GrossVolume = 1000m,        // barrels
    BSW = 0.5m,                 // 0.5%
    Temperature = 60m,           // Fahrenheit
    ApiGravity = 35.5m
};

var netVolume = measurement.GrossVolume * (1 - measurement.BSW / 100m);
```

### Revenue Allocation
- Allocate revenue to **working interests** based on ownership
- Deduct **royalties** before allocation
- Apply **production sharing** agreements if applicable
- Maintain allocation audit trail

**Best Practice**:
```csharp
var revenueAllocation = new REVENUE_ALLOCATION
{
    REVENUE_TRANSACTION_ID = revenueTransaction.REVENUE_TRANSACTION_ID,
    INTEREST_OWNER_BA_ID = ownerId,
    INTEREST_PERCENTAGE = 0.25m,  // 25% working interest
    ALLOCATED_AMOUNT = grossRevenue * 0.25m,
    ALLOCATION_METHOD = "WORKING_INTEREST"
};
```

## Cost Management

### AFE (Authorization for Expenditure) Management
- Require **AFE approval** before capital expenditures
- Track **budget vs. actual** costs
- Monitor **variance** and require approval for overruns
- Link all costs to AFE for approval tracking

**Best Practice**:
```csharp
var afe = new AFE
{
    AFE_NUMBER = "AFE-2024-001",
    PROPERTY_ID = propertyId,
    BUDGET_AMOUNT = 5000000m,
    ACTUAL_AMOUNT = 5200000m,
    STATUS = "APPROVED",
    START_DATE = new DateTime(2024, 1, 1),
    END_DATE = new DateTime(2024, 12, 31)
};

// Link cost transaction to AFE
var costTransaction = new COST_TRANSACTION
{
    PROPERTY_ID = propertyId,
    AFE_ID = afe.AFE_ID,
    COST_TYPE = "DRILLING",
    AMOUNT = 2000000m,
    IS_CAPITALIZED = true
};
```

### Cost Centers
- Organize costs by **property**, **well**, **field**, or **region**
- Enable cost allocation and reporting
- Support both Successful Efforts and Full Cost methods

**Best Practice**:
```csharp
var costCenter = new COST_CENTER
{
    COST_CENTER_ID = Guid.NewGuid().ToString(),
    COST_CENTER_NAME = "Permian Basin - Field A",
    DESCRIPTION = "Cost center for Permian Basin Field A operations"
};

var costAllocation = new COST_ALLOCATION
{
    COST_TRANSACTION_ID = costTransaction.COST_TRANSACTION_ID,
    COST_CENTER_ID = costCenter.COST_CENTER_ID,
    ALLOCATED_AMOUNT = 1000000m,
    ALLOCATION_PERCENTAGE = 0.50m,  // 50% allocated to this cost center
    ALLOCATION_METHOD = "VOLUME_BASED"
};
```

### Cost Allocation
- Allocate **shared costs** (facilities, overhead) equitably
- Use **volume-based**, **revenue-based**, or **equity-based** allocation
- Document allocation methodology
- Maintain allocation audit trail

**Best Practice**:
```csharp
// Volume-based allocation
var allocation = allocationEngine.AllocateCosts(
    totalCost: 1000000m,
    allocationBasis: AllocationBasis.Volume,
    volumes: new Dictionary<string, decimal>
    {
        { "WELL-001", 5000m },  // 50% of volume
        { "WELL-002", 3000m },  // 30% of volume
        { "WELL-003", 2000m }   // 20% of volume
    }
);
```

## Unproved Property and Impairment

- Track **unproved acquisitions** separately from proved properties
- Capitalize acquisition costs and **test impairment** at least annually
- Reclassify to proved upon **commercial discovery**
- Record impairments without overwriting original acquisition records
- Group unproved leases into **carrying groups** for pooled impairment testing
- Track **lease expiries**, **options**, and **delay rentals** with effective dates
- Write off expired leasehold costs with clear reason codes

**Service Alignment**:
- `UnprovedPropertyService` for acquisition, impairment, and reclassification

**Best Practice**:
```csharp
var acquisition = await unprovedPropertyService.RecordUnprovedAcquisitionAsync(
    propertyId,
    cost: 250000m,
    acquisitionDate: DateTime.UtcNow,
    userId: userId);

var impairment = await unprovedPropertyService.TestImpairmentAsync(
    propertyId,
    DateTime.UtcNow,
    userId);
```

## Drilling Scenario Accounting

- Expense **dry holes** immediately under Successful Efforts
- Capitalize **successful drilling** and completion costs
- Track **sidetracks** and **plug-back** costs as separate scenarios
- Keep scenario metadata for auditability and reserve reconciliation
- Record **salvage recoveries** as offsets to dry-hole expense
- Expense **test-well contributions** when no proved reserves are found

**Service Alignment**:
- `DrillingScenarioAccountingService` for scenario-based capitalization

**Best Practice**:
```csharp
var drillingCost = await drillingScenarioAccountingService.RecordDrillingCostAsync(
    wellId,
    cost: 1500000m,
    scenario: "DRY_HOLE",
    costDate: DateTime.UtcNow,
    userId: userId);
```

## Inventory Valuation and LCM

- Value inventory using **FIFO/LIFO/Weighted Average**
- Apply **lower-of-cost-or-market (LCM)** at period close
- Record write-downs as **inventory adjustments** with reason codes
- Maintain valuation history for audit trail
- Use **NRV adjustments** for transport and quality deductions

**Service Alignment**:
- `InventoryService` for core valuation methods
- `InventoryLcmService` for LCM adjustments

**Best Practice**:
```csharp
var lcmAdjustment = await inventoryLcmService.ApplyLowerOfCostOrMarketAsync(
    inventoryItemId,
    DateTime.UtcNow,
    userId);
```

## Take-or-Pay Contracts

- Identify **minimum volume commitments** in sales contracts
- Record **deficiency charges** when deliveries fall below minimums
- Track make-up rights and obligation status
- Tie adjustments to the originating sales contract
- Use **contract schedules** for minimum volume and pricing by period

**Service Alignment**:
- `TakeOrPayService` for contract deficiency adjustments

**Best Practice**:
```csharp
var adjustment = await takeOrPayService.ApplyTakeOrPayAsync(
    runTicket,
    allocationResult,
    deliveredVolume,
    userId);
```

## Production Taxes

- Calculate **severance** and **ad valorem** taxes by jurisdiction
- Apply taxes on **gross revenue** before net allocations
- Persist tax transactions for filing and audit
- Align royalty statements to **tax deductions** where applicable
- Record **IDC deductions**, **tax depletion**, and **deferred tax** balances

**Service Alignment**:
- `ProductionTaxService` for production tax calculations

**Best Practice**:
```csharp
var tax = await productionTaxService.CalculateProductionTaxesAsync(
    revenueTransaction,
    userId);
```

## Authorization and Approvals

- Require **AFE approval** before capitalizing exploration or development costs
- Enforce **approval** for non-operated costs and revenue adjustments
- Track **authorization status** and approver metadata for auditability
- Reject or hold transactions when authorization is missing or expired

**Service Alignment**:
- `AuthorizationWorkflowService` for approval validation
- `AfeService` for AFE lifecycle and budgeting
- `InternalControlService` for segregation-of-duties checks

## Production Accounting

### Run Ticket Management
- Create run ticket from **certified measurement**
- Include **BSW**, **temperature**, **API gravity**
- Link to **lease**, **well**, **tank battery**
- Track **disposition** (sale, transfer, inventory)

**Best Practice**:
```csharp
var runTicket = productionManager.CreateRunTicket(
    leaseId: "LEASE-001",
    wellId: "WELL-001",
    tankBatteryId: "TANK-001",
    measurement: measurementRecord,
    dispositionType: DispositionType.Sale,
    purchaser: "PURCHASER-001"
);
```

### Production Allocation
- Allocate production to **working interests** accurately
- Use **measured volumes** from run tickets
- Apply **allocation factors** (ownership percentages)
- Handle **imbalances** and adjustments

**Best Practice**:
```csharp
var allocationEngine = new AllocationEngine();
var ownershipInterests = ownershipManager.GetOwnershipInterests(leaseId);

var allocations = allocationEngine.AllocateProduction(
    runTicket: runTicket,
    ownershipInterests: ownershipInterests,
    allocationMethod: AllocationMethod.WorkingInterest
);
```

### Tank Inventory Management
- Maintain **opening inventory**, **receipts**, **deliveries**, **closing inventory**
- Reconcile **book inventory** vs. **actual inventory**
- Track **inventory adjustments** and reasons
- Calculate **inventory valuation** (FIFO, LIFO, weighted average)

**Best Practice**:
```csharp
var inventory = productionManager.CreateTankInventory(
    tankBatteryId: "TANK-001",
    inventoryDate: DateTime.UtcNow,
    openingInventory: 10000m,
    receipts: 5000m,
    deliveries: 3000m,
    actualClosingInventory: 12000m  // May differ from calculated
);

// Calculate variance
var calculatedClosing = openingInventory + receipts - deliveries;
var variance = actualClosingInventory - calculatedClosing;
```

## Royalty Management

### Royalty Calculation
- Calculate royalty based on **gross revenue** (before deductions)
- Apply **royalty interest percentage** from lease or contract
- Deduct **royalty** before allocating to working interests
- Support **different royalty types** (mineral, overriding, production payment)

**Best Practice**:
```csharp
var royaltyManager = new RoyaltyManager();

// Register royalty interest
var royaltyInterest = new ROYALTY_INTEREST
{
    PROPERTY_ID = propertyId,
    ROYALTY_OWNER_BA_ID = royaltyOwnerId,
    INTEREST_TYPE = "MINERAL_ROYALTY",
    INTEREST_PERCENTAGE = 0.125m,  // 12.5%
    EFFECTIVE_DATE = new DateTime(2023, 1, 1)
};

// Calculate and create payment
var royaltyPayment = royaltyManager.CalculateAndCreatePayment(
    salesTransaction: salesTransaction,
    royaltyOwnerId: royaltyOwnerId,
    royaltyInterest: 0.125m,
    paymentDate: DateTime.UtcNow
);
```

### Royalty Payment
- Pay royalties **monthly** or **quarterly** per lease terms
- Provide **royalty statements** with production and revenue details
- Track **payment status** (pending, paid, overdue)
- Maintain **payment history**

**Best Practice**:
```csharp
var royaltyStatement = royaltyManager.CreateStatement(
    royaltyOwnerId: royaltyOwnerId,
    propertyOrLeaseId: leaseId,
    periodStart: new DateTime(2024, 1, 1),
    periodEnd: new DateTime(2024, 1, 31),
    transactions: salesTransactions,
    royaltyInterest: 0.125m
);
```

## Lease and Economic Interests

- Maintain **working interest**, **royalty interest**, and **NRI** with effective dates
- Validate **interest totals** do not exceed 100% per property/lease
- Use **division orders** to drive revenue and royalty allocations
- Tie lease economics to JIB, revenue, and reporting outputs

**Service Alignment**:
- `LeaseEconomicInterestService` for ownership/economic validation
- `AllocationService` for interest-based splits
- `RoyaltyService` for royalty computation and statements

## Joint Interest Billing (JIB)

### Cost Sharing
- Automate **cost allocation** among working interest owners
- Bill **non-operated** costs to working interest owners
- Include **overhead** and **administrative** charges
- Provide **detailed line items** for all costs

**Best Practice**:
```csharp
var joib = new JOINT_INTEREST_BILL
{
    JIB_NUMBER = "JIB-2024-001",
    OPERATOR_BA_ID = operatorId,
    BILLING_PERIOD_START = new DateTime(2024, 1, 1),
    BILLING_PERIOD_END = new DateTime(2024, 1, 31),
    TOTAL_AMOUNT = 1000000m
};

// Create line items
var lineItem = new JOIB_LINE_ITEM
{
    JOINT_INTEREST_BILL_ID = joib.JOINT_INTEREST_BILL_ID,
    COST_CATEGORY = "DRILLING",
    DESCRIPTION = "Development Well Drilling",
    AMOUNT = 500000m
};

// Allocate to working interest owners
var allocation = new JOIB_ALLOCATION
{
    JOIB_LINE_ITEM_ID = lineItem.JOIB_LINE_ITEM_ID,
    INTEREST_OWNER_BA_ID = ownerId,
    ALLOCATION_PERCENTAGE = 0.25m,  // 25% working interest
    ALLOCATED_AMOUNT = 500000m * 0.25m
};
```

### COPAS Overhead
- Apply **COPAS overhead** schedules consistently by cost category
- Document overhead bases and **effective dates**
- Separate overhead from direct costs in statements
- Audit overhead rate changes and approvals
- Maintain **overhead schedules** with effective/expiry dates

**Service Alignment**:
- `CopasOverheadService` for overhead calculation and statement charges

**Best Practice**:
```csharp
var overhead = await copasOverheadService.CalculateOverheadAsync(
    leaseId,
    baseAmount: totalCosts,
    asOfDate: periodEnd);
```

### Timely Billing
- Bill within **60-90 days** of cost incurrence
- Provide **approval workflow** for non-operated costs
- Track **payment status** and **aging**
- Reconcile with **operator statements**

## Internal Controls

### Segregation of Duties
- Separate **production**, **accounting**, and **approval** functions
- Require **dual approval** for capital expenditures
- Restrict **system access** based on role
- Maintain **audit logs** for all transactions

### Authorization
- Require **AFE approval** before capital expenditures
- Require **approval** for non-operated costs
- Require **approval** for revenue adjustments
- Maintain **approval workflow** and history

### Reconciliation
- Reconcile **production volumes** (run tickets vs. sales)
- Reconcile **revenue** (sales vs. cash receipts)
- Reconcile **costs** (invoices vs. payments)
- Reconcile **inventory** (book vs. actual)
- Perform **monthly reconciliations**

**Best Practice**:
```csharp
// Production reconciliation
var runTicketVolume = runTickets.Sum(rt => rt.NetVolume);
var salesVolume = salesTransactions.Sum(st => st.Volume);
var variance = runTicketVolume - salesVolume;

// Revenue reconciliation
var salesRevenue = salesTransactions.Sum(st => st.Revenue);
var cashReceipts = cashReceipts.Sum(cr => cr.Amount);
var accountsReceivable = salesRevenue - cashReceipts;
```

### Audit Trail
- Maintain **complete audit trail** of all transactions
- Track **who**, **what**, **when**, **why** for all changes
- Retain **supporting documentation** (run tickets, invoices, contracts)
- Implement **change tracking** for critical data

**Best Practice**:
```csharp
// All entity classes include standard PPDM audit columns
var transaction = new REVENUE_TRANSACTION
{
    // ... transaction data ...
    ROW_CREATED_BY = userId,
    ROW_CREATED_DATE = DateTime.UtcNow,
    ROW_CHANGED_BY = userId,
    ROW_CHANGED_DATE = DateTime.UtcNow
};
```

## Data Integrity

### PPDM Compliance
- Use **PPDM standard tables** where possible
- Follow **PPDM naming conventions**
- Include **standard audit columns** (ROW_CREATED_BY, ROW_CREATED_DATE, etc.)
- Maintain **referential integrity** with foreign keys

### Data Validation
- Validate **all inputs** (volumes, prices, costs)
- Enforce **business rules** (e.g., volumes cannot be negative)
- Check **data quality** (e.g., BSW cannot exceed 100%)
- Implement **data validation** at entry point

**Best Practice**:
```csharp
// Validation example
if (volume < 0)
    throw new InvalidAccountingDataException("Volume cannot be negative");

if (bsw > 100m)
    throw new InvalidAccountingDataException("BSW cannot exceed 100%");

if (price < 0)
    throw new InvalidAccountingDataException("Price cannot be negative");
```

### Version Control
- Track **changes** to critical data (ownership, contracts)
- Maintain **version history** for contracts and agreements
- Implement **effective dating** for time-based data
- Support **audit queries** for historical data

## Reporting and Compliance

### Financial Reports
- Generate **Income Statement** (revenue, costs, amortization)
- Generate **Balance Sheet** (assets, liabilities, equity)
- Generate **Cash Flow Statement** (operating, investing, financing)
- Include **FASB disclosures** (reserves, costs, production)

### Operational Reports
- Generate **Production Reports** (volumes by property, well, field)
- Generate **Revenue Reports** (revenue by property, product, customer)
- Generate **Cost Reports** (costs by property, well, cost center)
- Generate **AFE Reports** (budget vs. actual, variance analysis)

### Compliance Reports
- Generate **SEC Reports** (10-K, 10-Q disclosures)
- Generate **Tax Reports** (federal, state, local)
- Generate **Royalty Statements** (for royalty owners)
- Generate **JIB Statements** (for working interest owners)

## System Integration

### Integration with Production Systems
- Integrate with **production measurement** systems
- Integrate with **SCADA** systems for automatic measurement
- Integrate with **LACT units** for automatic measurement
- Maintain **data synchronization** between systems

### Integration with Land Systems
- Integrate with **land management** systems for ownership
- Integrate with **lease management** systems for contracts
- Maintain **ownership and contract** data synchronization

### Integration with Financial Systems
- Integrate with **ERP systems** for general ledger
- Integrate with **banking systems** for cash receipts
- Maintain **financial data** synchronization

## Accounting Foundation (GL)

### Core Accounting Services
- Post operational activity to **Journal Entries** and the **General Ledger**
- Maintain **Chart of Accounts** and account validation rules
- Provide **trial balance**, **financial statements**, and **GL reports**
- Use standardized **period closing** and **bank reconciliation** workflows

**Implementation Alignment**:
```csharp
var glAccountService = new GLAccountService();
var journalEntryService = new JournalEntryService();
var periodClosingService = new PeriodClosingService();
var financialStatementService = new FinancialStatementService();
```

### Posting and Controls
- Enforce **balanced entries** before posting to GL
- Require **approval** for manual journal entries
- Tag entries with **source module** for traceability
- Support **reversing entries** for corrections and accruals

## Period Close and Accruals

### Close Calendar and Cutoff
- Maintain a **monthly close calendar** with clear cutoff times
- Lock production and pricing data after close
- Use **effective dating** for late adjustments
- Require approvals for reopenings

**Best Practice**:
```csharp
var closePeriod = new ACCOUNTING_PERIOD
{
    PERIOD_NAME = "2024-01",
    START_DATE = new DateTime(2024, 1, 1),
    END_DATE = new DateTime(2024, 1, 31),
    STATUS = "CLOSED",
    CLOSED_BY = userId,
    CLOSED_DATE = DateTime.UtcNow
};
```

### Revenue and Expense Accruals
- Accrue **unbilled revenue** based on production and pricing estimates
- Accrue **unmatched expenses** for received services without invoices
- Reverse accruals when actuals post
- Track accrual **source and basis** for auditability

**Best Practice**:
```csharp
var accrual = new ACCRUAL_TRANSACTION
{
    PROPERTY_ID = propertyId,
    PERIOD = "2024-01",
    ACCRUAL_TYPE = "REVENUE_ESTIMATE",
    AMOUNT = 250000m,
    BASIS = "RUN_TICKET_VOLUME * INDEX_PRICE",
    REVERSAL_PERIOD = "2024-02"
};
```

## Adjustments and Corrections

### Adjustment Controls
- Require **reason codes** and approvals for adjustments
- Preserve **original transactions**; do not overwrite
- Post **reversals** and **correcting entries**
- Maintain **effective dates** for regulatory compliance

**Best Practice**:
```csharp
var adjustment = new ADJUSTMENT_REQUEST
{
    SOURCE_TRANSACTION_ID = revenueTransactionId,
    ADJUSTMENT_REASON = "PRICE_CORRECTION",
    REQUESTED_BY = userId,
    REQUESTED_DATE = DateTime.UtcNow,
    APPROVAL_STATUS = "PENDING"
};
```

### Measurement Corrections
- Log **meter corrections** and revised run tickets
- Retain **before/after** values for volume and BSW
- Recalculate allocations and downstream revenue
- Notify affected owners and partners

**Best Practice**:
```csharp
var measurementCorrection = new MEASUREMENT_ADJUSTMENT
{
    RUN_TICKET_ID = runTicketId,
    ORIGINAL_NET_VOLUME = 995.0m,
    CORRECTED_NET_VOLUME = 1002.5m,
    ADJUSTMENT_REASON = "METER_FACTOR_UPDATE"
};
```

## Partner and Joint Venture Accounting

### Cash Calls and Billings
- Issue **cash calls** for operated projects when required
- Track **call vs. receipt** and apply to AFEs
- Use **standard overhead rates** and document changes
- Reconcile operator vs. non-operator statements

**Best Practice**:
```csharp
var cashCall = new CASH_CALL
{
    AFE_ID = afe.AFE_ID,
    CALL_NUMBER = "CC-2024-001",
    CALL_AMOUNT = 1500000m,
    DUE_DATE = new DateTime(2024, 2, 15)
};
```

### Partner Statements
- Provide **monthly partner statements** with detail
- Include **ownership changes** and effective dates
- Highlight **disputes** and resolution status
- Maintain statement delivery audit trail

**Best Practice**:
```csharp
var partnerStatement = new PARTNER_STATEMENT
{
    PARTNER_BA_ID = partnerId,
    PERIOD = "2024-01",
    TOTAL_CHARGES = 450000m,
    TOTAL_CREDITS = 120000m,
    NET_DUE = 330000m
};
```

## Tax and Regulatory Reporting

### Severance and Production Taxes
- Calculate **severance taxes** by jurisdiction
- Apply **tax exemptions** and reduced rates
- Track **taxable volume** separately from net volume
- Reconcile tax accruals to filings

**Best Practice**:
```csharp
var taxCalculation = new TAX_CALCULATION
{
    JURISDICTION = "TX",
    TAX_TYPE = "SEVERANCE",
    TAXABLE_VOLUME = 10000m,
    TAX_RATE = 0.046m,
    TAX_AMOUNT = 460m
};
```

### Regulatory Filings
- Support **state production** and **royalty** filings
- Maintain **audit-ready** documentation
- Track filing **deadlines** and confirmations
- Store filing references and acknowledgments

## Data Governance and Master Data

### Master Data Stewardship
- Define **data owners** for properties, wells, units, owners
- Enforce **unique identifiers** and naming standards
- Validate **ownership effective dates** and hierarchy
- Maintain change history with approvals

**Best Practice**:
```csharp
var validationResult = masterDataValidator.ValidateOwnership(
    leaseId: leaseId,
    effectiveDate: new DateTime(2024, 1, 1),
    interests: ownershipInterests
);
if (!validationResult.IsValid)
    throw new InvalidAccountingDataException(validationResult.Message);
```

### Reference Data Controls
- Centralize **product codes**, cost types, and reason codes
- Version **pricing indices** and allocation methods
- Restrict changes to authorized roles
- Apply changes with effective dating

## Monitoring and Exception Management

### Automated Exception Rules
- Monitor **volume variances** vs. expected ranges
- Flag **negative prices** or out-of-range differentials
- Alert on **unbalanced allocations** and missing owners
- Escalate exceptions based on severity

**Best Practice**:
```csharp
var exceptionRule = new EXCEPTION_RULE
{
    RULE_NAME = "VOLUME_VARIANCE",
    THRESHOLD_PERCENT = 2.5m,
    SEVERITY = "HIGH",
    ACTION = "NOTIFY_AND_HOLD"
};
```

### KPI Monitoring
- Track **days-to-close**, **billing timeliness**, **dispute rate**
- Monitor **inventory variance** and **measurement corrections**
- Report **accrual accuracy** and reversal timing
- Use dashboards for operational visibility

## Conclusion

Following these best practices ensures:
- **FASB and SEC compliance**
- **Accurate financial reporting**
- **Efficient operations**
- **Strong internal controls**
- **Data integrity**
- **Regulatory compliance**

The `Beep.OilandGas.ProductionAccounting` system implements these best practices throughout all modules.
