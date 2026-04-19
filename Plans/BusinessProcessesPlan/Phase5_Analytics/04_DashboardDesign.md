# Phase 5 — Dashboard Design
## Blazor Analytics Dashboard Layout and MudBlazor Chart Components

---

## Page Structure: AnalyticsDashboard.razor

```razor
@page "/ppdm39/analytics"
@attribute [Authorize]
@inject ApiClient ApiClient
@inject IFieldOrchestrator FieldOrchestrator

<MudGrid Spacing="3">

    <!-- Header row: date range filter -->
    <MudItem xs="12">
        <MudPaper Elevation="1" Class="pa-3">
            <MudGrid Spacing="2" AlignItems="Center">
                <MudItem xs="3">
                    <MudSelect T="string" Label="Date Range" @bind-Value="_range" ValueChanged="LoadAsync">
                        <MudSelectItem Value='"30"'>Last 30 days</MudSelectItem>
                        <MudSelectItem Value='"90"'>Last 90 days</MudSelectItem>
                        <MudSelectItem Value='"365"'>Last 365 days</MudSelectItem>
                    </MudSelect>
                </MudItem>
                <MudItem xs="3">
                    <MudNumericField T="double" Label="Exposure Hours (HSE)" @bind-Value="_exposureHours" />
                </MudItem>
                <MudItem xs="2">
                    <MudButton Variant="Variant.Filled" OnClick="LoadAsync">Refresh</MudButton>
                </MudItem>
            </MudGrid>
        </MudPaper>
    </MudItem>

    <!-- KPI Card row 1: WO + Gate Review -->
    <MudItem xs="12" md="6">
        <KPICard Title="Work Orders" KPISet="@_dashboard?.WorkOrder" />
    </MudItem>
    <MudItem xs="12" md="6">
        <KPICard Title="Gate Reviews" KPISet="@_dashboard?.GateReview" />
    </MudItem>

    <!-- KPI Card row 2: HSE + Compliance -->
    <MudItem xs="12" md="6">
        <KPICard Title="HSE" KPISet="@_dashboard?.HSE" IsHSE="true" />
    </MudItem>
    <MudItem xs="12" md="6">
        <KPICard Title="Compliance" KPISet="@_dashboard?.Compliance" />
    </MudItem>

    <!-- Trend chart row -->
    <MudItem xs="12">
        <KPITrendChart Title="Monthly Production (BOE)" DataPoints="@_productionTrend" />
    </MudItem>

</MudGrid>
```

---

## KPICard Component

```razor
<!-- Beep.OilandGas.Web/Components/Analytics/KPICard.razor -->
[Parameter] public string Title { get; set; }
[Parameter] public object? KPISet { get; set; }   // Cast to specific type based on Title
[Parameter] public bool IsHSE { get; set; }

<MudCard Elevation="2">
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">@Title</MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        @if (KPISet is WorkOrderKPISet wo)
        {
            <MudGrid Spacing="2">
                <MudItem xs="6">
                    <MudText Typo="Typo.caption">Completion Rate</MudText>
                    <MudText Typo="Typo.h5" Color="@RateColor(wo.CompletionRate)">@wo.CompletionRate.ToString("F1")%</MudText>
                </MudItem>
                <MudItem xs="6">
                    <MudText Typo="Typo.caption">Overdue</MudText>
                    <MudText Typo="Typo.h5" Color="@(wo.OverdueCount > 0 ? Color.Error : Color.Success)">@wo.OverdueCount</MudText>
                </MudItem>
            </MudGrid>
        }
        <!-- Similar for HSEKPISet, ComplianceKPISet, etc. -->
    </MudCardContent>
</MudCard>
```

---

## KPITrendChart Component

Uses `MudChart` line chart for time-series data:

```razor
<!-- KPITrendChart.razor -->
[Parameter] public string Title { get; set; }
[Parameter] public List<(DateTime Period, double Value)> DataPoints { get; set; } = [];

<MudChart ChartType="ChartType.Line"
          ChartSeries="@_series"
          XAxisLabels="@_labels"
          Width="100%"
          Height="300px">
</MudChart>

@code {
    private List<ChartSeries> _series = [];
    private string[] _labels = [];

    protected override void OnParametersSet()
    {
        _labels  = DataPoints.Select(p => p.Period.ToString("MMM yy")).ToArray();
        _series  =
        [
            new ChartSeries
            {
                Name = Title,
                Data = DataPoints.Select(p => p.Value).ToArray()
            }
        ];
    }
}
```

**Note**: Verify `MudChart` API (ChartType enum values, ChartSeries properties) against your MudBlazor version before implementation.

---

## AnalyticsController (API)

```csharp
[ApiController]
[Route("api/field/current/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<AnalyticsDashboard>> GetDashboardAsync(
        [FromQuery] int days = 90,
        [FromQuery] double exposureHours = 200000)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId;
        var range   = new DateRangeFilter(DateTime.UtcNow.AddDays(-days), DateTime.UtcNow);
        var result  = await _analytics.GetDashboardSummaryAsync(fieldId, range, exposureHours);
        return Ok(result);
    }

    [HttpGet("production-trend")]
    public async Task<ActionResult<List<object>>> GetProductionTrendAsync([FromQuery] int days = 365)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId;
        var range   = new DateRangeFilter(DateTime.UtcNow.AddDays(-days), DateTime.UtcNow);
        var result  = await _analytics.GetProductionKPIsAsync(fieldId, range);
        return Ok(result);
    }
}
```

---

## Nav Sidebar Entry

```razor
<MudNavLink Href="/ppdm39/analytics" Icon="@Icons.Material.Filled.Analytics">
    Analytics Dashboard
</MudNavLink>
```
