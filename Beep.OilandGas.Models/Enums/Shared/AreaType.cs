using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Shared
{
    public enum AreaType
    {
        [Description("Field")]
        Field,
        [Description("Block")]
        Block,
        [Description("Lease")]
        Lease,
        [Description("Basin")]
        Basin,
        [Description("Region")]
        Region,
        [Description("Country")]
        Country,
        [Description("Unknown")]
        Unknown
    }
}
