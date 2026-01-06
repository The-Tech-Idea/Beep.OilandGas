using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    #region Field Orchestrator DTOs

    /// <summary>
    /// Field list item for selection dropdowns and lists
    /// </summary>
    public class FieldListItem
    {
        public string FieldId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CurrentPhase { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }

    /// <summary>
    /// Request to set the active field
    /// </summary>
    public class SetActiveFieldRequest
    {
        public string FieldId { get; set; } = string.Empty;
        public string? ConnectionName { get; set; }
    }

    /// <summary>
    /// Response from setting the active field
    /// </summary>
    public class SetActiveFieldResponse
    {
        public bool Success { get; set; }
        public string? FieldId { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Simple response containing field data for orchestrator
    /// </summary>
    public class FieldOrchestratorResponse
    {
        public object? Field { get; set; }
        public string? FieldId { get; set; }
        public string? FieldName { get; set; }
    }

    /// <summary>
    /// Summary of field lifecycle across all phases
    /// </summary>
    public class FieldLifecycleSummary
    {
        public string FieldId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        
        // Exploration phase summary
        public int ProspectCount { get; set; }
        public int SeismicSurveyCount { get; set; }
        public int ExploratoryWellCount { get; set; }
        
        // Development phase summary
        public int PoolCount { get; set; }
        public int DevelopmentWellCount { get; set; }
        public int FacilityCount { get; set; }
        public int PipelineCount { get; set; }
        
        // Production phase summary
        public int ProductionWellCount { get; set; }
        public decimal? TotalProductionVolume { get; set; }
        public DateTime? LastProductionDate { get; set; }
        
        // Decommissioning phase summary
        public int AbandonedWellCount { get; set; }
        public int DecommissionedFacilityCount { get; set; }
        
        // Overall status
        public string? CurrentLifecyclePhase { get; set; }
        public DateTime? FieldDiscoveryDate { get; set; }
        public DateTime? FieldStartProductionDate { get; set; }
    }

    /// <summary>
    /// Statistical aggregates for a field
    /// </summary>
    public class FieldStatistics
    {
        public string FieldId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        
        // Well statistics
        public int TotalWellCount { get; set; }
        public int ActiveWellCount { get; set; }
        public int InactiveWellCount { get; set; }
        
        // Production statistics
        public decimal? TotalOilProduction { get; set; }
        public decimal? TotalGasProduction { get; set; }
        public decimal? TotalWaterProduction { get; set; }
        public decimal? AverageDailyProduction { get; set; }
        
        // Reserves statistics
        public decimal? ProvedReserves { get; set; }
        public decimal? ProbableReserves { get; set; }
        public decimal? PossibleReserves { get; set; }
        
        // Facility statistics
        public int TotalFacilityCount { get; set; }
        public int ActiveFacilityCount { get; set; }
        
        // Financial statistics (if available)
        public decimal? TotalInvestment { get; set; }
        public decimal? TotalRevenue { get; set; }
        public decimal? NetPresentValue { get; set; }
        
        // Date ranges
        public DateTime? FirstProductionDate { get; set; }
        public DateTime? LastProductionDate { get; set; }
        public DateTime? StatisticsAsOfDate { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Timeline event for field lifecycle
    /// </summary>
    public class FieldTimelineEvent
    {
        public string EventId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty; // Exploration, Development, Production, Decommissioning
        public string EventDescription { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string? EntityType { get; set; } // PROSPECT, WELL, FACILITY, etc.
        public string? EntityId { get; set; }
        public string? EntityName { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    /// <summary>
    /// Timeline of events across all phases for a field
    /// </summary>
    public class FieldTimeline
    {
        public string FieldId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public List<FieldTimelineEvent> Events { get; set; } = new List<FieldTimelineEvent>();
        public DateTime? EarliestEventDate { get; set; }
        public DateTime? LatestEventDate { get; set; }
        public Dictionary<string, int> EventCountsByPhase { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// Performance metric for field dashboard
    /// </summary>
    public class FieldPerformanceMetric
    {
        public string MetricName { get; set; } = string.Empty;
        public string MetricLabel { get; set; } = string.Empty;
        public string MetricType { get; set; } = string.Empty; // "count", "volume", "percentage", "currency", "date"
        public object? CurrentValue { get; set; }
        public object? PreviousValue { get; set; }
        public object? TargetValue { get; set; }
        public double? ChangePercentage { get; set; }
        public string? Unit { get; set; }
        public string? Phase { get; set; } // Exploration, Development, Production, Decommissioning
        public DateTime? AsOfDate { get; set; }
    }

    /// <summary>
    /// Field dashboard with performance metrics and KPIs
    /// </summary>
    public class FieldDashboard
    {
        public string FieldId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string? CurrentLifecyclePhase { get; set; }
        public DateTime? DashboardAsOfDate { get; set; } = DateTime.UtcNow;

        // Overall performance metrics
        public List<FieldPerformanceMetric> PerformanceMetrics { get; set; } = new List<FieldPerformanceMetric>();

        // Phase-specific summaries
        public FieldDashboardPhaseSummary? ExplorationSummary { get; set; }
        public FieldDashboardPhaseSummary? DevelopmentSummary { get; set; }
        public FieldDashboardPhaseSummary? ProductionSummary { get; set; }
        public FieldDashboardPhaseSummary? DecommissioningSummary { get; set; }

        // Recent activity
        public List<FieldTimelineEvent> RecentEvents { get; set; } = new List<FieldTimelineEvent>();

        // Alerts and warnings
        public List<FieldDashboardAlert> Alerts { get; set; } = new List<FieldDashboardAlert>();

        // Key performance indicators
        public Dictionary<string, object> KPIs { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Phase-specific summary for dashboard
    /// </summary>
    public class FieldDashboardPhaseSummary
    {
        public string PhaseName { get; set; } = string.Empty;
        public int EntityCount { get; set; }
        public Dictionary<string, int> EntityCountsByType { get; set; } = new Dictionary<string, int>();
        public DateTime? LastActivityDate { get; set; }
        public List<FieldPerformanceMetric> PhaseMetrics { get; set; } = new List<FieldPerformanceMetric>();
    }

    /// <summary>
    /// Alert or warning for field dashboard
    /// </summary>
    public class FieldDashboardAlert
    {
        public string AlertId { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty; // "info", "warning", "error", "success"
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Phase { get; set; }
        public DateTime AlertDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    #endregion
}




