using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum WellIntent
    {
        [Description("Wildcat - Exploration in unproven area")]
        Wildcat,
        [Description("Appraisal - Determine extent of discovery")]
        Appraisal,
        [Description("Development - Prepare for production")]
        Development,
        [Description("Production - Extraction")]
        Production,
        [Description("Injection - Pressure maintenance/Sweep")]
        Injection,
        [Description("Unknown")]
        Unknown
    }
}
