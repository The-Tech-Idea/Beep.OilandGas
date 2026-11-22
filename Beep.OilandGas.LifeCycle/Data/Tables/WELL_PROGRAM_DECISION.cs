using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// One gate decision on a well program (EXP-05). Append-only: a decision is never edited or removed.
/// </summary>
/// <remarks>
/// The program row keeps the current state and the latest decision for fast reads; this table keeps every decision
/// with its actor, date, rationale, conditions and the state it produced, which is what the history panel shows.
/// </remarks>
public class WELL_PROGRAM_DECISION : ModelEntityBase
{
    [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.MaxLength(128)]
    public string PROGRAM_ID
    {
        get => ProgramIdValue;
        set => SetProperty(ref ProgramIdValue, value);
    }

    private string ProgramIdValue = string.Empty;

    [System.ComponentModel.DataAnnotations.Key]
    public int DECISION_SEQ_NO
    {
        get => DecisionSeqNoValue;
        set => SetProperty(ref DecisionSeqNoValue, value);
    }

    private int DecisionSeqNoValue;

    /// <summary>APPROVE, APPROVE_WITH_CONDITIONS, RETURN or REJECT.</summary>
    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    public string? DECISION
    {
        get => DecisionValue;
        set => SetProperty(ref DecisionValue, value);
    }

    private string? DecisionValue;

    /// <summary>The state the decision produced.</summary>
    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    public string? RESULTING_STATE
    {
        get => ResultingStateValue;
        set => SetProperty(ref ResultingStateValue, value);
    }

    private string? ResultingStateValue;

    [System.ComponentModel.DataAnnotations.MaxLength(1000)]
    public string? RATIONALE
    {
        get => RationaleValue;
        set => SetProperty(ref RationaleValue, value);
    }

    private string? RationaleValue;

    [System.ComponentModel.DataAnnotations.MaxLength(2000)]
    public string? CONDITIONS_TEXT
    {
        get => ConditionsValue;
        set => SetProperty(ref ConditionsValue, value);
    }

    private string? ConditionsValue;

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
