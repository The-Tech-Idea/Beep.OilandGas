# Data Quality Services

## Overview

The data quality services provide comprehensive data quality assessment and monitoring capabilities. The framework includes `PPDMDataQualityService` for quality metrics and `PPDMDataQualityDashboardService` for real-time dashboards.

## PPDMDataQualityService

### Key Features

- **Data Completeness Scoring**: Calculates completeness scores for entities
- **Field-Level Quality Metrics**: Quality metrics for individual fields
- **Table-Level Quality Metrics**: Quality metrics for entire tables
- **Quality Issue Identification**: Identifies and categorizes quality issues
- **Overall Quality Scoring**: Provides overall quality scores

### Key Methods

#### CalculateQualityScoreAsync

Calculates data quality score for an entity.

```csharp
public async Task<DataQualityScore> CalculateQualityScoreAsync(object entity, string tableName)
```

**Example:**
```csharp
var qualityService = new PPDMDataQualityService(editor, commonColumnHandler, defaults, metadata);
var score = await qualityService.CalculateQualityScoreAsync(well, "WELL");

Console.WriteLine($"Overall Score: {score.OverallScore:P2}");
foreach (var fieldScore in score.FieldScores)
{
    Console.WriteLine($"{fieldScore.Key}: {fieldScore.Value:P2}");
}
```

#### CalculateTableQualityAsync

Calculates quality metrics for an entire table.

```csharp
public async Task<TableQualityMetrics> CalculateTableQualityAsync(string tableName, List<AppFilter> filters = null)
```

**Example:**
```csharp
var tableMetrics = await qualityService.CalculateTableQualityAsync("WELL");
Console.WriteLine($"Table Completeness: {tableMetrics.CompletenessScore:P2}");
Console.WriteLine($"Total Records: {tableMetrics.TotalRecords}");
Console.WriteLine($"Quality Issues: {tableMetrics.IssueCount}");
```

## PPDMDataQualityDashboardService

### Key Features

- **Real-Time Quality Metrics**: Real-time quality metrics aggregation
- **Quality Trends**: Quality trends over time
- **Quality Alerts**: Alerts for quality issues
- **Field-Level Quality Scores**: Field-level quality scoring
- **Dashboard Data Aggregation**: Aggregated data for dashboards

### Key Methods

#### GetDashboardDataAsync

Gets aggregated dashboard data.

```csharp
public async Task<QualityDashboardData> GetDashboardDataAsync(string tableName = null)
```

**Example:**
```csharp
var dashboardService = new PPDMDataQualityDashboardService(editor, commonColumnHandler, defaults, metadata, qualityService);
var dashboardData = await dashboardService.GetDashboardDataAsync("WELL");

Console.WriteLine($"Overall Quality: {dashboardData.OverallQuality:P2}");
Console.WriteLine($"Trend: {dashboardData.QualityTrend}");
```

## Quality Metrics

### DataQualityScore

```csharp
public class DataQualityScore
{
    public object Entity { get; set; }
    public string TableName { get; set; }
    public double OverallScore { get; set; }
    public Dictionary<string, double> FieldScores { get; set; }
    public List<DataQualityIssue> Issues { get; set; }
}
```

### TableQualityMetrics

```csharp
public class TableQualityMetrics
{
    public string TableName { get; set; }
    public double CompletenessScore { get; set; }
    public int TotalRecords { get; set; }
    public int IssueCount { get; set; }
    public List<DataQualityIssue> Issues { get; set; }
}
```

## Best Practices

### 1. Regular Quality Assessment

```csharp
// ✅ Good - Regular quality checks
var metrics = await qualityService.CalculateTableQualityAsync("WELL");
if (metrics.CompletenessScore < 0.8)
{
    _logger.LogWarning("Table quality below threshold: {Score}", metrics.CompletenessScore);
}
```

### 2. Monitor Quality Trends

```csharp
// ✅ Good - Monitor quality trends
var dashboardData = await dashboardService.GetDashboardDataAsync("WELL");
if (dashboardData.QualityTrend == QualityTrend.Decreasing)
{
    // Investigate quality degradation
}
```

## Related Documentation

- [Overview](beep-dataaccess-overview.md) - Framework overview
- [Data Validation](beep-dataaccess-validation.md) - Validation service
- [Usage Examples](beep-dataaccess-examples.md) - Practical examples

