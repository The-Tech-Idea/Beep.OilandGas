# Invoice Module Enhancement Plan

## Current State Analysis

### Existing Files
- `InvoiceManager.cs` - Manager class with IDataSource integration, uses INVOICE entity directly ✅

### Issues Identified
1. **No Service Interface**: Missing IInvoiceService interface
2. **Missing Workflows**: No invoice approval workflow, no invoice payment tracking, no invoice aging reports

## Entity/DTO Migration

### Classes Status

**Already in Beep.OilandGas.Models:**
- `INVOICE` (entity class) ✅
- `INVOICE_LINE_ITEM` (entity class) ✅
- `INVOICE_PAYMENT` (entity class) ✅

**DTOs Status:**
- DTOs already exist in `Beep.OilandGas.Models/DTOs/Accounting/` ✅

**No Migration Needed** - Entities already in correct location

## Service Class Creation

### New Service: InvoiceService

**Location**: `Beep.OilandGas.ProductionAccounting/Invoice/InvoiceService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IInvoiceService.cs`

```csharp
public interface IInvoiceService
{
    Task<INVOICE> CreateInvoiceAsync(CreateInvoiceRequest request, string userId, string? connectionName = null);
    Task<INVOICE?> GetInvoiceAsync(string invoiceId, string? connectionName = null);
    Task<List<INVOICE>> GetInvoicesByCustomerAsync(string customerId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    Task<INVOICE> UpdateInvoiceAsync(UpdateInvoiceRequest request, string userId, string? connectionName = null);
    Task<bool> DeleteInvoiceAsync(string invoiceId, string userId, string? connectionName = null);
    
    Task<INVOICE_PAYMENT> RecordPaymentAsync(CreateInvoicePaymentRequest request, string userId, string? connectionName = null);
    Task<List<INVOICE_PAYMENT>> GetInvoicePaymentsAsync(string invoiceId, string? connectionName = null);
    
    // Missing workflows
    Task<InvoiceApprovalResult> ApproveInvoiceAsync(string invoiceId, string approverId, string? connectionName = null);
    Task<List<InvoiceAgingSummary>> GetInvoiceAgingAsync(string? customerId, string? connectionName = null);
    Task<InvoicePaymentStatus> GetInvoicePaymentStatusAsync(string invoiceId, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Uses entities directly (already doing this ✅)

## Database Integration

### Tables Status

**Already Exist:**
- `INVOICE` ✅
- `INVOICE_LINE_ITEM` ✅
- `INVOICE_PAYMENT` ✅

**May Need Additional Tables:**
- `INVOICE_APPROVAL` (for approval workflow)

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("INVOICE");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Accounting.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "INVOICE");
```

## Missing Workflows

### 1. Invoice Approval Workflow
- Require approval for invoices above threshold
- Track approval status
- Maintain approval history
- Approval notifications

### 2. Invoice Payment Tracking
- Track invoice payment status
- Calculate outstanding balances
- Payment application
- Payment history

### 3. Invoice Aging Reports
- Calculate invoice aging
- Identify overdue invoices
- Aging by customer
- Aging analysis reports

### 4. Invoice Integration with Revenue
- Link invoices to revenue transactions
- Auto-create invoices from revenue
- Invoice vs revenue reconciliation

### 5. Invoice Reporting
- Invoice summary reports
- Invoice detail reports
- Invoice by customer reports
- Invoice payment reports

## Database Scripts

### Scripts Status

**Already Exist:**
- INVOICE scripts ✅

**May Need Additional Scripts:**
- `INVOICE_APPROVAL_TAB.sql` (if new table needed)

## Implementation Steps

### Step 1: Create Service Interface
1. Create `IInvoiceService` interface
2. Define all service methods

### Step 2: Refactor InvoiceManager to InvoiceService
1. Rename InvoiceManager.cs to InvoiceService.cs
2. Update to implement IInvoiceService
3. Use PPDMGenericRepository (already using entities directly ✅)
4. Add missing workflow methods

### Step 3: Implement Missing Workflows
1. Implement invoice approval workflow
2. Implement invoice payment tracking enhancements
3. Implement invoice aging reports
4. Implement invoice integration with revenue
5. Enhance invoice reporting

## Testing Requirements

1. Test invoice creation and retrieval
2. Test invoice payment recording
3. Test invoice approval workflow
4. Test invoice aging calculation
5. Test invoice reporting

## Dependencies

- Beep.OilandGas.Models (for entity classes) ✅
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Revenue/Accounting module (for revenue integration)

