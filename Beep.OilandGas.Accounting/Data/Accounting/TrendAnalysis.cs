using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
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
}
