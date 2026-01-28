using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ResourceEstimate : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private decimal OilEstimateValue;

        public decimal OilEstimate

        {

            get { return this.OilEstimateValue; }

            set { SetProperty(ref OilEstimateValue, value); }

        }
        private decimal GasEstimateValue;

        public decimal GasEstimate

        {

            get { return this.GasEstimateValue; }

            set { SetProperty(ref GasEstimateValue, value); }

        }
    }
}
