using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
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
}
