using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CompressorDesign : ModelEntityBase
    {
        private string DesignIdValue = string.Empty;

        public string DesignId

        {

            get { return this.DesignIdValue; }

            set { SetProperty(ref DesignIdValue, value); }

        }
        private DateTime DesignDateValue;

        public DateTime DesignDate

        {

            get { return this.DesignDateValue; }

            set { SetProperty(ref DesignDateValue, value); }

        }
        private string CompressorTypeValue = string.Empty;

        public string CompressorType

        {

            get { return this.CompressorTypeValue; }

            set { SetProperty(ref CompressorTypeValue, value); }

        }
        private decimal RequiredFlowRateValue;

        public decimal RequiredFlowRate

        {

            get { return this.RequiredFlowRateValue; }

            set { SetProperty(ref RequiredFlowRateValue, value); }

        }
        private decimal RequiredDischargePressureValue;

        public decimal RequiredDischargePressure

        {

            get { return this.RequiredDischargePressureValue; }

            set { SetProperty(ref RequiredDischargePressureValue, value); }

        }
        private decimal RequiredInletPressureValue;

        public decimal RequiredInletPressure

        {

            get { return this.RequiredInletPressureValue; }

            set { SetProperty(ref RequiredInletPressureValue, value); }

        }
        private decimal GasSpecificGravityValue;

        public decimal GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
        private decimal DesignTemperatureValue;

        public decimal DesignTemperature

        {

            get { return this.DesignTemperatureValue; }

            set { SetProperty(ref DesignTemperatureValue, value); }

        }
        private decimal RecommendedStagesValue;

        public decimal RecommendedStages

        {

            get { return this.RecommendedStagesValue; }

            set { SetProperty(ref RecommendedStagesValue, value); }

        }
        private decimal EstimatedEfficiencyValue;

        public decimal EstimatedEfficiency

        {

            get { return this.EstimatedEfficiencyValue; }

            set { SetProperty(ref EstimatedEfficiencyValue, value); }

        }
        private decimal EstimatedPowerValue;

        public decimal EstimatedPower

        {

            get { return this.EstimatedPowerValue; }

            set { SetProperty(ref EstimatedPowerValue, value); }

        }
        private List<CompressorStage> StagesValue = new();

        public List<CompressorStage> Stages

        {

            get { return this.StagesValue; }

            set { SetProperty(ref StagesValue, value); }

        }
    }
}
