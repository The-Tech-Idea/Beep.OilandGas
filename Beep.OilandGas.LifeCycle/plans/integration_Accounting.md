# Beep.OilandGas.Accounting - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.Accounting** is a comprehensive .NET library for oil and gas accounting calculations, supporting both **Successful Efforts** and **Full Cost** accounting methods as defined by FASB Statement No. 19.

### Key Capabilities
- **Successful Efforts Accounting**: Capitalize only costs directly related to discovered reserves
- **Full Cost Accounting**: Capitalize all exploration and development costs
- **Cost Categorization**: Proper classification of exploration, development, and production costs
- **Amortization**: Reserve-based amortization of capitalized costs
- **Interest Capitalization**: Capitalizing interest on qualifying assets
- **Dry Hole Expense**: Expensing unsuccessful exploration costs
- **Impairment**: Loss recognition for unproved properties

### Current Status
✅ **Partially Integrated** - Used in `PPDMAccountingService` for production accounting operations

---

## Key Classes and Interfaces

### Main Classes

#### `AccountingManager`
Static manager class for accounting operations.

**Key Methods:**
```csharp
public static class AccountingManager
{
    public static SuccessfulEffortsAccounting CreateSuccessfulEffortsAccounting();
    public static FullCostAccounting CreateFullCostAccounting();
    public static decimal CalculateAmortization(
        decimal netCapitalizedCosts,
        decimal totalProvedReservesBOE,
        decimal productionBOE);
    public static decimal CalculateInterestCapitalization(InterestCapitalizationData data);
    public static decimal ConvertProductionToBOE(ProductionData production);
    public static decimal ConvertReservesToBOE(ProvedReserves reserves);
}
```

#### `SuccessfulEffortsAccounting`
Successful Efforts accounting implementation.

**Key Methods:**
```csharp
public class SuccessfulEffortsAccounting
{
    public void RecordAcquisition(UnprovedProperty property);
    public void RecordExplorationCosts(ExplorationCosts costs);
    public void RecordDevelopmentCosts(DevelopmentCosts costs);
    public void RecordProductionCosts(ProductionCosts costs);
    public void RecordDryHole(ExplorationCosts costs);
    public void ClassifyAsProved(UnprovedProperty property, ProvedReserves reserves);
    public decimal CalculateAmortization(ProvedProperty property, ProvedReserves reserves, decimal production);
    public void RecordImpairment(UnprovedProperty property, decimal impairmentAmount);
}
```

#### `FullCostAccounting`
Full Cost accounting implementation.

**Key Methods:**
```csharp
public class FullCostAccounting
{
    public void RecordExplorationCosts(ExplorationCosts costs);
    public void RecordDevelopmentCosts(DevelopmentCosts costs);
    public decimal CalculateAmortization(
        decimal totalCapitalizedCosts,
        decimal totalProvedReserves,
        decimal production);
}
```

### Models

#### `UnprovedProperty`
Unproved property model.

```csharp
public class UnprovedProperty
{
    public string PropertyId { get; set; }
    public decimal AcquisitionCost { get; set; }
    public DateTime AcquisitionDate { get; set; }
}
```

#### `ProvedReserves`
Proved reserves model.

```csharp
public class ProvedReserves
{
    public string PropertyId { get; set; }
    public decimal OilReserves { get; set; }  // barrels
    public decimal GasReserves { get; set; }   // MCF
    public DateTime ReserveDate { get; set; }
}
```

#### `ExplorationCosts`
Exploration costs model.

```csharp
public class ExplorationCosts
{
    public string PropertyId { get; set; }
    public decimal GeologicalGeophysicalCosts { get; set; }
    public decimal ExploratoryDrillingCosts { get; set; }
    public bool IsDryHole { get; set; }
    public bool FoundProvedReserves { get; set; }
}
```

---

## Integration with LifeCycle Services

### Current Integration

**Service:** `PPDMAccountingService`  
**Location:** `Beep.OilandGas.LifeCycle.Services.Accounting.PPDMAccountingService`

### Integration Points

1. **Production Accounting**
   - Volume reconciliation
   - Royalty calculations
   - Cost allocation
   - Uses PPDM tables for production data

2. **Data Flow**
   ```
   Production Data (PPDM) → Accounting Calculations → Accounting Results → PPDM Database
   ```

3. **Storage**
   - Results stored in PPDM accounting-related tables
   - Uses `PPDMGenericRepository` for data access
   - Links to `WELL`, `POOL`, `FIELD` via IDs

