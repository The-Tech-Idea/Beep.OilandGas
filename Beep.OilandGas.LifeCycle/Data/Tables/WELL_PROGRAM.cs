using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// A drilling proposal for a prospect, and the gate decision on it (EXP-05).
/// </summary>
/// <remarks>
/// Not part of standard PPDM 3.9 - the LifeCycle project owns it, and the LIFECYCLE module registers it so module
/// setup creates the table. The state machine is DRAFT -&gt; PENDING -&gt; (APPROVED | APPROVED_WITH_CONDITIONS |
/// RETURNED | REJECTED); RETURNED and REJECTED programs may be revised and resubmitted. Decisions are appended to
/// <see cref="WELL_PROGRAM_DECISION"/>, never overwritten here.
/// </remarks>
public class WELL_PROGRAM : ModelEntityBase
{
    [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.MaxLength(128)]
    public string PROGRAM_ID
    {
        get => ProgramIdValue;
        set => SetProperty(ref ProgramIdValue, value);
    }

    private string ProgramIdValue = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(40)]
    public string? PROSPECT_ID
    {
        get => ProspectIdValue;
        set => SetProperty(ref ProspectIdValue, value);
    }

    private string? ProspectIdValue;

    [System.ComponentModel.DataAnnotations.MaxLength(40)]
    public string? FIELD_ID
    {
        get => FieldIdValue;
        set => SetProperty(ref FieldIdValue, value);
    }

    private string? FieldIdValue;

    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string? PROGRAM_NAME
    {
        get => ProgramNameValue;
        set => SetProperty(ref ProgramNameValue, value);
    }

    private string? ProgramNameValue;

    [System.ComponentModel.DataAnnotations.MaxLength(1000)]
    public string? OBJECTIVE
    {
        get => ObjectiveValue;
        set => SetProperty(ref ObjectiveValue, value);
    }

    private string? ObjectiveValue;

    public decimal? TARGET_DEPTH
    {
        get => TargetDepthValue;
        set => SetProperty(ref TargetDepthValue, value);
    }

    private decimal? TargetDepthValue;

    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string? DEPTH_OUOM
    {
        get => DepthUnitValue;
        set => SetProperty(ref DepthUnitValue, value);
    }

    private string? DepthUnitValue;

    public decimal? ESTIMATED_COST
    {
        get => EstimatedCostValue;
        set => SetProperty(ref EstimatedCostValue, value);
    }

    private decimal? EstimatedCostValue;

    [System.ComponentModel.DataAnnotations.MaxLength(10)]
    public string? CURRENCY
    {
        get => CurrencyValue;
        set => SetProperty(ref CurrencyValue, value);
    }

    private string? CurrencyValue;

    /// <summary>DRAFT, PENDING, APPROVED, APPROVED_WITH_CONDITIONS, RETURNED or REJECTED.</summary>
    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    public string? STATE
    {
        get => StateValue;
        set => SetProperty(ref StateValue, value);
    }

    private string? StateValue;

    /// <summary>The person who submitted the program; the same person may not decide it.</summary>
    [System.ComponentModel.DataAnnotations.MaxLength(60)]
    public string? AUTHOR
    {
        get => AuthorValue;
        set => SetProperty(ref AuthorValue, value);
    }

    private string? AuthorValue;

    public System.DateTime? SUBMITTED_AT
    {
        get => SubmittedAtValue;
        set => SetProperty(ref SubmittedAtValue, value);
    }

    private System.DateTime? SubmittedAtValue;

    [System.ComponentModel.DataAnnotations.MaxLength(60)]
    public string? DECIDED_BY
    {
        get => DecidedByValue;
        set => SetProperty(ref DecidedByValue, value);
    }

    private string? DecidedByValue;

    public System.DateTime? DECIDED_AT
    {
        get => DecidedAtValue;
        set => SetProperty(ref DecidedAtValue, value);
    }

    private System.DateTime? DecidedAtValue;

    /// <summary>The latest decision's rationale.</summary>
    [System.ComponentModel.DataAnnotations.MaxLength(1000)]
    public string? RATIONALE
    {
        get => RationaleValue;
        set => SetProperty(ref RationaleValue, value);
    }

    private string? RationaleValue;

    /// <summary>The conditions attached to an approval, one per line (DEV-05 reads them as constraints).</summary>
    [System.ComponentModel.DataAnnotations.MaxLength(2000)]
    public string? CONDITIONS_TEXT
    {
        get => ConditionsValue;
        set => SetProperty(ref ConditionsValue, value);
    }

    private string? ConditionsValue;

    /// <summary>Increments on every decision, so a decision loaded earlier cannot overwrite a newer one.</summary>
    public int? ROW_VERSION
    {
        get => RowVersionValue;
        set => SetProperty(ref RowVersionValue, value);
    }

    private int? RowVersionValue;

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
