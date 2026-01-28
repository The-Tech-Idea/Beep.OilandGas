using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public class GasLiftPotentialResult : ModelEntityBase
    {
        /// <summary>
        /// Optimal gas injection rate (Mscf/day or m³/day)
        /// </summary>
        private decimal OptimalGasInjectionRateValue;

        public decimal OptimalGasInjectionRate

        {

            get { return this.OptimalGasInjectionRateValue; }

            set { SetProperty(ref OptimalGasInjectionRateValue, value); }

        }

        /// <summary>
        /// Maximum production rate achievable (STB/day or m³/day)
        /// </summary>
        private decimal MaximumProductionRateValue;

        public decimal MaximumProductionRate

        {

            get { return this.MaximumProductionRateValue; }

            set { SetProperty(ref MaximumProductionRateValue, value); }

        }

        /// <summary>
        /// Optimal gas-liquid ratio
        /// </summary>
        private decimal OptimalGasLiquidRatioValue;

        public decimal OptimalGasLiquidRatio

        {

            get { return this.OptimalGasLiquidRatioValue; }

            set { SetProperty(ref OptimalGasLiquidRatioValue, value); }

        }

        /// <summary>
        /// Performance points for different gas injection rates
        /// </summary>
        private List<GasLiftPerformancePoint> PerformancePointsValue = new List<GasLiftPerformancePoint>();

        public List<GasLiftPerformancePoint> PerformancePoints

        {

            get { return this.PerformancePointsValue; }

            set { SetProperty(ref PerformancePointsValue, value); }

        }
    }
}
