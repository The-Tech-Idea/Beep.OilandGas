# Management (Lease) Module Enhancement Plan

## Current State Analysis

### Existing Files
- `LeaseManager.cs` - Manager class with IDataSource integration
- `Models/LeaseModels.cs` - Contains FeeMineralLease, GovernmentLease, NetProfitLease, JointInterestLease models
- `Models/AgreementModels.cs` - Contains SalesAgreement, TransportationAgreement, ProcessingAgreement, StorageAgreement models

### Issues Identified
1. **Models in Wrong Location**: LeaseModels.cs and AgreementModels.cs classes should be in Beep.OilandGas.Models
2. **No Service Interface**: Missing ILeaseService interface
3. **Missing Workflows**: No lease renewal workflow, no lease expiration tracking, no lease reporting

## Entity/DTO Migration

### Classes to Move to Beep.OilandGas.Models

**Move to `Beep.OilandGas.Models/Data/Lease/`:**
- `FeeMineralLease` → `FEE_MINERAL_LEASE` (entity class with PPDM audit columns)
- `GovernmentLease` → `GOVERNMENT_LEASE` (entity class)
- `NetProfitLease` → `NET_PROFIT_LEASE` (entity class)
- `JointInterestLease` → `JOINT_INTEREST_LEASE` (entity class)

**Move to `Beep.OilandGas.Models/Data/Agreement/`:**
- `SalesAgreement` → `SALES_AGREEMENT` (entity class)
- `TransportationAgreement` → `TRANSPORTATION_AGREEMENT` (entity class)
- `ProcessingAgreement` → `PROCESSING_AGREEMENT` (entity class)
- `StorageAgreement` → `STORAGE_AGREEMENT` (entity class)

**Create DTOs in `Beep.OilandGas.Models/DTOs/Lease/` and `Beep.OilandGas.Models/DTOs/Agreement/`**

## Service Class Creation

### New Service: LeaseService

**Location**: `Beep.OilandGas.ProductionAccounting/Management/LeaseService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/ILeaseService.cs`

```csharp
public interface ILeaseService
{
    Task<FEE_MINERAL_LEASE> RegisterFeeMineralLeaseAsync(CreateFeeMineralLeaseRequest request, string userId, string? connectionName = null);
    Task<GOVERNMENT_LEASE> RegisterGovernmentLeaseAsync(CreateGovernmentLeaseRequest request, string userId, string? connectionName = null);
    Task<NET_PROFIT_LEASE> RegisterNetProfitLeaseAsync(CreateNetProfitLeaseRequest request, string userId, string? connectionName = null);
    Task<JOINT_INTEREST_LEASE> RegisterJointInterestLeaseAsync(CreateJointInterestLeaseRequest request, string userId, string? connectionName = null);
    
    Task<object?> GetLeaseAsync(string leaseId, string? connectionName = null);
    Task<List<object>> GetLeasesByPropertyAsync(string propertyId, string? connectionName = null);
    
    Task<SALES_AGREEMENT> RegisterSalesAgreementAsync(CreateSalesAgreementRequest request, string userId, string? connectionName = null);
    Task<TRANSPORTATION_AGREEMENT> RegisterTransportationAgreementAsync(CreateTransportationAgreementRequest request, string userId, string? connectionName = null);
    
    // Missing workflows
    Task<LeaseRenewalResult> RenewLeaseAsync(string leaseId, LeaseRenewalRequest request, string userId, string? connectionName = null);
    Task<List<LeaseExpirationAlert>> GetLeasesExpiringAsync(DateTime? expirationDate, string? connectionName = null);
    Task<LeaseSummary> GetLeaseSummaryAsync(string leaseId, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Uses entities directly
- Uses LAND_RIGHT or PROPERTY (PPDM tables) for lease relationships

## Database Integration

### Tables Required

**FEE_MINERAL_LEASE**, **GOVERNMENT_LEASE**, **NET_PROFIT_LEASE**, **JOINT_INTEREST_LEASE**:
- LEASE_ID (PK)
- PROPERTY_ID (FK to PROPERTY or LAND_RIGHT)
- LEASE_NUMBER
- EFFECTIVE_DATE
- EXPIRY_DATE
- WORKING_INTEREST
- NET_REVENUE_INTEREST
- ROYALTY_RATE
- Standard PPDM audit columns

**SALES_AGREEMENT**, **TRANSPORTATION_AGREEMENT**, **PROCESSING_AGREEMENT**, **STORAGE_AGREEMENT**:
- Similar structure
- Links to LEASE or PROPERTY
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("FEE_MINERAL_LEASE");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Lease.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "FEE_MINERAL_LEASE");
```

## Missing Workflows

### 1. Lease Renewal Workflow
- Track lease renewals
- Renewal approval workflow
- Renewal history
- Renewal reporting

### 2. Lease Expiration Tracking
- Track lease expiration dates
- Generate expiration alerts
- Expiration reporting
- Expiration management

### 3. Lease Reporting
- Lease summary reports
- Lease detail reports
- Lease by property reports
- Lease expiration reports

### 4. Agreement Management
- Track agreement terms
- Agreement expiration tracking
- Agreement renewal workflow
- Agreement reporting

## Database Scripts

### Scripts to Create

**For all lease and agreement tables**:
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql` (FKs to PROPERTY, LAND_RIGHT)

## Implementation Steps

### Step 1: Create Entity Classes
1. Create entity classes in `Beep.OilandGas.Models/Data/Lease/` and `Beep.OilandGas.Models/Data/Agreement/`
2. Add standard PPDM audit columns

### Step 2: Create DTOs
1. Create request/response DTOs in `Beep.OilandGas.Models/DTOs/Lease/` and `Beep.OilandGas.Models/DTOs/Agreement/`

### Step 3: Create Service Interface
1. Create `ILeaseService` interface
2. Define all service methods

### Step 4: Refactor LeaseManager to LeaseService
1. Rename LeaseManager.cs to LeaseService.cs
2. Update to implement ILeaseService
3. Use PPDMGenericRepository (already has IDataSource ✅)
4. Use entities directly
5. Add missing workflow methods

### Step 5: Create Database Scripts
1. Generate TAB/PK/FK scripts for all tables (6 database types)

### Step 6: Implement Missing Workflows
1. Implement lease renewal workflow
2. Implement lease expiration tracking
3. Enhance lease reporting
4. Enhance agreement management

## Testing Requirements

1. Test lease registration
2. Test agreement registration
3. Test lease renewal
4. Test lease expiration tracking
5. Test lease reporting

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- PROPERTY or LAND_RIGHT (PPDM tables) ✅

