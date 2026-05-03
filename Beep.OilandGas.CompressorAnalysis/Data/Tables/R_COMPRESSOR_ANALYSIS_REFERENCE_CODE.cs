using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.CompressorAnalysis.Data;

/// <summary>
/// Extension reference list for compressor analysis picklists (analysis mode and compressor kind).
/// Codes align with <see cref="CompressorAnalysisWellKnown"/> wire values (shared in Models).
/// </summary>
public partial class R_COMPRESSOR_ANALYSIS_REFERENCE_CODE : ModelEntityBase
{
    private string REFERENCE_SETValue = string.Empty;

    public string REFERENCE_SET
    {
        get => REFERENCE_SETValue;
        set => SetProperty(ref REFERENCE_SETValue, value);
    }

    private string REFERENCE_CODEValue = string.Empty;

    public string REFERENCE_CODE
    {
        get => REFERENCE_CODEValue;
        set => SetProperty(ref REFERENCE_CODEValue, value);
    }

    private string LONG_NAMEValue = string.Empty;

    public string LONG_NAME
    {
        get => LONG_NAMEValue;
        set => SetProperty(ref LONG_NAMEValue, value);
    }
}
