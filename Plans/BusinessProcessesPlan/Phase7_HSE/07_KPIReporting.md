# Phase 7 — HSE KPI Reporting
## IOGP 2022e Tier 1–4 Frequency Rates, Leading Indicators, Exposure Hours

---

## KPI Definitions

### Lagging Indicators (Outcome-Based)

| KPI ID | Name | Formula | IOGP Ref |
|---|---|---|---|
| HSE-KPI-01 | Tier 1 PSE Rate | (Tier 1 PSE count × 1,000,000) ÷ Exposure Hours | IOGP 2022e §3.1 |
| HSE-KPI-02 | Tier 2 PSE Rate | (Tier 2 PSE count × 1,000,000) ÷ Exposure Hours | IOGP 2022e §3.2 |
| HSE-KPI-03 | Total Recordable Injury Rate (TRIR) | (Recordable injuries × 1,000,000) ÷ Exposure Hours | OSHA 300 |
| HSE-KPI-04 | Lost Time Injury Frequency (LTIF) | (LTI count × 1,000,000) ÷ Exposure Hours | IOGP 2022e §4 |
| HSE-KPI-05 | Fatality Rate | (Fatalities × 100,000,000) ÷ Exposure Hours | IOGP 2022e §4 |

### Leading Indicators (Activity-Based)

| KPI ID | Name | Formula | Basis |
|---|---|---|---|
| HSE-KPI-06 | CA On-Time Completion | Completed CAs on time ÷ Total CAs due × 100 | Internal |
| HSE-KPI-07 | Barrier Degradation Rate | Degraded barriers ÷ Total barriers inspected × 100 | API RP 754 §5 |
| HSE-KPI-08 | HAZOP Closure Rate | Closed deviations ÷ Total deviations × 100 | IEC 61882 |
| HSE-KPI-09 | Safety Observation Frequency | Safety observations filed ÷ Exposure Hours × 1000 | IOGP 2022e §7 |

---

## Exposure Hour Calculation

Exposure hours are the denominator for all frequency rates. They are stored in the `PDEN_SOURCE` table with `SOURCE_TYPE = 'EXPOSURE_HOURS'` and `SOURCE_DATE` as the period month.

```csharp
// IHSEKPIService
Task<double> GetExposureHoursAsync(string fieldId, DateRangeFilter range);
// Queries PDEN_SOURCE WHERE SOURCE_TYPE='EXPOSURE_HOURS' AND FIELD_ID=fieldId
// SUM(SOURCE_QUANTITY) WHERE SOURCE_DATE BETWEEN range.Start AND range.End
```

Manual entry via Blazor form: `ExposureHoursEntry.razor`

---

## IHSEKPIService Interface

```csharp
public interface IHSEKPIService
{
    Task<HSEKPISet> GetKPIsAsync(string fieldId, DateRangeFilter range);
    Task<List<TierRateTrend>> GetTierRateTrendAsync(string fieldId, int months);
    Task<double> GetExposureHoursAsync(string fieldId, DateRangeFilter range);
    Task<double> GetTRIRAsync(string fieldId, DateRangeFilter range);
}

public record HSEKPISet(
    double Tier1PSERate, double Tier2PSERate,
    double TRIR, double LTIF, double FatalityRate,
    double CAOnTimeRate, double BarrierDegradationRate,
    double HAZOPClosureRate, double ExposureHours,
    DateRangeFilter Period);

public record TierRateTrend(string Month, double Tier1Rate, double Tier2Rate);
```

---

## KPI Query Design

### Tier 1 PSE Rate

```csharp
var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "FIELD_ID",      Operator = "=", FilterValue = fieldId },
    new AppFilter { FieldName = "INCIDENT_TIER", Operator = "=", FilterValue = "1" },
    new AppFilter { FieldName = "INCIDENT_DATE", Operator = ">=", FilterValue = range.Start.ToString("yyyy-MM-dd") },
    new AppFilter { FieldName = "INCIDENT_DATE", Operator = "<=", FilterValue = range.End.ToString("yyyy-MM-dd") },
    new AppFilter { FieldName = "ACTIVE_IND",    Operator = "=", FilterValue = "Y" }
};
var tier1Count = (await repo.GetAsync(filters)).Count;
return (tier1Count * 1_000_000.0) / exposureHours;
```

### TRIR

TRIR query: filter `HSE_INCIDENT_BA.INJURY_TYPE IN ('LTI', 'RWC', 'MTC')` joined to incident by date range.

---

## KPI Dashboard (Blazor Component)

```razor
<!-- HSEKPIDashboard.razor — reuses KPICard from Phase 5 -->
<MudGrid>
    <MudItem xs="12" sm="4">
        <KPICard Title="Tier 1 PSE Rate" Value="@_kpis?.Tier1PSERate.ToString("F4")"
                 Subtitle="per 1M exposure hours" Color="@TierColor(_kpis?.Tier1PSERate)" />
    </MudItem>
    <MudItem xs="12" sm="4">
        <KPICard Title="TRIR" Value="@_kpis?.TRIR.ToString("F2")"
                 Subtitle="per 1M exposure hours" />
    </MudItem>
    <MudItem xs="12" sm="4">
        <KPICard Title="CA On-Time %" Value="@($"{_kpis?.CAOnTimeRate:F1}%")"
                 Color="@OnTimeColor(_kpis?.CAOnTimeRate)" />
    </MudItem>
</MudGrid>

<MudText Typo="Typo.subtitle2" Class="mt-4">Tier Rate Trend (12 months)</MudText>
<MudChart ChartType="ChartType.Line" ChartSeries="@_trendSeries"
          XAxisLabels="@_trendLabels" Width="100%" Height="280px" />
```
