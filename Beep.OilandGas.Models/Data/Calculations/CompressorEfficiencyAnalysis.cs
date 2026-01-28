using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CompressorEfficiencyAnalysis : ModelEntityBase
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
        private decimal IsentropicEfficiencyValue;

        public decimal IsentropicEfficiency

        {

            get { return this.IsentropicEfficiencyValue; }

            set { SetProperty(ref IsentropicEfficiencyValue, value); }

        }
        private decimal PolyIsentropicEfficiencyValue;

        public decimal PolyIsentropicEfficiency

        {

            get { return this.PolyIsentropicEfficiencyValue; }

            set { SetProperty(ref PolyIsentropicEfficiencyValue, value); }

        }
        private decimal VolumetricEfficiencyValue;

        public decimal VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        }
        private decimal MechanicalEfficiencyValue;

        public decimal MechanicalEfficiency

        {

            get { return this.MechanicalEfficiencyValue; }

            set { SetProperty(ref MechanicalEfficiencyValue, value); }

        }
        private decimal OverallEfficiencyValue;

        public decimal OverallEfficiency

        {

            get { return this.OverallEfficiencyValue; }

            set { SetProperty(ref OverallEfficiencyValue, value); }

        }
        private decimal EfficiencyTrendValue;

        public decimal EfficiencyTrend

        {

            get { return this.EfficiencyTrendValue; }

            set { SetProperty(ref EfficiencyTrendValue, value); }

        } // -1 to +1
        private string EfficiencyStatusValue = string.Empty;

        public string EfficiencyStatus

        {

            get { return this.EfficiencyStatusValue; }

            set { SetProperty(ref EfficiencyStatusValue, value); }

        } // Good, Fair, Poor
    }
}
