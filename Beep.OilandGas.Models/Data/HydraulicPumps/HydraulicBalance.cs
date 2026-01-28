using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class HydraulicBalance : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool IsBalancedValue;

        public bool IsBalanced

        {

            get { return this.IsBalancedValue; }

            set { SetProperty(ref IsBalancedValue, value); }

        }
        private decimal BalanceScoreValue;

        public decimal BalanceScore

        {

            get { return this.BalanceScoreValue; }

            set { SetProperty(ref BalanceScoreValue, value); }

        }
        private List<BalanceFactor> FactorsValue = new();

        public List<BalanceFactor> Factors

        {

            get { return this.FactorsValue; }

            set { SetProperty(ref FactorsValue, value); }

        }
        private List<string> RecommendationsValue = new();

        public List<string> Recommendations

        {

            get { return this.RecommendationsValue; }

            set { SetProperty(ref RecommendationsValue, value); }

        }
    }
}
