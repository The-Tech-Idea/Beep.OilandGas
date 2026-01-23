using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// Result of gas lift potential analysis
    /// DTO for calculations - Entity classes: GAS_LIFT_POTENTIAL_RESULT, GAS_LIFT_PERFORMANCE_POINT
    /// </summary>
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

    /// <summary>
    /// Performance point for a specific gas injection rate
    /// DTO for calculations - Entity class: GAS_LIFT_PERFORMANCE_POINT
    /// </summary>
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






