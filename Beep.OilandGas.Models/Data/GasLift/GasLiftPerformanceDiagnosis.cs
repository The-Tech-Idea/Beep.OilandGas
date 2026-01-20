using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// Performance diagnosis result for gas lift operations
    /// </summary>
    public class GasLiftPerformanceDiagnosisResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the diagnosis date
        /// </summary>
        public DateTime DiagnosisDate { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the diagnosis
        /// </summary>
        public string DiagnosedByUser { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the well UWI
        /// </summary>
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the production change percentage
        /// </summary>
        public decimal ProductionChangePercent { get; set; }

        /// <summary>
        /// Gets or sets the gas injection rate change percentage
        /// </summary>
        public decimal GasInjectionChangePercent { get; set; }

        /// <summary>
        /// Gets or sets the efficiency change percentage
        /// </summary>
        public decimal EfficiencyChangePercent { get; set; }

        /// <summary>
        /// Gets or sets the list of detected issues
        /// </summary>
        public List<string> IssuesDetected { get; set; } = new();

        /// <summary>
        /// Gets or sets the performance status (Normal, Degraded, Poor, Critical)
        /// </summary>
        public string PerformanceStatus { get; set; } = "Normal";

        /// <summary>
        /// Gets or sets the recommended actions
        /// </summary>
        public List<string> RecommendedActions { get; set; } = new();
    }
}
