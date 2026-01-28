using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public class ComparisonEntry : ModelEntityBase
    {
        private string Method1Value = string.Empty;

        public string Method1

        {

            get { return this.Method1Value; }

            set { SetProperty(ref Method1Value, value); }

        }
        private string Method2Value = string.Empty;

        public string Method2

        {

            get { return this.Method2Value; }

            set { SetProperty(ref Method2Value, value); }

        }
        private double PermeabilityDifferenceValue;

        public double PermeabilityDifference

        {

            get { return this.PermeabilityDifferenceValue; }

            set { SetProperty(ref PermeabilityDifferenceValue, value); }

        }
        private double SkinFactorDifferenceValue;

        public double SkinFactorDifference

        {

            get { return this.SkinFactorDifferenceValue; }

            set { SetProperty(ref SkinFactorDifferenceValue, value); }

        }
        private double ConfidenceLevelValue;

        public double ConfidenceLevel

        {

            get { return this.ConfidenceLevelValue; }

            set { SetProperty(ref ConfidenceLevelValue, value); }

        }
    }
}
