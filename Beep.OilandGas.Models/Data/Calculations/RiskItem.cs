using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class RiskItem : ModelEntityBase
    {
        private string RiskIdValue;

        public string RiskId

        {

            get { return this.RiskIdValue; }

            set { SetProperty(ref RiskIdValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private double ProbabilityValue;

        public double Probability

        {

            get { return this.ProbabilityValue; }

            set { SetProperty(ref ProbabilityValue, value); }

        }
        private double ImpactValue;

        public double Impact

        {

            get { return this.ImpactValue; }

            set { SetProperty(ref ImpactValue, value); }

        }
    }
}
