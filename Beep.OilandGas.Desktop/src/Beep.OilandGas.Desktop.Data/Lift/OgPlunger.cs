using System.Globalization;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.PlungerLift.Calculations;
using Beep.OilandGas.PlungerLift.Constants;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;

/// <summary>The plunger types, read from the owning constants (LFT-05 AC-4).</summary>
public static class PlungerTypes
{
    public static IReadOnlyList<string> All { get; } =
    [
        PlungerLiftConstants.PlungerTypeBar,
        PlungerLiftConstants.PlungerTypePad,
        PlungerLiftConstants.PlungerTypeBrush,
        PlungerLiftConstants.PlungerTypeContinuous,
    ];

    public static bool IsSupported(string? type) =>
        !string.IsNullOrWhiteSpace(type)
        && All.Contains(type.Trim().ToUpperInvariant(), StringComparer.Ordinal);

    public static string NameOf(string type) => type.Trim().ToUpperInvariant() switch
    {
        PlungerLiftConstants.PlungerTypeBar => "Bar stock",
        PlungerLiftConstants.PlungerTypePad => "Pad",
        PlungerLiftConstants.PlungerTypeBrush => "Brush",
        PlungerLiftConstants.PlungerTypeContinuous => "Continuous",
        _ => type,
    };
}

/// <summary>The three verdicts (LFT-05): feasible, infeasible and feasible but uneconomic.</summary>
public static class PlungerVerdicts
{
    public const string Feasible = "FEASIBLE";
    public const string Infeasible = "INFEASIBLE";
    public const string Uneconomic = "UNECONOMIC";

    public static IReadOnlyList<string> All { get; } = [Feasible, Uneconomic, Infeasible];
}

/// <summary>One feasibility criterion and what it decided (LFT-05 AC-1).</summary>
public sealed record OgPlungerCriterion(
    string Code,
    bool Passed,
    string ValueText,
    string RangeText,
    string Message)
{
    public string OutcomeText => Passed ? "pass" : "FAIL";

    public string Describe() => $"{Code.ToLowerInvariant()} · {OutcomeText}: {Message}";
}

/// <summary>The inputs of one plunger lift assessment (LFT-05). Every value is required.</summary>
public sealed record OgPlungerInputs(
    string? WellId,
    string? PlungerType,
    decimal? DepthFt,
    decimal? TubingDiameterIn,
    decimal? CasingPressure,
    decimal? LinePressure,
    decimal? GasRateMscfPerDay,
    decimal? LiquidRateBblPerDay,
    decimal? GasLiquidRatio);

