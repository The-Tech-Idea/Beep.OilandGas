using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class ComponentContribution : ModelEntityBase
    {
        private string ComponentNameValue = string.Empty;

        public string ComponentName

        {

            get { return this.ComponentNameValue; }

            set { SetProperty(ref ComponentNameValue, value); }

        }
        private decimal MoleFractionValue;

        public decimal MoleFraction

        {

            get { return this.MoleFractionValue; }

            set { SetProperty(ref MoleFractionValue, value); }

        }
        private decimal CriticalTemperatureContributionValue;

        public decimal CriticalTemperatureContribution

        {

            get { return this.CriticalTemperatureContributionValue; }

            set { SetProperty(ref CriticalTemperatureContributionValue, value); }

        }
        private decimal CriticalPressureContributionValue;

        public decimal CriticalPressureContribution

        {

            get { return this.CriticalPressureContributionValue; }

            set { SetProperty(ref CriticalPressureContributionValue, value); }

        }
    }
}
