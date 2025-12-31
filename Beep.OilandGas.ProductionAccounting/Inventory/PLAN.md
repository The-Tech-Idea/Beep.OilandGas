# Inventory Module Enhancement Plan

## Current State Analysis

### Existing Files
- `InventoryTransactionManager.cs` - Manager class with IDataSource integration, uses INVENTORY_TRANSACTION entity directly ✅
- `CrudeOilInventory.cs` - Inventory manager with IDataSource integration

### Issues Identified
1. **No Service Interface**: Missing IInventoryService interface
2. **Missing Workflows**: No inventory valuation workflow, no inventory adjustment workflow, no inventory reconciliation

## Entity/DTO Migration

### Classes Status

**Already in Beep.OilandGas.Models:**
- `INVENTORY_ITEM` (entity class) ✅
- `INVENTORY_TRANSACTION` (entity class) ✅
- `INVENTORY_ADJUSTMENT` (entity class) ✅
- `INVENTORY_VALUATION` (entity class) ✅

**DTOs Status:**
- DTOs may need to be created in `Beep.OilandGas.Models/DTOs/Inventory/`

**No Major Migration Needed** - Entities already in correct location

## Service Class Creation

### New Service: InventoryService

**Location**: `Beep.OilandGas.ProductionAccounting/Inventory/InventoryService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IInventoryService.cs`

```csharp
public interface IInventoryService
{
    Task<INVENTORY_ITEM> CreateItemAsync(CreateInventoryItemRequest request, string userId, string? connectionName = null);
    Task<INVENTORY_ITEM?> GetItemAsync(string itemId, string? connectionName = null);
    Task<List<INVENTORY_ITEM>> GetItemsAsync(string? connectionName = null);
    
    Task<INVENTORY_TRANSACTION> CreateTransactionAsync(
        CreateInventoryTransactionRequest request,
        string userId,
        string? connectionName = null);
    
    Task<List<INVENTORY_TRANSACTION>> GetTransactionsByItemAsync(string itemId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    
    Task<INVENTORY_ADJUSTMENT> CreateAdjustmentAsync(CreateInventoryAdjustmentRequest request, string userId, string? connectionName = null);
    
    Task<INVENTORY_VALUATION> CalculateValuationAsync(
        string itemId,
        ValuationMethod method,
        DateTime valuationDate,
        string userId,
        string? connectionName = null);
    
    // Missing workflows
    Task<InventoryReconciliationResult> ReconcileInventoryAsync(string itemId, DateTime reconciliationDate, string userId, string? connectionName = null);
    Task<InventorySummary> GetInventorySummaryAsync(string? itemId, string? connectionName = null);
    Task<List<InventoryItem>> GetItemsRequiringReconciliationAsync(string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Uses entities directly (already doing this ✅)

## Database Integration

### Tables Status

**Already Exist:**
- `INVENTORY_ITEM` ✅
- `INVENTORY_TRANSACTION` ✅
- `INVENTORY_ADJUSTMENT` ✅
- `INVENTORY_VALUATION` ✅

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("INVENTORY_ITEM");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Accounting.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "INVENTORY_ITEM");
```

## Missing Workflows

### 1. Inventory Valuation Workflow
- Calculate inventory valuation (FIFO, LIFO, Weighted Average, LCM)
- Track valuation history
- Valuation reporting
- Valuation adjustments

### 2. Inventory Adjustment Workflow
- Record inventory adjustments
- Track adjustment reasons
- Adjustment approval workflow
- Adjustment history

### 3. Inventory Reconciliation
- Reconcile book inventory vs physical inventory
- Identify inventory discrepancies
- Generate reconciliation reports
- Track reconciliation history

### 4. Inventory Reporting
- Inventory summary reports
- Inventory detail reports
- Inventory valuation reports
- Inventory movement reports

### 5. Inventory Integration with Production
- Link inventory to production
- Track inventory movements from production
- Production vs inventory reconciliation

## Database Scripts

### Scripts Status

**Already Exist:**
- INVENTORY_ITEM scripts ✅

## Implementation Steps

### Step 1: Create Service Interface
1. Create `IInventoryService` interface
2. Define all service methods

### Step 2: Refactor InventoryTransactionManager to InventoryService
1. Rename InventoryTransactionManager.cs to InventoryService.cs
2. Update to implement IInventoryService
3. Use PPDMGenericRepository (already using entities directly ✅)
4. Add missing workflow methods

### Step 3: Implement Missing Workflows
1. Implement inventory valuation workflow
2. Implement inventory adjustment workflow
3. Implement inventory reconciliation
4. Enhance inventory reporting
5. Implement inventory integration with production

## Testing Requirements

1. Test inventory item creation and retrieval
2. Test inventory transaction creation
3. Test inventory valuation calculation
4. Test inventory reconciliation
5. Test inventory reporting

## Dependencies

- Beep.OilandGas.Models (for entity classes) ✅
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Production module (for production integration)

