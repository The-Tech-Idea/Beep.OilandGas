# Full Cost Accounting Module Enhancement Plan

## Current State Analysis

### Existing Files
- `FullCostAccounting.cs` - Class with IDataSource integration

### Issues Identified
1. **DTO Conversions**: Uses DTO to Entity conversion methods (should use entities directly)
2. **Models in Wrong Location**: CostModels.cs in ProductionAccounting.Models (should be in Beep.OilandGas.Models)
3. **No Service Interface**: Missing IFullCostService interface
4. **Missing Workflows**: No ceiling test workflow, no cost center management, no cost rollup by cost center

## Entity/DTO Migration

### Classes to Move to Beep.OilandGas.Models

**Move to `Beep.OilandGas.Models/Data/Financial/`:**
- Cost models already exist as DTOs, need entity classes:
- `EXPLORATION_COSTS` (entity class)
- `DEVELOPMENT_COSTS` (entity class)
- `COST_CENTER` (entity class)
- `CEILING_TEST_CALCULATION` (entity class)

**Keep DTOs in `Beep.OilandGas.Models/DTOs/ProductionAccounting/`:**
- `ExplorationCostsDto` (already exists)
- `DevelopmentCostsDto` (already exists)

## Service Class Creation

### New Service: FullCostService

**Location**: `Beep.OilandGas.ProductionAccounting/Financial/FullCost/FullCostService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IFullCostService.cs`

```csharp
public interface IFullCostService
{
    Task<EXPLORATION_COSTS> RecordExplorationCostsAsync(
        string propertyId,
        string costCenterId,
        ExplorationCostsDto costs,
        string userId,
        string? connectionName = null);
    
    Task<DEVELOPMENT_COSTS> RecordDevelopmentCostsAsync(
        string propertyId,
        string costCenterId,
        DevelopmentCostsDto costs,
        string userId,
        string? connectionName = null);
    
    Task<COST_CENTER> CreateCostCenterAsync(CreateCostCenterRequest request, string userId, string? connectionName = null);
    Task<List<COST_CENTER>> GetCostCentersAsync(string? connectionName = null);
    
    Task<CEILING_TEST_CALCULATION> PerformCeilingTestAsync(
        string costCenterId,
        DateTime testDate,
        string userId,
        string? connectionName = null);
    
    Task<List<CEILING_TEST_CALCULATION>> GetCeilingTestHistoryAsync(string costCenterId, string? connectionName = null);
    
    // Missing workflows
    Task<CostCenterRollup> GetCostCenterRollupAsync(string costCenterId, DateTime? asOfDate, string? connectionName = null);
    Task<ImpairmentResult> CalculateImpairmentAsync(string costCenterId, DateTime testDate, string userId, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Removes DTO conversion methods
- Uses entities directly

## Database Integration

### Tables Required

**COST_CENTER**:
- COST_CENTER_ID (PK)
- COST_CENTER_NAME
- COST_CENTER_TYPE (Country, Region, Field)
- DESCRIPTION
- Standard PPDM audit columns

**CEILING_TEST_CALCULATION**:
- CEILING_TEST_ID (PK)
- COST_CENTER_ID (FK)
- TEST_DATE
- NET_CAPITALIZED_COST
- DISCOUNTED_FUTURE_NET_CASH_FLOWS
- DISCOUNT_RATE
- IMPAIRMENT_AMOUNT
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("COST_CENTER");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Financial.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "COST_CENTER");
```

## Missing Workflows

### 1. Ceiling Test Workflow
- Annual ceiling test calculation
- Compare capitalized costs vs discounted future cash flows
- Record impairment if ceiling exceeded
- Ceiling test history tracking
- Ceiling test reporting

### 2. Cost Center Management
- Create and manage cost centers
- Organize costs by cost center
- Cost center hierarchy
- Cost center reporting

### 3. Cost Rollup by Cost Center
- Rollup all costs by cost center
- Calculate total capitalized costs
- Cost center balance reports
- Cost center analysis

### 4. Impairment Calculation
- Calculate impairment when ceiling exceeded
- Record impairment charges
- Track impairment history
- Impairment reporting

### 5. Cost Allocation to Cost Centers
- Allocate costs to cost centers
- Track cost allocation
- Cost allocation reports

## Database Scripts

### Scripts to Create

**For COST_CENTER, CEILING_TEST_CALCULATION**:
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql`

## Implementation Steps

### Step 1: Create Entity Classes
1. Create entity classes in `Beep.OilandGas.Models/Data/Financial/`
2. Add standard PPDM audit columns

### Step 2: Create Service Interface
1. Create `IFullCostService` interface
2. Define all service methods

### Step 3: Refactor FullCostAccounting to FullCostService
1. Rename class to FullCostService
2. Implement IFullCostService
3. Remove DTO conversion methods
4. Use entities directly with PPDMGenericRepository
5. Add missing workflow methods

### Step 4: Create Database Scripts
1. Generate TAB/PK/FK scripts for all tables (6 database types)
2. Place scripts in appropriate directories

### Step 5: Implement Missing Workflows
1. Implement ceiling test workflow
2. Implement cost center management
3. Implement cost rollup by cost center
4. Implement impairment calculation
5. Implement cost allocation to cost centers

## Testing Requirements

1. Test exploration cost recording
2. Test development cost recording
3. Test cost center creation
4. Test ceiling test calculation
5. Test impairment calculation

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Amortization module (for amortization calculations)

