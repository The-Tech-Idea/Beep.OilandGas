using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PumpPerformanceAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal CurrentFlowRateValue;

        public decimal CurrentFlowRate

        {

            get { return this.CurrentFlowRateValue; }

            set { SetProperty(ref CurrentFlowRateValue, value); }

        }
        private decimal CurrentEfficiencyValue;

        public decimal CurrentEfficiency

        {

            get { return this.CurrentEfficiencyValue; }

            set { SetProperty(ref CurrentEfficiencyValue, value); }

        }
        private List<string> RecommendationsValue = new();

        public List<string> Recommendations

        {

            get { return this.RecommendationsValue; }

            set { SetProperty(ref RecommendationsValue, value); }

        }
    }
}
