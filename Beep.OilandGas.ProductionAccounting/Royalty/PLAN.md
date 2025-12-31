# Royalty Module Enhancement Plan

## Current State Analysis

### Existing Files
- `RoyaltyManager.cs` - Manager class with IDataSource integration
- `RoyaltyModels.cs` - Contains RoyaltyInterest, RoyaltyPayment, RoyaltyStatement models
- `RoyaltyCalculation.cs` - Static calculation methods
- `RoyaltyStatement.cs` - Statement generation
- `TaxReporting.cs` - Tax reporting (1099 forms)

### Issues Identified
1. **Models in Wrong Location**: RoyaltyModels.cs classes should be in Beep.OilandGas.Models
2. **No Service Interface**: Missing IRoyaltyService interface
3. **Missing Workflows**: No royalty owner management, no royalty payment approval workflow, no royalty audit trail
4. **Missing Integration**: No integration with revenue transactions for automatic royalty calculation

## Entity/DTO Migration

### Classes to Move to Beep.OilandGas.Models

**Move to `Beep.OilandGas.Models/Data/Royalty/`:**
- `RoyaltyInterest` → `ROYALTY_INTEREST` (entity class with PPDM audit columns)
- `RoyaltyPayment` → `ROYALTY_PAYMENT` (entity class)
- `RoyaltyStatement` → `ROYALTY_STATEMENT` (entity class)
- `RoyaltyCalculation` → `ROYALTY_CALCULATION` (entity class)

**Create DTOs in `Beep.OilandGas.Models/DTOs/Royalty/`:**
- `CreateRoyaltyInterestRequest`
- `RoyaltyInterestResponse`
- `CreateRoyaltyPaymentRequest`
- `RoyaltyPaymentResponse`
- `CreateRoyaltyStatementRequest`
- `RoyaltyStatementResponse`

**Keep in ProductionAccounting:**
- `RoyaltyCalculator` static methods (calculation logic)
- `TaxReporting` static methods (tax logic)

## Service Class Creation

### New Service: RoyaltyService

**Location**: `Beep.OilandGas.ProductionAccounting/Royalty/RoyaltyService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IRoyaltyService.cs`

```csharp
public interface IRoyaltyService
{
    Task<ROYALTY_INTEREST> RegisterRoyaltyInterestAsync(CreateRoyaltyInterestRequest request, string userId, string? connectionName = null);
    Task<ROYALTY_INTEREST?> GetRoyaltyInterestAsync(string interestId, string? connectionName = null);
    Task<List<ROYALTY_INTEREST>> GetRoyaltyInterestsByPropertyAsync(string propertyId, string? connectionName = null);
    
    Task<ROYALTY_PAYMENT> CalculateAndCreatePaymentAsync(
        string revenueTransactionId,
        string royaltyOwnerId,
        decimal royaltyInterest,
        DateTime paymentDate,
        string userId,
        string? connectionName = null);
    
    Task<ROYALTY_PAYMENT?> GetRoyaltyPaymentAsync(string paymentId, string? connectionName = null);
    Task<List<ROYALTY_PAYMENT>> GetRoyaltyPaymentsByOwnerAsync(string ownerId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    
    Task<ROYALTY_STATEMENT> CreateStatementAsync(CreateRoyaltyStatementRequest request, string userId, string? connectionName = null);
    Task<ROYALTY_STATEMENT?> GetStatementAsync(string statementId, string? connectionName = null);
    
    // Missing workflows
    Task<List<RoyaltyOwnerSummary>> GetRoyaltyOwnerSummaryAsync(string ownerId, string? connectionName = null);
    Task<RoyaltyPaymentApprovalResult> ApprovePaymentAsync(string paymentId, string approverId, string? connectionName = null);
    Task<List<RoyaltyAuditTrail>> GetRoyaltyAuditTrailAsync(string interestId, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Calls RoyaltyCalculator static methods for calculations
- Integrates with Revenue transactions

## Database Integration

### Tables Required

**ROYALTY_INTEREST**:
- ROYALTY_INTEREST_ID (PK)
- PROPERTY_ID (FK to PROPERTY)
- ROYALTY_OWNER_BA_ID (FK to BUSINESS_ASSOCIATE)
- INTEREST_TYPE
- INTEREST_PERCENTAGE
- EFFECTIVE_DATE
- EXPIRY_DATE
- Standard PPDM audit columns

**ROYALTY_PAYMENT**:
- ROYALTY_PAYMENT_ID (PK)
- ROYALTY_INTEREST_ID (FK)
- REVENUE_TRANSACTION_ID (FK to REVENUE_TRANSACTION)
- PAYMENT_DATE
- ROYALTY_AMOUNT
- PAYMENT_STATUS
- Standard PPDM audit columns

**ROYALTY_STATEMENT**, **ROYALTY_CALCULATION**:
- Similar structure
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("ROYALTY_INTEREST");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Royalty.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "ROYALTY_INTEREST");
```

