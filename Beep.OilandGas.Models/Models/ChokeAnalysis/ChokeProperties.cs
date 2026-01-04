using System;
using Beep.OilandGas.Models.ChokeAnalysis;

namespace Beep.OilandGas.Models.ChokeAnalysis
{
    /// <summary>
    /// Represents choke properties
    /// DTO for calculations - Entity class: CHOKE_PROPERTIES
    /// </summary>
    public class ChokeProperties
    {
        /// <summary>
        /// Choke diameter in inches
        /// </summary>
        public decimal ChokeDiameter { get; set; }

        /// <summary>
        /// Choke type (bean, adjustable, etc.)
        /// </summary>
        public ChokeType ChokeType { get; set; }

        /// <summary>
        /// Discharge coefficient
        /// </summary>
        public decimal DischargeCoefficient { get; set; } = 0.85m;

        /// <summary>
        /// Choke area in square inches
        /// </summary>
        public decimal ChokeArea => (decimal)Math.PI * ChokeDiameter * ChokeDiameter / 4m;
    }
}
