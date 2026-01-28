using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PumpDesignResult : ModelEntityBase
    {
        private string DesignIdValue = string.Empty;

        public string DesignId

        {

            get { return this.DesignIdValue; }

            set { SetProperty(ref DesignIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string PumpTypeValue = string.Empty;

        public string PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

        }
        private DateTime DesignDateValue;

        public DateTime DesignDate

        {

            get { return this.DesignDateValue; }

            set { SetProperty(ref DesignDateValue, value); }

        }
        private decimal PumpDepthValue;

        public decimal PumpDepth

        {

            get { return this.PumpDepthValue; }

            set { SetProperty(ref PumpDepthValue, value); }

        }
        private decimal TubingSizeValue;

        public decimal TubingSize

        {

            get { return this.TubingSizeValue; }

            set { SetProperty(ref TubingSizeValue, value); }

        }
        private decimal CasingSizeValue;

        public decimal CasingSize

        {

            get { return this.CasingSizeValue; }

            set { SetProperty(ref CasingSizeValue, value); }

        }
        private decimal DesignFlowRateValue;

        public decimal DesignFlowRate

        {

            get { return this.DesignFlowRateValue; }

            set { SetProperty(ref DesignFlowRateValue, value); }

        }
        private decimal DesignPressureValue;

        public decimal DesignPressure

        {

            get { return this.DesignPressureValue; }

            set { SetProperty(ref DesignPressureValue, value); }

        }
        private decimal ExpectedEfficiencyValue;

        public decimal ExpectedEfficiency

        {

            get { return this.ExpectedEfficiencyValue; }

            set { SetProperty(ref ExpectedEfficiencyValue, value); }

        }
        private decimal EstimatedPowerValue;

        public decimal EstimatedPower

        {

            get { return this.EstimatedPowerValue; }

            set { SetProperty(ref EstimatedPowerValue, value); }

        }
        private string StatusValue = "Designed";

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private List<string> DesignRecommendationsValue = new();

        public List<string> DesignRecommendations

        {

            get { return this.DesignRecommendationsValue; }

            set { SetProperty(ref DesignRecommendationsValue, value); }

        }
    }
}
