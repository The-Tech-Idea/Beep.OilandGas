# Imbalance Module Enhancement Plan

## Current State Analysis

### Existing Files
- `ImbalanceManager.cs` - Manager class with IDataSource integration
- `ImbalanceModels.cs` - Contains ProductionAvail, Nomination, ActualDelivery, Imbalance models

### Issues Identified
1. **Models in Wrong Location**: ImbalanceModels.cs classes should be in Beep.OilandGas.Models
2. **No Service Interface**: Missing IImbalanceService interface
3. **Missing Workflows**: No imbalance reconciliation workflow, no imbalance reporting, no imbalance settlement

## Entity/DTO Migration

### Classes to Move to Beep.OilandGas.Models

**Move to `Beep.OilandGas.Models/Data/Imbalance/`:**
- `ProductionAvail` → `PRODUCTION_AVAIL` (entity class with PPDM audit columns)
- `Nomination` → `NOMINATION` (entity class)
- `ActualDelivery` → `ACTUAL_DELIVERY` (entity class)
- `Imbalance` → `IMBALANCE` (entity class)

**Create DTOs in `Beep.OilandGas.Models/DTOs/Imbalance/`:**
- `CreateProductionAvailRequest`
- `ProductionAvailResponse`
- `CreateNominationRequest`
- `NominationResponse`

## Service Class Creation

### New Service: ImbalanceService

**Location**: `Beep.OilandGas.ProductionAccounting/Imbalance/ImbalanceService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IImbalanceService.cs`

```csharp
public interface IImbalanceService
{
    Task<PRODUCTION_AVAIL> CreateProductionAvailAsync(CreateProductionAvailRequest request, string userId, string? connectionName = null);
    Task<PRODUCTION_AVAIL?> GetProductionAvailAsync(string availId, string? connectionName = null);
    
    Task<NOMINATION> CreateNominationAsync(CreateNominationRequest request, string userId, string? connectionName = null);
    Task<List<NOMINATION>> GetNominationsByPeriodAsync(DateTime periodStart, DateTime periodEnd, string? connectionName = null);
    
    Task<ACTUAL_DELIVERY> RecordActualDeliveryAsync(CreateActualDeliveryRequest request, string userId, string? connectionName = null);
    Task<List<ACTUAL_DELIVERY>> GetDeliveriesByNominationAsync(string nominationId, string? connectionName = null);
    
    Task<IMBALANCE> CalculateImbalanceAsync(string nominationId, string userId, string? connectionName = null);
    Task<List<IMBALANCE>> GetImbalancesByPeriodAsync(DateTime periodStart, DateTime periodEnd, string? connectionName = null);
    
    // Missing workflows
    Task<ImbalanceReconciliationResult> ReconcileImbalanceAsync(string imbalanceId, string userId, string? connectionName = null);
    Task<ImbalanceSettlementResult> SettleImbalanceAsync(string imbalanceId, DateTime settlementDate, string userId, string? connectionName = null);
    Task<List<ImbalanceSummary>> GetImbalanceSummaryAsync(DateTime? periodStart, DateTime? periodEnd, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Uses entities directly

## Database Integration

### Tables Required

**PRODUCTION_AVAIL**:
- PRODUCTION_AVAIL_ID (PK)
- PROPERTY_ID (FK to PROPERTY)
- AVAIL_DATE
- ESTIMATED_VOLUME
- Standard PPDM audit columns

**NOMINATION**, **ACTUAL_DELIVERY**, **IMBALANCE**:
- Similar structure
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("PRODUCTION_AVAIL");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Imbalance.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "PRODUCTION_AVAIL");
```

## Missing Workflows

### 1. Imbalance Reconciliation
- Reconcile nominations vs actual deliveries
- Identify imbalances
- Calculate imbalance amounts
- Generate reconciliation reports

### 2. Imbalance Settlement
- Settle imbalances
- Calculate settlement amounts
- Generate settlement reports
- Track settlement history

### 3. Imbalance Reporting
- Imbalance summary reports
- Imbalance detail reports
- Imbalance by period reports
- Imbalance trends

### 4. Nomination Management
- Create and manage nominations
- Track nomination status
- Nomination vs actual comparison
- Nomination reporting

## Database Scripts

### Scripts to Create

**For PRODUCTION_AVAIL, NOMINATION, ACTUAL_DELIVERY, IMBALANCE**:
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql` (FKs to PROPERTY)

## Implementation Steps

### Step 1: Create Entity Classes
1. Create entity classes in `Beep.OilandGas.Models/Data/Imbalance/`
2. Add standard PPDM audit columns

### Step 2: Create DTOs
1. Create request/response DTOs in `Beep.OilandGas.Models/DTOs/Imbalance/`

### Step 3: Create Service Interface
1. Create `IImbalanceService` interface
2. Define all service methods

### Step 4: Refactor ImbalanceManager to ImbalanceService
1. Rename ImbalanceManager.cs to ImbalanceService.cs
2. Update to implement IImbalanceService
3. Use PPDMGenericRepository (already has IDataSource ✅)
4. Use entities directly
5. Add missing workflow methods

### Step 5: Create Database Scripts
1. Generate TAB/PK/FK scripts for all tables (6 database types)

### Step 6: Implement Missing Workflows
1. Implement imbalance reconciliation
2. Implement imbalance settlement
3. Enhance imbalance reporting
4. Enhance nomination management

## Testing Requirements

1. Test production avail creation
2. Test nomination creation
3. Test actual delivery recording
4. Test imbalance calculation
5. Test imbalance reconciliation

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Production module (for production data)

