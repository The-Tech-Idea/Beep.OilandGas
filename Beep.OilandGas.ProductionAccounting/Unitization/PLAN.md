# Unitization Module Enhancement Plan

## Current State Analysis

### Existing Files
- `UnitManager.cs` - Manager class
- `UnitModels.cs` - Contains UnitAgreement, ParticipatingArea, TractParticipation models

### Issues Identified
1. **No Database Integration**: UnitManager may not have IDataSource integration
2. **Models in Wrong Location**: UnitModels.cs classes should be in Beep.OilandGas.Models
3. **No Service Interface**: Missing IUnitizationService interface
4. **Missing Workflows**: No unit agreement approval workflow, no unit operations workflow, no unit reporting

## Entity/DTO Migration

### Classes to Move to Beep.OilandGas.Models

**Move to `Beep.OilandGas.Models/Data/Unitization/`:**
- `UnitAgreement` → `UNIT_AGREEMENT` (entity class with PPDM audit columns)
- `ParticipatingArea` → `PARTICIPATING_AREA` (entity class)
- `TractParticipation` → `TRACT_PARTICIPATION` (entity class)

**Create DTOs in `Beep.OilandGas.Models/DTOs/Unitization/`:**
- `CreateUnitAgreementRequest`
- `UnitAgreementResponse`
- `CreateParticipatingAreaRequest`
- `ParticipatingAreaResponse`

## Service Class Creation

### New Service: UnitizationService

**Location**: `Beep.OilandGas.ProductionAccounting/Unitization/UnitizationService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IUnitizationService.cs`

```csharp
public interface IUnitizationService
{
    Task<UNIT_AGREEMENT> CreateUnitAgreementAsync(CreateUnitAgreementRequest request, string userId, string? connectionName = null);
    Task<UNIT_AGREEMENT?> GetUnitAgreementAsync(string agreementId, string? connectionName = null);
    Task<List<UNIT_AGREEMENT>> GetUnitAgreementsAsync(string? connectionName = null);
    
    Task<PARTICIPATING_AREA> CreateParticipatingAreaAsync(CreateParticipatingAreaRequest request, string userId, string? connectionName = null);
    Task<List<PARTICIPATING_AREA>> GetParticipatingAreasByUnitAsync(string unitId, string? connectionName = null);
    
    Task<TRACT_PARTICIPATION> RegisterTractParticipationAsync(CreateTractParticipationRequest request, string userId, string? connectionName = null);
    Task<List<TRACT_PARTICIPATION>> GetTractParticipationsByAreaAsync(string areaId, string? connectionName = null);
    
    // Missing workflows
    Task<UnitApprovalResult> ApproveUnitAgreementAsync(string agreementId, string approverId, string? connectionName = null);
    Task<UnitOperationsSummary> GetUnitOperationsSummaryAsync(string unitId, string? connectionName = null);
    Task<List<UnitAgreement>> GetAgreementsRequiringApprovalAsync(string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Uses entities directly

## Database Integration

### Tables Required

**UNIT_AGREEMENT**:
- UNIT_AGREEMENT_ID (PK)
- UNIT_NAME
- UNIT_NUMBER
- EFFECTIVE_DATE
- EXPIRY_DATE
- Standard PPDM audit columns

**PARTICIPATING_AREA**, **TRACT_PARTICIPATION**:
- Similar structure
- Links to UNIT_AGREEMENT
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("UNIT_AGREEMENT");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Unitization.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "UNIT_AGREEMENT");
```

## Missing Workflows

### 1. Unit Agreement Approval Workflow
- Require approval for unit agreements
- Track approval status
- Maintain approval history

### 2. Unit Operations Management
- Track unit operations
- Unit production allocation
- Unit cost allocation
- Unit reporting

### 3. Unit Reporting
- Unit agreement reports
- Participating area reports
- Tract participation reports
- Unit operations reports

## Database Scripts

### Scripts to Create

**For UNIT_AGREEMENT, PARTICIPATING_AREA, TRACT_PARTICIPATION**:
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql`

## Implementation Steps

### Step 1: Create Entity Classes
1. Create entity classes in `Beep.OilandGas.Models/Data/Unitization/`
2. Add standard PPDM audit columns

### Step 2: Create DTOs
1. Create request/response DTOs in `Beep.OilandGas.Models/DTOs/Unitization/`

### Step 3: Create Service Interface
1. Create `IUnitizationService` interface
2. Define all service methods

### Step 4: Create/Refactor UnitManager to UnitizationService
1. Create UnitizationService.cs (or refactor UnitManager)
2. Implement IUnitizationService
3. Add IDataSource integration if missing
4. Use PPDMGenericRepository
5. Use entities directly
6. Add missing workflow methods

### Step 5: Create Database Scripts
1. Generate TAB/PK/FK scripts for all tables (6 database types)

### Step 6: Implement Missing Workflows
1. Implement unit agreement approval workflow
2. Implement unit operations management
3. Enhance unit reporting

## Testing Requirements

1. Test unit agreement creation
2. Test participating area creation
3. Test tract participation registration
4. Test unit approval workflow

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Allocation module (for unit allocation)

