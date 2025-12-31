# Accounting (Sales) Module Enhancement Plan

## Current State Analysis

### Existing Files
- `SalesTransaction.cs` - Sales transaction model
- `SalesJournal.cs` - Sales journal entry generation
- `SalesStatement.cs` - Sales statement generation
- `Receivable.cs` - Receivable management
- `WellheadSaleAccounting.cs` - Wellhead sale accounting

### Issues Identified
1. **No Database Integration**: These are static classes or models without database integration
2. **Models in Wrong Location**: SalesTransaction, Receivable should be entity classes in Beep.OilandGas.Models
3. **No Service Class**: Missing IAccountingService interface
4. **Missing Workflows**: No sales transaction approval workflow, no sales reconciliation, no sales reporting

## Entity/DTO Migration

### Classes to Move to Beep.OilandGas.Models

**Move to `Beep.OilandGas.Models/Data/Accounting/`:**
- `SalesTransaction` → `SALES_TRANSACTION` (entity class with PPDM audit columns)
- `Receivable` → `RECEIVABLE` (entity class)

**Create DTOs in `Beep.OilandGas.Models/DTOs/Accounting/`:**
- `CreateSalesTransactionRequest`
- `SalesTransactionResponse`
- `CreateReceivableRequest`
- `ReceivableResponse`

**Keep in ProductionAccounting:**
- `SalesJournalEntryGenerator` static methods (calculation logic)
- `SalesStatementGenerator` static methods (statement logic)
- `WellheadSaleAccounting` static methods (calculation logic)

## Service Class Creation

### New Service: AccountingService

**Location**: `Beep.OilandGas.ProductionAccounting/Accounting/AccountingService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IAccountingService.cs`

```csharp
public interface IAccountingService
{
    Task<SALES_TRANSACTION> CreateSalesTransactionAsync(CreateSalesTransactionRequest request, string userId, string? connectionName = null);
    Task<SALES_TRANSACTION?> GetSalesTransactionAsync(string transactionId, string? connectionName = null);
    Task<List<SALES_TRANSACTION>> GetSalesTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null);
    
    Task<RECEIVABLE> CreateReceivableAsync(CreateReceivableRequest request, string userId, string? connectionName = null);
    Task<List<RECEIVABLE>> GetReceivablesByCustomerAsync(string customerId, string? connectionName = null);
    
    Task<JOURNAL_ENTRY> CreateSalesJournalEntryAsync(string salesTransactionId, string userId, string? connectionName = null);
    
    // Missing workflows
    Task<SalesApprovalResult> ApproveSalesTransactionAsync(string transactionId, string approverId, string? connectionName = null);
    Task<SalesReconciliationResult> ReconcileSalesAsync(SalesReconciliationRequest request, string userId, string? connectionName = null);
    Task<SalesStatement> GenerateSalesStatementAsync(string customerId, DateTime statementDate, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Calls static methods for journal entry generation
- Uses entities directly

## Database Integration

### Tables Required

**SALES_TRANSACTION**:
- SALES_TRANSACTION_ID (PK)
- RUN_TICKET_ID (FK to RUN_TICKET)
- CUSTOMER_BA_ID (FK to BUSINESS_ASSOCIATE)
- SALES_DATE
- VOLUME
- PRICE
- TOTAL_AMOUNT
- Standard PPDM audit columns

**RECEIVABLE**:
- RECEIVABLE_ID (PK)
- SALES_TRANSACTION_ID (FK)
- CUSTOMER_BA_ID (FK to BUSINESS_ASSOCIATE)
- AMOUNT
- DUE_DATE
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("SALES_TRANSACTION");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Accounting.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "SALES_TRANSACTION");
```

## Missing Workflows

### 1. Sales Transaction Approval Workflow
- Require approval for sales transactions
- Track approval status
- Maintain approval history

### 2. Sales Reconciliation
- Reconcile sales vs production
- Reconcile sales vs revenue
- Identify sales discrepancies
- Generate reconciliation reports

### 3. Sales Reporting
- Sales summary reports
- Sales detail reports
- Sales by customer reports
- Sales statement generation

### 4. Receivable Management
- Track receivables
- Receivable aging
- Receivable collection
- Receivable reporting

## Database Scripts

### Scripts to Create

**For SALES_TRANSACTION, RECEIVABLE**:
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql` (FKs to RUN_TICKET, BUSINESS_ASSOCIATE)

## Implementation Steps

### Step 1: Create Entity Classes
1. Create entity classes in `Beep.OilandGas.Models/Data/Accounting/`
2. Add standard PPDM audit columns

### Step 2: Create DTOs
1. Create request/response DTOs in `Beep.OilandGas.Models/DTOs/Accounting/`

### Step 3: Create Service Interface
1. Create `IAccountingService` interface
2. Define all service methods

### Step 4: Create Service Class
1. Create `AccountingService.cs`
2. Implement IAccountingService
3. Use PPDMGenericRepository
4. Call static methods for journal entries
5. Use entities directly
6. Add missing workflow methods

### Step 5: Create Database Scripts
1. Generate TAB/PK/FK scripts for all tables (6 database types)

### Step 6: Implement Missing Workflows
1. Implement sales transaction approval workflow
2. Implement sales reconciliation
3. Enhance sales reporting
4. Enhance receivable management

## Testing Requirements

1. Test sales transaction creation
2. Test receivable creation
3. Test sales journal entry generation
4. Test sales reconciliation
5. Test sales reporting

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Production module (for RunTicket)
- GeneralLedger module (for journal entries)
- BUSINESS_ASSOCIATE (PPDM table for customers) ✅

