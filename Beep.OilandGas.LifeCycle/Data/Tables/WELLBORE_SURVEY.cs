using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Which directional survey belongs to which wellbore (WEL-03 AC-3).
/// </summary>
/// <remarks>
/// PPDM's WELL_DIR_SRVY is keyed by the well, not the bore, so the bore association is its own table. Nothing
/// populates it yet - the survey pages (VSB-07, ENG-01) will - so today it only blocks a delete when a link exists.
/// </remarks>
public class WELLBORE_SURVEY : ModelEntityBase
{
    [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.MaxLength(40)]
    public string UWI
    {
        get => UwiValue;
        set => SetProperty(ref UwiValue, value);
    }

    private string UwiValue = string.Empty;

    [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.MaxLength(40)]
    public string WELLBORE_ID
    {
        get => WellboreIdValue;
        set => SetProperty(ref WellboreIdValue, value);
    }

    private string WellboreIdValue = string.Empty;

    [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.MaxLength(40)]
    public string SURVEY_ID
    {
        get => SurveyIdValue;
        set => SetProperty(ref SurveyIdValue, value);
    }

    private string? SurveyIdValue = string.Empty;

    [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.MaxLength(60)]
    public string SOURCE
    {
        get => SourceValue;
        set => SetProperty(ref SourceValue, value);
    }

    private string SourceValue = string.Empty;
}
