using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
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
}
