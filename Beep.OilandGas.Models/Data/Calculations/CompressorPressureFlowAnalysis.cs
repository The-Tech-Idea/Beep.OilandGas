using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CompressorPressureFlowAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private List<PressureFlowPoint> PerformancePointsValue = new();

        public List<PressureFlowPoint> PerformancePoints

        {

            get { return this.PerformancePointsValue; }

            set { SetProperty(ref PerformancePointsValue, value); }

        }
        private decimal SurgeLimitValue;

        public decimal SurgeLimit

        {

            get { return this.SurgeLimitValue; }

            set { SetProperty(ref SurgeLimitValue, value); }

        }
        private decimal ChokingLimitValue;

        public decimal ChokingLimit

        {

            get { return this.ChokingLimitValue; }

            set { SetProperty(ref ChokingLimitValue, value); }

        }
        private decimal OptimalFlowRateValue;

        public decimal OptimalFlowRate

        {

            get { return this.OptimalFlowRateValue; }

            set { SetProperty(ref OptimalFlowRateValue, value); }

        }
        private decimal OptimalPressureValue;

        public decimal OptimalPressure

        {

            get { return this.OptimalPressureValue; }

            set { SetProperty(ref OptimalPressureValue, value); }

        }
    }
}
