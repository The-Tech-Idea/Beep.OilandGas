using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CompressorPowerAnalysis : ModelEntityBase
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
        private decimal InletPowerValue;

        public decimal InletPower

        {

            get { return this.InletPowerValue; }

            set { SetProperty(ref InletPowerValue, value); }

        }
        private decimal FrictionLossesValue;

        public decimal FrictionLosses

        {

            get { return this.FrictionLossesValue; }

            set { SetProperty(ref FrictionLossesValue, value); }

        }
        private decimal IsothermicPowerValue;

        public decimal IsothermicPower

        {

            get { return this.IsothermicPowerValue; }

            set { SetProperty(ref IsothermicPowerValue, value); }

        }
        private decimal PolyIsentropicPowerValue;

        public decimal PolyIsentropicPower

        {

            get { return this.PolyIsentropicPowerValue; }

            set { SetProperty(ref PolyIsentropicPowerValue, value); }

        }
        private decimal IsentropicPowerValue;

        public decimal IsentropicPower

        {

            get { return this.IsentropicPowerValue; }

            set { SetProperty(ref IsentropicPowerValue, value); }

        }
        private decimal ActualPowerValue;

        public decimal ActualPower

        {

            get { return this.ActualPowerValue; }

            set { SetProperty(ref ActualPowerValue, value); }

        }
        private decimal PowerSavingsValue;

        public decimal PowerSavings

        {

            get { return this.PowerSavingsValue; }

            set { SetProperty(ref PowerSavingsValue, value); }

        }
        private string OptimizationRecommendationValue = string.Empty;

        public string OptimizationRecommendation

        {

            get { return this.OptimizationRecommendationValue; }

            set { SetProperty(ref OptimizationRecommendationValue, value); }

        }
    }
}
