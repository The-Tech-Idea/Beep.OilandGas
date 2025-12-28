# Oil and Gas Accounting Best Practices

## Overview

This document outlines best practices for oil and gas accounting following FASB, SEC, and industry standards. These practices are implemented throughout the `Beep.OilandGas.ProductionAccounting` system.

## Table of Contents

1. [Financial Accounting Standards](#financial-accounting-standards)
2. [Revenue Recognition](#revenue-recognition)
3. [Cost Management](#cost-management)
4. [Production Accounting](#production-accounting)
5. [Royalty Management](#royalty-management)
6. [Joint Interest Billing](#joint-interest-billing)
7. [Internal Controls](#internal-controls)
8. [Data Integrity](#data-integrity)
9. [Reporting and Compliance](#reporting-and-compliance)
10. [System Integration](#system-integration)

## Financial Accounting Standards

### Successful Efforts Method (FASB Statement No. 19)

#### Property Acquisition
- ✅ **Capitalize** all acquisition costs as unproved property
- ✅ Track by property ID with acquisition date and cost
- ✅ Reclassify to proved property when reserves are discovered
- ✅ Test for impairment annually on unproved properties

**Implementation**:
```csharp
var seAccounting = AccountingManager.CreateSuccessfulEffortsAccounting();
seAccounting.RecordAcquisition(unprovedProperty);
```

#### Exploration Costs
- ✅ **Expense** geological and geophysical (G&G) costs as incurred
- ✅ **Capitalize** exploratory drilling costs if well finds proved reserves
- ✅ **Expense** exploratory drilling costs if well is dry hole
- ✅ Maintain detailed records of exploration activities

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
- ✅ **Capitalize** all development costs (drilling, completion, facilities)
- ✅ Track by property and well
- ✅ Link to AFE (Authorization for Expenditure) for approval tracking

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
- ✅ **Expense** all production costs (lifting costs) as incurred
- ✅ Include operating expenses, maintenance, workovers
- ✅ Track by property and well for cost allocation

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
- ✅ Use **units-of-production method** based on proved reserves
- ✅ Calculate amortization rate: Net Capitalized Costs / Total Proved Reserves BOE
- ✅ Apply rate to production for period
- ✅ Update reserves annually

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
- ✅ **Capitalize** all exploration and development costs
- ✅ Organize by cost center (country, region, or field)
- ✅ No distinction between successful and unsuccessful exploration

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
- ✅ Perform **annually** to ensure capitalized costs don't exceed discounted future net cash flows
- ✅ Calculate: Net Capitalized Costs vs. Discounted Future Net Cash Flows
- ✅ Record impairment if ceiling exceeded
- ✅ Use appropriate discount rate (typically 10%)

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

## Revenue Recognition

### Point of Sale
- ✅ Recognize revenue when **title transfers** (typically at wellhead or delivery point)
- ✅ Use run tickets or LACT unit measurements for volume
- ✅ Apply pricing at time of sale (index-based with differentials)

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
- ✅ Use **index-based pricing** (WTI, Brent, Henry Hub)
- ✅ Apply **differentials** for quality, location, transportation
- ✅ Document pricing methodology in contracts
- ✅ Maintain price index history

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
- ✅ Use **certified measurement** (run tickets, LACT units, meters)
- ✅ Apply **BSW (Basic Sediment and Water)** adjustments
- ✅ Convert to **standard conditions** (temperature, pressure)
- ✅ Maintain measurement audit trail

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
- ✅ Allocate revenue to **working interests** based on ownership
- ✅ Deduct **royalties** before allocation
- ✅ Apply **production sharing** agreements if applicable
- ✅ Maintain allocation audit trail

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
- ✅ Require **AFE approval** before capital expenditures
- ✅ Track **budget vs. actual** costs
- ✅ Monitor **variance** and require approval for overruns
- ✅ Link all costs to AFE for approval tracking

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
- ✅ Organize costs by **property**, **well**, **field**, or **region**
- ✅ Enable cost allocation and reporting
- ✅ Support both Successful Efforts and Full Cost methods

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
- ✅ Allocate **shared costs** (facilities, overhead) equitably
- ✅ Use **volume-based**, **revenue-based**, or **equity-based** allocation
- ✅ Document allocation methodology
- ✅ Maintain allocation audit trail

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

## Production Accounting

### Run Ticket Management
- ✅ Create run ticket from **certified measurement**
- ✅ Include **BSW**, **temperature**, **API gravity**
- ✅ Link to **lease**, **well**, **tank battery**
- ✅ Track **disposition** (sale, transfer, inventory)

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
- ✅ Allocate production to **working interests** accurately
- ✅ Use **measured volumes** from run tickets
- ✅ Apply **allocation factors** (ownership percentages)
- ✅ Handle **imbalances** and adjustments

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
- ✅ Maintain **opening inventory**, **receipts**, **deliveries**, **closing inventory**
- ✅ Reconcile **book inventory** vs. **actual inventory**
- ✅ Track **inventory adjustments** and reasons
- ✅ Calculate **inventory valuation** (FIFO, LIFO, weighted average)

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
- ✅ Calculate royalty based on **gross revenue** (before deductions)
- ✅ Apply **royalty interest percentage** from lease or contract
- ✅ Deduct **royalty** before allocating to working interests
- ✅ Support **different royalty types** (mineral, overriding, production payment)

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
- ✅ Pay royalties **monthly** or **quarterly** per lease terms
- ✅ Provide **royalty statements** with production and revenue details
- ✅ Track **payment status** (pending, paid, overdue)
- ✅ Maintain **payment history**

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

## Joint Interest Billing (JIB)

### Cost Sharing
- ✅ Automate **cost allocation** among working interest owners
- ✅ Bill **non-operated** costs to working interest owners
- ✅ Include **overhead** and **administrative** charges
- ✅ Provide **detailed line items** for all costs

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

### Timely Billing
- ✅ Bill within **60-90 days** of cost incurrence
- ✅ Provide **approval workflow** for non-operated costs
- ✅ Track **payment status** and **aging**
- ✅ Reconcile with **operator statements**

## Internal Controls

### Segregation of Duties
- ✅ Separate **production**, **accounting**, and **approval** functions
- ✅ Require **dual approval** for capital expenditures
- ✅ Restrict **system access** based on role
- ✅ Maintain **audit logs** for all transactions

### Authorization
- ✅ Require **AFE approval** before capital expenditures
- ✅ Require **approval** for non-operated costs
- ✅ Require **approval** for revenue adjustments
- ✅ Maintain **approval workflow** and history

### Reconciliation
- ✅ Reconcile **production volumes** (run tickets vs. sales)
- ✅ Reconcile **revenue** (sales vs. cash receipts)
- ✅ Reconcile **costs** (invoices vs. payments)
- ✅ Reconcile **inventory** (book vs. actual)
- ✅ Perform **monthly reconciliations**

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
- ✅ Maintain **complete audit trail** of all transactions
- ✅ Track **who**, **what**, **when**, **why** for all changes
- ✅ Retain **supporting documentation** (run tickets, invoices, contracts)
- ✅ Implement **change tracking** for critical data

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
- ✅ Use **PPDM standard tables** where possible
- ✅ Follow **PPDM naming conventions**
- ✅ Include **standard audit columns** (ROW_CREATED_BY, ROW_CREATED_DATE, etc.)
- ✅ Maintain **referential integrity** with foreign keys

### Data Validation
- ✅ Validate **all inputs** (volumes, prices, costs)
- ✅ Enforce **business rules** (e.g., volumes cannot be negative)
- ✅ Check **data quality** (e.g., BSW cannot exceed 100%)
- ✅ Implement **data validation** at entry point

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
- ✅ Track **changes** to critical data (ownership, contracts)
- ✅ Maintain **version history** for contracts and agreements
- ✅ Implement **effective dating** for time-based data
- ✅ Support **audit queries** for historical data

## Reporting and Compliance

### Financial Reports
- ✅ Generate **Income Statement** (revenue, costs, amortization)
- ✅ Generate **Balance Sheet** (assets, liabilities, equity)
- ✅ Generate **Cash Flow Statement** (operating, investing, financing)
- ✅ Include **FASB disclosures** (reserves, costs, production)

### Operational Reports
- ✅ Generate **Production Reports** (volumes by property, well, field)
- ✅ Generate **Revenue Reports** (revenue by property, product, customer)
- ✅ Generate **Cost Reports** (costs by property, well, cost center)
- ✅ Generate **AFE Reports** (budget vs. actual, variance analysis)

### Compliance Reports
- ✅ Generate **SEC Reports** (10-K, 10-Q disclosures)
- ✅ Generate **Tax Reports** (federal, state, local)
- ✅ Generate **Royalty Statements** (for royalty owners)
- ✅ Generate **JIB Statements** (for working interest owners)

## System Integration

### Integration with Production Systems
- ✅ Integrate with **production measurement** systems
- ✅ Integrate with **SCADA** systems for automatic measurement
- ✅ Integrate with **LACT units** for automatic measurement
- ✅ Maintain **data synchronization** between systems

### Integration with Land Systems
- ✅ Integrate with **land management** systems for ownership
- ✅ Integrate with **lease management** systems for contracts
- ✅ Maintain **ownership and contract** data synchronization

### Integration with Financial Systems
- ✅ Integrate with **ERP systems** for general ledger
- ✅ Integrate with **banking systems** for cash receipts
- ✅ Maintain **financial data** synchronization

## Conclusion

Following these best practices ensures:
- ✅ **FASB and SEC compliance**
- ✅ **Accurate financial reporting**
- ✅ **Efficient operations**
- ✅ **Strong internal controls**
- ✅ **Data integrity**
- ✅ **Regulatory compliance**

The `Beep.OilandGas.ProductionAccounting` system implements these best practices throughout all modules.

