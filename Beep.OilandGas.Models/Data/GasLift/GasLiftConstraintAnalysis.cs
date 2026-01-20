using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// Constraint analysis result for gas lift design
    /// </summary>
    public class GasLiftConstraintAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the analysis date
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the analysis
        /// </summary>
        public string AnalyzedByUser { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the well UWI
        /// </summary>
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum tubing pressure (psia)
        /// </summary>
        public decimal MaxTubingPressure { get; set; }

        /// <summary>
        /// Gets or sets the maximum casing pressure (psia)
        /// </summary>
        public decimal MaxCasingPressure { get; set; }

        /// <summary>
        /// Gets or sets the maximum surface equipment pressure (psia)
        /// </summary>
        public decimal MaxSurfaceEquipmentPressure { get; set; }

        /// <summary>
        /// Gets or sets the maximum available gas supply (Mscf/day)
        /// </summary>
        public decimal MaxAvailableGasSupply { get; set; }

        /// <summary>
        /// Gets or sets the maximum production capacity (BPD)
        /// </summary>
        public decimal MaxProductionCapacity { get; set; }

        /// <summary>
        /// Gets or sets the maximum tubing temperature (°F)
        /// </summary>
        public decimal MaxTubingTemperature { get; set; }

        /// <summary>
        /// Gets or sets the list of active constraints
        /// </summary>
        public List<string> ActiveConstraints { get; set; } = new();
    }
}
