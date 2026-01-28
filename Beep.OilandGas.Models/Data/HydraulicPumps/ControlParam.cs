using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ControlParam : ModelEntityBase
    {
        private string ParameterNameValue = string.Empty;

        public string ParameterName

        {

            get { return this.ParameterNameValue; }

            set { SetProperty(ref ParameterNameValue, value); }

        }
        private decimal CurrentValueValue;

        public decimal CurrentValue

        {

            get { return this.CurrentValueValue; }

            set { SetProperty(ref CurrentValueValue, value); }

        }
        private decimal MinValueValue;

        public decimal MinValue

        {

            get { return this.MinValueValue; }

            set { SetProperty(ref MinValueValue, value); }

        }
        private decimal MaxValueValue;

        public decimal MaxValue

        {

            get { return this.MaxValueValue; }

            set { SetProperty(ref MaxValueValue, value); }

        }
    }
}
