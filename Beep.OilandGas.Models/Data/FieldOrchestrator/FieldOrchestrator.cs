using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    #region Field Orchestrator DTOs

    /// <summary>
    /// Field list item for selection dropdowns and lists
    /// </summary>
    public class FieldListItem : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string? CurrentPhaseValue;

        public string? CurrentPhase

        {

            get { return this.CurrentPhaseValue; }

            set { SetProperty(ref CurrentPhaseValue, value); }

        }
        private DateTime? LastModifiedDateValue;

        public DateTime? LastModifiedDate

        {

            get { return this.LastModifiedDateValue; }

            set { SetProperty(ref LastModifiedDateValue, value); }

        }
    }

    /// <summary>
    /// Request to set the active field
    /// </summary>
    public class SetActiveFieldRequest : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }

    /// <summary>
    /// Response from setting the active field
    /// </summary>
    public class SetActiveFieldResponse : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }

    /// <summary>
    /// Simple response containing field data for orchestrator
    /// </summary>
    public class FieldOrchestratorResponse : ModelEntityBase
    {
        private object? FieldValue;

        public object? Field

        {

            get { return this.FieldValue; }

            set { SetProperty(ref FieldValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? FieldNameValue;

        public string? FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
    }

    /// <summary>
    /// Summary of field lifecycle across all phases
    /// </summary>
    public class FieldLifecycleSummary : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        
        // Exploration phase summary
        private int ProspectCountValue;

        public int ProspectCount

        {

            get { return this.ProspectCountValue; }

            set { SetProperty(ref ProspectCountValue, value); }

        }
        private int SeismicSurveyCountValue;

        public int SeismicSurveyCount

        {

            get { return this.SeismicSurveyCountValue; }

            set { SetProperty(ref SeismicSurveyCountValue, value); }

        }
        private int ExploratoryWellCountValue;

        public int ExploratoryWellCount

        {

            get { return this.ExploratoryWellCountValue; }

            set { SetProperty(ref ExploratoryWellCountValue, value); }

        }
        
        // Development phase summary
        private int PoolCountValue;

        public int PoolCount

        {

            get { return this.PoolCountValue; }

            set { SetProperty(ref PoolCountValue, value); }

        }
        private int DevelopmentWellCountValue;

        public int DevelopmentWellCount

        {

            get { return this.DevelopmentWellCountValue; }

            set { SetProperty(ref DevelopmentWellCountValue, value); }

        }
        private int FacilityCountValue;

        public int FacilityCount

        {

            get { return this.FacilityCountValue; }

            set { SetProperty(ref FacilityCountValue, value); }

        }
        private int PipelineCountValue;

        public int PipelineCount

        {

            get { return this.PipelineCountValue; }

            set { SetProperty(ref PipelineCountValue, value); }

        }
        
        // Production phase summary
        private int ProductionWellCountValue;

        public int ProductionWellCount

        {

            get { return this.ProductionWellCountValue; }

            set { SetProperty(ref ProductionWellCountValue, value); }

        }
        private decimal? TotalProductionVolumeValue;

        public decimal? TotalProductionVolume

        {

            get { return this.TotalProductionVolumeValue; }

            set { SetProperty(ref TotalProductionVolumeValue, value); }

        }
        private DateTime? LastProductionDateValue;

        public DateTime? LastProductionDate

        {

            get { return this.LastProductionDateValue; }

            set { SetProperty(ref LastProductionDateValue, value); }

        }
        
        // Decommissioning phase summary
        private int AbandonedWellCountValue;

        public int AbandonedWellCount

        {

            get { return this.AbandonedWellCountValue; }

            set { SetProperty(ref AbandonedWellCountValue, value); }

        }
        private int DecommissionedFacilityCountValue;

        public int DecommissionedFacilityCount

        {

            get { return this.DecommissionedFacilityCountValue; }

            set { SetProperty(ref DecommissionedFacilityCountValue, value); }

        }
        
        // Overall status
        private string? CurrentLifecyclePhaseValue;

        public string? CurrentLifecyclePhase

        {

            get { return this.CurrentLifecyclePhaseValue; }

            set { SetProperty(ref CurrentLifecyclePhaseValue, value); }

        }
        private DateTime? FieldDiscoveryDateValue;

        public DateTime? FieldDiscoveryDate

        {

            get { return this.FieldDiscoveryDateValue; }

            set { SetProperty(ref FieldDiscoveryDateValue, value); }

        }
        private DateTime? FieldStartProductionDateValue;

        public DateTime? FieldStartProductionDate

        {

            get { return this.FieldStartProductionDateValue; }

            set { SetProperty(ref FieldStartProductionDateValue, value); }

        }
    }

    /// <summary>
    /// Statistical aggregates for a field
    /// </summary>
    public class FieldStatistics : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        
        // Well statistics
        private int TotalWellCountValue;

        public int TotalWellCount

        {

            get { return this.TotalWellCountValue; }

            set { SetProperty(ref TotalWellCountValue, value); }

        }
        private int ActiveWellCountValue;

        public int ActiveWellCount

        {

            get { return this.ActiveWellCountValue; }

            set { SetProperty(ref ActiveWellCountValue, value); }

        }
        private int InactiveWellCountValue;

        public int InactiveWellCount

        {

            get { return this.InactiveWellCountValue; }

            set { SetProperty(ref InactiveWellCountValue, value); }

        }
        
        // Production statistics
        private decimal? TotalOilProductionValue;

        public decimal? TotalOilProduction

        {

            get { return this.TotalOilProductionValue; }

            set { SetProperty(ref TotalOilProductionValue, value); }

        }
        private decimal? TotalGasProductionValue;

        public decimal? TotalGasProduction

        {

            get { return this.TotalGasProductionValue; }

            set { SetProperty(ref TotalGasProductionValue, value); }

        }
        private decimal? TotalWaterProductionValue;

        public decimal? TotalWaterProduction

        {

            get { return this.TotalWaterProductionValue; }

            set { SetProperty(ref TotalWaterProductionValue, value); }

        }
        private decimal? AverageDailyProductionValue;

        public decimal? AverageDailyProduction

        {

            get { return this.AverageDailyProductionValue; }

            set { SetProperty(ref AverageDailyProductionValue, value); }

        }
        
        // Reserves statistics
        private decimal? ProvedReservesValue;

        public decimal? ProvedReserves

        {

            get { return this.ProvedReservesValue; }

            set { SetProperty(ref ProvedReservesValue, value); }

        }
        private decimal? ProbableReservesValue;

        public decimal? ProbableReserves

        {

            get { return this.ProbableReservesValue; }

            set { SetProperty(ref ProbableReservesValue, value); }

        }
        private decimal? PossibleReservesValue;

        public decimal? PossibleReserves

        {

            get { return this.PossibleReservesValue; }

            set { SetProperty(ref PossibleReservesValue, value); }

        }
        
        // Facility statistics
        private int TotalFacilityCountValue;

        public int TotalFacilityCount

        {

            get { return this.TotalFacilityCountValue; }

            set { SetProperty(ref TotalFacilityCountValue, value); }

        }
        private int ActiveFacilityCountValue;

        public int ActiveFacilityCount

        {

            get { return this.ActiveFacilityCountValue; }

            set { SetProperty(ref ActiveFacilityCountValue, value); }

        }
        
        // Financial statistics (if available)
        private decimal? TotalInvestmentValue;

        public decimal? TotalInvestment

        {

            get { return this.TotalInvestmentValue; }

            set { SetProperty(ref TotalInvestmentValue, value); }

        }
        private decimal? TotalRevenueValue;

        public decimal? TotalRevenue

        {

            get { return this.TotalRevenueValue; }

            set { SetProperty(ref TotalRevenueValue, value); }

        }
        private decimal? NetPresentValueValue;

        public decimal? NetPresentValue

        {

            get { return this.NetPresentValueValue; }

            set { SetProperty(ref NetPresentValueValue, value); }

        }
        
        // Date ranges
        private DateTime? FirstProductionDateValue;

        public DateTime? FirstProductionDate

        {

            get { return this.FirstProductionDateValue; }

            set { SetProperty(ref FirstProductionDateValue, value); }

        }
        private DateTime? LastProductionDateValue;

        public DateTime? LastProductionDate

        {

            get { return this.LastProductionDateValue; }

            set { SetProperty(ref LastProductionDateValue, value); }

        }
        private DateTime? StatisticsAsOfDateValue = DateTime.UtcNow;

        public DateTime? StatisticsAsOfDate

        {

            get { return this.StatisticsAsOfDateValue; }

            set { SetProperty(ref StatisticsAsOfDateValue, value); }

        }
    }

    /// <summary>
    /// Timeline event for field lifecycle
    /// </summary>
    public class FieldTimelineEvent : ModelEntityBase
    {
        private string EventIdValue = string.Empty;

        public string EventId

        {

            get { return this.EventIdValue; }

            set { SetProperty(ref EventIdValue, value); }

        }
        private string EventTypeValue = string.Empty;

        public string EventType

        {

            get { return this.EventTypeValue; }

            set { SetProperty(ref EventTypeValue, value); }

        } // Exploration, Development, Production, Decommissioning
        private string EventDescriptionValue = string.Empty;

        public string EventDescription

        {

            get { return this.EventDescriptionValue; }

            set { SetProperty(ref EventDescriptionValue, value); }

        }
        private DateTime EventDateValue;

        public DateTime EventDate

        {

            get { return this.EventDateValue; }

            set { SetProperty(ref EventDateValue, value); }

        }
        private string? EntityTypeValue;

        public string? EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        } // PROSPECT, WELL, FACILITY, etc.
        private string? EntityIdValue;

        public string? EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string? EntityNameValue;

        public string? EntityName

        {

            get { return this.EntityNameValue; }

            set { SetProperty(ref EntityNameValue, value); }

        }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    /// <summary>
    /// Timeline of events across all phases for a field
    /// </summary>
    public class FieldTimeline : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private List<FieldTimelineEvent> EventsValue = new List<FieldTimelineEvent>();

        public List<FieldTimelineEvent> Events

        {

            get { return this.EventsValue; }

            set { SetProperty(ref EventsValue, value); }

        }
        private DateTime? EarliestEventDateValue;

        public DateTime? EarliestEventDate

        {

            get { return this.EarliestEventDateValue; }

            set { SetProperty(ref EarliestEventDateValue, value); }

        }
        private DateTime? LatestEventDateValue;

        public DateTime? LatestEventDate

        {

            get { return this.LatestEventDateValue; }

            set { SetProperty(ref LatestEventDateValue, value); }

        }
        public Dictionary<string, int> EventCountsByPhase { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// Performance metric for field dashboard
    /// </summary>
    public class FieldPerformanceMetric : ModelEntityBase
    {
        private string MetricNameValue = string.Empty;

        public string MetricName

        {

            get { return this.MetricNameValue; }

            set { SetProperty(ref MetricNameValue, value); }

        }
        private string MetricLabelValue = string.Empty;

        public string MetricLabel

        {

            get { return this.MetricLabelValue; }

            set { SetProperty(ref MetricLabelValue, value); }

        }
        private string MetricTypeValue = string.Empty;

        public string MetricType

        {

            get { return this.MetricTypeValue; }

            set { SetProperty(ref MetricTypeValue, value); }

        } // "count", "volume", "percentage", "currency", "date"
        private object? CurrentValueValue;

        public object? CurrentValue

        {

            get { return this.CurrentValueValue; }

            set { SetProperty(ref CurrentValueValue, value); }

        }
        private object? PreviousValueValue;

        public object? PreviousValue

        {

            get { return this.PreviousValueValue; }

            set { SetProperty(ref PreviousValueValue, value); }

        }
        private object? TargetValueValue;

        public object? TargetValue

        {

            get { return this.TargetValueValue; }

            set { SetProperty(ref TargetValueValue, value); }

        }
        private double? ChangePercentageValue;

        public double? ChangePercentage

        {

            get { return this.ChangePercentageValue; }

            set { SetProperty(ref ChangePercentageValue, value); }

        }
        private string? UnitValue;

        public string? Unit

        {

            get { return this.UnitValue; }

            set { SetProperty(ref UnitValue, value); }

        }
        private string? PhaseValue;

        public string? Phase

        {

            get { return this.PhaseValue; }

            set { SetProperty(ref PhaseValue, value); }

        } // Exploration, Development, Production, Decommissioning
        private DateTime? AsOfDateValue;

        public DateTime? AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

        }
    }

    /// <summary>
    /// Field dashboard with performance metrics and KPIs
    /// </summary>
    public class FieldDashboard : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string? CurrentLifecyclePhaseValue;

        public string? CurrentLifecyclePhase

        {

            get { return this.CurrentLifecyclePhaseValue; }

            set { SetProperty(ref CurrentLifecyclePhaseValue, value); }

        }
        private DateTime? DashboardAsOfDateValue = DateTime.UtcNow;

        public DateTime? DashboardAsOfDate

        {

            get { return this.DashboardAsOfDateValue; }

            set { SetProperty(ref DashboardAsOfDateValue, value); }

        }

        // Overall performance metrics
        private List<FieldPerformanceMetric> PerformanceMetricsValue = new List<FieldPerformanceMetric>();

        public List<FieldPerformanceMetric> PerformanceMetrics

        {

            get { return this.PerformanceMetricsValue; }

            set { SetProperty(ref PerformanceMetricsValue, value); }

        }

        // Phase-specific summaries
        private FieldDashboardPhaseSummary? ExplorationSummaryValue;

        public FieldDashboardPhaseSummary? ExplorationSummary

        {

            get { return this.ExplorationSummaryValue; }

            set { SetProperty(ref ExplorationSummaryValue, value); }

        }
        private FieldDashboardPhaseSummary? DevelopmentSummaryValue;

        public FieldDashboardPhaseSummary? DevelopmentSummary

        {

            get { return this.DevelopmentSummaryValue; }

            set { SetProperty(ref DevelopmentSummaryValue, value); }

        }
        private FieldDashboardPhaseSummary? ProductionSummaryValue;

        public FieldDashboardPhaseSummary? ProductionSummary

        {

            get { return this.ProductionSummaryValue; }

            set { SetProperty(ref ProductionSummaryValue, value); }

        }
        private FieldDashboardPhaseSummary? DecommissioningSummaryValue;

        public FieldDashboardPhaseSummary? DecommissioningSummary

        {

            get { return this.DecommissioningSummaryValue; }

            set { SetProperty(ref DecommissioningSummaryValue, value); }

        }

        // Recent activity
        private List<FieldTimelineEvent> RecentEventsValue = new List<FieldTimelineEvent>();

        public List<FieldTimelineEvent> RecentEvents

        {

            get { return this.RecentEventsValue; }

            set { SetProperty(ref RecentEventsValue, value); }

        }

        // Alerts and warnings
        private List<FieldDashboardAlert> AlertsValue = new List<FieldDashboardAlert>();

        public List<FieldDashboardAlert> Alerts

        {

            get { return this.AlertsValue; }

            set { SetProperty(ref AlertsValue, value); }

        }

        // Key performance indicators
        public Dictionary<string, object> KPIs { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Phase-specific summary for dashboard
    /// </summary>
    public class FieldDashboardPhaseSummary : ModelEntityBase
    {
        private string PhaseNameValue = string.Empty;

        public string PhaseName

        {

            get { return this.PhaseNameValue; }

            set { SetProperty(ref PhaseNameValue, value); }

        }
        private int EntityCountValue;

        public int EntityCount

        {

            get { return this.EntityCountValue; }

            set { SetProperty(ref EntityCountValue, value); }

        }
        public Dictionary<string, int> EntityCountsByType { get; set; } = new Dictionary<string, int>();
        private DateTime? LastActivityDateValue;

        public DateTime? LastActivityDate

        {

            get { return this.LastActivityDateValue; }

            set { SetProperty(ref LastActivityDateValue, value); }

        }
        private List<FieldPerformanceMetric> PhaseMetricsValue = new List<FieldPerformanceMetric>();

        public List<FieldPerformanceMetric> PhaseMetrics

        {

            get { return this.PhaseMetricsValue; }

            set { SetProperty(ref PhaseMetricsValue, value); }

        }
    }

    /// <summary>
    /// Alert or warning for field dashboard
    /// </summary>
    public class FieldDashboardAlert : ModelEntityBase
    {
        private string AlertIdValue = string.Empty;

        public string AlertId

        {

            get { return this.AlertIdValue; }

            set { SetProperty(ref AlertIdValue, value); }

        }
        private string AlertTypeValue = string.Empty;

        public string AlertType

        {

            get { return this.AlertTypeValue; }

            set { SetProperty(ref AlertTypeValue, value); }

        } // "info", "warning", "error", "success"
        private string TitleValue = string.Empty;

        public string Title

        {

            get { return this.TitleValue; }

            set { SetProperty(ref TitleValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? PhaseValue;

        public string? Phase

        {

            get { return this.PhaseValue; }

            set { SetProperty(ref PhaseValue, value); }

        }
        private DateTime AlertDateValue;

        public DateTime AlertDate

        {

            get { return this.AlertDateValue; }

            set { SetProperty(ref AlertDateValue, value); }

        }
        private bool IsActiveValue = true;

        public bool IsActive

        {

            get { return this.IsActiveValue; }

            set { SetProperty(ref IsActiveValue, value); }

        }
    }

    #endregion
}







