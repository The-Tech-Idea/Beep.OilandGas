using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class GasLiftValveData : ModelEntityBase
    {
        private decimal DepthValue;

        public decimal Depth

        {

            get { return this.DepthValue; }

            set { SetProperty(ref DepthValue, value); }

        } // feet
        private decimal PortSizeValue;

        public decimal PortSize

        {

            get { return this.PortSizeValue; }

            set { SetProperty(ref PortSizeValue, value); }

        } // inches
        private decimal OpeningPressureValue;

        public decimal OpeningPressure

        {

            get { return this.OpeningPressureValue; }

            set { SetProperty(ref OpeningPressureValue, value); }

        } // psia
        private decimal ClosingPressureValue;

        public decimal ClosingPressure

        {

            get { return this.ClosingPressureValue; }

            set { SetProperty(ref ClosingPressureValue, value); }

        } // psia
        private string ValveTypeValue = string.Empty;

        public string ValveType

        {

            get { return this.ValveTypeValue; }

            set { SetProperty(ref ValveTypeValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        } // Rankine
        private decimal GasInjectionRateValue;

        public decimal GasInjectionRate

        {

            get { return this.GasInjectionRateValue; }

            set { SetProperty(ref GasInjectionRateValue, value); }

        } // Mscf/day
    }
}
