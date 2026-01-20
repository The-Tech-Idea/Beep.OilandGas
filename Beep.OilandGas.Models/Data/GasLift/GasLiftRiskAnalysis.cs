using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// Risk analysis result for gas lift operations
    /// </summary>
    public class GasLiftRiskAnalysisResult : ModelEntityBase
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
        /// Gets or sets the tubing/casing stress risk (0-100%)
        /// </summary>
        public decimal TubingCasingStressRisk { get; set; }

        /// <summary>
        /// Gets or sets the scale/corrosion risk (0-100%)
        /// </summary>
        public decimal ScaleCorrosionRisk { get; set; }

        /// <summary>
        /// Gets or sets the valve reliability risk (0-100%)
        /// </summary>
        public decimal ValveReliabilityRisk { get; set; }

        /// <summary>
        /// Gets or sets the gas supply interruption risk (0-100%)
        /// </summary>
        public decimal GasSupplyInterruptionRisk { get; set; }

        /// <summary>
        /// Gets or sets the overall risk rating (0-100%)
        /// </summary>
        public decimal OverallRiskRating { get; set; }

        /// <summary>
        /// Gets or sets the risk level (Low, Medium, High, Critical)
        /// </summary>
        public string RiskLevel { get; set; } = "Medium";

        /// <summary>
        /// Gets or sets the recommended mitigation actions
        /// </summary>
        public List<string> RecommendedMitigationActions { get; set; } = new();
    }
}