### Service Methods

```csharp
public interface IAccountingService
{
    Task<AccountingAllocationResult> AllocateProductionVolumesAsync(AllocationRequest request);
    Task<RoyaltyCalculationResult> CalculateRoyaltiesAsync(RoyaltyRequest request);
    Task<CostAllocationResult> AllocateCostsAsync(CostAllocationRequest request);
}
```

---

## Usage Examples

### Example 1: Successful Efforts Accounting

```csharp
using Beep.OilandGas.Accounting;
using Beep.OilandGas.Accounting.SuccessfulEfforts;
using Beep.OilandGas.Accounting.Models;

// Create accounting instance
var accounting = AccountingManager.CreateSuccessfulEffortsAccounting();

// Record property acquisition
var property = new UnprovedProperty
{
    PropertyId = "Lease-A-001",
    AcquisitionCost = 60000m,
    AcquisitionDate = new DateTime(2023, 1, 1)
};
accounting.RecordAcquisition(property);

// Record exploration costs
var explorationCosts = new ExplorationCosts
{
    PropertyId = "Lease-A-001",
    GeologicalGeophysicalCosts = 50000m,
    ExploratoryDrillingCosts = 300000m
};
accounting.RecordExplorationCosts(explorationCosts);

// If well is successful, classify as proved
var provedReserves = new ProvedReserves
{
    PropertyId = "Lease-A-001",
    OilReserves = 1000000m,  // barrels
    GasReserves = 5000000m,   // MCF
    ReserveDate = new DateTime(2023, 12, 31)
};
accounting.ClassifyAsProved(property, provedReserves);

// Calculate amortization
var amortization = accounting.CalculateAmortization(
    property: provedProperty,
    reserves: provedReserves,
    production: 100000m);  // BOE
```

### Example 2: Full Cost Accounting

```csharp
using Beep.OilandGas.Accounting.FullCost;

var fullCostAccounting = AccountingManager.CreateFullCostAccounting();

// Record exploration costs (all capitalized)
fullCostAccounting.RecordExplorationCosts(explorationCosts);
fullCostAccounting.RecordDevelopmentCosts(developmentCosts);

// Calculate amortization based on total reserves
var amortization = fullCostAccounting.CalculateAmortization(
    totalCapitalizedCosts: 5000000m,
    totalProvedReserves: 10000000m,  // BOE
    production: 500000m);              // BOE
```

### Example 3: Integration with LifeCycle Service

```csharp
// In a controller or service
var accountingService = serviceProvider.GetRequiredService<IAccountingService>();

// Allocate production volumes
var allocationRequest = new AllocationRequest
{
    FieldId = "FIELD-001",
    ProductionDate = DateTime.Now,
    TotalProduction = 10000m,  // barrels
    AllocationMethod = "WorkingInterest"
};

var allocationResult = await accountingService.AllocateProductionVolumesAsync(allocationRequest);

// Calculate royalties
var royaltyRequest = new RoyaltyRequest
{
    FieldId = "FIELD-001",
    ProductionDate = DateTime.Now,
    RoyaltyRate = 0.125m  // 12.5%
};

var royaltyResult = await accountingService.CalculateRoyaltiesAsync(royaltyRequest);
```

### Example 4: Amortization Calculation

```csharp
// Calculate amortization using AccountingManager
var netCapitalizedCosts = 5000000m;
var totalProvedReservesBOE = 10000000m;
var productionBOE = 500000m;

var amortization = AccountingManager.CalculateAmortization(
    netCapitalizedCosts,
    totalProvedReservesBOE,
    productionBOE);

Console.WriteLine($"Amortization: ${amortization:N2}");
```

### Example 5: BOE Conversion

```csharp
// Convert production to BOE
var production = new ProductionData
{
    OilProduction = 1000m,  // barrels
    GasProduction = 5000m    // MCF
};

var boe = AccountingManager.ConvertProductionToBOE(production);
Console.WriteLine($"Total Production: {boe:F2} BOE");

// Convert reserves to BOE
var reserves = new ProvedReserves
{
    OilReserves = 1000000m,  // barrels
    GasReserves = 5000000m    // MCF
};

var reservesBOE = AccountingManager.ConvertReservesToBOE(reserves);
Console.WriteLine($"Total Reserves: {reservesBOE:F2} BOE");
```

---

## Integration Patterns

