using MudBlazor;

namespace Beep.OilandGas.Web.Components.Shared;

public sealed class CalculationKpiMetric
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public Color ValueColor { get; init; } = Color.Primary;
    public string TrendIcon { get; init; } = string.Empty;
    public Color TrendColor { get; init; } = Color.Success;
    public string TrendText { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
}

public sealed class CalculationFactItem
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public sealed class OptimizationDeltaItem
{
    public string Label { get; init; } = string.Empty;
    public string BaselineValue { get; init; } = string.Empty;
    public string OptimizedValue { get; init; } = string.Empty;
    public string DeltaValue { get; init; } = string.Empty;
    public Color DeltaColor { get; init; } = Color.Info;
}

public sealed class RecommendationFactItem
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public Color ValueColor { get; init; } = Color.Primary;
}