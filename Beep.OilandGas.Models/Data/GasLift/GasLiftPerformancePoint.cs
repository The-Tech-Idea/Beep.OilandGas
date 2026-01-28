using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public class GasLiftPerformancePoint : ModelEntityBase
    {
        /// <summary>
        /// Gas injection rate (Mscf/day or m³/day)
        /// </summary>
        private decimal GasInjectionRateValue;

        public decimal GasInjectionRate

        {

            get { return this.GasInjectionRateValue; }

            set { SetProperty(ref GasInjectionRateValue, value); }

        }

        /// <summary>
        /// Production rate (STB/day or m³/day)
        /// </summary>
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }

        /// <summary>
        /// Gas-liquid ratio
        /// </summary>
        private decimal GasLiquidRatioValue;

        public decimal GasLiquidRatio

        {

            get { return this.GasLiquidRatioValue; }

            set { SetProperty(ref GasLiquidRatioValue, value); }

        }

        /// <summary>
        /// Bottom hole pressure (psia or kPa)
        /// </summary>
        private decimal BottomHolePressureValue;

        public decimal BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        }
    }
}
