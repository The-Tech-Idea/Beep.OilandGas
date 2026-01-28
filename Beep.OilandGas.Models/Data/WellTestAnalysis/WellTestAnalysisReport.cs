using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public class WellTestAnalysisReport : ModelEntityBase
    {
        private string ReportIdValue = string.Empty;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private DateTime GeneratedDateValue;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
        private string GeneratedByValue = string.Empty;

        public string GeneratedBy

        {

            get { return this.GeneratedByValue; }

            set { SetProperty(ref GeneratedByValue, value); }

        }
        private List<WellTestAnalysisResult> AnalysisResultsValue = new();

        public List<WellTestAnalysisResult> AnalysisResults

        {

            get { return this.AnalysisResultsValue; }

            set { SetProperty(ref AnalysisResultsValue, value); }

        }
        private string ExecutiveSummaryValue = string.Empty;

        public string ExecutiveSummary

        {

            get { return this.ExecutiveSummaryValue; }

            set { SetProperty(ref ExecutiveSummaryValue, value); }

        }
        private List<string> RecommendationsValue = new();

        public List<string> Recommendations

        {

            get { return this.RecommendationsValue; }

            set { SetProperty(ref RecommendationsValue, value); }

        }
    }
}
