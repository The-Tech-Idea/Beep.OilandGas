# Beep.OilandGas.ProductionAccounting - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.ProductionAccounting** is a comprehensive oil and gas production accounting and operations management system. It provides complete production accounting functionality covering crude oil properties, lease management, production measurement, allocation, pricing, royalty calculations, and reporting.

### Key Capabilities
- **Crude Oil Properties**: Classification and property calculations
- **Lease Management**: Fee, Government, Net Profit, Joint Interest leases
- **Storage Facilities**: Tank batteries, test separators, LACT units
- **Production Measurement**: Production allocation and measurement
- **Pricing and Valuation**: Oil pricing and valuation
- **Royalty Calculations**: Royalty calculations and payments
- **Sales Accounting**: Sales agreements and accounting
- **Reporting**: Internal and external reporting

### Current Status
✅ **Partially Integrated** - Used in `PPDMAccountingService` for production accounting operations

---

## Key Classes and Interfaces

### Main Classes

#### `LeaseManager`
Lease management operations.

**Key Methods:**
```csharp
public class LeaseManager
{
    public void RegisterLease(Lease lease);
    public Lease GetLease(string leaseId);
    public void UpdateLease(Lease lease);
    public void RegisterSalesAgreement(OilSalesAgreement agreement);
}
```

#### `CrudeOilCalculations`
Crude oil property calculations.

**Key Methods:**
```csharp
public static class CrudeOilCalculations
{
    public static decimal CalculateNetVolume(decimal grossVolume, decimal bsw);
    public static decimal CalculateAPIFromSpecificGravity(decimal specificGravity);
    public static CrudeOilType GetCrudeOilType(decimal apiGravity);
}
```

### Models

#### `CrudeOilProperties`
Crude oil properties model.

```csharp
public class CrudeOilProperties
{
    public decimal ApiGravity { get; set; }
    public decimal SulfurContent { get; set; }
    public decimal BSW { get; set; }              // Basic Sediment and Water
    public decimal WaterContent { get; set; }
    public CrudeOilType GetCrudeOilType();
}
```

#### `FeeMineralLease`
Fee mineral lease model.

```csharp
public class FeeMineralLease
{
    public string LeaseId { get; set; }
    public string LeaseName { get; set; }
    public DateTime EffectiveDate { get; set; }
    public int PrimaryTermMonths { get; set; }
    public decimal WorkingInterest { get; set; }
    public decimal NetRevenueInterest { get; set; }
    public decimal RoyaltyRate { get; set; }
}
```

#### `OilSalesAgreement`
Oil sales agreement model.

```csharp
public class OilSalesAgreement
{
    public string AgreementId { get; set; }
    public string Seller { get; set; }
    public string Purchaser { get; set; }
    public DateTime EffectiveDate { get; set; }
    public PricingTerms PricingTerms { get; set; }
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
   Production Data (PPDM) → Production Accounting → Accounting Results → PPDM Database
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

### Example 1: Crude Oil Properties

```csharp
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Calculations;

var properties = new CrudeOilProperties
{
    ApiGravity = 35.5m,
    SulfurContent = 0.8m,
    BSW = 0.5m,
    WaterContent = 0.3m
};

var oilType = properties.GetCrudeOilType(); // Light, Medium, Heavy
var netVolume = CrudeOilCalculations.CalculateNetVolume(1000m, 0.5m);
Console.WriteLine($"Net Volume: {netVolume} barrels");
```

### Example 2: Lease Management

```csharp
using Beep.OilandGas.ProductionAccounting.Management;
using Beep.OilandGas.ProductionAccounting.Models;

var manager = new LeaseManager();

var lease = new FeeMineralLease
{
    LeaseId = "Lease-001",
    LeaseName = "Smith Ranch",
    EffectiveDate = new DateTime(2023, 1, 1),
    PrimaryTermMonths = 60,
    WorkingInterest = 0.75m,
    NetRevenueInterest = 0.65625m,
    RoyaltyRate = 0.125m
};

manager.RegisterLease(lease);
var retrievedLease = manager.GetLease("Lease-001");
```

### Example 3: Sales Agreements

```csharp
var salesAgreement = new OilSalesAgreement
{
    AgreementId = "SA-001",
    Seller = "Producer Co",
    Purchaser = "Refiner Co",
    EffectiveDate = new DateTime(2023, 1, 1),
    PricingTerms = new PricingTerms
    {
        PricingMethod = PricingMethod.IndexBased,
        PriceIndex = "WTI",
        Differential = 2.50m
    }
};

