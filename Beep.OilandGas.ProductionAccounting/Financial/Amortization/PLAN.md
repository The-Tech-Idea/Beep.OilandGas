# Amortization Module Enhancement Plan

## Current State Analysis

### Existing Files
- `AmortizationCalculator.cs` - Static class with calculation methods
- `InterestCapitalizationCalculator.cs` - Static class with calculation methods

### Issues Identified
1. **No Database Integration**: Static classes don't save amortization records
2. **No Service Class**: Missing IAmortizationService interface
3. **Missing Workflows**: No amortization record tracking, no amortization history, no amortization reporting

## Entity/DTO Migration

### Classes to Create in Beep.OilandGas.Models

**Create in `Beep.OilandGas.Models/Data/Financial/`:**
- `AMORTIZATION_RECORD` (entity class with PPDM audit columns)
- `INTEREST_CAPITALIZATION_RECORD` (entity class)

**Create DTOs in `Beep.OilandGas.Models/DTOs/Financial/`:**
- `CreateAmortizationRecordRequest`
- `AmortizationRecordResponse`
- `CreateInterestCapitalizationRequest`
- `InterestCapitalizationResponse`

**Keep in ProductionAccounting:**
- `AmortizationCalculator` static methods (calculation logic)
- `InterestCapitalizationCalculator` static methods (calculation logic)

## Service Class Creation

### New Service: AmortizationService

**Location**: `Beep.OilandGas.ProductionAccounting/Financial/Amortization/AmortizationService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IAmortizationService.cs`

```csharp
public interface IAmortizationService
{
    Task<AMORTIZATION_RECORD> CalculateAndRecordAmortizationAsync(
        CalculateAmortizationRequest request,
        string userId,
        string? connectionName = null);
    
    Task<AMORTIZATION_RECORD?> GetAmortizationRecordAsync(string recordId, string? connectionName = null);
    Task<List<AMORTIZATION_RECORD>> GetAmortizationHistoryAsync(string propertyId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    
    Task<INTEREST_CAPITALIZATION_RECORD> CalculateAndRecordInterestCapitalizationAsync(
        CalculateInterestCapitalizationRequest request,
        string userId,
        string? connectionName = null);
    
    Task<INTEREST_CAPITALIZATION_RECORD?> GetInterestCapitalizationRecordAsync(string recordId, string? connectionName = null);
    
    // Missing workflows
    Task<AmortizationSchedule> GenerateAmortizationScheduleAsync(GenerateScheduleRequest request, string? connectionName = null);
    Task<AmortizationSummary> GetAmortizationSummaryAsync(string propertyId, DateTime? asOfDate, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Calls AmortizationCalculator static methods for calculations
- Saves results to database

## Database Integration

### Tables Required

**AMORTIZATION_RECORD**:
- AMORTIZATION_RECORD_ID (PK)
- PROPERTY_ID (FK to PROPERTY)
- AMORTIZATION_DATE
- NET_CAPITALIZED_COST
- TOTAL_PROVED_RESERVES_BOE
- PRODUCTION_BOE
- AMORTIZATION_RATE
- AMORTIZATION_AMOUNT
- Standard PPDM audit columns

**INTEREST_CAPITALIZATION_RECORD**:
- INTEREST_CAPITALIZATION_ID (PK)
- PROPERTY_ID (FK to PROPERTY)
- CAPITALIZATION_DATE
- BORROWING_COST
- WEIGHTED_AVERAGE_RATE
- CAPITALIZED_INTEREST
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("AMORTIZATION_RECORD");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Financial.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "AMORTIZATION_RECORD");
```

## Missing Workflows

### 1. Amortization Record Tracking
- Track all amortization calculations
- Maintain amortization history
- Amortization audit trail

### 2. Amortization Schedule Generation
- Generate amortization schedules
- Project future amortization
- Amortization schedule reporting

### 3. Amortization Reporting
- Amortization summary reports
- Amortization detail reports
- Amortization by property/well/field
- Amortization trends

### 4. Interest Capitalization Tracking
- Track interest capitalization
- Maintain capitalization history
- Capitalization reporting

### 5. Amortization Reconciliation
- Reconcile amortization vs reserves
- Reconcile amortization vs production
- Identify amortization discrepancies

### 6. Reserve Updates Impact
- Calculate impact of reserve updates on amortization
- Recalculate amortization when reserves change
- Track reserve update impacts

## Database Scripts

### Scripts to Create

**For AMORTIZATION_RECORD, INTEREST_CAPITALIZATION_RECORD**:
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql` (FKs to PROPERTY)

## Implementation Steps

### Step 1: Create Entity Classes
1. Create entity classes in `Beep.OilandGas.Models/Data/Financial/`
2. Add standard PPDM audit columns

### Step 2: Create DTOs
1. Create request/response DTOs in `Beep.OilandGas.Models/DTOs/Financial/`
2. Create DTOs for missing workflows

### Step 3: Create Service Interface
1. Create `IAmortizationService` interface
2. Define all service methods

### Step 4: Create Service Class
1. Create `AmortizationService.cs`
2. Implement IAmortizationService
3. Use PPDMGenericRepository
4. Call AmortizationCalculator static methods
5. Save results to database
6. Add missing workflow methods

### Step 5: Create Database Scripts
1. Generate TAB/PK/FK scripts for all tables (6 database types)
2. Place scripts in appropriate directories

### Step 6: Implement Missing Workflows
1. Implement amortization record tracking
2. Implement amortization schedule generation
3. Implement amortization reporting
4. Implement interest capitalization tracking
5. Implement amortization reconciliation
6. Implement reserve updates impact

## Testing Requirements

1. Test amortization calculation
2. Test amortization record creation
3. Test interest capitalization calculation
4. Test amortization schedule generation
5. Test amortization reporting

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Financial/SuccessfulEfforts (for property costs)
- Financial/FullCost (for cost center costs)

