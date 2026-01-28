using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class UncertaintyRange : ModelEntityBase
    {
        private double LowCaseValue;

        public double LowCase

        {

            get { return this.LowCaseValue; }

            set { SetProperty(ref LowCaseValue, value); }

        }
        private double BaseCaseValue;

        public double BaseCase

        {

            get { return this.BaseCaseValue; }

            set { SetProperty(ref BaseCaseValue, value); }

        }
        private double HighCaseValue;

        public double HighCase

        {

            get { return this.HighCaseValue; }

            set { SetProperty(ref HighCaseValue, value); }

        }
    }
}
