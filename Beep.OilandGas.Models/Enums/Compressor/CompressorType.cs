using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.Compressor
{
    public enum CompressorType
    {
        [Description("Reciprocating")]
        Reciprocating,
        [Description("Centrifugal")]
        Centrifugal,
        [Description("Screw")]
        Screw,
        [Description("Axial")]
        Axial,
        [Description("Unknown")]
        Unknown
    }
}
