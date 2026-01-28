using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class QualityDashboardData : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private DateTime LastUpdatedValue;

        public DateTime LastUpdated

        {

            get { return this.LastUpdatedValue; }

            set { SetProperty(ref LastUpdatedValue, value); }

        }
        private double OverallQualityScoreValue;

        public double OverallQualityScore

        {

            get { return this.OverallQualityScoreValue; }

            set { SetProperty(ref OverallQualityScoreValue, value); }

        }
        private DataQualityMetrics CurrentMetricsValue;

        public DataQualityMetrics CurrentMetrics

        {

            get { return this.CurrentMetricsValue; }

            set { SetProperty(ref CurrentMetricsValue, value); }

        }
        private List<QualityTrend> RecentTrendsValue = new List<QualityTrend>();

        public List<QualityTrend> RecentTrends

        {

            get { return this.RecentTrendsValue; }

            set { SetProperty(ref RecentTrendsValue, value); }

        }
        private List<QualityAlert> ActiveAlertsValue = new List<QualityAlert>();

        public List<QualityAlert> ActiveAlerts

        {

            get { return this.ActiveAlertsValue; }

            set { SetProperty(ref ActiveAlertsValue, value); }

        }
        public Dictionary<string, double> FieldQualityScores { get; set; } = new Dictionary<string, double>();
    }
}
