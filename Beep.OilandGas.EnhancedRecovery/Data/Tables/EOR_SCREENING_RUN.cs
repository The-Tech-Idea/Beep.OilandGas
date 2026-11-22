using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.EnhancedRecovery.Data;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// One EOR screening run (RES-04): the pool it was made for, the reservoir and fluid inputs it used, and who ran it.
/// </summary>
/// <remarks>
/// The inputs are stored on the run rather than read back from the pool, so a saved scheme stays reproducible after the
/// pool's recorded properties change - which is what AC-5 asks for ("a saved scheme links to the pool and the screening
/// inputs").
/// </remarks>
public class EOR_SCREENING_RUN : ModelEntityBase
{
    [Key, MaxLength(64)]
    public string RUN_ID
    {
        get => RunIdValue;
        set => SetProperty(ref RunIdValue, value);
    }

    private string RunIdValue = string.Empty;

    [MaxLength(64)]
    public string? FIELD_ID
    {
        get => FieldIdValue;
        set => SetProperty(ref FieldIdValue, value);
    }

    private string? FieldIdValue;

    /// <summary>The pool the screen was made for; the link AC-5 requires.</summary>
    [MaxLength(64)]
    public string? POOL_ID
    {
        get => PoolIdValue;
        set => SetProperty(ref PoolIdValue, value);
    }

    private string? PoolIdValue;

    public decimal? API_GRAVITY
    {
        get => ApiGravityValue;
        set => SetProperty(ref ApiGravityValue, value);
    }

    private decimal? ApiGravityValue;

    public decimal? VISCOSITY_CP
    {
        get => ViscosityValue;
        set => SetProperty(ref ViscosityValue, value);
    }

    private decimal? ViscosityValue;

    public decimal? DEPTH_FT
    {
        get => DepthValue;
        set => SetProperty(ref DepthValue, value);
    }

    private decimal? DepthValue;

    public decimal? TEMPERATURE_F
    {
        get => TemperatureValue;
        set => SetProperty(ref TemperatureValue, value);
    }

    private decimal? TemperatureValue;

    public int CANDIDATE_COUNT
    {
        get => CandidateCountValue;
        set => SetProperty(ref CandidateCountValue, value);
    }

    private int CandidateCountValue;

    /// <summary>How many methods came out eligible, so the run reads without its candidate rows.</summary>
    public int ELIGIBLE_COUNT
    {
        get => EligibleCountValue;
        set => SetProperty(ref EligibleCountValue, value);
    }

    private int EligibleCountValue;

    public int INSUFFICIENT_COUNT
    {
        get => InsufficientCountValue;
        set => SetProperty(ref InsufficientCountValue, value);
    }

    private int InsufficientCountValue;

    /// <summary>The method saved as the scheme for this pool, if one was chosen.</summary>
    [MaxLength(32)]
    public string? CHOSEN_METHOD
    {
        get => ChosenMethodValue;
        set => SetProperty(ref ChosenMethodValue, value);
    }

    private string? ChosenMethodValue;

    [MaxLength(128)]
    public string? ACTOR
    {
        get => ActorValue;
        set => SetProperty(ref ActorValue, value);
    }

    private string? ActorValue;

    public DateTime? RECORDED_AT
    {
        get => RecordedAtValue;
        set => SetProperty(ref RecordedAtValue, value);
    }

    private DateTime? RecordedAtValue;
}
