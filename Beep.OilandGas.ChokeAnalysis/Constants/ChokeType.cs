using System.ComponentModel;

namespace Beep.OilandGas.ChokeAnalysis.Constants;

/// <summary>
/// Choke hardware categories for engineering defaults, labels, and optional enum seeding.
/// Canonical string codes for reference data live in <see cref="ChokeAnalysisReferenceCodes"/>.
/// </summary>
public enum ChokeType
{
    [Description("Bean")]
    Bean,

    [Description("Adjustable")]
    Adjustable,

    [Description("Positive")]
    Positive,

    [Description("Cage")]
    Cage,

    [Description("Needle")]
    Needle,

    [Description("Plug")]
    Plug,

    [Description("Unknown")]
    Unknown
}
