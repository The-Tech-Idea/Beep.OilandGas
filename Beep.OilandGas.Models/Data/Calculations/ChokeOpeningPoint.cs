using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeOpeningPoint : ModelEntityBase
    {
        private decimal ChokeDiameterValue;

        public decimal ChokeDiameter

        {

            get { return this.ChokeDiameterValue; }

            set { SetProperty(ref ChokeDiameterValue, value); }

        }
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }
        private decimal PressureDropValue;

        public decimal PressureDrop

        {

            get { return this.PressureDropValue; }

            set { SetProperty(ref PressureDropValue, value); }

        }
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
    }
}
