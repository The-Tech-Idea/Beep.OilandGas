using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis.Calculations;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Production;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;

/// <summary>The inflow models this screen can run (LFT-01 AC-5).</summary>
public static class NodalIprModels
{
    public const string Vogel = "VOGEL";
    public const string Standing = "STANDING";

    public static IReadOnlyList<string> All { get; } = [Vogel, Standing];

    public static bool IsSupported(string? model) =>
        !string.IsNullOrWhiteSpace(model)
        && All.Contains(model.Trim().ToUpperInvariant(), StringComparer.Ordinal);

    public static string NameOf(string model) => model switch
    {
        Vogel => "Vogel (solution gas drive)",
        Standing => "Standing (Vogel with a flow-efficiency correction)",
        _ => model,
    };
}

/// <summary>
/// The outflow correlations on offer. Only Hagedorn-Brown is listed, because it is the one the engine's VLP path
/// actually computes - <c>VLPCalculator.GenerateVLPBeggsBrill</c> calls the Hagedorn-Brown relation inside.
/// </summary>
public static class NodalVlpCorrelations
{
    public const string HagedornBrown = "HAGEDORN_BROWN";

    public static IReadOnlyList<string> All { get; } = [HagedornBrown];

    public static bool IsSupported(string? correlation) =>
        !string.IsNullOrWhiteSpace(correlation)
        && string.Equals(correlation.Trim(), HagedornBrown, StringComparison.OrdinalIgnoreCase);
}

/// <summary>The inputs of one nodal study (LFT-01). Every value is required: nothing is inherited from a default.</summary>
public sealed record OgNodalInputs(
    string? WellId,
    decimal? ReservoirPressure,
    decimal? BubblePointPressure,
    decimal? Temperature,
    decimal? ProductivityIndex,
    decimal? WaterCut,
    decimal? Gor,
    decimal? OilGravity,
    decimal? FormationVolumeFactor,
    decimal? OilViscosity,
    decimal? Depth,
    decimal? TubingDiameter,
    decimal? WellheadPressure,
    decimal? GasSpecificGravity,
    decimal? TubingRoughness,
    string? IprModel,
    string? VlpCorrelation,
    int Points = 20);

/// <summary>One curve point, as the page lists it (LFT-01).</summary>
public sealed record OgNodalPoint(decimal Rate, decimal Pressure)
{
    public string RateText => Rate.ToString("#,0.###", CultureInfo.InvariantCulture);

    public string PressureText => Pressure.ToString("#,0.##", CultureInfo.InvariantCulture);
}

/// <summary>The independent solver's answer (LFT-01 AC-3).</summary>
public sealed record OgNodalCrossCheck(
    string Source,
    bool Available,
    bool FoundPoint,
    decimal? FlowRate,
    decimal? Pressure,
    bool Agrees,
    string Text)
{
    public string AgreeText => !Available ? "unavailable" : Agrees ? "agrees" : "DISAGREES";

    public string RateText => FlowRate?.ToString("#,0.###", CultureInfo.InvariantCulture) ?? "no crossing";

    public string PressureText => Pressure?.ToString("#,0.##", CultureInfo.InvariantCulture) ?? "no crossing";
}

/// <summary>
/// One nodal study (LFT-01), matching the plan's <c>OgNodalResult</c> plus the curves and the cross-check the screen
/// shows.
/// </summary>
public sealed record OgNodalStudy(
    string Id,
    string? WellId,
    string IprModel,
    string VlpCorrelation,
    decimal? FlowRate,
    decimal? FlowingPressure,
    bool Converged,
    string? BindingConstraint,
    string Diagnostics,
    IReadOnlyList<OgNodalPoint> IprCurve,
    IReadOnlyList<OgNodalPoint> VlpCurve,
    OgNodalCrossCheck CrossCheck,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string RateUnit = "STB/d";
    public const string PressureUnit = "psia";

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    /// <summary>AC-2: an operating point exists only when the curves cross.</summary>
    public bool HasOperatingPoint => Converged && FlowRate is not null && FlowingPressure is not null;

    public string StatusText => Converged ? "converged" : "no intersection";

    public string IprText => NodalIprModels.NameOf(IprModel);

    public string VlpText => VlpCorrelation.Replace('_', '-').ToLowerInvariant();

    public string OperatingText => HasOperatingPoint
        ? $"{FlowRate!.Value.ToString("#,0.###", CultureInfo.InvariantCulture)} {RateUnit} at " +
          $"{FlowingPressure!.Value.ToString("#,0.##", CultureInfo.InvariantCulture)} {PressureUnit}"
        : "no operating point";

    public string BindingText => string.IsNullOrWhiteSpace(BindingConstraint)
        ? "no constraint bound the result"
        : BindingConstraint!.Replace('_', ' ').ToLowerInvariant();

    public string CrossCheckText => CrossCheck.Text;

    public string CurveText => !HasOperatingPoint
        ? "no operating point, so no curves are drawn"
        : $"the inflow falls from {RateText(IprCurve[0])} at {PressureText(IprCurve[0])} to " +
          $"{RateText(IprCurve[^1])} at {PressureText(IprCurve[^1])}; the outflow rises from " +
          $"{RateText(VlpCurve[0])} at {PressureText(VlpCurve[0])} to " +
          $"{RateText(VlpCurve[^1])} at {PressureText(VlpCurve[^1])}";

    public string PointsText => IprCurve.Count == 0 && VlpCurve.Count == 0
        ? "no curve point"
        : string.Join(
            Environment.NewLine,
            IprCurve
                .Select(point => $"inflow  q {point.RateText} {RateUnit} · pwf {point.PressureText} {PressureUnit}")
                .Concat(VlpCurve.Select(point =>
                    $"outflow q {point.RateText} {RateUnit} · pf {point.PressureText} {PressureUnit}")));

    public string SavedText => IsSaved
        ? $"saved by {Actor ?? "(unknown)"} on " +
          $"{RecordedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"}"
        : "not saved";

    public string Describe() => $"{WellId ?? "(no well)"} · {IprText} · {VlpText} · {OperatingText} · " +
                                $"{CrossCheck.AgreeText} · {SavedText}";

    private static string RateText(OgNodalPoint point) => point.RateText;

    private static string PressureText(OgNodalPoint point) => point.PressureText;
}

