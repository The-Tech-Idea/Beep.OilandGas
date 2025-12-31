# Accounts Receivable Module Enhancement Plan

## Current State Analysis

### Existing Files
- `ARManager.cs` - Manager class with IDataSource integration, uses AR_INVOICE entity directly ✅

### Issues Identified
1. **No Service Interface**: Missing IARService interface
2. **Missing Workflows**: No AR invoice approval workflow, no payment application workflow, no customer management, no AR aging reports

## Entity/DTO Migration

### Classes Status

**Already in Beep.OilandGas.Models:**
- `AR_INVOICE` (entity class) ✅
- `AR_PAYMENT` (entity class) ✅
- `AR_CREDIT_MEMO` (entity class) ✅

**DTOs Status:**
- DTOs already exist in `Beep.OilandGas.Models/DTOs/Accounting/` ✅

**No Migration Needed** - Entities already in correct location

## Service Class Creation

### New Service: ARService

**Location**: `Beep.OilandGas.ProductionAccounting/AccountsReceivable/ARService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IARService.cs`

```csharp
public interface IARService
{
    Task<AR_INVOICE> CreateInvoiceAsync(CreateARInvoiceRequest request, string userId, string? connectionName = null);
    Task<AR_INVOICE?> GetInvoiceAsync(string invoiceId, string? connectionName = null);
    Task<List<AR_INVOICE>> GetInvoicesByCustomerAsync(string customerId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    Task<AR_INVOICE> UpdateInvoiceAsync(UpdateARInvoiceRequest request, string userId, string? connectionName = null);
    
    Task<AR_PAYMENT> CreatePaymentAsync(CreateARPaymentRequest request, string userId, string? connectionName = null);
    Task<AR_PAYMENT> ApplyPaymentAsync(string paymentId, string invoiceId, decimal amount, string userId, string? connectionName = null);
    Task<List<AR_PAYMENT>> GetPaymentsByInvoiceAsync(string invoiceId, string? connectionName = null);
    
    Task<AR_CREDIT_MEMO> CreateCreditMemoAsync(CreateARCreditMemoRequest request, string userId, string? connectionName = null);
    
    // Missing workflows
    Task<ARApprovalResult> ApproveInvoiceAsync(string invoiceId, string approverId, string? connectionName = null);
    Task<List<ARAgingSummary>> GetARAgingAsync(string? customerId, string? connectionName = null);
    Task<CustomerSummary> GetCustomerSummaryAsync(string customerId, string? connectionName = null);
    Task<PaymentApplicationResult> ApplyPaymentToInvoicesAsync(PaymentApplicationRequest request, string userId, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Uses entities directly (already doing this ✅)
- Uses BUSINESS_ASSOCIATE for customers (PPDM integration)

## Database Integration

### Tables Status

**Already Exist:**
- `AR_INVOICE` ✅
- `AR_PAYMENT` ✅
- `AR_CREDIT_MEMO` ✅

**Uses PPDM Tables:**
- `BUSINESS_ASSOCIATE` for customers ✅

**May Need Additional Tables:**
- `AR_INVOICE_APPROVAL` (for approval workflow)
- `PAYMENT_APPLICATION` (for payment application tracking)

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("AR_INVOICE");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Accounting.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "AR_INVOICE");
```

## Missing Workflows

### 1. AR Invoice Approval Workflow
- Require approval for invoices above threshold
- Track approval status
- Maintain approval history
- Approval notifications

### 2. Payment Application Workflow
- Apply customer payments to invoices
- Partial payment application
- Payment allocation rules
- Payment application history

### 3. Customer Management
- Register customers (using BUSINESS_ASSOCIATE)
- Track customer information
- Customer credit limits
- Customer payment terms

### 4. AR Aging Reports
- Calculate AR aging
- Identify overdue invoices
- Aging by customer
- Aging analysis reports

### 5. AR Integration with Revenue
- Link AR invoices to revenue transactions
- Auto-create AR invoices from revenue
- AR vs revenue reconciliation

### 6. AR Reporting
- AR summary reports
- AR detail reports
- AR by customer reports
- AR payment reports

## Database Scripts

### Scripts Status

**Already Exist:**
- AR_INVOICE scripts ✅

**May Need Additional Scripts:**
- `AR_INVOICE_APPROVAL_TAB.sql` (if new table needed)
- `PAYMENT_APPLICATION_TAB.sql` (if new table needed)

## Implementation Steps

### Step 1: Create Service Interface
1. Create `IARService` interface
2. Define all service methods

### Step 2: Refactor ARManager to ARService
1. Rename ARManager.cs to ARService.cs
2. Update to implement IARService
3. Use PPDMGenericRepository (already using entities directly ✅)
4. Add missing workflow methods

### Step 3: Implement Missing Workflows
1. Implement AR invoice approval workflow
2. Implement payment application workflow
3. Implement customer management
4. Implement AR aging reports
5. Implement AR integration with revenue
6. Enhance AR reporting

## Testing Requirements

1. Test AR invoice creation and retrieval
2. Test AR payment creation and application
3. Test AR approval workflow
4. Test AR aging calculation
5. Test customer management
6. Test payment application

## Dependencies

- Beep.OilandGas.Models (for entity classes) ✅
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- BUSINESS_ASSOCIATE (PPDM table for customers) ✅
- Revenue/Accounting module (for revenue integration)

