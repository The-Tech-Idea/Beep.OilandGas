using System.Globalization;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis.Calculations;
using Beep.OilandGas.WellTestAnalysis.Exceptions;
using Beep.OilandGas.WellTestAnalysis.Validation;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Production;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.WellTest;

/// <summary>The analysis types and methods, from <see cref="WellTestAnalysisWellKnown"/> (WTA-01 AC-2).</summary>
public static class WellTestAnalyses
{
    public const string BuildUp = WellTestAnalysisWellKnown.AnalysisClassification.BuildUp;
    public const string DrawDown = WellTestAnalysisWellKnown.AnalysisClassification.DrawDown;

    public const string Horner = WellTestAnalysisWellKnown.AnalysisMethod.Horner;
    public const string Mdh = WellTestAnalysisWellKnown.AnalysisMethod.Mdh;

    public static IReadOnlyList<string> Types { get; } = [BuildUp, DrawDown];

    public static IReadOnlyList<string> Methods { get; } = [Horner, Mdh];

    /// <summary>Which methods a type supports: a drawdown has no Horner straight line.</summary>
    public static IReadOnlyList<string> MethodsFor(string? type) =>
        string.Equals(type?.Trim(), DrawDown, StringComparison.OrdinalIgnoreCase) ? [Mdh] : [Horner, Mdh];

    public static bool IsBuildUp(string? type) =>
        string.Equals(type?.Trim(), BuildUp, StringComparison.OrdinalIgnoreCase);

    public static bool IsSupportedType(string? type) =>
        !string.IsNullOrWhiteSpace(type)
        && Types.Contains(type.Trim().ToUpperInvariant(), StringComparer.Ordinal);

    public static bool IsSupportedMethod(string? method) =>
        !string.IsNullOrWhiteSpace(method)
        && Methods.Contains(method.Trim().ToUpperInvariant(), StringComparer.Ordinal);
}

/// <summary>One pressure-time reading as the engineer enters it (WTA-01).</summary>
public sealed record OgWellTestReading(decimal TimeHours, decimal Pressure)
{
    public string Describe() =>
        TimeHours.ToString("#,0.####", CultureInfo.InvariantCulture) + " h · " +
        Pressure.ToString("#,0.##", CultureInfo.InvariantCulture) + " psia";
}

/// <summary>The inputs of one well test analysis (WTA-01).</summary>
public sealed record OgWellTestInputs(
    string? WellId,
    string? AnalysisType,
    string? Method,
    string? TimeUnit,
    string? PressureUnit,
    string? RateUnit,
    IReadOnlyList<OgWellTestReading> Readings,
    decimal? WindowStartHours,
    decimal? WindowEndHours,
    decimal? FlowRate,
    decimal? WellboreRadius,
    decimal? Thickness,
    decimal? Porosity,
    decimal? TotalCompressibility,
    decimal? Viscosity,
    decimal? FormationVolumeFactor,
    decimal? ProductionTimeHours,
    decimal? InitialReservoirPressure);

/// <summary>The independent cross-check's answer (WTA-01 AC-4, SG-08-2).</summary>
public sealed record OgWellTestCrossCheck(
    string Source,
    bool Available,
    decimal? Slope,
    decimal? Permeability,
    bool Agrees,
    string Text)
{
    public string AgreeText => !Available ? "unavailable" : Agrees ? "agrees" : "DISAGREES";

    public string SlopeText => Slope?.ToString("#,0.####", CultureInfo.InvariantCulture) ?? "not available";
}

