using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PumpTypeOption : ModelEntityBase
    {
        private string PumpTypeValue = string.Empty;

        public string PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

        }
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
        private decimal CostValue;

        public decimal Cost

        {

            get { return this.CostValue; }

            set { SetProperty(ref CostValue, value); }

        }
        private decimal ReliabilityValue;

        public decimal Reliability

        {

            get { return this.ReliabilityValue; }

            set { SetProperty(ref ReliabilityValue, value); }

        }
        private List<string> AdvantagesValue = new();

        public List<string> Advantages

        {

            get { return this.AdvantagesValue; }

            set { SetProperty(ref AdvantagesValue, value); }

        }
        private List<string> DisadvantagesValue = new();

        public List<string> Disadvantages

        {

            get { return this.DisadvantagesValue; }

            set { SetProperty(ref DisadvantagesValue, value); }

        }
    }
}
