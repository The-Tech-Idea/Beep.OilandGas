# Allocation Module Enhancement Plan

## Current State Analysis

### Existing Files
- `AllocationEngine.cs` - Static class with allocation calculation methods
- `AllocationModels.cs` - Contains AllocationResult, AllocationDetail, WellAllocationData, LeaseAllocationData, TractAllocationData
- `AdvancedAllocationMethods.cs` - Additional allocation methods

### Issues Identified
1. **No Database Integration**: AllocationEngine is static and doesn't save allocation results to database
2. **Models in Wrong Location**: AllocationModels.cs classes should be in Beep.OilandGas.Models
3. **No Service Class**: Missing service class with IDataSource and connectionName parameter
4. **No Persistence**: Allocation results are calculated but not saved
5. **Missing Workflows**: No allocation history, no allocation audit trail, no allocation reconciliation

## Entity/DTO Migration

### Classes to Move to Beep.OilandGas.Models

**Move to `Beep.OilandGas.Models/Data/Allocation/`:**
- `AllocationResult` → `ALLOCATION_RESULT` (entity class with PPDM audit columns)
- `AllocationDetail` → `ALLOCATION_DETAIL` (entity class)
- `WellAllocationData` → Keep as DTO in `Beep.OilandGas.Models/DTOs/Allocation/`
- `LeaseAllocationData` → Keep as DTO in `Beep.OilandGas.Models/DTOs/Allocation/`
- `TractAllocationData` → Keep as DTO in `Beep.OilandGas.Models/DTOs/Allocation/`

**Keep in ProductionAccounting:**
- `AllocationMethod` enum (business logic)
- `AllocationEngine` static methods (calculation logic)

## Service Class Creation

### New Service: AllocationService

**Location**: `Beep.OilandGas.ProductionAccounting/Allocation/AllocationService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IAllocationService.cs`

```csharp
public interface IAllocationService
{
    Task<ALLOCATION_RESULT> AllocateProductionAsync(
        string runTicketId,
        AllocationMethod method,
        List<WellAllocationData> wells,
        string userId,
        string? connectionName = null);
    
    Task<ALLOCATION_RESULT> AllocateToLeasesAsync(
        string runTicketId,
        AllocationMethod method,
        List<LeaseAllocationData> leases,
        string userId,
        string? connectionName = null);
    
    Task<ALLOCATION_RESULT> AllocateToTractsAsync(
        string runTicketId,
        AllocationMethod method,
        List<TractAllocationData> tracts,
        string userId,
        string? connectionName = null);
    
    Task<ALLOCATION_RESULT?> GetAllocationResultAsync(string allocationId, string? connectionName = null);
    Task<List<ALLOCATION_RESULT>> GetAllocationHistoryAsync(string runTicketId, string? connectionName = null);
    Task<List<ALLOCATION_DETAIL>> GetAllocationDetailsAsync(string allocationId, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for ALLOCATION_RESULT and ALLOCATION_DETAIL tables
- Calls AllocationEngine static methods for calculations
- Saves results to database

## Database Integration

### New Tables Required

**ALLOCATION_RESULT**:
- ALLOCATION_RESULT_ID (PK)
- RUN_TICKET_ID (FK to RUN_TICKET)
- ALLOCATION_DATE
- ALLOCATION_METHOD
- TOTAL_VOLUME
- ALLOCATED_VOLUME
- ALLOCATION_VARIANCE
- Standard PPDM audit columns

**ALLOCATION_DETAIL**:
- ALLOCATION_DETAIL_ID (PK)
- ALLOCATION_RESULT_ID (FK)
- ENTITY_ID (well, lease, tract)
- ENTITY_TYPE
- ALLOCATED_VOLUME
- ALLOCATION_PERCENTAGE
- ALLOCATION_BASIS
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("ALLOCATION_RESULT");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Allocation.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "ALLOCATION_RESULT");
```

## Missing Workflows

### 1. Allocation History Tracking
- Track all allocation runs for a run ticket
- Compare different allocation methods
- View allocation changes over time

### 2. Allocation Reconciliation
- Reconcile allocated volumes vs actual production
- Identify allocation discrepancies
- Generate reconciliation reports

### 3. Allocation Audit Trail
- Track who performed allocations
- Track allocation method changes
- Track allocation adjustments

### 4. Allocation Approval Workflow
- Require approval for allocation changes
- Track approval status
- Maintain approval history

### 5. Allocation Reporting
- Allocation summary reports
- Allocation detail reports
- Allocation variance reports
- Allocation by well/lease/tract reports

### 6. Commingled Production Allocation
- Allocate commingled production from multiple wells
- Support complex allocation rules
- Handle allocation factors

### 7. Retroactive Allocation
- Apply allocation changes retroactively
- Maintain allocation history
- Track retroactive adjustments

## Database Scripts

### Scripts to Create

**For ALLOCATION_RESULT**:
- `ALLOCATION_RESULT_TAB.sql` (6 database types)
- `ALLOCATION_RESULT_PK.sql`
- `ALLOCATION_RESULT_FK.sql` (FK to RUN_TICKET)

**For ALLOCATION_DETAIL**:
- `ALLOCATION_DETAIL_TAB.sql` (6 database types)
- `ALLOCATION_DETAIL_PK.sql`
- `ALLOCATION_DETAIL_FK.sql` (FK to ALLOCATION_RESULT)

## Implementation Steps

### Step 1: Create Entity Classes
1. Create `ALLOCATION_RESULT` entity in `Beep.OilandGas.Models/Data/Allocation/`
2. Create `ALLOCATION_DETAIL` entity in `Beep.OilandGas.Models/Data/Allocation/`
3. Add standard PPDM audit columns

### Step 2: Create DTOs
1. Move `WellAllocationData`, `LeaseAllocationData`, `TractAllocationData` to `Beep.OilandGas.Models/DTOs/Allocation/`
2. Create request/response DTOs for allocation operations

### Step 3: Create Service Interface
1. Create `IAllocationService` interface in `Beep.OilandGas.PPDM39/Core/Interfaces/`
2. Define all service methods

### Step 4: Implement Service Class
1. Create `AllocationService.cs` in `Beep.OilandGas.ProductionAccounting/Allocation/`
2. Implement constructor with IDataSource dependencies
3. Implement all interface methods using PPDMGenericRepository
4. Call AllocationEngine static methods for calculations
5. Save results to database

### Step 5: Create Database Scripts
1. Generate TAB/PK/FK scripts for ALLOCATION_RESULT (6 database types)
2. Generate TAB/PK/FK scripts for ALLOCATION_DETAIL (6 database types)
3. Place scripts in appropriate directories

### Step 6: Add Missing Workflows
1. Implement allocation history tracking
2. Implement allocation reconciliation
3. Implement allocation audit trail
4. Implement allocation approval workflow
5. Implement allocation reporting

### Step 7: Update Existing Code
1. Update AllocationEngine to work with service class
2. Remove Dictionary conversions
3. Update ProductionManager to use AllocationService
4. Update any other code that uses AllocationEngine

## Testing Requirements

1. Test allocation calculations (existing functionality)
2. Test database persistence of allocation results
3. Test allocation history retrieval
4. Test allocation reconciliation
5. Test allocation approval workflow
6. Test allocation reporting

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Production module (for RunTicket)
- Ownership module (for ownership interests)

## References

- See `data-access-patterns.md` for PPDMGenericRepository usage
- See `ppdm-integration-patterns.md` for PPDM table patterns
- See `database-script-generation.md` for script patterns

