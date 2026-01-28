using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class InteractionFactor : ModelEntityBase
    {
        private string FactorNameValue = string.Empty;

        public string FactorName

        {

            get { return this.FactorNameValue; }

            set { SetProperty(ref FactorNameValue, value); }

        }
        private decimal ImpactValue;

        public decimal Impact

        {

            get { return this.ImpactValue; }

            set { SetProperty(ref ImpactValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