/// <summary>One well test analysis (WTA-01), matching the plan's <c>OgWellTestResult</c> plus the derivative and window.</summary>
public sealed record OgWellTestResult(
    string Id,
    string? WellId,
    string AnalysisType,
    string Method,
    decimal Permeability,
    decimal Skin,
    decimal? WellboreStorage,
    decimal? ReservoirPressure,
    decimal? ProductivityIndex,
    bool Converged,
    decimal? RSquared,
    string? IdentifiedModel,
    decimal WindowStartHours,
    decimal WindowEndHours,
    int PointsInWindow,
    bool DerivativeUsable,
    string? DerivativeReason,
    int DerivativePoints,
    OgWellTestCrossCheck CrossCheck,
    string UnitsText,
    decimal? AnalysisSlope,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string PermeabilityUnit = "md";
    public const string PressureUnitText = "psia";
    public const string TimeUnitText = "h";

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    public string PermeabilityText =>
        Permeability.ToString("#,0.####", CultureInfo.InvariantCulture) + " " + PermeabilityUnit;

    public string SkinText => Skin.ToString("#,0.###", CultureInfo.InvariantCulture);

    public string PressureText => ReservoirPressure is { } pressure
        ? pressure.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + PressureUnitText
        : "not computed";

    public string SlopeText => AnalysisSlope is { } slope
        ? slope.ToString("#,0.####", CultureInfo.InvariantCulture) + " psi/cycle"
        : "not computed";

    public string WindowText =>
        $"{WindowStartHours.ToString("#,0.####", CultureInfo.InvariantCulture)} to " +
        $"{WindowEndHours.ToString("#,0.####", CultureInfo.InvariantCulture)} {TimeUnitText} " +
        $"({PointsInWindow.ToString(CultureInfo.InvariantCulture)} reading(s))";

    /// <summary>SG-08-3: a derivative that did not converge is unusable, and says so.</summary>
    public string DerivativeText => DerivativeUsable
        ? $"derivative usable: {DerivativePoints.ToString(CultureInfo.InvariantCulture)} point(s)"
        : "derivative UNUSABLE: " + (DerivativeReason ?? "it did not converge");

    public string ModelText => string.IsNullOrWhiteSpace(IdentifiedModel)
        ? "no model was identified"
        : IdentifiedModel!;

    public string FitText => RSquared is { } r2
        ? "R2 " + r2.ToString("0.####", CultureInfo.InvariantCulture)
        : "no fit quality reported";

    public string Describe() =>
        $"{WellId ?? "(no well)"} · {AnalysisType} · {Method} · k {PermeabilityText} · skin {SkinText} · " +
        $"window {WindowText} · {CrossCheck.AgreeText}";

    public string SavedText => IsSaved
        ? $"saved by {Actor ?? "(unknown)"} on " +
          $"{RecordedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"}"
        : "not saved";
}

/// <summary>A result whose save was not acknowledged: retained and retried (WTA-01's state table).</summary>
public sealed record OgWellTestPending(OgWellTestResult Result, string Actor);

