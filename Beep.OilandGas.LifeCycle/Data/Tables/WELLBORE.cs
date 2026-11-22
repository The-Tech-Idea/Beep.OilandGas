using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// A physical hole under a well: the original bore, a sidetrack or a re-drill (WEL-03).
/// </summary>
/// <remarks>
/// Not part of standard PPDM 3.9 - the LifeCycle project owns it, and the LIFECYCLE module registers it so module
/// setup creates the table. The key is (UWI, WELLBORE_ID): a bore never exists without its parent well (AC-2).
/// </remarks>
public class WELLBORE : ModelEntityBase
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

    [System.ComponentModel.DataAnnotations.MaxLength(120)]
    public string? WELLBORE_NAME
    {
        get => WellboreNameValue;
        set => SetProperty(ref WellboreNameValue, value);
    }

    private string? WellboreNameValue;

    /// <summary>The bore this one was drilled from; required for a sidetrack or a re-drill (AC-5).</summary>
    [System.ComponentModel.DataAnnotations.MaxLength(40)]
    public string? PARENT_WELLBORE_ID
    {
        get => ParentWellboreIdValue;
        set => SetProperty(ref ParentWellboreIdValue, value);
    }

    private string? ParentWellboreIdValue;

    /// <summary>ORIGINAL, SIDETRACK or REDRILL.</summary>
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string? KIND
    {
        get => KindValue;
        set => SetProperty(ref KindValue, value);
    }

    private string? KindValue;

    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    public string? STATUS
    {
        get => StatusValue;
        set => SetProperty(ref StatusValue, value);
    }

    private string? StatusValue;

    public System.DateTime? SPUD_DATE
    {
        get => SpudDateValue;
        set => SetProperty(ref SpudDateValue, value);
    }

    private System.DateTime? SpudDateValue;

    public System.Decimal? TOTAL_DEPTH
    {
        get => TotalDepthValue;
        set => SetProperty(ref TotalDepthValue, value);
    }

    private System.Decimal? TotalDepthValue;

    /// <summary>The unit of TOTAL_DEPTH; without it, survey and geometry screens are disabled (AC-4).</summary>
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string? TOTAL_DEPTH_OUOM
    {
        get => TotalDepthUnitValue;
        set => SetProperty(ref TotalDepthUnitValue, value);
    }

    private string? TotalDepthUnitValue;

    public System.Decimal? SURFACE_LATITUDE
    {
        get => SurfaceLatitudeValue;
        set => SetProperty(ref SurfaceLatitudeValue, value);
    }

    private System.Decimal? SurfaceLatitudeValue;

    public System.Decimal? SURFACE_LONGITUDE
    {
        get => SurfaceLongitudeValue;
        set => SetProperty(ref SurfaceLongitudeValue, value);
    }

    private System.Decimal? SurfaceLongitudeValue;

    [System.ComponentModel.DataAnnotations.MaxLength(40)]
    public string? DEPTH_DATUM
    {
        get => DepthDatumValue;
        set => SetProperty(ref DepthDatumValue, value);
    }

    private string? DepthDatumValue;

    [System.ComponentModel.DataAnnotations.MaxLength(60)]
    public string? ROW_CREATED_BY
    {
        get => RowCreatedByValue;
        set => SetProperty(ref RowCreatedByValue, value);
    }

    private string? RowCreatedByValue;

    public System.DateTime? ROW_CREATED_DATE
    {
        get => RowCreatedDateValue;
        set => SetProperty(ref RowCreatedDateValue, value);
    }

    private System.DateTime? RowCreatedDateValue;

    [System.ComponentModel.DataAnnotations.MaxLength(60)]
    public string? ROW_CHANGED_BY
    {
        get => RowChangedByValue;
        set => SetProperty(ref RowChangedByValue, value);
    }

    private string? RowChangedByValue;

    public System.DateTime? ROW_CHANGED_DATE
    {
        get => RowChangedDateValue;
        set => SetProperty(ref RowChangedDateValue, value);
    }

    private System.DateTime? RowChangedDateValue;
}