/// <summary>One plunger lift assessment (LFT-05), matching the plan's <c>OgPlungerDesign</c>.</summary>
public sealed record OgPlungerResult(
    string Id,
    string? WellId,
    string PlungerType,
    decimal DepthFt,
    decimal TubingDiameter,
    decimal CasingPressure,
    decimal LinePressure,
    decimal GasRate,
    decimal LiquidRate,
    decimal Glr,
    decimal FallVelocityFpm,
    decimal RiseVelocityFpm,
    decimal FallTime,
    decimal RiseTime,
    decimal ShutInTime,
    decimal CycleTime,
    decimal CyclesPerDay,
    decimal RatePerCycle,
    decimal? DailyRate,
    string Verdict,
    bool Economic,
    string EconomicBasis,
    IReadOnlyList<OgPlungerCriterion> Criteria,
    string? Actor,
    DateTime? RecordedAt)
{
    public const string RateUnit = "bbl/d";
    public const string CycleUnit = "bbl";
    public const string VelocityUnit = "ft/min";
    public const string TimeUnit = "min";
    public const string GlrUnit = "scf/bbl";

    public bool IsSaved => !string.IsNullOrWhiteSpace(Id);

    public bool IsFeasible => string.Equals(Verdict, PlungerVerdicts.Feasible, StringComparison.Ordinal);

    public bool IsInfeasible => string.Equals(Verdict, PlungerVerdicts.Infeasible, StringComparison.Ordinal);

    public bool IsUneconomic => string.Equals(Verdict, PlungerVerdicts.Uneconomic, StringComparison.Ordinal);

    public IReadOnlyList<OgPlungerCriterion> Failed => Criteria.Where(criterion => !criterion.Passed).ToList();

    public string TypeText => PlungerTypes.NameOf(PlungerType);

    public string VerdictText => Verdict.ToLowerInvariant();

    /// <summary>AC-2: an infeasible assessment never shows a rate.</summary>
    public string RateText => DailyRate is { } rate
        ? rate.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + RateUnit
        : "no rate: the well is infeasible";

    public string RatioText => Glr.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + GlrUnit;

    public string FallText => FallVelocityFpm.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + VelocityUnit;

    public string RiseText => RiseVelocityFpm.ToString("#,0.##", CultureInfo.InvariantCulture) + " " + VelocityUnit;

    /// <summary>AC-5: the three phases add up to the cycle, and the text says so.</summary>
    public string CycleText =>
        $"fall {FallTime.ToString("#,0.##", CultureInfo.InvariantCulture)} + rise " +
        $"{RiseTime.ToString("#,0.##", CultureInfo.InvariantCulture)} + shut-in " +
        $"{ShutInTime.ToString("#,0.##", CultureInfo.InvariantCulture)} = cycle " +
        $"{CycleTime.ToString("#,0.##", CultureInfo.InvariantCulture)} {TimeUnit} · " +
        $"{CyclesPerDay.ToString("#,0.##", CultureInfo.InvariantCulture)} cycle(s) per day";

    public string PerCycleText => RatePerCycle.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + CycleUnit;

    public string CriteriaText => Criteria.Count == 0
        ? "no criterion was evaluated"
        : string.Join(Environment.NewLine, Criteria.Select(criterion => criterion.Describe()));

    public string FailedText => Failed.Count == 0
        ? "no criterion failed"
        : string.Join(", ", Failed.Select(criterion => criterion.Code.ToLowerInvariant()));

    public string EconomicText => Economic
        ? "the well is economic for plunger lift: the cycle keeps up with the inflow"
        : "uneconomic: " + EconomicBasis;

    public string SavedText => IsSaved
        ? $"saved by {Actor ?? "(unknown)"} on " +
          $"{RecordedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"}"
        : "not saved";

    public string Describe() =>
        $"{WellId ?? "(no well)"} · {TypeText} · {VerdictText} · rate {RateText} · cycle " +
        $"{CycleTime.ToString("#,0.##", CultureInfo.InvariantCulture)} {TimeUnit} · {SavedText}";
}

/// <summary>An assessment whose save was not acknowledged: retained and retried (LFT-05's state table).</summary>
public sealed record OgPlungerPending(OgPlungerResult Result, string Actor);