/// <summary>The well test workspace (WTA-01).</summary>
public sealed record OgWellTestWorkspace(
    IReadOnlyList<OgWellTestResult> Results,
    OgWellTestResult? Selected,
    IReadOnlyList<string> Types,
    IReadOnlyList<string> Methods,
    IReadOnlyList<string> DisabledAnalyses,
    decimal CrossCheckTolerance,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string ResultsText => Results.Count == 0
        ? "No well test analysis is saved yet."
        : string.Join(Environment.NewLine, Results.Select(result => result.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved analyses could not be read; new analyses are still allowed"
        : $"{Results.Count.ToString(CultureInfo.InvariantCulture)} saved analysis(es)";

    /// <summary>The feature's open question, answered: which analyses are real and which are stubs.</summary>
    public string SupportedText =>
        $"analyses {string.Join(", ", Types)} · methods {string.Join(", ", Methods)} · not available " +
        $"({string.Join(", ", DisabledAnalyses)})";

    public string ReferenceText =>
        $"cross-check tolerance {CrossCheckTolerance.ToString("0.##%", CultureInfo.InvariantCulture)} on " +
        "permeability against the tools' HornerAnalysis";
}

/// <summary>The well test rules, stated once so the page, the service and the tests agree.</summary>
public static class WellTestRules
{
    public const string CodeNoConnection = "welltest-no-connection";
    public const string CodeReadFailed = "welltest-read-failed";
    public const string CodeWriteFailed = "welltest-write-failed";
    public const string CodeActorRequired = "welltest-actor-required";
    public const string CodeWellRequired = "welltest-well-required";
    public const string CodeInputRequired = "welltest-input-required";
    public const string CodeTypeUnknown = "welltest-type-unknown";
    public const string CodeMethodUnsupported = "welltest-method-unsupported";
    public const string CodeMixedUnits = "welltest-mixed-units";
    public const string CodeNoWindow = "welltest-no-window";
    public const string CodeWindowTooSmall = "welltest-window-too-small";
    public const string CodeWindowOutsideData = "welltest-window-outside-data";
    public const string CodeAnalysisRefused = "welltest-analysis-refused";
    public const string CodeNothingToSave = "welltest-nothing-to-save";
    public const string CodeNothingPending = "welltest-nothing-pending";

    /// <summary>The tolerance the independent Horner check is judged at (AC-4, SG-08-2).</summary>
    public const decimal CrossCheckTolerance = 0.02m;

    /// <summary>The fewest readings a straight-line fit can use.</summary>
    public const int MinimumPointsInWindow = 3;

    /// <summary>The units the analysis accepts - one of each, stated on the record.</summary>
    public const string AcceptedTimeUnit = "h";
    public const string AcceptedPressureUnit = "psia";
    public static IReadOnlyList<string> AcceptedRateUnits { get; } = ["STB/d", "Mscf/d"];

    public static IReadOnlyList<string> Missing(OgWellTestInputs inputs)
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

        if (string.IsNullOrWhiteSpace(inputs.AnalysisType))
        {
            missing.Add("analysis type");
        }

        if (string.IsNullOrWhiteSpace(inputs.Method))
        {
            missing.Add("method");
        }

        Need(inputs.WindowStartHours, "radial-flow window start");
        Need(inputs.WindowEndHours, "radial-flow window end");
        Need(inputs.FlowRate, "flow rate");
        Need(inputs.WellboreRadius, "wellbore radius");
        Need(inputs.Thickness, "formation thickness");
        Need(inputs.Porosity, "porosity");
        Need(inputs.TotalCompressibility, "total compressibility");
        Need(inputs.Viscosity, "oil viscosity");
        Need(inputs.FormationVolumeFactor, "formation volume factor");
        Need(inputs.ProductionTimeHours, "producing time");

        if (inputs.Readings is null || inputs.Readings.Count == 0)
        {
            missing.Add("at least one pressure-time reading");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgWellTestInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return missing.Contains("well")
                ? (CodeWellRequired, "Choose the well the test is for.")
                : (CodeInputRequired, $"The analysis needs {string.Join(", ", missing)}.");
        }

        if (!WellTestAnalyses.IsSupportedType(inputs.AnalysisType))
        {
            return (CodeTypeUnknown,
                $"'{inputs.AnalysisType}' is not an analysis type; use {string.Join(", ", WellTestAnalyses.Types)}.");
        }

        if (!WellTestAnalyses.IsSupportedMethod(inputs.Method))
        {
            return (CodeMethodUnsupported,
                $"'{inputs.Method}' is not a method; the module implements {string.Join(", ", WellTestAnalyses.Methods)}.");
        }

        if (!WellTestAnalyses.MethodsFor(inputs.AnalysisType)
                .Contains(inputs.Method!.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeMethodUnsupported,
                $"a {inputs.AnalysisType} cannot be analysed by {inputs.Method}: " +
                $"{inputs.AnalysisType} supports {string.Join(", ", WellTestAnalyses.MethodsFor(inputs.AnalysisType))}.");
        }

        // The state table's "mixed units - refused with the offending column".
        if (!string.Equals(inputs.TimeUnit?.Trim(), AcceptedTimeUnit, StringComparison.OrdinalIgnoreCase))
        {
            return (CodeMixedUnits,
                $"The time column is in '{inputs.TimeUnit}'; the analysis takes hours ({AcceptedTimeUnit}) only.");
        }

        if (!string.Equals(inputs.PressureUnit?.Trim(), AcceptedPressureUnit, StringComparison.OrdinalIgnoreCase))
        {
            return (CodeMixedUnits,
                $"The pressure column is in '{inputs.PressureUnit}'; the analysis takes psia ({AcceptedPressureUnit}) only.");
        }

        if (!string.IsNullOrWhiteSpace(inputs.RateUnit)
            && !AcceptedRateUnits.Contains(inputs.RateUnit.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return (CodeMixedUnits,
                $"The rate column is in '{inputs.RateUnit}'; use {string.Join(" or ", AcceptedRateUnits)}.");
        }

        // AC-3: a missing radial-flow window is refused, never guessed.
        if (inputs.WindowStartHours <= 0m || inputs.WindowEndHours <= inputs.WindowStartHours)
        {
            return (CodeWindowOutsideData,
                $"The radial-flow window must start above zero and end after it starts; it is " +
                $"{inputs.WindowStartHours?.ToString("#,0.####", CultureInfo.InvariantCulture)} to " +
                $"{inputs.WindowEndHours?.ToString("#,0.####", CultureInfo.InvariantCulture)} h.");
        }

        var inWindow = inputs.Readings!
            .Count(reading => reading.TimeHours >= inputs.WindowStartHours && reading.TimeHours <= inputs.WindowEndHours);

        if (inWindow == 0)
        {
            return (CodeNoWindow,
                $"No reading falls inside the window " +
                $"{inputs.WindowStartHours!.Value.ToString("#,0.####", CultureInfo.InvariantCulture)} to " +
                $"{inputs.WindowEndHours!.Value.ToString("#,0.####", CultureInfo.InvariantCulture)} h; select the " +
                "radial-flow window on the readings before analysing.");
        }

        if (inWindow < MinimumPointsInWindow)
        {
            return (CodeWindowTooSmall,
                $"The window holds {inWindow.ToString(CultureInfo.InvariantCulture)} reading(s); a straight line needs " +
                $"at least {MinimumPointsInWindow.ToString(CultureInfo.InvariantCulture)}.");
        }

        return inputs.FlowRate <= 0m
            ? (CodeInputRequired, "The flow rate must be above zero.")
            : null;
    }
}

/// <summary>WTA-01: well test analysis.</summary>
public interface IOgWellTest
{
    /// <summary>Analyses the test. Nothing is written.</summary>
    Task<OgResult<OgWellTestWorkspace>> AnalyseAsync(
        OgWellTestInputs inputs,
        CancellationToken cancellationToken = default);

    /// <summary>Saves the result with its window and method (AC-6).</summary>
    Task<OgResult<OgWellTestWorkspace>> SaveAsync(
        OgWellTestResult result,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved analyses; an unavailable history is a state.</summary>
    Task<OgResult<OgWellTestWorkspace>> LoadAsync(
        string? wellId = null,
        string? resultId = null,
        CancellationToken cancellationToken = default);

    /// <summary>The result retained when its save was not acknowledged.</summary>
    OgWellTestPending? Pending { get; }

    /// <summary>Retries the same insert.</summary>
    Task<OgResult<OgWellTestWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// WTA-01 over the WellTestAnalysis module's build-up, MDH, drawdown and derivative calculators, with the sibling tools'
/// <c>HornerAnalysis</c> as the independent cross-check, through a unit of work.
/// </summary>
/// <remarks>
/// The engine's own validators are used as refusals rather than caught: <c>WellTestDataValidator.FindProblem</c> names the
/// property at fault before the analysis runs. The radial-flow window is the engineer's - it is sliced from the readings
/// before the analysis, so a result always says which part of the test it came from (AC-6), and a missing window is
/// refused rather than guessed (AC-3).
/// </remarks>
public sealed class OgWellTestService : IOgWellTest
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWellTestService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgWellTestPending? Pending { get; private set; }

    public async Task<OgResult<OgWellTestWorkspace>> AnalyseAsync(
        OgWellTestInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (WellTestRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgWellTestWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var built = Build(inputs);

            if (built.Refusal is { } refused)
            {
                return OgResult<OgWellTestWorkspace>.Failed(refused.Message, refused.Code);
            }

            var source = _source();
            var history = source is null
                ? []
                : await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgWellTestWorkspace>.Ok(Workspace(history, built.Result));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"analysing a well test for well {inputs.WellId}",
                "the analysis is not shown",
                FailureSeverity.Error);

            return OgResult<OgWellTestWorkspace>.Failed(
                "The well test analysis could not be run.", reference ?? WellTestRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgWellTestWorkspace>> SaveAsync(
        OgWellTestResult result,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellTestWorkspace>.Failed(
                "A saved result must record who saved it.", WellTestRules.CodeActorRequired);
        }

        if (!result.Converged || string.IsNullOrWhiteSpace(result.Method))
        {
            return OgResult<OgWellTestWorkspace>.Failed(
                "There is no converged analysis to save; run one that converges first.",
                WellTestRules.CodeNothingToSave);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellTestWorkspace>.Failed(
                "No connection is open, so the result cannot be saved.", WellTestRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<WELL_TEST_ANALYSIS_RESULT>(source);
            unit.PrimaryKey = "WELL_TEST_ANALYSIS_RESULT_ID";

            unit.Add(new WELL_TEST_ANALYSIS_RESULT
            {
                WELL_TEST_ANALYSIS_RESULT_ID = Guid.NewGuid().ToString("N"),
                ANALYSIS_ID = Guid.NewGuid().ToString("N"),
                WELL_UWI = result.WellId,
                WELL_ID = result.WellId,
                ANALYSIS_TYPE = result.AnalysisType,
                ANALYSIS_METHOD = result.Method,
                PERMEABILITY = result.Permeability,
                SKIN_FACTOR = result.Skin,
                RESERVOIR_PRESSURE = result.ReservoirPressure ?? 0m,
                PRODUCTIVITY_INDEX = result.ProductivityIndex ?? 0m,
                R_SQUARED = result.RSquared ?? 0m,
                RSQUARED = result.RSquared is { } squared ? (double)squared : 0.0,
                IDENTIFIED_MODEL = result.IdentifiedModel,
                IDENTIFIED_MODEL_STRING = result.IdentifiedModel,
                STATUS = "COMPLETED",
                IS_SUCCESSFUL = result.Converged,
                ANALYSIS_DATE = now,
                CALCULATION_DATE = now,
                ANALYSIS_BY_USER = actor.Trim(),
                WINDOW_START_HOURS = result.WindowStartHours,
                WINDOW_END_HOURS = result.WindowEndHours,
                POINTS_IN_WINDOW = result.PointsInWindow,
                WINDOW_TEXT = result.WindowText,
                CONVERGED_IND = result.Converged ? "Y" : "N",
                DERIVATIVE_USABLE_IND = result.DerivativeUsable ? "Y" : "N",
                DERIVATIVE_REASON = result.DerivativeReason,
                DERIVATIVE_POINTS = result.DerivativePoints,
                CROSS_CHECK_SOURCE = result.CrossCheck.Source,
                CROSS_CHECK_SLOPE = result.CrossCheck.Slope,
                CROSS_CHECK_PERMEABILITY = result.CrossCheck.Permeability,
                CROSS_CHECK_AGREES = result.CrossCheck.Available
                    ? (result.CrossCheck.Agrees ? "Y" : "N")
                    : null,
                CROSS_CHECK_TEXT = result.CrossCheck.Text,
                ANALYSIS_SLOPE = result.AnalysisSlope,
                DATA_UNITS_TEXT = result.UnitsText,
                ACTOR = actor.Trim(),
                USER_ID = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                Pending = new OgWellTestPending(result, actor.Trim());

                return OgResult<OgWellTestWorkspace>.Failed(
                    OgUow.Message(committed, "The result was not saved; the original is retained and can be retried."),
                    WellTestRules.CodeWriteFailed);
            }

            Pending = null;

            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);
            var saved = result with { Id = string.Empty, Actor = actor.Trim(), RecordedAt = now };

            return OgResult<OgWellTestWorkspace>.Ok(Workspace(history, saved));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgWellTestPending(result, actor.Trim());

            var reference = _failures.ReportHandled(
                ex,
                $"saving a well test analysis for well {result.WellId}",
                "the result was not acknowledged; the original is retained and the save can be retried",
                FailureSeverity.Error);

            return OgResult<OgWellTestWorkspace>.Failed(
                "The result was not acknowledged; the original is retained and the save can be retried.",
                reference ?? WellTestRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWellTestWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgWellTestWorkspace>.Failed(
                "There is no result waiting to be retried.", WellTestRules.CodeNothingPending);
        }

        var retried = await SaveAsync(pending.Result, actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    public async Task<OgResult<OgWellTestWorkspace>> LoadAsync(
        string? wellId = null,
        string? resultId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellTestWorkspace>.Failed(
                "No connection is open, so the saved analyses cannot be read.", WellTestRules.CodeNoConnection);
        }

        try
        {
            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            var selected = string.IsNullOrWhiteSpace(resultId)
                ? history.FirstOrDefault(result => string.IsNullOrWhiteSpace(wellId)
                    || string.Equals(result.WellId, wellId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault()
                : history.FirstOrDefault(result =>
                      string.Equals(result.Id, resultId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault();

            return OgResult<OgWellTestWorkspace>.Ok(Workspace(history, selected));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the saved well test analyses",
                "the saved analyses are unavailable; new analyses are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgWellTestWorkspace>.Ok(Workspace(
                [],
                null,
                true,
                $"the saved analyses could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the analysis

    /// <summary>What an analysis produced: either a result, or the refusal the engine or a rule stated.</summary>
    public sealed record BuiltResult(OgWellTestResult? Result, (string Code, string Message)? Refusal);

    /// <summary>
    /// Runs the engine for the type and method, the derivative diagnostic and the independent Horner check.
    /// </summary>
    public BuiltResult Build(OgWellTestInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var type = inputs.AnalysisType!.Trim().ToUpperInvariant();
        var method = inputs.Method!.Trim().ToUpperInvariant();

        var windowReadings = inputs.Readings!
            .Where(reading => reading.TimeHours >= inputs.WindowStartHours!.Value
                              && reading.TimeHours <= inputs.WindowEndHours!.Value)
            .OrderBy(reading => reading.TimeHours)
            .ToList();

        var data = new WELL_TEST_DATA
        {
            WELL_TEST_DATA_ID = Guid.NewGuid().ToString("N"),
            TEST_TYPE = WellTestAnalyses.IsBuildUp(type) ? nameof(WellTestType.BuildUp) : nameof(WellTestType.Drawdown),
            FLOW_RATE = inputs.FlowRate!.Value,
            WELLBORE_RADIUS = inputs.WellboreRadius!.Value,
            FORMATION_THICKNESS = inputs.Thickness!.Value,
            POROSITY = inputs.Porosity!.Value,
            TOTAL_COMPRESSIBILITY = inputs.TotalCompressibility!.Value,
            OIL_VISCOSITY = inputs.Viscosity!.Value,
            OIL_FORMATION_VOLUME_FACTOR = inputs.FormationVolumeFactor!.Value,
            PRODUCTION_TIME = inputs.ProductionTimeHours!.Value,
            INITIAL_RESERVOIR_PRESSURE = inputs.InitialReservoirPressure ?? 0m,
            Time = windowReadings.Select(reading => (double)reading.TimeHours).ToList(),
            Pressure = windowReadings.Select(reading => (double)reading.Pressure).ToList(),
        };

        // The engine's own non-throwing validator names the property at fault before anything is computed.
        if (WellTestDataValidator.FindProblem(data) is { } problem)
        {
            return new BuiltResult(null, (WellTestRules.CodeAnalysisRefused, problem.Sentence));
        }

        WELL_TEST_ANALYSIS_RESULT analysis;

        try
        {
            analysis = (WellTestAnalyses.IsBuildUp(type), method) switch
            {
                (true, WellTestAnalyses.Horner) => BuildUpAnalysis.AnalyzeHorner(data),
                (true, WellTestAnalyses.Mdh) => BuildUpAnalysis.AnalyzeMDH(data),
                _ => DrawdownAnalysis.AnalyzeDrawdown(data),
            };
        }
        catch (WellTestException ex)
        {
            // The engine refused the analysis (a window with no straight line, a non-converged fit). The exception goes
            // to the store and the caller gets a sentence with the reference, not the exception's text.
            var reference = _failures.ReportHandled(
                ex,
                $"analysing a well test for well {inputs.WellId}",
                "the analysis is refused and no result is shown",
                FailureSeverity.Degraded);

            return new BuiltResult(null, (WellTestRules.CodeAnalysisRefused,
                $"The analysis did not converge on this window: the engine could not fit a straight line through the " +
                $"readings between {inputs.WindowStartHours!.Value.ToString("#,0.####", CultureInfo.InvariantCulture)} and " +
                $"{inputs.WindowEndHours!.Value.ToString("#,0.####", CultureInfo.InvariantCulture)} h. Select a wider or " +
                $"later radial-flow window and run it again (reference {reference})."));
        }

        // The derivative diagnostic: a derivative that does not produce usable points is reported as unusable.
        var derivativePoints = windowReadings
            .Select((reading, index) => new PRESSURE_TIME_POINT
            {
                PRESSURE_TIME_POINT_ID = index.ToString(CultureInfo.InvariantCulture),
                TIME = (double)reading.TimeHours,
                PRESSURE = (double)reading.Pressure,
                POINT_ORDER = index,
            })
            .ToList();

        var derivative = DerivativeAnalysis.CalculateDerivative(derivativePoints);
        var usable = derivative.Count >= 2
                     && derivative.All(point => point.PRESSURE_DERIVATIVE is { } value && double.IsFinite(value));
        var derivativeReason = usable
            ? null
            : derivative.Count < 2
                ? "the derivative produced fewer than two points"
                : "the derivative produced a non-finite value";

        string? model = null;

        if (usable)
        {
            model = DerivativeAnalysis.IdentifyModel(derivative).ToString();
        }

        var slope = analysis.PERMEABILITY > 0m && inputs.Thickness > 0m
            ? 162.6m * inputs.FlowRate!.Value * inputs.FormationVolumeFactor!.Value * inputs.Viscosity!.Value /
              (analysis.PERMEABILITY * inputs.Thickness.Value)
            : (decimal?)null;

        var crossCheck = CrossCheck(inputs, method, analysis.PERMEABILITY, slope);

        return new BuiltResult(
            new OgWellTestResult(
                string.Empty,
                inputs.WellId!.Trim(),
                type,
                method,
                analysis.PERMEABILITY,
                analysis.SKIN_FACTOR,
                null,
                analysis.RESERVOIR_PRESSURE == 0m ? null : analysis.RESERVOIR_PRESSURE,
                analysis.PRODUCTIVITY_INDEX == 0m ? null : analysis.PRODUCTIVITY_INDEX,
                true,
                analysis.R_SQUARED == 0m ? null : analysis.R_SQUARED,
                model,
                inputs.WindowStartHours.Value,
                inputs.WindowEndHours.Value,
                windowReadings.Count,
                usable,
                derivativeReason,
                usable ? derivative.Count : 0,
                crossCheck,
                $"time {inputs.TimeUnit} · pressure {inputs.PressureUnit} · rate {inputs.RateUnit}",
                slope,
                null,
                null),
            null);
    }

    /// <summary>
    /// AC-4 and SG-08-2: the sibling tools' <c>HornerAnalysis</c> is fitted to the same readings and compared. It is
    /// guarded, because a cross-check that cannot run is a state of the answer, not a failure of the analysis.
    /// </summary>
    private OgWellTestCrossCheck CrossCheck(
        OgWellTestInputs inputs,
        string method,
        decimal permeability,
        decimal? slope)
    {
        const string source = "TheTechIdea.OilGas.Production HornerAnalysis (independent implementation)";

        if (!string.Equals(method, WellTestAnalyses.Horner, StringComparison.OrdinalIgnoreCase))
        {
            return new OgWellTestCrossCheck(
                source,
                false,
                null,
                null,
                false,
                "the cross-check is a Horner build-up fit, so it is not applied to this method");
        }

        try
        {
            var windowReadings = inputs.Readings!
                .Where(reading => reading.TimeHours >= inputs.WindowStartHours!.Value
                                  && reading.TimeHours <= inputs.WindowEndHours!.Value)
                .OrderBy(reading => reading.TimeHours)
                .ToList();

            var buildUpInputs = new BuildupInputs
            {
                Rate = (double)inputs.FlowRate!.Value,
                OilFvf = (double)inputs.FormationVolumeFactor!.Value,
                Viscosity = (double)inputs.Viscosity!.Value,
                Thickness = (double)inputs.Thickness!.Value,
                Porosity = (double)inputs.Porosity!.Value,
                TotalCompressibility = (double)inputs.TotalCompressibility!.Value,
                WellboreRadius = (double)inputs.WellboreRadius!.Value,
                PwfAtShutIn = windowReadings.Count == 0 ? double.NaN : (double)windowReadings[0].Pressure,
                ProducingTimeHours = (double)inputs.ProductionTimeHours!.Value,
            };

            var readings = windowReadings
                .Select(reading => new BuildupReading(
                    (double)reading.TimeHours,
                    (double)reading.Pressure))
                .ToList();

            var tool = HornerAnalysis.Analyze(buildUpInputs, readings);

            var agrees = slope is { } ours
                          && ours > 0m
                          && Math.Abs(ours - (decimal)tool.Slope) / ours <= WellTestRules.CrossCheckTolerance;

            return new OgWellTestCrossCheck(
                source,
                true,
                (decimal)tool.Slope,
                (decimal)tool.Permeability,
                agrees,
                agrees
                    ? $"the independent fitter agrees: slope {tool.Slope.ToString("#,0.####", CultureInfo.InvariantCulture)} " +
                      $"psi/cycle and k {tool.Permeability.ToString("#,0.####", CultureInfo.InvariantCulture)} md against " +
                      $"this analysis's {slope!.Value.ToString("#,0.####", CultureInfo.InvariantCulture)} and " +
                      $"{permeability.ToString("#,0.####", CultureInfo.InvariantCulture)} " +
                      $"(within {WellTestRules.CrossCheckTolerance.ToString("0.##%", CultureInfo.InvariantCulture)})"
                    : $"the independent fitter reads slope {tool.Slope.ToString("#,0.####", CultureInfo.InvariantCulture)} " +
                      $"psi/cycle and k {tool.Permeability.ToString("#,0.####", CultureInfo.InvariantCulture)} md against " +
                      $"this analysis's {(slope is { } value ? value.ToString("#,0.####", CultureInfo.InvariantCulture) : "none")} " +
                      $"and {permeability.ToString("#,0.####", CultureInfo.InvariantCulture)}; both are shown and the " +
                      "difference stands");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "running the independent Horner cross-check",
                "only this analysis is shown, because the cross-check could not be run",
                FailureSeverity.Degraded);

            return new OgWellTestCrossCheck(
                source,
                false,
                null,
                null,
                false,
                $"the cross-check could not be run, so only this analysis is shown (reference {reference})");
        }
    }

    // ------------------------------------------------------------------ the reads

    private static async Task<IReadOnlyList<OgWellTestResult>> ReadHistoryAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<WELL_TEST_ANALYSIS_RESULT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.WELL_TEST_ANALYSIS_RESULT_ID))
            .OrderByDescending(row => row.RECORDED_AT ?? row.ANALYSIS_DATE)
            .ThenBy(row => row.WELL_TEST_ANALYSIS_RESULT_ID, StringComparer.Ordinal)
            .Select(ToResult)
            .Take(50)
            .ToList();
    }

    private static OgWellTestResult ToResult(WELL_TEST_ANALYSIS_RESULT row)
    {
        var converged = string.Equals(row.CONVERGED_IND, "Y", StringComparison.OrdinalIgnoreCase);

        return new OgWellTestResult(
            row.WELL_TEST_ANALYSIS_RESULT_ID ?? string.Empty,
            row.WELL_UWI,
            row.ANALYSIS_TYPE ?? WellTestAnalyses.BuildUp,
            row.ANALYSIS_METHOD ?? WellTestAnalyses.Horner,
            row.PERMEABILITY,
            row.SKIN_FACTOR,
            null,
            row.RESERVOIR_PRESSURE == 0m ? null : row.RESERVOIR_PRESSURE,
            row.PRODUCTIVITY_INDEX == 0m ? null : row.PRODUCTIVITY_INDEX,
            converged,
            row.R_SQUARED == 0m ? null : row.R_SQUARED,
            row.IDENTIFIED_MODEL,
            row.WINDOW_START_HOURS ?? 0m,
            row.WINDOW_END_HOURS ?? 0m,
            row.POINTS_IN_WINDOW ?? 0,
            string.Equals(row.DERIVATIVE_USABLE_IND, "Y", StringComparison.OrdinalIgnoreCase),
            row.DERIVATIVE_REASON,
            row.DERIVATIVE_POINTS ?? 0,
            new OgWellTestCrossCheck(
                row.CROSS_CHECK_SOURCE ?? "not recorded",
                row.CROSS_CHECK_SLOPE is not null || row.CROSS_CHECK_PERMEABILITY is not null,
                row.CROSS_CHECK_SLOPE,
                row.CROSS_CHECK_PERMEABILITY,
                string.Equals(row.CROSS_CHECK_AGREES, "Y", StringComparison.OrdinalIgnoreCase),
                row.CROSS_CHECK_TEXT ?? "no cross-check was recorded"),
            row.DATA_UNITS_TEXT ?? string.Empty,
            row.ANALYSIS_SLOPE,
            row.ACTOR ?? row.ANALYSIS_BY_USER,
            row.RECORDED_AT ?? row.ANALYSIS_DATE);
    }

    private static OgWellTestWorkspace Workspace(
        IReadOnlyList<OgWellTestResult> history,
        OgWellTestResult? selected,
        bool historyFailed = false,
        string? historyReason = null) =>
        new(
            history,
            selected,
            WellTestAnalyses.Types,
            WellTestAnalyses.Methods,
            DisabledAnalyses,
            WellTestRules.CrossCheckTolerance,
            historyFailed,
            historyReason);

    /// <summary>
    /// The analyses the module does not implement, from its own <c>Unimplemented</c> partial - the feature's open question
    /// answered by naming them rather than offering them.
    /// </summary>
    private static IReadOnlyList<string> DisabledAnalyses { get; } =
    [
        "type-curve matching",
        "multi-rate",
        "deconvolution",
        "boundary detection",
        "gas build-up (no analysis-type token in WellTestAnalysisWellKnown)",
    ];
}
