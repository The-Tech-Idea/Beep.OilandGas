using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// Valve spacing optimization result
    /// </summary>
    public class GasLiftValveOptimizationResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the number of valves in this configuration
        /// </summary>
        public int NumberOfValves { get; set; }

        /// <summary>
        /// Gets or sets the total gas injection rate (Mscf/day)
        /// </summary>
        public decimal TotalGasInjectionRate { get; set; }

        /// <summary>
        /// Gets or sets the valve spacing (feet)
        /// </summary>
        public decimal ValveSpacing { get; set; }

        /// <summary>
        /// Gets or sets the design quality score (0-100%)
        /// </summary>
        public decimal DesignQuality { get; set; }

        /// <summary>
        /// Gets or sets the cost-effectiveness score (0-100%)
        /// </summary>
        public decimal CostEffectiveness { get; set; }
    }
}