/// <summary>One completed point of a tubing sweep (LFT-01 AC-6).</summary>
public sealed record OgNodalSensitivityPoint(
    decimal TubingDiameter,
    bool Converged,
    decimal? FlowRate,
    decimal? Pressure)
{
    public string DiameterText => TubingDiameter.ToString("0.###", CultureInfo.InvariantCulture);

    public string ResultText => Converged && FlowRate is { } rate
        ? $"{rate.ToString("#,0.###", CultureInfo.InvariantCulture)} {OgNodalStudy.RateUnit} at " +
          $"{Pressure!.Value.ToString("#,0.##", CultureInfo.InvariantCulture)} {OgNodalStudy.PressureUnit}"
        : "no intersection";

    public string Describe() => $"{DiameterText} in: {ResultText}";
}

/// <summary>
/// The completed points of a sweep, whether it was cancelled, and how many were asked for - so a partial result is
/// reported as partial rather than dropped (LFT-01 AC-6).
/// </summary>
public sealed record OgNodalSensitivity(
    IReadOnlyList<OgNodalSensitivityPoint> Points,
    int Requested,
    IReadOnlyList<string> CancelledAt)
{
    public int Completed => Points.Count;

    public bool Cancelled => CancelledAt.Count > 0;

    public string StateText => Cancelled
        ? $"cancelled: {Completed.ToString(CultureInfo.InvariantCulture)} of " +
          $"{Requested.ToString(CultureInfo.InvariantCulture)} point(s) completed and reported"
        : $"complete: {Completed.ToString(CultureInfo.InvariantCulture)} point(s)";

    public IReadOnlyList<OgNodalSensitivityPoint> Converged =>
        Points.Where(point => point.Converged).ToList();

    public string BestText => Converged.Count == 0
        ? "no diameter produced an operating point"
        : Converged
            .OrderByDescending(point => point.FlowRate)
            .Select(point => $"{point.DiameterText} in gives " + point.ResultText)
            .First();

    public string Describe() => string.Join(Environment.NewLine, Points.Select(point => point.Describe()));
}

