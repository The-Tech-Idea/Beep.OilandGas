using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Financial Dashboard
    /// </summary>
    public partial class FinancialDashboard : ModelEntityBase
    {
        private System.DateTime AsOfDateValue;
        /// <summary>
        /// Gets or sets the as of date.
        /// </summary>
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private System.DateTime GeneratedDateValue;
        /// <summary>
        /// Gets or sets the generated date.
        /// </summary>
        public System.DateTime GeneratedDate
        {
            get { return this.GeneratedDateValue; }
            set { SetProperty(ref GeneratedDateValue, value); }
        }

        private List<KPI> KPIsValue = new List<KPI>();
        /// <summary>
        /// Gets or sets KPIs.
        /// </summary>
        public List<KPI> KPIs
        {
            get { return this.KPIsValue; }
            set { SetProperty(ref KPIsValue, value); }
        }
    }

    /// <summary>
    /// Key Performance Indicator
    /// </summary>
    public partial class KPI : ModelEntityBase
    {
        private System.String NameValue = string.Empty;
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public System.String Name
        {
            get { return this.NameValue; }
            set { SetProperty(ref NameValue, value); }
        }

        private System.Decimal ValueValue;
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public System.Decimal Value
        {
            get { return this.ValueValue; }
            set { SetProperty(ref ValueValue, value); }
        }

        private System.String FormatValue = string.Empty;
        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        public System.String Format
        {
            get { return this.FormatValue; }
            set { SetProperty(ref FormatValue, value); }
        }

        private System.String StatusValue = string.Empty;
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public System.String Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }

        private System.String TrendValue = string.Empty;
        /// <summary>
        /// Gets or sets the trend.
        /// </summary>
        public System.String Trend
        {
            get { return this.TrendValue; }
            set { SetProperty(ref TrendValue, value); }
        }

        private System.Decimal TargetValue;
        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        public System.Decimal Target
        {
            get { return this.TargetValue; }
            set { SetProperty(ref TargetValue, value); }
        }

        private System.Decimal ThresholdValue;
        /// <summary>
        /// Gets or sets the threshold.
        /// </summary>
        public System.Decimal Threshold
        {
            get { return this.ThresholdValue; }
            set { SetProperty(ref ThresholdValue, value); }
        }
    }

    /// <summary>
    /// Executive Summary
    /// </summary>
    public partial class ExecutiveSummary : ModelEntityBase
    {
        private System.DateTime PeriodStartValue;
        /// <summary>
        /// Gets or sets period start.
        /// </summary>
        public System.DateTime PeriodStart
        {
            get { return this.PeriodStartValue; }
            set { SetProperty(ref PeriodStartValue, value); }
        }

        private System.DateTime PeriodEndValue;
        /// <summary>
        /// Gets or sets period end.
        /// </summary>
        public System.DateTime PeriodEnd
        {
            get { return this.PeriodEndValue; }
            set { SetProperty(ref PeriodEndValue, value); }
        }

        private System.DateTime AsOfDateValue;
        /// <summary>
        /// Gets or sets as of date.
        /// </summary>
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private System.DateTime GeneratedDateValue;
        /// <summary>
        /// Gets or sets generated date.
        /// </summary>
        public System.DateTime GeneratedDate
        {
            get { return this.GeneratedDateValue; }
            set { SetProperty(ref GeneratedDateValue, value); }
        }

        private List<HighlightedMetric> HighlightedMetricsValue = new List<HighlightedMetric>();
        /// <summary>
        /// Gets or sets highlighted metrics.
        /// </summary>
        public List<HighlightedMetric> HighlightedMetrics
        {
            get { return this.HighlightedMetricsValue; }
            set { SetProperty(ref HighlightedMetricsValue, value); }
        }

        private System.Decimal TotalAssetsValue;
        /// <summary>
        /// Gets or sets total assets.
        /// </summary>
        public System.Decimal TotalAssets
        {
            get { return this.TotalAssetsValue; }
            set { SetProperty(ref TotalAssetsValue, value); }
        }

        private System.Decimal TotalLiabilitiesValue;
        /// <summary>
        /// Gets or sets total liabilities.
        /// </summary>
        public System.Decimal TotalLiabilities
        {
            get { return this.TotalLiabilitiesValue; }
            set { SetProperty(ref TotalLiabilitiesValue, value); }
        }

        private System.Decimal TotalEquityValue;
        /// <summary>
        /// Gets or sets total equity.
        /// </summary>
        public System.Decimal TotalEquity
        {
            get { return this.TotalEquityValue; }
            set { SetProperty(ref TotalEquityValue, value); }
        }

        private System.Decimal NetIncomeValue;
        /// <summary>
        /// Gets or sets net income.
        /// </summary>
        public System.Decimal NetIncome
        {
            get { return this.NetIncomeValue; }
            set { SetProperty(ref NetIncomeValue, value); }
        }

        private System.Decimal HealthScoreValue;
        /// <summary>
        /// Gets or sets health score.
        /// </summary>
        public System.Decimal HealthScore
        {
            get { return this.HealthScoreValue; }
            set { SetProperty(ref HealthScoreValue, value); }
        }
    }

    /// <summary>
    /// Highlighted Metric
    /// </summary>
    public partial class HighlightedMetric : ModelEntityBase
    {
        private System.String LabelValue = string.Empty;
        /// <summary>
        /// Gets or sets label.
        /// </summary>
        public System.String Label
        {
            get { return this.LabelValue; }
            set { SetProperty(ref LabelValue, value); }
        }

        private System.Decimal ValueValue;
        /// <summary>
        /// Gets or sets value.
        /// </summary>
        public System.Decimal Value
        {
            get { return this.ValueValue; }
            set { SetProperty(ref ValueValue, value); }
        }

        private System.String StatusValue = string.Empty;
        /// <summary>
        /// Gets or sets status.
        /// </summary>
        public System.String Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }

        private System.String InsightValue = string.Empty;
        /// <summary>
        /// Gets or sets insight.
        /// </summary>
        public System.String Insight
        {
            get { return this.InsightValue; }
            set { SetProperty(ref InsightValue, value); }
        }
    }

    /// <summary>
    /// Trend Analysis
    /// </summary>
    public partial class TrendAnalysis : ModelEntityBase
    {
        private System.DateTime AnalysisStartValue;
        /// <summary>
        /// Gets or sets analysis start.
        /// </summary>
        public System.DateTime AnalysisStart
        {
            get { return this.AnalysisStartValue; }
            set { SetProperty(ref AnalysisStartValue, value); }
        }

        private System.DateTime AnalysisEndValue;
        /// <summary>
        /// Gets or sets analysis end.
        /// </summary>
        public System.DateTime AnalysisEnd
        {
            get { return this.AnalysisEndValue; }
            set { SetProperty(ref AnalysisEndValue, value); }
        }

        private System.Int32 MonthsAnalyzedValue;
        /// <summary>
        /// Gets or sets months analyzed.
        /// </summary>
        public System.Int32 MonthsAnalyzed
        {
            get { return this.MonthsAnalyzedValue; }
            set { SetProperty(ref MonthsAnalyzedValue, value); }
        }

        private System.DateTime AnalysisDateValue;
        /// <summary>
        /// Gets or sets analysis date.
        /// </summary>
        public System.DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private List<TrendLine> TrendLinesValue = new List<TrendLine>();
        /// <summary>
        /// Gets or sets trend lines.
        /// </summary>
        public List<TrendLine> TrendLines
        {
            get { return this.TrendLinesValue; }
            set { SetProperty(ref TrendLinesValue, value); }
        }

        private System.String OverallTrendValue = string.Empty;
        /// <summary>
        /// Gets or sets overall trend.
        /// </summary>
        public System.String OverallTrend
        {
            get { return this.OverallTrendValue; }
            set { SetProperty(ref OverallTrendValue, value); }
        }
    }

    /// <summary>
    /// Trend Line
    /// </summary>
    public partial class TrendLine : ModelEntityBase
    {
        private System.String MetricNameValue = string.Empty;
        /// <summary>
        /// Gets or sets metric name.
        /// </summary>
        public System.String MetricName
        {
            get { return this.MetricNameValue; }
            set { SetProperty(ref MetricNameValue, value); }
        }

        private System.Decimal StartValueValue;
        /// <summary>
        /// Gets or sets start value.
        /// </summary>
        public System.Decimal StartValue
        {
            get { return this.StartValueValue; }
            set { SetProperty(ref StartValueValue, value); }
        }

        private System.Decimal EndValueValue;
        /// <summary>
        /// Gets or sets end value.
        /// </summary>
        public System.Decimal EndValue
        {
            get { return this.EndValueValue; }
            set { SetProperty(ref EndValueValue, value); }
        }

        private System.Decimal ChangeValue;
        /// <summary>
        /// Gets or sets change.
        /// </summary>
        public System.Decimal Change
        {
            get { return this.ChangeValue; }
            set { SetProperty(ref ChangeValue, value); }
        }

        private System.Decimal ChangePercentValue;
        /// <summary>
        /// Gets or sets change percent.
        /// </summary>
        public System.Decimal ChangePercent
        {
            get { return this.ChangePercentValue; }
            set { SetProperty(ref ChangePercentValue, value); }
        }

        private System.String DirectionValue = string.Empty;
        /// <summary>
        /// Gets or sets direction.
        /// </summary>
        public System.String Direction
        {
            get { return this.DirectionValue; }
            set { SetProperty(ref DirectionValue, value); }
        }

        private System.Int32 DataPointsValue;
        /// <summary>
        /// Gets or sets data points.
        /// </summary>
        public System.Int32 DataPoints
        {
            get { return this.DataPointsValue; }
            set { SetProperty(ref DataPointsValue, value); }
        }
    }
}


