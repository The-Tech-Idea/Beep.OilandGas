using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    /// <summary>
    /// Represents pump card (load vs position)
    /// DTO for calculations
    /// </summary>
    public class PumpCard : ModelEntityBase
    {
        /// <summary>
        /// Peak load
        /// </summary>
        public decimal PeakLoad { get; set; }

        /// <summary>
        /// Minimum load
        /// </summary>
        public decimal MinimumLoad { get; set; }

        /// <summary>
        /// Net area
        /// </summary>
        public decimal NetArea { get; set; }

        /// <summary>
        /// Points on the pump card
        /// </summary>
        public List<PumpCardPoint> Points { get; set; } = new();
    }

    /// <summary>
    /// Represents a point on pump card
    /// DTO for calculations
    /// </summary>
    public class PumpCardPoint : ModelEntityBase
    {
        /// <summary>
        /// Position (0-1, where 0 = bottom of stroke, 1 = top of stroke)
        /// </summary>
        public decimal Position { get; set; }

        /// <summary>
        /// Load in pounds
        /// </summary>
        public decimal Load { get; set; }
    }
}



