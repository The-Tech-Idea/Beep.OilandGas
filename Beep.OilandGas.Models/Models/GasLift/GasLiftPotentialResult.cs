using System.Collections.Generic;

namespace Beep.OilandGas.Models.GasLift
{
    /// <summary>
    /// Result of gas lift potential analysis
    /// DTO for calculations - Entity classes: GAS_LIFT_POTENTIAL_RESULT, GAS_LIFT_PERFORMANCE_POINT
    /// </summary>
    public class GasLiftPotentialResult
    {
        /// <summary>
        /// Optimal gas injection rate (Mscf/day or m³/day)
        /// </summary>
        public decimal OptimalGasInjectionRate { get; set; }

        /// <summary>
        /// Maximum production rate achievable (STB/day or m³/day)
        /// </summary>
        public decimal MaximumProductionRate { get; set; }

        /// <summary>
        /// Optimal gas-liquid ratio
        /// </summary>
        public decimal OptimalGasLiquidRatio { get; set; }

        /// <summary>
        /// Performance points for different gas injection rates
        /// </summary>
        public List<GasLiftPerformancePoint> PerformancePoints { get; set; } = new List<GasLiftPerformancePoint>();
    }

    /// <summary>
    /// Performance point for a specific gas injection rate
    /// DTO for calculations - Entity class: GAS_LIFT_PERFORMANCE_POINT
    /// </summary>
    public class GasLiftPerformancePoint
    {
        /// <summary>
        /// Gas injection rate (Mscf/day or m³/day)
        /// </summary>
        public decimal GasInjectionRate { get; set; }

        /// <summary>
        /// Production rate (STB/day or m³/day)
        /// </summary>
        public decimal ProductionRate { get; set; }

        /// <summary>
        /// Gas-liquid ratio
        /// </summary>
        public decimal GasLiquidRatio { get; set; }

        /// <summary>
        /// Bottom hole pressure (psia or kPa)
        /// </summary>
        public decimal BottomHolePressure { get; set; }
    }
}
