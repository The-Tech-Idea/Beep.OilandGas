using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum WellControlMethod
    {
        [Description("Driller's Method")] DrillersMethod,
        [Description("Wait & Weight")] WaitAndWeight,
        [Description("Bullheading")] Bullheading,
        [Description("Volumetric")] Volumetric,
        [Description("Reverse Circulation")] ReverseCirculation,
        [Description("Unknown")] Unknown
    }
}
