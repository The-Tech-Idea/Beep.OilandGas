# Production Module Enhancement Plan

## Current State Analysis

### Existing Files
- `ProductionManager.cs` - Manager class with IDataSource integration
- `RunTicket.cs` - RunTicket model class

### Issues Identified
1. **Dictionary Conversions**: ProductionManager uses ConvertRunTicketToDictionary/ConvertDictionaryToRunTicket (should use entities directly)
2. **RunTicket in Wrong Location**: RunTicket should be entity class in Beep.OilandGas.Models
3. **Missing Service Class**: ProductionManager exists but should be refactored to ProductionService with proper interface
4. **Missing Workflows**: No production forecasting, no production optimization, no production reconciliation
5. **Missing Tables**: RUN_TICKET and TANK_INVENTORY tables may not exist in PPDM

## Entity/DTO Migration

### Classes to Move to Beep.OilandGas.Models

**Move to `Beep.OilandGas.Models/Data/Production/`:**
- `RunTicket` → `RUN_TICKET` (entity class with PPDM audit columns)
- `TankInventory` (if exists) → `TANK_INVENTORY` (entity class)

**Create DTOs in `Beep.OilandGas.Models/DTOs/Production/`:**
- `CreateRunTicketRequest`
- `RunTicketResponse`
- `CreateTankInventoryRequest`
- `TankInventoryResponse`

**Keep in ProductionAccounting:**
- `DispositionType` enum (business logic)
- Production calculation methods (business logic)

## Service Class Creation

### New Service: ProductionService

**Location**: `Beep.OilandGas.ProductionAccounting/Production/ProductionService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IProductionService.cs`

```csharp
public interface IProductionService
{
    Task<RUN_TICKET> CreateRunTicketAsync(
        CreateRunTicketRequest request,
        string userId,
        string? connectionName = null);
    
    Task<RUN_TICKET?> GetRunTicketAsync(string runTicketNumber, string? connectionName = null);
    Task<List<RUN_TICKET>> GetRunTicketsByLeaseAsync(string leaseId, string? connectionName = null);
    Task<List<RUN_TICKET>> GetRunTicketsByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null);
    
    Task<TANK_INVENTORY> CreateTankInventoryAsync(
        CreateTankInventoryRequest request,
        string userId,
        string? connectionName = null);
    
    Task<TANK_INVENTORY?> GetTankInventoryAsync(string inventoryId, string? connectionName = null);
    
    Task<decimal> CalculateTotalProductionAsync(string leaseId, DateTime startDate, DateTime endDate, string? connectionName = null);
    Task<Dictionary<DispositionType, decimal>> CalculateDispositionsByTypeAsync(DateTime startDate, DateTime endDate, string? connectionName = null);
    
    // Missing workflows
    Task<ProductionForecast> CreateProductionForecastAsync(CreateProductionForecastRequest request, string userId, string? connectionName = null);
    Task<ProductionOptimizationResult> OptimizeProductionAsync(ProductionOptimizationRequest request, string userId, string? connectionName = null);
    Task<ProductionReconciliationResult> ReconcileProductionAsync(ProductionReconciliationRequest request, string userId, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for RUN_TICKET and TANK_INVENTORY tables
- Removes all Dictionary conversions
- Uses entities directly

## Database Integration

### Tables Required

**RUN_TICKET** (may need to create if not in PPDM):
- RUN_TICKET_ID (PK)
- RUN_TICKET_NUMBER
- TICKET_DATE_TIME
- LEASE_ID (FK to LAND_RIGHT or PROPERTY)
- WELL_ID (FK to WELL)
- TANK_BATTERY_ID (FK to EQUIPMENT or FACILITY)
- GROSS_VOLUME
- BSW_VOLUME
- BSW_PERCENTAGE
- NET_VOLUME
- TEMPERATURE
- API_GRAVITY
- DISPOSITION_TYPE
- PURCHASER
- MEASUREMENT_METHOD
- MEASUREMENT_RECORD_ID
- Standard PPDM audit columns

**TANK_INVENTORY** (may need to create if not in PPDM):
- TANK_INVENTORY_ID (PK)
- INVENTORY_DATE
- TANK_BATTERY_ID (FK)
- OPENING_INVENTORY
- RECEIPTS
- DELIVERIES
- ADJUSTMENTS
- SHRINKAGE
- THEFT_LOSS
- ACTUAL_CLOSING_INVENTORY
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("RUN_TICKET");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Production.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "RUN_TICKET");
```

