using System.Collections.Generic;
using Beep.OilandGas.Models.Data.FlashCalculations;

namespace Beep.OilandGas.Models.Data.FlashCalculations
{
    /// <summary>
    /// Request for multi-stage flash calculations.
    /// </summary>
    public class MultiStageFlashRequest : ModelEntityBase
    {
        /// <summary>Initial conditions applied to all stages.</summary>
        public FLASH_CONDITIONS? Conditions { get; set; }

        /// <summary>Number of stages to simulate.</summary>
        public int Stages { get; set; } = 2;
    }
}
