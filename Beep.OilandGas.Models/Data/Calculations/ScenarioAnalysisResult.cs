using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ScenarioAnalysisResult : ModelEntityBase
    {
        private double BaseCaseValue;

        public double BaseCase

        {

            get { return this.BaseCaseValue; }

            set { SetProperty(ref BaseCaseValue, value); }

        }
        private double DownsideCaseValue;

        public double DownsideCase

        {

            get { return this.DownsideCaseValue; }

            set { SetProperty(ref DownsideCaseValue, value); }

        }
        private double UpsideCaseValue;

        public double UpsideCase

        {

            get { return this.UpsideCaseValue; }

            set { SetProperty(ref UpsideCaseValue, value); }

        }
        private double ProbabilityBaseCaseValue;

        public double ProbabilityBaseCase

        {

            get { return this.ProbabilityBaseCaseValue; }

            set { SetProperty(ref ProbabilityBaseCaseValue, value); }

        }
        private double ProbabilityDownsideValue;

        public double ProbabilityDownside

        {

            get { return this.ProbabilityDownsideValue; }

            set { SetProperty(ref ProbabilityDownsideValue, value); }

        }
        private double ProbabilityUpsideValue;

        public double ProbabilityUpside

        {

            get { return this.ProbabilityUpsideValue; }

            set { SetProperty(ref ProbabilityUpsideValue, value); }

        }
    }
}
