# Export Module Enhancement Plan

## Current State Analysis

### Existing Files
- `ExportManager.cs` - Export manager class

### Issues Identified
1. **No Service Interface**: Missing IExportService interface
2. **Missing Workflows**: No export history tracking, no export scheduling, no export reporting

## Entity/DTO Migration

### Classes Status

**Export is typically stateless** - No entity migration needed unless tracking export history

## Service Class Creation

### New Service: ExportService

**Location**: `Beep.OilandGas.ProductionAccounting/Export/ExportService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IExportService.cs`

```csharp
public interface IExportService
{
    Task<ExportResult> ExportToCsvAsync(ExportToCsvRequest request, string? connectionName = null);
    Task<ExportResult> ExportToExcelAsync(ExportToExcelRequest request, string? connectionName = null);
    Task<ExportResult> ExportToJsonAsync(ExportToJsonRequest request, string? connectionName = null);
    
    // Missing workflows
    Task<ExportHistory> GetExportHistoryAsync(string? exportType, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    Task<ExportSchedule> ScheduleExportAsync(ScheduleExportRequest request, string userId, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository to query data for export
- May use EXPORT_HISTORY table if tracking needed

## Database Integration

### Tables (Optional)

**EXPORT_HISTORY** (if tracking needed):
- EXPORT_HISTORY_ID (PK)
- EXPORT_TYPE
- EXPORT_DATE
- EXPORTED_BY
- Standard PPDM audit columns

### PPDMGenericRepository Usage

**For Data Queries** (not for export storage):
```csharp
// Use PPDMGenericRepository to query data for export
var metadata = await _metadata.GetTableMetadataAsync("TABLE_NAME");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "TABLE_NAME");
```

## Missing Workflows

### 1. Export History Tracking
- Track all exports
- Export history search
- Export audit trail

### 2. Export Scheduling
- Schedule exports to run automatically
- Recurring export schedules
- Schedule management

### 3. Export Reporting
- Export summary reports
- Export detail reports

## Database Scripts

### Scripts to Create (if tables needed)

**For EXPORT_HISTORY** (if implemented):
- `EXPORT_HISTORY_TAB.sql` (6 database types)
- `EXPORT_HISTORY_PK.sql`

## Implementation Steps

### Step 1: Create Service Interface
1. Create `IExportService` interface
2. Define all service methods

### Step 2: Refactor ExportManager to ExportService
1. Rename ExportManager.cs to ExportService.cs
2. Update to implement IExportService
3. Use PPDMGenericRepository for data queries
4. Add missing workflow methods

### Step 3: Create Database Scripts (if tables needed)
1. Generate TAB/PK scripts for EXPORT_HISTORY (if implemented)

### Step 4: Implement Missing Workflows
1. Implement export history tracking
2. Implement export scheduling
3. Enhance export reporting

## Testing Requirements

1. Test CSV export
2. Test Excel export
3. Test JSON export
4. Test export history tracking

## Dependencies

- Beep.OilandGas.PPDM39 (for PPDMGenericRepository for data queries)
- All other modules (for data to export)

