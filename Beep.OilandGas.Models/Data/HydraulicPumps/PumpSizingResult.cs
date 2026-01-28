using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PumpSizingResult : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal RecommendedDisplacementValue;

        public decimal RecommendedDisplacement

        {

            get { return this.RecommendedDisplacementValue; }

            set { SetProperty(ref RecommendedDisplacementValue, value); }

        }
        private decimal MaximumFlowRateValue;

        public decimal MaximumFlowRate

        {

            get { return this.MaximumFlowRateValue; }

            set { SetProperty(ref MaximumFlowRateValue, value); }

        }
        private decimal MaximumPressureValue;

        public decimal MaximumPressure

        {

            get { return this.MaximumPressureValue; }

            set { SetProperty(ref MaximumPressureValue, value); }

        }
        private decimal OptimalSpeedValue;

        public decimal OptimalSpeed

        {

            get { return this.OptimalSpeedValue; }

            set { SetProperty(ref OptimalSpeedValue, value); }

        }
        private string RecommendedPumpModelValue = string.Empty;

        public string RecommendedPumpModel

        {

            get { return this.RecommendedPumpModelValue; }

            set { SetProperty(ref RecommendedPumpModelValue, value); }

        }
    }
}
