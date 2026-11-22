using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.EnhancedRecovery.Data;

/// <summary>
/// One method's outcome in an EOR screening run (RES-04): whether it is eligible, insufficient data or ineligible, the
/// rule that decided it, its score and the recovery factor the module's constants give it.
/// </summary>
/// <remarks>
/// The deciding rule is a stored column, not a sentence rebuilt in the view, because AC-1 requires every method to state
/// the rule that decided it and a saved scheme must still show the rule it was decided under.
/// </remarks>
public class EOR_SCREENING_CANDIDATE : ModelEntityBase
{
    [Key, MaxLength(64)]
    public string CANDIDATE_ID
    {
        get => CandidateIdValue;
        set => SetProperty(ref CandidateIdValue, value);
    }

    private string CandidateIdValue = string.Empty;

    [MaxLength(64)]
    public string RUN_ID
    {
        get => RunIdValue;
        set => SetProperty(ref RunIdValue, value);
    }

    private string RunIdValue = string.Empty;

    /// <summary>The EorMethodConstants code this row judged.</summary>
    [MaxLength(32)]
    public string METHOD_CODE
    {
        get => MethodCodeValue;
        set => SetProperty(ref MethodCodeValue, value);
    }

    private string MethodCodeValue = string.Empty;

    /// <summary>ELIGIBLE, INELIGIBLE or INSUFFICIENT_DATA. Insufficient data is never stored as ineligible (AC-2).</summary>
    [MaxLength(32)]
    public string STATE
    {
        get => StateValue;
        set => SetProperty(ref StateValue, value);
    }

    private string StateValue = string.Empty;

    /// <summary>The rule that decided the state, including the window and the values it was applied to.</summary>
    [MaxLength(512)]
    public string DECIDING_RULE
    {
        get => DecidingRuleValue;
        set => SetProperty(ref DecidingRuleValue, value);
    }

    private string DecidingRuleValue = string.Empty;

    /// <summary>Relative position inside the window, 0 to 1; null when the method could not be scored.</summary>
    public decimal? SCORE
    {
        get => ScoreValue;
        set => SetProperty(ref ScoreValue, value);
    }

    private decimal? ScoreValue;

    /// <summary>
    /// The module's screening recovery factor for this method (% OOIP), from EnhancedRecoveryConstants; null when the
    /// method has none.
    /// </summary>
    public decimal? INCREMENTAL_RECOVERY
    {
        get => IncrementalRecoveryValue;
        set => SetProperty(ref IncrementalRecoveryValue, value);
    }

    private decimal? IncrementalRecoveryValue;

    /// <summary>
    /// A cost the module's own constants state for this method, with <see cref="COST_BASIS"/> saying what it is. Null
    /// where the constants state none - the panel then says the economics is unavailable rather than inventing one.
    /// </summary>
    public decimal? COST_VALUE
    {
        get => CostValueValue;
        set => SetProperty(ref CostValueValue, value);
    }

    private decimal? CostValueValue;

    [MaxLength(256)]
    public string? COST_BASIS
    {
        get => CostBasisValue;
        set => SetProperty(ref CostBasisValue, value);
    }

    private string? CostBasisValue;

    /// <summary>'Y' on the method the operator chose as the pool's scheme.</summary>
    [MaxLength(1)]
    public string CHOSEN_IND
    {
        get => ChosenIndValue;
        set => SetProperty(ref ChosenIndValue, value);
    }

    private string ChosenIndValue = "N";
}
