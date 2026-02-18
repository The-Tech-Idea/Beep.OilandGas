using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum PermitToWorkType
    {
        [Description("Hot Work")] HotWork,
        [Description("Cold Work")] ColdWork,
        [Description("Confined Space")] ConfinedSpace,
        [Description("Working at Height")] WorkingAtHeight,
        [Description("Pressure Testing")] PressureTesting,
        [Description("Electrical Isolation")] ElectricalIsolation,
        [Description("Lifting Operations")] LiftingOperations,
        [Description("General")] General,
        [Description("Unknown")] Unknown
    }
}
