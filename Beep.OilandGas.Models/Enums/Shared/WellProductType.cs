using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum WellProductType
    {
        [Description("Oil")]
        Oil,
        [Description("Gas")]
        Gas,
        [Description("Gas Condensate")]
        GasCondensate,
        [Description("Water")]
        Water,
        [Description("Dry")]
        Dry,
        [Description("Unknown")]
        Unknown
    }
}