## Missing Workflows

### 1. Royalty Owner Management
- Register royalty owners (using BUSINESS_ASSOCIATE)
- Track royalty owner contact information
- Manage royalty owner relationships
- Royalty owner search and reporting

### 2. Automatic Royalty Calculation
- Automatically calculate royalties from revenue transactions
- Batch royalty calculation
- Royalty calculation rules engine
- Royalty calculation history

### 3. Royalty Payment Approval Workflow
- Require approval for royalty payments
- Track approval status
- Maintain approval history
- Payment approval notifications

### 4. Royalty Audit Trail
- Track all royalty calculations
- Track royalty payment changes
- Track royalty interest changes
- Generate audit reports

### 5. Royalty Reconciliation
- Reconcile royalty calculations vs payments
- Identify royalty discrepancies
- Generate reconciliation reports

### 6. Royalty Reporting
- Royalty owner statements
- Royalty payment reports
- Royalty interest reports
- Royalty tax reports (1099)

### 7. Royalty Interest Changes
- Track royalty interest changes over time
- Effective dating for royalty interests
- Royalty interest history

## Database Scripts

### Scripts to Create

**For ROYALTY_INTEREST, ROYALTY_PAYMENT, ROYALTY_STATEMENT, ROYALTY_CALCULATION**:
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql` (FKs to PROPERTY, BUSINESS_ASSOCIATE, REVENUE_TRANSACTION)

## Implementation Steps

### Step 1: Create Entity Classes
1. Create entity classes in `Beep.OilandGas.Models/Data/Royalty/`
2. Add standard PPDM audit columns
3. Map all properties from RoyaltyModels

### Step 2: Create DTOs
1. Create request/response DTOs in `Beep.OilandGas.Models/DTOs/Royalty/`
2. Create DTOs for missing workflows

### Step 3: Create Service Interface
1. Create `IRoyaltyService` interface
2. Define all service methods

### Step 4: Refactor RoyaltyManager to RoyaltyService
1. Rename RoyaltyManager.cs to RoyaltyService.cs
2. Update to implement IRoyaltyService
3. Use entities directly with PPDMGenericRepository
4. Add missing workflow methods

### Step 5: Create Database Scripts
1. Generate TAB/PK/FK scripts for all tables (6 database types)
2. Place scripts in appropriate directories

### Step 6: Implement Missing Workflows
1. Implement royalty owner management
2. Implement automatic royalty calculation
3. Implement royalty payment approval workflow
4. Implement royalty audit trail
5. Implement royalty reconciliation
6. Implement royalty reporting enhancements

## Testing Requirements

1. Test royalty interest registration
2. Test royalty payment calculation and creation
3. Test royalty statement generation
4. Test tax reporting (1099)
5. Test royalty approval workflow
6. Test royalty reconciliation

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Revenue/Accounting module (for revenue transactions)
- Ownership module (for ownership interests)

