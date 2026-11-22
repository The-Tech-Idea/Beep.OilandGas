using System.Globalization;
using Beep.OilandGas.EnhancedRecovery.Constants;
using Beep.OilandGas.EnhancedRecovery.Data;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;

/// <summary>The three states a screened method can be in (RES-04).</summary>
public static class EorStates
{
    public const string Eligible = "ELIGIBLE";
    public const string Ineligible = "INELIGIBLE";

    /// <summary>AC-2: a method that could not be judged is not the same as one that failed.</summary>
    public const string InsufficientData = "INSUFFICIENT_DATA";

    public static IReadOnlyList<string> All { get; } = [Eligible, Ineligible, InsufficientData];
}

/// <summary>The reservoir and fluid inputs of a screening run (RES-04).</summary>
public sealed record OgEorInputs(
    string? FieldId,
    string? PoolId,
    decimal? ApiGravity,
    decimal? ViscosityCp,
    decimal? DepthFt,
    decimal? TemperatureF);

/// <summary>One screened method, matching the plan's <c>OgEorCandidate</c>.</summary>
public sealed record OgEorCandidate(
    string Method,
    string MethodName,
    string State,
    decimal? Score,
    string DecidingRule,
    decimal? IncrementalRecovery,
    decimal? CostValue,
    string? CostBasis,
    bool Chosen)
{
    public bool IsEligible => string.Equals(State, EorStates.Eligible, StringComparison.Ordinal);

    /// <summary>AC-2: "insufficient data" is its own answer, never a rejection.</summary>
    public bool IsInsufficient => string.Equals(State, EorStates.InsufficientData, StringComparison.Ordinal);

    public bool IsIneligible => string.Equals(State, EorStates.Ineligible, StringComparison.Ordinal);

    public string StateText => State.ToLowerInvariant().Replace('_', ' ');

    public string ScoreText => Score is { } score
        ? (score * 100m).ToString("0", CultureInfo.InvariantCulture) + "%"
        : "not scored";

    /// <summary>AC-4: the recovery factor is the module's, and it says so; it is not a forecast.</summary>
    public string RecoveryText => IncrementalRecovery is { } recovery
        ? recovery.ToString("0.##", CultureInfo.InvariantCulture) + "% OOIP (screening default)"
        : "not stated by the module";

    public string CostText => CostValue is { } cost
        ? cost.ToString("#,0.##", CultureInfo.InvariantCulture) + " · " + (CostBasis ?? string.Empty)
        : "unavailable: no cost for this method is in the module's constants";

    public string ChosenText => Chosen ? "chosen" : "not chosen";

    public string Describe() =>
        $"{MethodName} ({Method}) · {StateText} · {ScoreText} · {DecidingRule} · {RecoveryText} · {ChosenText}";
}

