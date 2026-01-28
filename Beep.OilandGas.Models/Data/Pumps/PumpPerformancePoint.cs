using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class PumpPerformancePoint : ModelEntityBase
    {
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // GPM or bbl/day
        private decimal HeadValue;

        public decimal Head

        {

            get { return this.HeadValue; }

            set { SetProperty(ref HeadValue, value); }

        } // feet
        private decimal PowerValue;

        public decimal Power

        {

            get { return this.PowerValue; }

            set { SetProperty(ref PowerValue, value); }

        } // horsepower
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        } // fraction 0-1
    }
}
