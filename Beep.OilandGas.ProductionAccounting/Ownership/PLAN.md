# Ownership Module Enhancement Plan

## Current State Analysis

### Existing Files
- `OwnershipManager.cs` - Manager class with IDataSource integration
- `OwnershipModels.cs` - Contains DivisionOrder, TransferOrder, OwnershipInterest models
- `OwnershipTree.cs` - Hierarchical ownership tree

### Issues Identified
1. **Models in Wrong Location**: OwnershipModels.cs classes should be in Beep.OilandGas.Models
2. **No Service Interface**: Missing IOwnershipService interface
3. **Missing Workflows**: No ownership change tracking, no ownership approval workflow, no ownership reporting

## Entity/DTO Migration

### Classes to Move to Beep.OilandGas.Models

**Move to `Beep.OilandGas.Models/Data/Ownership/`:**
- `DivisionOrder` → `DIVISION_ORDER` (entity class with PPDM audit columns)
- `TransferOrder` → `TRANSFER_ORDER` (entity class)
- `OwnershipInterest` → `OWNERSHIP_INTEREST` (entity class)

**Create DTOs in `Beep.OilandGas.Models/DTOs/Ownership/`:**
- `CreateDivisionOrderRequest`
- `DivisionOrderResponse`
- `CreateTransferOrderRequest`
- `TransferOrderResponse`

**Keep in ProductionAccounting:**
- `OwnershipTree` (business logic for hierarchy)

## Service Class Creation

### New Service: OwnershipService

**Location**: `Beep.OilandGas.ProductionAccounting/Ownership/OwnershipService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IOwnershipService.cs`

```csharp
public interface IOwnershipService
{
    Task<DIVISION_ORDER> CreateDivisionOrderAsync(CreateDivisionOrderRequest request, string userId, string? connectionName = null);
    Task<DIVISION_ORDER?> GetDivisionOrderAsync(string orderId, string? connectionName = null);
    Task<List<DIVISION_ORDER>> GetDivisionOrdersByPropertyAsync(string propertyId, string? connectionName = null);
    
    Task<TRANSFER_ORDER> CreateTransferOrderAsync(CreateTransferOrderRequest request, string userId, string? connectionName = null);
    Task<TRANSFER_ORDER?> GetTransferOrderAsync(string orderId, string? connectionName = null);
    
    Task<OWNERSHIP_INTEREST> RegisterOwnershipInterestAsync(CreateOwnershipInterestRequest request, string userId, string? connectionName = null);
    Task<List<OWNERSHIP_INTEREST>> GetOwnershipInterestsByPropertyAsync(string propertyId, string? connectionName = null);
    Task<OwnershipTree> GetOwnershipTreeAsync(string propertyId, string? connectionName = null);
    
    // Missing workflows
    Task<OwnershipChangeResult> RecordOwnershipChangeAsync(OwnershipChangeRequest request, string userId, string? connectionName = null);
    Task<OwnershipApprovalResult> ApproveOwnershipChangeAsync(string changeId, string approverId, string? connectionName = null);
    Task<List<OwnershipChangeHistory>> GetOwnershipChangeHistoryAsync(string propertyId, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Uses entities directly
- Uses BUSINESS_ASSOCIATE for owners (PPDM integration)

## Database Integration

### Tables Required

**DIVISION_ORDER**:
- DIVISION_ORDER_ID (PK)
- PROPERTY_ID (FK to PROPERTY)
- OWNER_BA_ID (FK to BUSINESS_ASSOCIATE)
- WORKING_INTEREST
- NET_REVENUE_INTEREST
- EFFECTIVE_DATE
- Standard PPDM audit columns

**TRANSFER_ORDER**, **OWNERSHIP_INTEREST**:
- Similar structure
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("DIVISION_ORDER");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Ownership.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "DIVISION_ORDER");
```

## Missing Workflows

### 1. Ownership Change Tracking
- Track ownership changes over time
- Effective dating for ownership
- Ownership change history
- Ownership change reporting

### 2. Ownership Approval Workflow
- Require approval for ownership changes
- Track approval status
- Maintain approval history
- Approval notifications

### 3. Ownership Reconciliation
- Reconcile ownership interests
- Verify ownership percentages sum to 100%
- Identify ownership discrepancies
- Ownership reconciliation reports

### 4. Ownership Reporting
- Ownership summary reports
- Ownership detail reports
- Ownership by property reports
- Ownership change reports

### 5. Ownership Integration with Allocation
- Link ownership to allocation
- Use ownership for allocation calculations
- Ownership vs allocation reconciliation

## Database Scripts

### Scripts to Create

**For DIVISION_ORDER, TRANSFER_ORDER, OWNERSHIP_INTEREST**:
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql` (FKs to PROPERTY, BUSINESS_ASSOCIATE)

## Implementation Steps

### Step 1: Create Entity Classes
1. Create entity classes in `Beep.OilandGas.Models/Data/Ownership/`
2. Add standard PPDM audit columns

### Step 2: Create DTOs
1. Create request/response DTOs in `Beep.OilandGas.Models/DTOs/Ownership/`

### Step 3: Create Service Interface
1. Create `IOwnershipService` interface
2. Define all service methods

### Step 4: Refactor OwnershipManager to OwnershipService
1. Rename OwnershipManager.cs to OwnershipService.cs
2. Update to implement IOwnershipService
3. Use PPDMGenericRepository
4. Use entities directly
5. Add missing workflow methods

### Step 5: Create Database Scripts
1. Generate TAB/PK/FK scripts for all tables (6 database types)

### Step 6: Implement Missing Workflows
1. Implement ownership change tracking
2. Implement ownership approval workflow
3. Implement ownership reconciliation
4. Enhance ownership reporting
5. Implement ownership integration with allocation

## Testing Requirements

1. Test division order creation
2. Test transfer order creation
3. Test ownership interest registration
4. Test ownership tree generation
5. Test ownership change tracking

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- PROPERTY (PPDM table) ✅
- BUSINESS_ASSOCIATE (PPDM table for owners) ✅
- Allocation module (for allocation integration)