/// <summary>The screening of one pool (RES-04).</summary>
public sealed record OgEorWorkspace(
    IReadOnlyList<OgEorCandidate> Candidates,
    OgEorInputs Inputs,
    string? RunId,
    string Summary,
    string EconomicsText,
    bool EconomicsAvailable,
    string MethodsText,
    int EligibleCount,
    int IneligibleCount,
    int InsufficientCount,
    string? Actor,
    DateTime? RecordedAt,
    IReadOnlyList<OgEorSavedScheme> History,
    bool HistoryFailed,
    string? HistoryReason)
{
    public OgEorCandidate? Best => Candidates
        .Where(candidate => candidate.IsEligible)
        .OrderByDescending(candidate => candidate.Score ?? 0m)
        .FirstOrDefault();

    public string HistoryText => History.Count == 0
        ? "No screening run is saved yet."
        : string.Join(Environment.NewLine, History.Select(scheme => scheme.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved history could not be read; new screens are still allowed"
        : $"{History.Count.ToString(CultureInfo.InvariantCulture)} saved run(s)";

    public string CandidateText => Candidates.Count == 0
        ? "no method has been screened yet"
        : string.Join(Environment.NewLine, Candidates.Select(candidate => candidate.Describe()));
}

/// <summary>A saved screening run, as the history reads it (RES-04 AC-5).</summary>
public sealed record OgEorSavedScheme(
    string RunId,
    string? FieldId,
    string? PoolId,
    decimal? ApiGravity,
    decimal? ViscosityCp,
    decimal? DepthFt,
    string? ChosenMethod,
    int EligibleCount,
    string? Actor,
    DateTime? RecordedAt)
{
    public string Describe() =>
        $"run {RunId} · pool {PoolId ?? "(none)"} · API {ApiGravity?.ToString("0.##", CultureInfo.InvariantCulture) ?? "(none)"} · " +
        $"viscosity {ViscosityCp?.ToString("#,0.##", CultureInfo.InvariantCulture) ?? "(none)"} cp · " +
        $"{EligibleCount.ToString(CultureInfo.InvariantCulture)} eligible · " +
        $"chosen {ChosenMethod ?? "(none)"} · saved by {Actor ?? "(unknown)"}";
}

/// <summary>The screening rules, stated once so the page, the service and the tests agree.</summary>
public static class EorRules
{
    public const string CodeNoConnection = "eor-no-connection";
    public const string CodeReadFailed = "eor-read-failed";
    public const string CodeWriteFailed = "eor-write-failed";
    public const string CodeActorRequired = "eor-actor-required";
    public const string CodeInputsRequired = "eor-inputs-required";
    public const string CodeApiInvalid = "eor-api-invalid";
    public const string CodeViscosityInvalid = "eor-viscosity-invalid";
    public const string CodeNothingToSave = "eor-nothing-to-save";
    public const string CodeChosenUnknown = "eor-chosen-method-unknown";
    public const string CodeChosenIneligible = "eor-chosen-method-ineligible";

    /// <summary>The methods screened, in the order the reference set lists them.</summary>
    public static IReadOnlyList<string> Methods { get; } =
    [
        EorMethodConstants.WaterFlood,
        EorMethodConstants.GasInjection,
        EorMethodConstants.CO2Miscible,
        EorMethodConstants.HydrocarbonMiscible,
        EorMethodConstants.Polymer,
        EorMethodConstants.ASP,
        EorMethodConstants.Chemical,
        EorMethodConstants.Steam,
        EorMethodConstants.Thermal,
    ];

    public static string NameOf(string method) => method switch
    {
        EorMethodConstants.WaterFlood => "Water flooding",
        EorMethodConstants.GasInjection => "Gas injection",
        EorMethodConstants.CO2Miscible => "CO2 miscible",
        EorMethodConstants.HydrocarbonMiscible => "Hydrocarbon miscible",
        EorMethodConstants.Polymer => "Polymer flooding",
        EorMethodConstants.ASP => "ASP",
        EorMethodConstants.Chemical => "Chemical flooding",
        EorMethodConstants.Steam => "Steam flooding",
        EorMethodConstants.Thermal => "Thermal recovery",
        _ => method,
    };

    /// <summary>
    /// AC-3: every threshold is read from <see cref="EnhancedRecoveryConstants.ScreeningWindows"/>; this type holds no
    /// number of its own.
    /// </summary>
    public static EnhancedRecoveryConstants.ScreeningWindow WindowOf(string method) =>
        EnhancedRecoveryConstants.ScreeningWindows.TryGetValue(method, out var window)
            ? window
            : new EnhancedRecoveryConstants.ScreeningWindow(
                null, null, null, null, "the module states no screening window for this method");

    /// <summary>AC-4: the module's own screening recovery factor for a method, or null when it states none.</summary>
    public static decimal? RecoveryFactorOf(string method) => method switch
    {
        EorMethodConstants.WaterFlood => EnhancedRecoveryConstants.RecoveryFactorWaterFlood,
        EorMethodConstants.GasInjection => EnhancedRecoveryConstants.RecoveryFactorGasInjection,
        EorMethodConstants.CO2Miscible => EnhancedRecoveryConstants.RecoveryFactorCO2Miscible,
        EorMethodConstants.HydrocarbonMiscible => EnhancedRecoveryConstants.RecoveryFactorGasInjection,
        EorMethodConstants.Polymer => EnhancedRecoveryConstants.RecoveryFactorChemical,
        EorMethodConstants.ASP => EnhancedRecoveryConstants.RecoveryFactorChemical,
        EorMethodConstants.Chemical => EnhancedRecoveryConstants.RecoveryFactorChemical,
        EorMethodConstants.Steam => EnhancedRecoveryConstants.RecoveryFactorThermal,
        EorMethodConstants.Thermal => EnhancedRecoveryConstants.RecoveryFactorThermal,
        EorMethodConstants.Injection => EnhancedRecoveryConstants.RecoveryFactorInjection,
        _ => EnhancedRecoveryConstants.RecoveryFactorDefault,
    };

    /// <summary>
    /// The only costs the module states are the chemical ones, and they are per unit of chemical - not per incremental
    /// barrel. They are shown with that basis spelled out; every other method has no cost here, so the panel says the
    /// economics is unavailable instead of inventing a figure (AC-4).
    /// </summary>
    public static (decimal? Value, string? Basis)? CostOf(string method) => method switch
    {
        EorMethodConstants.Polymer => ((decimal)ChemicalTypeConstants.CostPolymer,
            "polymer cost per unit of chemical (ChemicalTypeConstants), not a cost per incremental barrel"),
        EorMethodConstants.ASP => ((decimal)ChemicalTypeConstants.CostSurfactant,
            "surfactant cost per unit of chemical (ChemicalTypeConstants), not a cost per incremental barrel"),
        EorMethodConstants.Chemical => ((decimal)ChemicalTypeConstants.CostDefault,
            "chemical cost per unit of chemical (ChemicalTypeConstants), not a cost per incremental barrel"),
        _ => null,
    };

    /// <summary>The validation the screen needs before it can judge anything.</summary>
    public static (string Code, string Message)? Validate(OgEorInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (inputs.ApiGravity is null && inputs.ViscosityCp is null)
        {
            return (CodeInputsRequired,
                "A screen needs at least the oil's API gravity or its viscosity; without either no method can be judged.");
        }

        if (inputs.ApiGravity is <= 0m or > 100m)
        {
            return (CodeApiInvalid, "API gravity must be above 0 and at most 100.");
        }

        return inputs.ViscosityCp is <= 0m
            ? (CodeViscosityInvalid, "Oil viscosity must be above 0 cp.")
            : null;
    }
}

/// <summary>RES-04: EOR screening.</summary>
public interface IOgEorScreening
{
    /// <summary>Judges every method against the inputs. Nothing is written.</summary>
    Task<OgResult<OgEorWorkspace>> ScreenAsync(OgEorInputs inputs, CancellationToken cancellationToken = default);

    /// <summary>Saves the run, its candidates and the chosen scheme (AC-5).</summary>
    Task<OgResult<OgEorWorkspace>> SaveAsync(
        OgEorWorkspace workspace,
        string? chosenMethod,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved runs and their summaries; an unavailable history is a state.</summary>
    Task<OgResult<OgEorWorkspace>> LoadAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// RES-04 over the EnhancedRecovery module's constants and the <c>EOR_SCREENING_RUN</c> and
/// <c>EOR_SCREENING_CANDIDATE</c> rows, through a unit of work.
/// </summary>
/// <remarks>
/// The screening windows, the recovery factors and the chemical costs are all read from
/// <c>EnhancedRecoveryConstants</c> and <c>ChemicalTypeConstants</c>; this service and the page hold none of those
/// numbers (AC-3). A method with a missing input is reported as insufficient data rather than ineligible (AC-2), and the
/// economics panel reports only what the module states (AC-4).
/// </remarks>
public sealed class OgEorScreeningService : IOgEorScreening
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgEorScreeningService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public Task<OgResult<OgEorWorkspace>> ScreenAsync(
        OgEorInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        cancellationToken.ThrowIfCancellationRequested();

        if (EorRules.Validate(inputs) is { } refusal)
        {
            return Task.FromResult(OgResult<OgEorWorkspace>.Failed(refusal.Message, refusal.Code));
        }

        var candidates = EorRules.Methods.Select(method => Judge(method, inputs)).ToList();

        return Task.FromResult(OgResult<OgEorWorkspace>.Ok(Workspace(candidates, inputs, null, null, null, [], false, null)));
    }

    public async Task<OgResult<OgEorWorkspace>> SaveAsync(
        OgEorWorkspace workspace,
        string? chosenMethod,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgEorWorkspace>.Failed(
                "A saved scheme must record who saved it.", EorRules.CodeActorRequired);
        }

        if (workspace.Candidates.Count == 0)
        {
            return OgResult<OgEorWorkspace>.Failed(
                "There is no screening to save; screen the pool first.", EorRules.CodeNothingToSave);
        }

        var chosen = string.IsNullOrWhiteSpace(chosenMethod) ? null : chosenMethod.Trim().ToUpperInvariant();

        if (chosen is not null)
        {
            var candidate = workspace.Candidates.FirstOrDefault(item =>
                string.Equals(item.Method, chosen, StringComparison.OrdinalIgnoreCase));

            if (candidate is null)
            {
                return OgResult<OgEorWorkspace>.Failed(
                    $"'{chosen}' is not one of the screened methods.", EorRules.CodeChosenUnknown);
            }

            // A scheme is only saved for a method the screen judged eligible: an ineligible or unjudged method is not a
            // scheme, and saying nothing about it would be worse than refusing.
            if (!candidate.IsEligible)
            {
                return OgResult<OgEorWorkspace>.Failed(
                    $"{EorRules.NameOf(candidate.Method)} is {candidate.StateText}, so it cannot be saved as this " +
                    $"pool's scheme: {candidate.DecidingRule}", EorRules.CodeChosenIneligible);
            }
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgEorWorkspace>.Failed(
                "No connection is open, so the scheme cannot be saved.", EorRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var runId = Guid.NewGuid().ToString("N");
            var inputs = workspace.Inputs;

            var runUnit = OgUow.For<EOR_SCREENING_RUN>(source);
            runUnit.PrimaryKey = "RUN_ID";

            runUnit.Add(new EOR_SCREENING_RUN
            {
                RUN_ID = runId,
                FIELD_ID = inputs.FieldId,
                POOL_ID = inputs.PoolId,
                API_GRAVITY = inputs.ApiGravity,
                VISCOSITY_CP = inputs.ViscosityCp,
                DEPTH_FT = inputs.DepthFt,
                TEMPERATURE_F = inputs.TemperatureF,
                CANDIDATE_COUNT = workspace.Candidates.Count,
                ELIGIBLE_COUNT = workspace.EligibleCount,
                INSUFFICIENT_COUNT = workspace.InsufficientCount,
                CHOSEN_METHOD = chosen,
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var candidateUnit = OgUow.For<EOR_SCREENING_CANDIDATE>(source);
            candidateUnit.PrimaryKey = "CANDIDATE_ID";

            foreach (var candidate in workspace.Candidates)
            {
                candidateUnit.Add(new EOR_SCREENING_CANDIDATE
                {
                    CANDIDATE_ID = Guid.NewGuid().ToString("N"),
                    RUN_ID = runId,
                    METHOD_CODE = candidate.Method,
                    STATE = candidate.State,
                    DECIDING_RULE = candidate.DecidingRule,
                    SCORE = candidate.Score,
                    INCREMENTAL_RECOVERY = candidate.IncrementalRecovery,
                    COST_VALUE = candidate.CostValue,
                    COST_BASIS = candidate.CostBasis,
                    CHOSEN_IND = string.Equals(candidate.Method, chosen, StringComparison.OrdinalIgnoreCase) ? "Y" : "N",
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }

            var committedCandidates = await candidateUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committedCandidates))
            {
                return OgResult<OgEorWorkspace>.Failed(
                    OgUow.Message(committedCandidates, "The candidates were not saved, so no run was recorded."),
                    EorRules.CodeWriteFailed);
            }

            var committedRun = await runUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committedRun))
            {
                return OgResult<OgEorWorkspace>.Failed(
                    OgUow.Message(committedRun, "The run was not saved."), EorRules.CodeWriteFailed);
            }

            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            var marked = workspace.Candidates
                .Select(candidate => candidate with
                {
                    Chosen = string.Equals(candidate.Method, chosen, StringComparison.OrdinalIgnoreCase),
                })
                .ToList();

            return OgResult<OgEorWorkspace>.Ok(Workspace(
                marked,
                inputs,
                runId,
                actor.Trim(),
                now,
                history,
                false,
                null));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving an EOR screening for pool {workspace.Inputs.PoolId}",
                "the scheme was not saved",
                FailureSeverity.Error);

            return OgResult<OgEorWorkspace>.Failed(
                "The scheme could not be saved.", reference ?? EorRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgEorWorkspace>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgEorWorkspace>.Failed(
                "No connection is open, so the EOR screening history cannot be read.", EorRules.CodeNoConnection);
        }

        try
        {
            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgEorWorkspace>.Ok(Workspace(
                [], new OgEorInputs(null, null, null, null, null, null), null, null, null, history, false, null));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the EOR screening history",
                "the saved history is unavailable; new screens are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgEorWorkspace>.Ok(Workspace(
                [], new OgEorInputs(null, null, null, null, null, null), null, null, null, [], true,
                $"the saved history could not be read (reference {reference})"));
        }
    }

    // ---------------------------------------------------------------- the screen

    /// <summary>
    /// AC-1 and AC-2: every method gets a rule sentence, and a method whose window needs a value the inputs do not carry
    /// is "insufficient data" with the missing value named - never ineligible.
    /// </summary>
    public static OgEorCandidate Judge(string method, OgEorInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var window = EorRules.WindowOf(method);
        var missing = new List<string>();

        if ((window.MinimumApiGravity is not null || window.MaximumApiGravity is not null) && inputs.ApiGravity is null)
        {
            missing.Add("API gravity");
        }

        if (window.MaximumViscosityCp is not null && inputs.ViscosityCp is null)
        {
            missing.Add("oil viscosity");
        }

        // The CO2 depth rule is the one window that needs a depth; without it the method cannot be judged.
        var needsDepth = string.Equals(method, EorMethodConstants.CO2Miscible, StringComparison.OrdinalIgnoreCase);

        if (needsDepth && inputs.DepthFt is null)
        {
            missing.Add("depth");
        }

        if (missing.Count > 0)
        {
            return Candidate(
                method,
                EorStates.InsufficientData,
                null,
                $"insufficient data: the screen needs {string.Join(" and ", missing)} to judge this method, so it is " +
                "not judged rather than called ineligible");
        }

        var breaks = new List<string>();

        if (window.MinimumApiGravity is { } minimumApi && inputs.ApiGravity is { } apiBelow && apiBelow < minimumApi)
        {
            breaks.Add(
                $"API gravity {apiBelow.ToString("0.##", CultureInfo.InvariantCulture)} is below the " +
                $"{minimumApi.ToString("0.##", CultureInfo.InvariantCulture)} minimum");
        }

        if (window.MaximumApiGravity is { } maximumApi && inputs.ApiGravity is { } apiAbove && apiAbove > maximumApi)
        {
            breaks.Add(
                $"API gravity {apiAbove.ToString("0.##", CultureInfo.InvariantCulture)} is above the " +
                $"{maximumApi.ToString("0.##", CultureInfo.InvariantCulture)} maximum");
        }

        if (window.MaximumViscosityCp is { } maximumViscosity && inputs.ViscosityCp is { } viscosity
            && viscosity > maximumViscosity)
        {
            breaks.Add(
                $"oil viscosity {viscosity.ToString("#,0.##", CultureInfo.InvariantCulture)} cp is above the " +
                $"{maximumViscosity.ToString("#,0.##", CultureInfo.InvariantCulture)} cp maximum");
        }

        if (needsDepth && inputs.ApiGravity is { } api && inputs.DepthFt is { } depth)
        {
            var requiredDepth = EnhancedRecoveryConstants.Co2MinimumDepthFor(api);

            if (requiredDepth is null)
            {
                breaks.Add(
                    $"API gravity {api.ToString("0.##", CultureInfo.InvariantCulture)} is below the 22 the CO2 " +
                    "miscible screen starts at, whatever the depth");
            }
            else if (depth < requiredDepth.Value)
            {
                breaks.Add(
                    $"depth {depth.ToString("#,0.##", CultureInfo.InvariantCulture)} ft is shallower than the " +
                    $"{requiredDepth.Value.ToString("#,0.##", CultureInfo.InvariantCulture)} ft this oil gravity " +
                    "needs for miscibility");
            }
        }

        if (breaks.Count > 0)
        {
            return Candidate(
                method,
                EorStates.Ineligible,
                null,
                $"ineligible: the window is {window.Basis}; {string.Join("; ", breaks)}");
        }

        return Candidate(
            method,
            EorStates.Eligible,
            Score(method, inputs, window),
            $"eligible: the window is {window.Basis}; {Satisfied(inputs, window)}");
    }

    /// <summary>
    /// The score is this screen's own ranking heuristic - the mean relative margin inside the window, 1 at the ideal end
    /// and approaching 0 at the boundary. It is a way to order eligible methods, not a Taber value, and the page says so.
    /// </summary>
    private static decimal? Score(
        string method,
        OgEorInputs inputs,
        EnhancedRecoveryConstants.ScreeningWindow window)
    {
        var margins = new List<decimal>();

        if (window.MinimumApiGravity is { } minimumApi && inputs.ApiGravity is { } api)
        {
            var headroom = 100m - minimumApi;

            if (headroom > 0m)
            {
                margins.Add(Math.Clamp((api - minimumApi) / headroom, 0m, 1m));
            }
        }

        if (window.MaximumApiGravity is { } maximumApi && window.MinimumApiGravity is { } lowest
            && inputs.ApiGravity is { } apiInBand)
        {
            var span = maximumApi - lowest;

            if (span > 0m)
            {
                margins.Add(1m - Math.Clamp(Math.Abs(apiInBand - ((maximumApi + lowest) / 2m)) / (span / 2m), 0m, 1m));
            }
        }

        if (window.MaximumViscosityCp is { } maximumViscosity && inputs.ViscosityCp is { } viscosity
            && maximumViscosity > 0m)
        {
            margins.Add(Math.Clamp(1m - (viscosity / maximumViscosity), 0m, 1m));
        }

        if (string.Equals(method, EorMethodConstants.CO2Miscible, StringComparison.OrdinalIgnoreCase)
            && inputs.ApiGravity is { } apiForDepth && inputs.DepthFt is { } depth
            && EnhancedRecoveryConstants.Co2MinimumDepthFor(apiForDepth) is { } requiredDepth && requiredDepth > 0m)
        {
            margins.Add(Math.Clamp((depth - requiredDepth) / requiredDepth, 0m, 1m));
        }

        return margins.Count == 0 ? null : Math.Round(margins.Average(), 4);
    }

    private static string Satisfied(OgEorInputs inputs, EnhancedRecoveryConstants.ScreeningWindow window)
    {
        var facts = new List<string>();

        if (inputs.ApiGravity is { } api)
        {
            facts.Add($"API gravity {api.ToString("0.##", CultureInfo.InvariantCulture)}");
        }

        if (inputs.ViscosityCp is { } viscosity)
        {
            facts.Add($"viscosity {viscosity.ToString("#,0.##", CultureInfo.InvariantCulture)} cp");
        }

        if (inputs.DepthFt is { } depth && window.MinimumDepthFt is not null)
        {
            facts.Add($"depth {depth.ToString("#,0.##", CultureInfo.InvariantCulture)} ft");
        }

        return facts.Count == 0
            ? "no property is constrained for this method"
            : string.Join(", ", facts) + " all sit inside it";
    }

    private static OgEorCandidate Candidate(string method, string state, decimal? score, string rule)
    {
        var cost = EorRules.CostOf(method);

        return new OgEorCandidate(
            method,
            EorRules.NameOf(method),
            state,
            score,
            rule,
            EorRules.RecoveryFactorOf(method),
            cost?.Value,
            cost?.Basis,
            false);
    }

    private static OgEorWorkspace Workspace(
        IReadOnlyList<OgEorCandidate> candidates,
        OgEorInputs inputs,
        string? runId,
        string? actor,
        DateTime? recordedAt,
        IReadOnlyList<OgEorSavedScheme> history,
        bool historyFailed,
        string? historyReason)
    {
        var eligible = candidates.Count(candidate => candidate.IsEligible);
        var ineligible = candidates.Count(candidate => candidate.IsIneligible);
        var insufficient = candidates.Count(candidate => candidate.IsInsufficient);

        var summary = candidates.Count == 0
            ? "no method has been screened yet"
            : $"{candidates.Count.ToString(CultureInfo.InvariantCulture)} method(s) · " +
              $"{eligible.ToString(CultureInfo.InvariantCulture)} eligible · " +
              $"{ineligible.ToString(CultureInfo.InvariantCulture)} ineligible · " +
              $"{insufficient.ToString(CultureInfo.InvariantCulture)} with insufficient data";

        return new OgEorWorkspace(
            candidates,
            inputs,
            runId,
            summary,
            EconomicsText,
            false,
            string.Join(", ", EorRules.Methods.Select(method => EorRules.NameOf(method))),
            eligible,
            ineligible,
            insufficient,
            actor,
            recordedAt,
            history,
            historyFailed,
            historyReason);
    }

    /// <summary>
    /// AC-4: the economics the screen can honestly show is the module's screening recovery factor and, for the chemical
    /// methods, the module's chemical cost. A cost per incremental barrel and an incremental NPV are ECO-01's job, which
    /// is not built, so the panel says exactly that instead of producing a number.
    /// </summary>
    private const string EconomicsText =
        "Recovery is the module's screening factor (% OOIP) and is not a forecast. The only costs the module states are " +
        "the chemical ones, per unit of chemical; a cost per incremental barrel and an incremental NPV belong to ECO-01, " +
        "which is not built, so they are not shown rather than estimated here.";

    private static async Task<IReadOnlyList<OgEorSavedScheme>> ReadHistoryAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var runs = await OgUow.ReadAllAsync<EOR_SCREENING_RUN>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return runs
            .Where(row => !string.IsNullOrWhiteSpace(row.RUN_ID))
            .OrderByDescending(row => row.RECORDED_AT ?? DateTime.MinValue)
            .ThenBy(row => row.RUN_ID, StringComparer.Ordinal)
            .Select(row => new OgEorSavedScheme(
                row.RUN_ID ?? string.Empty,
                row.FIELD_ID,
                row.POOL_ID,
                row.API_GRAVITY,
                row.VISCOSITY_CP,
                row.DEPTH_FT,
                row.CHOSEN_METHOD,
                row.ELIGIBLE_COUNT,
                row.ACTOR,
                row.RECORDED_AT))
            .Take(50)
            .ToList();
    }
}
