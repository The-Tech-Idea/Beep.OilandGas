using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    /// <summary>
    /// Request object for sucker rod pump performance analysis using API 11L calculations.
    /// </summary>
    public class SuckerRodAnalyzeRequest
    {
        /// <summary>System properties describing the well and fluid conditions.</summary>
        public SUCKER_ROD_SYSTEM_PROPERTIES? SystemProperties { get; set; }

        /// <summary>Rod string configuration for load calculations.</summary>
        public SUCKER_ROD_STRING? RodString { get; set; }
    }
}
