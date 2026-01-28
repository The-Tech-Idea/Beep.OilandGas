using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FailureRiskAssessment : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string FailureRiskValue = string.Empty;

        public string FailureRisk

        {

            get { return this.FailureRiskValue; }

            set { SetProperty(ref FailureRiskValue, value); }

        }
        private decimal RiskScoreValue;

        public decimal RiskScore

        {

            get { return this.RiskScoreValue; }

            set { SetProperty(ref RiskScoreValue, value); }

        }
    }
}
