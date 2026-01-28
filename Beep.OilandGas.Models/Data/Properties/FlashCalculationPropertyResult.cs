using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FlashCalculationPropertyResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal LiquidFractionValue;

        public decimal LiquidFraction

        {

            get { return this.LiquidFractionValue; }

            set { SetProperty(ref LiquidFractionValue, value); }

        }
        private decimal VaporFractionValue;

        public decimal VaporFraction

        {

            get { return this.VaporFractionValue; }

            set { SetProperty(ref VaporFractionValue, value); }

        }
        private FluidComposition LiquidCompositionValue = new();

        public FluidComposition LiquidComposition

        {

            get { return this.LiquidCompositionValue; }

            set { SetProperty(ref LiquidCompositionValue, value); }

        }
        private FluidComposition VaporCompositionValue = new();

        public FluidComposition VaporComposition

        {

            get { return this.VaporCompositionValue; }

            set { SetProperty(ref VaporCompositionValue, value); }

        }
        private string FlashTypeValue = string.Empty;

        public string FlashType

        {

            get { return this.FlashTypeValue; }

            set { SetProperty(ref FlashTypeValue, value); }

        }
        private string CalculationMethodValue = string.Empty;

        public string CalculationMethod

        {

            get { return this.CalculationMethodValue; }

            set { SetProperty(ref CalculationMethodValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
    }
}
