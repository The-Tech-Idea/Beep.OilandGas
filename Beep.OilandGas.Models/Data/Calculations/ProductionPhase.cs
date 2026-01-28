using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ProductionPhase : ModelEntityBase
    {
        private int PhaseNumberValue;

        public int PhaseNumber

        {

            get { return this.PhaseNumberValue; }

            set { SetProperty(ref PhaseNumberValue, value); }

        }
        private double TargetCapacityValue;

        public double TargetCapacity

        {

            get { return this.TargetCapacityValue; }

            set { SetProperty(ref TargetCapacityValue, value); }

        }
        private int DurationMonthsValue;

        public int DurationMonths

        {

            get { return this.DurationMonthsValue; }

            set { SetProperty(ref DurationMonthsValue, value); }

        }
    }
}