### Adding Accounting to a New Service

1. **Add Dependency**
   ```csharp
   using Beep.OilandGas.Accounting;
   using Beep.OilandGas.Accounting.SuccessfulEfforts;
   using Beep.OilandGas.Accounting.Models;
   ```

2. **Use Accounting Manager**
   ```csharp
   public class MyService
   {
       public void PerformAccounting(string propertyId)
       {
           var accounting = AccountingManager.CreateSuccessfulEffortsAccounting();
           
           // Record costs, calculate amortization, etc.
       }
   }
   ```

3. **Use Accounting Service**
   ```csharp
   public class MyService
   {
       private readonly IAccountingService _accountingService;
       
       public async Task<RoyaltyCalculationResult> CalculateRoyaltiesAsync(string fieldId)
       {
           var request = new RoyaltyRequest
           {
               FieldId = fieldId,
               ProductionDate = DateTime.Now,
               RoyaltyRate = 0.125m
           };
           
           return await _accountingService.CalculateRoyaltiesAsync(request);
       }
   }
   ```

### Error Handling

```csharp
try
{
    var accounting = AccountingManager.CreateSuccessfulEffortsAccounting();
    accounting.RecordAcquisition(property);
}
catch (InvalidAccountingDataException ex)
{
    _logger.LogError(ex, "Invalid accounting data: {FieldName} - {Message}", 
        ex.FieldName, ex.Message);
    throw;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error performing accounting operation");
    throw;
}
```

---

## Data Storage

### PPDM Tables

#### Existing PPDM Tables (Use These)

**Status:** ✅ **Existing PPDM Tables** - These tables already exist in PPDM39.

1. **`FINANCE`** - Financial transactions
   - Stores financial transaction data
   - Links to entities via foreign keys
   - Use for: General financial transactions

2. **`OBLIGATION`** - Obligations and payments
   - Stores obligation records
   - Links to properties, leases, etc.
   - Use for: Royalty obligations, payment obligations

3. **`OBLIG_PAYMENT`** - Payment details
   - Stores payment transaction details
   - Links to `OBLIGATION`
   - Use for: Payment records

4. **`OBLIGATION_COMPONENT`** - Obligation components
   - Stores component details of obligations
   - Links to `OBLIGATION`
   - Use for: Detailed obligation breakdowns

#### New Tables Needed (To Be Created)

**Status:** ⚠️ **New Tables Needed** - These tables do not exist in PPDM39 and must be created following PPDM patterns.

1. **`ACCOUNTING_METHOD`** - Accounting method configuration
   ```sql
   CREATE TABLE ACCOUNTING_METHOD (
       ACCOUNTING_METHOD_ID VARCHAR(50) PRIMARY KEY,
       FIELD_ID VARCHAR(50) NULL,
       METHOD_TYPE VARCHAR(50),  -- 'SUCCESSFUL_EFFORTS' or 'FULL_COST'
       EFFECTIVE_DATE DATETIME,
       -- Standard PPDM columns
       ROW_ID VARCHAR(50),
       ROW_CHANGED_BY VARCHAR(50),
       ROW_CHANGED_DATE DATETIME,
       ROW_CREATED_BY VARCHAR(50),
       ROW_CREATED_DATE DATETIME
   );
   ```

2. **`ACCOUNTING_COST`** - Accounting cost records
   ```sql
   CREATE TABLE ACCOUNTING_COST (
       ACCOUNTING_COST_ID VARCHAR(50) PRIMARY KEY,
       PROPERTY_ID VARCHAR(50) NULL,  -- Link to lease/property
       WELL_ID VARCHAR(50) NULL,
       FIELD_ID VARCHAR(50) NULL,
       COST_TYPE VARCHAR(50),  -- 'EXPLORATION', 'DEVELOPMENT', 'PRODUCTION', 'ACQUISITION'
       COST_CATEGORY VARCHAR(50),  -- 'G&G', 'DRILLING', 'FACILITY', etc.
       AMOUNT DECIMAL(18,2),
       COST_DATE DATETIME,
       IS_CAPITALIZED BIT,
       IS_EXPENSED BIT,
       DRY_HOLE_FLAG BIT,
       -- Standard PPDM columns
       ROW_ID VARCHAR(50),
       ROW_CHANGED_BY VARCHAR(50),
       ROW_CHANGED_DATE DATETIME,
       ROW_CREATED_BY VARCHAR(50),
       ROW_CREATED_DATE DATETIME
   );
   ```