/// <summary>The plunger workspace (LFT-05).</summary>
public sealed record OgPlungerWorkspace(
    IReadOnlyList<OgPlungerResult> Designs,
    OgPlungerResult? Selected,
    IReadOnlyList<string> PlungerTypes,
    IReadOnlyList<decimal> StandardPlungerDiameters,
    IReadOnlyList<decimal> StandardTubingDiameters,
    bool HistoryFailed,
    string? HistoryReason,
    string ReferenceText)
{
    public string DesignsText => Designs.Count == 0
        ? "No plunger assessment is saved yet."
        : string.Join(Environment.NewLine, Designs.Select(design => design.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the saved assessments could not be read; new assessments are still allowed"
        : $"{Designs.Count.ToString(CultureInfo.InvariantCulture)} saved assessment(s)";
}

/// <summary>The plunger rules, stated once so the page, the service and the tests agree.</summary>
public static class PlungerRules
{
    public const string CodeNoConnection = "plunger-no-connection";
    public const string CodeReadFailed = "plunger-read-failed";
    public const string CodeWriteFailed = "plunger-write-failed";
    public const string CodeActorRequired = "plunger-actor-required";
    public const string CodeWellRequired = "plunger-well-required";
    public const string CodeInputRequired = "plunger-input-required";
    public const string CodeInputInvalid = "plunger-input-invalid";
    public const string CodeTypeUnknown = "plunger-type-unknown";
    public const string CodeNothingToSave = "plunger-nothing-to-save";
    public const string CodeNothingPending = "plunger-nothing-pending";

    /// <summary>
    /// The slack a criterion is compared with, relative to its limit.
    /// </summary>
    /// <remarks>
    /// <c>PlungerLiftCalculator.EstimateRiseVelocity</c> computes in <c>double</c> and returns <c>decimal</c>, so a well
    /// sitting exactly on a limit comes back a fraction above it: the module's minimum differential of 50 psi gives
    /// 8.333333333333334 ft/s, which is 500.0000000000004 ft/min against a 500 ft/min maximum. Comparing with a relative
    /// 1e-9 accepts the boundary the engineer actually hit without hiding a real violation.
    /// </remarks>
    public const decimal ComparisonTolerance = 0.000000001m;

    public static IReadOnlyList<string> Missing(OgPlungerInputs inputs)
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

        if (string.IsNullOrWhiteSpace(inputs.PlungerType))
        {
            missing.Add("plunger type");
        }

        Need(inputs.DepthFt, "depth");
        Need(inputs.TubingDiameterIn, "tubing diameter");
        Need(inputs.CasingPressure, "casing pressure");
        Need(inputs.LinePressure, "line pressure");
        Need(inputs.GasRateMscfPerDay, "gas rate");
        Need(inputs.LiquidRateBblPerDay, "liquid rate");

        // The state table: "missing GLR - refused with the reason".
        Need(inputs.GasLiquidRatio, "gas-liquid ratio");

        return missing;
    }

    public static (string Code, string Message)? Validate(OgPlungerInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return missing.Contains("well")
                ? (CodeWellRequired, "Choose the well the assessment is for.")
                : missing.Contains("gas-liquid ratio")
                    ? (CodeInputRequired,
                        "A plunger assessment needs the gas-liquid ratio: it is the criterion that decides whether the " +
                        "well can lift at all.")
                    : (CodeInputRequired, $"The assessment needs {string.Join(", ", missing)}.");
        }

        if (!PlungerTypes.IsSupported(inputs.PlungerType))
        {
            return (CodeTypeUnknown,
                $"'{inputs.PlungerType}' is not a plunger type; the module's types are " +
                $"{string.Join(", ", PlungerTypes.All)}.");
        }

        if (inputs.DepthFt <= 0m)
        {
            return (CodeInputInvalid, "The depth must be above zero.");
        }

        if (inputs.LiquidRateBblPerDay is <= 0m)
        {
            return (CodeInputInvalid, "The liquid rate must be above zero.");
        }

        if (inputs.CasingPressure is < 0m || inputs.LinePressure is < 0m)
        {
            return (CodeInputInvalid, "A pressure cannot be negative.");
        }

        return inputs.GasLiquidRatio is <= 0m
            ? (CodeInputInvalid, "The gas-liquid ratio must be above zero.")
            : null;
    }
}

/// <summary>LFT-05: plunger lift.</summary>
public interface IOgPlunger
{
    /// <summary>Assesses the well and designs the cycle. Nothing is written.</summary>
    Task<OgResult<OgPlungerWorkspace>> AssessAsync(
        OgPlungerInputs inputs,
        CancellationToken cancellationToken = default);

