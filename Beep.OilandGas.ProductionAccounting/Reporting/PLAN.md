# Reporting Module Enhancement Plan

## Current State Analysis

### Existing Files
- `ReportManager.cs` - Manager class with IDataSource integration
- `ReportGenerator.cs` - Static report generation methods
- `ReportModels.cs` - Contains report model classes

### Issues Identified
1. **Models in Wrong Location**: ReportModels.cs classes may need to be DTOs in Beep.OilandGas.Models
2. **No Service Interface**: Missing IReportingService interface
3. **Missing Workflows**: No report scheduling, no report distribution, no report history tracking

## Entity/DTO Migration

### Classes Status

**Report Models** - These are typically DTOs, not entities:
- Keep in `Beep.OilandGas.Models/DTOs/Reporting/` as DTOs
- Not database entities (reports are generated, not stored)

**No Entity Migration Needed** - Reports are generated on-demand

## Service Class Creation

### New Service: ReportingService

**Location**: `Beep.OilandGas.ProductionAccounting/Reporting/ReportingService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IReportingService.cs`

```csharp
public interface IReportingService
{
    Task<ReportResult> GenerateOperationalReportAsync(GenerateOperationalReportRequest request, string? connectionName = null);
    Task<ReportResult> GenerateFinancialReportAsync(GenerateFinancialReportRequest request, string? connectionName = null);
    Task<ReportResult> GenerateRoyaltyStatementAsync(GenerateRoyaltyStatementRequest request, string? connectionName = null);
    Task<ReportResult> GenerateJIBStatementAsync(GenerateJIBStatementRequest request, string? connectionName = null);
    
    // Missing workflows
    Task<ReportSchedule> ScheduleReportAsync(ScheduleReportRequest request, string userId, string? connectionName = null);
    Task<List<ReportSchedule>> GetScheduledReportsAsync(string? connectionName = null);
    Task<ReportDistributionResult> DistributeReportAsync(string reportId, ReportDistributionRequest request, string userId, string? connectionName = null);
    Task<List<ReportHistory>> GetReportHistoryAsync(string? reportType, DateTime? startDate, DateTime? endDate, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository to query data for reports
- Calls ReportGenerator static methods for report generation
- May need REPORT_HISTORY table for tracking

## Database Integration

### Tables May Be Needed

**REPORT_HISTORY** (optional, for tracking):
- REPORT_HISTORY_ID (PK)
- REPORT_TYPE
- REPORT_DATE
- GENERATED_BY
- Standard PPDM audit columns

**REPORT_SCHEDULE** (optional, for scheduling):
- REPORT_SCHEDULE_ID (PK)
- REPORT_TYPE
- SCHEDULE_FREQUENCY
- NEXT_RUN_DATE
- Standard PPDM audit columns

### PPDMGenericRepository Usage

**For Data Queries** (not for report storage):
```csharp
// Use PPDMGenericRepository to query data for reports
var metadata = await _metadata.GetTableMetadataAsync("PRODUCTION");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Production.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "PRODUCTION");
```

## Missing Workflows

### 1. Report Scheduling
- Schedule reports to run automatically
- Recurring report schedules
- Schedule management
- Schedule reporting

### 2. Report Distribution
- Distribute reports to recipients
- Email report distribution
- Report distribution history
- Distribution management

### 3. Report History Tracking
- Track all generated reports
- Report history search
- Report history reporting
- Report audit trail

### 4. Report Templates
- Create report templates
- Template management
- Template versioning
- Template reporting

### 5. Report Export
- Export reports to various formats (PDF, Excel, CSV)
- Export scheduling
- Export history

## Database Scripts

### Scripts to Create (if tables needed)

**For REPORT_HISTORY, REPORT_SCHEDULE** (if implemented):
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql`

## Implementation Steps

### Step 1: Organize Report DTOs
1. Move report models to `Beep.OilandGas.Models/DTOs/Reporting/` as DTOs
2. Create request/response DTOs for report generation

### Step 2: Create Service Interface
1. Create `IReportingService` interface
2. Define all service methods

### Step 3: Refactor ReportManager to ReportingService
1. Rename ReportManager.cs to ReportingService.cs
2. Update to implement IReportingService
3. Use PPDMGenericRepository for data queries
4. Call ReportGenerator static methods
5. Add missing workflow methods

### Step 4: Create Database Scripts (if tables needed)
1. Generate TAB/PK/FK scripts for REPORT_HISTORY, REPORT_SCHEDULE (if implemented)

### Step 5: Implement Missing Workflows
1. Implement report scheduling
2. Implement report distribution
3. Implement report history tracking
4. Implement report templates
5. Enhance report export

## Testing Requirements

1. Test report generation
2. Test report scheduling
3. Test report distribution
4. Test report history tracking

## Dependencies

- Beep.OilandGas.Models (for DTOs)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository for data queries)
- All other modules (for data to report on)