manager.RegisterSalesAgreement(salesAgreement);
```

### Example 4: Integration with LifeCycle Service

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

---

## Integration Patterns

### Adding Production Accounting to LifeCycle Services

1. **Add Dependency**
   ```csharp
   using Beep.OilandGas.ProductionAccounting.Models;
   using Beep.OilandGas.ProductionAccounting.Calculations;
   using Beep.OilandGas.ProductionAccounting.Management;
   ```

2. **Use Accounting Service**
   ```csharp
   public class PPDMProductionService
   {
       private readonly IAccountingService _accountingService;
       
       public async Task<RoyaltyCalculationResult> CalculateRoyaltiesForFieldAsync(
           string fieldId)
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

---

## Data Storage

### PPDM Tables

#### Existing PPDM Tables (Use These)

**Status:** ✅ **Existing PPDM Tables** - These tables already exist in PPDM39.

1. **`PDEN`** - Production entities
   - Stores production entity data
   - Links to `WELL`, `POOL`, `FIELD`
   - Use for: Production entity records

2. **`PDEN_VOL_SUMMARY`** - Production volume summaries
   - Stores production volume summary data
   - Links to `PDEN`
   - Use for: Production volume summaries

3. **`OBLIGATION`** - Obligations and payments
   - Stores obligation records
   - Links to properties, leases, etc.
   - Use for: Royalty obligations

4. **`OBLIG_PAYMENT`** - Payment details
   - Stores payment transaction details
   - Links to `OBLIGATION`
   - Use for: Royalty payment records

5. **`CONTRACT`** - Contracts
   - Stores contract data
   - Use for: Sales agreements, lease agreements

#### New Tables Needed (To Be Created)

**Status:** ⚠️ **New Tables Needed** - These tables do not exist in PPDM39 and must be created following PPDM patterns.

1. **`PRODUCTION_ALLOCATION`** - Production allocation records
   ```sql
   CREATE TABLE PRODUCTION_ALLOCATION (
       PRODUCTION_ALLOCATION_ID VARCHAR(50) PRIMARY KEY,
       PDEN_ID VARCHAR(50) NULL,
       FIELD_ID VARCHAR(50) NULL,
       ALLOCATION_DATE DATETIME,
       TOTAL_PRODUCTION DECIMAL(18,2),  -- barrels
       ALLOCATION_METHOD VARCHAR(50),    -- 'WORKING_INTEREST', 'NET_REVENUE_INTEREST', etc.
       ALLOCATION_RESULTS_JSON NVARCHAR(MAX),  -- JSON serialized allocation results
       -- Standard PPDM columns
       ROW_ID VARCHAR(50),
       ROW_CHANGED_BY VARCHAR(50),
       ROW_CHANGED_DATE DATETIME,
       ROW_CREATED_BY VARCHAR(50),
       ROW_CREATED_DATE DATETIME
   );
   ```

2. **`ROYALTY_CALCULATION`** - Royalty calculation records
   ```sql
   CREATE TABLE ROYALTY_CALCULATION (
       ROYALTY_CALCULATION_ID VARCHAR(50) PRIMARY KEY,
       OBLIGATION_ID VARCHAR(50) NULL,
       FIELD_ID VARCHAR(50) NULL,
       WELL_ID VARCHAR(50) NULL,
       CALCULATION_DATE DATETIME,
       PRODUCTION_VOLUME DECIMAL(18,2),
       ROYALTY_RATE DECIMAL(10,4),
       ROYALTY_AMOUNT DECIMAL(18,2),
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
- `PDEN` links to `WELL`, `POOL`, `FIELD` via foreign keys
- `PDEN_VOL_SUMMARY.PDEN_ID` → `PDEN.PDEN_ID`
- `OBLIGATION` links to properties and leases
- `OBLIG_PAYMENT.OBLIGATION_ID` → `OBLIGATION.OBLIGATION_ID`

**New Tables:**
- `PRODUCTION_ALLOCATION.PDEN_ID` → `PDEN.PDEN_ID`
- `PRODUCTION_ALLOCATION.FIELD_ID` → `FIELD.FIELD_ID`
- `ROYALTY_CALCULATION.OBLIGATION_ID` → `OBLIGATION.OBLIGATION_ID`
- `ROYALTY_CALCULATION.FIELD_ID` → `FIELD.FIELD_ID`
- `ROYALTY_CALCULATION.WELL_ID` → `WELL.WELL_ID`

---

## Best Practices

1. **Lease Management**
   - Maintain accurate lease records
   - Track lease terms and expiration dates
   - Update working interest and royalty rates

2. **Production Allocation**
   - Use appropriate allocation methods
   - Maintain allocation history
   - Validate allocation results

3. **Royalty Calculations**
   - Use correct royalty rates
   - Account for different royalty types
   - Maintain royalty payment records

---

## Future Enhancements

### Planned Integrations

1. **Production Service Integration**
   - Automatic production allocation
   - Real-time royalty calculations
   - Production reporting

2. **FieldOrchestrator Integration**
   - Field-level production accounting
   - Portfolio accounting
   - Comparative analysis

---

## References

- **Project Location:** `Beep.OilandGas.ProductionAccounting`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Accounting.PPDMAccountingService`
- **Documentation:** `Beep.OilandGas.ProductionAccounting/README.md`
- **PPDM Tables:** `PDEN`, `PDEN_VOL_SUMMARY`, `OBLIGATION`

---

**Last Updated:** 2024  
**Status:** ✅ Partially Integrated