3. **`ACCOUNTING_AMORTIZATION`** - Amortization records
   ```sql
   CREATE TABLE ACCOUNTING_AMORTIZATION (
       ACCOUNTING_AMORTIZATION_ID VARCHAR(50) PRIMARY KEY,
       PROPERTY_ID VARCHAR(50) NULL,
       WELL_ID VARCHAR(50) NULL,
       POOL_ID VARCHAR(50) NULL,
       PERIOD_START_DATE DATETIME,
       PERIOD_END_DATE DATETIME,
       CAPITALIZED_COST DECIMAL(18,2),
       PRODUCTION_BOE DECIMAL(18,2),
       TOTAL_RESERVES_BOE DECIMAL(18,2),
       AMORTIZATION_AMOUNT DECIMAL(18,2),
       -- Standard PPDM columns
       ROW_ID VARCHAR(50),
       ROW_CHANGED_BY VARCHAR(50),
       ROW_CHANGED_DATE DATETIME,
       ROW_CREATED_BY VARCHAR(50),
       ROW_CREATED_DATE DATETIME
   );
   ```

4. **`ACCOUNTING_IMPAIRMENT`** - Impairment records
   ```sql
   CREATE TABLE ACCOUNTING_IMPAIRMENT (
       ACCOUNTING_IMPAIRMENT_ID VARCHAR(50) PRIMARY KEY,
       PROPERTY_ID VARCHAR(50) NULL,
       IMPAIRMENT_DATE DATETIME,
       IMPAIRMENT_AMOUNT DECIMAL(18,2),
       REASON VARCHAR(500),
       -- Standard PPDM columns
       ROW_ID VARCHAR(50),
       ROW_CHANGED_BY VARCHAR(50),
       ROW_CHANGED_DATE DATETIME,
       ROW_CREATED_BY VARCHAR(50),
       ROW_CREATED_DATE DATETIME
   );
   ```

### Relationships

**Existing Tables:**
- `FINANCE` links to `FIELD`, `WELL`, `POOL` via foreign keys
- `OBLIGATION` links to properties and leases
- `OBLIG_PAYMENT.OBLIGATION_ID` → `OBLIGATION.OBLIGATION_ID`

**New Tables:**
- `ACCOUNTING_COST.FIELD_ID` → `FIELD.FIELD_ID`
- `ACCOUNTING_COST.WELL_ID` → `WELL.WELL_ID`
- `ACCOUNTING_AMORTIZATION.POOL_ID` → `POOL.POOL_ID`
- `ACCOUNTING_METHOD.FIELD_ID` → `FIELD.FIELD_ID`

---

## Best Practices

1. **Accounting Method Selection**
   - Choose Successful Efforts or Full Cost based on company policy
   - Maintain consistency across all properties
   - Document method selection

2. **Cost Classification**
   - Properly classify exploration, development, and production costs
   - Follow FASB Statement No. 19 guidelines
   - Maintain detailed cost records

3. **Reserve Updates**
   - Update reserves regularly for accurate amortization
   - Use proved reserves only for amortization
   - Convert to BOE for consistent calculations

4. **Impairment Testing**
   - Regularly test unproved properties for impairment
   - Record impairments when properties become worthless
   - Maintain impairment documentation

---

## Future Enhancements

### Planned Integrations

1. **Exploration Service Integration**
   - Automatic cost recording for exploration activities
   - Integration with PROSPECT system
   - Dry hole expense tracking

2. **Development Service Integration**
   - Automatic cost recording for development activities
   - Development cost capitalization
   - Integration with PIPELINE system

3. **Production Service Integration**
   - Production cost tracking
   - Amortization calculations based on production
   - Royalty calculations

4. **Economic Analysis Integration**
   - Integration with economic analysis for project evaluation
   - Cost inputs for NPV/IRR calculations
   - Financial reporting

### Potential Improvements

- Add support for international accounting standards
- Integration with financial systems
- Automated journal entry generation
- Enhanced reporting and analytics
- Support for joint venture accounting

---

## References

- **Project Location:** `Beep.OilandGas.Accounting`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Accounting.PPDMAccountingService`
- **Documentation:** `Beep.OilandGas.Accounting/README.md`
- **PPDM Tables:** `FINANCE`, `OBLIGATION`, `OBLIG_PAYMENT`

---

**Last Updated:** 2024  
**Status:** ✅ Partially Integrated

