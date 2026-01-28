using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class EnvironmentalComparisonResult : ModelEntityBase
    {
        private double LowestEmissionsValue;

        public double LowestEmissions

        {

            get { return this.LowestEmissionsValue; }

            set { SetProperty(ref LowestEmissionsValue, value); }

        }
        private double HighestEmissionsValue;

        public double HighestEmissions

        {

            get { return this.HighestEmissionsValue; }

            set { SetProperty(ref HighestEmissionsValue, value); }

        }
    }
}
