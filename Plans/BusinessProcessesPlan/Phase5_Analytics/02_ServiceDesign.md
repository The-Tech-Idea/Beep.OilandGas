# Phase 5 — Service Design
## IAnalyticsService Interface and AnalyticsQueryService Implementation

---

## Interface

```csharp
// Beep.OilandGas.Models/Core/Interfaces/IAnalyticsService.cs

public interface IAnalyticsService
{
    // Work Order KPIs
    Task<WorkOrderKPISet>    GetWorkOrderKPIsAsync(string fieldId, DateRangeFilter range);

    // Gate Review KPIs
    Task<GateReviewKPISet>   GetGateReviewKPIsAsync(string fieldId, DateRangeFilter range);

    // HSE KPIs
    Task<HSEKPISet>          GetHSEKPIsAsync(string fieldId, DateRangeFilter range, double exposureHours);

    // Compliance KPIs
    Task<ComplianceKPISet>   GetComplianceKPIsAsync(string fieldId, DateRangeFilter range);

    // Production KPIs
    Task<ProductionKPISet>   GetProductionKPIsAsync(string fieldId, DateRangeFilter range);

    // Combined dashboard summary
    Task<AnalyticsDashboard> GetDashboardSummaryAsync(string fieldId, DateRangeFilter range, double exposureHours);
}
```

---

## KPI Result Types

```csharp
public record WorkOrderKPISet(
    double CompletionRate,
    double OnTimeCompletionRate,
    double MeanTimeToCompleteHours,
    int    OverdueCount,
    double ContractorUtilizationRate);

public record HSEKPISet(
    double Tier1PSERate,
    double Tier2PSERate,
    double TRIR,
    double NearMissReportingRate,
    double MeanDaysToCloseCA);

public record ComplianceKPISet(
    double ObligationOnTimeRate,
    int    OverdueObligations,
    double RoyaltyPaymentTimeliness);

public record AnalyticsDashboard(
    WorkOrderKPISet    WorkOrder,
    GateReviewKPISet   GateReview,
    HSEKPISet          HSE,
    ComplianceKPISet   Compliance,
    ProductionKPISet   Production,
    DateTime           GeneratedAt);
```

---

## AnalyticsQueryService Implementation Skeleton

```csharp
// Beep.OilandGas.PPDM39.DataManagement/Services/Analytics/AnalyticsQueryService.cs

public class AnalyticsQueryService : IAnalyticsService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<AnalyticsQueryService> _logger;

    public AnalyticsQueryService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<AnalyticsQueryService> logger)
    {
        _editor            = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults          = defaults;
        _metadata          = metadata;
        _connectionName    = connectionName;
        _logger            = logger;
    }

    public async Task<WorkOrderKPISet> GetWorkOrderKPIsAsync(string fieldId, DateRangeFilter range)
    {
        Log.Information("Computing WO KPIs for field {FieldId}", fieldId);

        var metadata    = await _metadata.GetTableMetadataAsync("PROJECT");
        var entityType  = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
        var repo        = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, "PROJECT");

        var filters = new List<AppFilter>
        {
            new() { FieldName = "FIELD_ID",      Operator = "=",  FilterValue = fieldId          },
            new() { FieldName = "ACTIVE_IND",    Operator = "=",  FilterValue = "Y"              },
            new() { FieldName = "CREATE_DATE",   Operator = ">=", FilterValue = range.From.ToString("yyyy-MM-dd") },
            new() { FieldName = "CREATE_DATE",   Operator = "<=", FilterValue = range.To.ToString("yyyy-MM-dd")   }
        };

        var allWOs      = await repo.GetAsync(filters);
        // ... compute metrics from allWOs list ...
        // See 03_QueryDesign.md for per-KPI filter patterns

        return new WorkOrderKPISet(
            CompletionRate:           /* computed */ 0,
            OnTimeCompletionRate:     /* computed */ 0,
            MeanTimeToCompleteHours:  /* computed */ 0,
            OverdueCount:             /* computed */ 0,
            ContractorUtilizationRate: /* computed */ 0);
    }

    // Other IAnalyticsService method implementations follow the same pattern
}
```

---

## Service Registration (Program.cs addition)

```csharp
builder.Services.AddScoped<IAnalyticsService>(sp =>
{
    var editor            = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults          = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata          = sp.GetRequiredService<IPPDMMetadataRepository>();
    var logger            = sp.GetRequiredService<ILoggerFactory>().CreateLogger<AnalyticsQueryService>();

    return new AnalyticsQueryService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});
```
