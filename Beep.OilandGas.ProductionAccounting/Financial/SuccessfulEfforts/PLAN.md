# Successful Efforts Accounting Module Enhancement Plan

## Current State Analysis

### Existing Files
- `SuccessfulEffortsAccounting.cs` - Class with IDataSource integration

### Issues Identified
1. **DTO Conversions**: Uses ConvertDtoToUnprovedPropertyEntity methods (should use entities directly)
2. **Models in Wrong Location**: PropertyModels.cs, CostModels.cs in ProductionAccounting.Models (should be in Beep.OilandGas.Models)
3. **No Service Interface**: Missing ISuccessfulEffortsService interface
4. **Missing Workflows**: No impairment testing workflow, no property reclassification workflow, no cost rollup reports

## Entity/DTO Migration

### Classes to Move to Beep.OilandGas.Models

**Move to `Beep.OilandGas.Models/Data/Financial/`:**
- `UnprovedProperty` → `UNPROVED_PROPERTY` (entity class with PPDM audit columns)
- `ProvedProperty` → `PROVED_PROPERTY` (entity class)
- `ExplorationCosts` → `EXPLORATION_COSTS` (entity class)
- `DevelopmentCosts` → `DEVELOPMENT_COSTS` (entity class)
- `ProductionCosts` → `PRODUCTION_COSTS` (entity class)

**Keep DTOs in `Beep.OilandGas.Models/DTOs/ProductionAccounting/`:**
- `UnprovedPropertyDto` (already exists)
- `ExplorationCostsDto` (already exists)
- `DevelopmentCostsDto` (already exists)
- `ProductionCostsDto` (already exists)

## Service Class Creation

### New Service: SuccessfulEffortsService

**Location**: `Beep.OilandGas.ProductionAccounting/Financial/SuccessfulEfforts/SuccessfulEffortsService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/ISuccessfulEffortsService.cs`

```csharp
public interface ISuccessfulEffortsService
{
    Task<UNPROVED_PROPERTY> RecordAcquisitionAsync(UnprovedPropertyDto property, string userId, string? connectionName = null);
    Task<EXPLORATION_COSTS> RecordExplorationCostsAsync(ExplorationCostsDto costs, string userId, string? connectionName = null);
    Task<DEVELOPMENT_COSTS> RecordDevelopmentCostsAsync(DevelopmentCostsDto costs, string userId, string? connectionName = null);
    Task<PRODUCTION_COSTS> RecordProductionCostsAsync(ProductionCostsDto costs, string userId, string? connectionName = null);
    Task<PROVED_PROPERTY> ReclassifyToProvedPropertyAsync(string propertyId, ProvedReservesDto reserves, string userId, string? connectionName = null);
    Task<ImpairmentResult> TestImpairmentAsync(string propertyId, string userId, string? connectionName = null);
    Task<List<UNPROVED_PROPERTY>> GetUnprovedPropertiesAsync(string? connectionName = null);
    Task<List<PROVED_PROPERTY>> GetProvedPropertiesAsync(string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Removes all DTO to Entity conversion methods
- Uses entities directly

## Database Integration

### Tables Required

**UNPROVED_PROPERTY**:
- UNPROVED_PROPERTY_ID (PK)
- PROPERTY_ID (FK to PROPERTY)
- PROPERTY_NAME
- ACQUISITION_COST
- ACQUISITION_DATE
- PROPERTY_TYPE
- WORKING_INTEREST
- NET_REVENUE_INTEREST
- ACCUMULATED_IMPAIRMENT
- Standard PPDM audit columns

**PROVED_PROPERTY**:
- PROVED_PROPERTY_ID (PK)
- PROPERTY_ID (FK to PROPERTY)
- PROVED_RESERVES_OIL
- PROVED_RESERVES_GAS
- PROVED_RESERVES_BOE
- Standard PPDM audit columns

**EXPLORATION_COSTS**, **DEVELOPMENT_COSTS**, **PRODUCTION_COSTS**:
- Similar structure with cost details
- Links to PROPERTY, WELL
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("UNPROVED_PROPERTY");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Financial.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "UNPROVED_PROPERTY");
```

## Missing Workflows

### 1. Impairment Testing
- Annual impairment testing for unproved properties
- Calculate impairment based on time/activity
- Record impairment charges
- Track impairment history

### 2. Property Reclassification
- Reclassify unproved to proved when reserves discovered
- Transfer costs from unproved to proved
- Maintain reclassification history

### 3. Cost Rollup and Reporting
- Rollup costs by property/well/field
- Generate cost reports by category
- Track capitalized vs expensed costs
- Cost allocation reports

### 4. Amortization Integration
- Link to amortization calculations
- Track amortization basis
- Generate amortization reports

### 5. Cost Reconciliation
- Reconcile costs vs AFE
- Reconcile costs vs budget
- Identify cost variances

## Database Scripts

### Scripts to Create

**For UNPROVED_PROPERTY, PROVED_PROPERTY, EXPLORATION_COSTS, DEVELOPMENT_COSTS, PRODUCTION_COSTS**:
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql` (FKs to PROPERTY, WELL)

## Implementation Steps

### Step 1: Create Entity Classes
1. Create entity classes in `Beep.OilandGas.Models/Data/Financial/`
2. Add standard PPDM audit columns
3. Map all properties from existing models

### Step 2: Create Service Interface
1. Create `ISuccessfulEffortsService` interface
2. Define all service methods

### Step 3: Refactor SuccessfulEffortsAccounting to SuccessfulEffortsService
1. Rename class to SuccessfulEffortsService
2. Implement ISuccessfulEffortsService
3. Remove DTO conversion methods
4. Use entities directly with PPDMGenericRepository
5. Add missing workflow methods

### Step 4: Create Database Scripts
1. Generate TAB/PK/FK scripts for all tables (6 database types)
2. Place scripts in appropriate directories

### Step 5: Implement Missing Workflows
1. Implement impairment testing
2. Implement property reclassification
3. Implement cost rollup and reporting
4. Implement amortization integration
5. Implement cost reconciliation

## Testing Requirements

1. Test property acquisition recording
2. Test exploration cost recording (G&G expensed, drilling capitalized)
3. Test development cost recording
4. Test production cost recording
5. Test impairment testing
6. Test property reclassification

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Amortization module (for amortization calculations)