    /// <summary>Saves a feasible assessment (AC-6).</summary>
    Task<OgResult<OgPlungerWorkspace>> SaveAsync(
        OgPlungerResult result,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The saved assessments; an unavailable history is a state.</summary>
    Task<OgResult<OgPlungerWorkspace>> LoadAsync(
        string? wellId = null,
        string? designId = null,
        CancellationToken cancellationToken = default);

    /// <summary>The result retained when its save was not acknowledged.</summary>
    OgPlungerPending? Pending { get; }

    /// <summary>Retries the same insert.</summary>
    Task<OgResult<OgPlungerWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// LFT-05 over the PlungerLift module's velocity and gas-requirement calculators, its constants' criteria and the
/// <c>PLUNGER_DESIGN</c> row, through a unit of work.
/// </summary>
/// <remarks>
/// The velocities are the owner's (<c>PlungerLiftCalculator.EstimateFallVelocity</c> and <c>EstimateRiseVelocity</c>) and
/// the limits are <c>PlungerLiftConstants</c>'. The screen evaluates every criterion, keeps infeasible distinct from
/// uneconomic, and never shows a rate for an infeasible well.
/// </remarks>
public sealed class OgPlungerService : IOgPlunger
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgPlungerService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgPlungerPending? Pending { get; private set; }

    public async Task<OgResult<OgPlungerWorkspace>> AssessAsync(
        OgPlungerInputs inputs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (PlungerRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgPlungerWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var result = Build(inputs);

            var source = _source();
            var history = source is null
                ? []
                : await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgPlungerWorkspace>.Ok(Workspace(history, result));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"assessing plunger lift for well {inputs.WellId}",
                "the assessment is not shown",
                FailureSeverity.Error);

            return OgResult<OgPlungerWorkspace>.Failed(
                "The plunger assessment could not be run.", reference ?? PlungerRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgPlungerWorkspace>> SaveAsync(
        OgPlungerResult result,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPlungerWorkspace>.Failed(
                "A saved assessment must record who saved it.", PlungerRules.CodeActorRequired);
        }

        // An infeasible assessment is a finding, not a design: there is nothing to install.
        if (result.IsInfeasible)
        {
            return OgResult<OgPlungerWorkspace>.Failed(
                "An infeasible assessment cannot be saved as a design: " + result.FailedText + ".",
                PlungerRules.CodeNothingToSave);
        }

        if (result.Criteria.Count == 0)
        {
            return OgResult<OgPlungerWorkspace>.Failed(
                "There is no assessment to save; run it first.", PlungerRules.CodeNothingToSave);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPlungerWorkspace>.Failed(
                "No connection is open, so the assessment cannot be saved.", PlungerRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var unit = OgUow.For<PLUNGER_DESIGN>(source);
            unit.PrimaryKey = "DESIGN_ID";

            unit.Add(new PLUNGER_DESIGN
            {
                DESIGN_ID = Guid.NewGuid().ToString("N"),
                WELL_UWI = result.WellId,
                DESIGN_DATE = now,
                PLUNGER_TYPE = result.PlungerType,
                DEPTH_FT = result.DepthFt,
                TUBING_DIAMETER_IN = result.TubingDiameter,
                CASING_PRESSURE_PSIA = result.CasingPressure,
                LINE_PRESSURE_PSIA = result.LinePressure,
                GAS_RATE_MSCFD = result.GasRate,
                LIQUID_RATE_BBLD = result.LiquidRate,
                GLR_SCF_BBL = result.Glr,
                FALL_VELOCITY_FPM = result.FallVelocityFpm,
                RISE_VELOCITY_FPM = result.RiseVelocityFpm,
                FALL_TIME_MIN = result.FallTime,
                RISE_TIME_MIN = result.RiseTime,
                SHUT_IN_TIME_MIN = result.ShutInTime,
                CYCLE_TIME_MIN = result.CycleTime,
                CYCLES_PER_DAY = result.CyclesPerDay,
                RATE_PER_CYCLE_BBL = result.RatePerCycle,
                DAILY_RATE_BBLD = result.DailyRate,
                FEASIBILITY = result.Verdict,
                ECONOMIC_IND = result.Economic ? "Y" : "N",
                ECONOMIC_BASIS = result.Economic ? null : result.EconomicBasis,
                CRITERIA_TEXT = result.CriteriaText,
                FAILED_CRITERIA = result.Failed.Count == 0 ? null : result.FailedText,
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
                Pending = new OgPlungerPending(result, actor.Trim());

                return OgResult<OgPlungerWorkspace>.Failed(
                    OgUow.Message(committed, "The assessment was not saved; the original is retained and can be retried."),
                    PlungerRules.CodeWriteFailed);
            }

            Pending = null;

            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);
            var saved = result with { Id = string.Empty, Actor = actor.Trim(), RecordedAt = now };

            return OgResult<OgPlungerWorkspace>.Ok(Workspace(history, saved));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgPlungerPending(result, actor.Trim());

            var reference = _failures.ReportHandled(
                ex,
                $"saving a plunger assessment for well {result.WellId}",
                "the assessment was not acknowledged; the original is retained and the save can be retried",
                FailureSeverity.Error);

            return OgResult<OgPlungerWorkspace>.Failed(
                "The assessment was not acknowledged; the original is retained and the save can be retried.",
                reference ?? PlungerRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPlungerWorkspace>> RetrySaveAsync(
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgPlungerWorkspace>.Failed(
                "There is no assessment waiting to be retried.", PlungerRules.CodeNothingPending);
        }

        var retried = await SaveAsync(pending.Result, actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    public async Task<OgResult<OgPlungerWorkspace>> LoadAsync(
        string? wellId = null,
        string? designId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgPlungerWorkspace>.Failed(
                "No connection is open, so the saved assessments cannot be read.", PlungerRules.CodeNoConnection);
        }

        try
        {
            var history = await ReadHistoryAsync(source, cancellationToken).ConfigureAwait(false);

            var selected = string.IsNullOrWhiteSpace(designId)
                ? history.FirstOrDefault(design => string.IsNullOrWhiteSpace(wellId)
                    || string.Equals(design.WellId, wellId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault()
                : history.FirstOrDefault(design =>
                      string.Equals(design.Id, designId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault();

            return OgResult<OgPlungerWorkspace>.Ok(Workspace(history, selected));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the saved plunger assessments",
                "the saved assessments are unavailable; new assessments are still allowed",
                FailureSeverity.Degraded);

            return OgResult<OgPlungerWorkspace>.Ok(new OgPlungerWorkspace(
                [],
                null,
                PlungerTypes.All,
                PlungerLiftConstants.StandardPlungerDiameters,
                PlungerLiftConstants.StandardTubingDiameters,
                true,
                $"the saved assessments could not be read (reference {reference})",
                ReferenceText()));
        }
    }

    // ------------------------------------------------------------------ the assessment

    /// <summary>
    /// Evaluates every criterion, builds the cycle from the owner's velocities and decides the verdict. An infeasible
    /// assessment carries no daily rate (AC-2); a feasible one that cannot keep up is uneconomic with its basis (AC-3).
    /// </summary>
    public static OgPlungerResult Build(OgPlungerInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var type = inputs.PlungerType!.Trim().ToUpperInvariant();
        var depth = inputs.DepthFt!.Value;
        var casing = inputs.CasingPressure!.Value;
        var line = inputs.LinePressure!.Value;
        var differential = casing - line;
        var glr = inputs.GasLiquidRatio!.Value;

        // The owner's velocities are feet per second; the module's limits are feet per minute.
        var fallVelocity = PlungerLiftCalculator.EstimateFallVelocity(type, inLiquid: false) * 60m;
        var riseVelocity = PlungerLiftCalculator.EstimateRiseVelocity(differential) * 60m;

        var fallTime = fallVelocity > 0m ? depth / fallVelocity : 0m;
        var riseTime = riseVelocity > 0m ? depth / riseVelocity : 0m;
        var shutIn = PlungerLiftConstants.StandardShutInTime;
        var cycleTime = fallTime + riseTime + shutIn;

        var cyclesPerDay = cycleTime > 0m ? PlungerLiftConstants.MinutesPerDay / cycleTime : 0m;

        // The plunger lifts what accumulates in a cycle, up to the largest slug the module allows.
        var inflowPerCycle = cyclesPerDay > 0m
            ? inputs.LiquidRateBblPerDay!.Value / cyclesPerDay
            : 0m;

        var ratePerCycle = Math.Min(inflowPerCycle, PlungerLiftConstants.MaximumLiquidSlugSize);
        var dailyRate = ratePerCycle * cyclesPerDay;

        var criteria = new List<OgPlungerCriterion>
        {
            Criterion(
                "GLR_RANGE",
                glr,
                PlungerLiftConstants.MinimumGasLiquidRatio,
                PlungerLiftConstants.MaximumGasLiquidRatio,
                OgPlungerResult.GlrUnit,
                "the gas-liquid ratio"),

            Criterion(
                "PRESSURE_DIFFERENTIAL",
                differential,
                0m,
                decimal.MaxValue,
                "psi",
                "the pressure differential between casing and line"),

            Criterion(
                "FALL_VELOCITY_RANGE",
                fallVelocity,
                PlungerLiftConstants.MinimumFallVelocity,
                PlungerLiftConstants.MaximumFallVelocity,
                OgPlungerResult.VelocityUnit,
                "the fall velocity of the " + PlungerTypes.NameOf(type).ToLowerInvariant() + " plunger"),

            Criterion(
                "RISE_VELOCITY_RANGE",
                riseVelocity,
                PlungerLiftConstants.MinimumRiseVelocity,
                PlungerLiftConstants.MaximumRiseVelocity,
                OgPlungerResult.VelocityUnit,
                "the rise velocity at this pressure differential"),

            Criterion(
                "CYCLE_TIME_RANGE",
                cycleTime,
                PlungerLiftConstants.MinimumCycleTime,
                PlungerLiftConstants.MaximumCycleTime,
                OgPlungerResult.TimeUnit,
                "the cycle time"),

            Criterion(
                "SLUG_SIZE_RANGE",
                ratePerCycle,
                PlungerLiftConstants.MinimumLiquidSlugSize,
                PlungerLiftConstants.MaximumLiquidSlugSize,
                OgPlungerResult.CycleUnit,
                "the liquid lifted per cycle"),
        };

        // The differential criterion has no upper bound; a differential under the module's minimum is what fails it.
        criteria[1] = criteria[1] with
        {
            RangeText = $"at least {PlungerLiftConstants.MinimumPressureDifferential.ToString("#,0.##", CultureInfo.InvariantCulture)} psi",
            Passed = differential >= PlungerLiftConstants.MinimumPressureDifferential,
            Message = differential >= PlungerLiftConstants.MinimumPressureDifferential
                ? $"the differential of {differential.ToString("#,0.##", CultureInfo.InvariantCulture)} psi meets the " +
                  $"module's {PlungerLiftConstants.MinimumPressureDifferential.ToString("#,0.##", CultureInfo.InvariantCulture)} psi minimum"
                : $"the differential of {differential.ToString("#,0.##", CultureInfo.InvariantCulture)} psi is below the " +
                  $"module's {PlungerLiftConstants.MinimumPressureDifferential.ToString("#,0.##", CultureInfo.InvariantCulture)} psi minimum",
        };

        var failed = criteria.Where(criterion => !criterion.Passed).ToList();

        // AC-1, AC-2 and AC-3: infeasible when a mechanical criterion fails; uneconomic when the cycle cannot keep up.
        var verdict = failed.Count > 0
            ? PlungerVerdicts.Infeasible
            : dailyRate + 0.000001m < inputs.LiquidRateBblPerDay.Value
                ? PlungerVerdicts.Uneconomic
                : PlungerVerdicts.Feasible;

        var economicBasis = verdict == PlungerVerdicts.Uneconomic
            ? $"the cycle lifts at most {dailyRate.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
              $"{OgPlungerResult.RateUnit} against an inflow of " +
              $"{inputs.LiquidRateBblPerDay.Value.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
              $"{OgPlungerResult.RateUnit}, so the well would not be kept unloaded; this module has no price basis to " +
              "judge the cost of a cycle"
            : string.Empty;

        return new OgPlungerResult(
            string.Empty,
            inputs.WellId!.Trim(),
            type,
            depth,
            inputs.TubingDiameterIn!.Value,
            casing,
            line,
            inputs.GasRateMscfPerDay!.Value,
            inputs.LiquidRateBblPerDay.Value,
            glr,
            fallVelocity,
            riseVelocity,
            fallTime,
            riseTime,
            shutIn,
            cycleTime,
            cyclesPerDay,
            ratePerCycle,
            verdict == PlungerVerdicts.Infeasible ? null : dailyRate,
            verdict,
            verdict == PlungerVerdicts.Feasible,
            economicBasis,
            criteria,
            null,
            null);
    }

    /// <summary>
    /// Whether a value is inside a range, allowing the module's <see cref="PlungerRules.ComparisonTolerance"/> relative to
    /// the bound - the velocities come from a double-precision correlation and can land a fraction past a limit.
    /// </summary>
    private static bool Within(decimal value, decimal minimum, decimal maximum)
    {
        var lower = minimum - Math.Abs(minimum) * PlungerRules.ComparisonTolerance;
        var upper = maximum == decimal.MaxValue
            ? decimal.MaxValue
            : maximum + Math.Abs(maximum) * PlungerRules.ComparisonTolerance;

        return value >= lower && value <= upper;
    }

    private static OgPlungerCriterion Criterion(
        string code,
        decimal value,
        decimal minimum,
        decimal maximum,
        string unit,
        string subject)
    {
        var passed = Within(value, minimum, maximum);

        var range = maximum == decimal.MaxValue
            ? $"at least {minimum.ToString("#,0.##", CultureInfo.InvariantCulture)} {unit}"
            : $"{minimum.ToString("#,0.##", CultureInfo.InvariantCulture)} to " +
              $"{maximum.ToString("#,0.##", CultureInfo.InvariantCulture)} {unit}";

        var shown = value.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + unit;

        return new OgPlungerCriterion(
            code,
            passed,
            shown,
            range,
            passed
                ? $"{subject} at {shown} is inside {range}"
                : $"{subject} at {shown} is outside {range}");
    }

    // ------------------------------------------------------------------ the reads

    private static async Task<IReadOnlyList<OgPlungerResult>> ReadHistoryAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<PLUNGER_DESIGN>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.DESIGN_ID))
            .OrderByDescending(row => row.RECORDED_AT ?? row.DESIGN_DATE ?? DateTime.MinValue)
            .ThenBy(row => row.DESIGN_ID, StringComparer.Ordinal)
            .Select(ToResult)
            .Take(50)
            .ToList();
    }

    private static OgPlungerResult ToResult(PLUNGER_DESIGN row)
    {
        var verdict = string.IsNullOrWhiteSpace(row.FEASIBILITY)
            ? PlungerVerdicts.Infeasible
            : row.FEASIBILITY!;

        var criteria = string.IsNullOrWhiteSpace(row.CRITERIA_TEXT)
            ? []
            : row.CRITERIA_TEXT!
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line => new OgPlungerCriterion(line.Split(' ')[0].ToUpperInvariant(), true, line, string.Empty, line))
                .ToList();

        return new OgPlungerResult(
            row.DESIGN_ID ?? string.Empty,
            row.WELL_UWI,
            row.PLUNGER_TYPE ?? PlungerLiftConstants.PlungerTypeDefault,
            row.DEPTH_FT,
            row.TUBING_DIAMETER_IN,
            row.CASING_PRESSURE_PSIA,
            row.LINE_PRESSURE_PSIA,
            row.GAS_RATE_MSCFD,
            row.LIQUID_RATE_BBLD,
            row.GLR_SCF_BBL,
            row.FALL_VELOCITY_FPM,
            row.RISE_VELOCITY_FPM,
            row.FALL_TIME_MIN,
            row.RISE_TIME_MIN,
            row.SHUT_IN_TIME_MIN,
            row.CYCLE_TIME_MIN,
            row.CYCLES_PER_DAY,
            row.RATE_PER_CYCLE_BBL,
            row.DAILY_RATE_BBLD,
            verdict,
            string.Equals(row.ECONOMIC_IND, "Y", StringComparison.OrdinalIgnoreCase),
            row.ECONOMIC_BASIS ?? string.Empty,
            criteria,
            row.ACTOR,
            row.RECORDED_AT ?? row.DESIGN_DATE);
    }

    private static OgPlungerWorkspace Workspace(IReadOnlyList<OgPlungerResult> history, OgPlungerResult? selected) =>
        new(
            history,
            selected,
            PlungerTypes.All,
            PlungerLiftConstants.StandardPlungerDiameters,
            PlungerLiftConstants.StandardTubingDiameters,
            false,
            null,
            ReferenceText());

    private static string ReferenceText() =>
        $"types {string.Join(", ", PlungerTypes.All)} · fall {PlungerLiftConstants.MinimumFallVelocity} to " +
        $"{PlungerLiftConstants.MaximumFallVelocity} ft/min · rise {PlungerLiftConstants.MinimumRiseVelocity} to " +
        $"{PlungerLiftConstants.MaximumRiseVelocity} ft/min · GLR {PlungerLiftConstants.MinimumGasLiquidRatio} to " +
        $"{PlungerLiftConstants.MaximumGasLiquidRatio} scf/bbl · cycle {PlungerLiftConstants.MinimumCycleTime} to " +
        $"{PlungerLiftConstants.MaximumCycleTime} min";
}
