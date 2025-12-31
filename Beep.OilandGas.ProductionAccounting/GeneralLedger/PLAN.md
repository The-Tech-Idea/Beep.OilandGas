# General Ledger Module Enhancement Plan

## Current State Analysis

### Existing Files
- `GLAccountManager.cs` - Manager class with IDataSource integration, uses GL_ACCOUNT entity directly
- `JournalEntryManager.cs` - Manager class with IDataSource integration

### Issues Identified
1. **No Service Interface**: Missing IGLAccountService and IJournalEntryService interfaces
2. **Missing Workflows**: No GL account hierarchy management, no account balance reconciliation, no period closing workflow
3. **Missing Integration**: No automatic GL posting from other modules

## Entity/DTO Migration

### Classes Status

**Already in Beep.OilandGas.Models:**
- `GL_ACCOUNT` (entity class) ✅
- `GL_ENTRY` (entity class) ✅
- `JOURNAL_ENTRY` (entity class) ✅
- `JOURNAL_ENTRY_LINE` (entity class) ✅

**DTOs Status:**
- DTOs already exist in `Beep.OilandGas.Models/DTOs/Accounting/` ✅

**No Migration Needed** - Entities already in correct location

## Service Class Creation

### New Service: GLAccountService

**Location**: `Beep.OilandGas.ProductionAccounting/GeneralLedger/GLAccountService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IGLAccountService.cs`

```csharp
public interface IGLAccountService
{
    Task<GL_ACCOUNT> CreateAccountAsync(CreateGLAccountRequest request, string userId, string? connectionName = null);
    Task<GL_ACCOUNT?> GetAccountAsync(string accountId, string? connectionName = null);
    Task<GL_ACCOUNT?> GetAccountByNumberAsync(string accountNumber, string? connectionName = null);
    Task<List<GL_ACCOUNT>> GetAccountsByTypeAsync(string accountType, string? connectionName = null);
    Task<List<GL_ACCOUNT>> GetAccountHierarchyAsync(string? connectionName = null);
    Task<GL_ACCOUNT> UpdateAccountAsync(UpdateGLAccountRequest request, string userId, string? connectionName = null);
    Task<bool> DeleteAccountAsync(string accountId, string userId, string? connectionName = null);
    
    // Missing workflows
    Task<AccountBalanceSummary> GetAccountBalanceAsync(string accountId, DateTime? asOfDate, string? connectionName = null);
    Task<AccountReconciliationResult> ReconcileAccountAsync(string accountId, DateTime reconciliationDate, string userId, string? connectionName = null);
    Task<List<GL_ACCOUNT>> GetAccountsRequiringReconciliationAsync(string? connectionName = null);
}
```

### New Service: JournalEntryService

**Location**: `Beep.OilandGas.ProductionAccounting/GeneralLedger/JournalEntryService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IJournalEntryService.cs`

```csharp
public interface IJournalEntryService
{
    Task<JOURNAL_ENTRY> CreateJournalEntryAsync(CreateJournalEntryRequest request, string userId, string? connectionName = null);
    Task<JOURNAL_ENTRY?> GetJournalEntryAsync(string entryId, string? connectionName = null);
    Task<List<JOURNAL_ENTRY>> GetJournalEntriesByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null);
    Task<List<JOURNAL_ENTRY>> GetJournalEntriesByAccountAsync(string accountId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    Task<JOURNAL_ENTRY> PostJournalEntryAsync(string entryId, string userId, string? connectionName = null);
    Task<JOURNAL_ENTRY> ReverseJournalEntryAsync(string entryId, string reason, string userId, string? connectionName = null);
    
    // Missing workflows
    Task<PeriodClosingResult> ClosePeriodAsync(DateTime periodEndDate, string userId, string? connectionName = null);
    Task<List<UnpostedEntries>> GetUnpostedEntriesAsync(string? connectionName = null);
    Task<JournalEntryApprovalResult> ApproveJournalEntryAsync(string entryId, string approverId, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Uses entities directly (no Dictionary conversions)

## Database Integration

### Tables Status

**Already Exist:**
- `GL_ACCOUNT` ✅
- `GL_ENTRY` ✅
- `JOURNAL_ENTRY` ✅
- `JOURNAL_ENTRY_LINE` ✅

**May Need Additional Tables:**
- `ACCOUNT_RECONCILIATION` (for reconciliation tracking)
- `PERIOD_CLOSING` (for period closing)

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("GL_ACCOUNT");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Accounting.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "GL_ACCOUNT");
```

## Missing Workflows

### 1. GL Account Hierarchy Management
- Build account hierarchy tree
- Validate parent-child relationships
- Rollup balances from child to parent accounts
- Account hierarchy reporting

### 2. Account Balance Reconciliation
- Reconcile account balances with sub-ledgers
- Track reconciliation status
- Generate reconciliation reports
- Flag accounts requiring reconciliation

### 3. Period Closing Workflow
- Close accounting periods
- Generate closing entries
- Lock closed periods
- Period closing reports

### 4. Automatic GL Posting
- Auto-post from revenue transactions
- Auto-post from cost transactions
- Auto-post from inventory transactions
- Auto-post from AP/AR transactions

### 5. Journal Entry Approval Workflow
- Require approval for journal entries
- Track approval status
- Maintain approval history
- Approval notifications

### 6. Unposted Entries Management
- Track unposted journal entries
- Batch posting of entries
- Posting validation
- Posting reports

### 7. GL Reporting
- Trial balance reports
- General ledger detail reports
- Account balance reports
- Financial statement preparation

## Database Scripts

### Scripts Status

**Already Exist:**
- GL_ACCOUNT scripts ✅
- JOURNAL_ENTRY scripts ✅

**May Need Additional Scripts:**
- `ACCOUNT_RECONCILIATION_TAB.sql` (if new table needed)
- `PERIOD_CLOSING_TAB.sql` (if new table needed)

## Implementation Steps

### Step 1: Create Service Interfaces
1. Create `IGLAccountService` interface
2. Create `IJournalEntryService` interface
3. Define all service methods

### Step 2: Refactor Managers to Services
1. Rename GLAccountManager.cs to GLAccountService.cs
2. Rename JournalEntryManager.cs to JournalEntryService.cs
3. Update to implement interfaces
4. Use PPDMGenericRepository (already using entities directly)

### Step 3: Implement Missing Workflows
1. Implement GL account hierarchy management
2. Implement account balance reconciliation
3. Implement period closing workflow
4. Implement automatic GL posting
5. Implement journal entry approval workflow
6. Implement unposted entries management
7. Enhance GL reporting

### Step 4: Create Additional Database Scripts (if needed)
1. Generate scripts for ACCOUNT_RECONCILIATION if new table needed
2. Generate scripts for PERIOD_CLOSING if new table needed

## Testing Requirements

1. Test GL account creation and retrieval
2. Test journal entry creation and posting
3. Test account balance calculations
4. Test account reconciliation
5. Test period closing
6. Test automatic GL posting

## Dependencies

- Beep.OilandGas.Models (for entity classes) ✅
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Revenue/Accounting module (for auto-posting)
- Cost Accounting module (for auto-posting)

