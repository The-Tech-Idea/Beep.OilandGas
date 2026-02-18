using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum OperationalSiteType
    {
        [Description("Platform")]
        Platform,
        [Description("Subsea Completion (Tieback)")]
        SubseaCompletion,
        [Description("Satellite Location")]
        SatelliteLocation,
        [Description("Central Hub")]
        CentralHub,
        [Description("Onshore Pad")]
        OnshorePad,
        [Description("Unknown")]
        Unknown
    }
}
