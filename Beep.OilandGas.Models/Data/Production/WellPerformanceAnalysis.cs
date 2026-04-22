using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Production;

public class WellPerformanceAnalysisResponse : ModelEntityBase
{
    public string WellId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double OilRate { get; set; }
    public double GasRate { get; set; }
    public double WaterRate { get; set; }
    public double PotentialRate { get; set; }
    public double CumOil { get; set; }
    public DateTime? LastTestDate { get; set; }
    public List<WellPerformanceFinding> Findings { get; set; } = new();
    public WellPerformanceRecommendation? Recommendation { get; set; }
    public List<WellPerformanceHistoryItem> History { get; set; } = new();
}

public class WellPerformanceFinding : ModelEntityBase
{
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}

public class WellPerformanceRecommendation : ModelEntityBase
{
    public string Action { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public int UpliftBopd { get; set; }
    public string RecommendedLiftStudy { get; set; } = string.Empty;
    public bool HasOperationalFollowUp { get; set; }
    public string SuggestedWorkOrderSubType { get; set; } = string.Empty;
    public decimal? SuggestedAfeBudgetUsd { get; set; }
}

public class WellPerformanceHistoryItem : ModelEntityBase
{
    public string Event { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public string User { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
}

public class PerformanceDeviationRequest : ModelEntityBase
{
    public string? Note { get; set; }
    public string? RecommendationAction { get; set; }
    public string? RecommendationRationale { get; set; }
    public int EstimatedUpliftBopd { get; set; }
    public string? Severity { get; set; }
    public List<string> Findings { get; set; } = new();
}

public class PerformanceDeviationResult : ModelEntityBase
{
    public bool Success { get; set; }
    public string WellId { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public DateTime LoggedAtUtc { get; set; }
    public string? Message { get; set; }
}