using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    /// <summary>
    /// Represents sucker rod string configuration
    /// DTO for calculations - Entity class: SUCKER_ROD_STRING
    /// </summary>
    public class SuckerRodString : ModelEntityBase
    {
        /// <summary>
        /// Rod sections
        /// </summary>
        public List<RodSection> Sections { get; set; } = new();

        /// <summary>
        /// Total length in feet
        /// </summary>
        public decimal TotalLength { get; set; }

        /// <summary>
        /// Total weight in pounds
        /// </summary>
        public decimal TotalWeight { get; set; }
    }
}



