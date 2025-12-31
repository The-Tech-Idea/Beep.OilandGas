# Storage Module Enhancement Plan

## Current State Analysis

### Existing Files
- `StorageManager.cs` - Manager class with IDataSource integration
- `StorageFacility.cs` - Storage facility model
- `ServiceUnit.cs` - Service unit model

### Issues Identified
1. **Models in Wrong Location**: StorageFacility, ServiceUnit should be entity classes in Beep.OilandGas.Models
2. **No Service Interface**: Missing IStorageService interface
3. **Missing Workflows**: No storage capacity management, no storage utilization tracking, no storage reporting

## Entity/DTO Migration

### Classes to Move to Beep.OilandGas.Models

**Move to `Beep.OilandGas.Models/Data/Storage/`:**
- `StorageFacility` → `STORAGE_FACILITY` (entity class with PPDM audit columns)
- `TankBattery` → `TANK_BATTERY` (entity class)
- `ServiceUnit` → `SERVICE_UNIT` (entity class)
- `LACTUnit` → `LACT_UNIT` (entity class)

**Create DTOs in `Beep.OilandGas.Models/DTOs/Storage/`:**
- `CreateStorageFacilityRequest`
- `StorageFacilityResponse`
- `CreateTankBatteryRequest`
- `TankBatteryResponse`

**Keep in ProductionAccounting:**
- Storage calculation methods (business logic)

## Service Class Creation

### New Service: StorageService

**Location**: `Beep.OilandGas.ProductionAccounting/Storage/StorageService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IStorageService.cs`

```csharp
public interface IStorageService
{
    Task<STORAGE_FACILITY> RegisterFacilityAsync(CreateStorageFacilityRequest request, string userId, string? connectionName = null);
    Task<STORAGE_FACILITY?> GetFacilityAsync(string facilityId, string? connectionName = null);
    Task<List<STORAGE_FACILITY>> GetFacilitiesAsync(string? connectionName = null);
    
    Task<TANK_BATTERY> RegisterTankBatteryAsync(CreateTankBatteryRequest request, string userId, string? connectionName = null);
    Task<TANK_BATTERY?> GetTankBatteryAsync(string tankBatteryId, string? connectionName = null);
    Task<List<TANK_BATTERY>> GetTankBatteriesByFacilityAsync(string facilityId, string? connectionName = null);
    
    Task<SERVICE_UNIT> RegisterServiceUnitAsync(CreateServiceUnitRequest request, string userId, string? connectionName = null);
    Task<LACT_UNIT> RegisterLACTUnitAsync(CreateLACTUnitRequest request, string userId, string? connectionName = null);
    
    // Missing workflows
    Task<StorageCapacitySummary> GetStorageCapacityAsync(string facilityId, string? connectionName = null);
    Task<StorageUtilizationReport> GetStorageUtilizationAsync(string facilityId, DateTime? asOfDate, string? connectionName = null);
    Task<List<StorageFacility>> GetFacilitiesRequiringMaintenanceAsync(string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for all entity tables
- Uses entities directly

## Database Integration

### Tables Required

**STORAGE_FACILITY**:
- STORAGE_FACILITY_ID (PK)
- FACILITY_NAME
- FACILITY_TYPE
- LOCATION
- CAPACITY
- Standard PPDM audit columns

**TANK_BATTERY**, **SERVICE_UNIT**, **LACT_UNIT**:
- Similar structure
- Links to STORAGE_FACILITY
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("STORAGE_FACILITY");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Storage.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "STORAGE_FACILITY");
```

## Missing Workflows

### 1. Storage Capacity Management
- Track storage capacity
- Monitor capacity utilization
- Capacity planning
- Capacity reporting

### 2. Storage Utilization Tracking
- Track storage utilization over time
- Identify storage bottlenecks
- Utilization trends
- Utilization reporting

### 3. Storage Maintenance Management
- Track storage facility maintenance
- Schedule maintenance
- Maintenance history
- Maintenance reporting

### 4. Storage Integration with Production
- Link storage to production
- Track production receipts to storage
- Track deliveries from storage
- Storage vs production reconciliation

### 5. Storage Reporting
- Storage facility reports
- Tank battery reports
- Storage utilization reports
- Storage capacity reports

## Database Scripts

### Scripts to Create

**For STORAGE_FACILITY, TANK_BATTERY, SERVICE_UNIT, LACT_UNIT**:
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql` (FKs to STORAGE_FACILITY)

## Implementation Steps

### Step 1: Create Entity Classes
1. Create entity classes in `Beep.OilandGas.Models/Data/Storage/`
2. Add standard PPDM audit columns

### Step 2: Create DTOs
1. Create request/response DTOs in `Beep.OilandGas.Models/DTOs/Storage/`

### Step 3: Create Service Interface
1. Create `IStorageService` interface
2. Define all service methods

### Step 4: Refactor StorageManager to StorageService
1. Rename StorageManager.cs to StorageService.cs
2. Update to implement IStorageService
3. Use PPDMGenericRepository
4. Use entities directly
5. Add missing workflow methods

### Step 5: Create Database Scripts
1. Generate TAB/PK/FK scripts for all tables (6 database types)

### Step 6: Implement Missing Workflows
1. Implement storage capacity management
2. Implement storage utilization tracking
3. Implement storage maintenance management
4. Implement storage integration with production
5. Enhance storage reporting

## Testing Requirements

1. Test storage facility registration
2. Test tank battery registration
3. Test service unit registration
4. Test storage capacity tracking
5. Test storage utilization tracking

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- Production module (for production integration)

