using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.Models.Data
{
    public class NodalAnalysisRunResult : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private OperatingPoint OperatingPointValue = new();

        public OperatingPoint OperatingPoint

        {

            get { return this.OperatingPointValue; }

            set { SetProperty(ref OperatingPointValue, value); }

        }
        private List<IPRPoint> IPRCurveValue = new();

        public List<IPRPoint> IPRCurve

        {

            get { return this.IPRCurveValue; }

            set { SetProperty(ref IPRCurveValue, value); }

        }
        private List<VLPPoint> VLPCurveValue = new();

        public List<VLPPoint> VLPCurve

        {

            get { return this.VLPCurveValue; }

            set { SetProperty(ref VLPCurveValue, value); }

        }
        private decimal OptimalFlowRateValue;

        public decimal OptimalFlowRate

        {

            get { return this.OptimalFlowRateValue; }

            set { SetProperty(ref OptimalFlowRateValue, value); }

        }
        private decimal OptimalBottomholePressureValue;

        public decimal OptimalBottomholePressure

        {

            get { return this.OptimalBottomholePressureValue; }

            set { SetProperty(ref OptimalBottomholePressureValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}
