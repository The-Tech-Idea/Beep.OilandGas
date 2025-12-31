# Purchase Order Module Enhancement Plan

## Current State Analysis

### Existing Files
- `PurchaseOrderManager.cs` - Manager class with IDataSource integration

### Issues Identified
1. **No Service Interface**: Missing IPurchaseOrderService interface
2. **Missing Workflows**: No PO approval workflow, no PO receipt workflow, no vendor management integration

## Entity/DTO Migration

### Classes Status

**Already in Beep.OilandGas.Models:**
- `PURCHASE_ORDER` (entity class) ✅
- `PO_LINE_ITEM` (entity class) ✅
- `PO_RECEIPT` (entity class) ✅

**DTOs Status:**
- DTOs already exist in `Beep.OilandGas.Models/DTOs/Accounting/` ✅

**No Migration Needed** - Entities already in correct location

## Service Class Creation

### New Service: PurchaseOrderService

**Location**: `Beep.OilandGas.ProductionAccounting/PurchaseOrder/PurchaseOrderService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IPurchaseOrderService.cs`

```csharp
public interface IPurchaseOrderService
{
    Task<PURCHASE_ORDER> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request, string userId, string? connectionName = null);
    Task<PURCHASE_ORDER?> GetPurchaseOrderAsync(string poId, string? connectionName = null);
    Task<List<PURCHASE_ORDER>> GetPurchaseOrdersByVendorAsync(string vendorId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    Task<PURCHASE_ORDER> UpdatePurchaseOrderAsync(UpdatePurchaseOrderRequest request, string userId, string? connectionName = null);
    
    Task<PO_RECEIPT> CreateReceiptAsync(CreatePOReceiptRequest request, string userId, string? connectionName = null);
    Task<List<PO_RECEIPT>> GetReceiptsByPOAsync(string poId, string? connectionName = null);
    
    // Missing workflows
    Task<POApprovalResult> ApprovePurchaseOrderAsync(string poId, string approverId, string? connectionName = null);
    Task<POStatusSummary> GetPOStatusAsync(string poId, string? connectionName = null);
    Task<List<PURCHASE_ORDER>> GetPOsRequiringApprovalAsync(string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Uses entities directly
- Uses BUSINESS_ASSOCIATE for vendors (PPDM integration)

## Database Integration

### Tables Status

**Already Exist:**
- `PURCHASE_ORDER` ✅
- `PO_LINE_ITEM` ✅
- `PO_RECEIPT` ✅

**Uses PPDM Tables:**
- `BUSINESS_ASSOCIATE` for vendors ✅

**May Need Additional Tables:**
- `PO_APPROVAL` (for approval workflow)

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("PURCHASE_ORDER");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Accounting.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "PURCHASE_ORDER");
```

## Missing Workflows

### 1. PO Approval Workflow
- Require approval for POs above threshold
- Track approval status
- Maintain approval history
- Approval notifications

### 2. PO Receipt Workflow
- Record PO receipts
- Link receipts to PO line items
- Track receipt status
- Receipt vs PO reconciliation

### 3. PO Integration with AP
- Link POs to AP invoices
- PO vs invoice reconciliation
- Three-way matching (PO, Receipt, Invoice)

### 4. PO Reporting
- PO summary reports
- PO detail reports
- PO by vendor reports
- PO status reports

## Database Scripts

### Scripts Status

**Already Exist:**
- PURCHASE_ORDER scripts ✅

**May Need Additional Scripts:**
- `PO_APPROVAL_TAB.sql` (if new table needed)

## Implementation Steps

### Step 1: Create Service Interface
1. Create `IPurchaseOrderService` interface
2. Define all service methods

### Step 2: Refactor PurchaseOrderManager to PurchaseOrderService
1. Rename PurchaseOrderManager.cs to PurchaseOrderService.cs
2. Update to implement IPurchaseOrderService
3. Use PPDMGenericRepository
4. Add missing workflow methods

### Step 3: Implement Missing Workflows
1. Implement PO approval workflow
2. Implement PO receipt workflow
3. Implement PO integration with AP
4. Enhance PO reporting

## Testing Requirements

1. Test PO creation and retrieval
2. Test PO receipt creation
3. Test PO approval workflow
4. Test PO integration with AP

## Dependencies

- Beep.OilandGas.Models (for entity classes) ✅
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- BUSINESS_ASSOCIATE (PPDM table for vendors) ✅
- Accounts Payable module (for AP integration)