## Missing Workflows

### 1. Production Forecasting
- Decline curve analysis (DCA)
- Production forecasting by well/lease/field
- Forecast vs actual comparison
- Forecast accuracy tracking

### 2. Production Optimization
- Identify underperforming wells
- Recommend optimization actions
- Track optimization results
- Calculate optimization ROI

### 3. Production Reconciliation
- Reconcile run tickets vs sales
- Reconcile production vs allocations
- Identify production discrepancies
- Generate reconciliation reports

### 4. Production Reporting
- Daily production reports
- Monthly production summaries
- Production by well/lease/field
- Production trends and analysis

### 5. Production Volume Validation
- Validate production volumes against measurements
- Flag unusual production patterns
- Validate BSW percentages
- Validate temperature and API gravity

### 6. Production History Analysis
- Historical production trends
- Production decline analysis
- Production comparison reports
- Production forecasting based on history

### 7. Production Allocation Integration
- Link run tickets to allocations
- Track allocated vs actual production
- Generate allocation vs production reports

## Database Scripts

### Scripts to Create

**For RUN_TICKET**:
- `RUN_TICKET_TAB.sql` (6 database types)
- `RUN_TICKET_PK.sql`
- `RUN_TICKET_FK.sql` (FKs to LEASE, WELL, TANK_BATTERY)

**For TANK_INVENTORY**:
- `TANK_INVENTORY_TAB.sql` (6 database types)
- `TANK_INVENTORY_PK.sql`
- `TANK_INVENTORY_FK.sql` (FK to TANK_BATTERY)

**For Production Forecasting** (if new tables needed):
- `PRODUCTION_FORECAST_TAB.sql`
- `PRODUCTION_FORECAST_PK.sql`
- `PRODUCTION_FORECAST_FK.sql`

## Implementation Steps

### Step 1: Create Entity Classes
1. Create `RUN_TICKET` entity in `Beep.OilandGas.Models/Data/Production/`
2. Create `TANK_INVENTORY` entity in `Beep.OilandGas.Models/Data/Production/`
3. Add standard PPDM audit columns
4. Map all RunTicket properties to RUN_TICKET entity

### Step 2: Create DTOs
1. Create `CreateRunTicketRequest` in `Beep.OilandGas.Models/DTOs/Production/`
2. Create `RunTicketResponse` in `Beep.OilandGas.Models/DTOs/Production/`
3. Create `CreateTankInventoryRequest` and `TankInventoryResponse`
4. Create DTOs for missing workflows (forecasting, optimization, reconciliation)

### Step 3: Create Service Interface
1. Create `IProductionService` interface in `Beep.OilandGas.PPDM39/Core/Interfaces/`
2. Define all service methods including missing workflows

### Step 4: Refactor ProductionManager to ProductionService
1. Rename ProductionManager.cs to ProductionService.cs
2. Update to implement IProductionService
3. Remove all Dictionary conversion methods
4. Use entities directly with PPDMGenericRepository
5. Add missing workflow methods

### Step 5: Create Database Scripts
1. Generate TAB/PK/FK scripts for RUN_TICKET (6 database types)
2. Generate TAB/PK/FK scripts for TANK_INVENTORY (6 database types)
3. Generate scripts for production forecasting tables if needed
4. Place scripts in appropriate directories

### Step 6: Implement Missing Workflows
1. Implement production forecasting (integrate with DCA services)
2. Implement production optimization
3. Implement production reconciliation
4. Implement production reporting
5. Implement production volume validation
6. Implement production history analysis

### Step 7: Update Existing Code
1. Remove Dictionary conversions
2. Update all code that uses ProductionManager to use ProductionService
3. Update AllocationService to use ProductionService
4. Update PricingService to use ProductionService

## Testing Requirements

1. Test run ticket creation and retrieval
2. Test tank inventory creation and retrieval
3. Test production calculations
4. Test production forecasting
5. Test production optimization
6. Test production reconciliation
7. Test production reporting

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Measurement module (for measurement records)
- Storage module (for tank batteries)
- Allocation module (for production allocation)

## References

- See `data-access-patterns.md` for PPDMGenericRepository usage
- See `ppdm-integration-patterns.md` for PPDM table patterns
- See `database-script-generation.md` for script patterns
- See `production-accounting-workflows.md` for workflow patterns