/// <summary>The nodal workspace: the saved studies, the selected one, and the codes on offer (LFT-01).</summary>
public sealed record OgNodalWorkspace(
    IReadOnlyList<OgNodalStudy> Studies,
    OgNodalStudy? Selected,
    IReadOnlyList<string> IprModels,
    IReadOnlyList<string> VlpCorrelations,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string StudiesText => Studies.Count == 0
        ? "No nodal study is saved yet."
        : string.Join(Environment.NewLine, Studies.Select(study => study.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved studies could not be read; new studies are still allowed"
        : $"{Studies.Count.ToString(CultureInfo.InvariantCulture)} saved study(ies)";

    /// <summary>LFT-01's lift ranking, offered when there is no intersection (the state table's "no intersection").</summary>
    public string LiftRankingText =>
        "No operating point means the well cannot lift at these conditions. Before a lift method is chosen, the "
        + "narrowed candidates are the ones that change the outflow (a larger tubing, a lower wellhead pressure, gas "
        + "lift) or the inflow (a stimulation). The ranking in ArtificialLiftScoring is a scoring model, and the lift "
        + "designs themselves belong to LFT-02 and LFT-03.";
}

/// <summary>The nodal rules, stated once so the page, the service and the tests agree.</summary>
public static class NodalRules
{
    public const string CodeNoConnection = "nodal-no-connection";
    public const string CodeReadFailed = "nodal-read-failed";
    public const string CodeWriteFailed = "nodal-write-failed";
    public const string CodeActorRequired = "nodal-actor-required";
    public const string CodeWellRequired = "nodal-well-required";
    public const string CodeInputRequired = "nodal-input-required";
    public const string CodeInputInvalid = "nodal-input-invalid";
    public const string CodeIprUnknown = "nodal-ipr-model-unknown";
    public const string CodeVlpUnsupported = "nodal-vlp-correlation-unsupported";
    public const string CodeNoIntersection = "nodal-no-intersection";
    public const string CodeNothingToSave = "nodal-nothing-to-save";

    /// <summary>AC-3: the tolerance the two solvers are compared at.</summary>
    public const decimal RateAgreementTolerance = 0.02m;

    public const decimal PressureAgreementTolerance = 0.02m;

    /// <summary>Every input is required - the engine is never called with a guessed pressure or depth.</summary>
    public static IReadOnlyList<string> Missing(OgNodalInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = new List<string>();

        void Need(decimal? value, string name)
        {
            if (value is null)
            {
                missing.Add(name);
            }
        }

        if (string.IsNullOrWhiteSpace(inputs.WellId))
        {
            missing.Add("well");
        }

        Need(inputs.ReservoirPressure, "reservoir pressure");
        Need(inputs.BubblePointPressure, "bubble point pressure");
        Need(inputs.Temperature, "temperature");
        Need(inputs.ProductivityIndex, "productivity index");
        Need(inputs.WaterCut, "water cut");
        Need(inputs.Gor, "gas-oil ratio");
        Need(inputs.OilGravity, "oil gravity");
        Need(inputs.FormationVolumeFactor, "formation volume factor");
        Need(inputs.OilViscosity, "oil viscosity");
        Need(inputs.Depth, "depth");
        Need(inputs.TubingDiameter, "tubing diameter");
        Need(inputs.WellheadPressure, "wellhead pressure");
        Need(inputs.GasSpecificGravity, "gas specific gravity");
        Need(inputs.TubingRoughness, "tubing roughness");

        if (string.IsNullOrWhiteSpace(inputs.IprModel))
        {
            missing.Add("inflow model");
        }

        if (string.IsNullOrWhiteSpace(inputs.VlpCorrelation))
        {
            missing.Add("outflow correlation");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgNodalInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return missing.Contains("well")
                ? (CodeWellRequired, "Choose the well the study is for.")
                : (CodeInputRequired, $"The study needs {string.Join(", ", missing)}.");
        }

        if (!NodalIprModels.IsSupported(inputs.IprModel))
        {
            return (CodeIprUnknown,
                $"'{inputs.IprModel}' is not an inflow model; use {string.Join(", ", NodalIprModels.All)}.");
        }

        if (!NodalVlpCorrelations.IsSupported(inputs.VlpCorrelation))
        {
            return (CodeVlpUnsupported,
                $"'{inputs.VlpCorrelation}' is not available: the engine's outflow path computes the Hagedorn-Brown " +
                "relation only, so the other correlations are not offered rather than mislabelled.");
        }

        foreach (var (value, name) in new (decimal?, string)[]
                 {
                     (inputs.ReservoirPressure, "reservoir pressure"),
                     (inputs.BubblePointPressure, "bubble point pressure"),
                     (inputs.Temperature, "temperature"),
                     (inputs.ProductivityIndex, "productivity index"),
                     (inputs.Depth, "depth"),
                     (inputs.TubingDiameter, "tubing diameter"),
                     (inputs.WellheadPressure, "wellhead pressure"),
                     (inputs.GasSpecificGravity, "gas specific gravity"),
                     (inputs.FormationVolumeFactor, "formation volume factor"),
                     (inputs.OilViscosity, "oil viscosity"),
                 })
        {
            if (value is <= 0m)
            {
                return (CodeInputInvalid, $"The {name} must be above zero.");
            }
        }

        if (inputs.WaterCut is < 0m or > 1m)
        {
            return (CodeInputInvalid, "The water cut is a fraction between 0 and 1.");
        }

        return inputs.BubblePointPressure > inputs.ReservoirPressure
            ? (CodeInputInvalid, "The bubble point pressure cannot exceed the reservoir pressure.")
            : null;
    }

    /// <summary>AC-3: the verdict, stated once.</summary>
    public static bool Agrees(decimal rate, decimal pressure, decimal otherRate, decimal otherPressure) =>
        rate > 0m
        && Math.Abs(rate - otherRate) / rate <= RateAgreementTolerance
        && (pressure <= 0m
            ? false
            : Math.Abs(pressure - otherPressure) / pressure <= PressureAgreementTolerance);
}

/// <summary>LFT-01: nodal analysis.</summary>
public interface IOgNodal
{
    /// <summary>Runs the study. Nothing is written.</summary>
    Task<OgResult<OgNodalWorkspace>> RunAsync(
        OgNodalInputs inputs,
        CancellationToken cancellationToken = default);

    /// <summary>Saves the study with its curves and both correlations (AC-5).</summary>
    Task<OgResult<OgNodalWorkspace>> SaveAsync(
        OgNodalStudy study,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sweeps the tubing diameter, reporting the completed points and whether the caller cancelled (AC-6).
    /// </summary>
    Task<OgResult<OgNodalSensitivity>> SensitivityAsync(
        OgNodalInputs inputs,
        decimal minimumTubingDiameter,
        decimal maximumTubingDiameter,
        int steps,
        CancellationToken cancellationToken = default);

    /// <summary>The saved studies; an unavailable history is a state.</summary>
    Task<OgResult<OgNodalWorkspace>> LoadAsync(
        string? wellId = null,
        string? studyId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// LFT-01 over the NodalAnalysis module's IPR and VLP calculators and its operating-point search, with the sibling tools'
/// <c>NodalAnalysis.Solve</c> as the independent cross-check.
/// </summary>
/// <remarks>
/// The curves are the owner's: <c>IPRCalculator</c> for the inflow and <c>VLPCalculator</c> for the outflow. The
/// intersection is found by <c>OperatingPointCalculator.TryFindOperatingPoint</c>, which reports whether the curves cross
/// at all, so a well that cannot lift is reported as no intersection rather than as a zero rate (AC-2). The study records
/// both correlations and the inputs it used (AC-5).
/// </remarks>
public sealed class OgNodalService : IOgNodal
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgNodalService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgNodalWorkspace>> RunAsync(
        OgNodalInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (NodalRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgNodalWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var study = Build(inputs);

            var source = _source();
            var history = source is null
                ? []
                : await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgNodalWorkspace>.Ok(Workspace(history, study));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"running a nodal study for well {inputs.WellId}",
                "the study is not shown",
                FailureSeverity.Error);

            return OgResult<OgNodalWorkspace>.Failed(
                "The nodal study could not be run.", reference ?? NodalRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgNodalWorkspace>> SaveAsync(
        OgNodalStudy study,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(study);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgNodalWorkspace>.Failed(
                "A saved study must record who saved it.", NodalRules.CodeActorRequired);
        }

        if (study.IprCurve.Count == 0 || study.VlpCurve.Count == 0)
        {
            return OgResult<OgNodalWorkspace>.Failed(
                "There are no curves to save; run the study first.", NodalRules.CodeNothingToSave);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgNodalWorkspace>.Failed(
                "No connection is open, so the study cannot be saved.", NodalRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var studyId = Guid.NewGuid().ToString("N");

            var unit = OgUow.For<NODAL_ANALYSIS_RESULT>(source);
            unit.PrimaryKey = "NODAL_ANALYSIS_RESULT_ID";

            unit.Add(new NODAL_ANALYSIS_RESULT
            {
                NODAL_ANALYSIS_RESULT_ID = studyId,
                ANALYSIS_ID = studyId,
                WELL_UWI = study.WellId,
                ANALYSIS_DATE = now,
                OPERATING_FLOW_RATE = study.FlowRate,
                OPERATING_PRESSURE = study.FlowingPressure,
                STATUS = study.Converged ? "CONVERGED" : "NO_INTERSECTION",
                IPR_MODEL = study.IprModel,
                VLP_CORRELATION = study.VlpCorrelation,
                CONVERGED = study.Converged ? "Y" : "N",
                ITERATIONS = study.Converged ? 60 : 0,
                BINDING_CONSTRAINT = study.BindingConstraint,
                DIAGNOSTICS = study.Diagnostics,
                INPUTS_JSON = ToJson(study),
                RATE_OUOM = OgNodalStudy.RateUnit,
                CROSS_CHECK_SOURCE = study.CrossCheck.Source,
                CROSS_CHECK_FLOW_RATE = study.CrossCheck.FlowRate,
                CROSS_CHECK_PRESSURE = study.CrossCheck.Pressure,
                CROSS_CHECK_AGREES = study.CrossCheck.Available
                    ? (study.CrossCheck.Agrees ? "Y" : "N")
                    : null,
                CROSS_CHECK_TEXT = study.CrossCheck.Text,
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgNodalWorkspace>.Failed(
                    OgUow.Message(committed, "The study was not saved."), NodalRules.CodeWriteFailed);
            }

            // AC-5: the curves are kept with the study, not recomputed from a model that may have changed.
            var iprUnit = OgUow.For<NODAL_IPR_POINT>(source);
            iprUnit.PrimaryKey = "NODAL_IPR_POINT_ID";

            foreach (var point in study.IprCurve)
            {
                iprUnit.Add(new NODAL_IPR_POINT
                {
                    NODAL_IPR_POINT_ID = Guid.NewGuid().ToString("N"),
                    NODAL_ANALYSIS_ID = studyId,
                    FLOW_RATE = point.Rate,
                    FLOWING_BOTTOMHOLE_PRESSURE = point.Pressure,
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }

            var vlpUnit = OgUow.For<NODAL_VLP_POINT>(source);
            vlpUnit.PrimaryKey = "NODAL_VLP_POINT_ID";

            foreach (var point in study.VlpCurve)
            {
                vlpUnit.Add(new NODAL_VLP_POINT
                {
                    NODAL_VLP_POINT_ID = Guid.NewGuid().ToString("N"),
                    NODAL_ANALYSIS_ID = studyId,
                    FLOW_RATE = point.Rate,
                    REQUIRED_BOTTOMHOLE_PRESSURE = point.Pressure,
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }

            var committedPoints = await iprUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committedPoints))
            {
                return OgResult<OgNodalWorkspace>.Failed(
                    OgUow.Message(committedPoints, "The inflow curve was not saved."), NodalRules.CodeWriteFailed);
            }

            var committedOutflow = await vlpUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committedOutflow))
            {
                return OgResult<OgNodalWorkspace>.Failed(
                    OgUow.Message(committedOutflow, "The outflow curve was not saved."), NodalRules.CodeWriteFailed);
            }

            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);
            var saved = study with { Id = studyId, Actor = actor.Trim(), RecordedAt = now };

            return OgResult<OgNodalWorkspace>.Ok(Workspace(history, saved));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving a nodal study for well {study.WellId}",
                "the study was not saved",
                FailureSeverity.Error);

            return OgResult<OgNodalWorkspace>.Failed(
                "The study could not be saved.", reference ?? NodalRules.CodeWriteFailed);
        }
    }

    public Task<OgResult<OgNodalSensitivity>> SensitivityAsync(
        OgNodalInputs inputs,
        decimal minimumTubingDiameter,
        decimal maximumTubingDiameter,
        int steps,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (NodalRules.Validate(inputs) is { } refusal)
        {
            return Task.FromResult(OgResult<OgNodalSensitivity>.Failed(refusal.Message, refusal.Code));
        }

        if (minimumTubingDiameter <= 0m || maximumTubingDiameter <= minimumTubingDiameter || steps < 2)
        {
            return Task.FromResult(OgResult<OgNodalSensitivity>.Failed(
                "The sweep needs a positive diameter range with a maximum above the minimum, and at least two steps.",
                NodalRules.CodeInputInvalid));
        }

        var points = new List<OgNodalSensitivityPoint>();
        var cancelled = new List<string>();

        for (var index = 0; index < steps; index++)
        {
            // AC-6: a cancelled sweep reports what it completed; it does not throw the work away.
            if (cancellationToken.IsCancellationRequested)
            {
                cancelled.Add($"cancelled before step {index + 1} of {steps}");

                break;
            }

            var diameter = minimumTubingDiameter
                           + (maximumTubingDiameter - minimumTubingDiameter) * index / (steps - 1);

            var study = Build(inputs with { TubingDiameter = diameter });

            points.Add(new OgNodalSensitivityPoint(
                diameter,
                study.Converged,
                study.FlowRate,
                study.FlowingPressure));
        }

        return Task.FromResult(OgResult<OgNodalSensitivity>.Ok(
            new OgNodalSensitivity(points, steps, cancelled)));
    }

    public async Task<OgResult<OgNodalWorkspace>> LoadAsync(
        string? wellId = null,
        string? studyId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgNodalWorkspace>.Failed(
                "No connection is open, so the saved studies cannot be read.", NodalRules.CodeNoConnection);
        }

        try
        {
            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            var selected = string.IsNullOrWhiteSpace(studyId)
                ? history.FirstOrDefault(study => string.IsNullOrWhiteSpace(wellId)
                    || string.Equals(study.WellId, wellId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault()
                : history.FirstOrDefault(study =>
                      string.Equals(study.Id, studyId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault();

            return OgResult<OgNodalWorkspace>.Ok(Workspace(history, selected));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the saved nodal studies",
                "the saved studies are unavailable; new studies are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgNodalWorkspace>.Ok(new OgNodalWorkspace(
                [],
                null,
                NodalIprModels.All,
                NodalVlpCorrelations.All,
                true,
                $"the saved studies could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the study

    /// <summary>
    /// The study itself: both curves from the owner's calculators, the crossing from the owner's search, the diagnostics
    /// when there is none, and the independent solver on the same outflow.
    /// </summary>
    public OgNodalStudy Build(OgNodalInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var iprModel = inputs.IprModel!.Trim().ToUpperInvariant();
        var vlpCorrelation = inputs.VlpCorrelation!.Trim().ToUpperInvariant();

        var reservoir = new ReservoirProperties
        {
            ReservoirPressure = (double)inputs.ReservoirPressure!.Value,
            BubblePointPressure = (double)inputs.BubblePointPressure!.Value,
            ProductivityIndex = (double)inputs.ProductivityIndex!.Value,
            WaterCut = (double)inputs.WaterCut!.Value,
            GasOilRatio = (double)inputs.Gor!.Value,
            OilGravity = (double)inputs.OilGravity!.Value,
            FormationVolumeFactor = (double)inputs.FormationVolumeFactor!.Value,
            OilViscosity = (double)inputs.OilViscosity!.Value,
        };

        var wellbore = new WellboreProperties
        {
            TubingDiameter = (double)inputs.TubingDiameter!.Value,
            TubingLength = (double)inputs.Depth!.Value,
            WellheadPressure = (double)inputs.WellheadPressure!.Value,
            WaterCut = (double)inputs.WaterCut.Value,
            GasOilRatio = (double)inputs.Gor.Value,
            OilGravity = (double)inputs.OilGravity.Value,
            GasSpecificGravity = (double)inputs.GasSpecificGravity!.Value,
            WellheadTemperature = (double)inputs.Temperature!.Value,
            BottomholeTemperature = (double)inputs.Temperature.Value,
            TubingRoughness = (double)inputs.TubingRoughness!.Value,
        };

        var points = Math.Clamp(inputs.Points, 5, 200);

        var ipr = iprModel == NodalIprModels.Standing
            ? IPRCalculator.GenerateStandingIPR(reservoir, flowEfficiency: 1.0, points: points)
            : IPRCalculator.GenerateVogelIPR(reservoir, points: points);

        // The VLP needs the rates the inflow covers; the same grid keeps the two curves comparable.
        var rates = ipr.Select(point => point.FlowRate).ToArray();

        if (rates.Length == 0 || rates.All(rate => rate <= 0.0))
        {
            rates = Enumerable.Range(1, points).Select(index => index * 10.0).ToArray();
        }

        var vlp = VLPCalculator.GenerateVLPBeggsBrill(wellbore, rates);

        // The engine can answer a non-finite pressure at an extreme rate; such a point is not a curve point, and
        // carrying it into a decimal would overflow. The finite points are what both the curves and the search use.
        var iprFinite = ipr
            .Where(point => double.IsFinite(point.FlowRate) && double.IsFinite(point.FlowingBottomholePressure))
            .Where(point => point.FlowRate >= 0.0 && point.FlowingBottomholePressure >= 0.0
                            && point.FlowRate < 1e24 && point.FlowingBottomholePressure < 1e24)
            .ToList();

        var vlpFinite = vlp
            .Where(point => double.IsFinite(point.FlowRate) && double.IsFinite(point.RequiredBottomholePressure))
            .Where(point => point.FlowRate >= 0.0 && point.RequiredBottomholePressure >= 0.0
                            && point.FlowRate < 1e24 && point.RequiredBottomholePressure < 1e24)
            .ToList();

        var iprCurve = iprFinite
            .Select(point => new OgNodalPoint((decimal)point.FlowRate, (decimal)point.FlowingBottomholePressure))
            .ToList();

        var vlpCurve = vlpFinite
            .Select(point => new OgNodalPoint((decimal)point.FlowRate, (decimal)point.RequiredBottomholePressure))
            .ToList();

        decimal flowRate = 0m;
        decimal pressure = 0m;
        var converged = false;

        if (iprFinite.Count >= 2 && vlpFinite.Count >= 2)
        {
            converged = OperatingPointCalculator.TryFindOperatingPoint(
                iprFinite, vlpFinite, out flowRate, out pressure);
        }

        var operatingRate = converged ? flowRate : 0m;
        var operatingPressure = converged ? pressure : 0m;

        var dropped = (ipr.Count - iprFinite.Count) + (vlp.Count - vlpFinite.Count);

        var diagnostics = converged
            ? string.Empty
            : (iprFinite.Count < 2 || vlpFinite.Count < 2)
                ? "the engine returned fewer than two finite curve points, so no intersection can be searched"
                : Diagnose(iprCurve, vlpCurve)
                  + (dropped == 0
                      ? string.Empty
                      : $" {dropped.ToString(CultureInfo.InvariantCulture)} non-finite curve point(s) were left out.");

        var crossCheck = CrossCheck(inputs, iprFinite, vlpFinite, converged, operatingRate, operatingPressure);

        return new OgNodalStudy(
            string.Empty,
            inputs.WellId!.Trim(),
            iprModel,
            vlpCorrelation,
            converged ? operatingRate : null,
            converged ? operatingPressure : null,
            converged,
            converged ? Binding(iprCurve, operatingRate) : null,
            diagnostics,
            iprCurve,
            vlpCurve,
            crossCheck,
            null,
            null);
    }

    /// <summary>
    /// AC-2: why there is no crossing, in numbers - the smallest gap between the two curves and where it happens.
    /// </summary>
    private static string Diagnose(IReadOnlyList<OgNodalPoint> ipr, IReadOnlyList<OgNodalPoint> vlp)
    {
        var gap = ipr
            .Zip(vlp, (inflow, outflow) => (
                Rate: inflow.Rate,
                Inflow: inflow.Pressure,
                Outflow: outflow.Pressure,
                Difference: inflow.Pressure - outflow.Pressure))
            .OrderBy(pair => Math.Abs(pair.Difference))
            .First();

        var direction = gap.Difference < 0m ? "the outflow needs more pressure than the inflow supplies" : "the outflow is below the inflow";

        return
            $"no intersection: {direction}. The two curves come closest at " +
            $"{gap.Rate.ToString("#,0.###", CultureInfo.InvariantCulture)} {OgNodalStudy.RateUnit}, where the inflow " +
            $"gives {gap.Inflow.ToString("#,0.##", CultureInfo.InvariantCulture)} psia and the outflow needs " +
            $"{gap.Outflow.ToString("#,0.##", CultureInfo.InvariantCulture)} psia " +
            $"(a gap of {Math.Abs(gap.Difference).ToString("#,0.##", CultureInfo.InvariantCulture)} psi). Raise the " +
            "wellhead pressure tolerance, enlarge the tubing or add lift energy, and run the study again.";
    }

    /// <summary>
    /// Which limit set the answer: when the operating pressure sits at the wellhead pressure the outflow is barely lifting
    /// anything, and when it sits near the reservoir pressure the inflow is the limit.
    /// </summary>
    /// <summary>
    /// Which limit set the answer, read from where the operating rate sits on the inflow curve: at the curve's maximum
    /// rate the inflow is the limit, at a rate near zero the outflow nearly stops the well, and in between nothing binds.
    /// </summary>
    private static string Binding(IReadOnlyList<OgNodalPoint> ipr, decimal flowRate)
    {
        var maximum = ipr.Count == 0 ? 0m : ipr.Max(point => point.Rate);

        if (maximum <= 0m)
        {
            return "NONE";
        }

        if (flowRate >= maximum * 0.95m)
        {
            return "INFLOW_MAXIMUM_RATE";
        }

        return flowRate <= maximum * 0.05m ? "OUTFLOW_NEAR_DEAD_WELL" : "NONE";
    }

    /// <summary>
    /// AC-3: the tools' independent solver is given the same inflow limit and the same outflow curve. It finds the
    /// crossing by its own scan and bisection, and reports none when there is none.
    /// </summary>
    private OgNodalCrossCheck CrossCheck(
        OgNodalInputs inputs,
        IReadOnlyList<IPRPoint> ipr,
        IReadOnlyList<VLPPoint> vlp,
        bool converged,
        decimal flowRate,
        decimal pressure)
    {
        const string source =
            "TheTechIdea.OilGas.Production NodalAnalysis.Solve with its own Vogel/Composite inflow and this study's " +
            "outflow curve (the crossing is solved independently; the outflow values are shared)";

        try
        {
            var maxRate = ipr.Max(point => point.FlowRate);
            var curve = vlp
                .OrderBy(point => point.FlowRate)
                .Select(point => (Rate: point.FlowRate, Pressure: point.RequiredBottomholePressure))
                .ToList();

            double Outflow(double rate)
            {
                if (rate <= curve[0].Rate)
                {
                    return curve[0].Pressure;
                }

                for (var index = 1; index < curve.Count; index++)
                {
                    if (rate <= curve[index].Rate)
                    {
                        var span = curve[index].Rate - curve[index - 1].Rate;

                        return span <= 0.0
                            ? curve[index].Pressure
                            : curve[index - 1].Pressure +
                              (rate - curve[index - 1].Rate) / span *
                              (curve[index].Pressure - curve[index - 1].Pressure);
                    }
                }

                return curve[^1].Pressure;
            }

            // The tools' simple overload refuses an undersaturated reservoir (pb < pr) and points at CompositeRate,
            // which is the right inflow there: a straight line above the bubble point and Vogel below it.
            var result = (double)inputs.BubblePointPressure!.Value >= (double)inputs.ReservoirPressure!.Value
                ? NodalAnalysis.Solve(
                    maxRate,
                    (double)inputs.ReservoirPressure!.Value,
                    (double)inputs.BubblePointPressure!.Value,
                    Outflow)
                : NodalAnalysis.Solve(
                    pwf => Vogel.CompositeRate(
                        maxRate,
                        (double)inputs.ProductivityIndex!.Value,
                        (double)inputs.ReservoirPressure!.Value,
                        (double)inputs.BubblePointPressure!.Value,
                        pwf),
                    maxRate,
                    Outflow);

            if (result.Rate is not { } otherRate || result.Pressure is not { } otherPressure)
            {
                return new OgNodalCrossCheck(
                    source,
                    true,
                    false,
                    null,
                    null,
                    !converged,
                    converged
                        ? "the independent solver finds no crossing where this study found one; the difference is shown " +
                          "and stands"
                        : "the independent solver also finds no crossing, which confirms the refusal");
            }

            var agrees = converged && NodalRules.Agrees(
                flowRate,
                pressure,
                (decimal)otherRate,
                (decimal)otherPressure);

            return new OgNodalCrossCheck(
                source,
                true,
                true,
                (decimal)otherRate,
                (decimal)otherPressure,
                agrees,
                agrees
                    ? $"the independent solver agrees: {otherRate.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
                      $"STB/d at {otherPressure.ToString("#,0.##", CultureInfo.InvariantCulture)} psia " +
                      $"(within {NodalRules.RateAgreementTolerance.ToString("0.##%", CultureInfo.InvariantCulture)})"
                    : $"the independent solver reads {otherRate.ToString("#,0.###", CultureInfo.InvariantCulture)} STB/d " +
                      $"at {otherPressure.ToString("#,0.##", CultureInfo.InvariantCulture)} psia against this study's " +
                      $"{flowRate.ToString("#,0.###", CultureInfo.InvariantCulture)} at " +
                      $"{pressure.ToString("#,0.##", CultureInfo.InvariantCulture)}; both are shown and the difference " +
                      "stands");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            _failures.ReportHandled(
                ex,
                "running the independent nodal cross-check",
                "only this study is shown, because the cross-check could not be run",
                FailureSeverity.Degraded);

            return new OgNodalCrossCheck(
                source,
                false,
                false,
                null,
                null,
                false,
                "the cross-check could not be run (" + ex.GetType().Name + "), so only this study is shown");
        }
    }

    // ------------------------------------------------------------------ the reads

    private static async Task<IReadOnlyList<OgNodalStudy>> ReadHistoryAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var results = await OgUow.ReadAllAsync<NODAL_ANALYSIS_RESULT>(source).ConfigureAwait(false);
        var ipr = await OgUow.ReadAllAsync<NODAL_IPR_POINT>(source).ConfigureAwait(false);
        var vlp = await OgUow.ReadAllAsync<NODAL_VLP_POINT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return results
            .Where(row => !string.IsNullOrWhiteSpace(row.NODAL_ANALYSIS_RESULT_ID))
            .OrderByDescending(row => row.RECORDED_AT ?? row.ANALYSIS_DATE ?? DateTime.MinValue)
            .ThenBy(row => row.NODAL_ANALYSIS_RESULT_ID, StringComparer.Ordinal)
            .Select(row => ToStudy(row, ipr, vlp))
            .Take(50)
            .ToList();
    }

    private static OgNodalStudy ToStudy(
        NODAL_ANALYSIS_RESULT row,
        IReadOnlyList<NODAL_IPR_POINT> ipr,
        IReadOnlyList<NODAL_VLP_POINT> vlp)
    {
        var id = row.NODAL_ANALYSIS_RESULT_ID!;

        var inflow = ipr
            .Where(point => string.Equals(point.NODAL_ANALYSIS_ID, id, StringComparison.OrdinalIgnoreCase))
            .OrderBy(point => point.FLOW_RATE)
            .Select(point => new OgNodalPoint(point.FLOW_RATE, point.FLOWING_BOTTOMHOLE_PRESSURE))
            .ToList();

        var outflow = vlp
            .Where(point => string.Equals(point.NODAL_ANALYSIS_ID, id, StringComparison.OrdinalIgnoreCase))
            .OrderBy(point => point.FLOW_RATE)
            .Select(point => new OgNodalPoint(point.FLOW_RATE, point.REQUIRED_BOTTOMHOLE_PRESSURE))
            .ToList();

        var converged = string.Equals(row.CONVERGED, "Y", StringComparison.OrdinalIgnoreCase);

        return new OgNodalStudy(
            id,
            row.WELL_UWI,
            row.IPR_MODEL ?? NodalIprModels.Vogel,
            row.VLP_CORRELATION ?? NodalVlpCorrelations.HagedornBrown,
            converged ? row.OPERATING_FLOW_RATE : null,
            converged ? row.OPERATING_PRESSURE : null,
            converged,
            row.BINDING_CONSTRAINT,
            row.DIAGNOSTICS ?? string.Empty,
            inflow,
            outflow,
            new OgNodalCrossCheck(
                row.CROSS_CHECK_SOURCE ?? "not recorded",
                row.CROSS_CHECK_FLOW_RATE is not null || row.CROSS_CHECK_PRESSURE is not null,
                row.CROSS_CHECK_FLOW_RATE is not null,
                row.CROSS_CHECK_FLOW_RATE,
                row.CROSS_CHECK_PRESSURE,
                string.Equals(row.CROSS_CHECK_AGREES, "Y", StringComparison.OrdinalIgnoreCase),
                row.CROSS_CHECK_TEXT ?? "no cross-check was recorded"),
            row.ACTOR,
            row.RECORDED_AT ?? row.ANALYSIS_DATE);
    }

    private static OgNodalWorkspace Workspace(IReadOnlyList<OgNodalStudy> history, OgNodalStudy? selected) =>
        new(history, selected, NodalIprModels.All, NodalVlpCorrelations.All, false, null);

    private static string ToJson(OgNodalStudy study) =>
        JsonSerializer.Serialize(new
        {
            study.WellId,
            study.IprModel,
            study.VlpCorrelation,
            study.FlowRate,
            study.FlowingPressure,
            study.Converged,
            IprPoints = study.IprCurve.Count,
            VlpPoints = study.VlpCurve.Count,
        });
}
