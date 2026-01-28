using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class EconomicComparisonResult : ModelEntityBase
    {
        private double HighestNPVValue;

        public double HighestNPV

        {

            get { return this.HighestNPVValue; }

            set { SetProperty(ref HighestNPVValue, value); }

        }
        private double LowestNPVValue;

        public double LowestNPV

        {

            get { return this.LowestNPVValue; }

            set { SetProperty(ref LowestNPVValue, value); }

        }
        private double BestIRRValue;

        public double BestIRR

        {

            get { return this.BestIRRValue; }

            set { SetProperty(ref BestIRRValue, value); }

        }
    }
}
