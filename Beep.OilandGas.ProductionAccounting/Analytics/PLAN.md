# Analytics Module Enhancement Plan

## Current State Analysis

### Existing Files
- `ProductionAnalytics.cs` - Static analytics methods

### Issues Identified
1. **No Database Integration**: Static class doesn't save analytics results
2. **No Service Class**: Missing IAnalyticsService interface
3. **Missing Workflows**: No analytics dashboard, no analytics reporting, no analytics history

## Entity/DTO Migration

### Classes to Create in Beep.OilandGas.Models

**Create in `Beep.OilandGas.Models/Data/Analytics/`:**
- `ANALYTICS_RESULT` (entity class with PPDM audit columns)
- `ANALYTICS_DASHBOARD` (entity class, optional)

**Create DTOs in `Beep.OilandGas.Models/DTOs/Analytics/`:**
- `AnalyticsRequest`
- `AnalyticsResultResponse`
- `DashboardRequest`
- `DashboardResponse`

**Keep in ProductionAccounting:**
- `ProductionAnalytics` static methods (calculation logic)

## Service Class Creation

### New Service: AnalyticsService

**Location**: `Beep.OilandGas.ProductionAccounting/Analytics/AnalyticsService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IAnalyticsService.cs`

```csharp
public interface IAnalyticsService
{
    Task<AnalyticsResult> CalculateProductionTrendsAsync(ProductionTrendsRequest request, string? connectionName = null);
    Task<AnalyticsResult> CalculateRevenueTrendsAsync(RevenueTrendsRequest request, string? connectionName = null);
    Task<AnalyticsResult> CalculateCostTrendsAsync(CostTrendsRequest request, string? connectionName = null);
    
    Task<ANALYTICS_RESULT> SaveAnalyticsResultAsync(AnalyticsResult result, string userId, string? connectionName = null);
    Task<List<ANALYTICS_RESULT>> GetAnalyticsHistoryAsync(string? analyticsType, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    
    // Missing workflows
    Task<DashboardData> GetDashboardDataAsync(DashboardRequest request, string? connectionName = null);
    Task<List<AnalyticsInsight>> GetAnalyticsInsightsAsync(string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for ANALYTICS_RESULT table
- Calls ProductionAnalytics static methods for calculations
- Saves results to database

## Database Integration

### Tables Required

**ANALYTICS_RESULT**:
- ANALYTICS_RESULT_ID (PK)
- ANALYTICS_TYPE
- CALCULATION_DATE
- RESULT_DATA (JSON or structured)
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("ANALYTICS_RESULT");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Analytics.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "ANALYTICS_RESULT");
```

## Missing Workflows

### 1. Analytics Dashboard
- Real-time analytics dashboard
- Dashboard customization
- Dashboard sharing
- Dashboard reporting

### 2. Analytics Reporting
- Analytics summary reports
- Analytics detail reports
- Analytics trends reports
- Analytics insights

### 3. Analytics History
- Track analytics calculations
- Analytics history search
- Analytics comparison
- Analytics audit trail

## Database Scripts

### Scripts to Create

**For ANALYTICS_RESULT**:
- `ANALYTICS_RESULT_TAB.sql` (6 database types)
- `ANALYTICS_RESULT_PK.sql`
- `ANALYTICS_RESULT_FK.sql`

## Implementation Steps

### Step 1: Create Entity Class
1. Create `ANALYTICS_RESULT` entity in `Beep.OilandGas.Models/Data/Analytics/`
2. Add standard PPDM audit columns

### Step 2: Create DTOs
1. Create request/response DTOs in `Beep.OilandGas.Models/DTOs/Analytics/`

### Step 3: Create Service Interface
1. Create `IAnalyticsService` interface
2. Define all service methods

### Step 4: Create Service Class
1. Create `AnalyticsService.cs`
2. Implement IAnalyticsService
3. Use PPDMGenericRepository
4. Call ProductionAnalytics static methods
5. Save results to database
6. Add missing workflow methods

### Step 5: Create Database Scripts
1. Generate TAB/PK/FK scripts (6 database types)

### Step 6: Implement Missing Workflows
1. Implement analytics dashboard
2. Enhance analytics reporting
3. Implement analytics history

## Testing Requirements

1. Test analytics calculations
2. Test analytics result storage
3. Test analytics dashboard
4. Test analytics reporting

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- All other modules (for data to analyze)

