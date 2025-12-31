# Accounts Payable Module Enhancement Plan

## Current State Analysis

### Existing Files
- `APManager.cs` - Manager class with IDataSource integration, uses AP_INVOICE entity directly ✅

### Issues Identified
1. **No Service Interface**: Missing IAPService interface
2. **Missing Workflows**: No AP invoice approval workflow, no payment processing workflow, no vendor management, no AP aging reports

## Entity/DTO Migration

### Classes Status

**Already in Beep.OilandGas.Models:**
- `AP_INVOICE` (entity class) ✅
- `AP_PAYMENT` (entity class) ✅
- `AP_CREDIT_MEMO` (entity class) ✅

**DTOs Status:**
- DTOs already exist in `Beep.OilandGas.Models/DTOs/Accounting/` ✅

**No Migration Needed** - Entities already in correct location

## Service Class Creation

### New Service: APService

**Location**: `Beep.OilandGas.ProductionAccounting/AccountsPayable/APService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IAPService.cs`

```csharp
public interface IAPService
{
    Task<AP_INVOICE> CreateInvoiceAsync(CreateAPInvoiceRequest request, string userId, string? connectionName = null);
    Task<AP_INVOICE?> GetInvoiceAsync(string invoiceId, string? connectionName = null);
    Task<List<AP_INVOICE>> GetInvoicesByVendorAsync(string vendorId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    Task<AP_INVOICE> UpdateInvoiceAsync(UpdateAPInvoiceRequest request, string userId, string? connectionName = null);
    
    Task<AP_PAYMENT> CreatePaymentAsync(CreateAPPaymentRequest request, string userId, string? connectionName = null);
    Task<List<AP_PAYMENT>> GetPaymentsByInvoiceAsync(string invoiceId, string? connectionName = null);
    Task<AP_PAYMENT> ProcessPaymentAsync(string paymentId, string userId, string? connectionName = null);
    
    Task<AP_CREDIT_MEMO> CreateCreditMemoAsync(CreateAPCreditMemoRequest request, string userId, string? connectionName = null);
    
    // Missing workflows
    Task<APApprovalResult> ApproveInvoiceAsync(string invoiceId, string approverId, string? connectionName = null);
    Task<List<APAgingSummary>> GetAPAgingAsync(string? vendorId, string? connectionName = null);
    Task<VendorSummary> GetVendorSummaryAsync(string vendorId, string? connectionName = null);
    Task<PaymentBatchResult> ProcessPaymentBatchAsync(PaymentBatchRequest request, string userId, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Uses entities directly (already doing this ✅)
- Uses BUSINESS_ASSOCIATE for vendors (PPDM integration)

## Database Integration

### Tables Status

**Already Exist:**
- `AP_INVOICE` ✅
- `AP_PAYMENT` ✅
- `AP_CREDIT_MEMO` ✅

**Uses PPDM Tables:**
- `BUSINESS_ASSOCIATE` for vendors ✅

**May Need Additional Tables:**
- `AP_INVOICE_APPROVAL` (for approval workflow)
- `PAYMENT_BATCH` (for batch payment processing)

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("AP_INVOICE");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Accounting.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "AP_INVOICE");
```

## Missing Workflows

### 1. AP Invoice Approval Workflow
- Require approval for invoices above threshold
- Track approval status
- Maintain approval history
- Approval notifications

### 2. Payment Processing Workflow
- Process vendor payments
- Payment batch processing
- Payment method selection (check, ACH, wire)
- Payment reconciliation

### 3. Vendor Management
- Register vendors (using BUSINESS_ASSOCIATE)
- Track vendor information
- Vendor payment terms
- Vendor performance tracking

### 4. AP Aging Reports
- Calculate AP aging
- Identify overdue invoices
- Aging by vendor
- Aging analysis reports

### 5. AP Integration with Cost Accounting
- Link AP invoices to cost transactions
- Link AP invoices to AFEs
- AP vs cost reconciliation

### 6. AP Reporting
- AP summary reports
- AP detail reports
- AP by vendor reports
- AP payment reports

## Database Scripts

### Scripts Status

**Already Exist:**
- AP_INVOICE scripts ✅

**May Need Additional Scripts:**
- `AP_INVOICE_APPROVAL_TAB.sql` (if new table needed)
- `PAYMENT_BATCH_TAB.sql` (if new table needed)

## Implementation Steps

### Step 1: Create Service Interface
1. Create `IAPService` interface
2. Define all service methods

### Step 2: Refactor APManager to APService
1. Rename APManager.cs to APService.cs
2. Update to implement IAPService
3. Use PPDMGenericRepository (already using entities directly ✅)
4. Add missing workflow methods

### Step 3: Implement Missing Workflows
1. Implement AP invoice approval workflow
2. Implement payment processing workflow
3. Implement vendor management
4. Implement AP aging reports
5. Implement AP integration with cost accounting
6. Enhance AP reporting

## Testing Requirements

1. Test AP invoice creation and retrieval
2. Test AP payment creation and processing
3. Test AP approval workflow
4. Test AP aging calculation
5. Test vendor management
6. Test payment batch processing

## Dependencies

- Beep.OilandGas.Models (for entity classes) ✅
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- BUSINESS_ASSOCIATE (PPDM table for vendors) ✅
- Cost Accounting module (for cost integration)

