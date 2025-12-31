# Trading Module Enhancement Plan

## Current State Analysis

### Existing Files
- `TradingManager.cs` - Manager class with IDataSource integration
- `ExchangeModels.cs` - Contains ExchangeContract, ExchangeCommitment, ExchangeTransaction models
- `ExchangeAccounting.cs` - Static accounting methods
- `ExchangeStatement.cs` - Statement generation
- `ExchangeReconciliation.cs` - Reconciliation engine
- `DifferentialCalculator.cs` - Static differential calculation

### Issues Identified
1. **Models in Wrong Location**: ExchangeModels.cs classes should be in Beep.OilandGas.Models
2. **No Service Interface**: Missing ITradingService interface
3. **Missing Workflows**: No exchange contract approval workflow, no exchange settlement workflow, no exchange reporting

## Entity/DTO Migration

### Classes to Move to Beep.OilandGas.Models

**Move to `Beep.OilandGas.Models/Data/Trading/`:**
- `ExchangeContract` → `EXCHANGE_CONTRACT` (entity class with PPDM audit columns)
- `ExchangeCommitment` → `EXCHANGE_COMMITMENT` (entity class)
- `ExchangeTransaction` → `EXCHANGE_TRANSACTION` (entity class)

**Create DTOs in `Beep.OilandGas.Models/DTOs/Trading/`:**
- `CreateExchangeContractRequest`
- `ExchangeContractResponse`
- `CreateExchangeCommitmentRequest`
- `ExchangeCommitmentResponse`

**Keep in ProductionAccounting:**
- `ExchangeAccounting` static methods (calculation logic)
- `ExchangeStatementGenerator` static methods (statement logic)
- `ExchangeReconciliationEngine` static methods (reconciliation logic)
- `DifferentialCalculator` static methods (calculation logic)

## Service Class Creation

### New Service: TradingService

**Location**: `Beep.OilandGas.ProductionAccounting/Trading/TradingService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/ITradingService.cs`

```csharp
public interface ITradingService
{
    Task<EXCHANGE_CONTRACT> RegisterContractAsync(CreateExchangeContractRequest request, string userId, string? connectionName = null);
    Task<EXCHANGE_CONTRACT?> GetContractAsync(string contractId, string? connectionName = null);
    Task<List<EXCHANGE_CONTRACT>> GetContractsAsync(string? connectionName = null);
    
    Task<EXCHANGE_COMMITMENT> CreateCommitmentAsync(CreateExchangeCommitmentRequest request, string userId, string? connectionName = null);
    Task<List<EXCHANGE_COMMITMENT>> GetCommitmentsByContractAsync(string contractId, string? connectionName = null);
    
    Task<EXCHANGE_TRANSACTION> RecordTransactionAsync(CreateExchangeTransactionRequest request, string userId, string? connectionName = null);
    Task<List<EXCHANGE_TRANSACTION>> GetTransactionsByContractAsync(string contractId, string? connectionName = null);
    
    // Missing workflows
    Task<ExchangeSettlementResult> SettleExchangeAsync(string contractId, DateTime settlementDate, string userId, string? connectionName = null);
    Task<ExchangeReconciliationResult> ReconcileExchangeAsync(string contractId, DateTime reconciliationDate, string userId, string? connectionName = null);
    Task<ExchangeStatement> GenerateStatementAsync(string contractId, DateTime statementDate, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Calls static methods for calculations
- Uses entities directly

## Database Integration

### Tables Required

**EXCHANGE_CONTRACT**:
- EXCHANGE_CONTRACT_ID (PK)
- CONTRACT_NUMBER
- COUNTERPARTY_BA_ID (FK to BUSINESS_ASSOCIATE)
- COMMODITY_TYPE
- CONTRACT_DATE
- EFFECTIVE_DATE
- EXPIRY_DATE
- Standard PPDM audit columns

**EXCHANGE_COMMITMENT**, **EXCHANGE_TRANSACTION**:
- Similar structure
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("EXCHANGE_CONTRACT");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Trading.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "EXCHANGE_CONTRACT");
```

## Missing Workflows

### 1. Exchange Contract Approval Workflow
- Require approval for exchange contracts
- Track approval status
- Maintain approval history

### 2. Exchange Settlement Workflow
- Settle exchange transactions
- Calculate settlement amounts
- Generate settlement reports

### 3. Exchange Reconciliation
- Reconcile exchange transactions
- Identify discrepancies
- Generate reconciliation reports

### 4. Exchange Reporting
- Exchange contract reports
- Exchange transaction reports
- Exchange statement reports
- Exchange reconciliation reports

## Database Scripts

### Scripts to Create

**For EXCHANGE_CONTRACT, EXCHANGE_COMMITMENT, EXCHANGE_TRANSACTION**:
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql` (FKs to BUSINESS_ASSOCIATE)

## Implementation Steps

### Step 1: Create Entity Classes
1. Create entity classes in `Beep.OilandGas.Models/Data/Trading/`
2. Add standard PPDM audit columns

### Step 2: Create DTOs
1. Create request/response DTOs in `Beep.OilandGas.Models/DTOs/Trading/`

### Step 3: Create Service Interface
1. Create `ITradingService` interface
2. Define all service methods

### Step 4: Refactor TradingManager to TradingService
1. Rename TradingManager.cs to TradingService.cs
2. Update to implement ITradingService
3. Use PPDMGenericRepository
4. Use entities directly
5. Add missing workflow methods

### Step 5: Create Database Scripts
1. Generate TAB/PK/FK scripts for all tables (6 database types)

### Step 6: Implement Missing Workflows
1. Implement exchange contract approval workflow
2. Implement exchange settlement workflow
3. Implement exchange reconciliation enhancements
4. Enhance exchange reporting

## Testing Requirements

1. Test exchange contract registration
2. Test exchange commitment creation
3. Test exchange transaction recording
4. Test exchange settlement
5. Test exchange reconciliation

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- BUSINESS_ASSOCIATE (PPDM table for counterparties) ✅

