using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class NodalOperatingPoint : ModelEntityBase
    {
        private decimal RateValue;

        public decimal Rate

        {

            get { return this.RateValue; }

            set { SetProperty(ref RateValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private bool IsStableValue;

        public bool IsStable

        {

            get { return this.IsStableValue; }

            set { SetProperty(ref IsStableValue, value); }

        }
    }
}
